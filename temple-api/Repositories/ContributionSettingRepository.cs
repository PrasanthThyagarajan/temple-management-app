using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Repositories.Interfaces;

namespace TempleApi.Repositories
{
    public class ContributionSettingRepository : Repository<ContributionSetting>, IContributionSettingRepository
    {
        public ContributionSettingRepository(TempleDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ContributionSetting>> GetByEventIdAsync(int eventId)
        {
            return await _dbSet
                .Include(c => c.Event)
                .Where(c => c.EventId == eventId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ContributionSetting>> GetActiveContributionsAsync()
        {
            return await _dbSet
                .Include(c => c.Event)
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        public override async Task<IEnumerable<ContributionSetting>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.Event)
                .ToListAsync();
        }

        public override async Task<ContributionSetting?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
