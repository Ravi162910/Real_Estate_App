using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Models;

namespace Real_Estate_App.Data
{
    public class AgentDbContext : DbContext
    {
        public AgentDbContext(DbContextOptions<AgentDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Agent> Agents_Set { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
