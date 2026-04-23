using Real_Estate_App.Models;

namespace Real_Estate_App.Repositories
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetAllWithPropertyAsync();
        Task<Transaction?> GetByIdWithPropertyAsync(int id);
        Task<IEnumerable<Transaction>> GetByUserIdAsync(int userId);
    }
}
