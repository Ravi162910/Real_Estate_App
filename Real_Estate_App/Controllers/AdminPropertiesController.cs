using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Real_Estate_App.Data;
using Real_Estate_App.Models;
using Real_Estate_App.Services;
using Real_Estate_App.UnitOfWork;

namespace Real_Estate_App.Controllers
{
    [Authorize(Roles = "Admin,PropertyAdmin")]
    public class AdminPropertiesController : Controller
    {
        public readonly AppDbContext _appDbContext;
        private readonly IUnitOfWork _unitofwork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminPropertiesController(AppDbContext appDbContext, IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment) 
        {
            _appDbContext = appDbContext;
            _unitofwork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

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
        public async Task<ActionResult> Create(Models.Property property, IFormFile file)
        {
            if (!ImageUploadValidator.IsValid(file, out var imageError))
            {
                ModelState.AddModelError(nameof(property.ImageUrl), imageError);
            }

            ModelState.Remove("OpenHomes");// ignore openhome data before the ModelState.IsValid check
            if (!ModelState.IsValid)
            {
                return View(property);
            }

            // Save the (validated) image only after the model passes, so a
            // rejected submission never leaves an orphaned file on disk.
            if (file != null && file.Length > 0)
            {
                string wwwrootpath = _webHostEnvironment.WebRootPath;

                string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                string adminpath = Path.Combine(wwwrootpath, "images", "admin_properties");

                if (!Directory.Exists(adminpath))
                {
                    Directory.CreateDirectory(adminpath);
                }

                using (var filestream = new FileStream(Path.Combine(adminpath, filename), FileMode.Create))
                {
                    await file.CopyToAsync(filestream);
                }

                property.ImageUrl = "/images/admin_properties/" + filename;
            }

            _unitofwork.Properties.AddAsync(property);
            _unitofwork.SaveChanges();
            TempData["success"] = "Property successfully added as an admin";
            return RedirectToAction("Index");
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
        public async Task<ActionResult> Edit(Models.Property property, IFormFile? file)
        {
            if (!ImageUploadValidator.IsValid(file, out var imageError))
            {
                ModelState.AddModelError(nameof(property.ImageUrl), imageError);
            }

            ModelState.Remove("OpenHomes");
            if (!ModelState.IsValid)
            {
                return View(property);
            }

            // Load the tracked row so EF compares the [ConcurrencyCheck] on
            // IsAvailable against its real database value. Updating the posted
            // (disconnected) entity made EF use the *posted* IsAvailable as the
            // concurrency "original", so toggling availability always matched 0
            // rows and threw DbUpdateConcurrencyException.
            var existing = await _unitofwork.Properties.GetByIdAsync(property.PropertyId);
            if (existing == null)
            {
                return NotFound();
            }

            // Replace the saved image only when a new file is uploaded; delete
            // the old file using the value on disk, not the posted one.
            if (file != null && file.Length > 0)
            {
                string wwwrootpath = _webHostEnvironment.WebRootPath;

                if (!string.IsNullOrEmpty(existing.ImageUrl))
                {
                    string oldimagepath = Path.Combine(wwwrootpath, existing.ImageUrl.TrimStart('/'));

                    if (System.IO.File.Exists(oldimagepath))
                    {
                        System.IO.File.Delete(oldimagepath);
                    }
                }

                string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                string adminpath = Path.Combine(wwwrootpath, "images", "admin_properties");

                if (!Directory.Exists(adminpath))
                {
                    Directory.CreateDirectory(adminpath);
                }

                using (var filestream = new FileStream(Path.Combine(adminpath, filename), FileMode.Create))
                {
                    await file.CopyToAsync(filestream);
                }

                existing.ImageUrl = "/images/admin_properties/" + filename;
            }

            // Copy the editable fields from the posted model onto the tracked row.
            existing.PropertyName = property.PropertyName;
            existing.PropertyAddress = property.PropertyAddress;
            existing.PropertyBedrooms = property.PropertyBedrooms;
            existing.PropertyBathrooms = property.PropertyBathrooms;
            existing.PropertyPets = property.PropertyPets;
            existing.PropertyGarages = property.PropertyGarages;
            existing.ExtendedDescription = property.ExtendedDescription;
            existing.Price = property.Price;
            existing.PropertyType = property.PropertyType;
            existing.IsAvailable = property.IsAvailable;
            existing.NearbySchools = property.NearbySchools;
            existing.NearbyShops = property.NearbyShops;

            _unitofwork.SaveChanges();
            TempData["success"] = "Property successfully edited as an admin";
            return RedirectToAction("Index");
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
