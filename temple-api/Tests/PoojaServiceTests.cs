using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services;
using Xunit;

namespace TempleApi.Tests
{
    public class PoojaServiceTests : IDisposable
    {
        private readonly TempleDbContext _context;
        private readonly IRepository<Pooja> _poojaRepository;
        private readonly PoojaService _poojaService;

        public PoojaServiceTests()
        {
            var options = new DbContextOptionsBuilder<TempleDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TempleDbContext(options);
            _poojaRepository = new TempleApi.Repositories.Repository<Pooja>(_context);
            _poojaService = new PoojaService(_poojaRepository);
        }

        [Fact]
        public async Task CreatePoojaAsync_ShouldCreatePooja_WhenValidData()
        {
            // Arrange
            var createPoojaDto = new CreatePoojaDto
            {
                Name = "Ganesh Pooja",
                Description = "Traditional Ganesh worship ceremony",
                Price = 500.00m
            };

            // Act
            var result = await _poojaService.CreatePoojaAsync(createPoojaDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createPoojaDto.Name, result.Name);
            Assert.Equal(createPoojaDto.Description, result.Description);
            Assert.Equal(createPoojaDto.Price, result.Price);
            Assert.True(result.Id > 0);
        }

        [Fact]
        public async Task GetPoojaByIdAsync_ShouldReturnPooja_WhenPoojaExists()
        {
            // Arrange
            var pooja = new Pooja
            {
                Name = "Lakshmi Pooja",
                Description = "Goddess Lakshmi worship ceremony",
                Price = 750.00m
            };
            _context.Poojas.Add(pooja);
            await _context.SaveChangesAsync();

            // Act
            var result = await _poojaService.GetPoojaByIdAsync(pooja.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(pooja.Name, result.Name);
            Assert.Equal(pooja.Description, result.Description);
            Assert.Equal(pooja.Price, result.Price);
        }

        [Fact]
        public async Task GetPoojaByIdAsync_ShouldReturnNull_WhenPoojaDoesNotExist()
        {
            // Act
            var result = await _poojaService.GetPoojaByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllPoojasAsync_ShouldReturnAllActivePoojas()
        {
            // Arrange
            var poojas = new List<Pooja>
            {
                new Pooja { Name = "Pooja 1", Description = "Description 1", Price = 100.00m },
                new Pooja { Name = "Pooja 2", Description = "Description 2", Price = 200.00m },
                new Pooja { Name = "Pooja 3", Description = "Description 3", Price = 300.00m, IsActive = false }
            };
            _context.Poojas.AddRange(poojas);
            await _context.SaveChangesAsync();

            // Act
            var result = await _poojaService.GetAllPoojasAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task UpdatePoojaAsync_ShouldUpdatePooja_WhenPoojaExists()
        {
            // Arrange
            var pooja = new Pooja
            {
                Name = "Original Pooja",
                Description = "Original Description",
                Price = 400.00m
            };
            _context.Poojas.Add(pooja);
            await _context.SaveChangesAsync();

            var updateDto = new CreatePoojaDto
            {
                Name = "Updated Pooja",
                Description = "Updated Description",
                Price = 600.00m
            };

            // Act
            var result = await _poojaService.UpdatePoojaAsync(pooja.Id, updateDto);

            // Assert
            Assert.Equal(updateDto.Name, result.Name);
            Assert.Equal(updateDto.Description, result.Description);
            Assert.Equal(updateDto.Price, result.Price);
        }

        [Fact]
        public async Task UpdatePoojaAsync_ShouldThrowException_WhenPoojaDoesNotExist()
        {
            // Arrange
            var updateDto = new CreatePoojaDto
            {
                Name = "Updated Pooja",
                Description = "Updated Description",
                Price = 600.00m
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _poojaService.UpdatePoojaAsync(999, updateDto));
        }

        [Fact]
        public async Task DeletePoojaAsync_ShouldDeactivatePooja_WhenPoojaExists()
        {
            // Arrange
            var pooja = new Pooja
            {
                Name = "Test Pooja",
                Description = "Test Description",
                Price = 350.00m
            };
            _context.Poojas.Add(pooja);
            await _context.SaveChangesAsync();

            // Act
            var result = await _poojaService.DeletePoojaAsync(pooja.Id);

            // Assert
            Assert.True(result);
            var deletedPooja = await _context.Poojas.FindAsync(pooja.Id);
            Assert.False(deletedPooja!.IsActive);
        }

        [Fact]
        public async Task DeletePoojaAsync_ShouldReturnFalse_WhenPoojaDoesNotExist()
        {
            // Act
            var result = await _poojaService.DeletePoojaAsync(999);

            // Assert
            Assert.False(result);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
