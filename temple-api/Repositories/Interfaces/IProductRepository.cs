using TempleApi.Domain.Entities;

namespace TempleApi.Repositories.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetByCategoryAsync(string category);
        Task<bool> CheckAvailabilityAsync(int productId, int requestedQuantity);
    }
}
