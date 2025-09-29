using System.Collections.Generic;
using System.Threading.Tasks;
using TempleApi.Domain.Entities;

namespace TempleApi.Repositories.Interfaces
{
    public interface IContributionSettingRepository : IRepository<ContributionSetting>
    {
        Task<IEnumerable<ContributionSetting>> GetByEventIdAsync(int eventId);
        Task<IEnumerable<ContributionSetting>> GetActiveContributionsAsync();
    }
}
