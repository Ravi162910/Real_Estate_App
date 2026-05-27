using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Real_Estate_App.Data;
using Real_Estate_App.Models;
using Real_Estate_App.UnitOfWork;

namespace Real_Estate_App.Controllers
{
    public class AgentOpenHomeController : Controller
    {
        public readonly IUnitOfWork _unitofwork;
        public readonly AppDbContext _context;

        public AgentOpenHomeController(IUnitOfWork unitofwork, AppDbContext appDbContext)
        {
            _unitofwork = unitofwork;
            _context = appDbContext;
        }




        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> Index(string? searchString, string? propertyType,
                    int? minBedrooms, int? maxBedrooms, int? minBathrooms, int? maxBathrooms,
                    decimal? minPrice, decimal? maxPrice, int? minGarages, int? minPets)
        {

            var unitofwork = await _unitofwork.Properties.GetAvailableFilteredAsync(searchString, propertyType, minBedrooms, maxBedrooms, minBathrooms, maxBathrooms, minPrice, maxPrice, minGarages, minPets, availableOnly: false);

            // Pass current filter values back to the view
            ViewData["SearchString"] = searchString;
            ViewData["PropertyType"] = propertyType;
            ViewData["MinBedrooms"] = minBedrooms;
            ViewData["MaxBedrooms"] = maxBedrooms;
            ViewData["MinBathrooms"] = minBathrooms;
            ViewData["MaxBathrooms"] = maxBathrooms;
            ViewData["MinPrice"] = minPrice;
            ViewData["MaxPrice"] = maxPrice;
            ViewData["MinGarages"] = minGarages;
            ViewData["MinPets"] = minPets;

            // Get distinct property types for the dropdown
            ViewData["PropertyTypes"] = await _unitofwork.Properties.GetDistinctPropertyTypesAsync();

            return View(unitofwork.ToList());
        }

        [HttpGet]
        [Authorize(Roles = "Agent")]
        public IActionResult AddOpenHome(int propertyId)
        {
            var openhome = new OpenHome
            {
                PropertyId = propertyId,
            };
            return View(openhome);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Agent")]
        public IActionResult AddOpenHome(OpenHome openHome)
        {
            if (ModelState.IsValid)
            {
                // PropertyId arrives via the hidden field; OpenHomeId is a DB identity.
                _context.OpenHomesSet.Add(openHome);
                _context.SaveChanges();

                TempData["success"] = "Added an open home";

                return RedirectToAction("Index");
            }
            return View(openHome);
        }
    }
}
