using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Real_Estate_App.Data;
using Real_Estate_App.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Real_Estate_App.Controllers
{
    public class UserAdminController : Controller
    {
        private readonly UsersPropertiesViewingDbContext _context;
        private readonly IPasswordHasher<User_Data> _passwordHasher;

        public UserAdminController(UsersPropertiesViewingDbContext context, IPasswordHasher<User_Data> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registration(RegisterViewModel registerViewModelobj)
        {
            if (ModelState.IsValid)
            {
                var user_Data = new User_Data
                {
                    First_Name = registerViewModelobj.First_Name,
                    Last_Name = registerViewModelobj.Last_Name,
                    Email = registerViewModelobj.Email,
                    UserName = registerViewModelobj.UserName,
                    IsAdmin = false,
                };
                user_Data.Password = _passwordHasher.HashPassword(user_Data, registerViewModelobj.Password);

                try
                {
                    _context.UsersandAdminsset.Add(user_Data);
                    _context.SaveChanges();
                    ModelState.Clear();
                    //TODO: put viewbag message or follow viewdata popup message lab
                    return View();
                }
                catch (DbUpdateException ex)
                {
                    //TODO: put viewbag message or follow viewdata popup message lab
                    return View(registerViewModelobj);
                }
            }
            return View(registerViewModelobj);
        }



        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginViewModelobj)
        {
            if (!ModelState.IsValid)
            {
                return View(loginViewModelobj);
            }

            // Look up by username or email only — password is verified by the hasher, not the DB
            var useroradmin = await _context.UsersandAdminsset
                .FirstOrDefaultAsync(x => x.UserName == loginViewModelobj.UserNameorEmail
                                       || x.Email == loginViewModelobj.UserNameorEmail);

            if (useroradmin == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username/email or password.");
                return View(loginViewModelobj);
            }

            var verifyResult = _passwordHasher.VerifyHashedPassword(
                useroradmin, useroradmin.Password, loginViewModelobj.Password);
            bool passwordOk = verifyResult == PasswordVerificationResult.Success
                           || verifyResult == PasswordVerificationResult.SuccessRehashNeeded;

            // Legacy upgrade: if the stored password is still plain-text (pre-hash era),
            // accept it and upgrade to a hashed value on this login.
            if (!passwordOk && useroradmin.Password == loginViewModelobj.Password)
            {
                passwordOk = true;
                verifyResult = PasswordVerificationResult.SuccessRehashNeeded;
            }

            if (!passwordOk)
            {
                ModelState.AddModelError(string.Empty, "Invalid username/email or password.");
                return View(loginViewModelobj);
            }

            // Rehash if the stored password was plain-text or used an old algorithm version
            if (verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                useroradmin.Password = _passwordHasher.HashPassword(useroradmin, loginViewModelobj.Password);
                await _context.SaveChangesAsync();
            }

            // Build claims based on IsAdmin flag instead of hardcoded username comparison
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, useroradmin.IsAdmin ? useroradmin.UserName : useroradmin.First_Name),
                new Claim(ClaimTypes.Role, useroradmin.IsAdmin ? "Admin" : "User"),
                new Claim("UserID", useroradmin.UserID!.Value.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return RedirectToAction(useroradmin.IsAdmin ? "LoggedinAdminPage" : "LoggedinUsersPage");
        }

        [Authorize(Roles = "User")]
        public IActionResult LoggedinUsersPage()
        {
            return RedirectToAction("Index", "Properties");

        }

        [Authorize(Roles = "Admin")]
        public IActionResult LoggedinAdminPage()
        {
            return View();

        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
