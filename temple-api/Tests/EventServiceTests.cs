using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using TempleApi.Data;
using TempleApi.Domain.Entities;
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

        var event1 = new Event 
        { 
            Name = "Test Event 1", 
            Description = "Test Description 1",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            Location = "Test Location 1",
            EventType = "Puja",
            MaxAttendees = 100,
            EntryFee = 50.00m,
            Status = "Scheduled",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var event2 = new Event 
        { 
            Name = "Test Event 2", 
            Description = "Test Description 2",
            StartDate = DateTime.UtcNow.AddDays(3),
            EndDate = DateTime.UtcNow.AddDays(4),
            Location = "Test Location 2",
            EventType = "Bhajan",
            MaxAttendees = 50,
            EntryFee = 25.00m,
            Status = "Scheduled",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Events.AddRange(event1, event2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllEventsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(e => e.Name == "Test Event 1");
        result.Should().Contain(e => e.Name == "Test Event 2");
    }

    [Fact]
    public async Task GetEventByIdAsync_WithValidId_ShouldReturnEvent()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

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

        var eventEntity = new Event 
        { 
            Name = "Test Event", 
            Description = "Test Description",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            Location = "Test Location",
            EventType = "Puja",
            MaxAttendees = 100,
            EntryFee = 50.00m,
            Status = "Scheduled",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Events.Add(eventEntity);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetEventByIdAsync(eventEntity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Event");
        result.Description.Should().Be("Test Description");
        result.EventType.Should().Be("Puja");
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
    public async Task GetEventsByTempleAsync_WithValidTempleId_ShouldReturnEvents()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

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

        var event1 = new Event 
        { 
            Name = "Event 1", 
            Description = "Description 1",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            Location = "Location 1",
            EventType = "Puja",
            MaxAttendees = 100,
            EntryFee = 50.00m,
            Status = "Scheduled",
            TempleId = temple1.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var event2 = new Event 
        { 
            Name = "Event 2", 
            Description = "Description 2",
            StartDate = DateTime.UtcNow.AddDays(3),
            EndDate = DateTime.UtcNow.AddDays(4),
            Location = "Location 2",
            EventType = "Bhajan",
            MaxAttendees = 50,
            EntryFee = 25.00m,
            Status = "Scheduled",
            TempleId = temple2.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Events.AddRange(event1, event2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetEventsByTempleAsync(temple1.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().TempleId.Should().Be(temple1.Id);
    }

    [Fact]
    public async Task GetUpcomingEventsAsync_WithValidTempleId_ShouldReturnUpcomingEvents()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

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

        var pastEvent = new Event 
        { 
            Name = "Past Event", 
            Description = "Past Description",
            StartDate = DateTime.UtcNow.AddDays(-2),
            EndDate = DateTime.UtcNow.AddDays(-1),
            Location = "Past Location",
            EventType = "Puja",
            MaxAttendees = 100,
            EntryFee = 50.00m,
            Status = "Scheduled",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var upcomingEvent = new Event 
        { 
            Name = "Upcoming Event", 
            Description = "Upcoming Description",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            Location = "Upcoming Location",
            EventType = "Bhajan",
            MaxAttendees = 50,
            EntryFee = 25.00m,
            Status = "Scheduled",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var cancelledEvent = new Event 
        { 
            Name = "Cancelled Event", 
            Description = "Cancelled Description",
            StartDate = DateTime.UtcNow.AddDays(3),
            EndDate = DateTime.UtcNow.AddDays(4),
            Location = "Cancelled Location",
            EventType = "Puja",
            MaxAttendees = 75,
            EntryFee = 35.00m,
            Status = "Cancelled",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Events.AddRange(pastEvent, upcomingEvent, cancelledEvent);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUpcomingEventsAsync(temple.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Upcoming Event");
    }

    [Fact]
    public async Task CreateEventAsync_WithValidDto_ShouldCreateEvent()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

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

        var createDto = new CreateEventDto
        {
            TempleId = temple.Id,
            Name = "New Event",
            Description = "New Description",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            Location = "New Location",
            EventType = "Puja",
            MaxAttendees = 100,
            RegistrationFee = 50.00m
        };

        // Act
        var result = await service.CreateEventAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Event");
        result.Description.Should().Be("New Description");
        result.Location.Should().Be("New Location");
        result.EventType.Should().Be("Puja");
        result.MaxAttendees.Should().Be(100);
        result.EntryFee.Should().Be(50.00m);
        result.Status.Should().Be("Scheduled");
        result.TempleId.Should().Be(temple.Id);
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateEventAsync_WithValidId_ShouldUpdateEvent()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

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

        var eventEntity = new Event 
        { 
            Name = "Original Event", 
            Description = "Original Description",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            Location = "Original Location",
            EventType = "Puja",
            MaxAttendees = 100,
            EntryFee = 50.00m,
            Status = "Scheduled",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Events.Add(eventEntity);
        await context.SaveChangesAsync();

        var updateDto = new CreateEventDto
        {
            TempleId = temple.Id,
            Name = "Updated Event",
            Description = "Updated Description",
            StartDate = DateTime.UtcNow.AddDays(3),
            EndDate = DateTime.UtcNow.AddDays(4),
            Location = "Updated Location",
            EventType = "Bhajan",
            MaxAttendees = 150,
            RegistrationFee = 75.00m
        };

        // Act
        var result = await service.UpdateEventAsync(eventEntity.Id, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Event");
        result.Description.Should().Be("Updated Description");
        result.Location.Should().Be("Updated Location");
        result.EventType.Should().Be("Bhajan");
        result.MaxAttendees.Should().Be(150);
        result.EntryFee.Should().Be(75.00m);
    }

    [Fact]
    public async Task UpdateEventAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

        var updateDto = new CreateEventDto
        {
            TempleId = 1,
            Name = "Updated Event",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            EventType = "Puja"
        };

        // Act
        var result = await service.UpdateEventAsync(999, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateEventStatusAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

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

        var eventEntity = new Event 
        { 
            Name = "Test Event", 
            Description = "Test Description",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            Location = "Test Location",
            EventType = "Puja",
            MaxAttendees = 100,
            EntryFee = 50.00m,
            Status = "Scheduled",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Events.Add(eventEntity);
        await context.SaveChangesAsync();

        // Act
        var result = await service.UpdateEventStatusAsync(eventEntity.Id, "Cancelled");

        // Assert
        result.Should().BeTrue();
        var updatedEvent = await context.Events.FindAsync(eventEntity.Id);
        updatedEvent!.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task UpdateEventStatusAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

        // Act
        var result = await service.UpdateEventStatusAsync(999, "Cancelled");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteEventAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

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

        var eventEntity = new Event 
        { 
            Name = "Test Event", 
            Description = "Test Description",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            Location = "Test Location",
            EventType = "Puja",
            MaxAttendees = 100,
            EntryFee = 50.00m,
            Status = "Scheduled",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Events.Add(eventEntity);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteEventAsync(eventEntity.Id);

        // Assert
        result.Should().BeTrue();
        var deletedEvent = await context.Events.FindAsync(eventEntity.Id);
        deletedEvent!.IsActive.Should().BeFalse();
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
    public async Task SearchEventsAsync_WithValidSearchTerm_ShouldReturnMatchingEvents()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

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

        var event1 = new Event 
        { 
            Name = "Hindu Puja", 
            Description = "Traditional Hindu ceremony",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            Location = "Main Hall",
            EventType = "Puja",
            MaxAttendees = 100,
            EntryFee = 50.00m,
            Status = "Scheduled",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var event2 = new Event 
        { 
            Name = "Bhajan Night", 
            Description = "Devotional singing",
            StartDate = DateTime.UtcNow.AddDays(3),
            EndDate = DateTime.UtcNow.AddDays(4),
            Location = "Garden",
            EventType = "Bhajan",
            MaxAttendees = 50,
            EntryFee = 25.00m,
            Status = "Scheduled",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Events.AddRange(event1, event2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchEventsAsync("Hindu");

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Hindu Puja");
    }

    [Fact]
    public async Task SearchEventsAsync_WithEmptySearchTerm_ShouldReturnAllEvents()
    {
        // Arrange
        using var context = CreateContext();
        var service = new EventService(context);

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

        var event1 = new Event 
        { 
            Name = "Test Event 1", 
            Description = "Test Description 1",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            Location = "Test Location 1",
            EventType = "Puja",
            MaxAttendees = 100,
            EntryFee = 50.00m,
            Status = "Scheduled",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var event2 = new Event 
        { 
            Name = "Test Event 2", 
            Description = "Test Description 2",
            StartDate = DateTime.UtcNow.AddDays(3),
            EndDate = DateTime.UtcNow.AddDays(4),
            Location = "Test Location 2",
            EventType = "Bhajan",
            MaxAttendees = 50,
            EntryFee = 25.00m,
            Status = "Scheduled",
            TempleId = temple.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Events.AddRange(event1, event2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchEventsAsync("");

        // Assert
        result.Should().HaveCount(2);
    }
}
