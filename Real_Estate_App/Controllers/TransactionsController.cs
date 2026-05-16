using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Real_Estate_App.Models;
using Real_Estate_App.Services;
using Real_Estate_App.UnitOfWork;
using Stripe;

namespace Real_Estate_App.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ILogger<TransactionsController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStripeService _stripeService;
        private readonly ICheckoutFulfillmentService _fulfillment;

        public TransactionsController(
            ILogger<TransactionsController> logger,
            IUnitOfWork unitOfWork,
            IStripeService stripeService,
            ICheckoutFulfillmentService fulfillment)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _stripeService = stripeService;
            _fulfillment = fulfillment;
        }

        // GET: Transactions/Checkout/5
        // Collects buyer contact details only. Card data is never entered
        // here - the buyer is sent to Stripe's hosted page for that.
        public async Task<IActionResult> Checkout(int id)
        {
            var property = await _unitOfWork.Properties.GetByIdAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            if (!property.IsAvailable)
            {
                TempData["Error"] = "This property is no longer available for purchase.";
                return RedirectToAction("Details", "Properties", new { id });
            }

            var viewModel = new CheckoutViewModel
            {
                PropertyId = property.PropertyId,
                PropertyName = property.PropertyName,
                PropertyAddress = property.PropertyAddress,
                Price = property.Price
            };

            return View(viewModel);
        }

        // POST: Transactions/Checkout
        // Validates buyer details, then creates a Stripe Checkout Session and
        // redirects to Stripe. No Transaction row is created until the payment
        // is confirmed (success redirect or webhook).
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("checkout")]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var property = await _unitOfWork.Properties.GetByIdAsync(model.PropertyId);
            if (property == null)
            {
                return NotFound();
            }

            if (!property.IsAvailable)
            {
                TempData["Error"] = "This property is no longer available for purchase.";
                return RedirectToAction("Details", "Properties", new { id = model.PropertyId });
            }

            if (!_stripeService.IsConfigured)
            {
                _logger.LogError("Stripe is not configured - cannot start checkout.");
                TempData["Error"] = "Online payments are temporarily unavailable. Please try again later.";
                return RedirectToAction("Details", "Properties", new { id = model.PropertyId });
            }

            // Link the purchase to the buyer's account if they're logged in.
            int buyerUserId = int.TryParse(User.FindFirst("UserID")?.Value, out var uid) ? uid : 0;

            // Both URLs are built server-side from our own routes - never from
            // user input - so this can't be turned into an open redirect.
            var successUrl = Url.Action(nameof(Success), "Transactions", null, Request.Scheme)
                             + "?session_id={CHECKOUT_SESSION_ID}";
            var cancelUrl = Url.Action("Details", "Properties",
                                new { id = model.PropertyId }, Request.Scheme)!;

            try
            {
                var session = await _stripeService.CreateCheckoutSessionAsync(
                    property, model, buyerUserId, successUrl, cancelUrl);

                return Redirect(session.Url);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe failed to create a checkout session for property {PropertyId}",
                    model.PropertyId);
                TempData["Error"] = "We couldn't start the payment. Please try again.";
                return RedirectToAction("Details", "Properties", new { id = model.PropertyId });
            }
        }

        // GET: Transactions/Success?session_id=cs_test_...
        // Stripe redirects here after a successful payment. We re-fetch the
        // session from Stripe (never trust the query string for paid state)
        // and fulfil it idempotently.
        public async Task<IActionResult> Success(string? session_id)
        {
            if (string.IsNullOrWhiteSpace(session_id))
            {
                return NotFound();
            }

            Stripe.Checkout.Session session;
            try
            {
                session = await _stripeService.GetSessionAsync(session_id);
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Could not retrieve Stripe session {SessionId}", session_id);
                return NotFound();
            }

            var result = await _fulfillment.FulfillAsync(session);

            switch (result.Outcome)
            {
                case FulfillmentOutcome.Created:
                case FulfillmentOutcome.AlreadyExists:
                    var transaction = result.Transaction!;
                    // One-time token so an anonymous buyer can view the receipt
                    // without exposing Confirmation to id enumeration.
                    var confirmToken = Guid.NewGuid().ToString("N");
                    TempData[ConfirmTokenKey(transaction.TransactionId)] = confirmToken;
                    TempData["EmailSent"] = result.Outcome == FulfillmentOutcome.Created;
                    return RedirectToAction(nameof(Confirmation),
                        new { id = transaction.TransactionId, token = confirmToken });

                case FulfillmentOutcome.NotPaid:
                    TempData["Error"] = "Your payment was not completed. No purchase request was created.";
                    break;
                case FulfillmentOutcome.AmountMismatch:
                    TempData["Error"] = "There was a pricing mismatch on this payment. Please contact support.";
                    break;
                default:
                    TempData["Error"] = "This property is no longer available.";
                    break;
            }

            var propertyId = session.ClientReferenceId;
            if (int.TryParse(propertyId, out var pid))
            {
                return RedirectToAction("Details", "Properties", new { id = pid });
            }
            return RedirectToAction("Index", "Properties");
        }

        // GET: Transactions/Confirmation/5
        public async Task<IActionResult> Confirmation(int id, string? token)
        {
            var transaction = await _unitOfWork.Transactions.GetByIdWithPropertyAsync(id);

            if (transaction == null)
            {
                return NotFound();
            }

            var expectedToken = TempData.Peek(ConfirmTokenKey(id)) as string;
            bool tokenOk = !string.IsNullOrEmpty(token)
                        && !string.IsNullOrEmpty(expectedToken)
                        && token == expectedToken;

            bool ownerOk = User.Identity?.IsAuthenticated == true
                        && int.TryParse(User.FindFirst("UserID")?.Value, out var uid)
                        && transaction.UserId == uid
                        && uid != 0;

            if (!tokenOk && !ownerOk)
            {
                return NotFound();
            }

            // Keep the token so refreshing the receipt page still works.
            if (tokenOk)
            {
                TempData.Keep(ConfirmTokenKey(id));
            }

            ViewData["EmailSent"] = TempData["EmailSent"];
            return View(transaction);
        }

        private static string ConfirmTokenKey(int transactionId) => $"ConfirmToken_{transactionId}";
    }
}
