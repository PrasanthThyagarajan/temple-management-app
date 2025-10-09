using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services;
using TempleApi.Enums;
using Xunit;
using FluentAssertions;

namespace TempleApi.Tests
{
    public class ProductServiceTests : IDisposable
    {
        private readonly TempleDbContext _context;
        private readonly IProductRepository _productRepository;
        private readonly ProductService _productService;
        private readonly string _databaseName;

        public ProductServiceTests()
        {
            _databaseName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<TempleDbContext>()
                .UseInMemoryDatabase(databaseName: _databaseName)
                .Options;

            _context = new TempleDbContext(options);
            var testDbContextFactory = new TestDbContextFactory(_databaseName);
            _productRepository = new TempleApi.Repositories.ProductRepository(testDbContextFactory);
            _productService = new ProductService(_productRepository);
        }

        #region Create Tests

        [Fact]
        public async Task CreateProductAsync_ShouldCreateProduct_WhenValidData()
        {
            // Arrange
            var createProductDto = new CreateProductDto
            {
                Name = "Test Product",
                Category = "Pooja Items",
                StockQuantity = 100,
                MinStockLevel = 10,
                Price = 25.50m,
                IsPreBookingAvailable = true,
                Status = "Active",
                Description = "Test Description",
                Notes = "Test Notes"
            };

            // Act
            var result = await _productService.CreateProductAsync(createProductDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(createProductDto.Name);
            result.Category.Should().Be(createProductDto.Category);
            result.StockQuantity.Should().Be(createProductDto.StockQuantity);
            result.Price.Should().Be(createProductDto.Price);
            result.IsPreBookingAvailable.Should().BeTrue();
            result.Status.Should().Be("Active");
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateProductAsync_ShouldCreateInactiveProduct_WhenStatusIsInactive()
        {
            // Arrange
            var createProductDto = new CreateProductDto
            {
                Name = "Inactive Product",
                Category = "Books",
                StockQuantity = 50,
                MinStockLevel = 5,
                Price = 15.00m,
                Status = "Inactive",
                Description = "Test Description"
            };

            // Act
            var result = await _productService.CreateProductAsync(createProductDto);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be("Inactive");
            result.IsPreBookingAvailable.Should().BeFalse();
        }

        #endregion

        #region Read Tests

        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnProduct_WhenProductExists()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Category = "Pooja Items",
                Quantity = 50,
                MinStockLevel = 5,
                Price = 15.75m,
                Description = "Test Description",
                Notes = "Test Notes",
                IsActive = true,
                IsPreBookingAvailable = true
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            var result = await _productService.GetProductByIdAsync(product.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be(product.Name);
            result!.Category.Should().Be(product.Category);
            result!.StockQuantity.Should().Be(product.Quantity);
            result!.Price.Should().Be(product.Price);
            result!.IsPreBookingAvailable.Should().BeTrue();
            result!.Status.Should().Be("Active");
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Act
            var result = await _productService.GetProductByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnAllActiveProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Name = "Product 1", Category = "Category 1", Quantity = 10, Price = 10.00m, IsActive = true },
                new Product { Name = "Product 2", Category = "Category 2", Quantity = 20, Price = 20.00m, IsActive = true },
                new Product { Name = "Product 3", Category = "Category 3", Quantity = 30, Price = 30.00m, IsActive = false }
            };
            _context.Products.AddRange(products);
            await _context.SaveChangesAsync();

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(p => p.Status == "Active");
        }

