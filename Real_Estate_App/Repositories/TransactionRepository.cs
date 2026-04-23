using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Data;
using Real_Estate_App.Models;

namespace Real_Estate_App.Repositories
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Transaction>> GetAllWithPropertyAsync()
            => await _dbSet.Include(t => t.Property).ToListAsync();

        public async Task<Transaction?> GetByIdWithPropertyAsync(int id)
            => await _dbSet.Include(t => t.Property)
                           .FirstOrDefaultAsync(t => t.TransactionId == id);

        public async Task<IEnumerable<Transaction>> GetByUserIdAsync(int userId)
            => await _dbSet.Include(t => t.Property)
                           .Where(t => t.UserId == userId)
                           .OrderByDescending(t => t.PurchaseDate)
                           .ToListAsync();
    }
}
