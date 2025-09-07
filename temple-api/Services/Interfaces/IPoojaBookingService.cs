using TempleApi.Models.DTOs;
using TempleApi.Enums;

namespace TempleApi.Services.Interfaces
{
    public interface IPoojaBookingService
    {
        Task<PoojaBookingDto> CreateBookingAsync(CreatePoojaBookingDto createBookingDto);
        Task<PoojaBookingDto?> GetBookingByIdAsync(int id);
        Task<IEnumerable<PoojaBookingDto>> GetAllBookingsAsync();
        Task<IEnumerable<PoojaBookingDto>> GetBookingsByCustomerAsync(int customerId);
        Task<IEnumerable<PoojaBookingDto>> GetBookingsByStaffAsync(int staffId);
        Task<IEnumerable<PoojaBookingDto>> GetBookingsByStatusAsync(BookingStatus status);
        Task<IEnumerable<PoojaBookingDto>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<PoojaBookingDto> UpdateBookingStatusAsync(int id, BookingStatus status);
        Task<PoojaBookingDto> AssignStaffToBookingAsync(int id, int staffId);
        Task<bool> DeleteBookingAsync(int id);
    }
}
