using TempleApi.Domain.Entities;

namespace TempleApi.Services.Interfaces
{
    public interface IEventTypeService
    {
        Task<IEnumerable<EventType>> GetAllAsync();
        Task<EventType?> GetByIdAsync(int id);
    }
}


