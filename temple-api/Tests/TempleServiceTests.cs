using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services;
using Xunit;
using FluentAssertions;

namespace TempleApi.Tests
{
	public class TempleServiceTests : IDisposable
	{
		private readonly TempleDbContext _context;
		private readonly TempleService _templeService;

		public TempleServiceTests()
		{
			var options = new DbContextOptionsBuilder<TempleDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			_context = new TempleDbContext(options);
			_templeService = new TempleService(_context);
		}

		#region Create Tests

		[Fact]
		public async Task CreateTempleAsync_ShouldCreateTemple_WhenValidData()
		{
			// Arrange
			var createTempleDto = new CreateTempleDto
			{
				Name = "Test Temple",
				Address = "123 Temple Street",
				City = "Test City",
				State = "Test State",
				PhoneNumber = "123-456-7890",
				Email = "test@temple.com",
				Description = "A beautiful test temple",
				Deity = "Lord Shiva",
				EstablishedDate = DateTime.UtcNow.AddYears(-50)
			};

			// Act
			var result = await _templeService.CreateTempleAsync(createTempleDto);

			// Assert
			result.Should().NotBeNull();
			result.Name.Should().Be(createTempleDto.Name);
			result.Address.Should().Be(createTempleDto.Address);
			result.City.Should().Be(createTempleDto.City);
			result.State.Should().Be(createTempleDto.State);
			result.Phone.Should().Be(createTempleDto.PhoneNumber);
			result.Email.Should().Be(createTempleDto.Email);
			result.Description.Should().Be(createTempleDto.Description);
			result.Deity.Should().Be(createTempleDto.Deity);
			result.EstablishedDate.Year.Should().Be(createTempleDto.EstablishedDate!.Value.Year);
			result.Id.Should().BeGreaterThan(0);
		}

		[Fact]
		public async Task CreateTempleAsync_ShouldSetDefaults_WhenOptionalFieldsMissing()
		{
			// Arrange
			var createTempleDto = new CreateTempleDto
			{
				Name = "Minimal Temple",
				Address = "456 Simple Street"
			};

			// Act
			var result = await _templeService.CreateTempleAsync(createTempleDto);

			// Assert
			result.Should().NotBeNull();
			result.Name.Should().Be(createTempleDto.Name);
			result.Address.Should().Be(createTempleDto.Address);
			result.City.Should().Be("");
			result.State.Should().Be("");
			result.Phone.Should().Be("");
			result.Email.Should().Be("");
			result.Description.Should().Be("");
			result.Deity.Should().Be("");
		}

		#endregion

		#region Read Tests

		[Fact]
		public async Task GetTempleByIdAsync_ShouldReturnTemple_WhenTempleExists()
		{
			// Arrange
			var temple = new Temple
			{
				Name = "Test Temple",
				Address = "123 Temple Street",
				City = "Test City",
				State = "Test State",
				PostalCode = "12345",
				Phone = "123-456-7890",
				Email = "test@temple.com",
				Description = "A beautiful test temple",
				Deity = "Lord Vishnu",
				EstablishedDate = DateTime.UtcNow.AddYears(-100),
				IsActive = true
			};
			_context.Temples.Add(temple);
			await _context.SaveChangesAsync();

			// Act
			var result = await _templeService.GetTempleByIdAsync(temple.Id);

			// Assert
			result.Should().NotBeNull();
			result!.Name.Should().Be(temple.Name);
			result.Address.Should().Be(temple.Address);
			result.City.Should().Be(temple.City);
			result.State.Should().Be(temple.State);
			result.PostalCode.Should().Be(temple.PostalCode);
			result.Phone.Should().Be(temple.Phone);
			result.Email.Should().Be(temple.Email);
			result.Description.Should().Be(temple.Description);
			result.Deity.Should().Be(temple.Deity);
		}

		[Fact]
		public async Task GetTempleByIdAsync_ShouldReturnNull_WhenTempleDoesNotExist()
		{
			// Act
			var result = await _templeService.GetTempleByIdAsync(999);

			// Assert
			result.Should().BeNull();
		}

		[Fact]
		public async Task GetAllTemplesAsync_ShouldReturnOnlyActiveTemples()
		{
			// Arrange
			var temples = new List<Temple>
			{
				new Temple { Name = "Temple 1", Address = "A1", City = "C1", State = "S1", IsActive = true },
				new Temple { Name = "Temple 2", Address = "A2", City = "C2", State = "S2", IsActive = true },
				new Temple { Name = "Temple 3", Address = "A3", City = "C3", State = "S3", IsActive = false }
			};
			_context.Temples.AddRange(temples);
			await _context.SaveChangesAsync();

			// Act
			var result = await _templeService.GetAllTemplesAsync();

			// Assert
			result.Should().HaveCount(2);
			result.Should().OnlyContain(t => t.IsActive);
		}

