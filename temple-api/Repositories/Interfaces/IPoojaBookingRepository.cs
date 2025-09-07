using TempleApi.Domain.Entities;
using TempleApi.Enums;

namespace TempleApi.Repositories.Interfaces
{
    public interface IPoojaBookingRepository : IRepository<PoojaBooking>
    {
        Task<IEnumerable<PoojaBooking>> GetByCustomerAsync(int customerId);
        Task<IEnumerable<PoojaBooking>> GetByStaffAsync(int staffId);
        Task<IEnumerable<PoojaBooking>> GetByStatusAsync(BookingStatus status);
        Task<IEnumerable<PoojaBooking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<PoojaBooking?> GetWithDetailsAsync(int id);
        Task<IEnumerable<PoojaBooking>> GetAllWithDetailsAsync();
    }
}
