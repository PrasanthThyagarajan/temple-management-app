using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services.Interfaces;
using TempleApi.Enums;

namespace TempleApi.Services
{
    public class PoojaBookingService : IPoojaBookingService
    {
        private readonly IPoojaBookingRepository _poojaBookingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRepository<Pooja> _poojaRepository;

        public PoojaBookingService(
            IPoojaBookingRepository poojaBookingRepository,
            IUserRepository userRepository,
            IRepository<Pooja> poojaRepository)
        {
            _poojaBookingRepository = poojaBookingRepository;
            _userRepository = userRepository;
            _poojaRepository = poojaRepository;
        }

        public async Task<PoojaBookingDto> CreateBookingAsync(CreatePoojaBookingDto createBookingDto)
        {
            // Validate that customer exists
            var customer = await _userRepository.GetByIdAsync(createBookingDto.UserId);
            if (customer == null || !customer.IsActive)
            {
                throw new KeyNotFoundException("Customer not found.");
            }

            // Validate that pooja exists
            var pooja = await _poojaRepository.GetByIdAsync(createBookingDto.PoojaId);
            if (pooja == null || !pooja.IsActive)
            {
                throw new KeyNotFoundException("Pooja not found.");
            }

            // Validate staff if provided
            if (createBookingDto.StaffId.HasValue)
            {
                var staff = await _userRepository.GetByIdAsync(createBookingDto.StaffId.Value);
                if (staff == null || !staff.IsActive)
                {
                    throw new KeyNotFoundException("Staff member not found.");
                }
            }

            var booking = new PoojaBooking
            {
                UserId = createBookingDto.UserId,
                PoojaId = createBookingDto.PoojaId,
                ScheduledDate = createBookingDto.ScheduledDate,
                StaffId = createBookingDto.StaffId,
                Amount = pooja.Price,
                Status = BookingStatus.Pending
            };

            await _poojaBookingRepository.AddAsync(booking);

            return await GetBookingByIdAsync(booking.Id) ?? throw new InvalidOperationException("Failed to retrieve created booking.");
        }

        public async Task<PoojaBookingDto?> GetBookingByIdAsync(int id)
        {
            var booking = await _poojaBookingRepository.GetWithDetailsAsync(id);

            return booking != null ? MapToDto(booking) : null;
        }

        public async Task<IEnumerable<PoojaBookingDto>> GetAllBookingsAsync()
        {
            var bookings = await _poojaBookingRepository.GetAllWithDetailsAsync();

            return bookings.Select(MapToDto);
        }

        public async Task<IEnumerable<PoojaBookingDto>> GetBookingsByCustomerAsync(int customerId)
        {
            var bookings = await _poojaBookingRepository.GetByCustomerAsync(customerId);

            return bookings.Select(MapToDto);
        }

        public async Task<IEnumerable<PoojaBookingDto>> GetBookingsByStaffAsync(int staffId)
        {
            var bookings = await _poojaBookingRepository.GetByStaffAsync(staffId);

            return bookings.Select(MapToDto);
        }

        public async Task<IEnumerable<PoojaBookingDto>> GetBookingsByStatusAsync(BookingStatus status)
        {
            var bookings = await _poojaBookingRepository.GetByStatusAsync(status);

            return bookings.Select(MapToDto);
        }

        public async Task<IEnumerable<PoojaBookingDto>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var bookings = await _poojaBookingRepository.GetByDateRangeAsync(startDate, endDate);

            return bookings.Select(MapToDto);
        }

        public async Task<PoojaBookingDto> UpdateBookingStatusAsync(int id, BookingStatus status)
        {
            var booking = await _poojaBookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                throw new KeyNotFoundException("Booking not found.");
            }

            booking.Status = status;
            await _poojaBookingRepository.UpdateAsync(booking);

            return await GetBookingByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated booking.");
        }

        public async Task<PoojaBookingDto> AssignStaffToBookingAsync(int id, int staffId)
        {
            var booking = await _poojaBookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                throw new KeyNotFoundException("Booking not found.");
            }

            var staff = await _userRepository.GetByIdAsync(staffId);
            if (staff == null || !staff.IsActive)
            {
                throw new KeyNotFoundException("Staff member not found.");
            }

            booking.StaffId = staffId;
            await _poojaBookingRepository.UpdateAsync(booking);

            return await GetBookingByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated booking.");
        }

        public async Task<bool> DeleteBookingAsync(int id)
        {
            return await _poojaBookingRepository.SoftDeleteAsync(id);
        }

        private static PoojaBookingDto MapToDto(PoojaBooking booking)
        {
            return new PoojaBookingDto
            {
                Id = booking.Id,
                UserId = booking.UserId,
                PoojaId = booking.PoojaId,
                BookingDate = booking.BookingDate,
                ScheduledDate = booking.ScheduledDate,
                StaffId = booking.StaffId,
                Amount = booking.Amount,
                Status = booking.Status,
                CustomerName = booking.Customer?.FullName ?? string.Empty,
                PoojaName = booking.Pooja?.Name ?? string.Empty,
                StaffName = booking.Staff?.FullName
            };
        }
    }
}
