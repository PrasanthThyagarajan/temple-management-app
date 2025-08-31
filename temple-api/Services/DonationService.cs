using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services.Interfaces;

namespace TempleApi.Services
{
    public class DonationService : IDonationService
    {
        private readonly TempleDbContext _context;

        public DonationService(TempleDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Donation>> GetAllDonationsAsync()
        {
            return await _context.Donations
                .Include(d => d.Temple)
                .Include(d => d.Devotee)
                .Where(d => d.IsActive)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();
        }

        public async Task<Donation?> GetDonationByIdAsync(int id)
        {
            return await _context.Donations
                .Include(d => d.Temple)
                .Include(d => d.Devotee)
                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);
        }

        public async Task<IEnumerable<Donation>> GetDonationsByTempleAsync(int templeId)
        {
            return await _context.Donations
                .Include(d => d.Devotee)
                .Where(d => d.TempleId == templeId && d.IsActive)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Donation>> GetDonationsByDevoteeAsync(int devoteeId)
        {
            return await _context.Donations
                .Include(d => d.Temple)
                .Where(d => d.DevoteeId == devoteeId && d.IsActive)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();
        }

        public async Task<Donation> CreateDonationAsync(CreateDonationDto createDto)
        {
            var donation = new Donation
            {
                TempleId = createDto.TempleId,
                DevoteeId = createDto.DevoteeId,
                DonorName = createDto.DonorName,
                Amount = createDto.Amount,
                DonationType = createDto.DonationType,
                Purpose = createDto.Purpose ?? string.Empty,
                Status = createDto.Status ?? "Pending",
                DonationDate = createDto.DonationDate ?? DateTime.UtcNow,
                ReceiptNumber = createDto.ReceiptNumber ?? string.Empty,
                Notes = createDto.Notes ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            return donation;
        }

        public async Task<Donation?> UpdateDonationStatusAsync(int id, string status)
        {
            var donation = await _context.Donations.FindAsync(id);
            if (donation == null || !donation.IsActive)
                return null;

            donation.Status = status;
            donation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return donation;
        }

        public async Task<bool> DeleteDonationAsync(int id)
        {
            var donation = await _context.Donations.FindAsync(id);
            if (donation == null || !donation.IsActive)
                return false;

            donation.IsActive = false;
            donation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetTotalDonationsByTempleAsync(int templeId)
        {
            return await _context.Donations
                .Where(d => d.TempleId == templeId && 
                           d.IsActive && 
                           d.Status == "Completed")
                .SumAsync(d => d.Amount);
        }

        public async Task<decimal> GetTotalDonationsByDateRangeAsync(int templeId, DateTime startDate, DateTime endDate)
        {
            return await _context.Donations
                .Where(d => d.TempleId == templeId && 
                           d.Status == "Completed" &&
                           d.DonationDate >= startDate &&
                           d.DonationDate <= endDate)
                .SumAsync(d => d.Amount);
        }
    }
}
