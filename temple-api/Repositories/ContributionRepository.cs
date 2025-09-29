using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Repositories.Interfaces;

namespace TempleApi.Repositories
{
    public class ContributionRepository : Repository<Contribution>, IContributionRepository
    {
        public ContributionRepository(TempleDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Contribution>> GetByEventIdAsync(int eventId)
        {
            return await _dbSet
                .Include(c => c.Event)
                .Include(c => c.Devotee)
                .Include(c => c.ContributionSetting)
                .Where(c => c.EventId == eventId && c.IsActive)
                .OrderByDescending(c => c.ContributionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Contribution>> GetByDevoteeIdAsync(int devoteeId)
        {
            return await _dbSet
                .Include(c => c.Event)
                .Include(c => c.Devotee)
                .Include(c => c.ContributionSetting)
                .Where(c => c.DevoteeId == devoteeId && c.IsActive)
                .OrderByDescending(c => c.ContributionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Contribution>> GetByContributionSettingIdAsync(int contributionSettingId)
        {
            return await _dbSet
                .Include(c => c.Event)
                .Include(c => c.Devotee)
                .Include(c => c.ContributionSetting)
                .Where(c => c.ContributionSettingId == contributionSettingId && c.IsActive)
                .OrderByDescending(c => c.ContributionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Contribution>> GetActiveContributionsAsync()
        {
            return await _dbSet
                .Include(c => c.Event)
                .Include(c => c.Devotee)
                .Include(c => c.ContributionSetting)
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.ContributionDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalContributionsByEventAsync(int eventId)
        {
            return await _dbSet
                .Where(c => c.EventId == eventId && c.IsActive)
                .SumAsync(c => c.Amount);
        }

        public async Task<IEnumerable<Contribution>> GetContributionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(c => c.Event)
                .Include(c => c.Devotee)
                .Include(c => c.ContributionSetting)
                .Where(c => c.ContributionDate >= startDate && c.ContributionDate <= endDate && c.IsActive)
                .OrderByDescending(c => c.ContributionDate)
                .ToListAsync();
        }

        public override async Task<IEnumerable<Contribution>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.Event)
                .Include(c => c.Devotee)
                .Include(c => c.ContributionSetting)
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.ContributionDate)
                .ToListAsync();
        }

        public override async Task<Contribution?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Event)
                .Include(c => c.Devotee)
                .Include(c => c.ContributionSetting)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
