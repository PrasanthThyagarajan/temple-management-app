using TempleApi.Data;
using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using TempleApi.Enums;
using Microsoft.EntityFrameworkCore;

namespace TempleApi.Repositories
{
    public class PoojaBookingRepository : Repository<PoojaBooking>, IPoojaBookingRepository
    {
        public PoojaBookingRepository(TempleDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PoojaBooking>> GetByCustomerAsync(int customerId)
        {
            return await _dbSet
                .Include(b => b.Customer)
                .Include(b => b.Pooja)
                .Include(b => b.Staff)
                .Where(b => b.UserId == customerId && b.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<PoojaBooking>> GetByStaffAsync(int staffId)
        {
            return await _dbSet
                .Include(b => b.Customer)
                .Include(b => b.Pooja)
                .Include(b => b.Staff)
                .Where(b => b.StaffId == staffId && b.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<PoojaBooking>> GetByStatusAsync(BookingStatus status)
        {
            return await _dbSet
                .Include(b => b.Customer)
                .Include(b => b.Pooja)
                .Include(b => b.Staff)
                .Where(b => b.Status == status && b.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<PoojaBooking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(b => b.Customer)
                .Include(b => b.Pooja)
                .Include(b => b.Staff)
                .Where(b => b.ScheduledDate >= startDate && b.ScheduledDate <= endDate && b.IsActive)
                .ToListAsync();
        }

        public async Task<PoojaBooking?> GetWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(b => b.Customer)
                .Include(b => b.Pooja)
                .Include(b => b.Staff)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<PoojaBooking>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(b => b.Customer)
                .Include(b => b.Pooja)
                .Include(b => b.Staff)
                .Where(b => b.IsActive)
                .ToListAsync();
        }
    }
}
