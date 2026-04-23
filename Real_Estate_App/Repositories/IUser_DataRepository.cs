using Real_Estate_App.Models;

namespace Real_Estate_App.Repositories
{
    public interface IUser_DataRepository : IRepository<User_Data>
    {
        Task<User_Data?> GetByUsernameOrEmailAsync(string usernameOrEmail);
        Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);
        Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null);
        Task<bool> HasAdminAsync();
    }
}
