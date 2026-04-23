using Real_Estate_App.Data;
using Real_Estate_App.Repositories;

namespace Real_Estate_App.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private bool _disposed;

        public IPropertyRepository Properties { get; }
        public ITransactionRepository Transactions { get; }
        public IViewingRepository Viewings { get; }
        public IUser_DataRepository Users { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Properties = new PropertyRepository(context);
            Transactions = new TransactionRepository(context);
            Viewings = new ViewingRepository(context);
            Users = new User_DataRepository(context);
        }

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

        public int SaveChanges() => _context.SaveChanges();

        public void Dispose()
        {
            if (_disposed) return;
            _context.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
