using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TempleApi.Models.DTOs;

namespace TempleApi.Services.Interfaces
{
    public interface IContributionService
    {
        Task<IEnumerable<ContributionDto>> GetAllAsync();
        Task<IEnumerable<ContributionDto>> GetByEventIdAsync(int eventId);
        Task<IEnumerable<ContributionDto>> GetByDevoteeIdAsync(int devoteeId);
        Task<ContributionDto?> GetByIdAsync(int id);
        Task<ContributionDto> CreateAsync(CreateContributionDto dto);
        Task<ContributionDto?> UpdateAsync(int id, CreateContributionDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<ContributionDto>> GetActiveContributionsAsync();
        Task<decimal> GetTotalContributionsByEventAsync(int eventId);
        Task<IEnumerable<ContributionDto>> GetContributionsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<ContributionSummaryDto>> GetContributionSummaryByEventAsync();
    }
}
