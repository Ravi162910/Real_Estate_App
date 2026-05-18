using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Real_Estate_App.Models;
using Real_Estate_App.Services;

namespace Real_Estate_App.Controllers
{
    // Public "Contact Us" support form. Submissions are emailed to the support
    // inbox (admin/agent) and the sender gets an automatic acknowledgement.
    // Nothing is persisted, so no migration is needed.
    public class ContactController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ContactController> _logger;

        public ContactController(IEmailService emailService, IConfiguration configuration, ILogger<ContactController> logger)
        {
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new ContactRequestViewModel();

            // Convenience: prefill the name for a signed-in visitor. Email is
            // not stored in the auth cookie, so that field stays blank.
            if (User?.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                model.Name = User.Identity.Name;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("contact")]
        public async Task<IActionResult> Index(ContactRequestViewModel model)
        {
            // Re-validate the dropdown value server-side - never trust the
            // posted value just because the UI offered a fixed list.
            if (!string.IsNullOrEmpty(model.Problem) && !ContactRequestViewModel.ProblemOptions.Contains(model.Problem))
            {
                ModelState.AddModelError(nameof(model.Problem), "Please choose a valid option from the list");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var supportEmail = smtpSettings["SupportEmail"];
            if (string.IsNullOrWhiteSpace(supportEmail))
            {
                supportEmail = smtpSettings["SenderEmail"];
            }

            if (string.IsNullOrWhiteSpace(supportEmail))
            {
                _logger.LogWarning("Contact form submitted but no SupportEmail/SenderEmail is configured.");
                TempData["error"] = "Sorry, the contact form is unavailable right now. Please try again later.";
                return View(model);
            }

            var submittedDate = DateTime.UtcNow;

            var notified = await _emailService.SendContactNotificationAsync(
                supportEmail, model.Name, model.Email, model.Problem, model.Comments, submittedDate);

            if (!notified)
            {
                TempData["error"] = "We could not send your message right now. Please try again in a moment.";
                return View(model);
            }

            // Acknowledgement is best-effort: the request was already received,
            // so a failed confirmation email should not block the user.
            await _emailService.SendContactAcknowledgementAsync(model.Email, model.Name, model.Problem);

            TempData["success"] = "Thanks! Your message has been sent. Our team will be in touch with you shortly.";
            return RedirectToAction(nameof(Index));
        }
    }
}
