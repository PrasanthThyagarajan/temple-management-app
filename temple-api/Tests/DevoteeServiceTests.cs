using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using TempleApi.Data;
using TempleApi.Models;
using TempleApi.Models.DTOs;
using TempleApi.Services;
using Xunit;

namespace TempleApi.Tests;

public class DevoteeServiceTests
{
    private readonly DbContextOptions<TempleDbContext> _options;

    public DevoteeServiceTests()
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
    public async Task GetAllDevoteesAsync_ShouldReturnAllDevotees()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DevoteeService(context);

        // Create a temple first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var devotee1 = new Devotee { FirstName = "John", LastName = "Doe", Email = "john@example.com", TempleId = temple.Id };
        var devotee2 = new Devotee { FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", TempleId = temple.Id };

        context.Devotees.AddRange(devotee1, devotee2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllDevoteesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.FirstName == "John");
        result.Should().Contain(d => d.FirstName == "Jane");
    }

    [Fact]
    public async Task GetDevoteeByIdAsync_WithValidId_ShouldReturnDevotee()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DevoteeService(context);

        // Create a temple first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var devotee = new Devotee { FirstName = "John", LastName = "Doe", Email = "john@example.com", TempleId = temple.Id };
        context.Devotees.Add(devotee);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetDevoteeByIdAsync(devotee.Id);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task GetDevoteeByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DevoteeService(context);

        // Act
        var result = await service.GetDevoteeByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateDevoteeAsync_WithValidDto_ShouldCreateDevotee()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DevoteeService(context);

        // Create a temple first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var createDto = new CreateDevoteeDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            TempleId = temple.Id
        };

        // Act
        var result = await service.CreateDevoteeAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john@example.com");
        result.PhoneNumber.Should().Be("1234567890");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify it was saved to database
        var savedDevotee = await context.Devotees.FindAsync(result.Id);
        savedDevotee.Should().NotBeNull();
        savedDevotee!.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task UpdateDevoteeAsync_WithValidId_ShouldUpdateDevotee()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DevoteeService(context);

        // Create a temple first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var devotee = new Devotee { FirstName = "Old", LastName = "Name", Email = "old@example.com", TempleId = temple.Id };
        context.Devotees.Add(devotee);
        await context.SaveChangesAsync();

        var updateDto = new CreateDevoteeDto
        {
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated@example.com",
            PhoneNumber = "0987654321",
            TempleId = temple.Id
        };

        // Act
        var result = await service.UpdateDevoteeAsync(devotee.Id, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Updated");
        result.Email.Should().Be("updated@example.com");
        result.PhoneNumber.Should().Be("0987654321");
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateDevoteeAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DevoteeService(context);

        // Create a temple first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var updateDto = new CreateDevoteeDto
        {
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated@example.com",
            TempleId = temple.Id
        };

        // Act
        var result = await service.UpdateDevoteeAsync(999, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteDevoteeAsync_WithValidId_ShouldSoftDeleteDevotee()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DevoteeService(context);

        // Create a temple first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var devotee = new Devotee { FirstName = "John", LastName = "Doe", Email = "john@example.com", TempleId = temple.Id };
        context.Devotees.Add(devotee);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteDevoteeAsync(devotee.Id);

        // Assert
        result.Should().BeTrue();

        // Verify soft delete
        var deletedDevotee = await context.Devotees.FindAsync(devotee.Id);
        deletedDevotee.Should().NotBeNull();
        deletedDevotee!.IsActive.Should().BeFalse();
        deletedDevotee.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DeleteDevoteeAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DevoteeService(context);

        // Act
        var result = await service.DeleteDevoteeAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetDevoteesByTempleAsync_WithValidTempleId_ShouldReturnDevotees()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DevoteeService(context);

        // Create temples
        var temple1 = new Temple { Name = "Temple 1", Address = "Address 1" };
        var temple2 = new Temple { Name = "Temple 2", Address = "Address 2" };
        context.Temples.AddRange(temple1, temple2);
        await context.SaveChangesAsync();

        var devotee1 = new Devotee { FirstName = "John", LastName = "Doe", TempleId = temple1.Id };
        var devotee2 = new Devotee { FirstName = "Jane", LastName = "Smith", TempleId = temple1.Id };
        var devotee3 = new Devotee { FirstName = "Bob", LastName = "Johnson", TempleId = temple2.Id };

        context.Devotees.AddRange(devotee1, devotee2, devotee3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetDevoteesByTempleAsync(temple1.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.FirstName == "John");
        result.Should().Contain(d => d.FirstName == "Jane");
        result.Should().NotContain(d => d.FirstName == "Bob");
    }

    [Fact]
    public async Task SearchDevoteesAsync_WithValidTerm_ShouldReturnMatchingDevotees()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DevoteeService(context);

        // Create a temple first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var devotee1 = new Devotee { FirstName = "John", LastName = "Doe", Email = "john@example.com", TempleId = temple.Id };
        var devotee2 = new Devotee { FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", TempleId = temple.Id };
        var devotee3 = new Devotee { FirstName = "Bob", LastName = "Johnson", Email = "bob@example.com", TempleId = temple.Id };

        context.Devotees.AddRange(devotee1, devotee2, devotee3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchDevoteesAsync("John");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.FirstName == "John");
        result.Should().Contain(d => d.LastName == "Johnson");
    }

    [Fact]
    public async Task SearchDevoteesAsync_WithInvalidTerm_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DevoteeService(context);

        // Create a temple first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var devotee = new Devotee { FirstName = "John", LastName = "Doe", TempleId = temple.Id };
        context.Devotees.Add(devotee);
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchDevoteesAsync("Nonexistent");

        // Assert
        result.Should().BeEmpty();
    }
}
