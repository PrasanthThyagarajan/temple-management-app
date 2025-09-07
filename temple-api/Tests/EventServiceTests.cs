using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services;
using Xunit;
using FluentAssertions;

namespace TempleApi.Tests
{
	public class EventServiceTests : IDisposable
	{
		private readonly TempleDbContext _context;
		private readonly EventService _eventService;

		public EventServiceTests()
		{
			var options = new DbContextOptionsBuilder<TempleDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			_context = new TempleDbContext(options);
			_eventService = new EventService(_context);
		}

		#region Create Tests

		[Fact]
		public async Task CreateEventAsync_ShouldCreateEvent_WhenValidData()
		{
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(temple);
			await _context.SaveChangesAsync();

			var dto = new CreateEventDto
			{
				TempleId = temple.Id,
				Name = "Event",
				Description = "Desc",
				EventType = "Festival",
				StartDate = DateTime.UtcNow.AddDays(1),
				EndDate = DateTime.UtcNow.AddDays(2),
				Location = "Hall",
				MaxAttendees = 100,
				RegistrationFee = 50
			};

			var result = await _eventService.CreateEventAsync(dto);

			result.Should().NotBeNull();
			result.TempleId.Should().Be(temple.Id);
			result.Name.Should().Be("Event");
			result.EventType.Should().Be("Festival");
			result.MaxAttendees.Should().Be(100);
			result.EntryFee.Should().Be(50);
		}

		#endregion

		#region Read Tests

		[Fact]
		public async Task GetEventByIdAsync_ShouldReturnEvent_WhenExists()
		{
			var t = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(t);
			await _context.SaveChangesAsync();
			var ev = new Event { TempleId = t.Id, Name = "E", Description = "D", EventType = "Festival", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(1), IsActive = true };
			_context.Events.Add(ev);
			await _context.SaveChangesAsync();

			var result = await _eventService.GetEventByIdAsync(ev.Id);
			result.Should().NotBeNull();
			result!.Name.Should().Be("E");
		}

		[Fact]
		public async Task GetAllEventsAsync_ShouldReturnOnlyActive()
		{
			var t = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(t);
			await _context.SaveChangesAsync();
			_context.Events.AddRange(
				new Event { TempleId = t.Id, Name = "E1", EventType = "Festival", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow, IsActive = true },
				new Event { TempleId = t.Id, Name = "E2", EventType = "Festival", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow, IsActive = false }
			);
			await _context.SaveChangesAsync();

			var result = await _eventService.GetAllEventsAsync();
			result.Should().HaveCount(1);
		}

		[Fact]
		public async Task GetEventsByTempleAsync_ShouldFilterByTemple()
		{
			var t1 = new Temple { Name = "T1", Address = "A1", City = "C1", State = "S1", IsActive = true };
			var t2 = new Temple { Name = "T2", Address = "A2", City = "C2", State = "S2", IsActive = true };
			_context.Temples.AddRange(t1, t2);
			await _context.SaveChangesAsync();
			_context.Events.AddRange(
				new Event { TempleId = t1.Id, Name = "E1", EventType = "Festival", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow, IsActive = true },
				new Event { TempleId = t2.Id, Name = "E2", EventType = "Festival", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow, IsActive = true }
			);
			await _context.SaveChangesAsync();

			var result = await _eventService.GetEventsByTempleAsync(t1.Id);
			result.Should().HaveCount(1);
		}

		[Fact]
		public async Task GetUpcomingEventsAsync_ShouldFilterByFutureAndScheduled()
		{
			var t = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(t);
			await _context.SaveChangesAsync();
			_context.Events.AddRange(
				new Event { TempleId = t.Id, Name = "Past", EventType = "F", StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow, Status = "Scheduled", IsActive = true },
				new Event { TempleId = t.Id, Name = "Future", EventType = "F", StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(2), Status = "Scheduled", IsActive = true }
			);
			await _context.SaveChangesAsync();

			var result = await _eventService.GetUpcomingEventsAsync(t.Id);
			result.Should().HaveCount(1);
		}

		#endregion

		#region Update/Delete

		[Fact]
		public async Task UpdateEventAsync_ShouldUpdate_WhenExists()
		{
			var t = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(t);
			await _context.SaveChangesAsync();
			var ev = new Event { TempleId = t.Id, Name = "E", Description = "D", EventType = "F", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(1), IsActive = true };
			_context.Events.Add(ev);
			await _context.SaveChangesAsync();

			var dto = new CreateEventDto { TempleId = t.Id, Name = "E2", Description = "D2", EventType = "Pooja", StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(2), Location = "L", MaxAttendees = 10, RegistrationFee = 5 };
			var result = await _eventService.UpdateEventAsync(ev.Id, dto);

			result.Should().NotBeNull();
			result!.Name.Should().Be("E2");
			result.MaxAttendees.Should().Be(10);
			result.EntryFee.Should().Be(5);
		}

		[Fact]
		public async Task UpdateEventAsync_ShouldReturnNull_WhenNotFound()
		{
			var dto = new CreateEventDto { TempleId = 1, Name = "E", EventType = "F", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow };
			var result = await _eventService.UpdateEventAsync(999, dto);
			result.Should().BeNull();
		}

		[Fact]
		public async Task UpdateEventStatusAsync_ShouldUpdateStatus()
		{
			var t = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(t);
			await _context.SaveChangesAsync();
			var ev = new Event { TempleId = t.Id, Name = "E", EventType = "F", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow, Status = "Scheduled", IsActive = true };
			_context.Events.Add(ev);
			await _context.SaveChangesAsync();

			var ok = await _eventService.UpdateEventStatusAsync(ev.Id, "Completed");
			ok.Should().BeTrue();
			var stored = await _context.Events.FindAsync(ev.Id);
			stored!.Status.Should().Be("Completed");
		}

		[Fact]
		public async Task DeleteEventAsync_ShouldSoftDelete()
		{
			var t = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(t);
			await _context.SaveChangesAsync();
			var ev = new Event { TempleId = t.Id, Name = "E", EventType = "F", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow, IsActive = true };
			_context.Events.Add(ev);
			await _context.SaveChangesAsync();

			var ok = await _eventService.DeleteEventAsync(ev.Id);
			ok.Should().BeTrue();
			var stored = await _context.Events.FindAsync(ev.Id);
			stored!.IsActive.Should().BeFalse();
		}

		#endregion

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}