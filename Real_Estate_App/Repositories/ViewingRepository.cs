using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Data;
using Real_Estate_App.Models;

namespace Real_Estate_App.Repositories
{
    public class ViewingRepository : Repository<Viewing>, IViewingRepository
    {
        public ViewingRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Viewing>> GetAllWithUserAndPropertyAsync()
            => await _dbSet.Include(v => v.Users)
                           .Include(v => v.Properties)
                           .ToListAsync();

        public async Task<Viewing?> GetByIdWithUserAndPropertyAsync(int id)
            => await _dbSet.Include(v => v.Users)
                           .Include(v => v.Properties)
                           .FirstOrDefaultAsync(v => v.Viewing_ID == id);

        public async Task<IEnumerable<Viewing>> GetByUserIdAsync(int userId)
            => await _dbSet.Include(v => v.Properties)
                           .Where(v => v.UserID == userId)
                           .OrderBy(v => v.Viewing_TimeDate)
                           .ToListAsync();
    }
}
