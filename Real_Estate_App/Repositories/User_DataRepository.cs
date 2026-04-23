using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Data;
using Real_Estate_App.Models;

namespace Real_Estate_App.Repositories
{
    public class User_DataRepository : Repository<User_Data>, IUser_DataRepository
    {
        public User_DataRepository(AppDbContext context) : base(context) { }

        public async Task<User_Data?> GetByUsernameOrEmailAsync(string usernameOrEmail)
            => await _dbSet.FirstOrDefaultAsync(u =>
                u.UserName == usernameOrEmail || u.Email == usernameOrEmail);

        public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
            => await _dbSet.AnyAsync(u =>
                u.Email == email && (excludeUserId == null || u.UserID != excludeUserId));

        public async Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null)
            => await _dbSet.AnyAsync(u =>
                u.UserName == username && (excludeUserId == null || u.UserID != excludeUserId));

        public async Task<bool> HasAdminAsync()
            => await _dbSet.AnyAsync(u => u.IsAdmin);
    }
}
