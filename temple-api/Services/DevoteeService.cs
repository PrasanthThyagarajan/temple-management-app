using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services.Interfaces;

namespace TempleApi.Services
{
    public class DevoteeService : IDevoteeService
    {
        private readonly TempleDbContext _context;

        public DevoteeService(TempleDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Devotee>> GetAllDevoteesAsync()
        {
            return await _context.Devotees
                .Include(d => d.Temple)
                .Where(d => d.IsActive)
                .OrderBy(d => d.FirstName)
                .ThenBy(d => d.LastName)
                .ToListAsync();
        }

        public async Task<Devotee?> GetDevoteeByIdAsync(int id)
        {
            return await _context.Devotees
                .Include(d => d.Temple)
                .Include(d => d.Donations.Where(don => don.IsActive))
                .Include(d => d.EventRegistrations.Where(er => er.IsActive))
                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);
        }

        public async Task<IEnumerable<Devotee>> GetDevoteesByTempleAsync(int templeId)
        {
            return await _context.Devotees
                .Where(d => d.TempleId == templeId && d.IsActive)
                .OrderBy(d => d.FirstName)
                .ThenBy(d => d.LastName)
                .ToListAsync();
        }

        public async Task<Devotee> CreateDevoteeAsync(CreateDevoteeDto createDto)
        {
            var devotee = new Devotee
            {
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                Email = createDto.Email ?? string.Empty,
                Phone = createDto.Phone ?? string.Empty,
                Address = createDto.Address ?? string.Empty,
                City = createDto.City ?? string.Empty,
                State = createDto.State ?? string.Empty,
                PostalCode = createDto.PostalCode ?? string.Empty,
                DateOfBirth = createDto.DateOfBirth,
                Gender = createDto.Gender ?? string.Empty,
                TempleId = createDto.TempleId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Devotees.Add(devotee);
            await _context.SaveChangesAsync();

            return devotee;
        }

        public async Task<Devotee?> UpdateDevoteeAsync(int id, CreateDevoteeDto updateDto)
        {
            var devotee = await _context.Devotees.FindAsync(id);
            if (devotee == null || !devotee.IsActive)
                return null;

            devotee.FirstName = updateDto.FirstName;
            devotee.LastName = updateDto.LastName;
            devotee.Email = updateDto.Email ?? string.Empty;
            devotee.Phone = updateDto.Phone ?? string.Empty;
            devotee.Address = updateDto.Address ?? string.Empty;
            devotee.City = updateDto.City ?? string.Empty;
            devotee.State = updateDto.State ?? string.Empty;
            devotee.PostalCode = updateDto.PostalCode ?? string.Empty;
            devotee.DateOfBirth = updateDto.DateOfBirth;
            devotee.Gender = updateDto.Gender ?? string.Empty;
            devotee.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return devotee;
        }

        public async Task<bool> DeleteDevoteeAsync(int id)
        {
            var devotee = await _context.Devotees.FindAsync(id);
            if (devotee == null || !devotee.IsActive)
                return false;

            devotee.IsActive = false;
            devotee.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Devotee>> SearchDevoteesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllDevoteesAsync();

            var normalizedSearchTerm = searchTerm.ToLower();
            return await _context.Devotees
                .Include(d => d.Temple)
                .Where(d => d.IsActive && (
                    d.FirstName.ToLower().Contains(normalizedSearchTerm) ||
                    d.LastName.ToLower().Contains(normalizedSearchTerm) ||
                    d.Email.ToLower().Contains(normalizedSearchTerm) ||
                    d.Phone.ToLower().Contains(normalizedSearchTerm) ||
                    d.City.ToLower().Contains(normalizedSearchTerm) ||
                    d.State.ToLower().Contains(normalizedSearchTerm)
                ))
                .OrderBy(d => d.FirstName)
                .ThenBy(d => d.LastName)
                .ToListAsync();
        }
    }
}