		[Fact]
		public async Task GetTemplesByLocationAsync_ShouldFilterByCityAndOptionalState()
		{
			// Arrange
			_context.Temples.AddRange(
				new Temple { Name = "T1", Address = "A1", City = "Mumbai", State = "MH", IsActive = true },
				new Temple { Name = "T2", Address = "A2", City = "Mumbai", State = "MH", IsActive = true },
				new Temple { Name = "T3", Address = "A3", City = "Mumbai", State = "GJ", IsActive = true },
				new Temple { Name = "T4", Address = "A4", City = "Delhi", State = "DL", IsActive = true }
			);
			await _context.SaveChangesAsync();

			// Act
			var onlyCity = await _templeService.GetTemplesByLocationAsync("Mumbai");
			var cityAndState = await _templeService.GetTemplesByLocationAsync("Mumbai", "MH");

			// Assert
			onlyCity.Should().HaveCount(3);
			cityAndState.Should().HaveCount(2);
			cityAndState.Should().OnlyContain(t => t.City == "Mumbai" && t.State == "MH");
		}

		[Fact]
		public async Task SearchTemplesAsync_ShouldMatchByNameCityStateOrDeity()
		{
			// Arrange
			_context.Temples.AddRange(
				new Temple { Name = "Krishna Temple", Address = "A1", City = "C1", State = "S1", Deity = "Krishna", IsActive = true },
				new Temple { Name = "Shiva Temple", Address = "A2", City = "C2", State = "S2", Deity = "Shiva", IsActive = true },
				new Temple { Name = "Mandir", Address = "A3", City = "Krishna Nagar", State = "S3", Deity = "Durga", IsActive = true }
			);
			await _context.SaveChangesAsync();

			// Act
			var byName = await _templeService.SearchTemplesAsync("Krishna");

			// Assert
			byName.Should().HaveCount(2);
		}

		#endregion

		#region Update Tests

		[Fact]
		public async Task UpdateTempleAsync_ShouldUpdate_WhenExists()
		{
			// Arrange
			var temple = new Temple
			{
				Name = "Original Temple",
				Address = "Original Address",
				City = "Original City",
				State = "Original State",
				IsActive = true
			};
			_context.Temples.Add(temple);
			await _context.SaveChangesAsync();

			var updateDto = new CreateTempleDto
			{
				Name = "Updated Temple",
				Address = "Updated Address",
				City = "Updated City",
				State = "Updated State",
				PhoneNumber = "999-999-9999",
				Email = "updated@temple.com",
				Description = "Updated description",
				Deity = "Ganesha"
			};

			// Act
			var result = await _templeService.UpdateTempleAsync(temple.Id, updateDto);

			// Assert
			result.Should().NotBeNull();
			result!.Name.Should().Be(updateDto.Name);
			result.Address.Should().Be(updateDto.Address);
			result.City.Should().Be(updateDto.City);
			result.State.Should().Be(updateDto.State);
			result.Phone.Should().Be(updateDto.PhoneNumber);
			result.Email.Should().Be(updateDto.Email);
			result.Description.Should().Be(updateDto.Description);
			result.Deity.Should().Be(updateDto.Deity);
		}

		[Fact]
		public async Task UpdateTempleAsync_ShouldReturnNull_WhenNotFound()
		{
			// Act
			var updateDto = new CreateTempleDto { Name = "X", Address = "Y" };
			var result = await _templeService.UpdateTempleAsync(999, updateDto);

			// Assert
			result.Should().BeNull();
		}

		#endregion

		#region Delete Tests

		[Fact]
		public async Task DeleteTempleAsync_ShouldSoftDelete_WhenExists()
		{
			// Arrange
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(temple);
			await _context.SaveChangesAsync();

			// Act
			var ok = await _templeService.DeleteTempleAsync(temple.Id);

			// Assert
			ok.Should().BeTrue();
			var stored = await _context.Temples.FindAsync(temple.Id);
			stored!.IsActive.Should().BeFalse();
		}

		[Fact]
		public async Task DeleteTempleAsync_ShouldReturnFalse_WhenNotFound()
		{
			var ok = await _templeService.DeleteTempleAsync(999);
			ok.Should().BeFalse();
		}

		[Fact]
		public async Task DeleteTempleAsync_ShouldReturnFalse_WhenAlreadyInactive()
		{
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = false };
			_context.Temples.Add(temple);
			await _context.SaveChangesAsync();

			var ok = await _templeService.DeleteTempleAsync(temple.Id);
			ok.Should().BeFalse();
		}

		#endregion

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}