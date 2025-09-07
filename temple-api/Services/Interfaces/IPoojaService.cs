using TempleApi.Models.DTOs;

namespace TempleApi.Services.Interfaces
{
    public interface IPoojaService
    {
        Task<PoojaDto> CreatePoojaAsync(CreatePoojaDto createPoojaDto);
        Task<PoojaDto?> GetPoojaByIdAsync(int id);
        Task<IEnumerable<PoojaDto>> GetAllPoojasAsync();
        Task<PoojaDto> UpdatePoojaAsync(int id, CreatePoojaDto updatePoojaDto);
        Task<bool> DeletePoojaAsync(int id);
    }
}
