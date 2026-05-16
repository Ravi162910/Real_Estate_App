using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Models;
using Real_Estate_App.Services;
using Real_Estate_App.UnitOfWork;
using Stripe;

namespace Real_Estate_App.Controllers
{
    [Authorize(Roles = "Admin,TransactionAdmin")]
    public class AdminTransactionsController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        private readonly IEmailService _emailService;
        private readonly IStripeService _stripeService;
        private readonly ILogger<AdminTransactionsController> _logger;

        public AdminTransactionsController(IUnitOfWork unitOfWork, IEmailService emailService, IStripeService stripeService, ILogger<AdminTransactionsController> logger)
        {
            _unitofwork = unitOfWork;
            _emailService = emailService;
            _stripeService = stripeService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? status = null)
        {
            var filter = string.IsNullOrWhiteSpace(status) ? Transaction.StatusPending : status;
            var transactions = await _unitofwork.Transactions.GetByStatusAsync(filter);

            ViewData["CurrentFilter"] = filter;
            ViewData["PendingCount"] = await _unitofwork.Transactions.CountByStatusAsync(Transaction.StatusPending);
            ViewData["ApprovedCount"] = await _unitofwork.Transactions.CountByStatusAsync(Transaction.StatusApproved);
            ViewData["RejectedCount"] = await _unitofwork.Transactions.CountByStatusAsync(Transaction.StatusRejected);

            return View(transactions);
        }

