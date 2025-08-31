using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using TempleApi.Data;
using TempleApi.Domain.Entities;
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

        var temple1 = new Temple 
        { 
            Name = "Test Temple 1", 
            Address = "Test Address 1",
            City = "Test City 1",
            State = "Test State 1",
            Phone = "123-456-7890",
            Email = "test1@temple.com",
            Deity = "Test Deity 1",
            EstablishedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var temple2 = new Temple 
        { 
            Name = "Test Temple 2", 
            Address = "Test Address 2",
            City = "Test City 2",
            State = "Test State 2",
            Phone = "098-765-4321",
            Email = "test2@temple.com",
            Deity = "Test Deity 2",
            EstablishedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

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

        var temple = new Temple 
        { 
            Name = "Test Temple", 
            Address = "Test Address",
            City = "Test City",
            State = "Test State",
            Phone = "123-456-7890",
            Email = "test@temple.com",
            Deity = "Test Deity",
            EstablishedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
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
            Description = "New Description",
            City = "New City",
            State = "New State",
            PhoneNumber = "123-456-7890",
            Email = "new@temple.com",
            Deity = "New Deity",
            EstablishedDate = DateTime.UtcNow
        };

        // Act
        var result = await service.CreateTempleAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Temple");
        result.Address.Should().Be("New Address");
        result.Description.Should().Be("New Description");
        result.City.Should().Be("New City");
        result.State.Should().Be("New State");
        result.Phone.Should().Be("123-456-7890");
        result.Email.Should().Be("new@temple.com");
        result.Deity.Should().Be("New Deity");
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateTempleAsync_WithValidId_ShouldUpdateTemple()
    {
        // Arrange
        using var context = CreateContext();
        var service = new TempleService(context);

        var temple = new Temple 
        { 
            Name = "Original Temple", 
            Address = "Original Address",
            City = "Original City",
            State = "Original State",
            Phone = "123-456-7890",
            Email = "original@temple.com",
            Deity = "Original Deity",
            EstablishedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var updateDto = new CreateTempleDto
        {
            Name = "Updated Temple",
            Address = "Updated Address",
            Description = "Updated Description",
            City = "Updated City",
            State = "Updated State",
            PhoneNumber = "098-765-4321",
            Email = "updated@temple.com",
            Deity = "Updated Deity",
            EstablishedDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = await service.UpdateTempleAsync(temple.Id, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Temple");
        result.Address.Should().Be("Updated Address");
        result.Description.Should().Be("Updated Description");
        result.City.Should().Be("Updated City");
        result.State.Should().Be("Updated State");
        result.Phone.Should().Be("098-765-4321");
        result.Email.Should().Be("updated@temple.com");
        result.Deity.Should().Be("Updated Deity");
    }

    [Fact]
    public async Task UpdateTempleAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var service = new TempleService(context);

        var updateDto = new CreateTempleDto
        {
            Name = "Updated Temple",
            Address = "Updated Address"
        };

        // Act
        var result = await service.UpdateTempleAsync(999, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTempleAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        using var context = CreateContext();
        var service = new TempleService(context);

        var temple = new Temple 
        { 
            Name = "Test Temple", 
            Address = "Test Address",
            City = "Test City",
            State = "Test State",
            Phone = "123-456-7890",
            Email = "test@temple.com",
            Deity = "Test Deity",
            EstablishedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteTempleAsync(temple.Id);

        // Assert
        result.Should().BeTrue();
        var deletedTemple = await context.Temples.FindAsync(temple.Id);
        deletedTemple!.IsActive.Should().BeFalse();
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
    public async Task SearchTemplesAsync_WithValidSearchTerm_ShouldReturnMatchingTemples()
    {
        // Arrange
        using var context = CreateContext();
        var service = new TempleService(context);

        var temple1 = new Temple 
        { 
            Name = "Hindu Temple", 
            Address = "Test Address 1",
            City = "Mumbai",
            State = "Maharashtra",
            Phone = "123-456-7890",
            Email = "hindu@temple.com",
            Deity = "Shiva",
            EstablishedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var temple2 = new Temple 
        { 
            Name = "Buddhist Temple", 
            Address = "Test Address 2",
            City = "Delhi",
            State = "Delhi",
            Phone = "098-765-4321",
            Email = "buddhist@temple.com",
            Deity = "Buddha",
            EstablishedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var temple3 = new Temple 
        { 
            Name = "Jain Temple", 
            Address = "Test Address 3",
            City = "Ahmedabad",
            State = "Gujarat",
            Phone = "555-555-5555",
            Email = "jain@temple.com",
            Deity = "Mahavira",
            EstablishedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Temples.AddRange(temple1, temple2, temple3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchTemplesAsync("Hindu");

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Hindu Temple");
    }

    [Fact]
    public async Task SearchTemplesAsync_WithEmptySearchTerm_ShouldReturnAllTemples()
    {
        // Arrange
        using var context = CreateContext();
        var service = new TempleService(context);

        var temple1 = new Temple 
        { 
            Name = "Test Temple 1", 
            Address = "Test Address 1",
            City = "Test City 1",
            State = "Test State 1",
            Phone = "123-456-7890",
            Email = "test1@temple.com",
            Deity = "Test Deity 1",
            EstablishedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var temple2 = new Temple 
        { 
            Name = "Test Temple 2", 
            Address = "Test Address 2",
            City = "Test City 2",
            State = "Test State 2",
            Phone = "098-765-4321",
            Email = "test2@temple.com",
            Deity = "Test Deity 2",
            EstablishedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Temples.AddRange(temple1, temple2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchTemplesAsync("");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTemplesByLocationAsync_WithValidCity_ShouldReturnTemplesInCity()
    {
        // Arrange
        using var context = CreateContext();
        var service = new TempleService(context);

        var temple1 = new Temple 
        { 
            Name = "Temple 1", 
            Address = "Address 1",
            City = "Mumbai",
            State = "Maharashtra",
            Phone = "123-456-7890",
            Email = "temple1@temple.com",
            Deity = "Deity 1",
            EstablishedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var temple2 = new Temple 
        { 
            Name = "Temple 2", 
            Address = "Address 2",
            City = "Mumbai",
            State = "Maharashtra",
            Phone = "098-765-4321",
            Email = "temple2@temple.com",
            Deity = "Deity 2",
            EstablishedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var temple3 = new Temple 
        { 
            Name = "Temple 3", 
            Address = "Address 3",
            City = "Delhi",
            State = "Delhi",
            Phone = "555-555-5555",
            Email = "temple3@temple.com",
            Deity = "Deity 3",
            EstablishedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Temples.AddRange(temple1, temple2, temple3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetTemplesByLocationAsync("Mumbai");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.City == "Mumbai");
    }

    [Fact]
    public async Task GetTemplesByLocationAsync_WithCityAndState_ShouldReturnTemplesInLocation()
    {
        // Arrange
        using var context = CreateContext();
        var service = new TempleService(context);

        var temple1 = new Temple 
        { 
            Name = "Temple 1", 
            Address = "Address 1",
            City = "Mumbai",
            State = "Maharashtra",
            Phone = "123-456-7890",
            Email = "temple1@temple.com",
            Deity = "Deity 1",
            EstablishedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var temple2 = new Temple 
        { 
            Name = "Temple 2", 
            Address = "Address 2",
            City = "Mumbai",
            State = "Karnataka",
            Phone = "098-765-4321",
            Email = "temple2@temple.com",
            Deity = "Deity 2",
            EstablishedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Temples.AddRange(temple1, temple2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetTemplesByLocationAsync("Mumbai", "Maharashtra");

        // Assert
        result.Should().HaveCount(1);
        result.First().State.Should().Be("Maharashtra");
    }
}
