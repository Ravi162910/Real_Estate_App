using Real_Estate_App.Models;

namespace Real_Estate_App.Repositories
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetAllWithPropertyAsync();
        Task<Transaction?> GetByIdWithPropertyAsync(int id);
        Task<IEnumerable<Transaction>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Transaction>> GetByStatusAsync(string status);
        Task<int> CountByStatusAsync(string status);

        // True if the property already has an approved (completed) sale. Used to
        // stop the same property being sold twice - e.g. when an admin unhides a
        // property that was already sold. Pass excludeTransactionId to ignore the
        // row being evaluated.
        Task<bool> HasApprovedForPropertyAsync(int propertyId, int? excludeTransactionId = null);
    }
}
