using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Configuration;

namespace TempleApi.Tests
{
	public class DevoteeServiceTests : IDisposable
	{
		private readonly TempleDbContext _context;
		private readonly DevoteeService _devoteeService;

		public DevoteeServiceTests()
		{
			var options = new DbContextOptionsBuilder<TempleDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			_context = new TempleDbContext(options);
			var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()).Build();
			var logger = new NullLogger<DevoteeService>();
			_devoteeService = new DevoteeService(_context, config, logger);
		}

		#region Create Tests

		[Fact]
		public async Task CreateDevoteeAsync_ShouldCreateDevotee_WhenValidData()
		{
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(temple);
			await _context.SaveChangesAsync();

            var dto = new CreateDevoteeDto
            {
                Name = "John Doe",
                Email = "john@test.com",
                Phone = "123",
                Address = "Addr",
                City = "City",
                State = "State",
                PostalCode = "00001",
                DateOfBirth = DateTime.UtcNow.AddYears(-30),
                Gender = "Male",
                TempleId = temple.Id,
                UserId = 0
            };

			var result = await _devoteeService.CreateDevoteeAsync(dto);

            result.Should().NotBeNull();
            result.FullName.Should().Be(dto.Name);
			result.Email.Should().Be(dto.Email);
			result.Phone.Should().Be(dto.Phone);
			result.Address.Should().Be(dto.Address);
			result.City.Should().Be(dto.City);
			result.State.Should().Be(dto.State);
			result.PostalCode.Should().Be(dto.PostalCode);
			result.Gender.Should().Be(dto.Gender);
			result.TempleId.Should().Be(dto.TempleId);
		}

		[Fact]
		public async Task CreateDevoteeWithUserAsync_ShouldCreateUser_AndReturnPassword_WhenEmailPresent()
		{
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(temple);
			
			// Add General role for user assignment
			var generalRole = new Role { RoleName = "General", IsActive = true, CreatedAt = DateTime.UtcNow };
			_context.Roles.Add(generalRole);
			await _context.SaveChangesAsync();

            var dto = new CreateDevoteeDto
            {
                Name = "User FromDevotee",
                Email = "devoteeuser@test.com",
                TempleId = temple.Id
            };

			var (devotee, generatedPassword) = await _devoteeService.CreateDevoteeWithUserAsync(dto);

			devotee.Should().NotBeNull();
			generatedPassword.Should().NotBeNull();
			generatedPassword!.Length.Should().BeGreaterOrEqualTo(8);

			var createdUser = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
				.FirstOrDefaultAsync(u => u.Email == dto.Email);
			createdUser.Should().NotBeNull();
			createdUser!.FullName.Should().Be("User FromDevotee");
			
			// Check role assignment
			createdUser.UserRoles.Should().HaveCount(1);
			createdUser.UserRoles.First().Role.RoleName.Should().Be("General");
		}

		[Fact]
		public async Task CreateDevoteeAsync_ShouldHandleMinimalData()
		{
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(temple);
			await _context.SaveChangesAsync();

            var dto = new CreateDevoteeDto { Name = "Jane Smith", TempleId = temple.Id };
			var result = await _devoteeService.CreateDevoteeAsync(dto);

			result.Should().NotBeNull();
            result.FullName.Should().Be("Jane Smith");
		}

		[Fact]
		public async Task CreateDevoteeWithUserAsync_ShouldUseExistingUser_WhenUserIdProvided()
		{
			// Arrange
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(temple);
			
			var existingUser = new User
			{
				Username = "existinguser",
				Email = "existing@test.com",
				FullName = "Existing User",
				PhoneNumber = "9876543210",
				Address = "123 Main St",
				Gender = "Male",
				IsActive = true,
				IsVerified = true,
				PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("password123"))
			};
			_context.Users.Add(existingUser);
			await _context.SaveChangesAsync();

