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
using TempleApi.Repositories;
using TempleApi.Services;
using Xunit;

namespace TempleApi.Tests
{
    public class ContributionServiceTests : IDisposable
    {
        private readonly TempleDbContext _context;
        private readonly ContributionService _service;
        private readonly Mock<ILogger<ContributionService>> _mockLogger;

        public ContributionServiceTests()
        {
            var options = new DbContextOptionsBuilder<TempleDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TempleDbContext(options);
            _mockLogger = new Mock<ILogger<ContributionService>>();

            var contributionRepository = new ContributionRepository(_context);
            var eventRepository = new Repository<Event>(_context);
            var devoteeRepository = new Repository<Devotee>(_context);
            var contributionSettingRepository = new Repository<ContributionSetting>(_context);

            _service = new ContributionService(
                contributionRepository, 
                eventRepository, 
                devoteeRepository, 
                contributionSettingRepository, 
                _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        private async Task<(Temple temple, EventType eventType, Area area, Event testEvent, Devotee devotee, ContributionSetting contributionSetting)> SetupTestDataAsync()
        {
            var temple = new Temple { Name = "Test Temple", Address = "Test Address", City = "Test City", State = "Test State", IsActive = true };
            var eventType = new EventType { Name = "Test Type", IsActive = true };
            _context.Temples.Add(temple);
            _context.EventTypes.Add(eventType);
            await _context.SaveChangesAsync();
            
            var area = new Area { TempleId = temple.Id, Name = "Test Area", IsActive = true };
            _context.Areas.Add(area);
            await _context.SaveChangesAsync();
            
            var testEvent = new Event 
            { 
                Name = "Test Event", 
                AreaId = area.Id,
                EventTypeId = eventType.Id,
                StartDate = DateTime.UtcNow.AddDays(1), 
                EndDate = DateTime.UtcNow.AddDays(2), 
                Description = "Test Description",
                IsActive = true 
            };

            var devotee = new Devotee
            {
                FullName = "Test Devotee",
                Email = "test@example.com",
                Phone = "1234567890",
                Address = "Test Address",
                City = "Test City",
                State = "Test State",
                PostalCode = "12345",
                Gender = "Male",
                UserId = 0,
                IsActive = true
            };

            var contributionSetting = new ContributionSetting
            {
                Name = "Test Contribution Setting",
                Description = "Test Description",
                EventId = testEvent.Id,
                ContributionType = "Single",
                Amount = 100.00m,
                IsActive = true
            };

            _context.Events.Add(testEvent);
            _context.Devotees.Add(devotee);
            await _context.SaveChangesAsync();

            contributionSetting.EventId = testEvent.Id;
            _context.ContributionSettings.Add(contributionSetting);
            await _context.SaveChangesAsync();

            return (temple, eventType, area, testEvent, devotee, contributionSetting);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateContribution_WhenValidData()
        {
            // Arrange
            var (temple, eventType, area, testEvent, devotee, contributionSetting) = await SetupTestDataAsync();

            var createDto = new CreateContributionDto
            {
                EventId = testEvent.Id,
                DevoteeId = devotee.Id,
                ContributionSettingId = contributionSetting.Id,
                Amount = 100.00m,
                ContributionDate = DateTime.UtcNow,
                PaymentMethod = "Cash",
                Notes = "Test contribution"
            };

            // Act
            var result = await _service.CreateAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.EventId.Should().Be(testEvent.Id);
            result.DevoteeId.Should().Be(devotee.Id);
            result.ContributionSettingId.Should().Be(contributionSetting.Id);
            result.Amount.Should().Be(100.00m);
            result.PaymentMethod.Should().Be("Cash");
            result.Notes.Should().Be("Test contribution");
            result.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenEventNotFound()
        {
            // Arrange
            var (temple, eventType, area, testEvent, devotee, contributionSetting) = await SetupTestDataAsync();

            var createDto = new CreateContributionDto
            {
                EventId = 999, // Non-existent event
                DevoteeId = devotee.Id,
                ContributionSettingId = contributionSetting.Id,
                Amount = 100.00m
            };

            // Act & Assert
            var act = async () => await _service.CreateAsync(createDto);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Event with ID 999 not found or inactive");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenDevoteeNotFound()
        {
            // Arrange
            var (temple, eventType, area, testEvent, devotee, contributionSetting) = await SetupTestDataAsync();

            var createDto = new CreateContributionDto
            {
                EventId = testEvent.Id,
                DevoteeId = 999, // Non-existent devotee
                ContributionSettingId = contributionSetting.Id,
                Amount = 100.00m
            };

            // Act & Assert
            var act = async () => await _service.CreateAsync(createDto);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Devotee with ID 999 not found or inactive");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenContributionSettingNotFound()
        {
            // Arrange
            var (temple, eventType, area, testEvent, devotee, contributionSetting) = await SetupTestDataAsync();

            var createDto = new CreateContributionDto
            {
                EventId = testEvent.Id,
                DevoteeId = devotee.Id,
                ContributionSettingId = 999, // Non-existent contribution setting
                Amount = 100.00m
            };

            // Act & Assert
            var act = async () => await _service.CreateAsync(createDto);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Contribution setting with ID 999 not found or inactive");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenContributionSettingDoesNotBelongToEvent()
        {
            // Arrange
            var (temple, eventType, area, testEvent, devotee, contributionSetting) = await SetupTestDataAsync();

            // Create another event
            var anotherEvent = new Event 
            { 
                Name = "Another Event", 
                AreaId = area.Id,
                EventTypeId = eventType.Id,
                StartDate = DateTime.UtcNow.AddDays(3), 
                EndDate = DateTime.UtcNow.AddDays(4), 
                Description = "Another Description",
                IsActive = true 
            };
            _context.Events.Add(anotherEvent);
            await _context.SaveChangesAsync();

            var createDto = new CreateContributionDto
            {
                EventId = anotherEvent.Id, // Different event
                DevoteeId = devotee.Id,
                ContributionSettingId = contributionSetting.Id, // Belongs to testEvent
                Amount = 100.00m
            };

            // Act & Assert
            var act = async () => await _service.CreateAsync(createDto);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Selected contribution setting does not belong to the selected event");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenAmountIsZeroOrNegative()
        {
            // Arrange
            var (temple, eventType, area, testEvent, devotee, contributionSetting) = await SetupTestDataAsync();

            var createDto = new CreateContributionDto
            {
                EventId = testEvent.Id,
                DevoteeId = devotee.Id,
                ContributionSettingId = contributionSetting.Id,
                Amount = 0 // Invalid amount
            };

            // Act & Assert
            var act = async () => await _service.CreateAsync(createDto);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Contribution amount must be greater than zero");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllActiveContributions()
        {
            // Arrange
            var (temple, eventType, area, testEvent, devotee, contributionSetting) = await SetupTestDataAsync();

            var contribution1 = new Contribution
            {
                EventId = testEvent.Id,
                DevoteeId = devotee.Id,
                ContributionSettingId = contributionSetting.Id,
                Amount = 100.00m,
                ContributionDate = DateTime.UtcNow,
                PaymentMethod = "Cash",
                IsActive = true
            };

            var contribution2 = new Contribution
            {
                EventId = testEvent.Id,
                DevoteeId = devotee.Id,
                ContributionSettingId = contributionSetting.Id,
                Amount = 200.00m,
                ContributionDate = DateTime.UtcNow,
                PaymentMethod = "Card",
                IsActive = false // Inactive
            };

            _context.Contributions.AddRange(contribution1, contribution2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().HaveCount(1); // Only active contributions
            result.First().Amount.Should().Be(100.00m);
        }

        [Fact]
        public async Task GetByEventIdAsync_ShouldReturnContributionsForSpecificEvent()
        {
            // Arrange
            var (temple, eventType, area, testEvent, devotee, contributionSetting) = await SetupTestDataAsync();

            // Create another event
            var anotherEvent = new Event 
            { 
                Name = "Another Event", 
                AreaId = area.Id,
                EventTypeId = eventType.Id,
                StartDate = DateTime.UtcNow.AddDays(3), 
                EndDate = DateTime.UtcNow.AddDays(4), 
                Description = "Another Description",
                IsActive = true 
            };
            _context.Events.Add(anotherEvent);
            await _context.SaveChangesAsync();

            var contribution1 = new Contribution
            {
                EventId = testEvent.Id,
                DevoteeId = devotee.Id,
                ContributionSettingId = contributionSetting.Id,
                Amount = 100.00m,
                ContributionDate = DateTime.UtcNow,
                PaymentMethod = "Cash",
                IsActive = true
            };

            var contribution2 = new Contribution
            {
                EventId = anotherEvent.Id,
                DevoteeId = devotee.Id,
                ContributionSettingId = contributionSetting.Id,
                Amount = 200.00m,
                ContributionDate = DateTime.UtcNow,
                PaymentMethod = "Card",
                IsActive = true
            };

            _context.Contributions.AddRange(contribution1, contribution2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetByEventIdAsync(testEvent.Id);

            // Assert
            result.Should().HaveCount(1);
            result.First().EventId.Should().Be(testEvent.Id);
            result.First().Amount.Should().Be(100.00m);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateContribution_WhenValidData()
        {
            // Arrange
            var (temple, eventType, area, testEvent, devotee, contributionSetting) = await SetupTestDataAsync();

            var contribution = new Contribution
            {
                EventId = testEvent.Id,
                DevoteeId = devotee.Id,
                ContributionSettingId = contributionSetting.Id,
                Amount = 100.00m,
                ContributionDate = DateTime.UtcNow,
                PaymentMethod = "Cash",
                Notes = "Original notes",
                IsActive = true
            };

            _context.Contributions.Add(contribution);
            await _context.SaveChangesAsync();

            var updateDto = new CreateContributionDto
            {
                EventId = testEvent.Id,
                DevoteeId = devotee.Id,
                ContributionSettingId = contributionSetting.Id,
                Amount = 150.00m,
                ContributionDate = DateTime.UtcNow,
                PaymentMethod = "Card",
                Notes = "Updated notes"
            };

            // Act
            var result = await _service.UpdateAsync(contribution.Id, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Amount.Should().Be(150.00m);
            result.PaymentMethod.Should().Be("Card");
            result.Notes.Should().Be("Updated notes");
        }

        [Fact]
        public async Task DeleteAsync_ShouldSoftDeleteContribution_WhenExists()
        {
            // Arrange
            var (temple, eventType, area, testEvent, devotee, contributionSetting) = await SetupTestDataAsync();

            var contribution = new Contribution
            {
                EventId = testEvent.Id,
                DevoteeId = devotee.Id,
                ContributionSettingId = contributionSetting.Id,
                Amount = 100.00m,
                ContributionDate = DateTime.UtcNow,
                PaymentMethod = "Cash",
                IsActive = true
            };

            _context.Contributions.Add(contribution);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteAsync(contribution.Id);

            // Assert
            result.Should().BeTrue();
            
            var deletedContribution = await _context.Contributions.FindAsync(contribution.Id);
            deletedContribution.Should().NotBeNull();
            deletedContribution.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
        {
            // Act
            var result = await _service.DeleteAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetTotalContributionsByEventAsync_ShouldReturnCorrectTotal()
        {
            // Arrange
            var (temple, eventType, area, testEvent, devotee, contributionSetting) = await SetupTestDataAsync();

            var contribution1 = new Contribution
            {
                EventId = testEvent.Id,
                DevoteeId = devotee.Id,
                ContributionSettingId = contributionSetting.Id,
                Amount = 100.00m,
                ContributionDate = DateTime.UtcNow,
                PaymentMethod = "Cash",
                IsActive = true
            };

            var contribution2 = new Contribution
            {
                EventId = testEvent.Id,
                DevoteeId = devotee.Id,
                ContributionSettingId = contributionSetting.Id,
                Amount = 200.00m,
                ContributionDate = DateTime.UtcNow,
                PaymentMethod = "Card",
                IsActive = true
            };

            var contribution3 = new Contribution
            {
                EventId = testEvent.Id,
                DevoteeId = devotee.Id,
                ContributionSettingId = contributionSetting.Id,
                Amount = 50.00m,
                ContributionDate = DateTime.UtcNow,
                PaymentMethod = "Online",
                IsActive = false // Inactive, should not be counted
            };

            _context.Contributions.AddRange(contribution1, contribution2, contribution3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetTotalContributionsByEventAsync(testEvent.Id);

            // Assert
            result.Should().Be(300.00m); // Only active contributions
        }

        [Fact]
        public async Task GetContributionSummaryByEventAsync_ShouldReturnCorrectSummary()
        {
            // Arrange
            var (temple, eventType, area, testEvent, devotee, contributionSetting) = await SetupTestDataAsync();

            var contribution1 = new Contribution
            {
                EventId = testEvent.Id,
                DevoteeId = devotee.Id,
                ContributionSettingId = contributionSetting.Id,
                Amount = 100.00m,
                ContributionDate = DateTime.UtcNow.AddDays(-1),
                PaymentMethod = "Cash",
                IsActive = true
            };

            var contribution2 = new Contribution
            {
                EventId = testEvent.Id,
                DevoteeId = devotee.Id,
                ContributionSettingId = contributionSetting.Id,
                Amount = 200.00m,
                ContributionDate = DateTime.UtcNow,
                PaymentMethod = "Card",
                IsActive = true
            };

            _context.Contributions.AddRange(contribution1, contribution2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetContributionSummaryByEventAsync();

            // Assert
            result.Should().HaveCount(1);
            var summary = result.First();
            summary.EventId.Should().Be(testEvent.Id);
            summary.TotalAmount.Should().Be(300.00m);
            summary.ContributionCount.Should().Be(2);
        }
    }
}
