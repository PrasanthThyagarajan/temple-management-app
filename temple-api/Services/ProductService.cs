using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services.Interfaces;

namespace TempleApi.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            var product = new Product
            {
                Name = createProductDto.Name,
                Category = createProductDto.Category,
                CategoryId = createProductDto.CategoryId,
                Quantity = createProductDto.StockQuantity,
                MinStockLevel = createProductDto.MinStockLevel,
                Price = createProductDto.Price,
                IsActive = createProductDto.Status == "Active",
                Description = createProductDto.Description,
                Notes = createProductDto.Notes
            };

            await _productRepository.AddAsync(product);

            return MapToDto(product);
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            
            return product != null ? MapToDto(product) : null;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            
            return products.Where(p => p.IsActive).Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category)
        {
            var products = await _productRepository.GetByCategoryAsync(category);
            
            return products.Select(MapToDto);
        }

        public async Task<ProductDto> UpdateProductAsync(int id, CreateProductDto updateProductDto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            product.Name = updateProductDto.Name;
            product.Category = updateProductDto.Category;
            product.CategoryId = updateProductDto.CategoryId;
            product.Quantity = updateProductDto.StockQuantity;
            product.MinStockLevel = updateProductDto.MinStockLevel;
            product.Price = updateProductDto.Price;
            product.IsActive = updateProductDto.Status == "Active";
            product.Description = updateProductDto.Description;
            product.Notes = updateProductDto.Notes;

            await _productRepository.UpdateAsync(product);
            return MapToDto(product);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            return await _productRepository.SoftDeleteAsync(id);
        }

        public async Task<bool> UpdateProductQuantityAsync(int id, int quantity)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return false;
            }

            product.Quantity = quantity;
            await _productRepository.UpdateAsync(product);
            return true;
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
        {
            var products = await _productRepository.GetAllAsync();
            var filteredProducts = products.Where(p => 
                p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Category.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            
            return filteredProducts.Select(MapToDto);
        }

        public async Task<bool> UpdateProductStatusAsync(int id, string status)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return false;
            }

            // Map status string to IsActive boolean
            product.IsActive = status == "Active";
            await _productRepository.UpdateAsync(product);
            return true;
        }

        public async Task<bool> CheckProductAvailabilityAsync(int id, int requestedQuantity)
        {
            return await _productRepository.CheckAvailabilityAsync(id, requestedQuantity);
        }

        private static ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Category = product.Category,
                CategoryId = product.CategoryId,
                CategoryNavigation = product.CategoryNavigation != null ? new CategoryDto
                {
                    Id = product.CategoryNavigation.Id,
                    Name = product.CategoryNavigation.Name,
                    Description = product.CategoryNavigation.Description,
                    IsActive = product.CategoryNavigation.IsActive,
                    SortOrder = product.CategoryNavigation.SortOrder,
                    CreatedAt = product.CategoryNavigation.CreatedAt,
                    UpdatedAt = product.CategoryNavigation.UpdatedAt
                } : null,
                Quantity = product.Quantity,
                StockQuantity = product.Quantity,
                MinStockLevel = product.MinStockLevel,
                Price = product.Price,
                Status = product.IsActive ? "Active" : "Inactive",
                Description = product.Description,
                Notes = product.Notes,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}
