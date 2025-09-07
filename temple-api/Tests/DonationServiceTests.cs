using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services;
using Xunit;
using FluentAssertions;

namespace TempleApi.Tests
{
	public class DonationServiceTests : IDisposable
	{
		private readonly TempleDbContext _context;
		private readonly DonationService _donationService;

		public DonationServiceTests()
		{
			var options = new DbContextOptionsBuilder<TempleDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			_context = new TempleDbContext(options);
			_donationService = new DonationService(_context);
		}

		#region Create Tests

		[Fact]
		public async Task CreateDonationAsync_ShouldCreateDonation_WithDevotee()
		{
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			var devotee = new Devotee { FirstName = "John", LastName = "Doe", TempleId = 0, IsActive = true };
			_context.Temples.Add(temple);
			await _context.SaveChangesAsync();
			devotee.TempleId = temple.Id;
			_context.Devotees.Add(devotee);
			await _context.SaveChangesAsync();

			var dto = new CreateDonationDto
			{
				TempleId = temple.Id,
				DevoteeId = devotee.Id,
				DonorName = "John Doe",
				Amount = 100,
				DonationType = "Cash",
				Purpose = "Maintenance",
				DonationDate = DateTime.UtcNow
			};

			var result = await _donationService.CreateDonationAsync(dto);

			result.Should().NotBeNull();
			result.TempleId.Should().Be(temple.Id);
			result.DevoteeId.Should().Be(devotee.Id);
			result.Amount.Should().Be(100);
			result.DonationType.Should().Be("Cash");
			result.Purpose.Should().Be("Maintenance");
		}

		[Fact]
		public async Task CreateDonationAsync_ShouldCreateDonation_Anonymous()
		{
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(temple);
			await _context.SaveChangesAsync();

			var dto = new CreateDonationDto
			{
				TempleId = temple.Id,
				DevoteeId = null,
				DonorName = "Anonymous",
				Amount = 50,
				DonationType = "Online"
			};

			var result = await _donationService.CreateDonationAsync(dto);
			result.DevoteeId.Should().BeNull();
			result.DonorName.Should().Be("Anonymous");
		}

		#endregion

		#region Read Tests

		[Fact]
		public async Task GetDonationByIdAsync_ShouldReturnDonation_WhenExists()
		{
			var temple = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(temple);
			await _context.SaveChangesAsync();
			var donation = new Donation { TempleId = temple.Id, DonorName = "D", Amount = 10, DonationType = "Cash", DonationDate = DateTime.UtcNow, IsActive = true };
			_context.Donations.Add(donation);
			await _context.SaveChangesAsync();

			var result = await _donationService.GetDonationByIdAsync(donation.Id);
			result.Should().NotBeNull();
			result!.TempleId.Should().Be(temple.Id);
		}

		[Fact]
		public async Task GetDonationByIdAsync_ShouldReturnNull_WhenNotFound()
		{
			var result = await _donationService.GetDonationByIdAsync(999);
			result.Should().BeNull();
		}

		[Fact]
		public async Task GetAllDonationsAsync_ShouldReturnOnlyActive()
		{
			var t = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(t);
			await _context.SaveChangesAsync();
			_context.Donations.AddRange(
				new Donation { TempleId = t.Id, DonorName = "A", Amount = 1, DonationType = "Cash", DonationDate = DateTime.UtcNow, IsActive = true },
				new Donation { TempleId = t.Id, DonorName = "B", Amount = 2, DonationType = "Cash", DonationDate = DateTime.UtcNow, IsActive = false }
			);
			await _context.SaveChangesAsync();

			var result = await _donationService.GetAllDonationsAsync();
			result.Should().HaveCount(1);
		}

		[Fact]
		public async Task GetDonationsByTempleAsync_ShouldFilterByTemple()
		{
			var t1 = new Temple { Name = "T1", Address = "A1", City = "C1", State = "S1", IsActive = true };
			var t2 = new Temple { Name = "T2", Address = "A2", City = "C2", State = "S2", IsActive = true };
			_context.Temples.AddRange(t1, t2);
			await _context.SaveChangesAsync();
			_context.Donations.AddRange(
				new Donation { TempleId = t1.Id, DonorName = "A", Amount = 1, DonationType = "Cash", DonationDate = DateTime.UtcNow, IsActive = true },
				new Donation { TempleId = t2.Id, DonorName = "B", Amount = 1, DonationType = "Cash", DonationDate = DateTime.UtcNow, IsActive = true }
			);
			await _context.SaveChangesAsync();

			var result = await _donationService.GetDonationsByTempleAsync(t1.Id);
			result.Should().HaveCount(1);
		}

