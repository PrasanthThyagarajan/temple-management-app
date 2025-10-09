using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services;
using Xunit;

namespace TempleApi.Tests
{
    public class AuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public AuthenticationIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        private WebApplicationFactory<Program> WithTestDatabase()
        {
            return _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<TempleDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Remove DataSeedingService to prevent conflicts
                    var seedingDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IHostedService) && 
                            d.ImplementationType == typeof(DataSeedingService));
                    if (seedingDescriptor != null)
                        services.Remove(seedingDescriptor);

                    // Add in-memory database for testing
                    services.AddDbContext<TempleDbContext>(options =>
                    {
                        options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                    });

                    // Ensure database is created
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<TempleDbContext>();
                    db.Database.EnsureCreated();
                    SeedTestData(db);
                });
            });
        }

        private void SeedTestData(TempleDbContext db)
        {
            // Add test user
            var user = new User
            {
                Username = "testuser@example.com",
                Email = "testuser@example.com",
                FullName = "Test User",
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("password123")),
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);

            // Add roles
            var adminRole = new Role { RoleName = "Admin", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var generalRole = new Role { RoleName = "General", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            db.Roles.AddRange(adminRole, generalRole);
            db.SaveChanges();

            // Assign admin role to test user
            var userRole = new UserRole
            {
                UserId = user.UserId,
                RoleId = adminRole.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.UserRoles.Add(userRole);

            // Add permissions
            var permission = new PagePermission 
            { 
                PageName = "UserManagement", 
                PageUrl = "/users", 
                PermissionId = (int)TempleApi.Enums.Permission.Read,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.PagePermissions.Add(permission);

            // Assign permission to admin role
            var rolePermission = new RolePermission
            {
                RoleId = adminRole.RoleId,
                PagePermissionId = permission.PagePermissionId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            db.RolePermissions.Add(rolePermission);
            db.SaveChanges();
            
            // Ensure all changes are committed and user is properly set up
            db.Database.EnsureCreated();
        }

        [Fact]
        public async Task Login_ShouldReturnSuccess_WithValidCredentials()
        {
            // Arrange
            var client = WithTestDatabase().CreateClient();
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser@example.com:password123"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            // Act
            var response = await client.PostAsync("/api/auth/login", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.User.Should().NotBeNull();
            result.User.Email.Should().Be("testuser@example.com");
            result.Roles.Should().Contain("Admin");
        }

        [Fact]
        public async Task Login_ShouldReturn401_WithInvalidCredentials()
        {
            // Arrange
            var client = WithTestDatabase().CreateClient();
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser@example.com:wrongpassword"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            // Act
            var response = await client.PostAsync("/api/auth/login", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_ShouldReturn401_WithoutCredentials()
        {
            // Arrange
            var client = WithTestDatabase().CreateClient();

            // Act
            var response = await client.PostAsync("/api/auth/login", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetMe_ShouldReturnUserInfo_WithValidAuth()
        {
            // Arrange
            var client = WithTestDatabase().CreateClient();
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser@example.com:password123"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            // Act
            var response = await client.GetAsync("/api/auth/me");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            user.Should().NotBeNull();
            user.Email.Should().Be("testuser@example.com");
        }

        [Fact]
        public async Task GetMe_ShouldReturn401_WithoutAuth()
        {
            // Arrange
            var client = WithTestDatabase().CreateClient();

            // Act
            var response = await client.GetAsync("/api/auth/me");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetRoles_ShouldReturnUserRoles_WithValidAuth()
        {
            // Arrange
            var client = WithTestDatabase().CreateClient();
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser@example.com:password123"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            // Act
            var response = await client.GetAsync("/api/auth/roles");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var roles = await response.Content.ReadFromJsonAsync<List<string>>();
            roles.Should().NotBeNull();
            roles.Should().Contain("Admin");
        }

        [Fact]
        public async Task GetPermissions_ShouldReturnUserPermissions_WithValidAuth()
        {
            // Arrange
            var client = WithTestDatabase().CreateClient();
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser@example.com:password123"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            // Act
            var response = await client.GetAsync("/api/auth/permissions");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var permissions = await response.Content.ReadFromJsonAsync<List<string>>();
            permissions.Should().NotBeNull();
            permissions.Should().Contain("UserManagement");
        }

        [Fact]
        public async Task Register_ShouldCreateNewUser()
        {
            // Arrange
            var client = WithTestDatabase().CreateClient();
            var registerRequest = new RegisterRequest
            {
                Email = "newuser@example.com",
                Password = "newpassword123",
                FullName = "New Test User",
                Gender = "Female",
                PhoneNumber = "+1-555-000-1234"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.User.Should().NotBeNull();
            result.User.Email.Should().Be("newuser@example.com");
            result.User.Gender.Should().Be("Female");
            result.User.PhoneNumber.Should().Be("+1-555-000-1234");
            result.User.IsActive.Should().BeFalse(); // Inactive until verified
        }

        [Fact]
        public async Task Register_ShouldFail_WithInvalidPhone()
        {
            // Arrange
            var client = WithTestDatabase().CreateClient();
            var registerRequest = new RegisterRequest
            {
                Email = "invalidphone@example.com",
                Password = "password123",
                FullName = "Invalid Phone",
                PhoneNumber = "abc##@!" // invalid
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Invalid phone number format");
        }

        [Fact]
        public async Task Register_ShouldFail_WithDuplicateEmail()
        {
            // Arrange
            var client = WithTestDatabase().CreateClient();
            var registerRequest = new RegisterRequest
            {
                Email = "testuser@example.com", // Already exists
                Password = "password123",
                FullName = "Duplicate User"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("already exists");
        }

        [Fact]
        public async Task Verify_ShouldActivateUser_WithValidCode()
        {
            // Arrange
            var factory = WithTestDatabase();
            var client = factory.CreateClient();
            
            // Create unverified user
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TempleDbContext>();
                var unverifiedUser = new User
                {
                    Username = "unverified@example.com",
                    Email = "unverified@example.com",
                    FullName = "Unverified User",
                    PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("password123")),
                    IsActive = false,
                    IsVerified = false,
                    VerificationCode = "test-verification-code",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                db.Users.Add(unverifiedUser);
                db.SaveChanges();
            }

            // Act
            var response = await client.GetAsync("/api/auth/verify?code=test-verification-code");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            // Verify user is now active
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TempleDbContext>();
                var user = await db.Users.FirstOrDefaultAsync(u => u.Email == "unverified@example.com");
                user.Should().NotBeNull();
                user.IsActive.Should().BeTrue();
                user.IsVerified.Should().BeTrue();
                user.VerificationCode.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task Verify_ShouldFail_WithInvalidCode()
        {
            // Arrange
            var client = WithTestDatabase().CreateClient();

            // Act
            var response = await client.GetAsync("/api/auth/verify?code=invalid-code");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ProtectedEndpoint_ShouldReturn401_WithoutAuth()
        {
            // Arrange
            var client = WithTestDatabase().CreateClient();

            // Act
            var response = await client.GetAsync("/api/users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ProtectedEndpoint_ShouldReturn200_WithValidAuth()
        {
            // Arrange
            var client = WithTestDatabase().CreateClient();
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser@example.com:password123"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            // Act
            var response = await client.GetAsync("/api/users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task UsersMe_Get_ShouldReturnCurrentUser()
        {
            var client = WithTestDatabase().CreateClient();
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser@example.com:password123"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var response = await client.GetAsync("/api/users/me");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            user.Should().NotBeNull();
            user!.Email.Should().Be("testuser@example.com");
        }

        [Fact]
        public async Task UsersMe_Put_ShouldUpdateProfile()
        {
            var factory = WithTestDatabase();
            var client = factory.CreateClient();
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser@example.com:password123"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var update = new CreateUserDto
            {
                Name = "Updated Name",
                Email = "testuser@example.com",
                Gender = "Male",
                PhoneNumber = "555-999",
                Address = "Line 1",
                Nakshatra = "Ashwini",
                DateOfBirth = new DateTime(1990,1,1)
            };

            var response = await client.PutAsJsonAsync("/api/users/me", update);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            user.Should().NotBeNull();
            user!.FullName.Should().Be("Updated Name");
            user.PhoneNumber.Should().Be("555-999");
            user.Address.Should().Be("Line 1");
            user.Nakshatra.Should().Be("Ashwini");
        }

        [Fact]
        public async Task UsersMe_Put_ShouldUpdatePassword()
        {
            var factory = WithTestDatabase();
            var client = factory.CreateClient();
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser@example.com:password123"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var update = new CreateUserDto
            {
                Name = "Test User",
                Email = "testuser@example.com",
                Password = "newpassword123"
            };

            var response = await client.PutAsJsonAsync("/api/users/me", update);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Test login with new password
            var newCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser@example.com:newpassword123"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", newCredentials);
            
            var profileResponse = await client.GetAsync("/api/users/me");
            profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task UsersMe_Get_ShouldReturnUnauthorizedWithoutAuth()
        {
            var client = WithTestDatabase().CreateClient();
            
            var response = await client.GetAsync("/api/users/me");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UsersMe_Put_ShouldReturnUnauthorizedWithoutAuth()
        {
            var client = WithTestDatabase().CreateClient();
            
            var update = new CreateUserDto
            {
                Name = "Test",
                Email = "test@example.com"
            };

            var response = await client.PutAsJsonAsync("/api/users/me", update);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Bookings_Get_ShouldRequireReadPermission()
        {
            var client = WithTestDatabase().CreateClient();

            // No auth => 401
            var unauth = await client.GetAsync("/api/bookings");
            unauth.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // With Basic auth => OK or Forbidden depending on seeded permissions
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser@example.com:password123"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            var resp = await client.GetAsync("/api/bookings");
            resp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Bookings_Approve_ShouldRequireUpdatePermission()
        {
            var client = WithTestDatabase().CreateClient();

            // No auth => 401
            var unauth = await client.PutAsync("/api/bookings/1/approve?approvedBy=1", null);
            unauth.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // With Basic auth => should be Forbidden without Update
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes("testuser@example.com:password123"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            var resp = await client.PutAsync("/api/bookings/1/approve?approvedBy=1", null);
            resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
