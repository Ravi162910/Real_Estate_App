using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Models;

namespace Real_Estate_App.Data
{
    public class UserAdminDbContext : DbContext
    {
        public UserAdminDbContext(DbContextOptions<UserAdminDbContext> options) : base(options)
        {
        }

        public DbSet<User_Data> UsersandAdminsset { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        public DbSet<Real_Estate_App.Models.RegisterViewModel> RegisterViewModel { get; set; } = default!;
    }
}
