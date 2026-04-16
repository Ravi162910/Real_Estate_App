using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Data;
using Real_Estate_App.Models;

namespace Real_Estate_App.Controllers
{
    public class AdminUsersDashboardController : Controller
    {
        private readonly UsersPropertiesViewingDbContext _userContext;
        public AdminUsersDashboardController(UsersPropertiesViewingDbContext usersContext)
        {
            _userContext = usersContext;
        }

        public IActionResult Index()
        {
            var users = _userContext.UsersandAdminsset.ToList();
            return View(users);
        }

        public IActionResult Details(int ID) 
        {
            if (ID == null) 
            {
                return NotFound();
            }

            var userID = _userContext.UsersandAdminsset.First(user => user.UserID == ID);
            return View(userID);
        }

        public IActionResult Create() 
        { 
            return View(); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(User_Data users) 
        {
            if (ModelState.IsValid) 
            {
                _userContext.UsersandAdminsset.Add(users);
                _userContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(users);
        }

        public IActionResult Edit(int id) 
        {
            var Userobj = _userContext.UsersandAdminsset.Find(id);
            if (Userobj == null) 
            {
                return NotFound();
            }
            return View(Userobj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, User_Data users) 
        {
            var userexists = _userContext.UsersandAdminsset.Find(id);

            if (userexists == null) 
            {
                return NotFound();
            }

            if (ModelState.IsValid) 
            {
                var userobj = _userContext.UsersandAdminsset.Any(user => user.Email == users.Email && user.UserID != id);
                if (userobj) 
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    return View(users);
                }

                var usernameobj = _userContext.UsersandAdminsset.Any(user => user.UserName == users.UserName && user.UserID != id);

                if (usernameobj) 
                {
                    ModelState.AddModelError("Username", "Username already exists.");
                    return View(users);
                }

                userexists.First_Name = users.First_Name;
                userexists.Last_Name = users.Last_Name;
                userexists.Email = users.Email;
                userexists.UserName = users.UserName;
                userexists.Password = users.Password;

                _userContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(users);
        }

        public IActionResult Delete(int id) 
        {
            var user = _userContext.UsersandAdminsset.FirstOrDefault(user => user.UserID == id);
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirm(int id, User_Data users) 
        {
            var UsersID = _userContext.UsersandAdminsset.Find(id);

            if (UsersID == null)
            {
                return NotFound();
            }
            else 
            {
                _userContext.UsersandAdminsset.Remove(UsersID);
                _userContext.SaveChanges(true);
                return RedirectToAction("Index");
            }
        }
    }
}
