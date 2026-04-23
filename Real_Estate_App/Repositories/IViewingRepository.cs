using Real_Estate_App.Models;

namespace Real_Estate_App.Repositories
{
    public interface IViewingRepository : IRepository<Viewing>
    {
        Task<IEnumerable<Viewing>> GetAllWithUserAndPropertyAsync();
        Task<Viewing?> GetByIdWithUserAndPropertyAsync(int id);
        Task<IEnumerable<Viewing>> GetByUserIdAsync(int userId);
    }
}
