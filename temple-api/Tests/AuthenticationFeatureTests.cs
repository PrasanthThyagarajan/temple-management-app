using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Security;
using TempleApi.Services;
using Xunit;

namespace TempleApi.Tests
{
    public class AuthenticationFeatureTests : IDisposable
    {
        private readonly TempleDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthenticationFeatureTests()
        {
            var options = new DbContextOptionsBuilder<TempleDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TempleDbContext(options);
            
            var configData = new Dictionary<string, string?>
            {
                {"Jwt:Key", "test-secret-key-for-testing-purposes-only-32chars"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"}
            };
            
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();
                
            _logger = new NullLogger<AuthService>();
        }

        [Fact]
        public async Task Register_ShouldCreateUser_WithNakshatraAndDOB()
        {
            // Arrange
            var authService = new AuthService(_context, _configuration, _logger);
            
            // Create General role
            var generalRole = new Role 
            { 
                RoleName = "General", 
                IsActive = true, 
                CreatedAt = DateTime.UtcNow 
            };
            _context.Roles.Add(generalRole);
            await _context.SaveChangesAsync();

            var registerRequest = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "password123",
                ConfirmPassword = "password123",
                FullName = "Test User",
                Username = "testuser",
                Nakshatra = "Ashwini",
                DateOfBirth = new DateTime(1990, 5, 15, 10, 30, 0)
            };

            // Act
            var result = await authService.RegisterAsync(registerRequest);

            // Assert
            result.Success.Should().BeTrue();
            result.User.Should().NotBeNull();
            result.User!.Email.Should().Be("test@example.com");
            result.User.Nakshatra.Should().Be("Ashwini");
            result.User.DateOfBirth.Should().Be(new DateTime(1990, 5, 15, 10, 30, 0));
            result.User.IsActive.Should().BeFalse(); // Not active until verified
            result.Roles.Should().Contain("General");

            // Verify user in database
            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
            dbUser.Should().NotBeNull();
            dbUser!.IsVerified.Should().BeFalse();
            dbUser.VerificationCode.Should().NotBeNullOrEmpty();
            dbUser.Nakshatra.Should().Be("Ashwini");
            dbUser.DateOfBirth.Should().Be(new DateTime(1990, 5, 15, 10, 30, 0));

            // Verify role assignment
            var userRole = await _context.UserRoles
                .Include(ur => ur.Role)
                .FirstOrDefaultAsync(ur => ur.UserId == dbUser.UserId);
            userRole.Should().NotBeNull();
            userRole!.Role.RoleName.Should().Be("General");
        }

        [Fact]
        public async Task Register_ShouldHandleNull_NakshatraAndDOB()
        {
            // Arrange
            var authService = new AuthService(_context, _configuration, _logger);
            
            // Create General role
            var generalRole = new Role 
            { 
                RoleName = "General", 
                IsActive = true, 
                CreatedAt = DateTime.UtcNow 
            };
            _context.Roles.Add(generalRole);
            await _context.SaveChangesAsync();

            var registerRequest = new RegisterRequest
            {
                Email = "test2@example.com",
                Password = "password123",
                ConfirmPassword = "password123",
                FullName = "Test User 2",
                Username = "testuser2",
                Nakshatra = null,
                DateOfBirth = null
            };

            // Act
            var result = await authService.RegisterAsync(registerRequest);

            // Assert
            result.Success.Should().BeTrue();
            result.User.Should().NotBeNull();
            result.User!.Nakshatra.Should().BeNull();
            result.User.DateOfBirth.Should().BeNull();
        }

