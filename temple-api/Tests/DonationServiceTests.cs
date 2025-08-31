using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using TempleApi.Data;
using TempleApi.Models;
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

        // Create temple first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var donation1 = new Donation { DonorName = "John Doe", Amount = 100.00m, DonationType = "Cash", TempleId = temple.Id };
        var donation2 = new Donation { DonorName = "Jane Smith", Amount = 250.00m, DonationType = "Online", TempleId = temple.Id };

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

        // Create temple first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var donation = new Donation { DonorName = "John Doe", Amount = 100.00m, DonationType = "Cash", TempleId = temple.Id };
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
    public async Task CreateDonationAsync_WithValidDto_ShouldCreateDonation()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DonationService(context);

        // Create temple and devotee first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        var devotee = new Devotee { FirstName = "John", LastName = "Doe", Email = "john@example.com", TempleId = temple.Id };
        context.Temples.Add(temple);
        context.Devotees.Add(devotee);
        await context.SaveChangesAsync();

        var createDto = new CreateDonationDto
        {
            TempleId = temple.Id,
            DevoteeId = devotee.Id,
            DonorName = "John Doe",
            Amount = 100.00m,
            DonationType = "Cash"
        };

        // Act
        var result = await service.CreateDonationAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.DonorName.Should().Be("John Doe");
        result.Amount.Should().Be(100.00m);
        result.DonationType.Should().Be("Cash");
        result.TempleId.Should().Be(temple.Id);
        result.DevoteeId.Should().Be(devotee.Id);
        result.Status.Should().Be("Pending");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify it was saved to database
        var savedDonation = await context.Donations.FindAsync(result.Id);
        savedDonation.Should().NotBeNull();
        savedDonation!.DonorName.Should().Be("John Doe");
    }

    [Fact]
    public async Task UpdateDonationStatusAsync_WithValidId_ShouldUpdateStatus()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DonationService(context);

        // Create temple first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var donation = new Donation { DonorName = "John Doe", Amount = 100.00m, DonationType = "Cash", TempleId = temple.Id, Status = "Pending" };
        context.Donations.Add(donation);
        await context.SaveChangesAsync();

        // Act
        var result = await service.UpdateDonationStatusAsync(donation.Id, "Completed");

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Completed");
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
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
    public async Task DeleteDonationAsync_WithValidId_ShouldDeleteDonation()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DonationService(context);

        // Create temple first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var donation = new Donation { DonorName = "John Doe", Amount = 100.00m, DonationType = "Cash", TempleId = temple.Id };
        context.Donations.Add(donation);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteDonationAsync(donation.Id);

        // Assert
        result.Should().BeTrue();

        // Verify it was deleted from database
        var deletedDonation = await context.Donations.FindAsync(donation.Id);
        deletedDonation.Should().BeNull();
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
    public async Task GetDonationsByTempleAsync_WithValidTempleId_ShouldReturnDonations()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DonationService(context);

        // Create temples
        var temple1 = new Temple { Name = "Temple 1", Address = "Address 1" };
        var temple2 = new Temple { Name = "Temple 2", Address = "Address 2" };
        context.Temples.AddRange(temple1, temple2);
        await context.SaveChangesAsync();

        var donation1 = new Donation { DonorName = "John Doe", Amount = 100.00m, DonationType = "Cash", TempleId = temple1.Id };
        var donation2 = new Donation { DonorName = "Jane Smith", Amount = 250.00m, DonationType = "Online", TempleId = temple1.Id };
        var donation3 = new Donation { DonorName = "Bob Johnson", Amount = 150.00m, DonationType = "Check", TempleId = temple2.Id };

        context.Donations.AddRange(donation1, donation2, donation3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetDonationsByTempleAsync(temple1.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.DonorName == "John Doe");
        result.Should().Contain(d => d.DonorName == "Jane Smith");
        result.Should().NotContain(d => d.DonorName == "Bob Johnson");
    }

    [Fact]
    public async Task GetDonationsByDevoteeAsync_WithValidDevoteeId_ShouldReturnDonations()
    {
        // Arrange
        using var context = CreateContext();
        var service = new DonationService(context);

        // Create temple and devotees
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        var devotee1 = new Devotee { FirstName = "John", LastName = "Doe", TempleId = temple.Id };
        var devotee2 = new Devotee { FirstName = "Jane", LastName = "Smith", TempleId = temple.Id };
        context.Temples.Add(temple);
        context.Devotees.AddRange(devotee1, devotee2);
        await context.SaveChangesAsync();

        var donation1 = new Donation { DonorName = "John Doe", Amount = 100.00m, DonationType = "Cash", TempleId = temple.Id, DevoteeId = devotee1.Id };
        var donation2 = new Donation { DonorName = "John Doe", Amount = 200.00m, DonationType = "Online", TempleId = temple.Id, DevoteeId = devotee1.Id };
        var donation3 = new Donation { DonorName = "Jane Smith", Amount = 150.00m, DonationType = "Check", TempleId = temple.Id, DevoteeId = devotee2.Id };

        context.Donations.AddRange(donation1, donation2, donation3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetDonationsByDevoteeAsync(devotee1.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.DonorName == "John Doe" && d.Amount == 100.00m);
        result.Should().Contain(d => d.DonorName == "John Doe" && d.Amount == 200.00m);
        result.Should().NotContain(d => d.DonorName == "Jane Smith");
    }
}
