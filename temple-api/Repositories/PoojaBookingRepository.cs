using TempleApi.Data;
using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using TempleApi.Enums;

namespace TempleApi.Repositories
{
    public class PoojaBookingRepository : Repository<PoojaBooking>, IPoojaBookingRepository
    {
        public PoojaBookingRepository(TempleDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PoojaBooking>> GetByCustomerAsync(int customerId)
        {
            return await FindAsync(b => b.UserId == customerId && b.IsActive,
                b => b.Customer, b => b.Pooja, b => b.Staff);
        }

        public async Task<IEnumerable<PoojaBooking>> GetByStaffAsync(int staffId)
        {
            return await FindAsync(b => b.StaffId == staffId && b.IsActive,
                b => b.Customer, b => b.Pooja, b => b.Staff);
        }

        public async Task<IEnumerable<PoojaBooking>> GetByStatusAsync(BookingStatus status)
        {
            return await FindAsync(b => b.Status == status && b.IsActive,
                b => b.Customer, b => b.Pooja, b => b.Staff);
        }

        public async Task<IEnumerable<PoojaBooking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await FindAsync(b => b.ScheduledDate >= startDate && b.ScheduledDate <= endDate && b.IsActive,
                b => b.Customer, b => b.Pooja, b => b.Staff);
        }

        public async Task<PoojaBooking?> GetWithDetailsAsync(int id)
        {
            return await GetByIdAsync(id, b => b.Customer, b => b.Pooja, b => b.Staff);
        }

        public async Task<IEnumerable<PoojaBooking>> GetAllWithDetailsAsync()
        {
            return await GetAllAsync(b => b.Customer, b => b.Pooja, b => b.Staff);
        }
    }
}
