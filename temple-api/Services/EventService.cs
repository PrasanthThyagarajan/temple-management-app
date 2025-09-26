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
                .Include(e => e.Area)
                .Include(e => e.EventType)
                .Where(e => e.IsActive)
                .OrderByDescending(e => e.Id)
                .ToListAsync();
        }

        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _context.Events
                .Include(e => e.Area)
                .Include(e => e.EventType)
                .Include(e => e.Registrations.Where(r => r.Status == "Confirmed"))
                .FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
        }

        public async Task<IEnumerable<Event>> GetEventsByTempleAsync(int templeId)
        {
            return await _context.Events
                .Where(e => e.Area != null && e.Area.TempleId == templeId && e.IsActive)
                .OrderByDescending(e => e.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync(int templeId)
        {
            var currentDate = DateTime.UtcNow;
            return await _context.Events
                .Where(e => e.Area != null && e.Area.TempleId == templeId && 
                           e.IsActive && 
                           e.StartDate >= currentDate &&
                           e.Status == "Scheduled")
                .OrderByDescending(e => e.Id)
                .ToListAsync();
        }

        public async Task<Event> CreateEventAsync(CreateEventDto createDto)
        {
            var eventEntity = new Event
            {
                AreaId = createDto.AreaId,
                Name = createDto.Name,
                Description = createDto.Description ?? string.Empty,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                Location = createDto.Location ?? string.Empty,
                EventTypeId = createDto.EventTypeId,
                MaxAttendees = createDto.MaxAttendees,
                EntryFee = createDto.RegistrationFee,
                Status = string.IsNullOrWhiteSpace(createDto.Status) ? string.Empty : createDto.Status,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsApprovalNeeded = createDto.IsApprovalNeeded
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
            eventEntity.AreaId = updateDto.AreaId;
            eventEntity.Location = updateDto.Location ?? string.Empty;
            eventEntity.EventTypeId = updateDto.EventTypeId;
            eventEntity.MaxAttendees = updateDto.MaxAttendees;
            eventEntity.EntryFee = updateDto.RegistrationFee;
            eventEntity.Status = updateDto.Status;
            eventEntity.UpdatedAt = DateTime.UtcNow;
            eventEntity.IsApprovalNeeded = updateDto.IsApprovalNeeded;

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
                .Include(e => e.Area)
                .Where(e => e.IsActive && (
                    e.Name.ToLower().Contains(normalizedSearchTerm) ||
                    e.Description.ToLower().Contains(normalizedSearchTerm) ||
                    e.EventType.Name.ToLower().Contains(normalizedSearchTerm) ||
                    e.Location.ToLower().Contains(normalizedSearchTerm)
                ))
                .OrderByDescending(e => e.Id)
                .ToListAsync();
        }
    }
}
