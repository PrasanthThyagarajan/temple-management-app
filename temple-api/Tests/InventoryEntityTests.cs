using System;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using TempleApi.Domain.Entities;
using TempleApi.Enums;
using TempleApi.Models.DTOs;
using Xunit;

namespace TempleApi.Tests
{
    public class InventoryEntityTests
    {
        [Fact]
        public void Inventory_ShouldInheritFromBaseEntity()
        {
            // Arrange & Act
            var inventory = new Inventory();

            // Assert
            inventory.Should().BeAssignableTo<BaseEntity>();
        }

        [Fact]
        public void Inventory_ShouldHaveRequiredProperties()
        {
            // Arrange
            var inventory = new Inventory();

            // Act & Assert
            inventory.TempleId.Should().Be(0);
            inventory.AreaId.Should().Be(0);
            inventory.ItemName.Should().Be(string.Empty);
            inventory.ItemWorth.Should().Be(ItemWorth.Low); // Default enum value
            inventory.ApproximatePrice.Should().Be(0);
            inventory.Quantity.Should().Be(0);
            inventory.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            inventory.Active.Should().BeTrue();
        }

        [Fact]
        public void Inventory_ShouldSetDefaultValues()
        {
            // Arrange & Act
            var inventory = new Inventory();

            // Assert
            inventory.ItemName.Should().Be(string.Empty);
            inventory.Active.Should().BeTrue();
            inventory.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Fact]
        public void Inventory_ShouldAllowValidValues()
        {
            // Arrange
            var inventory = new Inventory
            {
                TempleId = 1,
                AreaId = 2,
                ItemName = "Test Item",
                ItemWorth = ItemWorth.High,
                ApproximatePrice = 250.75m,
                Quantity = 10,
                CreatedDate = DateTime.UtcNow.AddDays(-1),
                Active = false
            };

            // Act & Assert
            inventory.TempleId.Should().Be(1);
            inventory.AreaId.Should().Be(2);
            inventory.ItemName.Should().Be("Test Item");
            inventory.ItemWorth.Should().Be(ItemWorth.High);
            inventory.ApproximatePrice.Should().Be(250.75m);
            inventory.Quantity.Should().Be(10);
            inventory.CreatedDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(-1), TimeSpan.FromMinutes(1));
            inventory.Active.Should().BeFalse();
        }

        [Theory]
        [InlineData(ItemWorth.Low)]
        [InlineData(ItemWorth.Medium)]
        [InlineData(ItemWorth.High)]
        [InlineData(ItemWorth.Precious)]
        public void Inventory_ShouldAcceptAllItemWorthValues(ItemWorth itemWorth)
        {
            // Arrange
            var inventory = new Inventory();

            // Act
            inventory.ItemWorth = itemWorth;

            // Assert
            inventory.ItemWorth.Should().Be(itemWorth);
        }

        [Fact]
        public void Inventory_ShouldHaveNavigationProperties()
        {
            // Arrange
            var inventory = new Inventory();

            // Act & Assert - Navigation properties should be null by default
            inventory.Temple.Should().BeNull();
            inventory.Area.Should().BeNull();
        }

