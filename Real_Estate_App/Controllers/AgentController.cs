using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Data;

namespace Real_Estate_App.Controllers
{
    public class AgentController : Controller
    {
        private readonly AgentDbContext _context;
        public AgentController(AgentDbContext agentDbContext) 
        {
            _context = agentDbContext;
        }

        public IActionResult AgentOptions() 
        {
            var Agentlist = _context.Agents_Set.ToList();
            return View(Agentlist);
        }
    }
}