		[Fact]
		public async Task GetDonationsByDevoteeAsync_ShouldFilterByDevotee()
		{
			var t = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(t);
			await _context.SaveChangesAsync();
			var d1 = new Devotee { FirstName = "A", LastName = "A", TempleId = t.Id, IsActive = true };
			var d2 = new Devotee { FirstName = "B", LastName = "B", TempleId = t.Id, IsActive = true };
			_context.Devotees.AddRange(d1, d2);
			await _context.SaveChangesAsync();
			_context.Donations.AddRange(
				new Donation { TempleId = t.Id, DevoteeId = d1.Id, DonorName = "A", Amount = 1, DonationType = "Cash", DonationDate = DateTime.UtcNow, IsActive = true },
				new Donation { TempleId = t.Id, DevoteeId = d2.Id, DonorName = "B", Amount = 1, DonationType = "Cash", DonationDate = DateTime.UtcNow, IsActive = true }
			);
			await _context.SaveChangesAsync();

			var result = await _donationService.GetDonationsByDevoteeAsync(d1.Id);
			result.Should().HaveCount(1);
		}

		#endregion

		#region Update/Delete/Aggregations

		[Fact]
		public async Task UpdateDonationStatusAsync_ShouldUpdateStatus_WhenExists()
		{
			var t = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(t);
			await _context.SaveChangesAsync();
			var donation = new Donation { TempleId = t.Id, DonorName = "D", Amount = 10, DonationType = "Cash", DonationDate = DateTime.UtcNow, Status = "Pending", IsActive = true };
			_context.Donations.Add(donation);
			await _context.SaveChangesAsync();

			var updated = await _donationService.UpdateDonationStatusAsync(donation.Id, "Completed");
			updated!.Status.Should().Be("Completed");
		}

		[Fact]
		public async Task DeleteDonationAsync_ShouldSoftDelete_WhenExists()
		{
			var t = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(t);
			await _context.SaveChangesAsync();
			var donation = new Donation { TempleId = t.Id, DonorName = "D", Amount = 10, DonationType = "Cash", DonationDate = DateTime.UtcNow, IsActive = true };
			_context.Donations.Add(donation);
			await _context.SaveChangesAsync();

			var ok = await _donationService.DeleteDonationAsync(donation.Id);
			ok.Should().BeTrue();
			var stored = await _context.Donations.FindAsync(donation.Id);
			stored!.IsActive.Should().BeFalse();
		}

		[Fact]
		public async Task GetTotalDonationsByTempleAsync_ShouldSumCompleted()
		{
			var t = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(t);
			await _context.SaveChangesAsync();
			_context.Donations.AddRange(
				new Donation { TempleId = t.Id, DonorName = "A", Amount = 10, DonationType = "Cash", DonationDate = DateTime.UtcNow, Status = "Completed", IsActive = true },
				new Donation { TempleId = t.Id, DonorName = "B", Amount = 5, DonationType = "Cash", DonationDate = DateTime.UtcNow, Status = "Pending", IsActive = true }
			);
			await _context.SaveChangesAsync();

			var sum = await _donationService.GetTotalDonationsByTempleAsync(t.Id);
			sum.Should().Be(10);
		}

		[Fact]
		public async Task GetTotalDonationsByDateRangeAsync_ShouldSumWithinRange()
		{
			var t = new Temple { Name = "T", Address = "A", City = "C", State = "S", IsActive = true };
			_context.Temples.Add(t);
			await _context.SaveChangesAsync();
			var start = DateTime.UtcNow.AddDays(-5);
			var end = DateTime.UtcNow.AddDays(1);
			_context.Donations.AddRange(
				new Donation { TempleId = t.Id, DonorName = "A", Amount = 10, DonationType = "Cash", DonationDate = DateTime.UtcNow.AddDays(-2), Status = "Completed", IsActive = true },
				new Donation { TempleId = t.Id, DonorName = "B", Amount = 5, DonationType = "Cash", DonationDate = DateTime.UtcNow.AddDays(-10), Status = "Completed", IsActive = true }
			);
			await _context.SaveChangesAsync();

			var sum = await _donationService.GetTotalDonationsByDateRangeAsync(t.Id, start, end);
			sum.Should().Be(10);
		}

		#endregion

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}