using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;

namespace TempleApi.Services.Interfaces
{
    public interface IDonationService
    {
        Task<IEnumerable<Donation>> GetAllDonationsAsync();
        Task<Donation?> GetDonationByIdAsync(int id);
        Task<IEnumerable<Donation>> GetDonationsByTempleAsync(int templeId);
        Task<IEnumerable<Donation>> GetDonationsByDevoteeAsync(int devoteeId);
        Task<Donation> CreateDonationAsync(CreateDonationDto createDto);
        Task<Donation?> UpdateDonationStatusAsync(int id, string status);
        Task<bool> DeleteDonationAsync(int id);
        Task<decimal> GetTotalDonationsByTempleAsync(int templeId);
    }
}
