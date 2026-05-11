using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Real_Estate_App.Data;
using Real_Estate_App.Models;
using Real_Estate_App.Services;
using Real_Estate_App.UnitOfWork;

namespace Real_Estate_App.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<TransactionsController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;

        public TransactionsController(IEmailService emailService, ILogger<TransactionsController> logger, IUnitOfWork unitOfWork, IMemoryCache cache)
        {
            _emailService = emailService;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        // GET: Transactions/Checkout/5
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

            var submitToken = Guid.NewGuid().ToString("N");
            _cache.Set(SubmitTokenKey(submitToken), true, TimeSpan.FromMinutes(30));

            var viewModel = new CheckoutViewModel
            {
                PropertyId = property.PropertyId,
                PropertyName = property.PropertyName,
                PropertyAddress = property.PropertyAddress,
                Price = property.Price,
                SubmitToken = submitToken
            };

            return View(viewModel);
        }

        // POST: Transactions/Checkout
        // Creates a Pending transaction; the property stays available until a
        // Transaction Admin approves it from the queue.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("checkout")]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Consume the one-shot submit token. If it's missing/already-used,
            // this is a duplicate submission (refresh, double-click, back+submit).
            var tokenKey = SubmitTokenKey(model.SubmitToken);
            if (string.IsNullOrEmpty(model.SubmitToken) || !_cache.TryGetValue(tokenKey, out _))
            {
                TempData["Error"] = "This checkout has already been submitted. Please start a new purchase if you'd like to try again.";
                return RedirectToAction("Details", "Properties", new { id = model.PropertyId });
            }
            _cache.Remove(tokenKey);

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

            // Link the transaction to the buyer's account if they're logged in.
            // Anonymous buyers still get a row, with UserId = 0.
            int buyerUserId = int.TryParse(User.FindFirst("UserID")?.Value, out var uid) ? uid : 0;

            var transaction = new Transaction
            {
                PropertyId = model.PropertyId,
                UserId = buyerUserId,
                Price = property.Price,
                UserEmail = model.UserEmail,
                BuyerName = model.BuyerName,
                PurchaseDate = DateTime.UtcNow,
                Status = Transaction.StatusPending
            };

            await _unitOfWork.Transactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            try
            {
                var sent = await _emailService.SendPurchaseRequestReceivedAsync(
                    model.UserEmail,
                    model.BuyerName,
                    property.PropertyName,
                    property.PropertyAddress,
                    property.Price,
                    transaction.PurchaseDate
                );
                TempData["EmailSent"] = sent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email notification failed for transaction {TransactionId}", transaction.TransactionId);
                TempData["EmailSent"] = false;
            }

            // One-time token so an anonymous buyer can view their receipt without
            // exposing the Confirmation page to ID enumeration.
            var confirmToken = Guid.NewGuid().ToString("N");
            TempData[ConfirmTokenKey(transaction.TransactionId)] = confirmToken;

            return RedirectToAction(nameof(Confirmation), new { id = transaction.TransactionId, token = confirmToken });
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

        private static string SubmitTokenKey(string token) => $"checkout-submit:{token}";
    }
}
