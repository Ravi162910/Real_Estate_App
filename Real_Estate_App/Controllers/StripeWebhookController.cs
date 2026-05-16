using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Real_Estate_App.Services;
using Stripe;

namespace Real_Estate_App.Controllers
{
    // Server-to-server endpoint Stripe calls when a payment completes. It is
    // the authoritative confirmation of payment - the success redirect is a
    // best-effort UX shortcut and may never arrive (closed tab, etc.).
    //
    // For a localhost demo, forward events with the Stripe CLI:
    //   stripe listen --forward-to https://localhost:<port>/stripe/webhook
    [AllowAnonymous]
    [ApiController]
    [Route("stripe/webhook")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly StripeSettings _settings;
        private readonly IStripeService _stripeService;
        private readonly ICheckoutFulfillmentService _fulfillment;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(
            IOptions<StripeSettings> settings,
            IStripeService stripeService,
            ICheckoutFulfillmentService fulfillment,
            ILogger<StripeWebhookController> logger)
        {
            _settings = settings.Value;
            _stripeService = stripeService;
            _fulfillment = fulfillment;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrWhiteSpace(_settings.WebhookSecret))
            {
                _logger.LogError("Stripe webhook secret is not configured; rejecting webhook.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            Event stripeEvent;
            try
            {
                // Verifies the HMAC signature against the webhook secret. A
                // forged/replayed body without a valid signature throws here,
                // so unauthenticated callers can't fabricate payments.
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _settings.WebhookSecret);
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Rejected Stripe webhook with bad signature.");
                return BadRequest();
            }

            if (stripeEvent.Type == "checkout.session.completed")
            {
                if (stripeEvent.Data.Object is not Stripe.Checkout.Session sessionStub
                    || string.IsNullOrEmpty(sessionStub.Id))
                {
                    _logger.LogWarning("checkout.session.completed event had no session payload.");
                    return Ok();
                }

                // Re-fetch from Stripe so paid state / amount are authoritative
                // and fulfilment is identical to the redirect path.
                Stripe.Checkout.Session session;
                try
                {
                    session = await _stripeService.GetSessionAsync(sessionStub.Id);
                }
                catch (StripeException ex)
                {
                    _logger.LogError(ex, "Webhook could not retrieve session {SessionId}", sessionStub.Id);
                    // 500 -> Stripe will retry later.
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }

                var result = await _fulfillment.FulfillAsync(session);
                _logger.LogInformation("Webhook fulfilled session {SessionId}: {Outcome}",
                    session.Id, result.Outcome);
            }

            // Acknowledge everything else so Stripe stops retrying.
            return Ok();
        }
    }
}
