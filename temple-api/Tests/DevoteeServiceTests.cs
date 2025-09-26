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
                TempleId = temple.Id
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

			var createdUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
			createdUser.Should().NotBeNull();
			createdUser!.FullName.Should().Be("User FromDevotee");
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
                new Devotee { FullName = "A A", TempleId = temple.Id, IsActive = true },
                new Devotee { FullName = "B B", TempleId = temple.Id, IsActive = true },
                new Devotee { FullName = "C C", TempleId = temple.Id, IsActive = false }
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
                new Devotee { FullName = "A A", TempleId = t1.Id, IsActive = true },
                new Devotee { FullName = "B B", TempleId = t1.Id, IsActive = true },
                new Devotee { FullName = "C C", TempleId = t2.Id, IsActive = true }
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
                new Devotee { FullName = "John Doe", Email = "j@x.com", Phone = "111", City = "X", State = "Y", TempleId = t.Id, IsActive = true },
                new Devotee { FullName = "Jane Smith", Email = "js@x.com", Phone = "222", City = "Z", State = "W", TempleId = t.Id, IsActive = true }
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

            var d = new Devotee { FullName = "Orig User", TempleId = t.Id, IsActive = true };
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
		public async Task DeleteDevoteeAsync_ShouldSoftDelete_WhenExists()
		{
			var t = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(t);
			await _context.SaveChangesAsync();
            var d = new Devotee { FullName = "Del Me", TempleId = t.Id, IsActive = true };
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