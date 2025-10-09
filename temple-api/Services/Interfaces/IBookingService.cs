using TempleApi.Models.DTOs;

namespace TempleApi.Services.Interfaces
{
    public interface IBookingService
    {
        Task<IEnumerable<BookingDto>> GetAllAsync();
        Task<BookingDto?> GetByIdAsync(int id);
        Task<BookingDto> CreateAsync(CreateBookingDto dto);
        Task<bool> ApproveAsync(int id, int approvedByUserId);
        Task<bool> RejectAsync(int id, int approvedByUserId);
        Task<bool> DeleteAsync(int id);
    }
}


