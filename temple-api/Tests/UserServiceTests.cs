using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services;
using Xunit;
using System.Threading.Tasks;
using System;

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
                Gender = "Male",
                PhoneNumber = "5551234567",
                Address = "123 Sample Street",
                Password = "password123",
                Nakshatra = "Ashwini",
                DateOfBirth = new DateTime(1990, 5, 15, 10, 30, 0)
            };

            // Act
            var result = await _userService.CreateUserAsync(createUserDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createUserDto.Name, result.FullName);
            Assert.Equal(createUserDto.Email, result.Email);
            Assert.Equal(createUserDto.Gender, result.Gender);
            Assert.Equal(createUserDto.PhoneNumber, result.PhoneNumber);
            Assert.Equal(createUserDto.Address, result.Address);
            Assert.Equal(createUserDto.Nakshatra, result.Nakshatra);
            Assert.Equal(createUserDto.DateOfBirth, result.DateOfBirth);
            Assert.True(result.UserId > 0);
            Assert.True(result.IsActive); // Admin-created users are active
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowException_WhenEmailExists()
        {
            // Arrange
            var existingUser = new User
            {
                Username = "existing",
                Email = "existing@example.com",
                FullName = "Existing User",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsVerified = true
            };

            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var createUserDto = new CreateUserDto
            {
                Name = "John Doe",
                Email = "existing@example.com",
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
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserByIdAsync(user.UserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.FullName, result.FullName);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.Username, result.Username);
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
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserByEmailAsync("test@example.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.FullName, result.FullName);
            Assert.Equal(user.Email, result.Email);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Act
            var result = await _userService.GetUserByEmailAsync("nonexistent@example.com");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnAllUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Username = "user1",
                    Email = "user1@example.com",
                    FullName = "User One",
                    PasswordHash = "hash1",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new User
                {
                    Username = "user2",
                    Email = "user2@example.com",
                    FullName = "User Two",
                    PasswordHash = "hash2",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            };

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateUser_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var updateDto = new CreateUserDto
            {
                Name = "Updated User",
                Email = "updated@example.com",
                Password = "newpassword123"
            };

            // Act
            var result = await _userService.UpdateUserAsync(user.UserId, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateDto.Name, result.FullName);
            Assert.Equal(updateDto.Email, result.Email);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldThrowException_WhenUserDoesNotExist()
        {
            // Arrange
            var updateDto = new CreateUserDto
            {
                Name = "Updated User",
                Email = "updated@example.com",
                Password = "newpassword123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.UpdateUserAsync(999, updateDto));
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldReturnTrue_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.DeleteUserAsync(user.UserId);

            // Assert
            Assert.True(result);
            
            // Verify user is soft deleted
            var deletedUser = await _context.Users.FindAsync(user.UserId);
            Assert.NotNull(deletedUser);
            Assert.False(deletedUser.IsActive);
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
            var password = "password123";
            
            // Create user with plain password - UserService will hash it
            var createUserDto = new CreateUserDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = password
            };
            
            var user = await _userService.CreateUserAsync(createUserDto);

            // Act
            var result = await _userService.ValidateUserCredentialsAsync("test@example.com", password);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateUserCredentialsAsync_ShouldReturnFalse_WhenCredentialsAreInvalid()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.ValidateUserCredentialsAsync("test@example.com", "wrongpassword");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateUserCredentialsAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Act
            var result = await _userService.ValidateUserCredentialsAsync("nonexistent@example.com", "password123");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldGenerateNewPassword_AndAllowLogin()
        {
            // Arrange
            var user = new User
            {
                Username = "resetuser",
                Email = "reset@example.com",
                FullName = "Reset User",
                PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("oldpass1")),
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsVerified = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var newPassword = await _userService.ResetPasswordAsync(user.UserId);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(newPassword));
            Assert.True(newPassword.Length >= 8);

            var updated = await _context.Users.FindAsync(user.UserId);
            Assert.NotNull(updated);
            Assert.NotEqual(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("oldpass1")), updated!.PasswordHash);

            // New password should validate
            var ok = await _userService.ValidateUserCredentialsAsync(user.Email, newPassword);
            Assert.True(ok);
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldThrow_WhenUserNotFound()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.ResetPasswordAsync(99999));
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}