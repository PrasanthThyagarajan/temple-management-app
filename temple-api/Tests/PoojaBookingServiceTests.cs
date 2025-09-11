using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using TempleApi.Enums;
using TempleApi.Models.DTOs;
using TempleApi.Services;
using Xunit;

namespace TempleApi.Tests
{
    public class PoojaBookingServiceTests : IDisposable
    {
        private readonly TempleDbContext _context;
        private readonly IPoojaBookingRepository _poojaBookingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRepository<Pooja> _poojaRepository;
        private readonly PoojaBookingService _bookingService;

        public PoojaBookingServiceTests()
        {
            var options = new DbContextOptionsBuilder<TempleDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TempleDbContext(options);
            _poojaBookingRepository = new TempleApi.Repositories.PoojaBookingRepository(_context);
            _userRepository = new TempleApi.Repositories.UserRepository(_context);
            _poojaRepository = new TempleApi.Repositories.Repository<Pooja>(_context);
            _bookingService = new PoojaBookingService(_poojaBookingRepository, _userRepository, _poojaRepository);
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldCreateBooking_WhenValidData()
        {
            // Arrange
            var customer = new User
            {
                Username = "customer",
                Email = "customer@example.com",
                FullName = "Customer",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            var pooja = new Pooja
            {
                Name = "Ganesh Pooja",
                Description = "Traditional Ganesh worship",
                Price = 500.00m
            };

            _context.Users.Add(customer);
            _context.Poojas.Add(pooja);
            await _context.SaveChangesAsync();

            var createBookingDto = new CreatePoojaBookingDto
            {
                UserId = customer.UserId,
                PoojaId = pooja.Id,
                ScheduledDate = DateTime.UtcNow.AddDays(7)
            };

            // Act
            var result = await _bookingService.CreateBookingAsync(createBookingDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customer.UserId, result.UserId);
            Assert.Equal(pooja.Id, result.PoojaId);
            Assert.Equal(pooja.Price, result.Amount);
            Assert.Equal(BookingStatus.Pending, result.Status);
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldThrowException_WhenCustomerDoesNotExist()
        {
            // Arrange
            var pooja = new Pooja
            {
                Name = "Ganesh Pooja",
                Description = "Traditional Ganesh worship",
                Price = 500.00m
            };
            _context.Poojas.Add(pooja);
            await _context.SaveChangesAsync();

            var createBookingDto = new CreatePoojaBookingDto
            {
                UserId = 999, // Non-existent customer
                PoojaId = pooja.Id,
                ScheduledDate = DateTime.UtcNow.AddDays(7)
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookingService.CreateBookingAsync(createBookingDto));
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldThrowException_WhenPoojaDoesNotExist()
        {
            // Arrange
            var customer = new User
            {
                Username = "customer",
                Email = "customer@example.com",
                FullName = "Customer",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Users.Add(customer);
            await _context.SaveChangesAsync();

            var createBookingDto = new CreatePoojaBookingDto
            {
                UserId = customer.UserId,
                PoojaId = 999, // Non-existent pooja
                ScheduledDate = DateTime.UtcNow.AddDays(7)
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookingService.CreateBookingAsync(createBookingDto));
        }

        [Fact]
        public async Task GetBookingByIdAsync_ShouldReturnBooking_WhenBookingExists()
        {
            // Arrange
            var customer = new User
            {
                Username = "customer",
                Email = "customer@example.com",
                FullName = "Customer",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            var pooja = new Pooja
            {
                Name = "Lakshmi Pooja",
                Description = "Goddess Lakshmi worship",
                Price = 750.00m
            };

            _context.Users.Add(customer);
            _context.Poojas.Add(pooja);
            await _context.SaveChangesAsync();

            var booking = new PoojaBooking
            {
                UserId = customer.UserId,
                PoojaId = pooja.Id,
                ScheduledDate = DateTime.UtcNow.AddDays(7),
                Amount = pooja.Price,
                Status = BookingStatus.Confirmed
            };
            _context.PoojaBookings.Add(booking);
            await _context.SaveChangesAsync();

            // Act
            var result = await _bookingService.GetBookingByIdAsync(booking.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(booking.Id, result.Id);
            Assert.Equal(customer.FullName, result.CustomerName);
            Assert.Equal(pooja.Name, result.PoojaName);
            Assert.Equal(BookingStatus.Confirmed, result.Status);
        }

        [Fact]
        public async Task UpdateBookingStatusAsync_ShouldUpdateStatus_WhenBookingExists()
        {
            // Arrange
            var customer = new User
            {
                Username = "customer",
                Email = "customer@example.com",
                FullName = "Customer",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            var pooja = new Pooja
            {
                Name = "Ganesh Pooja",
                Description = "Traditional Ganesh worship",
                Price = 500.00m
            };

            _context.Users.Add(customer);
            _context.Poojas.Add(pooja);
            await _context.SaveChangesAsync();

            var booking = new PoojaBooking
            {
                UserId = customer.UserId,
                PoojaId = pooja.Id,
                ScheduledDate = DateTime.UtcNow.AddDays(7),
                Amount = pooja.Price,
                Status = BookingStatus.Pending
            };
            _context.PoojaBookings.Add(booking);
            await _context.SaveChangesAsync();

            // Act
            var result = await _bookingService.UpdateBookingStatusAsync(booking.Id, BookingStatus.Confirmed);

            // Assert
            Assert.Equal(BookingStatus.Confirmed, result.Status);
        }

        [Fact]
        public async Task AssignStaffToBookingAsync_ShouldAssignStaff_WhenValidData()
        {
            // Arrange
            var customer = new User
            {
                Username = "customer",
                Email = "customer@example.com",
                FullName = "Customer",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            var staff = new User
            {
                Username = "staff",
                Email = "staff@example.com",
                FullName = "Staff",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            var pooja = new Pooja
            {
                Name = "Ganesh Pooja",
                Description = "Traditional Ganesh worship",
                Price = 500.00m
            };

            _context.Users.AddRange(customer, staff);
            _context.Poojas.Add(pooja);
            await _context.SaveChangesAsync();

            var booking = new PoojaBooking
            {
                UserId = customer.UserId,
                PoojaId = pooja.Id,
                ScheduledDate = DateTime.UtcNow.AddDays(7),
                Amount = pooja.Price,
                Status = BookingStatus.Confirmed
            };
            _context.PoojaBookings.Add(booking);
            await _context.SaveChangesAsync();

            // Act
            var result = await _bookingService.AssignStaffToBookingAsync(booking.Id, staff.UserId);

            // Assert
            Assert.Equal(staff.UserId, result.StaffId);
            Assert.Equal(staff.FullName, result.StaffName);
        }

        [Fact]
        public async Task DeleteBookingAsync_ShouldDeactivateBooking_WhenBookingExists()
        {
            // Arrange
            var customer = new User
            {
                Username = "customer",
                Email = "customer@example.com",
                FullName = "Customer",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            var pooja = new Pooja
            {
                Name = "Ganesh Pooja",
                Description = "Traditional Ganesh worship",
                Price = 500.00m
            };
            var booking = new PoojaBooking
            {
                UserId = customer.UserId,
                PoojaId = pooja.Id,
                ScheduledDate = DateTime.UtcNow.AddDays(7),
                Amount = pooja.Price,
                Status = BookingStatus.Pending
            };

            _context.Users.Add(customer);
            _context.Poojas.Add(pooja);
            _context.PoojaBookings.Add(booking);
            await _context.SaveChangesAsync();

            // Act
            var result = await _bookingService.DeleteBookingAsync(booking.Id);

            // Assert
            Assert.True(result);
            var deletedBooking = await _context.PoojaBookings.FindAsync(booking.Id);
            Assert.False(deletedBooking!.IsActive);
        }

        [Fact]
        public async Task GetAllBookingsAsync_ShouldReturnOnlyActiveBookings()
        {
            var user = new User { Username = "u", Email = "u@x", FullName = "U", PasswordHash = "h", CreatedAt = DateTime.UtcNow, IsActive = true };
            var staff = new User { Username = "s", Email = "s@x", FullName = "S", PasswordHash = "h", CreatedAt = DateTime.UtcNow, IsActive = true };
            var pooja = new Pooja { Name = "P", Description = "D", Price = 100 };
            _context.Users.AddRange(user, staff);
            _context.Poojas.Add(pooja);
            await _context.SaveChangesAsync();

            _context.PoojaBookings.AddRange(
                new PoojaBooking { UserId = user.UserId, PoojaId = pooja.Id, ScheduledDate = DateTime.UtcNow.AddDays(1), Amount = 100, Status = BookingStatus.Pending, IsActive = true },
                new PoojaBooking { UserId = user.UserId, PoojaId = pooja.Id, ScheduledDate = DateTime.UtcNow.AddDays(2), Amount = 100, Status = BookingStatus.Confirmed, IsActive = false }
            );
            await _context.SaveChangesAsync();

            var result = await _bookingService.GetAllBookingsAsync();
            Assert.Single(result);
            Assert.Equal(BookingStatus.Pending, result.First().Status);
        }

        [Fact]
        public async Task GetBookingsByCustomerAsync_ShouldReturnCustomerBookings()
        {
            var u1 = new User { Username = "u1", Email = "u1@x", FullName = "U1", PasswordHash = "h", CreatedAt = DateTime.UtcNow, IsActive = true };
            var u2 = new User { Username = "u2", Email = "u2@x", FullName = "U2", PasswordHash = "h", CreatedAt = DateTime.UtcNow, IsActive = true };
            var pooja = new Pooja { Name = "P", Description = "D", Price = 100 };
            _context.Users.AddRange(u1, u2);
            _context.Poojas.Add(pooja);
            await _context.SaveChangesAsync();

            _context.PoojaBookings.AddRange(
                new PoojaBooking { UserId = u1.UserId, PoojaId = pooja.Id, ScheduledDate = DateTime.UtcNow.AddDays(1), Amount = 100, Status = BookingStatus.Pending },
                new PoojaBooking { UserId = u2.UserId, PoojaId = pooja.Id, ScheduledDate = DateTime.UtcNow.AddDays(1), Amount = 100, Status = BookingStatus.Pending }
            );
            await _context.SaveChangesAsync();

            var result = await _bookingService.GetBookingsByCustomerAsync(u1.UserId);
            Assert.Single(result);
            Assert.All(result, b => Assert.Equal(u1.UserId, b.UserId));
        }

        [Fact]
        public async Task GetBookingsByStaffAsync_ShouldReturnStaffBookings()
        {
            var user = new User { Username = "u", Email = "u@x", FullName = "U", PasswordHash = "h", CreatedAt = DateTime.UtcNow, IsActive = true };
            var staff1 = new User { Username = "s1", Email = "s1@x", FullName = "S1", PasswordHash = "h", CreatedAt = DateTime.UtcNow, IsActive = true };
            var staff2 = new User { Username = "s2", Email = "s2@x", FullName = "S2", PasswordHash = "h", CreatedAt = DateTime.UtcNow, IsActive = true };
            var pooja = new Pooja { Name = "P", Description = "D", Price = 100 };
            _context.Users.AddRange(user, staff1, staff2);
            _context.Poojas.Add(pooja);
            await _context.SaveChangesAsync();

            _context.PoojaBookings.AddRange(
                new PoojaBooking { UserId = user.UserId, PoojaId = pooja.Id, StaffId = staff1.UserId, ScheduledDate = DateTime.UtcNow.AddDays(1), Amount = 100, Status = BookingStatus.Pending },
                new PoojaBooking { UserId = user.UserId, PoojaId = pooja.Id, StaffId = staff2.UserId, ScheduledDate = DateTime.UtcNow.AddDays(1), Amount = 100, Status = BookingStatus.Pending }
            );
            await _context.SaveChangesAsync();

            var result = await _bookingService.GetBookingsByStaffAsync(staff1.UserId);
            Assert.Single(result);
            Assert.All(result, b => Assert.Equal(staff1.UserId, b.StaffId));
        }

        [Fact]
        public async Task GetBookingsByStatusAsync_ShouldReturnOnlySpecifiedStatus()
        {
            var user = new User { Username = "u", Email = "u@x", FullName = "U", PasswordHash = "h", CreatedAt = DateTime.UtcNow, IsActive = true };
            var pooja = new Pooja { Name = "P", Description = "D", Price = 100 };
            _context.Users.Add(user);
            _context.Poojas.Add(pooja);
            await _context.SaveChangesAsync();

            _context.PoojaBookings.AddRange(
                new PoojaBooking { UserId = user.UserId, PoojaId = pooja.Id, ScheduledDate = DateTime.UtcNow.AddDays(1), Amount = 100, Status = BookingStatus.Pending },
                new PoojaBooking { UserId = user.UserId, PoojaId = pooja.Id, ScheduledDate = DateTime.UtcNow.AddDays(2), Amount = 100, Status = BookingStatus.Confirmed }
            );
            await _context.SaveChangesAsync();

            var result = await _bookingService.GetBookingsByStatusAsync(BookingStatus.Confirmed);
            Assert.Single(result);
            Assert.Equal(BookingStatus.Confirmed, result.First().Status);
        }

        [Fact]
        public async Task GetBookingsByDateRangeAsync_ShouldReturnWithinRange()
        {
            var user = new User { Username = "u", Email = "u@x", FullName = "U", PasswordHash = "h", CreatedAt = DateTime.UtcNow, IsActive = true };
            var pooja = new Pooja { Name = "P", Description = "D", Price = 100 };
            _context.Users.Add(user);
            _context.Poojas.Add(pooja);
            await _context.SaveChangesAsync();

            var start = DateTime.UtcNow.AddDays(-1);
            var end = DateTime.UtcNow.AddDays(2);
            _context.PoojaBookings.AddRange(
                new PoojaBooking { UserId = user.UserId, PoojaId = pooja.Id, ScheduledDate = DateTime.UtcNow, Amount = 100, Status = BookingStatus.Pending },
                new PoojaBooking { UserId = user.UserId, PoojaId = pooja.Id, ScheduledDate = DateTime.UtcNow.AddDays(5), Amount = 100, Status = BookingStatus.Pending }
            );
            await _context.SaveChangesAsync();

            var result = await _bookingService.GetBookingsByDateRangeAsync(start, end);
            Assert.Single(result);
        }

        [Fact]
        public async Task AssignStaffToBookingAsync_ShouldThrow_WhenBookingNotFound()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookingService.AssignStaffToBookingAsync(999, 1));
        }

        [Fact]
        public async Task UpdateBookingStatusAsync_ShouldThrow_WhenBookingNotFound()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookingService.UpdateBookingStatusAsync(999, BookingStatus.Cancelled));
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
