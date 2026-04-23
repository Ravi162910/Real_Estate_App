using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Real_Estate_App.Data;
using Real_Estate_App.Models;
using Real_Estate_App.UnitOfWork;

namespace Real_Estate_App.Controllers
{
    public class PropertiesController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        public PropertiesController(IUnitOfWork unitOfWork)
        {
            _unitofwork = unitOfWork;
        }

        // GET: Properties
        public async Task<IActionResult> Index(string? searchString, string? propertyType,
                    int? minBedrooms, int? maxBedrooms, int? minBathrooms, int? maxBathrooms,
                    decimal? minPrice, decimal? maxPrice, int? minGarages, int? minPets)
        {

            var properties = await _unitofwork.Properties.GetAvailableFilteredAsync(searchString, propertyType, minBedrooms, maxBedrooms, minBathrooms, maxBathrooms, minPrice, maxPrice, minGarages, minPets);

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

            return View(properties.ToList());
        }

        // GET: Properties/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var property = await _unitofwork.Properties.GetByIdAsync(id);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        [HttpPost]
        public async Task<IActionResult> Viewing(int PropertyID, int UserID, Viewing viewingobj)
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;

            if (userIdClaim == null)
            {
                TempData["warning"] = "Please log in before booking for any properties";
                return RedirectToAction("Login","UserAdmin");
            }

            int userID = int.Parse(userIdClaim);

            if (ModelState.IsValid)
            {
                viewingobj.PropertyID = PropertyID;
                viewingobj.UserID = userID;

                await _unitofwork.Viewings.AddAsync(viewingobj);
                await _unitofwork.SaveChangesAsync();
                TempData["success"] = "Viewing Booked Successfully!";
                return RedirectToAction("Index");
            }

            return View(viewingobj);
        }

        [HttpGet]
        public IActionResult Viewing(int propertyID, int userID) 
        {
            var modelused = new Viewing
            {
                PropertyID = propertyID,
                UserID = userID
            };
            return View(modelused);
        }

        public IActionResult Viewing() 
        {
            return View();
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> OwnUserViewing()
        {
            var userID = int.Parse(User.FindFirst("UserID")!.Value);

            var propertiesviewed = await _unitofwork.Viewings.GetByUserIdAsync(userID);

            return View(propertiesviewed);
        }
    }
}
