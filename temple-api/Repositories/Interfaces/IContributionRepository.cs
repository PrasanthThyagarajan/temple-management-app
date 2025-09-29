using System.Collections.Generic;
using System.Threading.Tasks;
using TempleApi.Domain.Entities;

namespace TempleApi.Repositories.Interfaces
{
    public interface IContributionRepository : IRepository<Contribution>
    {
        Task<IEnumerable<Contribution>> GetByEventIdAsync(int eventId);
        Task<IEnumerable<Contribution>> GetByDevoteeIdAsync(int devoteeId);
        Task<IEnumerable<Contribution>> GetByContributionSettingIdAsync(int contributionSettingId);
        Task<IEnumerable<Contribution>> GetActiveContributionsAsync();
        Task<decimal> GetTotalContributionsByEventAsync(int eventId);
        Task<IEnumerable<Contribution>> GetContributionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
