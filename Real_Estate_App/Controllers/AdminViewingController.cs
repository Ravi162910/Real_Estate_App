using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Data;
using Real_Estate_App.Models;
using Real_Estate_App.UnitOfWork;

namespace Real_Estate_App.Controllers
{
    public class AdminViewingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IUnitOfWork _unitofwork;
        public AdminViewingController(AppDbContext context, IUnitOfWork unitOfWork )
        {
            _context = context;
            _unitofwork = unitOfWork;
        }

        [Authorize(Roles = "Admin,PropertyAdmin")]
        public async Task<IActionResult> Index()
        {
            var viewings = await _unitofwork.Viewings.GetAllWithUserAndPropertyAsync();
            return View(viewings);
        }

        [Authorize(Roles = "Admin,PropertyAdmin")]
        public async Task<ActionResult> Details(int ID)
        {
            var viewings = await _unitofwork.Viewings.GetByIdWithUserAndPropertyAsync(ID);
            return View(viewings);
        }

        [Authorize(Roles = "Admin,PropertyAdmin")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,PropertyAdmin")]
        public ActionResult Create(int propertyID, int userID)
        {
            var properties = _context.Properties.Select(property => new { PropertyID = property.PropertyId, PropertyName = property.PropertyName }).ToList();

            var users = _context.UsersandAdminsset.Select(user => new { UserID = user.UserID, UserName = user.UserName }).ToList();

            ViewData["PropertyID"] = new SelectList(properties, "PropertyID", "PropertyName");
            ViewData["UserID"] = new SelectList(users, "UserID", "UserName");

            ViewBag.AvailableUsers = _context.UsersandAdminsset.Any();
            ViewBag.AvailableProperties = _context.Properties.Any();

            return View();

        }

        [HttpPost]
        [Authorize(Roles = "Admin,PropertyAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Viewing viewing)
        {

            var properties = _context.Properties.Select(property => new { PropertyID = property.PropertyId, PropertyName = property.PropertyName }).ToList();

            var users = _context.UsersandAdminsset.Select(user => new { UserID = user.UserID, UserName = user.UserName }).ToList();

            ViewData["PropertyID"] = new SelectList(properties, "PropertyID", "PropertyName");
            ViewData["UserID"] = new SelectList(users, "UserID", "UserName");

            ViewBag.AvailableUsers = _context.UsersandAdminsset.Any();
            ViewBag.AvailableProperties = _context.Properties.Any();

            if (ModelState.IsValid)
            {
                _unitofwork.Viewings.AddAsync(viewing);
                _unitofwork.SaveChanges();
                TempData["success"] = "Viewing successfully added as an admin";
                return RedirectToAction("Index");
            }
            
            return View(viewing);
        }

        [Authorize(Roles = "Admin,PropertyAdmin")]
        public async Task<ActionResult> Edit(int ID)
        {
            if (ID == 0)
            {
                return NotFound();
            }
            var viewing = await _unitofwork.Viewings.GetByIdWithUserAndPropertyAsync(ID);
            if (viewing == null)
            {
                return NotFound();
            }
            return View(viewing);
        }


        [HttpPost]
        [Authorize(Roles = "Admin,PropertyAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Viewing viewing)
        {
            _unitofwork.Viewings.Update(viewing);
            _unitofwork.SaveChanges();
            TempData["success"] = "Viewing successfully edited as an admin";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,PropertyAdmin")]
        public async Task<ActionResult> Delete(int ID)
        {
            if (ID == 0)
            {
                return NotFound();
            }

            var viewing = await _unitofwork.Viewings.GetByIdWithUserAndPropertyAsync(ID);
            if (viewing == null)
            {
                return NotFound();
            }

            return View(viewing);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirm(int ID, Viewing viewing)
        {
            var viewingID = _context.ViewingSet.Find(ID);

            if (viewingID == null) 
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _unitofwork.Viewings.Remove(viewingID);
                _unitofwork.SaveChanges();
                TempData["success"] = "Viewing successfully deleted as an admin";
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
