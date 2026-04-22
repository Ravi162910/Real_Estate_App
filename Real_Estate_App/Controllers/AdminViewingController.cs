using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Data;
using Real_Estate_App.Models;

namespace Real_Estate_App.Controllers
{
    public class AdminViewingController : Controller
    {
        private readonly UsersPropertiesViewingDbContext _context;
        private readonly AppDbContext _appContext;
        public AdminViewingController(UsersPropertiesViewingDbContext context, AppDbContext appContext)
        {
            _context = context;
            _appContext = appContext;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var viewings = _context.ViewingSet.Include(viewing => viewing.Properties).Include(users => users.Users).ToList();
            return View(viewings);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Details(int ID)
        {
            var viewings = _context.ViewingSet.Include(viewing => viewing.Properties).Include(users => users.Users).FirstOrDefault(viewingID => viewingID.Viewing_ID == ID);
            return View(viewings);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
                _context.ViewingSet.Add(viewing);
                _context.SaveChanges();
                TempData["success"] = "Viewing successfully added as an admin";
                return RedirectToAction("Index");
            }
            
            return View(viewing);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int ID)
        {
            if (ID == 0)
            {
                return NotFound();
            }
            var viewing = _context.ViewingSet.Include(user => user.Users).Include(property => property.Properties).FirstOrDefault(viewings => viewings.Viewing_ID == ID);
            if (viewing == null)
            {
                return NotFound();
            }
            return View(viewing);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Viewing viewing)
        {
            _context.ViewingSet.Update(viewing);
            _context.SaveChanges();
            TempData["success"] = "Viewing successfully edited as an admin";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int ID)
        {
            if (ID == 0)
            {
                return NotFound();
            }

            var viewing = _context.ViewingSet.Include(user => user.Users).Include(property => property.Properties).FirstOrDefault(viewings => viewings.Viewing_ID == ID);
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
                _context.ViewingSet.Remove(viewingID);
                _context.SaveChanges(true);
                TempData["success"] = "viewing successfully deleted as an admin";
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
