using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Data;

namespace Real_Estate_App.Models
{
    public class Agent_SeedData
    {
        public static void Initialize(IServiceProvider serviceP) 
        {
            using (var agentcontext = new AgentDbContext(serviceP.GetRequiredService<DbContextOptions<AgentDbContext>>())) 
            {
                if (agentcontext.Agents_Set.Any()) 
                {
                    return;
                }
                agentcontext.Agents_Set.AddRange(
                    new Agent 
                    {
                        Agent_FullName = "Greg Turner",
                        Agent_AgencyName = "Clark & Co Realty"
                    },
                    new Agent 
                    {
                        Agent_FullName = "Kelly Graham",
                        Agent_AgencyName = "Harcourts"
                    },
                    new Agent 
                    {
                        Agent_FullName = "Peter Remmington",
                        Agent_AgencyName = "Ray White"
                    });
                agentcontext.SaveChanges();
            }
        }
    }
}
