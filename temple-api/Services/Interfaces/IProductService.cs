using TempleApi.Models.DTOs;

namespace TempleApi.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category);
        Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm);
        Task<ProductDto> UpdateProductAsync(int id, CreateProductDto updateProductDto);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> UpdateProductQuantityAsync(int id, int quantity);
        Task<bool> UpdateProductStatusAsync(int id, string status);
        Task<bool> CheckProductAvailabilityAsync(int id, int requestedQuantity);
    }
}