			var dto = new CreateDevoteeDto
			{
				UserId = existingUser.UserId,
				Name = "Devotee Name",
				Email = "existing@test.com", // Same email as user
				Phone = "9876543210",
				TempleId = temple.Id
			};

			// Act
			var (devotee, generatedPassword) = await _devoteeService.CreateDevoteeWithUserAsync(dto);

			// Assert
			devotee.Should().NotBeNull();
			devotee.UserId.Should().Be(existingUser.UserId);
			devotee.FullName.Should().Be("Devotee Name");
			generatedPassword.Should().BeNull(); // No password generated for existing user
		}

		[Fact]
		public async Task CreateDevoteeWithUserAsync_ShouldThrowException_WhenUserIdProvidedButUserNotFound()
		{
			// Arrange
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(temple);
			await _context.SaveChangesAsync();

			var dto = new CreateDevoteeDto
			{
				UserId = 999, // Non-existent user
				Name = "Devotee Name",
				Email = "test@test.com",
				TempleId = temple.Id
			};

			// Act & Assert
			var act = async () => await _devoteeService.CreateDevoteeWithUserAsync(dto);
			await act.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage("The specified user does not exist.");
		}

		[Fact]
		public async Task CreateDevoteeWithUserAsync_ShouldThrowException_WhenEmailDoesNotMatchUser()
		{
			// Arrange
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(temple);
			
			var existingUser = new User
			{
				Username = "existinguser",
				Email = "existing@test.com",
				FullName = "Existing User",
				IsActive = true,
				IsVerified = true,
				PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("password123"))
			};
			_context.Users.Add(existingUser);
			await _context.SaveChangesAsync();

			var dto = new CreateDevoteeDto
			{
				UserId = existingUser.UserId,
				Name = "Devotee Name",
				Email = "different@test.com", // Different email than user
				TempleId = temple.Id
			};

			// Act & Assert
			var act = async () => await _devoteeService.CreateDevoteeWithUserAsync(dto);
			await act.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage("The provided email does not match the selected user's email.");
		}

		[Fact]
		public async Task CreateDevoteeWithUserAsync_ShouldUseUserEmail_WhenNoEmailProvided()
		{
			// Arrange
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(temple);
			
			var existingUser = new User
			{
				Username = "existinguser",
				Email = "existing@test.com",
				FullName = "Existing User",
				IsActive = true,
				IsVerified = true,
				PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("password123"))
			};
			_context.Users.Add(existingUser);
			await _context.SaveChangesAsync();

			var dto = new CreateDevoteeDto
			{
				UserId = existingUser.UserId,
				Name = "Devotee Name",
				Email = null, // No email provided
				TempleId = temple.Id
			};

			// Act
			var (devotee, generatedPassword) = await _devoteeService.CreateDevoteeWithUserAsync(dto);

			// Assert
			devotee.Should().NotBeNull();
			devotee.Email.Should().Be("existing@test.com"); // Should use user's email
			generatedPassword.Should().BeNull();
		}

		[Fact]
		public async Task CreateDevoteeWithUserAsync_ShouldThrowException_WhenUserAlreadyLinkedToDevotee()
		{
			// Arrange
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(temple);
			
			var existingUser = new User
			{
				Username = "existinguser",
				Email = "existing@test.com",
				FullName = "Existing User",
				IsActive = true,
				IsVerified = true,
				PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("password123"))
			};
			_context.Users.Add(existingUser);
			await _context.SaveChangesAsync();

			// Create an existing devotee linked to this user
			var existingDevotee = new Devotee
			{
				FullName = "Existing Devotee",
				Email = "existing@test.com",
				Phone = "", Address = "", City = "", State = "", PostalCode = "", Gender = "",
				TempleId = temple.Id,
				UserId = existingUser.UserId,
				IsActive = true
			};
			_context.Devotees.Add(existingDevotee);
			await _context.SaveChangesAsync();

			var dto = new CreateDevoteeDto
			{
				UserId = existingUser.UserId,
				Name = "New Devotee",
				Email = "existing@test.com",
				TempleId = temple.Id
			};

			// Act & Assert
			var act = async () => await _devoteeService.CreateDevoteeWithUserAsync(dto);
			await act.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage($"This user is already registered as a devotee with ID {existingDevotee.Id}.");
		}

		[Fact]
		public async Task CreateDevoteeWithUserAsync_ShouldThrowException_WhenEmailUserAlreadyLinkedToDevotee()
		{
			// Arrange
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(temple);
			
			var existingUser = new User
			{
				Username = "existinguser",
				Email = "existing@test.com",
				FullName = "Existing User",
				IsActive = true,
				IsVerified = true,
				PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("password123"))
			};
			_context.Users.Add(existingUser);
			
			// Create devotee linked to user
			var existingDevotee = new Devotee
			{
				FullName = "Existing Devotee",
				Email = "existing@test.com",
				Phone = "", Address = "", City = "", State = "", PostalCode = "", Gender = "",
				TempleId = temple.Id,
				UserId = existingUser.UserId,
				IsActive = true
			};
			_context.Devotees.Add(existingDevotee);
			await _context.SaveChangesAsync();

			var dto = new CreateDevoteeDto
			{
				Name = "New Devotee",
				Email = "existing@test.com", // Email of user already linked
				TempleId = temple.Id
			};

			// Act & Assert
			var act = async () => await _devoteeService.CreateDevoteeWithUserAsync(dto);
			await act.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage("A user with email existing@test.com is already registered as a devotee.");
		}

		[Fact]
		public async Task CreateDevoteeWithUserAsync_ShouldThrowException_WhenUserIsInactive()
		{
			// Arrange
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(temple);
			
			var inactiveUser = new User
			{
				Username = "inactiveuser",
				Email = "inactive@test.com",
				FullName = "Inactive User",
				IsActive = false, // Inactive user
				IsVerified = true,
				PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("password123"))
			};
			_context.Users.Add(inactiveUser);
			await _context.SaveChangesAsync();

			var dto = new CreateDevoteeDto
			{
				UserId = inactiveUser.UserId,
				Name = "Devotee Name",
				Email = "inactive@test.com",
				TempleId = temple.Id
			};

			// Act & Assert
			var act = async () => await _devoteeService.CreateDevoteeWithUserAsync(dto);
			await act.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage("The specified user is not active.");
		}

		[Fact]
		public async Task CreateDevoteeWithUserAsync_ShouldThrowException_WhenNameIsEmpty()
		{
			// Arrange
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(temple);
			await _context.SaveChangesAsync();

			var dto = new CreateDevoteeDto
			{
				Name = "", // Empty name
				Email = "test@test.com",
				TempleId = temple.Id
			};

			// Act & Assert
			var act = async () => await _devoteeService.CreateDevoteeWithUserAsync(dto);
			await act.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage("Devotee name is required.");
		}

		[Fact]
		public async Task CreateDevoteeWithUserAsync_ShouldThrowException_WhenTempleDoesNotExist()
		{
			// Arrange
			var dto = new CreateDevoteeDto
			{
				Name = "Test Name",
				Email = "test@test.com",
				TempleId = 999 // Non-existent temple
			};

			// Act & Assert
			var act = async () => await _devoteeService.CreateDevoteeWithUserAsync(dto);
			await act.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage("The specified temple does not exist or is inactive.");
		}

		[Fact]
		public async Task CreateDevoteeWithUserAsync_ShouldThrowException_WhenTempleIsInactive()
		{
			// Arrange
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = false };
			_context.Temples.Add(temple);
			await _context.SaveChangesAsync();

			var dto = new CreateDevoteeDto
			{
				Name = "Test Name",
				Email = "test@test.com",
				TempleId = temple.Id
			};

			// Act & Assert
			var act = async () => await _devoteeService.CreateDevoteeWithUserAsync(dto);
			await act.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage("The specified temple does not exist or is inactive.");
		}

		[Fact]
		public async Task CreateDevoteeWithUserAsync_ShouldThrowException_WhenUserIsAdmin()
		{
			// Arrange
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(temple);
			
			var adminUser = new User
			{
				Username = "admin",
				Email = "admin@test.com",
				FullName = "Admin User",
				IsActive = true,
				IsVerified = true,
				PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("password123"))
			};
			_context.Users.Add(adminUser);
			
			var adminRole = new Role { RoleName = "Admin", IsActive = true };
			_context.Roles.Add(adminRole);
			await _context.SaveChangesAsync();
			
			var userRole = new UserRole
			{
				UserId = adminUser.UserId,
				RoleId = adminRole.RoleId,
				IsActive = true
			};
			_context.UserRoles.Add(userRole);
			await _context.SaveChangesAsync();

			var dto = new CreateDevoteeDto
			{
				UserId = adminUser.UserId,
				Name = "Admin Name",
				Email = "admin@test.com",
				TempleId = temple.Id
			};

			// Act & Assert
			var act = async () => await _devoteeService.CreateDevoteeWithUserAsync(dto);
			await act.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage("Admin users cannot be registered as devotees. Admins have full control without being devotees.");
		}

		[Fact]
		public async Task CreateDevoteeWithUserAsync_ShouldThrowException_WhenEmailBelongsToAdmin()
		{
			// Arrange
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(temple);
			
			var adminUser = new User
			{
				Username = "admin",
				Email = "admin@test.com",
				FullName = "Admin User",
				IsActive = true,
				IsVerified = true,
				PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("password123"))
			};
			_context.Users.Add(adminUser);
			
			var adminRole = new Role { RoleName = "Admin", IsActive = true };
			_context.Roles.Add(adminRole);
			await _context.SaveChangesAsync();
			
			var userRole = new UserRole
			{
				UserId = adminUser.UserId,
				RoleId = adminRole.RoleId,
				IsActive = true
			};
			_context.UserRoles.Add(userRole);
			await _context.SaveChangesAsync();

			var dto = new CreateDevoteeDto
			{
				Name = "Test Name",
				Email = "admin@test.com", // Email of admin user
				TempleId = temple.Id
			};

			// Act & Assert
			var act = async () => await _devoteeService.CreateDevoteeWithUserAsync(dto);
			await act.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage("The user with email admin@test.com is an admin. Admin users cannot be registered as devotees.");
		}

		#endregion

		#region Read Tests

		[Fact]
		public async Task GetDevoteeByIdAsync_ShouldReturnDevotee_WhenExists()
		{
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(temple);
			await _context.SaveChangesAsync();

            var devotee = new Devotee
            {
                FullName = "John Doe",
                Email = "john@test.com",
                Phone = "123",
                Address = "Addr",
                City = "City",
                State = "State",
                PostalCode = "00001",
                DateOfBirth = DateTime.UtcNow.AddYears(-30),
                Gender = "Male",
                TempleId = temple.Id,
                UserId = 0,
                IsActive = true
            };
			_context.Devotees.Add(devotee);
			await _context.SaveChangesAsync();

			var result = await _devoteeService.GetDevoteeByIdAsync(devotee.Id);

            result.Should().NotBeNull();
            result!.FullName.Should().Be("John Doe");
			result.Temple.Should().NotBeNull();
		}

		[Fact]
		public async Task GetDevoteeByIdAsync_ShouldReturnNull_WhenNotFound()
		{
			var result = await _devoteeService.GetDevoteeByIdAsync(999);
			result.Should().BeNull();
		}

		[Fact]
		public async Task GetAllDevoteesAsync_ShouldReturnOnlyActive()
		{
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(temple);
			await _context.SaveChangesAsync();

            _context.Devotees.AddRange(
                new Devotee { FullName = "A A", Email = "", Phone = "", Address = "", City = "", State = "", PostalCode = "", Gender = "", TempleId = temple.Id, UserId = 0, IsActive = true },
                new Devotee { FullName = "B B", Email = "", Phone = "", Address = "", City = "", State = "", PostalCode = "", Gender = "", TempleId = temple.Id, UserId = 0, IsActive = true },
                new Devotee { FullName = "C C", Email = "", Phone = "", Address = "", City = "", State = "", PostalCode = "", Gender = "", TempleId = temple.Id, UserId = 0, IsActive = false }
            );
			await _context.SaveChangesAsync();

			var result = await _devoteeService.GetAllDevoteesAsync();
			result.Should().HaveCount(2);
		}

		[Fact]
		public async Task GetDevoteesByTempleAsync_ShouldFilterByTemple()
		{
			var t1 = new Temple { Name = "T1", Address = "A1", City = "C1", State = "S1", IsActive = true };
			var t2 = new Temple { Name = "T2", Address = "A2", City = "C2", State = "S2", IsActive = true };
			_context.Temples.AddRange(t1, t2);
			await _context.SaveChangesAsync();

            _context.Devotees.AddRange(
                new Devotee { FullName = "A A", Email = "", Phone = "", Address = "", City = "", State = "", PostalCode = "", Gender = "", TempleId = t1.Id, UserId = 0, IsActive = true },
                new Devotee { FullName = "B B", Email = "", Phone = "", Address = "", City = "", State = "", PostalCode = "", Gender = "", TempleId = t1.Id, UserId = 0, IsActive = true },
                new Devotee { FullName = "C C", Email = "", Phone = "", Address = "", City = "", State = "", PostalCode = "", Gender = "", TempleId = t2.Id, UserId = 0, IsActive = true }
            );
			await _context.SaveChangesAsync();

			var result = await _devoteeService.GetDevoteesByTempleAsync(t1.Id);
			result.Should().HaveCount(2);
		}

		[Fact]
		public async Task SearchDevoteesAsync_ShouldSearchMultipleFields()
		{
			var t = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(t);
			await _context.SaveChangesAsync();

            _context.Devotees.AddRange(
                new Devotee { FullName = "John Doe", Email = "j@x.com", Phone = "111", Address = "", City = "X", State = "Y", PostalCode = "", Gender = "", TempleId = t.Id, UserId = 0, IsActive = true },
                new Devotee { FullName = "Jane Smith", Email = "js@x.com", Phone = "222", Address = "", City = "Z", State = "W", PostalCode = "", Gender = "", TempleId = t.Id, UserId = 0, IsActive = true }
            );
			await _context.SaveChangesAsync();

			var result = await _devoteeService.SearchDevoteesAsync("john");
			result.Should().HaveCount(1);
		}

		#endregion

		#region Update/Delete

		[Fact]
		public async Task UpdateDevoteeAsync_ShouldUpdate_WhenExists()
		{
			var t = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(t);
			await _context.SaveChangesAsync();

            var d = new Devotee { FullName = "Orig User", Email = "", Phone = "", Address = "", City = "", State = "", PostalCode = "", Gender = "", TempleId = t.Id, UserId = 0, IsActive = true };
			_context.Devotees.Add(d);
			await _context.SaveChangesAsync();

            var dto = new CreateDevoteeDto { Name = "Upd User", Email = "u@test.com", Phone = "9", Address = "NA", City = "C", State = "S", PostalCode = "1", TempleId = t.Id };
			var result = await _devoteeService.UpdateDevoteeAsync(d.Id, dto);

			result.Should().NotBeNull();
            result!.FullName.Should().Be("Upd User");
			result.Email.Should().Be("u@test.com");
		}

		[Fact]
		public async Task UpdateDevoteeAsync_ShouldReturnNull_WhenNotFound()
		{
            var dto = new CreateDevoteeDto { Name = "X Y", TempleId = 1 };
			var result = await _devoteeService.UpdateDevoteeAsync(999, dto);
			result.Should().BeNull();
		}

		[Fact]
		public async Task UpdateDevoteeAsync_ShouldThrowException_WhenNameIsEmpty()
		{
			// Arrange
			var t = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(t);
			var d = new Devotee { FullName = "Existing", Email = "", Phone = "", Address = "", City = "", State = "", PostalCode = "", Gender = "", TempleId = t.Id, UserId = 0, IsActive = true };
			_context.Devotees.Add(d);
			await _context.SaveChangesAsync();

			var dto = new CreateDevoteeDto { Name = "", TempleId = t.Id };

			// Act & Assert
			var act = async () => await _devoteeService.UpdateDevoteeAsync(d.Id, dto);
			await act.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage("Devotee name is required.");
		}

		[Fact]
		public async Task UpdateDevoteeAsync_ShouldThrowException_WhenTempleDoesNotExist()
		{
			// Arrange
			var t = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(t);
			var d = new Devotee { FullName = "Existing", Email = "", Phone = "", Address = "", City = "", State = "", PostalCode = "", Gender = "", TempleId = t.Id, UserId = 0, IsActive = true };
			_context.Devotees.Add(d);
			await _context.SaveChangesAsync();

			var dto = new CreateDevoteeDto { Name = "Updated", TempleId = 999 }; // Non-existent temple

			// Act & Assert
			var act = async () => await _devoteeService.UpdateDevoteeAsync(d.Id, dto);
			await act.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage("The specified temple does not exist or is inactive.");
		}

		[Fact]
		public async Task UpdateDevoteeAsync_ShouldThrowException_WhenEmailChangedToUserAlreadyLinked()
		{
			// Arrange
			var t = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(t);
			
			// Create a user and link to a devotee
			var user1 = new User
			{
				Username = "user1",
				Email = "user1@test.com",
				FullName = "User 1",
				IsActive = true,
				IsVerified = true,
				PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("password123"))
			};
			_context.Users.Add(user1);
			
			var devotee1 = new Devotee 
			{ 
				FullName = "Devotee 1", 
				Email = "user1@test.com", 
				Phone = "", Address = "", City = "", State = "", PostalCode = "", Gender = "", 
				TempleId = t.Id, 
				UserId = user1.UserId, 
				IsActive = true 
			};
			_context.Devotees.Add(devotee1);
			
			// Create another devotee to update
			var devotee2 = new Devotee 
			{ 
				FullName = "Devotee 2", 
				Email = "original@test.com", 
				Phone = "", Address = "", City = "", State = "", PostalCode = "", Gender = "", 
				TempleId = t.Id, 
				UserId = 0, 
				IsActive = true 
			};
			_context.Devotees.Add(devotee2);
			await _context.SaveChangesAsync();

			var dto = new CreateDevoteeDto 
			{ 
				Name = "Updated Devotee 2", 
				Email = "user1@test.com", // Trying to use email of user already linked
				TempleId = t.Id 
			};

			// Act & Assert
			var act = async () => await _devoteeService.UpdateDevoteeAsync(devotee2.Id, dto);
			await act.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage("A user with email user1@test.com is already registered as a devotee.");
		}

		[Fact]
		public async Task DeleteDevoteeAsync_ShouldSoftDelete_WhenExists()
		{
			var t = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(t);
			await _context.SaveChangesAsync();
            var d = new Devotee { FullName = "Del Me", Email = "", Phone = "", Address = "", City = "", State = "", PostalCode = "", Gender = "", TempleId = t.Id, UserId = 0, IsActive = true };
			_context.Devotees.Add(d);
			await _context.SaveChangesAsync();

			var ok = await _devoteeService.DeleteDevoteeAsync(d.Id);
			ok.Should().BeTrue();
			var stored = await _context.Devotees.FindAsync(d.Id);
			stored!.IsActive.Should().BeFalse();
		}

		#endregion

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}