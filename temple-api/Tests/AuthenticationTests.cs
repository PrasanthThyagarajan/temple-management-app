using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Security;
using TempleApi.Services;
using TempleApi.Services.Interfaces;
using Xunit;

namespace TempleApi.Tests
{
    public class AuthenticationTests
    {
        private TempleDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<TempleDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new TempleDbContext(options);
        }

        private IConfiguration CreateConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"Jwt:Key", "test-secret-key-for-testing-purposes-only-32chars"},
                    {"Jwt:Issuer", "TestIssuer"},
                    {"Jwt:Audience", "TestAudience"}
                })
                .Build();
            return config;
        }

        [Fact]
        public async Task BasicAuthenticationHandler_ShouldAuthenticate_WithValidCredentials()
        {
            // Arrange
            using var db = CreateDb();
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PhoneNumber = "1234567890",
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("password123")),
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);

            var role = new Role { RoleName = "Admin", IsActive = true, CreatedAt = DateTime.UtcNow };
            db.Roles.Add(role);
            await db.SaveChangesAsync();

            var userRole = new UserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            db.UserRoles.Add(userRole);
            await db.SaveChangesAsync();

            var handler = new BasicAuthenticationHandler(
                new OptionsMonitor<AuthenticationSchemeOptions>(),
                new LoggerFactory(),
                UrlEncoder.Default,
                db);

            var context = new DefaultHttpContext();
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser:password123"));
            context.Request.Headers["Authorization"] = $"Basic {authHeader}";

            await handler.InitializeAsync(new AuthenticationScheme("Basic", null, typeof(BasicAuthenticationHandler)), context);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Principal.Should().NotBeNull();
            result.Principal.Identity.Name.Should().Be("testuser");
            result.Principal.FindFirst(ClaimTypes.Email)?.Value.Should().Be("test@example.com");
            result.Principal.IsInRole("Admin").Should().BeTrue();
        }

        [Fact]
        public async Task BasicAuthenticationHandler_ShouldFail_WithInvalidPassword()
        {
            // Arrange
            using var db = CreateDb();
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("password123")),
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var handler = new BasicAuthenticationHandler(
                new OptionsMonitor<AuthenticationSchemeOptions>(),
                new LoggerFactory(),
                UrlEncoder.Default,
                db);

            var context = new DefaultHttpContext();
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser:wrongpassword"));
            context.Request.Headers["Authorization"] = $"Basic {authHeader}";

            await handler.InitializeAsync(new AuthenticationScheme("Basic", null, typeof(BasicAuthenticationHandler)), context);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Failure?.Message.Should().Be("Invalid username or password.");
        }

        [Fact]
        public async Task BasicAuthenticationHandler_ShouldFail_WithInactiveUser()
        {
            // Arrange
            using var db = CreateDb();
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("password123")),
                IsActive = false, // Inactive user
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var handler = new BasicAuthenticationHandler(
                new OptionsMonitor<AuthenticationSchemeOptions>(),
                new LoggerFactory(),
                UrlEncoder.Default,
                db);

            var context = new DefaultHttpContext();
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser:password123"));
            context.Request.Headers["Authorization"] = $"Basic {authHeader}";

            await handler.InitializeAsync(new AuthenticationScheme("Basic", null, typeof(BasicAuthenticationHandler)), context);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Failure?.Message.Should().Be("Account is deactivated.");
        }

        [Fact]
        public async Task BasicAuthenticationHandler_ShouldFail_WithUnverifiedUser()
        {
            // Arrange
            using var db = CreateDb();
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("password123")),
                IsActive = true,
                IsVerified = false, // Unverified user
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var handler = new BasicAuthenticationHandler(
                new OptionsMonitor<AuthenticationSchemeOptions>(),
                new LoggerFactory(),
                UrlEncoder.Default,
                db);

            var context = new DefaultHttpContext();
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser:password123"));
            context.Request.Headers["Authorization"] = $"Basic {authHeader}";

            await handler.InitializeAsync(new AuthenticationScheme("Basic", null, typeof(BasicAuthenticationHandler)), context);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Failure?.Message.Should().Be("Email not verified. Please check your email for the verification link.");
        }

        [Fact]
        public async Task BasicAuthenticationHandler_ShouldAuthenticate_WithEmail()
        {
            // Arrange
            using var db = CreateDb();
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("password123")),
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var handler = new BasicAuthenticationHandler(
                new OptionsMonitor<AuthenticationSchemeOptions>(),
                new LoggerFactory(),
                UrlEncoder.Default,
                db);

            var context = new DefaultHttpContext();
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes("test@example.com:password123"));
            context.Request.Headers["Authorization"] = $"Basic {authHeader}";

            await handler.InitializeAsync(new AuthenticationScheme("Basic", null, typeof(BasicAuthenticationHandler)), context);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Principal.Should().NotBeNull();
            result.Principal.FindFirst(ClaimTypes.Email)?.Value.Should().Be("test@example.com");
        }

        [Fact]
        public async Task AuthService_LoginAsync_ShouldReturnSuccess_WithValidCredentials()
        {
            // Arrange
            using var db = CreateDb();
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("password123")),
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);

            var role = new Role { RoleName = "Admin", IsActive = true, CreatedAt = DateTime.UtcNow };
            db.Roles.Add(role);
            await db.SaveChangesAsync();

            var userRole = new UserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            db.UserRoles.Add(userRole);
            await db.SaveChangesAsync();

            var config = CreateConfiguration();
            var authService = new AuthService(db, config, new NullLogger<AuthService>());

            // Act
            var result = await authService.LoginAsync(new LoginRequest
            {
                Username = "testuser",
                Password = "password123"
            });

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Login successful");
            result.User.Should().NotBeNull();
            result.User.Username.Should().Be("testuser");
            result.Roles.Should().Contain("Admin");
        }

        [Fact]
        public async Task AuthService_LoginAsync_ShouldFail_WithInvalidPassword()
        {
            // Arrange
            using var db = CreateDb();
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("password123")),
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var config = CreateConfiguration();
            var authService = new AuthService(db, config, new NullLogger<AuthService>());

            // Act
            var result = await authService.LoginAsync(new LoginRequest
            {
                Username = "testuser",
                Password = "wrongpassword"
            });

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid username or password");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task AuthService_LoginAsync_ShouldFail_WithUnverifiedUser()
        {
            // Arrange
            using var db = CreateDb();
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("password123")),
                IsActive = true,
                IsVerified = false, // Unverified user
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var config = CreateConfiguration();
            var authService = new AuthService(db, config, new NullLogger<AuthService>());

            // Act
            var result = await authService.LoginAsync(new LoginRequest
            {
                Username = "testuser",
                Password = "password123"
            });

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Please verify your email before logging in. Check your email for the verification link.");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task AuthService_LoginAsync_ShouldFail_WithInactiveUser()
        {
            // Arrange
            using var db = CreateDb();
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("password123")),
                IsActive = false, // Inactive user
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var config = CreateConfiguration();
            var authService = new AuthService(db, config, new NullLogger<AuthService>());

            // Act
            var result = await authService.LoginAsync(new LoginRequest
            {
                Username = "testuser",
                Password = "password123"
            });

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Your account has been deactivated. Please contact support.");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task AuthService_RegisterAsync_ShouldCreateNewUser()
        {
            // Arrange
            using var db = CreateDb();
            var role = new Role { RoleName = "General", IsActive = true, CreatedAt = DateTime.UtcNow };
            db.Roles.Add(role);
            await db.SaveChangesAsync();

            var config = CreateConfiguration();
            var authService = new AuthService(db, config, new NullLogger<AuthService>());

            // Act
            var result = await authService.RegisterAsync(new RegisterRequest
            {
                Email = "newuser@example.com",
                Password = "password123",
                ConfirmPassword = "password123",
                FullName = "New User",
                Gender = "Male",
                PhoneNumber = "+91 98765 43210",
                Nakshatra = "Ashwini",
                DateOfBirth = new DateTime(1990, 5, 15, 10, 30, 0)
            });

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Contain("Registration successful");
            result.User.Should().NotBeNull();
            result.User.Email.Should().Be("newuser@example.com");
            result.User.IsActive.Should().BeFalse(); // Should be inactive until verified
            result.User.Gender.Should().Be("Male");
            result.User.PhoneNumber.Should().Be("+91 98765 43210");
            result.User.Nakshatra.Should().Be("Ashwini");
            result.User.DateOfBirth.Should().Be(new DateTime(1990, 5, 15, 10, 30, 0));
            result.Roles.Should().Contain("General");

            var createdUser = await db.Users.FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
            createdUser.Should().NotBeNull();
            createdUser.IsVerified.Should().BeFalse();
            createdUser.VerificationCode.Should().NotBeNullOrEmpty();
            createdUser.Nakshatra.Should().Be("Ashwini");
            createdUser.DateOfBirth.Should().Be(new DateTime(1990, 5, 15, 10, 30, 0));

            // Check if General role was assigned
            var userRole = await db.UserRoles
                .Include(ur => ur.Role)
                .FirstOrDefaultAsync(ur => ur.UserId == createdUser.UserId);
            userRole.Should().NotBeNull();
            userRole.Role.RoleName.Should().Be("General");
        }

        [Fact]
        public async Task AuthService_RegisterAsync_ShouldHandleNullOptionalFields()
        {
            // Arrange
            using var db = CreateDb();
            var role = new Role { RoleName = "General", IsActive = true, CreatedAt = DateTime.UtcNow };
            db.Roles.Add(role);
            await db.SaveChangesAsync();

            var config = CreateConfiguration();
            var authService = new AuthService(db, config, new NullLogger<AuthService>());

            // Act
            var result = await authService.RegisterAsync(new RegisterRequest
            {
                Email = "newuser2@example.com",
                Password = "password123",
                ConfirmPassword = "password123",
                FullName = "New User 2",
                Nakshatra = null, // Optional field
                DateOfBirth = null // Optional field
            });

            // Assert
            result.Success.Should().BeTrue();
            result.User.Should().NotBeNull();
            result.User.Nakshatra.Should().BeNull();
            result.User.DateOfBirth.Should().BeNull();

            var createdUser = await db.Users.FirstOrDefaultAsync(u => u.Email == "newuser2@example.com");
            createdUser.Should().NotBeNull();
            createdUser.Nakshatra.Should().BeNull();
            createdUser.DateOfBirth.Should().BeNull();
        }

        [Fact]
        public async Task AuthService_RegisterAsync_ShouldFail_WithInvalidPhone()
        {
            // Arrange
            using var db = CreateDb();
            var role = new Role { RoleName = "General", IsActive = true, CreatedAt = DateTime.UtcNow };
            db.Roles.Add(role);
            await db.SaveChangesAsync();

            var config = CreateConfiguration();
            var authService = new AuthService(db, config, new NullLogger<AuthService>());

            // Act
            var result = await authService.RegisterAsync(new RegisterRequest
            {
                Email = "badphone@example.com",
                Password = "password123",
                ConfirmPassword = "password123",
                FullName = "Bad Phone",
                PhoneNumber = "abc-123-!!" // invalid characters
            });

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Invalid phone number format");
        }

        [Fact]
        public async Task AuthService_RegisterAsync_ShouldFail_WithTooLongPhone()
        {
            // Arrange
            using var db = CreateDb();
            var role = new Role { RoleName = "General", IsActive = true, CreatedAt = DateTime.UtcNow };
            db.Roles.Add(role);
            await db.SaveChangesAsync();

            var config = CreateConfiguration();
            var authService = new AuthService(db, config, new NullLogger<AuthService>());

            // 21 digits is too long per validation
            var longPhone = "+" + new string('9', 21);

            // Act
            var result = await authService.RegisterAsync(new RegisterRequest
            {
                Email = "longphone@example.com",
                Password = "password123",
                ConfirmPassword = "password123",
                FullName = "Long Phone",
                PhoneNumber = longPhone
            });

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Phone number must be <= 20 characters");
        }

        [Fact]
        public async Task AuthService_GetUserRolesAndPermissions_ShouldReturnCorrectData()
        {
            // Arrange
            using var db = CreateDb();
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("password123")),
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);

            var role = new Role { RoleName = "Admin", IsActive = true, CreatedAt = DateTime.UtcNow };
            db.Roles.Add(role);

            var permission = new PagePermission { PageName = "UserManagement", PageUrl = "/users", IsActive = true };
            db.PagePermissions.Add(permission);
            await db.SaveChangesAsync();

            var userRole = new UserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            db.UserRoles.Add(userRole);

            var rolePermission = new RolePermission
            {
                RoleId = role.RoleId,
                PagePermissionId = permission.PagePermissionId,
                CreatedAt = DateTime.UtcNow
            };
            db.RolePermissions.Add(rolePermission);
            await db.SaveChangesAsync();

            var config = CreateConfiguration();
            var authService = new AuthService(db, config, new NullLogger<AuthService>());

            // Act
            var (roles, permissions) = await authService.GetUserRolesAndPermissionsAsync(user.UserId);

            // Assert
            roles.Should().Contain("Admin");
            permissions.Should().Contain("UserManagement");
        }

        [Fact]
        public async Task AuthService_VerifyAsync_ShouldActivateUser()
        {
            // Arrange
            using var db = CreateDb();
            var verificationCode = "test-verification-code";
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("password123")),
                IsActive = false,
                IsVerified = false,
                VerificationCode = verificationCode,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var config = CreateConfiguration();
            var authService = new AuthService(db, config, new NullLogger<AuthService>());

            // Act
            var result = await authService.VerifyAsync(verificationCode);

            // Assert
            result.Should().BeTrue();

            var verifiedUser = await db.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
            verifiedUser.Should().NotBeNull();
            verifiedUser.IsActive.Should().BeTrue();
            verifiedUser.IsVerified.Should().BeTrue();
            verifiedUser.VerificationCode.Should().BeEmpty();
        }

        [Fact]
        public async Task AuthService_VerifyAsync_ShouldReturnFalse_WithInvalidCode()
        {
            // Arrange
            using var db = CreateDb();
            var config = CreateConfiguration();
            var authService = new AuthService(db, config, new NullLogger<AuthService>());

            // Act
            var result = await authService.VerifyAsync("invalid-code");

            // Assert
            result.Should().BeFalse();
        }

        private class OptionsMonitor<T> : IOptionsMonitor<T> where T : class, new()
        {
            public T CurrentValue => new T();
            public T Get(string name) => new T();
            public IDisposable OnChange(Action<T, string> listener) => new Disposable();
            
            private class Disposable : IDisposable
            {
                public void Dispose() { }
            }
        }
    }
}
