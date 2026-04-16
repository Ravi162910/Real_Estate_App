using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Real_Estate_App.Data;
using Real_Estate_App.Models;

namespace Real_Estate_App.Controllers
{
    public class AdminPropertiesController : Controller
    {
        public readonly AppDbContext _appDbContext;
        public AdminPropertiesController(AppDbContext appDbContext) 
        {
            _appDbContext = appDbContext;
        }

        public async Task<IActionResult> Index(string? searchString, string? propertyType,
                    int? minBedrooms, int? maxBedrooms, int? minBathrooms, int? maxBathrooms,
                    decimal? minPrice, decimal? maxPrice, int? minGarages, int? minPets)
        {
            var properties = _appDbContext.Properties.Where(p => p.IsAvailable);

            // Search by name or address
            if (!string.IsNullOrEmpty(searchString))
            {
                properties = properties.Where(p =>
                    p.PropertyName.Contains(searchString) ||
                    p.PropertyAddress.Contains(searchString));
            }

            // Filter by property type
            if (!string.IsNullOrEmpty(propertyType))
            {
                properties = properties.Where(p => p.PropertyType == propertyType);
            }

            // Filter by bedrooms
            if (minBedrooms.HasValue)
                properties = properties.Where(p => p.PropertyBedrooms >= minBedrooms.Value);
            if (maxBedrooms.HasValue)
                properties = properties.Where(p => p.PropertyBedrooms <= maxBedrooms.Value);

            // Filter by bathrooms
            if (minBathrooms.HasValue)
                properties = properties.Where(p => p.PropertyBathrooms >= minBathrooms.Value);
            if (maxBathrooms.HasValue)
                properties = properties.Where(p => p.PropertyBathrooms <= maxBathrooms.Value);

            // Filter by price
            if (minPrice.HasValue)
                properties = properties.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                properties = properties.Where(p => p.Price <= maxPrice.Value);

            // Filter by garages
            if (minGarages.HasValue)
                properties = properties.Where(p => p.PropertyGarages >= minGarages.Value);

            // Filter by pets allowed
            if (minPets.HasValue)
                properties = properties.Where(p => p.PropertyPets >= minPets.Value);

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
            ViewData["PropertyTypes"] = await _appDbContext.Properties
                .Select(p => p.PropertyType)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            return View(await properties.ToListAsync());
        }

        public IActionResult Details(int id) 
        {
            var property = _appDbContext.Properties.FirstOrDefault(property => property.PropertyId == id);
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
                _appDbContext.Properties.Add(property);
                _appDbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(property);
        }


        public ActionResult Edit(int ID)
        {
            if (ID == null || ID == 0)
            {
                return NotFound();
            }
            var propertyId = _appDbContext.Properties.Find(ID);
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
                _appDbContext.Properties.Update(property);
                _appDbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }


        public ActionResult Delete(int? ID)
        {
            if (ID == null || ID == 0) 
            {
                return NotFound();
            }

            var propertyID = _appDbContext.Properties.Find(ID);
            if (propertyID == null) 
            {
                return NotFound();
            }

            return View(propertyID);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirm(int ID, Models.Property property)
        {
            var userID = _appDbContext.Properties.Find(ID); 
            if (userID == null) 
            {
                return NotFound();
            }
            _appDbContext.Properties.Remove(userID);
            _appDbContext.SaveChanges(true);
            return RedirectToAction("Index");
        }
    }
}
