using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Real_Estate_App.Models;
using Real_Estate_App.Services;
using Real_Estate_App.UnitOfWork;

namespace Real_Estate_App.Controllers
{
    [Authorize(Roles = "Admin,TransactionAdmin")]
    public class AdminTransactionsController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        private readonly IEmailService _emailService;
        private readonly ILogger<AdminTransactionsController> _logger;

        public AdminTransactionsController(IUnitOfWork unitOfWork, IEmailService emailService, ILogger<AdminTransactionsController> logger)
        {
            _unitofwork = unitOfWork;
            _emailService = emailService;
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

            transaction.Status = Transaction.StatusApproved;
            transaction.ReviewedDate = DateTime.Now;
            transaction.ReviewedByUserId = GetCurrentUserId();
            property.IsAvailable = false;

            _unitofwork.Transactions.Update(transaction);
            _unitofwork.Properties.Update(property);
            await _unitofwork.SaveChangesAsync();

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

            TempData["success"] = $"Transaction #{transaction.TransactionId} approved. The property is now marked as sold.";
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

            transaction.Status = Transaction.StatusRejected;
            transaction.ReviewedDate = DateTime.Now;
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

            TempData["success"] = $"Transaction #{transaction.TransactionId} rejected. The buyer has been notified.";
            return RedirectToAction(nameof(Index));
        }

        private int? GetCurrentUserId()
        {
            var raw = User.FindFirst("UserID")?.Value;
            return int.TryParse(raw, out var id) ? id : null;
        }
    }
}
