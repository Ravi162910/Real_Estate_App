using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Real_Estate_App.Data;
using Real_Estate_App.Models;
using Real_Estate_App.UnitOfWork;

namespace Real_Estate_App.Controllers
{
    public class AdminPropertiesController : Controller
    {
        public readonly AppDbContext _appDbContext;
        private readonly IUnitOfWork _unitofwork;
        public AdminPropertiesController(AppDbContext appDbContext, IUnitOfWork unitOfWork) 
        {
            _appDbContext = appDbContext;
            _unitofwork = unitOfWork;
        }

        public async Task<IActionResult> Index(string? searchString, string? propertyType,
                    int? minBedrooms, int? maxBedrooms, int? minBathrooms, int? maxBathrooms,
                    decimal? minPrice, decimal? maxPrice, int? minGarages, int? minPets)
        {

            var unitofwork = await _unitofwork.Properties.GetAvailableFilteredAsync(searchString, propertyType, minBedrooms, maxBedrooms, minBathrooms, maxBathrooms, minPrice, maxPrice, minGarages, minPets);

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

        public async Task<IActionResult> Details(int id) 
        {
            var property = await _unitofwork.Properties.GetByIdAsync(id);
            if (property == null) 
            {
                return NotFound();
            }
            return View(property);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Models.Property property)
        {
            if (ModelState.IsValid) 
            {
                _unitofwork.Properties.AddAsync(property);
                _unitofwork.SaveChanges();
                TempData["success"] = "Property successfully added as an admin";
                return RedirectToAction("Index");
            }
            return View(property);
        }


        public async Task<ActionResult> Edit(int ID)
        {
            if (ID == 0)
            {
                return NotFound();
            }
            var propertyId = await _unitofwork.Properties.GetByIdAsync(ID);
            if(propertyId == null)
            {
                return NotFound();
            }
            return View(propertyId);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Models.Property property)
        {
            if (ModelState.IsValid) 
            {
                _unitofwork.Properties.Update(property);
                _unitofwork.SaveChanges();
                TempData["success"] = "Property successfully edited as an admin";
                return RedirectToAction("Index");
            }
            return View();
        }


        public async Task<ActionResult> Delete(int ID)
        {
            if (ID == null || ID == 0) 
            {
                return NotFound();
            }

            var propertyID = await _unitofwork.Properties.GetByIdAsync(ID);
            if (propertyID == null) 
            {
                return NotFound();
            }

            return View(propertyID);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirm(int ID, Models.Property property)
        {
            var userID = await _unitofwork.Properties.GetByIdAsync(ID); 
            if (userID == null) 
            {
                return NotFound();
            }
            _unitofwork.Properties.Remove(userID);
            _unitofwork.SaveChanges();
            TempData["success"] = "Property successfully deleted as an admin";
            return RedirectToAction("Index");
        }
    }
}
