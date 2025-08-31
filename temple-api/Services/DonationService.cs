using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Models;
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
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();
        }

        public async Task<Donation?> GetDonationByIdAsync(int id)
        {
            return await _context.Donations
                .Include(d => d.Temple)
                .Include(d => d.Devotee)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Donation>> GetDonationsByTempleAsync(int templeId)
        {
            return await _context.Donations
                .Include(d => d.Devotee)
                .Where(d => d.TempleId == templeId)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Donation>> GetDonationsByDevoteeAsync(int devoteeId)
        {
            return await _context.Donations
                .Include(d => d.Temple)
                .Where(d => d.DevoteeId == devoteeId)
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
                DonorEmail = createDto.DonorEmail,
                DonorPhone = createDto.DonorPhone,
                Amount = createDto.Amount,
                DonationType = createDto.DonationType,
                Purpose = createDto.Purpose,
                Notes = createDto.Notes,
                PaymentMethod = createDto.PaymentMethod,
                DonationDate = createDto.DonationDate ?? DateTime.UtcNow,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            return donation;
        }

        public async Task<Donation?> UpdateDonationStatusAsync(int id, string status)
        {
            var donation = await _context.Donations.FindAsync(id);
            if (donation == null)
                return null;

            donation.Status = status;
            donation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return donation;
        }

        public async Task<bool> DeleteDonationAsync(int id)
        {
            var donation = await _context.Donations.FindAsync(id);
            if (donation == null)
                return false;

            _context.Donations.Remove(donation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetTotalDonationsByTempleAsync(int templeId)
        {
            return await _context.Donations
                .Where(d => d.TempleId == templeId && d.Status == "Completed")
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
