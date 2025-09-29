using System.Collections.Generic;
using System.Threading.Tasks;
using TempleApi.Models.DTOs;

namespace TempleApi.Services.Interfaces
{
    public interface IContributionSettingService
    {
        Task<IEnumerable<ContributionSettingDto>> GetAllAsync();
        Task<IEnumerable<ContributionSettingDto>> GetByEventIdAsync(int eventId);
        Task<ContributionSettingDto?> GetByIdAsync(int id);
        Task<ContributionSettingDto> CreateAsync(CreateContributionSettingDto dto);
        Task<ContributionSettingDto?> UpdateAsync(int id, CreateContributionSettingDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<ContributionSettingDto>> GetActiveContributionsAsync();
    }
}
