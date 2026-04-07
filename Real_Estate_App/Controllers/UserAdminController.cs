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
using System.Security.Claims;

namespace Real_Estate_App.Controllers
{
    public class UserAdminController : Controller
    {
        private readonly UsersPropertiesViewingDbContext _context;

        public UserAdminController(UsersPropertiesViewingDbContext context)
        {
            _context = context;
        }

        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registration(RegisterViewModel registerViewModelobj)
        {
            if (ModelState.IsValid)
            {
                User_Data user_Data = new User_Data();
                user_Data.First_Name = registerViewModelobj.First_Name;
                user_Data.Last_Name = registerViewModelobj.Last_Name;
                user_Data.Email = registerViewModelobj.Email;
                user_Data.UserName = registerViewModelobj.UserName;
                user_Data.Password = registerViewModelobj.Password;

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
        public IActionResult Login(LoginViewModel loginViewModelobj)
        {
            if (ModelState.IsValid)
            {
                var useroradmin = _context.UsersandAdminsset.Where(x => (x.UserName == loginViewModelobj.UserNameorEmail || x.Email == loginViewModelobj.UserNameorEmail && x.Password == loginViewModelobj.Password)).FirstOrDefault();
                if (useroradmin != null && useroradmin.UserName != "Adminusername" && useroradmin.Password != "AdminPassword")
                {
                    var cookieclaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, useroradmin.First_Name),
                        new Claim(ClaimTypes.Role, "User"),
                        new Claim("UserID", useroradmin.UserID.ToString())
                    };
                    var claimauthentication = new ClaimsIdentity(cookieclaims, CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimauthentication));
                    return RedirectToAction("LoggedinUsersPage");
                }
                else if (useroradmin != null && useroradmin.UserName == "AdminUsername" && useroradmin.Password == "AdminPassword")
                {
                    var cookieclaimadmin = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, useroradmin.UserName),
                        new Claim(ClaimTypes.Role, "Admin")
                    };
                    var claimauthentication = new ClaimsIdentity(cookieclaimadmin, CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimauthentication));
                    return RedirectToAction("LoggedinAdminPage");
                }
            }
            return View(loginViewModelobj);
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
    }
}