        [Fact]
        public void Inventory_ShouldHaveJsonIgnoreOnNavigationProperties()
        {
            // Arrange
            var inventory = new Inventory();
            var templeProperty = typeof(Inventory).GetProperty("Temple");
            var areaProperty = typeof(Inventory).GetProperty("Area");

            // Act & Assert
            templeProperty.Should().NotBeNull();
            areaProperty.Should().NotBeNull();

            var templeJsonIgnore = templeProperty.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonIgnoreAttribute), false);
            var areaJsonIgnore = areaProperty.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonIgnoreAttribute), false);

            templeJsonIgnore.Should().HaveCount(1);
            areaJsonIgnore.Should().HaveCount(1);
        }
    }

    public class InventoryDtoTests
    {
        [Fact]
        public void InventoryDto_ShouldHaveAllRequiredProperties()
        {
            // Arrange
            var dto = new InventoryDto();

            // Act & Assert
            dto.Id.Should().Be(0);
            dto.TempleId.Should().Be(0);
            dto.TempleName.Should().BeNull();
            dto.AreaId.Should().Be(0);
            dto.AreaName.Should().BeNull();
            dto.ItemName.Should().Be(string.Empty);
            dto.ItemWorth.Should().Be(ItemWorth.Low);
            dto.ItemWorthDisplay.Should().Be("Low");
            dto.ApproximatePrice.Should().Be(0);
            dto.Quantity.Should().Be(0);
            dto.CreatedDate.Should().Be(default(DateTime));
            dto.Active.Should().BeFalse();
            dto.CreatedAt.Should().Be(default(DateTime));
            dto.UpdatedAt.Should().BeNull();
            dto.IsActive.Should().BeFalse();
        }

        [Fact]
        public void InventoryDto_ShouldSetItemWorthDisplayCorrectly()
        {
            // Arrange
            var dto = new InventoryDto();

            // Act & Assert
            dto.ItemWorth = ItemWorth.Low;
            dto.ItemWorthDisplay.Should().Be("Low");

            dto.ItemWorth = ItemWorth.Medium;
            dto.ItemWorthDisplay.Should().Be("Medium");

            dto.ItemWorth = ItemWorth.High;
            dto.ItemWorthDisplay.Should().Be("High");

            dto.ItemWorth = ItemWorth.Precious;
            dto.ItemWorthDisplay.Should().Be("Precious");
        }

        [Fact]
        public void InventoryDto_ShouldAllowValidValues()
        {
            // Arrange
            var dto = new InventoryDto
            {
                Id = 1,
                TempleId = 2,
                TempleName = "Test Temple",
                AreaId = 3,
                AreaName = "Test Area",
                ItemName = "Test Item",
                ItemWorth = ItemWorth.High,
                ApproximatePrice = 500.25m,
                Quantity = 15,
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                Active = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                IsActive = true
            };

            // Act & Assert
            dto.Id.Should().Be(1);
            dto.TempleId.Should().Be(2);
            dto.TempleName.Should().Be("Test Temple");
            dto.AreaId.Should().Be(3);
            dto.AreaName.Should().Be("Test Area");
            dto.ItemName.Should().Be("Test Item");
            dto.ItemWorth.Should().Be(ItemWorth.High);
            dto.ItemWorthDisplay.Should().Be("High");
            dto.ApproximatePrice.Should().Be(500.25m);
            dto.Quantity.Should().Be(15);
            dto.CreatedDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(-5), TimeSpan.FromMinutes(1));
            dto.Active.Should().BeTrue();
            dto.CreatedAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(-5), TimeSpan.FromMinutes(1));
            dto.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(-1), TimeSpan.FromMinutes(1));
            dto.IsActive.Should().BeTrue();
        }
    }

    public class CreateInventoryDtoTests
    {
        [Fact]
        public void CreateInventoryDto_ShouldHaveAllRequiredProperties()
        {
            // Arrange
            var dto = new CreateInventoryDto();

            // Act & Assert
            dto.TempleId.Should().Be(0);
            dto.AreaId.Should().Be(0);
            dto.ItemName.Should().Be(string.Empty);
            dto.ItemWorth.Should().Be(ItemWorth.Low);
            dto.ApproximatePrice.Should().Be(0);
            dto.Quantity.Should().Be(0);
            dto.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            dto.Active.Should().BeTrue();
        }

        [Fact]
        public void CreateInventoryDto_ShouldSetDefaultValues()
        {
            // Arrange
            var dto = new CreateInventoryDto();

            // Act & Assert
            dto.ItemName.Should().Be(string.Empty);
            dto.ItemWorth.Should().Be(ItemWorth.Low);
            dto.ApproximatePrice.Should().Be(0);
            dto.Quantity.Should().Be(0);
            dto.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            dto.Active.Should().BeTrue();
        }

        [Fact]
        public void CreateInventoryDto_ShouldAllowValidValues()
        {
            // Arrange
            var dto = new CreateInventoryDto
            {
                TempleId = 1,
                AreaId = 2,
                ItemName = "Test Item",
                ItemWorth = ItemWorth.Precious,
                ApproximatePrice = 1000.00m,
                Quantity = 1,
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                Active = false
            };

            // Act & Assert
            dto.TempleId.Should().Be(1);
            dto.AreaId.Should().Be(2);
            dto.ItemName.Should().Be("Test Item");
            dto.ItemWorth.Should().Be(ItemWorth.Precious);
            dto.ApproximatePrice.Should().Be(1000.00m);
            dto.Quantity.Should().Be(1);
            dto.CreatedDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(-10), TimeSpan.FromMinutes(1));
            dto.Active.Should().BeFalse();
        }

        [Theory]
        [InlineData(ItemWorth.Low)]
        [InlineData(ItemWorth.Medium)]
        [InlineData(ItemWorth.High)]
        [InlineData(ItemWorth.Precious)]
        public void CreateInventoryDto_ShouldAcceptAllItemWorthValues(ItemWorth itemWorth)
        {
            // Arrange
            var dto = new CreateInventoryDto();

            // Act
            dto.ItemWorth = itemWorth;

            // Assert
            dto.ItemWorth.Should().Be(itemWorth);
        }
    }

    public class ItemWorthEnumTests
    {
        [Theory]
        [InlineData(ItemWorth.Low, 0)]
        [InlineData(ItemWorth.Medium, 1)]
        [InlineData(ItemWorth.High, 2)]
        [InlineData(ItemWorth.Precious, 3)]
        public void ItemWorth_ShouldHaveCorrectValues(ItemWorth itemWorth, int expectedValue)
        {
            // Act & Assert
            ((int)itemWorth).Should().Be(expectedValue);
        }

        [Fact]
        public void ItemWorth_ShouldHaveAllExpectedValues()
        {
            // Arrange
            var expectedValues = new[] { 1, 2, 3, 4 };
            var actualValues = Enum.GetValues<ItemWorth>().Select(e => (int)e).ToArray();

            // Act & Assert
            actualValues.Should().BeEquivalentTo(expectedValues);
        }

        [Fact]
        public void ItemWorth_ShouldHaveCorrectStringRepresentations()
        {
            // Act & Assert
            ItemWorth.Low.ToString().Should().Be("Low");
            ItemWorth.Medium.ToString().Should().Be("Medium");
            ItemWorth.High.ToString().Should().Be("High");
            ItemWorth.Precious.ToString().Should().Be("Precious");
        }
    }
}
