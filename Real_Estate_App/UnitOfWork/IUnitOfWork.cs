using Real_Estate_App.Repositories;

namespace Real_Estate_App.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IPropertyRepository Properties { get; }
        ITransactionRepository Transactions { get; }
        IViewingRepository Viewings { get; }
        IUser_DataRepository Users { get; }

        Task<int> SaveChangesAsync();
        int SaveChanges();
    }
}
