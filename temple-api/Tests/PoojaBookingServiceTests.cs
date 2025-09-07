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
                Name = "Customer",
                Email = "customer@example.com",
                Phone = "1234567890",
                Role = UserRole.Customer,
                PasswordHash = "hash"
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
                UserId = customer.Id,
                PoojaId = pooja.Id,
                ScheduledDate = DateTime.UtcNow.AddDays(7)
            };

            // Act
            var result = await _bookingService.CreateBookingAsync(createBookingDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customer.Id, result.UserId);
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
                Name = "Customer",
                Email = "customer@example.com",
                Phone = "1234567890",
                Role = UserRole.Customer,
                PasswordHash = "hash"
            };
            _context.Users.Add(customer);
            await _context.SaveChangesAsync();

            var createBookingDto = new CreatePoojaBookingDto
            {
                UserId = customer.Id,
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
                Name = "Customer",
                Email = "customer@example.com",
                Phone = "1234567890",
                Role = UserRole.Customer,
                PasswordHash = "hash"
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
                UserId = customer.Id,
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
            Assert.Equal(customer.Name, result.CustomerName);
            Assert.Equal(pooja.Name, result.PoojaName);
            Assert.Equal(BookingStatus.Confirmed, result.Status);
        }

        [Fact]
        public async Task UpdateBookingStatusAsync_ShouldUpdateStatus_WhenBookingExists()
        {
            // Arrange
            var customer = new User
            {
                Name = "Customer",
                Email = "customer@example.com",
                Phone = "1234567890",
                Role = UserRole.Customer,
                PasswordHash = "hash"
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
                UserId = customer.Id,
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
                Name = "Customer",
                Email = "customer@example.com",
                Phone = "1234567890",
                Role = UserRole.Customer,
                PasswordHash = "hash"
            };
            var staff = new User
            {
                Name = "Staff",
                Email = "staff@example.com",
                Phone = "9876543210",
                Role = UserRole.Staff,
                PasswordHash = "hash"
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
                UserId = customer.Id,
                PoojaId = pooja.Id,
                ScheduledDate = DateTime.UtcNow.AddDays(7),
                Amount = pooja.Price,
                Status = BookingStatus.Confirmed
            };
            _context.PoojaBookings.Add(booking);
            await _context.SaveChangesAsync();

            // Act
            var result = await _bookingService.AssignStaffToBookingAsync(booking.Id, staff.Id);

            // Assert
            Assert.Equal(staff.Id, result.StaffId);
            Assert.Equal(staff.Name, result.StaffName);
        }

        [Fact]
        public async Task DeleteBookingAsync_ShouldDeactivateBooking_WhenBookingExists()
        {
            // Arrange
            var customer = new User
            {
                Name = "Customer",
                Email = "customer@example.com",
                Phone = "1234567890",
                Role = UserRole.Customer,
                PasswordHash = "hash"
            };
            var pooja = new Pooja
            {
                Name = "Ganesh Pooja",
                Description = "Traditional Ganesh worship",
                Price = 500.00m
            };
            var booking = new PoojaBooking
            {
                UserId = customer.Id,
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
            var user = new User { Name = "U", Email = "u@x", Phone = "1", Role = UserRole.Customer, PasswordHash = "h" };
            var staff = new User { Name = "S", Email = "s@x", Phone = "2", Role = UserRole.Staff, PasswordHash = "h" };
            var pooja = new Pooja { Name = "P", Description = "D", Price = 100 };
            _context.Users.AddRange(user, staff);
            _context.Poojas.Add(pooja);
            await _context.SaveChangesAsync();

            _context.PoojaBookings.AddRange(
                new PoojaBooking { UserId = user.Id, PoojaId = pooja.Id, ScheduledDate = DateTime.UtcNow.AddDays(1), Amount = 100, Status = BookingStatus.Pending, IsActive = true },
                new PoojaBooking { UserId = user.Id, PoojaId = pooja.Id, ScheduledDate = DateTime.UtcNow.AddDays(2), Amount = 100, Status = BookingStatus.Confirmed, IsActive = false }
            );
            await _context.SaveChangesAsync();

            var result = await _bookingService.GetAllBookingsAsync();
            Assert.Single(result);
            Assert.Equal(BookingStatus.Pending, result.First().Status);
        }

        [Fact]
        public async Task GetBookingsByCustomerAsync_ShouldReturnCustomerBookings()
        {
            var u1 = new User { Name = "U1", Email = "u1@x", Phone = "1", Role = UserRole.Customer, PasswordHash = "h" };
            var u2 = new User { Name = "U2", Email = "u2@x", Phone = "2", Role = UserRole.Customer, PasswordHash = "h" };
            var pooja = new Pooja { Name = "P", Description = "D", Price = 100 };
            _context.Users.AddRange(u1, u2);
            _context.Poojas.Add(pooja);
            await _context.SaveChangesAsync();

            _context.PoojaBookings.AddRange(
                new PoojaBooking { UserId = u1.Id, PoojaId = pooja.Id, ScheduledDate = DateTime.UtcNow.AddDays(1), Amount = 100, Status = BookingStatus.Pending },
                new PoojaBooking { UserId = u2.Id, PoojaId = pooja.Id, ScheduledDate = DateTime.UtcNow.AddDays(1), Amount = 100, Status = BookingStatus.Pending }
            );
            await _context.SaveChangesAsync();

            var result = await _bookingService.GetBookingsByCustomerAsync(u1.Id);
            Assert.Single(result);
            Assert.All(result, b => Assert.Equal(u1.Id, b.UserId));
        }

        [Fact]
        public async Task GetBookingsByStaffAsync_ShouldReturnStaffBookings()
        {
            var user = new User { Name = "U", Email = "u@x", Phone = "1", Role = UserRole.Customer, PasswordHash = "h" };
            var staff1 = new User { Name = "S1", Email = "s1@x", Phone = "2", Role = UserRole.Staff, PasswordHash = "h" };
            var staff2 = new User { Name = "S2", Email = "s2@x", Phone = "3", Role = UserRole.Staff, PasswordHash = "h" };
            var pooja = new Pooja { Name = "P", Description = "D", Price = 100 };
            _context.Users.AddRange(user, staff1, staff2);
            _context.Poojas.Add(pooja);
            await _context.SaveChangesAsync();

            _context.PoojaBookings.AddRange(
                new PoojaBooking { UserId = user.Id, PoojaId = pooja.Id, StaffId = staff1.Id, ScheduledDate = DateTime.UtcNow.AddDays(1), Amount = 100, Status = BookingStatus.Pending },
                new PoojaBooking { UserId = user.Id, PoojaId = pooja.Id, StaffId = staff2.Id, ScheduledDate = DateTime.UtcNow.AddDays(1), Amount = 100, Status = BookingStatus.Pending }
            );
            await _context.SaveChangesAsync();

            var result = await _bookingService.GetBookingsByStaffAsync(staff1.Id);
            Assert.Single(result);
            Assert.All(result, b => Assert.Equal(staff1.Id, b.StaffId));
        }

        [Fact]
        public async Task GetBookingsByStatusAsync_ShouldReturnOnlySpecifiedStatus()
        {
            var user = new User { Name = "U", Email = "u@x", Phone = "1", Role = UserRole.Customer, PasswordHash = "h" };
            var pooja = new Pooja { Name = "P", Description = "D", Price = 100 };
            _context.Users.Add(user);
            _context.Poojas.Add(pooja);
            await _context.SaveChangesAsync();

            _context.PoojaBookings.AddRange(
                new PoojaBooking { UserId = user.Id, PoojaId = pooja.Id, ScheduledDate = DateTime.UtcNow.AddDays(1), Amount = 100, Status = BookingStatus.Pending },
                new PoojaBooking { UserId = user.Id, PoojaId = pooja.Id, ScheduledDate = DateTime.UtcNow.AddDays(2), Amount = 100, Status = BookingStatus.Confirmed }
            );
            await _context.SaveChangesAsync();

            var result = await _bookingService.GetBookingsByStatusAsync(BookingStatus.Confirmed);
            Assert.Single(result);
            Assert.Equal(BookingStatus.Confirmed, result.First().Status);
        }

        [Fact]
        public async Task GetBookingsByDateRangeAsync_ShouldReturnWithinRange()
        {
            var user = new User { Name = "U", Email = "u@x", Phone = "1", Role = UserRole.Customer, PasswordHash = "h" };
            var pooja = new Pooja { Name = "P", Description = "D", Price = 100 };
            _context.Users.Add(user);
            _context.Poojas.Add(pooja);
            await _context.SaveChangesAsync();

            var start = DateTime.UtcNow.AddDays(-1);
            var end = DateTime.UtcNow.AddDays(2);
            _context.PoojaBookings.AddRange(
                new PoojaBooking { UserId = user.Id, PoojaId = pooja.Id, ScheduledDate = DateTime.UtcNow, Amount = 100, Status = BookingStatus.Pending },
                new PoojaBooking { UserId = user.Id, PoojaId = pooja.Id, ScheduledDate = DateTime.UtcNow.AddDays(5), Amount = 100, Status = BookingStatus.Pending }
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
