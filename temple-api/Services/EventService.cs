using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services.Interfaces;

namespace TempleApi.Services
{
    public class EventService : IEventService
    {
        private readonly TempleDbContext _context;

        public EventService(TempleDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _context.Events
                .Include(e => e.Temple)
                .Where(e => e.IsActive)
                .OrderBy(e => e.StartDate)
                .ToListAsync();
        }

        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _context.Events
                .Include(e => e.Temple)
                .Include(e => e.Registrations.Where(r => r.Status == "Confirmed"))
                .FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
        }

        public async Task<IEnumerable<Event>> GetEventsByTempleAsync(int templeId)
        {
            return await _context.Events
                .Where(e => e.TempleId == templeId && e.IsActive)
                .OrderBy(e => e.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync(int templeId)
        {
            var currentDate = DateTime.UtcNow;
            return await _context.Events
                .Where(e => e.TempleId == templeId && 
                           e.IsActive && 
                           e.StartDate >= currentDate &&
                           e.Status == "Scheduled")
                .OrderBy(e => e.StartDate)
                .ToListAsync();
        }

        public async Task<Event> CreateEventAsync(CreateEventDto createDto)
        {
            var eventEntity = new Event
            {
                TempleId = createDto.TempleId,
                Name = createDto.Name,
                Description = createDto.Description ?? string.Empty,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                Location = createDto.Location ?? string.Empty,
                EventType = createDto.EventType,
                MaxAttendees = createDto.MaxAttendees,
                EntryFee = createDto.RegistrationFee,
                Status = "Scheduled",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            return eventEntity;
        }

        public async Task<Event?> UpdateEventAsync(int id, CreateEventDto updateDto)
        {
            var eventEntity = await _context.Events.FindAsync(id);
            if (eventEntity == null || !eventEntity.IsActive)
                return null;

            eventEntity.Name = updateDto.Name;
            eventEntity.Description = updateDto.Description ?? string.Empty;
            eventEntity.StartDate = updateDto.StartDate;
            eventEntity.EndDate = updateDto.EndDate;
            eventEntity.Location = updateDto.Location ?? string.Empty;
            eventEntity.EventType = updateDto.EventType;
            eventEntity.MaxAttendees = updateDto.MaxAttendees;
            eventEntity.EntryFee = updateDto.RegistrationFee;
            eventEntity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return eventEntity;
        }

        public async Task<bool> UpdateEventStatusAsync(int id, string status)
        {
            var eventEntity = await _context.Events.FindAsync(id);
            if (eventEntity == null || !eventEntity.IsActive)
                return false;

            eventEntity.Status = status;
            eventEntity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            var eventEntity = await _context.Events.FindAsync(id);
            if (eventEntity == null || !eventEntity.IsActive)
                return false;

            eventEntity.IsActive = false;
            eventEntity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllEventsAsync();

            var normalizedSearchTerm = searchTerm.ToLower();
            return await _context.Events
                .Include(e => e.Temple)
                .Where(e => e.IsActive && (
                    e.Name.ToLower().Contains(normalizedSearchTerm) ||
                    e.Description.ToLower().Contains(normalizedSearchTerm) ||
                    e.EventType.ToLower().Contains(normalizedSearchTerm) ||
                    e.Location.ToLower().Contains(normalizedSearchTerm)
                ))
                .OrderBy(e => e.StartDate)
                .ToListAsync();
        }
    }
}
