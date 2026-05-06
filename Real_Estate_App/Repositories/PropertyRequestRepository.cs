using Real_Estate_App.Data;
using Real_Estate_App.Models;

namespace Real_Estate_App.Repositories
{
    public class PropertyRequestRepository : Repository<PropertyRequest>, IPropertyRequestRepository
    {
        private readonly AppDbContext _db;

        public PropertyRequestRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task UpdateAsync(PropertyRequest request) 
        {
            _db.PropertyRequestsSet.Update(request);
        }
    }
}
