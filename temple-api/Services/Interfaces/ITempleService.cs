using TempleApi.Models;
using TempleApi.Models.DTOs;

namespace TempleApi.Services.Interfaces
{
    public interface ITempleService
    {
        Task<IEnumerable<Temple>> GetAllTemplesAsync();
        Task<Temple?> GetTempleByIdAsync(int id);
        Task<Temple> CreateTempleAsync(CreateTempleDto createDto);
        Task<Temple?> UpdateTempleAsync(int id, CreateTempleDto updateDto);
        Task<bool> DeleteTempleAsync(int id);
        Task<IEnumerable<Temple>> SearchTemplesAsync(string searchTerm);
        Task<IEnumerable<Temple>> GetTemplesByLocationAsync(string city, string? state);
    }
}
