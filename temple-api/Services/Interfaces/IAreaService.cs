using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;

namespace TempleApi.Services.Interfaces
{
    public interface IAreaService
    {
        Task<IEnumerable<Area>> GetAllAreasAsync();
        Task<Area?> GetAreaByIdAsync(int id);
        Task<IEnumerable<Area>> GetAreasByTempleAsync(int templeId);
        Task<Area> CreateAreaAsync(CreateAreaDto createDto);
        Task<Area?> UpdateAreaAsync(int id, CreateAreaDto updateDto);
        Task<bool> DeleteAreaAsync(int id);
    }
}


