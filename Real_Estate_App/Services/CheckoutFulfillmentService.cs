using System.Collections.Concurrent;
using Real_Estate_App.Models;
using Real_Estate_App.UnitOfWork;
using Stripe;
using Stripe.Checkout;

namespace Real_Estate_App.Services
{
    public enum FulfillmentOutcome
    {
        Created,        // a new Pending transaction was created
        AlreadyExists,  // this session was already fulfilled (idempotent hit)
        NotPaid,        // session is not in a paid state - nothing created
        PropertyGone,   // property no longer exists / not available
        AmountMismatch, // session total doesn't match the current price
        AlreadySold     // property already has an approved sale - buyer released/refunded
    }

    public record FulfillmentResult(FulfillmentOutcome Outcome, Transaction? Transaction);

    public interface ICheckoutFulfillmentService
    {
        // Idempotently turns a PAID Stripe Checkout session into a single
        // Pending Transaction (+ "request received" email). Safe to call from
        // both the success redirect and the webhook for the same session.
        Task<FulfillmentResult> FulfillAsync(Session session);
    }

    public class CheckoutFulfillmentService : ICheckoutFulfillmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IStripeService _stripeService;
        private readonly ILogger<CheckoutFulfillmentService> _logger;

        // Serialises fulfilment per session id so a near-simultaneous
        // redirect + webhook for the same payment can't both insert a row.
        // (Single-instance demo scope; prod should also have a filtered
        // unique index on StripeSessionId - see the security review notes.)
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        public CheckoutFulfillmentService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IStripeService stripeService,
            ILogger<CheckoutFulfillmentService> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _stripeService = stripeService;
            _logger = logger;
        }

        public async Task<FulfillmentResult> FulfillAsync(Session session)
        {
            if (session == null || string.IsNullOrEmpty(session.Id))
            {
                return new FulfillmentResult(FulfillmentOutcome.NotPaid, null);
            }

            // With manual capture the session is "unpaid" until we capture, so
            // we judge success by the PaymentIntent instead: "requires_capture"
            // means the card was authorized (funds held) - exactly what we want
            // before sending it to the admin queue. "succeeded" is accepted
            // defensively in case it was already captured.
            if (string.IsNullOrEmpty(session.PaymentIntentId))
            {
                _logger.LogWarning("Session {SessionId} has no PaymentIntent", session.Id);
                return new FulfillmentResult(FulfillmentOutcome.NotPaid, null);
            }

            PaymentIntent paymentIntent;
            try
            {
                paymentIntent = await _stripeService.GetPaymentIntentAsync(session.PaymentIntentId);
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Could not retrieve PaymentIntent {PaymentIntentId}",
                    session.PaymentIntentId);
                return new FulfillmentResult(FulfillmentOutcome.NotPaid, null);
            }

            if (paymentIntent.Status is not ("requires_capture" or "succeeded"))
            {
                _logger.LogWarning("Session {SessionId} PaymentIntent {PaymentIntentId} not authorized (status {Status})",
                    session.Id, paymentIntent.Id, paymentIntent.Status);
                return new FulfillmentResult(FulfillmentOutcome.NotPaid, null);
            }

            var gate = _locks.GetOrAdd(session.Id, _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync();
            try
            {
                // Idempotency: if any transaction already references this
                // session, fulfilment has run - return it without duplicating.
                var existing = (await _unitOfWork.Transactions
                        .FindAsync(t => t.StripeSessionId == session.Id))
                    .FirstOrDefault();
                if (existing != null)
                {
                    return new FulfillmentResult(FulfillmentOutcome.AlreadyExists, existing);
                }

                var metadata = session.Metadata ?? new Dictionary<string, string>();
                if (!metadata.TryGetValue("property_id", out var propIdRaw) ||
                    !int.TryParse(propIdRaw, out var propertyId))
                {
                    _logger.LogWarning("Session {SessionId} has no usable property_id metadata", session.Id);
                    return new FulfillmentResult(FulfillmentOutcome.PropertyGone, null);
                }

                var property = await _unitOfWork.Properties.GetByIdAsync(propertyId);
                if (property == null)
                {
                    _logger.LogWarning("Session {SessionId} references missing property {PropertyId}",
                        session.Id, propertyId);
                    return new FulfillmentResult(FulfillmentOutcome.PropertyGone, null);
                }

                // Re-derive the expected charge from the *current* persisted
                // price and confirm the authorization is for exactly that.
                // Stops a crafted/stale session authorizing a cheap amount.
                var expected = StripeService.ToMinorUnits(property.Price);
                if (paymentIntent.Amount != expected)
                {
                    _logger.LogWarning(
                        "Session {SessionId} authorized {Got} != expected {Expected} for property {PropertyId}",
                        session.Id, paymentIntent.Amount, expected, propertyId);
                    return new FulfillmentResult(FulfillmentOutcome.AmountMismatch, null);
                }

                // Never sell the same property twice. If it already has an
                // approved sale (e.g. an admin unhid/re-listed a property that
                // was already sold), do NOT create another purchase request -
                // release the buyer's hold (or refund if it was somehow already
                // captured) so they are never charged for an unavailable home.
                if (await _unitOfWork.Transactions.HasApprovedForPropertyAsync(property.PropertyId))
                {
                    try
                    {
                        await _stripeService.ReleaseOrRefundAsync(paymentIntent.Id);
                    }
                    catch (StripeException ex)
                    {
                        _logger.LogError(ex,
                            "Failed to release/refund authorization for already-sold property {PropertyId} (PI {PaymentIntentId}) - manual review required.",
                            property.PropertyId, paymentIntent.Id);
                    }

                    _logger.LogWarning(
                        "Session {SessionId} targeted already-sold property {PropertyId}; released buyer authorization, no transaction created.",
                        session.Id, property.PropertyId);
                    return new FulfillmentResult(FulfillmentOutcome.AlreadySold, null);
                }

                metadata.TryGetValue("buyer_user_id", out var buyerIdRaw);
                int.TryParse(buyerIdRaw, out var buyerUserId);
                metadata.TryGetValue("buyer_name", out var buyerName);
                metadata.TryGetValue("buyer_email", out var buyerEmail);

                // Prefer the address Stripe verified; fall back to our metadata.
                var email = !string.IsNullOrWhiteSpace(session.CustomerEmail)
                    ? session.CustomerEmail
                    : (buyerEmail ?? string.Empty);

                var transaction = new Transaction
                {
                    PropertyId = property.PropertyId,
                    UserId = buyerUserId,
                    Price = property.Price,
                    UserEmail = email,
                    BuyerName = buyerName ?? string.Empty,
                    PurchaseDate = DateTime.UtcNow,
                    Status = Transaction.StatusPending,
                    StripeSessionId = session.Id,
                    StripePaymentIntentId = paymentIntent.Id
                };

                await _unitOfWork.Transactions.AddAsync(transaction);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Fulfilled session {SessionId}: created Pending transaction {TransactionId}",
                    session.Id, transaction.TransactionId);

                try
                {
                    await _emailService.SendPurchaseRequestReceivedAsync(
                        transaction.UserEmail,
                        transaction.BuyerName,
                        property.PropertyName,
                        property.PropertyAddress,
                        property.Price,
                        transaction.PurchaseDate);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Email notification failed for transaction {TransactionId}",
                        transaction.TransactionId);
                }

                return new FulfillmentResult(FulfillmentOutcome.Created, transaction);
            }
            finally
            {
                gate.Release();
                // Best-effort cleanup so the dictionary doesn't grow unbounded.
                if (gate.CurrentCount == 1)
                {
                    _locks.TryRemove(session.Id, out _);
                }
            }
        }
    }
}
