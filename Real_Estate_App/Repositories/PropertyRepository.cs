using Microsoft.EntityFrameworkCore;
using Real_Estate_App.Data;
using Real_Estate_App.Models;

namespace Real_Estate_App.Repositories
{
    public class PropertyRepository : Repository<Property>, IPropertyRepository
    {
        public PropertyRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Property>> GetAvailableFilteredAsync(
            string? searchString,
            string? propertyType,
            int? minBedrooms,
            int? maxBedrooms,
            int? minBathrooms,
            int? maxBathrooms,
            decimal? minPrice,
            decimal? maxPrice,
            int? minGarages,
            int? minPets)
        {
            var query = _dbSet.Where(p => p.IsAvailable);

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p =>
                    p.PropertyName.Contains(searchString) ||
                    p.PropertyAddress.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(propertyType))
                query = query.Where(p => p.PropertyType == propertyType);

            if (minBedrooms.HasValue)
                query = query.Where(p => p.PropertyBedrooms >= minBedrooms.Value);
            if (maxBedrooms.HasValue)
                query = query.Where(p => p.PropertyBedrooms <= maxBedrooms.Value);

            if (minBathrooms.HasValue)
                query = query.Where(p => p.PropertyBathrooms >= minBathrooms.Value);
            if (maxBathrooms.HasValue)
                query = query.Where(p => p.PropertyBathrooms <= maxBathrooms.Value);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            if (minGarages.HasValue)
                query = query.Where(p => p.PropertyGarages >= minGarages.Value);
            if (minPets.HasValue)
                query = query.Where(p => p.PropertyPets >= minPets.Value);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<string>> GetDistinctPropertyTypesAsync()
            => await _dbSet
                .Select(p => p.PropertyType)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
    }
}
