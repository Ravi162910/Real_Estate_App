using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Data;
using Real_Estate_App.Models;

namespace Real_Estate_App.Controllers
{
    public class AdminTransactionsController : Controller
    {
        private readonly UsersPropertiesViewingDbContext _context;
        private readonly AppDbContext _appContext;
        public AdminTransactionsController(UsersPropertiesViewingDbContext context, AppDbContext appContext)
        {
            _context = context;
            _appContext = appContext;
        }

        public IActionResult Index()
        {
            var viewings = _appContext.Transactions.Include(properties => properties.Property).ToList();
            return View(viewings);
        }

        public ActionResult Details(int ID)
        {
            var viewings = _appContext.Transactions.Include(property => property.Property).FirstOrDefault(transactionID => transactionID.TransactionId == ID);
            return View(viewings);
        }
    }
}
