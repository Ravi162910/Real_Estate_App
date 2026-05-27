using Microsoft.Extensions.Options;
using Real_Estate_App.Models;
using Stripe;
using Stripe.Checkout;

namespace Real_Estate_App.Services
{
    public interface IStripeService
    {
        bool IsConfigured { get; }

        // Creates a Stripe Checkout Session for the given property purchase and
        // returns the hosted-page URL to redirect the buyer to.
        Task<Session> CreateCheckoutSessionAsync(
            Property property,
            CheckoutViewModel buyer,
            int buyerUserId,
            string successUrl,
            string cancelUrl);

        // Re-fetches a session from Stripe so its paid/amount state can be
        // trusted server-side (never trust the browser for this).
        Task<Session> GetSessionAsync(string sessionId);

        // The buyer's card is authorized (held) but not charged until an
        // admin approves. These drive that decision:
        Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId);

        // Charges a previously-authorized payment (admin Approve).
        Task<PaymentIntent> CapturePaymentIntentAsync(string paymentIntentId);

        // Releases the hold without charging (admin Reject).
        Task<PaymentIntent> CancelPaymentIntentAsync(string paymentIntentId);

        // Refunds an already-captured payment. Used to make the buyer whole
        // when capture succeeded but the sale then fell through (lost the
        // concurrent-approval race for the property).
        Task<Refund> RefundPaymentIntentAsync(string paymentIntentId);

        // Makes the buyer whole regardless of capture state: releases the hold
        // if the payment is still only authorized ("requires_capture"), or
        // refunds it if it was already captured ("succeeded"). Used when a sale
        // cannot proceed because the property turned out to be already sold.
        Task ReleaseOrRefundAsync(string paymentIntentId);
    }

    public class StripeService : IStripeService
    {
        private readonly StripeSettings _settings;
        private readonly ILogger<StripeService> _logger;

        public StripeService(IOptions<StripeSettings> settings, ILogger<StripeService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public bool IsConfigured => _settings.IsConfigured;

        public async Task<Session> CreateCheckoutSessionAsync(
            Property property,
            CheckoutViewModel buyer,
            int buyerUserId,
            string successUrl,
            string cancelUrl)
        {
            // Amount is derived from the persisted property price only - never
            // from anything the client submitted - so the buyer can't tamper
            // with what they are charged.
            var amountMinorUnits = ToMinorUnits(property.Price);
            if (amountMinorUnits <= 0)
            {
                throw new InvalidOperationException(
                    $"Property {property.PropertyId} has a non-positive price and cannot be sold.");
            }

            var options = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                CustomerEmail = buyer.UserEmail,
                ClientReferenceId = property.PropertyId.ToString(),
                // Manual capture: completing Checkout only AUTHORIZES the card
                // (places a hold). Funds are captured when an admin approves,
                // or released when an admin rejects - no manual refunds.
                PaymentIntentData = new SessionPaymentIntentDataOptions
                {
                    CaptureMethod = "manual"
                },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = _settings.Currency,
                            UnitAmount = amountMinorUnits,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = property.PropertyName,
                                Description = property.PropertyAddress
                            }
                        }
                    }
                },
                // Metadata is echoed back on the session we retrieve after
                // payment, so fulfilment never has to trust query-string data.
                Metadata = new Dictionary<string, string>
                {
                    ["property_id"] = property.PropertyId.ToString(),
                    ["buyer_user_id"] = buyerUserId.ToString(),
                    ["buyer_name"] = Truncate(buyer.BuyerName, 250),
                    ["buyer_email"] = Truncate(buyer.UserEmail, 250),
                    ["buyer_phone"] = Truncate(buyer.PhoneNumber, 250),
                    ["billing_address"] = Truncate(buyer.BillingAddress, 250)
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);
            _logger.LogInformation(
                "Created Stripe Checkout session {SessionId} for property {PropertyId}",
                session.Id, property.PropertyId);
            return session;
        }

        public async Task<Session> GetSessionAsync(string sessionId)
        {
            var service = new SessionService();
            return await service.GetAsync(sessionId);
        }

        public async Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            return await service.GetAsync(paymentIntentId);
        }

        public async Task<PaymentIntent> CapturePaymentIntentAsync(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            var pi = await service.CaptureAsync(paymentIntentId);
            _logger.LogInformation("Captured PaymentIntent {PaymentIntentId} (status {Status})",
                pi.Id, pi.Status);
            return pi;
        }

        public async Task<PaymentIntent> CancelPaymentIntentAsync(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            var pi = await service.CancelAsync(paymentIntentId);
            _logger.LogInformation("Canceled PaymentIntent {PaymentIntentId} (status {Status})",
                pi.Id, pi.Status);
            return pi;
        }

        public async Task<Refund> RefundPaymentIntentAsync(string paymentIntentId)
        {
            var service = new RefundService();
            var refund = await service.CreateAsync(new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId
            });
            _logger.LogInformation("Refunded PaymentIntent {PaymentIntentId} (refund {RefundId}, status {Status})",
                paymentIntentId, refund.Id, refund.Status);
            return refund;
        }

        public async Task ReleaseOrRefundAsync(string paymentIntentId)
        {
            var pi = await GetPaymentIntentAsync(paymentIntentId);
            if (pi.Status == "requires_capture")
            {
                // Authorized but never charged - just release the hold.
                await CancelPaymentIntentAsync(paymentIntentId);
            }
            else if (pi.Status == "succeeded")
            {
                // Already captured - return the funds to the buyer.
                await RefundPaymentIntentAsync(paymentIntentId);
            }
            else
            {
                // canceled / requires_payment_method / etc.: nothing held, so
                // there is nothing to release or refund.
                _logger.LogInformation(
                    "ReleaseOrRefund: PaymentIntent {PaymentIntentId} is in status {Status}; no action needed.",
                    paymentIntentId, pi.Status);
            }
        }

        // NZD (and the other 2-decimal currencies used here) are charged in
        // cents. Round to 2dp first so a stray third decimal can't slip in.
        public static long ToMinorUnits(decimal amount)
        {
            var rounded = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
            return (long)(rounded * 100m);
        }

        private static string Truncate(string? value, int max)
        {
            value ??= string.Empty;
            return value.Length <= max ? value : value.Substring(0, max);
        }
    }
}
