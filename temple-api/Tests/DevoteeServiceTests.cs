using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using TempleApi.Data;
using TempleApi.Domain.Entities;
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

        var devotee1 = new Devotee 
        { 
            FirstName = "John", 
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "123-456-7890",
            Address = "123 Main St",
            City = "Test City",
            State = "Test State",
            PostalCode = "12345",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var devotee2 = new Devotee 
        { 
            FirstName = "Jane", 
            LastName = "Smith",
            Email = "jane@example.com",
            Phone = "098-765-4321",
            Address = "456 Oak St",
            City = "Test City",
            State = "Test State",
            PostalCode = "54321",
            DateOfBirth = new DateTime(1985, 5, 15),
            Gender = "Female",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

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

        var devotee = new Devotee 
        { 
            FirstName = "John", 
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "123-456-7890",
            Address = "123 Main St",
            City = "Test City",
            State = "Test State",
            PostalCode = "12345",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
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

        var createDto = new CreateDevoteeDto
        {
            FirstName = "New",
            LastName = "Devotee",
            Email = "new@example.com",
            Phone = "555-555-5555",
            Address = "789 Pine St",
            City = "New City",
            State = "New State",
            PostalCode = "67890",
            DateOfBirth = new DateTime(1980, 10, 20),
            Gender = "Male",
            TempleId = temple.Id
        };

        // Act
        var result = await service.CreateDevoteeAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("New");
        result.LastName.Should().Be("Devotee");
        result.Email.Should().Be("new@example.com");
        result.Phone.Should().Be("555-555-5555");
        result.Address.Should().Be("789 Pine St");
        result.City.Should().Be("New City");
        result.State.Should().Be("New State");
        result.PostalCode.Should().Be("67890");
        result.DateOfBirth.Should().Be(new DateTime(1980, 10, 20));
        result.Gender.Should().Be("Male");
        result.TempleId.Should().Be(temple.Id);
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateDevoteeAsync_WithValidId_ShouldUpdateDevotee()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DevoteeService(context);

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

        var devotee = new Devotee 
        { 
            FirstName = "Original", 
            LastName = "Name",
            Email = "original@example.com",
            Phone = "123-456-7890",
            Address = "Original Address",
            City = "Original City",
            State = "Original State",
            PostalCode = "12345",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Devotees.Add(devotee);
        await context.SaveChangesAsync();

        var updateDto = new CreateDevoteeDto
        {
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated@example.com",
            Phone = "098-765-4321",
            Address = "Updated Address",
            City = "Updated City",
            State = "Updated State",
            PostalCode = "54321",
            DateOfBirth = new DateTime(1985, 5, 15),
            Gender = "Female",
            TempleId = temple.Id
        };

        // Act
        var result = await service.UpdateDevoteeAsync(devotee.Id, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Updated");
        result.Email.Should().Be("updated@example.com");
        result.Phone.Should().Be("098-765-4321");
        result.Address.Should().Be("Updated Address");
        result.City.Should().Be("Updated City");
        result.State.Should().Be("Updated State");
        result.PostalCode.Should().Be("54321");
        result.DateOfBirth.Should().Be(new DateTime(1985, 5, 15));
        result.Gender.Should().Be("Female");
    }

    [Fact]
    public async Task UpdateDevoteeAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DevoteeService(context);

        var updateDto = new CreateDevoteeDto
        {
            FirstName = "Updated",
            LastName = "Name",
            TempleId = 1
        };

        // Act
        var result = await service.UpdateDevoteeAsync(999, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteDevoteeAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DevoteeService(context);

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

        var devotee = new Devotee 
        { 
            FirstName = "John", 
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "123-456-7890",
            Address = "123 Main St",
            City = "Test City",
            State = "Test State",
            PostalCode = "12345",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Devotees.Add(devotee);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteDevoteeAsync(devotee.Id);

        // Assert
        result.Should().BeTrue();
        var deletedDevotee = await context.Devotees.FindAsync(devotee.Id);
        deletedDevotee!.IsActive.Should().BeFalse();
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

        var temple1 = new Temple 
        { 
            Name = "Temple 1", 
            Address = "Address 1",
            City = "City 1",
            State = "State 1",
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
            City = "City 2",
            State = "State 2",
            Phone = "098-765-4321",
            Email = "temple2@temple.com",
            Deity = "Deity 2",
            EstablishedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Temples.AddRange(temple1, temple2);
        await context.SaveChangesAsync();

        var devotee1 = new Devotee 
        { 
            FirstName = "John", 
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "123-456-7890",
            Address = "123 Main St",
            City = "City 1",
            State = "State 1",
            PostalCode = "12345",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male",
            TempleId = temple1.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var devotee2 = new Devotee 
        { 
            FirstName = "Jane", 
            LastName = "Smith",
            Email = "jane@example.com",
            Phone = "098-765-4321",
            Address = "456 Oak St",
            City = "City 2",
            State = "State 2",
            PostalCode = "54321",
            DateOfBirth = new DateTime(1985, 5, 15),
            Gender = "Female",
            TempleId = temple2.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var devotee3 = new Devotee 
        { 
            FirstName = "Bob", 
            LastName = "Johnson",
            Email = "bob@example.com",
            Phone = "555-555-5555",
            Address = "789 Pine St",
            City = "City 1",
            State = "State 1",
            PostalCode = "67890",
            DateOfBirth = new DateTime(1980, 10, 20),
            Gender = "Male",
            TempleId = temple1.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Devotees.AddRange(devotee1, devotee2, devotee3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetDevoteesByTempleAsync(temple1.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(d => d.TempleId == temple1.Id);
    }

    [Fact]
    public async Task SearchDevoteesAsync_WithValidSearchTerm_ShouldReturnMatchingDevotees()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DevoteeService(context);

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

        var devotee1 = new Devotee 
        { 
            FirstName = "John", 
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "123-456-7890",
            Address = "123 Main St",
            City = "Mumbai",
            State = "Maharashtra",
            PostalCode = "12345",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var devotee2 = new Devotee 
        { 
            FirstName = "Jane", 
            LastName = "Smith",
            Email = "jane@example.com",
            Phone = "098-765-4321",
            Address = "456 Oak St",
            City = "Delhi",
            State = "Delhi",
            PostalCode = "54321",
            DateOfBirth = new DateTime(1985, 5, 15),
            Gender = "Female",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Devotees.AddRange(devotee1, devotee2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchDevoteesAsync("John");

        // Assert
        result.Should().HaveCount(1);
        result.First().FirstName.Should().Be("John");
    }

    [Fact]
    public async Task SearchDevoteesAsync_WithEmptySearchTerm_ShouldReturnAllDevotees()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DevoteeService(context);

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

        var devotee1 = new Devotee 
        { 
            FirstName = "John", 
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "123-456-7890",
            Address = "123 Main St",
            City = "Test City",
            State = "Test State",
            PostalCode = "12345",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var devotee2 = new Devotee 
        { 
            FirstName = "Jane", 
            LastName = "Smith",
            Email = "jane@example.com",
            Phone = "098-765-4321",
            Address = "456 Oak St",
            City = "Test City",
            State = "Test State",
            PostalCode = "54321",
            DateOfBirth = new DateTime(1985, 5, 15),
            Gender = "Female",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Devotees.AddRange(devotee1, devotee2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchDevoteesAsync("");

        // Assert
        result.Should().HaveCount(2);
    }
}
