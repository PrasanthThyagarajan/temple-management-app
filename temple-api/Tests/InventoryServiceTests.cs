using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services;
using TempleApi.Services.Interfaces;
using TempleApi.Enums;
using Xunit;

namespace TempleApi.Tests
{
    public class InventoryServiceTests : IDisposable
    {
        private readonly TempleDbContext _context;
        private readonly InventoryService _service;
        private readonly Mock<ILogger<InventoryService>> _mockLogger;

        public InventoryServiceTests()
        {
            var options = new DbContextOptionsBuilder<TempleDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TempleDbContext(options);
            _mockLogger = new Mock<ILogger<InventoryService>>();
            _service = new InventoryService(_context, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        private async Task<(Temple temple, Area area)> SetupTestDataAsync()
        {
            var temple = new Temple 
            { 
                Name = "Test Temple", 
                Address = "Test Address", 
                City = "Test City", 
                State = "Test State", 
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Temples.Add(temple);
            await _context.SaveChangesAsync();
            
            var area = new Area 
            { 
                TempleId = temple.Id, 
                Name = "Test Area", 
                Description = "Test Area Description",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Areas.Add(area);
            await _context.SaveChangesAsync();

            return (temple, area);
        }

        private async Task<Inventory> CreateTestInventoryAsync(Temple temple, Area area)
        {
            var inventory = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Test Item",
                ItemWorth = ItemWorth.Medium,
                ApproximatePrice = 100.50m,
                Quantity = 5,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
            return inventory;
        }

        #region Create Tests

        [Fact]
        public async Task CreateAsync_ShouldCreateInventory_WhenValidData()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            var createDto = new CreateInventoryDto
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Test Item",
                ItemWorth = ItemWorth.High,
                ApproximatePrice = 250.75m,
                Quantity = 10,
                CreatedDate = DateTime.UtcNow,
                Active = true
            };

            // Act
            var result = await _service.CreateAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.ItemName.Should().Be(createDto.ItemName);
            result.TempleId.Should().Be(createDto.TempleId);
            result.AreaId.Should().Be(createDto.AreaId);
            result.ItemWorth.Should().Be(createDto.ItemWorth);
            result.ApproximatePrice.Should().Be(createDto.ApproximatePrice);
            result.Quantity.Should().Be(createDto.Quantity);
            result.Active.Should().Be(createDto.Active);
            result.Id.Should().BeGreaterThan(0);
            result.TempleName.Should().Be(temple.Name);
            result.AreaName.Should().Be(area.Name);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenTempleNotFound()
        {
            // Arrange
            var (_, area) = await SetupTestDataAsync();
            var createDto = new CreateInventoryDto
            {
                TempleId = 999, // Non-existent temple
                AreaId = area.Id,
                ItemName = "Test Item",
                ItemWorth = ItemWorth.Medium,
                ApproximatePrice = 100.00m,
                Quantity = 1,
                CreatedDate = DateTime.UtcNow,
                Active = true
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(createDto));
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenAreaNotFound()
        {
            // Arrange
            var (temple, _) = await SetupTestDataAsync();
            var createDto = new CreateInventoryDto
            {
                TempleId = temple.Id,
                AreaId = 999, // Non-existent area
                ItemName = "Test Item",
                ItemWorth = ItemWorth.Medium,
                ApproximatePrice = 100.00m,
                Quantity = 1,
                CreatedDate = DateTime.UtcNow,
                Active = true
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(createDto));
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenAreaDoesNotBelongToTemple()
        {
            // Arrange
            var (temple, _) = await SetupTestDataAsync();
            var anotherTemple = new Temple 
            { 
                Name = "Another Temple", 
                Address = "Another Address", 
                City = "Another City", 
                State = "Another State", 
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Temples.Add(anotherTemple);
            await _context.SaveChangesAsync();

            var anotherArea = new Area 
            { 
                TempleId = anotherTemple.Id, 
                Name = "Another Area", 
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Areas.Add(anotherArea);
            await _context.SaveChangesAsync();

            var createDto = new CreateInventoryDto
            {
                TempleId = temple.Id,
                AreaId = anotherArea.Id, // Area belongs to different temple
                ItemName = "Test Item",
                ItemWorth = ItemWorth.Medium,
                ApproximatePrice = 100.00m,
                Quantity = 1,
                CreatedDate = DateTime.UtcNow,
                Active = true
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(createDto));
        }

        #endregion

        #region Read Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllInventories()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            await CreateTestInventoryAsync(temple, area);
            await CreateTestInventoryAsync(temple, area);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnInventory_WhenExists()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            var inventory = await CreateTestInventoryAsync(temple, area);

            // Act
            var result = await _service.GetByIdAsync(inventory.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(inventory.Id);
            result.ItemName.Should().Be(inventory.ItemName);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var result = await _service.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByTempleIdAsync_ShouldReturnInventoriesForTemple()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            await CreateTestInventoryAsync(temple, area);

            var anotherTemple = new Temple 
            { 
                Name = "Another Temple", 
                Address = "Another Address", 
                City = "Another City", 
                State = "Another State", 
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Temples.Add(anotherTemple);
            await _context.SaveChangesAsync();

            var anotherArea = new Area 
            { 
                TempleId = anotherTemple.Id, 
                Name = "Another Area", 
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Areas.Add(anotherArea);
            await _context.SaveChangesAsync();

            var anotherInventory = new Inventory
            {
                TempleId = anotherTemple.Id,
                AreaId = anotherArea.Id,
                ItemName = "Another Item",
                ItemWorth = ItemWorth.Low,
                ApproximatePrice = 50.00m,
                Quantity = 3,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Inventories.Add(anotherInventory);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetByTempleIdAsync(temple.Id);

            // Assert
            result.Should().HaveCount(1);
            result.First().TempleId.Should().Be(temple.Id);
        }

        [Fact]
        public async Task GetByAreaIdAsync_ShouldReturnInventoriesForArea()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            await CreateTestInventoryAsync(temple, area);

            var anotherArea = new Area 
            { 
                TempleId = temple.Id, 
                Name = "Another Area", 
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Areas.Add(anotherArea);
            await _context.SaveChangesAsync();

            var anotherInventory = new Inventory
            {
                TempleId = temple.Id,
                AreaId = anotherArea.Id,
                ItemName = "Another Item",
                ItemWorth = ItemWorth.Low,
                ApproximatePrice = 50.00m,
                Quantity = 3,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Inventories.Add(anotherInventory);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetByAreaIdAsync(area.Id);

            // Assert
            result.Should().HaveCount(1);
            result.First().AreaId.Should().Be(area.Id);
        }

        [Fact]
        public async Task GetByItemWorthAsync_ShouldReturnInventoriesByWorth()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            
            var highWorthInventory = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "High Worth Item",
                ItemWorth = ItemWorth.High,
                ApproximatePrice = 500.00m,
                Quantity = 2,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var lowWorthInventory = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Low Worth Item",
                ItemWorth = ItemWorth.Low,
                ApproximatePrice = 10.00m,
                Quantity = 10,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Inventories.AddRange(highWorthInventory, lowWorthInventory);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetByItemWorthAsync(ItemWorth.High);

            // Assert
            result.Should().HaveCount(1);
            result.First().ItemWorth.Should().Be(ItemWorth.High);
        }

        [Fact]
        public async Task GetActiveItemsAsync_ShouldReturnOnlyActiveItems()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            
            var activeInventory = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Active Item",
                ItemWorth = ItemWorth.Medium,
                ApproximatePrice = 100.00m,
                Quantity = 5,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var inactiveInventory = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Inactive Item",
                ItemWorth = ItemWorth.Low,
                ApproximatePrice = 50.00m,
                Quantity = 3,
                CreatedDate = DateTime.UtcNow,
                Active = false,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Inventories.AddRange(activeInventory, inactiveInventory);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetActiveItemsAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().Active.Should().BeTrue();
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task UpdateAsync_ShouldUpdateInventory_WhenValidData()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            var inventory = await CreateTestInventoryAsync(temple, area);
            
            var updateDto = new CreateInventoryDto
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Updated Item Name",
                ItemWorth = ItemWorth.Precious,
                ApproximatePrice = 1000.00m,
                Quantity = 2,
                CreatedDate = DateTime.UtcNow.AddDays(-1),
                Active = false
            };

            // Act
            var result = await _service.UpdateAsync(inventory.Id, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.ItemName.Should().Be(updateDto.ItemName);
            result.ItemWorth.Should().Be(updateDto.ItemWorth);
            result.ApproximatePrice.Should().Be(updateDto.ApproximatePrice);
            result.Quantity.Should().Be(updateDto.Quantity);
            result.Active.Should().Be(updateDto.Active);
            result.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenInventoryNotFound()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            var updateDto = new CreateInventoryDto
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Updated Item",
                ItemWorth = ItemWorth.Medium,
                ApproximatePrice = 100.00m,
                Quantity = 1,
                CreatedDate = DateTime.UtcNow,
                Active = true
            };

            // Act
            var result = await _service.UpdateAsync(999, updateDto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenTempleNotFound()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            var inventory = await CreateTestInventoryAsync(temple, area);
            
            var updateDto = new CreateInventoryDto
            {
                TempleId = 999, // Non-existent temple
                AreaId = area.Id,
                ItemName = "Updated Item",
                ItemWorth = ItemWorth.Medium,
                ApproximatePrice = 100.00m,
                Quantity = 1,
                CreatedDate = DateTime.UtcNow,
                Active = true
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(inventory.Id, updateDto));
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task DeleteAsync_ShouldDeleteInventory_WhenExists()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            var inventory = await CreateTestInventoryAsync(temple, area);

            // Act
            var result = await _service.DeleteAsync(inventory.Id);

            // Assert
            result.Should().BeTrue();
            var deletedInventory = await _context.Inventories.FindAsync(inventory.Id);
            deletedInventory.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
        {
            // Act
            var result = await _service.DeleteAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Calculation Tests

        [Fact]
        public async Task GetTotalValueByAreaAsync_ShouldCalculateCorrectTotal()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            
            var inventory1 = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Item 1",
                ItemWorth = ItemWorth.Medium,
                ApproximatePrice = 100.00m,
                Quantity = 2,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var inventory2 = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Item 2",
                ItemWorth = ItemWorth.High,
                ApproximatePrice = 50.00m,
                Quantity = 3,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var inactiveInventory = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Inactive Item",
                ItemWorth = ItemWorth.Low,
                ApproximatePrice = 25.00m,
                Quantity = 4,
                CreatedDate = DateTime.UtcNow,
                Active = false, // Should not be included
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Inventories.AddRange(inventory1, inventory2, inactiveInventory);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetTotalValueByAreaAsync(area.Id);

            // Assert
            // (100 * 2) + (50 * 3) = 200 + 150 = 350
            result.Should().Be(350.00m);
        }

        [Fact]
        public async Task GetTotalQuantityByTempleAsync_ShouldCalculateCorrectTotal()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            
            var inventory1 = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Item 1",
                ItemWorth = ItemWorth.Medium,
                ApproximatePrice = 100.00m,
                Quantity = 5,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var inventory2 = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Item 2",
                ItemWorth = ItemWorth.High,
                ApproximatePrice = 50.00m,
                Quantity = 3,
                CreatedDate = DateTime.UtcNow,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var inactiveInventory = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Inactive Item",
                ItemWorth = ItemWorth.Low,
                ApproximatePrice = 25.00m,
                Quantity = 2,
                CreatedDate = DateTime.UtcNow,
                Active = false, // Should not be included
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Inventories.AddRange(inventory1, inventory2, inactiveInventory);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetTotalQuantityByTempleAsync(temple.Id);

            // Assert
            // 5 + 3 = 8 (inactive item excluded)
            result.Should().Be(8);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoInventories()
        {
            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByTempleIdAsync_ShouldReturnEmptyList_WhenNoInventoriesForTemple()
        {
            // Arrange
            var (temple, _) = await SetupTestDataAsync();

            // Act
            var result = await _service.GetByTempleIdAsync(temple.Id);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByAreaIdAsync_ShouldReturnEmptyList_WhenNoInventoriesForArea()
        {
            // Arrange
            var (_, area) = await SetupTestDataAsync();

            // Act
            var result = await _service.GetByAreaIdAsync(area.Id);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByItemWorthAsync_ShouldReturnEmptyList_WhenNoInventoriesWithWorth()
        {
            // Act
            var result = await _service.GetByItemWorthAsync(ItemWorth.Precious);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetActiveItemsAsync_ShouldReturnEmptyList_WhenNoActiveItems()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();
            
            var inactiveInventory = new Inventory
            {
                TempleId = temple.Id,
                AreaId = area.Id,
                ItemName = "Inactive Item",
                ItemWorth = ItemWorth.Low,
                ApproximatePrice = 25.00m,
                Quantity = 2,
                CreatedDate = DateTime.UtcNow,
                Active = false,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Inventories.Add(inactiveInventory);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetActiveItemsAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetTotalValueByAreaAsync_ShouldReturnZero_WhenNoActiveItemsInArea()
        {
            // Arrange
            var (temple, area) = await SetupTestDataAsync();

            // Act
            var result = await _service.GetTotalValueByAreaAsync(area.Id);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public async Task GetTotalQuantityByTempleAsync_ShouldReturnZero_WhenNoActiveItemsInTemple()
        {
            // Arrange
            var (temple, _) = await SetupTestDataAsync();

            // Act
            var result = await _service.GetTotalQuantityByTempleAsync(temple.Id);

            // Assert
            result.Should().Be(0);
        }

        #endregion
    }
}
