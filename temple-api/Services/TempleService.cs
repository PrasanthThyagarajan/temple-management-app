using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services.Interfaces;

namespace TempleApi.Services
{
    public class TempleService : ITempleService
    {
        private readonly TempleDbContext _context;

        public TempleService(TempleDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Temple>> GetAllTemplesAsync()
        {
            return await _context.Temples
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<Temple?> GetTempleByIdAsync(int id)
        {
            return await _context.Temples
                .Include(t => t.Devotees.Where(d => d.IsActive))
                .Include(t => t.Services.Where(s => s.IsActive))
                .Include(t => t.Events.Where(e => e.IsActive))
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
        }

        public async Task<Temple> CreateTempleAsync(CreateTempleDto createDto)
        {
            var temple = new Temple
            {
                Name = createDto.Name,
                Description = createDto.Description ?? string.Empty,
                Address = createDto.Address,
                City = createDto.City ?? string.Empty,
                State = createDto.State ?? string.Empty,
                Phone = createDto.PhoneNumber ?? string.Empty,
                Email = createDto.Email ?? string.Empty,
                Deity = createDto.Deity ?? string.Empty,
                EstablishedDate = createDto.EstablishedDate ?? DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Temples.Add(temple);
            await _context.SaveChangesAsync();

            return temple;
        }

        public async Task<Temple?> UpdateTempleAsync(int id, CreateTempleDto updateDto)
        {
            var temple = await _context.Temples.FindAsync(id);
            if (temple == null || !temple.IsActive)
                return null;

            temple.Name = updateDto.Name;
            temple.Description = updateDto.Description ?? string.Empty;
            temple.Address = updateDto.Address;
            temple.City = updateDto.City ?? string.Empty;
            temple.State = updateDto.State ?? string.Empty;
            temple.Phone = updateDto.PhoneNumber ?? string.Empty;
            temple.Email = updateDto.Email ?? string.Empty;
            temple.Deity = updateDto.Deity ?? string.Empty;
            temple.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return temple;
        }

        public async Task<bool> DeleteTempleAsync(int id)
        {
            var temple = await _context.Temples.FindAsync(id);
            if (temple == null || !temple.IsActive)
                return false;

            temple.IsActive = false;
            temple.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Temple>> SearchTemplesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllTemplesAsync();

            var normalizedSearchTerm = searchTerm.ToLower();
            return await _context.Temples
                .Where(t => t.IsActive && (
                    t.Name.ToLower().Contains(normalizedSearchTerm) ||
                    t.City.ToLower().Contains(normalizedSearchTerm) ||
                    t.State.ToLower().Contains(normalizedSearchTerm) ||
                    t.Deity.ToLower().Contains(normalizedSearchTerm)
                ))
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Temple>> GetTemplesByLocationAsync(string city, string? state = null)
        {
            var query = _context.Temples.Where(t => t.IsActive && t.City.ToLower() == city.ToLower());
            
            if (!string.IsNullOrWhiteSpace(state))
            {
                query = query.Where(t => t.State.ToLower() == state.ToLower());
            }

            return await query
                .OrderBy(t => t.Name)
                .ToListAsync();
        }
    }
}
