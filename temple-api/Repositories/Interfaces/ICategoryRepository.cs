using TempleApi.Domain.Entities;

namespace TempleApi.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<Category?> GetByNameAsync(string name);
    }
}
