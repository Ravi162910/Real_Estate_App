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

        public IActionResult Index()
        {
            var viewings = _context.ViewingSet.Include(viewing => viewing.Properties).Include(users => users.Users).ToList();
            return View(viewings);
        }

        public ActionResult Details(int ID)
        {
            var viewings = _context.ViewingSet.Include(viewing => viewing.Properties).Include(users => users.Users).FirstOrDefault(viewingID => viewingID.Viewing_ID == ID);
            return View(viewings);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Create(int propertyID, int userID)
        {
            var properties = _context.ViewingSet.Select(p => new {PropertyID = p.PropertyID, PropertyName = p.Properties.PropertyName}).ToList();

            var users = _context.ViewingSet.Select(u => new {UserID = u.UserID, UserName = u.Users.UserName}).ToList();

            ViewData["PropertyID"] = new SelectList(properties, "PropertyID", "PropertyName");

            ViewData["UserID"] = new SelectList(users, "UserID", "UserName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Viewing viewing)
        {
            if (ModelState.IsValid)
            {
                _context.ViewingSet.Add(viewing);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewData["PropertyID"] = new SelectList(_context.ViewingSet.Include(property => property.Properties).Include(propertyname => propertyname.Properties), "PropertyID", "PropertyName");
            ViewData["UserID"] = new SelectList(_context.ViewingSet.Include(user => user.Users).Include(userName => userName.Users), "UserID", "UserName");

            return View(viewing);
        }


        public ActionResult Edit(int ID)
        {
            if (ID == null || ID == 0)
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
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Viewing viewing)
        {
            ModelState.Remove("Properties.PropertyType");
            ModelState.Remove("Properties.Price");
            if (ModelState.IsValid)
            {
                _context.ViewingSet.Update(viewing);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(viewing);
        }


        public ActionResult Delete(int ID)
        {
            if (ID == null || ID == 0)
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
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
