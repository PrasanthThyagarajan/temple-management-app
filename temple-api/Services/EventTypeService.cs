using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Services.Interfaces;

namespace TempleApi.Services
{
    public class EventTypeService : IEventTypeService
    {
        private readonly TempleDbContext _context;

        public EventTypeService(TempleDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EventType>> GetAllAsync()
        {
            return await _context.EventTypes
                .OrderByDescending(t => t.Id)
                .ToListAsync();
        }

        public async Task<EventType?> GetByIdAsync(int id)
        {
            return await _context.EventTypes.FindAsync(id);
        }
    }
}


