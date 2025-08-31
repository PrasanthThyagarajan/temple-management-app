using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services;
using Xunit;

namespace TempleApi.Tests;

public class DonationServiceTests
{
    private readonly DbContextOptions<TempleDbContext> _options;

    public DonationServiceTests()
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
    public async Task GetAllDonationsAsync_ShouldReturnAllDonations()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DonationService(context);

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

        var donation1 = new Donation 
        { 
            TempleId = temple.Id,
            DonorName = "John Doe",
            Amount = 100.00m,
            DonationType = "Cash",
            Purpose = "Temple Maintenance",
            Status = "Completed",
            DonationDate = DateTime.UtcNow.AddDays(-1),
            ReceiptNumber = "R001",
            Notes = "Monthly donation",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var donation2 = new Donation 
        { 
            TempleId = temple.Id,
            DonorName = "Jane Smith",
            Amount = 250.00m,
            DonationType = "Bank Transfer",
            Purpose = "New Equipment",
            Status = "Completed",
            DonationDate = DateTime.UtcNow.AddDays(-2),
            ReceiptNumber = "R002",
            Notes = "Equipment donation",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Donations.AddRange(donation1, donation2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllDonationsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.DonorName == "John Doe");
        result.Should().Contain(d => d.DonorName == "Jane Smith");
    }

    [Fact]
    public async Task GetDonationByIdAsync_WithValidId_ShouldReturnDonation()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DonationService(context);

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

        var donation = new Donation 
        { 
            TempleId = temple.Id,
            DonorName = "John Doe",
            Amount = 100.00m,
            DonationType = "Cash",
            Purpose = "Temple Maintenance",
            Status = "Completed",
            DonationDate = DateTime.UtcNow.AddDays(-1),
            ReceiptNumber = "R001",
            Notes = "Monthly donation",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Donations.Add(donation);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetDonationByIdAsync(donation.Id);

        // Assert
        result.Should().NotBeNull();
        result!.DonorName.Should().Be("John Doe");
        result.Amount.Should().Be(100.00m);
        result.DonationType.Should().Be("Cash");
    }

    [Fact]
    public async Task GetDonationByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DonationService(context);

        // Act
        var result = await service.GetDonationByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDonationsByTempleAsync_WithValidTempleId_ShouldReturnDonations()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DonationService(context);

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

        var donation1 = new Donation 
        { 
            TempleId = temple1.Id,
            DonorName = "John Doe",
            Amount = 100.00m,
            DonationType = "Cash",
            Purpose = "Temple Maintenance",
            Status = "Completed",
            DonationDate = DateTime.UtcNow.AddDays(-1),
            ReceiptNumber = "R001",
            Notes = "Monthly donation",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var donation2 = new Donation 
        { 
            TempleId = temple2.Id,
            DonorName = "Jane Smith",
            Amount = 250.00m,
            DonationType = "Bank Transfer",
            Purpose = "New Equipment",
            Status = "Completed",
            DonationDate = DateTime.UtcNow.AddDays(-2),
            ReceiptNumber = "R002",
            Notes = "Equipment donation",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Donations.AddRange(donation1, donation2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetDonationsByTempleAsync(temple1.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().TempleId.Should().Be(temple1.Id);
    }

    [Fact]
    public async Task GetDonationsByDevoteeAsync_WithValidDevoteeId_ShouldReturnDonations()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DonationService(context);

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

        var donation1 = new Donation 
        { 
            TempleId = temple.Id,
            DevoteeId = devotee.Id,
            DonorName = "John Doe",
            Amount = 100.00m,
            DonationType = "Cash",
            Purpose = "Temple Maintenance",
            Status = "Completed",
            DonationDate = DateTime.UtcNow.AddDays(-1),
            ReceiptNumber = "R001",
            Notes = "Monthly donation",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var donation2 = new Donation 
        { 
            TempleId = temple.Id,
            DevoteeId = devotee.Id,
            DonorName = "John Doe",
            Amount = 250.00m,
            DonationType = "Bank Transfer",
            Purpose = "New Equipment",
            Status = "Completed",
            DonationDate = DateTime.UtcNow.AddDays(-2),
            ReceiptNumber = "R002",
            Notes = "Equipment donation",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Donations.AddRange(donation1, donation2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetDonationsByDevoteeAsync(devotee.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(d => d.DevoteeId == devotee.Id);
    }

    [Fact]
    public async Task CreateDonationAsync_WithValidDto_ShouldCreateDonation()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DonationService(context);

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

        var createDto = new CreateDonationDto
        {
            TempleId = temple.Id,
            DonorName = "New Donor",
            Amount = 500.00m,
            DonationType = "Check",
            Purpose = "Building Fund",
            Status = "Pending",
            DonationDate = DateTime.UtcNow,
            ReceiptNumber = "R003",
            Notes = "Building fund donation"
        };

        // Act
        var result = await service.CreateDonationAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.TempleId.Should().Be(temple.Id);
        result.DonorName.Should().Be("New Donor");
        result.Amount.Should().Be(500.00m);
        result.DonationType.Should().Be("Check");
        result.Purpose.Should().Be("Building Fund");
        result.Status.Should().Be("Pending");
        result.DonationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.ReceiptNumber.Should().Be("R003");
        result.Notes.Should().Be("Building fund donation");
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateDonationStatusAsync_WithValidId_ShouldReturnDonation()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DonationService(context);

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

        var donation = new Donation 
        { 
            TempleId = temple.Id,
            DonorName = "John Doe",
            Amount = 100.00m,
            DonationType = "Cash",
            Purpose = "Temple Maintenance",
            Status = "Pending",
            DonationDate = DateTime.UtcNow.AddDays(-1),
            ReceiptNumber = "R001",
            Notes = "Monthly donation",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Donations.Add(donation);
        await context.SaveChangesAsync();

        // Act
        var result = await service.UpdateDonationStatusAsync(donation.Id, "Completed");

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Completed");
        var updatedDonation = await context.Donations.FindAsync(donation.Id);
        updatedDonation!.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task UpdateDonationStatusAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DonationService(context);

        // Act
        var result = await service.UpdateDonationStatusAsync(999, "Completed");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteDonationAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DonationService(context);

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

        var donation = new Donation 
        { 
            TempleId = temple.Id,
            DonorName = "John Doe",
            Amount = 100.00m,
            DonationType = "Cash",
            Purpose = "Temple Maintenance",
            Status = "Completed",
            DonationDate = DateTime.UtcNow.AddDays(-1),
            ReceiptNumber = "R001",
            Notes = "Monthly donation",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Donations.Add(donation);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteDonationAsync(donation.Id);

        // Assert
        result.Should().BeTrue();
        var deletedDonation = await context.Donations.FindAsync(donation.Id);
        deletedDonation!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteDonationAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DonationService(context);

        // Act
        var result = await service.DeleteDonationAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetTotalDonationsByTempleAsync_WithValidTempleId_ShouldReturnTotal()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DonationService(context);

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

        var donation1 = new Donation 
        { 
            TempleId = temple.Id,
            DonorName = "John Doe",
            Amount = 100.00m,
            DonationType = "Cash",
            Purpose = "Temple Maintenance",
            Status = "Completed",
            DonationDate = DateTime.UtcNow.AddDays(-1),
            ReceiptNumber = "R001",
            Notes = "Monthly donation",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var donation2 = new Donation 
        { 
            TempleId = temple.Id,
            DonorName = "Jane Smith",
            Amount = 250.00m,
            DonationType = "Bank Transfer",
            Purpose = "New Equipment",
            Status = "Completed",
            DonationDate = DateTime.UtcNow.AddDays(-2),
            ReceiptNumber = "R002",
            Notes = "Equipment donation",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var pendingDonation = new Donation 
        { 
            TempleId = temple.Id,
            DonorName = "Bob Johnson",
            Amount = 75.00m,
            DonationType = "Cash",
            Purpose = "General Fund",
            Status = "Pending",
            DonationDate = DateTime.UtcNow.AddDays(-3),
            ReceiptNumber = "R003",
            Notes = "General donation",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Donations.AddRange(donation1, donation2, pendingDonation);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetTotalDonationsByTempleAsync(temple.Id);

        // Assert
        result.Should().Be(350.00m); // Only completed donations (100 + 250)
    }
}
