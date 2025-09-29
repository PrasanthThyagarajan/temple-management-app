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
    public class ContributionSettingServiceTests : IDisposable
    {
        private readonly TempleDbContext _context;
        private readonly ContributionSettingService _service;
        private readonly Mock<ILogger<ContributionSettingService>> _mockLogger;

        public ContributionSettingServiceTests()
        {
            var options = new DbContextOptionsBuilder<TempleDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TempleDbContext(options);
            _mockLogger = new Mock<ILogger<ContributionSettingService>>();

            var contributionRepository = new ContributionSettingRepository(_context);
            var eventRepository = new Repository<Event>(_context);

            _service = new ContributionSettingService(contributionRepository, eventRepository, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateContributionSetting_WhenValidData()
        {
            // Arrange
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

            _context.Events.Add(testEvent);
            await _context.SaveChangesAsync();

            var createDto = new CreateContributionSettingDto
            {
                Name = "Test Contribution",
                Description = "Test Description",
                EventId = testEvent.Id,
                ContributionType = "Single",
                Amount = 100.50m
            };

            // Act
            var result = await _service.CreateAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Test Contribution");
            result.Description.Should().Be("Test Description");
            result.EventId.Should().Be(testEvent.Id);
            result.ContributionType.Should().Be("Single");
            result.Amount.Should().Be(100.50m);
            result.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateRecurringContribution_WhenValidRecurringData()
        {
            // Arrange
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

            _context.Events.Add(testEvent);
            await _context.SaveChangesAsync();

            var createDto = new CreateContributionSettingDto
            {
                Name = "Monthly Contribution",
                Description = "Monthly recurring contribution",
                EventId = testEvent.Id,
                ContributionType = "Recurring",
                Amount = 500.00m,
                RecurringDay = 15,
                RecurringFrequency = "Monthly"
            };

            // Act
            var result = await _service.CreateAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Monthly Contribution");
            result.ContributionType.Should().Be("Recurring");
            result.Amount.Should().Be(500.00m);
            result.RecurringDay.Should().Be(15);
            result.RecurringFrequency.Should().Be("Monthly");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenEventNotFound()
        {
            // Arrange
            var createDto = new CreateContributionSettingDto
            {
                Name = "Test Contribution",
                EventId = 999, // Non-existent event
                ContributionType = "Single",
                Amount = 100.00m
            };

            // Act & Assert
            var act = async () => await _service.CreateAsync(createDto);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Event with ID 999 not found");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenInvalidContributionType()
        {
            // Arrange
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

            _context.Events.Add(testEvent);
            await _context.SaveChangesAsync();

            var createDto = new CreateContributionSettingDto
            {
                Name = "Test Contribution",
                EventId = testEvent.Id,
                ContributionType = "Invalid", // Invalid type
                Amount = 100.00m
            };

            // Act & Assert
            var act = async () => await _service.CreateAsync(createDto);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Contribution type must be 'Single' or 'Recurring'");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenRecurringWithoutDay()
        {
            // Arrange
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

            _context.Events.Add(testEvent);
            await _context.SaveChangesAsync();

            var createDto = new CreateContributionSettingDto
            {
                Name = "Test Contribution",
                EventId = testEvent.Id,
                ContributionType = "Recurring",
                Amount = 100.00m
                // Missing RecurringDay and RecurringFrequency
            };

            // Act & Assert
            var act = async () => await _service.CreateAsync(createDto);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Recurring contributions must have a valid day (1-31)");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllContributions()
        {
            // Arrange
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

            var contribution1 = new ContributionSetting
            {
                Name = "Contribution 1",
                EventId = testEvent.Id,
                ContributionType = "Single",
                Amount = 100.00m,
                IsActive = true
            };

            var contribution2 = new ContributionSetting
            {
                Name = "Contribution 2",
                EventId = testEvent.Id,
                ContributionType = "Recurring",
                Amount = 200.00m,
                RecurringDay = 15,
                RecurringFrequency = "Monthly",
                IsActive = true
            };

            _context.Events.Add(testEvent);
            _context.ContributionSettings.AddRange(contribution1, contribution2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(c => c.Name == "Contribution 1");
            result.Should().Contain(c => c.Name == "Contribution 2");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnContribution_WhenExists()
        {
            // Arrange
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

            var contribution = new ContributionSetting
            {
                Name = "Test Contribution",
                EventId = testEvent.Id,
                ContributionType = "Single",
                Amount = 100.00m,
                IsActive = true
            };

            _context.Events.Add(testEvent);
            _context.ContributionSettings.Add(contribution);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetByIdAsync(contribution.Id);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Test Contribution");
            result.Id.Should().Be(contribution.Id);
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
        public async Task UpdateAsync_ShouldUpdateContribution_WhenValidData()
        {
            // Arrange
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

            var contribution = new ContributionSetting
            {
                Name = "Original Name",
                EventId = testEvent.Id,
                ContributionType = "Single",
                Amount = 100.00m,
                IsActive = true
            };

            _context.Events.Add(testEvent);
            _context.ContributionSettings.Add(contribution);
            await _context.SaveChangesAsync();

            var updateDto = new CreateContributionSettingDto
            {
                Name = "Updated Name",
                Description = "Updated Description",
                EventId = testEvent.Id,
                ContributionType = "Single",
                Amount = 150.00m
            };

            // Act
            var result = await _service.UpdateAsync(contribution.Id, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Updated Name");
            result.Description.Should().Be("Updated Description");
            result.Amount.Should().Be(150.00m);
        }

        [Fact]
        public async Task DeleteAsync_ShouldSoftDeleteContribution_WhenExists()
        {
            // Arrange
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

            var contribution = new ContributionSetting
            {
                Name = "Test Contribution",
                EventId = testEvent.Id,
                ContributionType = "Single",
                Amount = 100.00m,
                IsActive = true
            };

            _context.Events.Add(testEvent);
            _context.ContributionSettings.Add(contribution);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteAsync(contribution.Id);

            // Assert
            result.Should().BeTrue();
            
            var deletedContribution = await _context.ContributionSettings.FindAsync(contribution.Id);
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
        public async Task GetActiveContributionsAsync_ShouldReturnOnlyActiveContributions()
        {
            // Arrange
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

            var activeContribution = new ContributionSetting
            {
                Name = "Active Contribution",
                EventId = testEvent.Id,
                ContributionType = "Single",
                Amount = 100.00m,
                IsActive = true
            };

            var inactiveContribution = new ContributionSetting
            {
                Name = "Inactive Contribution",
                EventId = testEvent.Id,
                ContributionType = "Single",
                Amount = 200.00m,
                IsActive = false
            };

            _context.Events.Add(testEvent);
            _context.ContributionSettings.AddRange(activeContribution, inactiveContribution);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetActiveContributionsAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Active Contribution");
        }
    }
}
