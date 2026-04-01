using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Models;

namespace Real_Estate_App.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Property> Properties { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Property>(entity =>
            {
                entity.HasKey(p => p.PropertyId);
                entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
            });
        }
    }
}
