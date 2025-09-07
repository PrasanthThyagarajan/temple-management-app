using TempleApi.Domain.Entities;

namespace TempleApi.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Temple> Temples { get; }
        IRepository<Devotee> Devotees { get; }
        IRepository<Donation> Donations { get; }
        IRepository<Event> Events { get; }
        IRepository<EventRegistration> EventRegistrations { get; }
        IRepository<Service> Services { get; }
        
        // Shop Management repositories
        IUserRepository Users { get; }
        // IProductRepository Products { get; } // Commented out - ProductRepository now requires IDbContextFactory
        ISaleRepository Sales { get; }
        IRepository<SaleItem> SaleItems { get; }
        IRepository<Pooja> Poojas { get; }
        IPoojaBookingRepository PoojaBookings { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