        [Fact]
        public async Task Login_ShouldFail_ForUnverifiedUser()
        {
            // Arrange
            var authService = new AuthService(_context, _configuration, _logger);
            
            var user = new User
            {
                Username = "unverified",
                Email = "unverified@example.com",
                FullName = "Unverified User",
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("password123")),
                IsActive = true,
                IsVerified = false,
                VerificationCode = "some-code",
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await authService.LoginAsync(new LoginRequest
            {
                Username = "unverified@example.com",
                Password = "password123"
            });

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Please verify your email");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task Login_ShouldFail_ForInactiveUser()
        {
            // Arrange
            var authService = new AuthService(_context, _configuration, _logger);
            
            var user = new User
            {
                Username = "inactive",
                Email = "inactive@example.com",
                FullName = "Inactive User",
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("password123")),
                IsActive = false,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await authService.LoginAsync(new LoginRequest
            {
                Username = "inactive@example.com",
                Password = "password123"
            });

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Your account has been deactivated");
            result.User.Should().BeNull();
        }

        [Fact]
        public async Task Login_ShouldSucceed_ForVerifiedUser_WithNakshatraAndDOB()
        {
            // Arrange
            var authService = new AuthService(_context, _configuration, _logger);
            
            var user = new User
            {
                Username = "verified",
                Email = "verified@example.com",
                FullName = "Verified User",
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("password123")),
                IsActive = true,
                IsVerified = true,
                Nakshatra = "Bharani",
                DateOfBirth = new DateTime(1985, 3, 20, 14, 45, 0),
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            
            var role = new Role { RoleName = "General", IsActive = true, CreatedAt = DateTime.UtcNow };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            
            var userRole = new UserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            // Act
            var result = await authService.LoginAsync(new LoginRequest
            {
                Username = "verified@example.com",
                Password = "password123"
            });

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Login successful");
            result.User.Should().NotBeNull();
            result.User!.Email.Should().Be("verified@example.com");
            result.User.Nakshatra.Should().Be("Bharani");
            result.User.DateOfBirth.Should().Be(new DateTime(1985, 3, 20, 14, 45, 0));
            result.Roles.Should().Contain("General");
        }

        [Fact]
        public async Task BasicAuth_ShouldFail_ForUnverifiedUser()
        {
            // Arrange
            var user = new User
            {
                Username = "basicunverified",
                Email = "basicunverified@example.com",
                FullName = "Basic Unverified User",
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("password123")),
                IsActive = true,
                IsVerified = false,
                VerificationCode = "some-code",
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var handler = new BasicAuthenticationHandler(
                new OptionsMonitor<AuthenticationSchemeOptions>(),
                new LoggerFactory(),
                UrlEncoder.Default,
                _context);

            var context = new DefaultHttpContext();
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes("basicunverified:password123"));
            context.Request.Headers["Authorization"] = $"Basic {authHeader}";

            await handler.InitializeAsync(new AuthenticationScheme("Basic", null, typeof(BasicAuthenticationHandler)), context);

            // Act
            var result = await handler.AuthenticateAsync();

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Failure?.Message.Should().Contain("Email not verified");
        }

        [Fact]
        public async Task Verify_ShouldActivateUser()
        {
            // Arrange
            var authService = new AuthService(_context, _configuration, _logger);
            var verificationCode = "test-verification-code";
            
            var user = new User
            {
                Username = "toverify",
                Email = "toverify@example.com",
                FullName = "To Verify User",
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("password123")),
                IsActive = false,
                IsVerified = false,
                VerificationCode = verificationCode,
                Nakshatra = "Krittika",
                DateOfBirth = new DateTime(1992, 7, 10, 8, 15, 0),
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await authService.VerifyAsync(verificationCode);

            // Assert
            result.Should().BeTrue();
            
            var verifiedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "toverify@example.com");
            verifiedUser.Should().NotBeNull();
            verifiedUser!.IsActive.Should().BeTrue();
            verifiedUser.IsVerified.Should().BeTrue();
            verifiedUser.VerificationCode.Should().BeEmpty();
            verifiedUser.Nakshatra.Should().Be("Krittika");
            verifiedUser.DateOfBirth.Should().Be(new DateTime(1992, 7, 10, 8, 15, 0));
        }

        [Fact]
        public async Task GetUserById_ShouldReturn_NakshatraAndDOB()
        {
            // Arrange
            var authService = new AuthService(_context, _configuration, _logger);
            
            var user = new User
            {
                Username = "getuser",
                Email = "getuser@example.com",
                FullName = "Get User",
                PasswordHash = "hash",
                IsActive = true,
                IsVerified = true,
                Nakshatra = "Rohini",
                DateOfBirth = new DateTime(1988, 12, 25, 6, 30, 0),
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await authService.GetUserByIdAsync(user.UserId);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be("getuser@example.com");
            result.Nakshatra.Should().Be("Rohini");
            result.DateOfBirth.Should().Be(new DateTime(1988, 12, 25, 6, 30, 0));
        }

        private class OptionsMonitor<T> : IOptionsMonitor<T> where T : class, new()
        {
            public T CurrentValue => new T();
            public T Get(string? name) => new T();
            public IDisposable OnChange(Action<T, string?> listener) => new Disposable();
            
            private class Disposable : IDisposable
            {
                public void Dispose() { }
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
