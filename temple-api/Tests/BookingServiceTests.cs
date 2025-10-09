using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Repositories;
using TempleApi.Repositories.Interfaces;
using TempleApi.Services;
using Xunit;

namespace TempleApi.Tests
{
    public class BookingServiceTests : IDisposable
    {
        private readonly TempleDbContext _context;
        private readonly IBookingRepository _bookingRepository;
        private readonly ISaleRepository _saleRepository;
        private readonly BookingService _bookingService;

        public BookingServiceTests()
        {
            var options = new DbContextOptionsBuilder<TempleDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TempleDbContext(options);
            _bookingRepository = new BookingRepository(_context);
            _saleRepository = new SaleRepository(_context);
            _bookingService = new BookingService(_bookingRepository, _saleRepository);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreatePendingBooking()
        {
            var dto = new CreateBookingDto
            {
                UserId = 1,
                StaffId = 2,
                BookingDate = DateTime.UtcNow,
                ProductId = 10,
                CategoryId = 20,
                EstimatedAmount = 100,
                PaymentMethod = "Cash",
                Notes = "Test"
            };

            var created = await _bookingService.CreateAsync(dto);

            Assert.NotNull(created);
            Assert.Equal("Pending", created.Status);
        }

        [Fact]
        public async Task RejectAsync_ShouldSetStatusRejected_WhenExists()
        {
            var booking = new Booking
            {
                UserId = 1,
                StaffId = 2,
                BookingDate = DateTime.UtcNow,
                ProductId = 10,
                CategoryId = 20,
                EstimatedAmount = 100,
                PaymentMethod = "Cash",
                Notes = "Test",
                Status = "Pending",
                IsActive = true
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var ok = await _bookingService.RejectAsync(booking.Id, approvedByUserId: 99);
            Assert.True(ok);

            var updated = await _context.Bookings.FindAsync(booking.Id);
            Assert.NotNull(updated);
            Assert.Equal("Rejected", updated!.Status);
            Assert.Equal(99, updated!.ApprovedBy);
            Assert.NotNull(updated!.ApprovedOn);
        }

        [Fact]
        public async Task DeleteAsync_ShouldSoftDeleteBooking()
        {
            var booking = new Booking
            {
                UserId = 1,
                StaffId = 2,
                BookingDate = DateTime.UtcNow,
                Status = "Pending",
                IsActive = true
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var ok = await _bookingService.DeleteAsync(booking.Id);
            Assert.True(ok);

            var deleted = await _context.Bookings.FindAsync(booking.Id);
            Assert.NotNull(deleted);
            Assert.False(deleted!.IsActive);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}


