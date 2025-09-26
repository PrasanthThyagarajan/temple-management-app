using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services.Interfaces;

namespace TempleApi.Services
{
    public class AreaService : IAreaService
    {
        private readonly TempleDbContext _context;

        public AreaService(TempleDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Area>> GetAllAreasAsync()
        {
            return await _context.Areas
                .Include(a => a.Temple)
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.Id)
                .ToListAsync();
        }

        public async Task<Area?> GetAreaByIdAsync(int id)
        {
            return await _context.Areas
                .Include(a => a.Temple)
                .FirstOrDefaultAsync(a => a.Id == id && a.IsActive);
        }

        public async Task<IEnumerable<Area>> GetAreasByTempleAsync(int templeId)
        {
            return await _context.Areas
                .Where(a => a.TempleId == templeId && a.IsActive)
                .OrderByDescending(a => a.Id)
                .ToListAsync();
        }

        public async Task<Area> CreateAreaAsync(CreateAreaDto createDto)
        {
            var area = new Area
            {
                TempleId = createDto.TempleId,
                Name = createDto.Name,
                Description = createDto.Description ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Areas.Add(area);
            await _context.SaveChangesAsync();
            return area;
        }

        public async Task<Area?> UpdateAreaAsync(int id, CreateAreaDto updateDto)
        {
            var area = await _context.Areas.FindAsync(id);
            if (area == null || !area.IsActive)
                return null;

            area.Name = updateDto.Name;
            area.Description = updateDto.Description ?? string.Empty;
            area.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return area;
        }

        public async Task<bool> DeleteAreaAsync(int id)
        {
            var area = await _context.Areas.FindAsync(id);
            if (area == null || !area.IsActive)
                return false;

            area.IsActive = false;
            area.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}


