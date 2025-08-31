using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using TempleApi.Data;
using TempleApi.Models;
using TempleApi.Models.DTOs;
using TempleApi.Services;
using Xunit;

namespace TempleApi.Tests;

public class EventServiceTests
{
    private readonly DbContextOptions<TempleDbContext> _options;

    public EventServiceTests()
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
    public async Task GetAllEventsAsync_ShouldReturnAllEvents()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

        // Create temple first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var event1 = new Event { Name = "Event 1", Description = "Description 1", StartDate = DateTime.UtcNow.AddDays(1), TempleId = temple.Id };
        var event2 = new Event { Name = "Event 2", Description = "Description 2", StartDate = DateTime.UtcNow.AddDays(2), TempleId = temple.Id };

        context.Events.AddRange(event1, event2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllEventsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(e => e.Name == "Event 1");
        result.Should().Contain(e => e.Name == "Event 2");
    }

    [Fact]
    public async Task GetEventByIdAsync_WithValidId_ShouldReturnEvent()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

        // Create temple first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var eventEntity = new Event { Name = "Test Event", Description = "Test Description", StartDate = DateTime.UtcNow.AddDays(1), TempleId = temple.Id };
        context.Events.Add(eventEntity);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetEventByIdAsync(eventEntity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Event");
        result.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task GetEventByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

        // Act
        var result = await service.GetEventByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateEventAsync_WithValidDto_ShouldCreateEvent()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var createDto = new CreateEventDto
        {
            TempleId = temple.Id,
            Name = "New Event",
            Description = "New Description",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            EventType = "Puja"
        };

        // Act
        var result = await service.CreateEventAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Event");
        result.Description.Should().Be("New Description");
        result.TempleId.Should().Be(temple.Id);
        result.Status.Should().Be("Scheduled");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify it was saved to database
        var savedEvent = await context.Events.FindAsync(result.Id);
        savedEvent.Should().NotBeNull();
        savedEvent!.Name.Should().Be("New Event");
    }

    [Fact]
    public async Task UpdateEventAsync_WithValidId_ShouldUpdateEvent()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

        // Create temple first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var eventEntity = new Event { Name = "Old Name", Description = "Old Description", StartDate = DateTime.UtcNow.AddDays(1), TempleId = temple.Id };
        context.Events.Add(eventEntity);
        await context.SaveChangesAsync();

        var updateDto = new CreateEventDto
        {
            TempleId = temple.Id,
            Name = "Updated Name",
            Description = "Updated Description",
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(3),
            EventType = "Updated Puja"
        };