        public async Task<ActionResult> Details(int ID)
        {
            var transaction = await _unitofwork.Transactions.GetByIdWithPropertyAsync(ID);
            if (transaction == null)
            {
                return NotFound();
            }
            return View(transaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var transaction = await _unitofwork.Transactions.GetByIdWithPropertyAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            if (transaction.Status != Transaction.StatusPending)
            {
                TempData["error"] = $"Transaction #{transaction.TransactionId} is already {transaction.Status} and cannot be changed.";
                return RedirectToAction(nameof(Details), new { ID = id });
            }

            var property = transaction.Property
                ?? await _unitofwork.Properties.GetByIdAsync(transaction.PropertyId);

            if (property == null)
            {
                TempData["error"] = "The property linked to this transaction no longer exists.";
                return RedirectToAction(nameof(Details), new { ID = id });
            }

            if (!property.IsAvailable)
            {
                TempData["error"] = "This property has already been sold to another buyer. The request has not been approved.";
                return RedirectToAction(nameof(Details), new { ID = id });
            }

            // Capture the buyer's authorized payment. Until now the card was
            // only held - this is the point the buyer is actually charged.
            // If capture fails (commonly: the ~7-day authorization expired),
            // do NOT mark the sale; the admin should reject instead.
            if (!string.IsNullOrEmpty(transaction.StripePaymentIntentId))
            {
                try
                {
                    await _stripeService.CapturePaymentIntentAsync(transaction.StripePaymentIntentId);
                }
                catch (StripeException ex)
                {
                    _logger.LogError(ex, "Capture failed for transaction {TransactionId} (PI {PaymentIntentId})",
                        transaction.TransactionId, transaction.StripePaymentIntentId);
                    TempData["error"] = "The buyer's card authorization could not be captured - it may have expired. Reject this request instead so the buyer is notified.";
                    return RedirectToAction(nameof(Details), new { ID = id });
                }
            }

            transaction.Status = Transaction.StatusApproved;
            transaction.ReviewedDate = DateTime.UtcNow;
            transaction.ReviewedByUserId = GetCurrentUserId();
            property.IsAvailable = false;

            _unitofwork.Transactions.Update(transaction);
            _unitofwork.Properties.Update(property);

            try
            {
                await _unitofwork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Another admin approved a different request for the same
                // property in parallel and won the race - but we already
                // captured this buyer's funds. Refund them automatically and
                // auto-reject this request so no one is left out of pocket.
                bool refunded = false;
                if (!string.IsNullOrEmpty(transaction.StripePaymentIntentId))
                {
                    try
                    {
                        await _stripeService.RefundPaymentIntentAsync(transaction.StripePaymentIntentId);
                        refunded = true;
                    }
                    catch (StripeException sx)
                    {
                        _logger.LogError(sx,
                            "Auto-refund FAILED for transaction {TransactionId} (PI {PaymentIntentId}) after losing the property race - manual refund required.",
                            transaction.TransactionId, transaction.StripePaymentIntentId);
                    }
                }

                // The SaveChanges rolled back. Reload the conflicting entity
                // (Property) so its refreshed concurrency token doesn't make
                // the next save fail too - we only want to persist the
                // transaction's new Rejected state now.
                foreach (var entry in ex.Entries)
                {
                    await entry.ReloadAsync();
                }

                transaction.Status = Transaction.StatusRejected;
                transaction.ReviewedDate = DateTime.UtcNow;
                transaction.ReviewedByUserId = GetCurrentUserId();
                transaction.RejectionReason = refunded
                    ? "The property was sold to another buyer first. Your payment has been refunded in full."
                    : "The property was sold to another buyer first. A refund is being processed.";
                _unitofwork.Transactions.Update(transaction);

                try
                {
                    await _unitofwork.SaveChangesAsync();
                }
                catch (Exception persistEx)
                {
                    _logger.LogError(persistEx,
                        "Failed to record auto-rejection for transaction {TransactionId}", transaction.TransactionId);
                }

                try
                {
                    await _emailService.SendPurchaseRejectedAsync(
                        transaction.UserEmail,
                        transaction.BuyerName,
                        property.PropertyName,
                        property.PropertyAddress,
                        transaction.Price,
                        transaction.ReviewedDate.Value,
                        transaction.RejectionReason);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx,
                        "Failed to send auto-rejection email for transaction {TransactionId}", transaction.TransactionId);
                }

                TempData["error"] = refunded
                    ? $"This property was just sold to another buyer by a different admin. Transaction #{transaction.TransactionId} was auto-rejected and the buyer's payment was refunded automatically."
                    : $"This property was just sold to another buyer. Transaction #{transaction.TransactionId} was auto-rejected, but the automatic refund FAILED - a manual refund is required in the Stripe dashboard.";
                return RedirectToAction(nameof(Details), new { ID = id });
            }

            try
            {
                await _emailService.SendPurchaseApprovedAsync(
                    transaction.UserEmail,
                    transaction.BuyerName,
                    property.PropertyName,
                    property.PropertyAddress,
                    transaction.Price,
                    transaction.ReviewedDate.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send approval email for transaction {TransactionId}", transaction.TransactionId);
            }

            TempData["success"] = $"Transaction #{transaction.TransactionId} approved. The buyer's card has been charged and the property is now marked as sold.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? rejectionReason)
        {
            var transaction = await _unitofwork.Transactions.GetByIdWithPropertyAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            if (transaction.Status != Transaction.StatusPending)
            {
                TempData["error"] = $"Transaction #{transaction.TransactionId} is already {transaction.Status} and cannot be changed.";
                return RedirectToAction(nameof(Details), new { ID = id });
            }

            // Release the buyer's card authorization (no charge ever happens).
            // If it was somehow already captured, block the rejection so it
            // isn't silently rejected while the buyer is out the money.
            if (!string.IsNullOrEmpty(transaction.StripePaymentIntentId))
            {
                try
                {
                    var pi = await _stripeService.GetPaymentIntentAsync(transaction.StripePaymentIntentId);
                    if (pi.Status == "requires_capture")
                    {
                        await _stripeService.CancelPaymentIntentAsync(transaction.StripePaymentIntentId);
                    }
                    else if (pi.Status == "succeeded")
                    {
                        _logger.LogWarning(
                            "Reject blocked: PI {PaymentIntentId} for transaction {TransactionId} is already captured.",
                            pi.Id, transaction.TransactionId);
                        TempData["error"] = "This payment was already captured and must be refunded in the Stripe dashboard before the request can be rejected.";
                        return RedirectToAction(nameof(Details), new { ID = id });
                    }
                    // "canceled" already (e.g. expired authorization): nothing
                    // to do - fall through and record the rejection.
                }
                catch (StripeException ex)
                {
                    _logger.LogError(ex, "Could not release authorization for transaction {TransactionId} (PI {PaymentIntentId})",
                        transaction.TransactionId, transaction.StripePaymentIntentId);
                    TempData["error"] = "Could not release the payment authorization with Stripe. Please try again.";
                    return RedirectToAction(nameof(Details), new { ID = id });
                }
            }

            transaction.Status = Transaction.StatusRejected;
            transaction.ReviewedDate = DateTime.UtcNow;
            transaction.ReviewedByUserId = GetCurrentUserId();
            transaction.RejectionReason = string.IsNullOrWhiteSpace(rejectionReason) ? null : rejectionReason.Trim();

            _unitofwork.Transactions.Update(transaction);
            await _unitofwork.SaveChangesAsync();

            try
            {
                var property = transaction.Property
                    ?? await _unitofwork.Properties.GetByIdAsync(transaction.PropertyId);

                if (property != null)
                {
                    await _emailService.SendPurchaseRejectedAsync(
                        transaction.UserEmail,
                        transaction.BuyerName,
                        property.PropertyName,
                        property.PropertyAddress,
                        transaction.Price,
                        transaction.ReviewedDate.Value,
                        transaction.RejectionReason);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send rejection email for transaction {TransactionId}", transaction.TransactionId);
            }

            TempData["success"] = $"Transaction #{transaction.TransactionId} rejected. The card hold has been released (the buyer was never charged) and they have been notified.";
            return RedirectToAction(nameof(Index));
        }

        private int? GetCurrentUserId()
        {
            var raw = User.FindFirst("UserID")?.Value;
            return int.TryParse(raw, out var id) ? id : null;
        }
    }
}
