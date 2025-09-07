using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Repositories.Interfaces;

namespace TempleApi.Repositories
{
    public class CategoryRepository : FactoryRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(IDbContextFactory contextFactory) : base(contextFactory) { }

        public override async Task<Category?> GetByIdAsync(int id)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            return await context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public override async Task<IEnumerable<Category>> GetAllAsync()
        {
            using var context = _contextFactory.CreateTempleDbContext();
            return await context.Categories
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            using var context = _contextFactory.CreateTempleDbContext();
            return await context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            using var context = _contextFactory.CreateTempleDbContext();
            return await context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }
    }
}
