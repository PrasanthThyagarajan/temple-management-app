using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Repositories.Interfaces;
using TempleApi.Services.Interfaces;

namespace TempleApi.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ISaleRepository _saleRepository;

        public BookingService(IBookingRepository bookingRepository, ISaleRepository saleRepository)
        {
            _bookingRepository = bookingRepository;
            _saleRepository = saleRepository;
        }

        public async Task<IEnumerable<BookingDto>> GetAllAsync()
        {
            var all = await _bookingRepository.GetAllAsync();
            return all.Select(MapToDto);
        }

        public async Task<BookingDto?> GetByIdAsync(int id)
        {
            var b = await _bookingRepository.GetByIdAsync(id);
            return b != null ? MapToDto(b) : null;
        }

        public async Task<BookingDto> CreateAsync(CreateBookingDto dto)
        {
            var booking = new Booking
            {
                UserId = dto.UserId,
                StaffId = dto.StaffId,
                BookingDate = dto.BookingDate,
                ProductId = dto.ProductId,
                CategoryId = dto.CategoryId,
                EstimatedAmount = dto.EstimatedAmount,
                PaymentMethod = dto.PaymentMethod,
                Notes = dto.Notes,
                Status = "Pending"
            };

            await _bookingRepository.AddAsync(booking);
            return MapToDto(booking);
        }

        public async Task<bool> ApproveAsync(int id, int approvedByUserId)
        {
            var b = await _bookingRepository.GetByIdAsync(id);
            if (b == null) return false;
            b.Status = "Approved";
            b.ApprovedBy = approvedByUserId;
            b.ApprovedOn = DateTime.UtcNow;
            await _bookingRepository.UpdateAsync(b);

            // Create corresponding sale record (header only)
            var sale = new Sale
            {
                UserId = b.UserId,
                StaffId = b.StaffId,
                SaleDate = b.BookingDate,
                TotalAmount = b.EstimatedAmount ?? 0,
                DiscountAmount = 0,
                FinalAmount = b.EstimatedAmount ?? 0,
                PaymentMethod = b.PaymentMethod,
                IsActive = true,
                Notes = b.Notes,
                ProductId = b.ProductId,
                SalesBookingStatusId = null,
                BookingToken = null
            };
            await _saleRepository.AddAsync(sale);
            return true;
        }

        public async Task<bool> RejectAsync(int id, int approvedByUserId)
        {
            var b = await _bookingRepository.GetByIdAsync(id);
            if (b == null) return false;
            b.Status = "Rejected";
            b.ApprovedBy = approvedByUserId;
            b.ApprovedOn = DateTime.UtcNow;
            await _bookingRepository.UpdateAsync(b);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _bookingRepository.SoftDeleteAsync(id);
        }

        private static BookingDto MapToDto(Booking b)
        {
            return new BookingDto
            {
                Id = b.Id,
                UserId = b.UserId,
                StaffId = b.StaffId,
                BookingDate = b.BookingDate,
                ProductId = b.ProductId,
                CategoryId = b.CategoryId,
                EstimatedAmount = b.EstimatedAmount,
                PaymentMethod = b.PaymentMethod,
                Notes = b.Notes,
                ApprovedBy = b.ApprovedBy,
                ApprovedOn = b.ApprovedOn,
                Status = b.Status
            };
        }
    }
}


