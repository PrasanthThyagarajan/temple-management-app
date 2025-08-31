using TempleApi.Models;
using TempleApi.Models.DTOs;

namespace TempleApi.Services.Interfaces
{
    public interface IEventService
    {
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task<Event?> GetEventByIdAsync(int id);
        Task<IEnumerable<Event>> GetEventsByTempleAsync(int templeId);
        Task<IEnumerable<Event>> GetUpcomingEventsAsync(int templeId);
        Task<Event> CreateEventAsync(CreateEventDto createDto);
        Task<Event?> UpdateEventAsync(int id, CreateEventDto updateDto);
        Task<bool> DeleteEventAsync(int id);
        Task<bool> UpdateEventStatusAsync(int id, string status);
        Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm);
    }
}
