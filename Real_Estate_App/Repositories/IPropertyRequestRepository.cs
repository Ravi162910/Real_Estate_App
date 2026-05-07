using Real_Estate_App.Models;

namespace Real_Estate_App.Repositories
{
    public interface IPropertyRequestRepository : IRepository<PropertyRequest>
    {
        Task UpdateAsync(PropertyRequest request);
    }
}
