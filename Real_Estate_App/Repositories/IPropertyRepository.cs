using Real_Estate_App.Models;

namespace Real_Estate_App.Repositories
{
    public interface IPropertyRepository : IRepository<Property>
    {
        Task<IEnumerable<Property>> GetAvailableFilteredAsync(
            string? searchString,
            string? propertyType,
            int? minBedrooms,
            int? maxBedrooms,
            int? minBathrooms,
            int? maxBathrooms,
            decimal? minPrice,
            decimal? maxPrice,
            int? minGarages,
            int? minPets,
            bool availableOnly = true);

        Task<IEnumerable<string>> GetDistinctPropertyTypesAsync();
    }
}
