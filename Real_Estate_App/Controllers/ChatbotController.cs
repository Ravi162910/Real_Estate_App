using Microsoft.AspNetCore.Mvc;
using Real_Estate_App.Models;
using Real_Estate_App.UnitOfWork;

namespace Real_Estate_App.Controllers
{
    public class ChatbotController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        private async Task<(string reply, string action)> GetMessage(string message) 
        {
            message = message.ToLower();

            if (message.Contains("booking") || message.Contains("viewing") || message.Contains("book"))
            {
                return ("Bookings/viewings can be made here by clicking this button, then click the Book a Viewing button with what property you would like to see: ", Url.Action("Index", "Properties"));
            }
            else if (message.Contains("login") || message.Contains("log in") || message.Contains("sign in"))
            {
                return ("Logging in with an account can be done here by clicking this button: ", Url.Action("Login", "UserAdmin"));
            }
            else if (message.Contains("register") || message.Contains("registration") || message.Contains("sign up"))
            {
                return ("Registering a new account can be done here by clicking this button: ", Url.Action("Registration", "UserAdmin"));
            }
            else if (message.Contains("purchase") || message.Contains("purchasing") || message.Contains("buy"))
            {
                return ("Purchasing a property can be done here by clicking the button, then pressing view details for what property you are purchasing, then press Purchase this property ", Url.Action("Index", "Properties"));
            }
            else if (message.Contains("contact") || message.Contains("support") || message.Contains("help")
                  || message.Contains("problem") || message.Contains("issue") || message.Contains("complaint"))
            {
                return ("If you need to reach our team, fill out the contact form here and we'll get back to you by email: ", Url.Action("Index", "Contact"));
            }
            return ("Sorry I cant help you with that, you can reach our team through the contact form: ", Url.Action("Index", "Contact"));
        }// Add saving the conversations in general? + saving conversations accross pages?

        [HttpPost]
        public async Task<IActionResult> Message([FromBody] string message) 
        {
            var (reply, action) = await GetMessage(message);
            return Json(new { reply, action });
        }


    }
}