        [Fact]
        public async Task GetProductsByCategoryAsync_ShouldReturnProductsInCategory()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Name = "Product 1", Category = "Pooja Items", Quantity = 10, Price = 10.00m, IsActive = true },
                new Product { Name = "Product 2", Category = "Pooja Items", Quantity = 20, Price = 20.00m, IsActive = true },
                new Product { Name = "Product 3", Category = "Books", Quantity = 30, Price = 30.00m, IsActive = true }
            };
            _context.Products.AddRange(products);
            await _context.SaveChangesAsync();

            // Act
            var result = await _productService.GetProductsByCategoryAsync("Pooja Items");

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(p => p.Category == "Pooja Items");
        }

        [Fact]
        public async Task SearchProductsAsync_ShouldReturnMatchingProducts_WhenSearchTermMatches()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Name = "Incense Stick", Category = "Pooja Items", Quantity = 10, Price = 10.00m, IsActive = true },
                new Product { Name = "Holy Book", Category = "Books", Quantity = 20, Price = 20.00m, IsActive = true },
                new Product { Name = "Incense Holder", Category = "Pooja Items", Quantity = 30, Price = 30.00m, IsActive = true }
            };
            _context.Products.AddRange(products);
            await _context.SaveChangesAsync();

            // Act
            var result = await _productService.SearchProductsAsync("Incense");

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(p => p.Name.Contains("Incense"));
        }

        [Fact]
        public async Task SearchProductsAsync_ShouldReturnEmptyList_WhenNoMatches()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Name = "Incense Stick", Category = "Pooja Items", Quantity = 10, Price = 10.00m, IsActive = true }
            };
            _context.Products.AddRange(products);
            await _context.SaveChangesAsync();

            // Act
            var result = await _productService.SearchProductsAsync("NonExistent");

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task UpdateProductAsync_ShouldUpdateProduct_WhenProductExists()
        {
            // Arrange
            var product = new Product
            {
                Name = "Original Product",
                Category = "Original Category",
                Quantity = 100,
                MinStockLevel = 10,
                Price = 25.00m,
                IsActive = true
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var updateDto = new CreateProductDto
            {
                Name = "Updated Product",
                Category = "Updated Category",
                StockQuantity = 150,
                MinStockLevel = 15,
                Price = 30.00m,
                IsPreBookingAvailable = true,
                Status = "Active",
                Description = "Updated Description",
                Notes = "Updated Notes"
            };

            // Act
            var result = await _productService.UpdateProductAsync(product.Id, updateDto);

            // Assert
            result.Name.Should().Be(updateDto.Name);
            result.Category.Should().Be(updateDto.Category);
            result.StockQuantity.Should().Be(updateDto.StockQuantity);
            result.Price.Should().Be(updateDto.Price);
            result.Description.Should().Be(updateDto.Description);
            result.Notes.Should().Be(updateDto.Notes);
            result.IsPreBookingAvailable.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldThrowException_WhenProductDoesNotExist()
        {
            // Arrange
            var updateDto = new CreateProductDto
            {
                Name = "Updated Product",
                Category = "Updated Category",
                StockQuantity = 150,
                MinStockLevel = 15,
                Price = 30.00m,
                Status = "Active"
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _productService.UpdateProductAsync(999, updateDto));
        }

        [Fact]
        public async Task UpdateProductQuantityAsync_ShouldUpdateQuantity_WhenProductExists()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Category = "Test Category",
                Quantity = 50,
                Price = 15.00m,
                IsActive = true
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            var result = await _productService.UpdateProductQuantityAsync(product.Id, 75);

            // Assert
            result.Should().BeTrue();
            var updatedProduct = await _productService.GetProductByIdAsync(product.Id);
            updatedProduct.Should().NotBeNull();
            updatedProduct?.Quantity.Should().Be(75);
        }

        [Fact]
        public async Task UpdateProductQuantityAsync_ShouldReturnFalse_WhenProductDoesNotExist()
        {
            // Act
            var result = await _productService.UpdateProductQuantityAsync(999, 75);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateProductStatusAsync_ShouldUpdateStatus_WhenProductExists()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Category = "Test Category",
                Quantity = 50,
                Price = 15.00m,
                IsActive = true
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            var result = await _productService.UpdateProductStatusAsync(product.Id, "Inactive");

            // Assert
            result.Should().BeTrue();
            var updatedProduct = await _productService.GetProductByIdAsync(product.Id);
            updatedProduct.Should().NotBeNull();
            updatedProduct?.Status.Should().Be("Inactive");
        }

        [Fact]
        public async Task UpdateProductStatusAsync_ShouldReturnFalse_WhenProductDoesNotExist()
        {
            // Act
            var result = await _productService.UpdateProductStatusAsync(999, "Active");

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task DeleteProductAsync_ShouldDeactivateProduct_WhenProductExists()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Category = "Test Category",
                Quantity = 50,
                Price = 15.00m,
                IsActive = true
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            var result = await _productService.DeleteProductAsync(product.Id);

            // Assert
            result.Should().BeTrue();
            var deletedProduct = await _productService.GetProductByIdAsync(product.Id);
            deletedProduct!.Status.Should().Be("Inactive");
        }

        [Fact]
        public async Task DeleteProductAsync_ShouldReturnFalse_WhenProductDoesNotExist()
        {
            // Act
            var result = await _productService.DeleteProductAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Availability Tests

        [Fact]
        public async Task CheckProductAvailabilityAsync_ShouldReturnTrue_WhenProductHasEnoughQuantity()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Category = "Test Category",
                Quantity = 100,
                Price = 15.00m,
                IsActive = true
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            var result = await _productService.CheckProductAvailabilityAsync(product.Id, 50);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CheckProductAvailabilityAsync_ShouldReturnFalse_WhenProductHasInsufficientQuantity()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Category = "Test Category",
                Quantity = 10,
                Price = 15.00m,
                IsActive = true
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            var result = await _productService.CheckProductAvailabilityAsync(product.Id, 50);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CheckProductAvailabilityAsync_ShouldReturnFalse_WhenProductDoesNotExist()
        {
            // Act
            var result = await _productService.CheckProductAvailabilityAsync(999, 10);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CheckProductAvailabilityAsync_ShouldReturnFalse_WhenProductIsInactive()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Category = "Test Category",
                Quantity = 100,
                Price = 15.00m,
                IsActive = false
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            var result = await _productService.CheckProductAvailabilityAsync(product.Id, 50);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        public void Dispose()
        {
            _context.Dispose();
        }
    }

    // Test implementation of IDbContextFactory
    public class TestDbContextFactory : IDbContextFactory
    {
        private readonly DbContextOptions<TempleDbContext> _options;

        public TestDbContextFactory(string databaseName)
        {
            _options = new DbContextOptionsBuilder<TempleDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;
        }

        public DbContext CreateDbContext(DatabaseProvider provider, string connectionString)
        {
            return new TempleDbContext(_options);
        }

        public TempleDbContext CreateTempleDbContext()
        {
            return new TempleDbContext(_options);
        }
    }
}