using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Repositories;
using TempleApi.Services;
using Xunit;

namespace TempleApi.Tests
{
    public class UserManagementFeatureTests : IDisposable
    {
        private readonly TempleDbContext _context;
        private readonly UserRepository _userRepository;
        private readonly UserService _userService;

        public UserManagementFeatureTests()
        {
            var options = new DbContextOptionsBuilder<TempleDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TempleDbContext(options);
            _userRepository = new UserRepository(_context);
            _userService = new UserService(_userRepository);
        }

        [Fact]
        public async Task CreateUser_ShouldInclude_NakshatraAndDOB()
        {
            // Arrange
            var createUserDto = new CreateUserDto
            {
                Name = "Admin Created User",
                Email = "admin.created@example.com",
                Gender = "Female",
                PhoneNumber = "+44 7700 900123",
                Password = "password123",
                Nakshatra = "Mrigashira",
                DateOfBirth = new DateTime(1995, 8, 22, 15, 45, 30)
            };

            // Act
            var result = await _userService.CreateUserAsync(createUserDto);

            // Assert
            result.Should().NotBeNull();
            result.FullName.Should().Be("Admin Created User");
            result.Email.Should().Be("admin.created@example.com");
            result.Gender.Should().Be("Female");
            result.PhoneNumber.Should().Be("+44 7700 900123");
            result.Nakshatra.Should().Be("Mrigashira");
            result.DateOfBirth.Should().Be(new DateTime(1995, 8, 22, 15, 45, 30));
            result.IsActive.Should().BeTrue(); // Admin-created users are active and verified

            // Verify in database
            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin.created@example.com");
            dbUser.Should().NotBeNull();
            dbUser!.IsVerified.Should().BeTrue(); // Admin-created users are pre-verified
            dbUser.Nakshatra.Should().Be("Mrigashira");
            dbUser.DateOfBirth.Should().Be(new DateTime(1995, 8, 22, 15, 45, 30));
        }

        [Fact]
        public async Task CreateUser_ShouldHandleNull_NakshatraAndDOB()
        {
            // Arrange
            var createUserDto = new CreateUserDto
            {
                Name = "User Without Optional Fields",
                Email = "no.optional@example.com",
                Password = "password123",
                Nakshatra = null,
                DateOfBirth = null
            };

            // Act
            var result = await _userService.CreateUserAsync(createUserDto);

            // Assert
            result.Should().NotBeNull();
            result.Nakshatra.Should().BeNull();
            result.DateOfBirth.Should().BeNull();
        }

        [Fact]
        public async Task UpdateUser_ShouldUpdate_NakshatraAndDOB()
        {
            // Arrange
            var user = new User
            {
                Username = "updatetest",
                Email = "update@example.com",
                FullName = "Update Test User",
                PasswordHash = "hash",
                IsActive = true,
                IsVerified = true,
                Nakshatra = "Ardra",
                DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0),
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var updateDto = new CreateUserDto
            {
                Name = "Updated Name",
                Email = "update@example.com",
                Password = "", // No password change
                Nakshatra = "Punarvasu",
                DateOfBirth = new DateTime(1990, 1, 1, 12, 30, 0)
            };

            // Act
            var result = await _userService.UpdateUserAsync(user.UserId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.FullName.Should().Be("Updated Name");
            result.Nakshatra.Should().Be("Punarvasu");
            result.DateOfBirth.Should().Be(new DateTime(1990, 1, 1, 12, 30, 0));

            // Verify in database
            var dbUser = await _context.Users.FindAsync(user.UserId);
            dbUser.Should().NotBeNull();
            dbUser!.Nakshatra.Should().Be("Punarvasu");
            dbUser.DateOfBirth.Should().Be(new DateTime(1990, 1, 1, 12, 30, 0));
        }

        [Fact]
        public async Task UpdateUser_ShouldClear_NakshatraAndDOB_WhenNull()
        {
            // Arrange
            var user = new User
            {
                Username = "cleartest",
                Email = "clear@example.com",
                FullName = "Clear Test User",
                PasswordHash = "hash",
                IsActive = true,
                IsVerified = true,
                Nakshatra = "Pushya",
                DateOfBirth = new DateTime(1985, 6, 15, 9, 0, 0),
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var updateDto = new CreateUserDto
            {
                Name = "Clear Test User",
                Email = "clear@example.com",
                Password = "",
                Nakshatra = null,
                DateOfBirth = null
            };

            // Act
            var result = await _userService.UpdateUserAsync(user.UserId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Nakshatra.Should().BeNull();
            result.DateOfBirth.Should().BeNull();
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturn_UsersWithNakshatraAndDOB()
        {
            // Arrange
            var users = new[]
            {
                new User
                {
                    Username = "user1",
                    Email = "user1@example.com",
                    FullName = "User One",
                    PasswordHash = "hash1",
                    IsActive = true,
                    IsVerified = true,
                    Nakshatra = "Ashlesha",
                    DateOfBirth = new DateTime(1991, 2, 14, 10, 0, 0),
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "user2",
                    Email = "user2@example.com",
                    FullName = "User Two",
                    PasswordHash = "hash2",
                    IsActive = true,
                    IsVerified = true,
                    Nakshatra = null,
                    DateOfBirth = null,
                    CreatedAt = DateTime.UtcNow
                }
            };
            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            result.Should().HaveCount(2);
            
            var userList = result.ToList();
            var user1 = userList.FirstOrDefault(u => u.Email == "user1@example.com");
            user1.Should().NotBeNull();
            user1!.Nakshatra.Should().Be("Ashlesha");
            user1.DateOfBirth.Should().Be(new DateTime(1991, 2, 14, 10, 0, 0));
            
            var user2 = userList.FirstOrDefault(u => u.Email == "user2@example.com");
            user2.Should().NotBeNull();
            user2!.Nakshatra.Should().BeNull();
            user2.DateOfBirth.Should().BeNull();
        }

        [Theory]
        [InlineData("Ashwini", "1990-05-15 10:30:00")]
        [InlineData("Bharani", "1985-03-20 14:45:00")]
        [InlineData("Krittika", "1992-07-10 08:15:00")]
        [InlineData("Rohini", "1988-12-25 06:30:00")]
        public async Task CreateUser_ShouldAccept_VariousNakshatrasAndDates(string nakshatra, string dateTimeStr)
        {
            // Arrange
            var dateTime = DateTime.Parse(dateTimeStr);
            var createUserDto = new CreateUserDto
            {
                Name = $"User {nakshatra}",
                Email = $"{nakshatra.ToLower()}@example.com",
                Password = "password123",
                Nakshatra = nakshatra,
                DateOfBirth = dateTime
            };

            // Act
            var result = await _userService.CreateUserAsync(createUserDto);

            // Assert
            result.Should().NotBeNull();
            result.Nakshatra.Should().Be(nakshatra);
            result.DateOfBirth.Should().Be(dateTime);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
