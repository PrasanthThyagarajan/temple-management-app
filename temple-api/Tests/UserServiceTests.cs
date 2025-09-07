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
    public class UserServiceTests : IDisposable
    {
        private readonly TempleDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            var options = new DbContextOptionsBuilder<TempleDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TempleDbContext(options);
            _userRepository = new TempleApi.Repositories.UserRepository(_context);
            _userService = new UserService(_userRepository);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreateUser_WhenValidData()
        {
            // Arrange
            var createUserDto = new CreateUserDto
            {
                Name = "John Doe",
                Email = "john@example.com",
                Phone = "1234567890",
                Role = UserRole.Customer,
                Password = "password123"
            };

            // Act
            var result = await _userService.CreateUserAsync(createUserDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createUserDto.Name, result.Name);
            Assert.Equal(createUserDto.Email, result.Email);
            Assert.Equal(createUserDto.Phone, result.Phone);
            Assert.Equal(createUserDto.Role, result.Role);
            Assert.True(result.Id > 0);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowException_WhenEmailExists()
        {
            // Arrange
            var existingUser = new User
            {
                Name = "Existing User",
                Email = "existing@example.com",
                Phone = "1234567890",
                Role = UserRole.Customer,
                PasswordHash = "hash"
            };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var createUserDto = new CreateUserDto
            {
                Name = "New User",
                Email = "existing@example.com",
                Phone = "9876543210",
                Role = UserRole.Staff,
                Password = "password123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateUserAsync(createUserDto));
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                Name = "Test User",
                Email = "test@example.com",
                Phone = "1234567890",
                Role = UserRole.Admin,
                PasswordHash = "hash"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserByIdAsync(user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Name, result.Name);
            Assert.Equal(user.Email, result.Email);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Act
            var result = await _userService.GetUserByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                Name = "Test User",
                Email = "test@example.com",
                Phone = "1234567890",
                Role = UserRole.Customer,
                PasswordHash = "hash"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserByEmailAsync("test@example.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Email, result.Email);
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnAllActiveUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Name = "User 1", Email = "user1@example.com", Phone = "111", Role = UserRole.Customer, PasswordHash = "hash" },
                new User { Name = "User 2", Email = "user2@example.com", Phone = "222", Role = UserRole.Staff, PasswordHash = "hash" },
                new User { Name = "User 3", Email = "user3@example.com", Phone = "333", Role = UserRole.Admin, PasswordHash = "hash", IsActive = false }
            };
            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetUsersByRoleAsync_ShouldReturnUsersWithSpecificRole()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Name = "Customer 1", Email = "customer1@example.com", Phone = "111", Role = UserRole.Customer, PasswordHash = "hash" },
                new User { Name = "Customer 2", Email = "customer2@example.com", Phone = "222", Role = UserRole.Customer, PasswordHash = "hash" },
                new User { Name = "Staff 1", Email = "staff1@example.com", Phone = "333", Role = UserRole.Staff, PasswordHash = "hash" }
            };
            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUsersByRoleAsync(UserRole.Customer);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, u => Assert.Equal(UserRole.Customer, u.Role));
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateUser_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                Name = "Original Name",
                Email = "original@example.com",
                Phone = "1234567890",
                Role = UserRole.Customer,
                PasswordHash = "hash"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var updateDto = new CreateUserDto
            {
                Name = "Updated Name",
                Email = "updated@example.com",
                Phone = "9876543210",
                Role = UserRole.Staff,
                Password = "newpassword"
            };

            // Act
            var result = await _userService.UpdateUserAsync(user.Id, updateDto);

            // Assert
            Assert.Equal(updateDto.Name, result.Name);
            Assert.Equal(updateDto.Email, result.Email);
            Assert.Equal(updateDto.Phone, result.Phone);
            Assert.Equal(updateDto.Role, result.Role);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldThrowException_WhenUserDoesNotExist()
        {
            // Arrange
            var updateDto = new CreateUserDto
            {
                Name = "Updated Name",
                Email = "updated@example.com",
                Phone = "9876543210",
                Role = UserRole.Staff,
                Password = "newpassword"
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.UpdateUserAsync(999, updateDto));
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldDeactivateUser_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                Name = "Test User",
                Email = "test@example.com",
                Phone = "1234567890",
                Role = UserRole.Customer,
                PasswordHash = "hash"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.DeleteUserAsync(user.Id);

            // Assert
            Assert.True(result);
            var deletedUser = await _context.Users.FindAsync(user.Id);
            Assert.False(deletedUser!.IsActive);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Act
            var result = await _userService.DeleteUserAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateUserCredentialsAsync_ShouldReturnTrue_WhenCredentialsAreValid()
        {
            // Arrange
            var createUserDto = new CreateUserDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Phone = "1234567890",
                Role = UserRole.Customer,
                Password = "password123"
            };
            var createdUser = await _userService.CreateUserAsync(createUserDto);

            // Act
            var result = await _userService.ValidateUserCredentialsAsync("test@example.com", "password123");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateUserCredentialsAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Act
            var result = await _userService.ValidateUserCredentialsAsync("nonexistent@example.com", "password");

            // Assert
            Assert.False(result);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
