using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using TempleApi.Data;
using TempleApi.Models;
using TempleApi.Models.DTOs;
using TempleApi.Services;
using Xunit;

namespace TempleApi.Tests;

public class TempleServiceTests
{
    private readonly DbContextOptions<TempleDbContext> _options;

    public TempleServiceTests()
    {
        _options = new DbContextOptionsBuilder<TempleDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private TempleDbContext CreateContext()
    {
        return new TempleDbContext(_options);
    }

    [Fact]
    public async Task GetAllTemplesAsync_ShouldReturnAllTemples()
    {
        // Arrange
        using var context = CreateContext();
        var service = new TempleService(context);

        var temple1 = new Temple { Name = "Test Temple 1", Address = "Test Address 1" };
        var temple2 = new Temple { Name = "Test Temple 2", Address = "Test Address 2" };

        context.Temples.AddRange(temple1, temple2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllTemplesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Name == "Test Temple 1");
        result.Should().Contain(t => t.Name == "Test Temple 2");
    }

    [Fact]
    public async Task GetTempleByIdAsync_WithValidId_ShouldReturnTemple()
    {
        // Arrange
        using var context = CreateContext();
        var service = new TempleService(context);

        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetTempleByIdAsync(temple.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Temple");
        result.Address.Should().Be("Test Address");
    }

    [Fact]
    public async Task GetTempleByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var service = new TempleService(context);

        // Act
        var result = await service.GetTempleByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateTempleAsync_WithValidDto_ShouldCreateTemple()
    {
        // Arrange
        using var context = CreateContext();
        var service = new TempleService(context);

        var createDto = new CreateTempleDto
        {
            Name = "New Temple",
            Address = "New Address",
            Description = "New Description"
        };

        // Act
        var result = await service.CreateTempleAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Temple");
        result.Address.Should().Be("New Address");
        result.Description.Should().Be("New Description");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify it was saved to database
        var savedTemple = await context.Temples.FindAsync(result.Id);
        savedTemple.Should().NotBeNull();
        savedTemple!.Name.Should().Be("New Temple");
    }

    [Fact]
    public async Task UpdateTempleAsync_WithValidId_ShouldUpdateTemple()
    {
        // Arrange
        using var context = CreateContext();
        var service = new TempleService(context);

        var temple = new Temple { Name = "Old Name", Address = "Old Address", Description = "Old Description" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var updateDto = new CreateTempleDto
        {
            Name = "Updated Name",
            Address = "Updated Address",
            Description = "Updated Description"
        };

        // Act
        var result = await service.UpdateTempleAsync(temple.Id, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Address.Should().Be("Updated Address");
        result.Description.Should().Be("Updated Description");
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateTempleAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var service = new TempleService(context);

        var updateDto = new CreateTempleDto
        {
            Name = "Updated Name",
            Address = "Updated Address",
            Description = "Updated Description"
        };

        // Act
        var result = await service.UpdateTempleAsync(999, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTempleAsync_WithValidId_ShouldSoftDeleteTemple()
    {
        // Arrange
        using var context = CreateContext();
        var service = new TempleService(context);

        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteTempleAsync(temple.Id);

        // Assert
        result.Should().BeTrue();

        // Verify soft delete
        var deletedTemple = await context.Temples.FindAsync(temple.Id);
        deletedTemple.Should().NotBeNull();
        deletedTemple!.IsActive.Should().BeFalse();
        deletedTemple.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DeleteTempleAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        using var context = CreateContext();
        var service = new TempleService(context);

        // Act
        var result = await service.DeleteTempleAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SearchTemplesAsync_WithValidTerm_ShouldReturnMatchingTemples()
    {
        // Arrange
        using var context = CreateContext();
        var service = new TempleService(context);

        var temple1 = new Temple { Name = "Hindu Temple", Address = "Address 1" };
        var temple2 = new Temple { Name = "Buddhist Temple", Address = "Address 2" };
        var temple3 = new Temple { Name = "Jain Temple", Address = "Address 3" };

        context.Temples.AddRange(temple1, temple2, temple3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchTemplesAsync("Hindu");

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(t => t.Name == "Hindu Temple");
    }

    [Fact]
    public async Task SearchTemplesAsync_WithInvalidTerm_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = CreateContext();
        var service = new TempleService(context);

        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchTemplesAsync("Nonexistent");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTemplesByLocationAsync_WithValidCity_ShouldReturnTemples()
    {
        // Arrange
        using var context = CreateContext();
        var service = new TempleService(context);

        var temple1 = new Temple { Name = "Temple 1", Address = "Address 1", City = "Mumbai", State = "Maharashtra" };
        var temple2 = new Temple { Name = "Temple 2", Address = "Address 2", City = "Mumbai", State = "Maharashtra" };
        var temple3 = new Temple { Name = "Temple 3", Address = "Address 3", City = "Delhi", State = "Delhi" };

        context.Temples.AddRange(temple1, temple2, temple3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetTemplesByLocationAsync("Mumbai", null);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Name == "Temple 1");
        result.Should().Contain(t => t.Name == "Temple 2");
        result.Should().NotContain(t => t.Name == "Temple 3");
    }

    [Fact]
    public async Task GetTemplesByLocationAsync_WithValidCityAndState_ShouldReturnTemples()
    {
        // Arrange
        using var context = CreateContext();
        var service = new TempleService(context);

        var temple1 = new Temple { Name = "Temple 1", Address = "Address 1", City = "Mumbai", State = "Maharashtra" };
        var temple2 = new Temple { Name = "Temple 2", Address = "Address 2", City = "Mumbai", State = "Maharashtra" };
        var temple3 = new Temple { Name = "Temple 3", Address = "Address 3", City = "Mumbai", State = "Delhi" };

        context.Temples.AddRange(temple1, temple2, temple3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetTemplesByLocationAsync("Mumbai", "Maharashtra");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Name == "Temple 1");
        result.Should().Contain(t => t.Name == "Temple 2");
        result.Should().NotContain(t => t.Name == "Temple 3");
    }
}
