using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Data;
using Real_Estate_App.Models;

namespace Real_Estate_App.Controllers
{
    public class AdminTransactionsController : Controller
    {
        private readonly AppDbContext _appContext;
        public AdminTransactionsController(AppDbContext appContext)
        {
            _appContext = appContext;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var viewings = _appContext.Transactions.Include(properties => properties.Property).ToList();
            return View(viewings);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Details(int ID)
        {
            var viewings = _appContext.Transactions.Include(property => property.Property).FirstOrDefault(transactionID => transactionID.TransactionId == ID);
            return View(viewings);
        }
    }
}
