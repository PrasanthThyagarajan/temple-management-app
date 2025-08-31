using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;

namespace TempleApi.Services.Interfaces
{
    public interface IDevoteeService
    {
        Task<IEnumerable<Devotee>> GetAllDevoteesAsync();
        Task<Devotee?> GetDevoteeByIdAsync(int id);
        Task<IEnumerable<Devotee>> GetDevoteesByTempleAsync(int templeId);
        Task<Devotee> CreateDevoteeAsync(CreateDevoteeDto createDto);
        Task<Devotee?> UpdateDevoteeAsync(int id, CreateDevoteeDto updateDto);
        Task<bool> DeleteDevoteeAsync(int id);
        Task<IEnumerable<Devotee>> SearchDevoteesAsync(string searchTerm);
    }
}
