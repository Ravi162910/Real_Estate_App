using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Data;
using Real_Estate_App.Models;
using Real_Estate_App.Services;

namespace Real_Estate_App.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(AppDbContext context, IEmailService emailService, ILogger<TransactionsController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            var transactions = await _context.Transactions
                .Include(t => t.Property)
                .OrderByDescending(t => t.PurchaseDate)
                .ToListAsync();

            return View(transactions);
        }

        // GET: Transactions/Checkout/5
        public async Task<IActionResult> Checkout(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties.FindAsync(id);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var property = await _context.Properties.FindAsync(model.PropertyId);
            if (property == null)
            {
                return NotFound();
            }

            if (!property.IsAvailable)
            {
                TempData["Error"] = "This property is no longer available for purchase.";
                return RedirectToAction("Details", "Properties", new { id = model.PropertyId });
            }

            // Create the transaction
            var transaction = new Transaction
            {
                PropertyId = model.PropertyId,
                UserId = 0, // Guest checkout - no user login required
                Price = property.Price,
                UserEmail = model.UserEmail,
                BuyerName = model.BuyerName,
                PurchaseDate = DateTime.Now
            };

            // Mark the property as no longer available
            property.IsAvailable = false;

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Send email notification
            try
            {
                await _emailService.SendPurchaseConfirmationAsync(
                    model.UserEmail,
                    model.BuyerName,
                    property.PropertyName,
                    property.PropertyAddress,
                    property.Price,
                    transaction.PurchaseDate
                );
                TempData["EmailSent"] = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email notification failed for transaction {TransactionId}", transaction.TransactionId);
                TempData["EmailSent"] = false;
            }

            return RedirectToAction(nameof(Confirmation), new { id = transaction.TransactionId });
        }

        // GET: Transactions/Confirmation/5
        public async Task<IActionResult> Confirmation(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Property)
                .FirstOrDefaultAsync(t => t.TransactionId == id);

            if (transaction == null)
            {
                return NotFound();
            }

            ViewData["EmailSent"] = TempData["EmailSent"];
            return View(transaction);
        }
    }
}
