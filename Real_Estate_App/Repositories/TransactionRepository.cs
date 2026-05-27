using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Data;
using Real_Estate_App.Models;

namespace Real_Estate_App.Repositories
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Transaction>> GetAllWithPropertyAsync()
            => await _dbSet.Include(t => t.Property)
                           .OrderByDescending(t => t.PurchaseDate)
                           .ToListAsync();

        public async Task<Transaction?> GetByIdWithPropertyAsync(int id)
            => await _dbSet.Include(t => t.Property)
                           .FirstOrDefaultAsync(t => t.TransactionId == id);

        public async Task<IEnumerable<Transaction>> GetByUserIdAsync(int userId)
            => await _dbSet.Include(t => t.Property)
                           .Where(t => t.UserId == userId)
                           .OrderByDescending(t => t.PurchaseDate)
                           .ToListAsync();

        public async Task<IEnumerable<Transaction>> GetByStatusAsync(string status)
            => await _dbSet.Include(t => t.Property)
                           .Where(t => t.Status == status)
                           .OrderByDescending(t => t.PurchaseDate)
                           .ToListAsync();

        public async Task<int> CountByStatusAsync(string status)
            => await _dbSet.CountAsync(t => t.Status == status);

        public async Task<bool> HasApprovedForPropertyAsync(int propertyId, int? excludeTransactionId = null)
            => await _dbSet.AnyAsync(t =>
                t.PropertyId == propertyId &&
                t.Status == Transaction.StatusApproved &&
                (excludeTransactionId == null || t.TransactionId != excludeTransactionId.Value));
    }
}
