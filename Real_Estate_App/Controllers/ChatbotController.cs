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

        // Topics the bot understands. Shown when greeting the user, when they
        // ask for "help", and when a message does not match a known intent, so
        // people always know what they can say. ASCII only.
        private const string Suggestions =
            "Here's what you can ask me about:\n" +
            "- Booking or viewing a property\n" +
            "- Logging in or registering an account\n" +
            "- Purchasing a property\n" +
            "- Contacting our support team\n" +
            "Just type it naturally, for example \"I'd like to view a property\".";

        // A recognisable intent: its trigger keywords/phrases, the reply text,
        // and a function that builds the optional action URL (the UI turns a
        // non-empty URL into a "go to page" button). Single-word triggers match
        // a whole word; any trigger containing a space is matched as a phrase.
        private sealed record Intent(string Name, string[] Triggers, string Reply, Func<IUrlHelper, string?> Action);

        private static readonly Intent[] Intents =
        {
            new Intent("Booking",
                new[] { "book", "booking", "bookings", "viewing", "viewings", "view", "see", "visit", "tour",
                        "inspect", "inspection", "open home", "open homes", "appointment", "schedule", "walkthrough" },
                "Bookings/viewings can be made here by clicking this button, then click the Book a Viewing button with what property you would like to see: ",
                url => url.Action("Index", "Properties")),

            new Intent("Login",
                new[] { "login", "log in", "signin", "sign in", "logging in" },
                "Logging in with an account can be done here by clicking this button: ",
                url => url.Action("Login", "UserAdmin")),

            new Intent("Register",
                new[] { "register", "registration", "registering", "signup", "sign up",
                        "create account", "create an account", "new account", "make an account" },
                "Registering a new account can be done here by clicking this button: ",
                url => url.Action("Registration", "UserAdmin")),

            new Intent("Purchase",
                new[] { "purchase", "purchasing", "buy", "buying", "bought", "acquire", "make an offer", "make a purchase" },
                "Purchasing a property can be done here by clicking the button, then pressing view details for what property you are purchasing, then press Purchase this property ",
                url => url.Action("Index", "Properties")),

            new Intent("Contact",
                new[] { "contact", "support", "problem", "issue", "complaint", "complain", "get in touch",
                        "speak to", "talk to", "email", "customer service", "report", "enquire", "enquiry",
                        "inquiry", "feedback", "question" },
                "If you need to reach our team, fill out the contact form here and we'll get back to you by email: ",
                url => url.Action("Index", "Contact")),

            new Intent("Thanks",
                new[] { "thanks", "thank", "thankyou", "thank you", "cheers", "appreciate", "appreciated" },
                "Happy to help! Let me know if there's anything else.",
                _ => ""),

            new Intent("Goodbye",
                new[] { "bye", "goodbye", "good bye", "see you", "see ya", "cya", "later" },
                "Thanks for visiting! Have a great day.",
                _ => ""),

            new Intent("Help",
                new[] { "help", "menu", "options", "option", "commands", "command",
                        "what can you do", "what can you", "how do you work", "who are you" },
                Suggestions,
                _ => ""),

            new Intent("Greeting",
                new[] { "hi", "hello", "hey", "hiya", "yo", "greetings", "howdy", "gday", "kia ora",
                        "good morning", "good afternoon", "good evening", "how are you", "what's up", "whats up" },
                "Hi there! I'm the Real Estate assistant.\n" + Suggestions,
                _ => ""),
        };

        // Scores every intent by how many of its triggers appear in the message
        // and returns the best match. This keyword/scoring approach is far more
        // forgiving of phrasing than the old exact-substring chain - for example
        // "I'd like to view a property" now matches the booking intent on "view"
        // even though it never says "viewing" or "book".
        private (string reply, string action) GetMessage(string message)
        {
            message = (message ?? string.Empty).ToLowerInvariant().Trim();

            // Tokenise so single-word triggers match a whole word only; a plain
            // Contains would fire on substrings (e.g. "hi" inside "this").
            var words = new HashSet<string>(message.Split(
                new[] { ' ', ',', '.', '!', '?', ';', ':', '\'', '"', '(', ')', '/', '\t', '\n', '\r' },
                StringSplitOptions.RemoveEmptyEntries));

            Intent? best = null;
            int bestScore = 0;
            foreach (var intent in Intents)
            {
                int score = 0;
                foreach (var trigger in intent.Triggers)
                {
                    bool matched = trigger.Contains(' ') ? message.Contains(trigger) : words.Contains(trigger);
                    if (matched)
                    {
                        score++;
                    }
                }

                // Strictly greater, so on a tie the earlier (more specific) intent
                // wins - topic intents are listed before greeting/help.
                if (score > bestScore)
                {
                    bestScore = score;
                    best = intent;
                }
            }

            if (best != null)
            {
                return (best.Reply, best.Action(Url) ?? "");
            }

            // Nothing matched - show the menu and offer the contact form.
            return ("Sorry, I didn't quite get that.\n" + Suggestions
                    + "\n\nStill can't find what you need? Reach our team through the contact form: ",
                Url.Action("Index", "Contact") ?? "");
        }// Add saving the conversations in general? + saving conversations accross pages?

        [HttpPost]
        public IActionResult Message([FromBody] string message)
        {
            var (reply, action) = GetMessage(message);
            return Json(new { reply, action });
        }
    }
}
