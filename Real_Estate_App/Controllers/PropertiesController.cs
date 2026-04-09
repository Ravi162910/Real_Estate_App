using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Real_Estate_App.Data;
using Real_Estate_App.Models;

namespace Real_Estate_App.Controllers
{
    public class PropertiesController : Controller
    {
        private readonly AppDbContext _context;

        private readonly UsersPropertiesViewingDbContext _viewingDbContext;

        public PropertiesController(AppDbContext context, UsersPropertiesViewingDbContext viewingDbContext)
        {
            _context = context;
            _viewingDbContext = viewingDbContext;
        }

        // GET: Properties
        public async Task<IActionResult> Index(string? searchString, string? propertyType,
            int? minBedrooms, int? maxBedrooms, int? minBathrooms, int? maxBathrooms,
            decimal? minPrice, decimal? maxPrice, int? minGarages, int? minPets)
        {
            var properties = _context.Properties.Where(p => p.IsAvailable);

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
            ViewData["PropertyTypes"] = await _context.Properties
                .Select(p => p.PropertyType)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            return View(await properties.ToListAsync());
        }

        // GET: Properties/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.PropertyId == id);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        // GET: Properties/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Properties/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PropertyName,PropertyAddress,PropertyBedrooms,PropertyBathrooms,PropertyPets,PropertyGarages,ExtendedDescription,Price,PropertyType,IsAvailable,NearbySchools,NearbyShops")] Models.Property property)
        {
            if (ModelState.IsValid)
            {
                _context.Add(property);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(property);
        }

        // GET: Properties/Edit/5
        public async Task<IActionResult> Edit(int? id)
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
            return View(property);
        }

        // POST: Properties/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PropertyId,PropertyName,PropertyAddress,PropertyBedrooms,PropertyBathrooms,PropertyPets,PropertyGarages,ExtendedDescription,Price,PropertyType,IsAvailable,NearbySchools,NearbyShops")] Models.Property property)
        {
            if (id != property.PropertyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(property);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(property);
        }

        // GET: Properties/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.PropertyId == id);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        // POST: Properties/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property != null)
            {
                _context.Properties.Remove(property);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Viewing(int PropertyID, int UserID, Viewing viewingobj)
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;

            if (userIdClaim == null)// If no User is logged in TODO: Add a message popup to ask the user to kindly login or register
            {
                return RedirectToAction("Login","UserAdmin");
            }

            int userID = int.Parse(userIdClaim);

            if (ModelState.IsValid)
            {
                viewingobj.PropertyID = PropertyID;
                viewingobj.UserID = userID;

                _viewingDbContext.ViewingSet.Add(viewingobj);
                await _viewingDbContext.SaveChangesAsync();

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

        public IActionResult OwnUserViewing() 
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;

            if (userIdClaim == null)// If no User is logged in TODO: Add a message popup to ask the user to kindly login or register
            {
                return RedirectToAction("Login", "UserAdmin");// Probably replace this with something else?
            }

            int userID = int.Parse(userIdClaim);
            var propertiesviewed = _viewingDbContext.ViewingSet.Where(viewing => viewing.UserID == userID).Include(viewingproperty => viewingproperty.Properties).ToList();
            return View(propertiesviewed);
        }
    }
}
