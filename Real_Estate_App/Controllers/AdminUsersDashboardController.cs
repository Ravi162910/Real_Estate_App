using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Data;
using Real_Estate_App.Models;

namespace Real_Estate_App.Controllers
{
    public class AdminUsersDashboardController : Controller
    {
        private readonly AppDbContext _userContext;
        private readonly IPasswordHasher<User_Data> _passwordHasher;
        public AdminUsersDashboardController(AppDbContext usersContext, IPasswordHasher<User_Data> passwordHasher)
        {
            _userContext = usersContext;
            _passwordHasher = passwordHasher;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var users = _userContext.UsersandAdminsset.ToList();
            return View(users);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Details(int ID) 
        {
            if (ID == 0) 
            {
                return NotFound();
            }

            var userID = _userContext.UsersandAdminsset.First(user => user.UserID == ID);
            return View(userID);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() 
        { 
            return View(); 
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int ID, User_Data users) 
        {

            users.Password = _passwordHasher.HashPassword(users, users.Password);

            var useemailrobj = _userContext.UsersandAdminsset.Any(user => user.Email == users.Email && user.UserID != ID);
            if (useemailrobj)
            {
                TempData["error"] = "Email already exists.";
                return View(users);
            }

            var usernameobj = _userContext.UsersandAdminsset.Any(user => user.UserName == users.UserName && user.UserID != ID);

            if (usernameobj)
            {
                TempData["error"] = "Username already exists.";
                return View(users);
            }

            if (ModelState.IsValid) 
            {
                _userContext.UsersandAdminsset.Add(users);
                _userContext.SaveChanges();
                TempData["success"] = "User successfully added as an admin";
                return RedirectToAction("Index");
            }
            return View(users);
        }

        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, User_Data users) 
        {
            var userexists = _userContext.UsersandAdminsset.Find(id);

            if (userexists == null) 
            {
                return NotFound();
            }


            var userobj = _userContext.UsersandAdminsset.Any(user => user.Email == users.Email && user.UserID != id);
            if (userobj)
            {
                TempData["error"] = "Email already exists.";
                return View(users);
            }

            var usernameobj = _userContext.UsersandAdminsset.Any(user => user.UserName == users.UserName && user.UserID != id);

            if (usernameobj)
            {
                TempData["error"] = "Username already exists.";
                return View(users);
            }

            userexists.First_Name = users.First_Name;
            userexists.Last_Name = users.Last_Name;
            userexists.Email = users.Email;
            userexists.UserName = users.UserName;

            _userContext.UsersandAdminsset.Update(userexists);
            _userContext.SaveChanges();
            TempData["success"] = "User successfully edited as an admin";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
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
                TempData["success"] = "User successfully deleted as an admin";
                return RedirectToAction("Index");
            }
        }
    }
}
