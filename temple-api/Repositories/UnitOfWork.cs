using Microsoft.EntityFrameworkCore.Storage;
using TempleApi.Data;
using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;

namespace TempleApi.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TempleDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(TempleDbContext context)
        {
            _context = context;
            
            // Initialize repositories
            Temples = new Repository<Temple>(_context);
            Devotees = new Repository<Devotee>(_context);
            Donations = new Repository<Donation>(_context);
            Events = new Repository<Event>(_context);
            EventRegistrations = new Repository<EventRegistration>(_context);
            Services = new Repository<Service>(_context);
            
            // Shop Management repositories - using specialized repositories
            Users = new UserRepository(_context);
            // Products = new ProductRepository(_context); // Commented out - ProductRepository now requires IDbContextFactory
            Sales = new SaleRepository(_context);
            SaleItems = new Repository<SaleItem>(_context);
            Poojas = new Repository<Pooja>(_context);
            PoojaBookings = new PoojaBookingRepository(_context);
        }

        public IRepository<Temple> Temples { get; }
        public IRepository<Devotee> Devotees { get; }
        public IRepository<Donation> Donations { get; }
        public IRepository<Event> Events { get; }
        public IRepository<EventRegistration> EventRegistrations { get; }
        public IRepository<Service> Services { get; }
        
        // Shop Management repositories
        public IUserRepository Users { get; }
        // public IProductRepository Products { get; } // Commented out - ProductRepository now requires IDbContextFactory
        public ISaleRepository Sales { get; }
        public IRepository<SaleItem> SaleItems { get; }
        public IRepository<Pooja> Poojas { get; }
        public IPoojaBookingRepository PoojaBookings { get; }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
