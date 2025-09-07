using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;

namespace TempleApi.Repositories
{
    public class ProductRepository : FactoryRepository<Product>, IProductRepository
    {
        public ProductRepository(IDbContextFactory contextFactory) : base(contextFactory)
        {
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            return await context.Products
                .Include(p => p.CategoryNavigation)
                .Where(p => p.Category == category && p.IsActive)
                .ToListAsync();
        }

        public async Task<bool> CheckAvailabilityAsync(int productId, int requestedQuantity)
        {
            var product = await GetByIdAsync(productId);
            return product != null && product.IsActive && product.Quantity >= requestedQuantity;
        }

        public override async Task<Product?> GetByIdAsync(int id)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            return await context.Products
                .Include(p => p.CategoryNavigation)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public override async Task<IEnumerable<Product>> GetAllAsync()
        {
            using var context = _contextFactory.CreateTempleDbContext();
            return await context.Products
                .Include(p => p.CategoryNavigation)
                .ToListAsync();
        }
    }
}