        // Act
        var result = await service.UpdateEventAsync(eventEntity.Id, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateEventAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

        // Create temple first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var updateDto = new CreateEventDto
        {
            TempleId = temple.Id,
            Name = "Updated Name",
            Description = "Updated Description",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            EventType = "Updated Puja"
        };

        // Act
        var result = await service.UpdateEventAsync(999, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteEventAsync_WithValidId_ShouldSoftDeleteEvent()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

        // Create temple first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var eventEntity = new Event { Name = "Test Event", Description = "Test Description", StartDate = DateTime.UtcNow.AddDays(1), TempleId = temple.Id };
        context.Events.Add(eventEntity);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteEventAsync(eventEntity.Id);

        // Assert
        result.Should().BeTrue();

        // Verify soft delete
        var deletedEvent = await context.Events.FindAsync(eventEntity.Id);
        deletedEvent.Should().NotBeNull();
        deletedEvent!.IsActive.Should().BeFalse();
        deletedEvent.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DeleteEventAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

        // Act
        var result = await service.DeleteEventAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateEventStatusAsync_WithValidId_ShouldUpdateStatus()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

        // Create temple first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var eventEntity = new Event { Name = "Test Event", Description = "Test Description", StartDate = DateTime.UtcNow.AddDays(1), TempleId = temple.Id, Status = "Scheduled" };
        context.Events.Add(eventEntity);
        await context.SaveChangesAsync();

        // Act
        var result = await service.UpdateEventStatusAsync(eventEntity.Id, "In Progress");

        // Assert
        result.Should().BeTrue();

        // Verify status was updated
        var updatedEvent = await context.Events.FindAsync(eventEntity.Id);
        updatedEvent.Should().NotBeNull();
        updatedEvent!.Status.Should().Be("In Progress");
        updatedEvent.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateEventStatusAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

        // Act
        var result = await service.UpdateEventStatusAsync(999, "In Progress");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetEventsByTempleAsync_WithValidTempleId_ShouldReturnEvents()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

        // Create temples
        var temple1 = new Temple { Name = "Temple 1", Address = "Address 1" };
        var temple2 = new Temple { Name = "Temple 2", Address = "Address 2" };
        context.Temples.AddRange(temple1, temple2);
        await context.SaveChangesAsync();

        var event1 = new Event { Name = "Event 1", StartDate = DateTime.UtcNow.AddDays(1), TempleId = temple1.Id };
        var event2 = new Event { Name = "Event 2", StartDate = DateTime.UtcNow.AddDays(2), TempleId = temple1.Id };
        var event3 = new Event { Name = "Event 3", StartDate = DateTime.UtcNow.AddDays(3), TempleId = temple2.Id };

        context.Events.AddRange(event1, event2, event3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetEventsByTempleAsync(temple1.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(e => e.Name == "Event 1");
        result.Should().Contain(e => e.Name == "Event 2");
        result.Should().NotContain(e => e.Name == "Event 3");
    }

    [Fact]
    public async Task GetUpcomingEventsAsync_WithValidTempleId_ShouldReturnUpcomingEvents()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

        // Create temple first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var pastEvent = new Event { Name = "Past Event", StartDate = DateTime.UtcNow.AddDays(-1), TempleId = temple.Id, Status = "Scheduled" };
        var upcomingEvent1 = new Event { Name = "Upcoming Event 1", StartDate = DateTime.UtcNow.AddDays(1), TempleId = temple.Id, Status = "Scheduled" };
        var upcomingEvent2 = new Event { Name = "Upcoming Event 2", StartDate = DateTime.UtcNow.AddDays(2), TempleId = temple.Id, Status = "Scheduled" };
        var cancelledEvent = new Event { Name = "Cancelled Event", StartDate = DateTime.UtcNow.AddDays(1), TempleId = temple.Id, Status = "Cancelled" };

        context.Events.AddRange(pastEvent, upcomingEvent1, upcomingEvent2, cancelledEvent);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUpcomingEventsAsync(temple.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(e => e.Name == "Upcoming Event 1");
        result.Should().Contain(e => e.Name == "Upcoming Event 2");
        result.Should().NotContain(e => e.Name == "Past Event");
        result.Should().NotContain(e => e.Name == "Cancelled Event");
    }

    [Fact]
    public async Task SearchEventsAsync_WithValidTerm_ShouldReturnMatchingEvents()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

        // Create temple first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var event1 = new Event { Name = "Hindu Puja", Description = "Traditional Hindu ceremony", StartDate = DateTime.UtcNow.AddDays(1), TempleId = temple.Id };
        var event2 = new Event { Name = "Buddhist Meditation", Description = "Peaceful meditation session", StartDate = DateTime.UtcNow.AddDays(2), TempleId = temple.Id };
        var event3 = new Event { Name = "Christian Service", Description = "Sunday service", StartDate = DateTime.UtcNow.AddDays(3), TempleId = temple.Id };

        context.Events.AddRange(event1, event2, event3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchEventsAsync("Puja");

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(e => e.Name == "Hindu Puja");
    }

    [Fact]
    public async Task SearchEventsAsync_WithInvalidTerm_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

        // Create temple first
        var temple = new Temple { Name = "Test Temple", Address = "Test Address" };
        context.Temples.Add(temple);
        await context.SaveChangesAsync();

        var eventEntity = new Event { Name = "Test Event", Description = "Test Description", StartDate = DateTime.UtcNow.AddDays(1), TempleId = temple.Id };
        context.Events.Add(eventEntity);
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchEventsAsync("Nonexistent");

        // Assert
        result.Should().BeEmpty();
    }
}
