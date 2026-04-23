using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Data;
using Real_Estate_App.Models;
using Real_Estate_App.UnitOfWork;

namespace Real_Estate_App.Controllers
{
    public class AdminTransactionsController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        public AdminTransactionsController(IUnitOfWork unitOfWork)
        {
            _unitofwork = unitOfWork;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var viewings = await _unitofwork.Transactions.GetAllWithPropertyAsync();
            return View(viewings);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Details(int ID)
        {
            var viewings = await _unitofwork.Transactions.GetByIdWithPropertyAsync(ID);
            return View(viewings);
        }
    }
}
