using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Data;
using Real_Estate_App.Models;
using Real_Estate_App.UnitOfWork;

namespace Real_Estate_App.Controllers
{
    public class AdminUsersDashboardController : Controller
    {
        private readonly AppDbContext _userContext;
        private readonly IPasswordHasher<User_Data> _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;
        public AdminUsersDashboardController(AppDbContext usersContext, IPasswordHasher<User_Data> passwordHasher, IUnitOfWork unitOfWork)
        {
            _userContext = usersContext;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return View(users);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int ID) 
        {
            if (ID == 0) 
            {
                return NotFound();
            }

            var userID = await _unitOfWork.Users.GetByIdAsync(ID);
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
        public async Task<IActionResult> Create(int ID, User_Data users) 
        {

            users.Password = _passwordHasher.HashPassword(users, users.Password);

            var useemailrobj = await _unitOfWork.Users.EmailExistsAsync(users.Email);
            if (useemailrobj)
            {
                TempData["error"] = "Email already exists.";
                return View(users);
            }

            var usernameobj = await _unitOfWork.Users.UsernameExistsAsync(users.UserName);

            if (usernameobj)
            {
                TempData["error"] = "Username already exists.";
                return View(users);
            }

            if (ModelState.IsValid) 
            {
                await _unitOfWork.Users.AddAsync(users);
                await _unitOfWork.SaveChangesAsync();
                TempData["success"] = "User successfully added as an admin";
                return RedirectToAction("Index");
            }
            return View(users);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id) 
        {
            var Userobj = await _unitOfWork.Users.GetByIdAsync(id);
            if (Userobj == null) 
            {
                return NotFound();
            }
            return View(Userobj);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User_Data users) 
        {
            var userexists = await _unitOfWork.Users.GetByIdAsync(id);

            if (userexists == null) 
            {
                return NotFound();
            }


            var userobj = await _unitOfWork.Users.UsernameExistsAsync(users.UserName, id);
            if (userobj)
            {
                TempData["error"] = "Email already exists.";
                return View(users);
            }

            var usernameobj = await _unitOfWork.Users.EmailExistsAsync(users.Email, id);

            if (usernameobj)
            {
                TempData["error"] = "Username already exists.";
                return View(users);
            }

            userexists.First_Name = users.First_Name;
            userexists.Last_Name = users.Last_Name;
            userexists.Email = users.Email;
            userexists.UserName = users.UserName;

            _unitOfWork.Users.Update(userexists);
            _unitOfWork.SaveChanges();
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
        public async Task<IActionResult> DeleteConfirm(int id, User_Data users) 
        {
            var UsersID = await _unitOfWork.Users.GetByIdAsync(id);

            if (UsersID == null)
            {
                return NotFound();
            }
            else 
            {
                _unitOfWork.Users.Remove(UsersID);
                _unitOfWork.SaveChanges();
                TempData["success"] = "User successfully deleted as an admin";
                return RedirectToAction("Index");
            }
        }
    }
}
