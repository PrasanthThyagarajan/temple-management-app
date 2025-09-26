using Xunit;
using Moq;
using TempleApi.Services;
using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using Microsoft.Extensions.Logging;

namespace TempleApi.Tests
{
    public class EventExpenseServiceTests
    {
        private readonly Mock<IRepository<EventExpense>> _EventExpenseRepositoryMock;
        private readonly Mock<IRepository<Role>> _roleRepositoryMock;
        private readonly Mock<ILogger<EventExpenseService>> _loggerMock;
        private readonly EventExpenseService _EventExpenseService;

        public EventExpenseServiceTests()
        {
            _EventExpenseRepositoryMock = new Mock<IRepository<EventExpense>>();
            _roleRepositoryMock = new Mock<IRepository<Role>>();
            _loggerMock = new Mock<ILogger<EventExpenseService>>();
            _EventExpenseService = new EventExpenseService(
                _EventExpenseRepositoryMock.Object,
                _roleRepositoryMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task CreateEventExpenseAsync_ShouldCreateEventExpense_WhenValidData()
        {
            // Arrange
            var createDto = new CreateEventExpenseDto
            {
                Name = "Test Expense Item",
                Description = "Test Description"
            };

            var createdItem = new EventExpense
            {
                Id = 1,
                Name = "Test Expense Item",
                Description = "Test Description",
                CreatedAt = DateTime.UtcNow
            };

            _EventExpenseRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<EventExpense>()))
                .ReturnsAsync(createdItem);

            // Act
            var result = await _EventExpenseService.CreateEventExpenseAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Expense Item", result.Name);
            Assert.Equal("Test Description", result.Description);
            _EventExpenseRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<EventExpense>()), Times.Once);
        }

        [Fact]
        public async Task GetAllEventExpensesAsync_ShouldReturnAllItems()
        {
            // Arrange
            var items = new List<EventExpense>
            {
                new EventExpense { Id = 1, Name = "Item 1", Description = "Description 1" },
                new EventExpense { Id = 2, Name = "Item 2", Description = "Description 2" }
            };

            _EventExpenseRepositoryMock.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(items);

            // Act
            var result = await _EventExpenseService.GetAllEventExpensesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetEventExpenseByIdAsync_ShouldReturnItem_WhenExists()
        {
            // Arrange
            var item = new EventExpense { Id = 1, Name = "Test Item", Description = "Test Description" };

            _EventExpenseRepositoryMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(item);

            // Act
            var result = await _EventExpenseService.GetEventExpenseByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Item", result.Name);
            Assert.Equal("Test Description", result.Description);
        }

        [Fact]
        public async Task UpdateEventExpenseAsync_ShouldUpdateItem_WhenExists()
        {
            // Arrange
            var existingItem = new EventExpense { Id = 1, Name = "Old Name", Description = "Test Description" };
            var updateDto = new UpdateEventExpenseDto
            {
                Name = "Updated Name",
                Description = "Updated Description"
            };

            _EventExpenseRepositoryMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(existingItem);

            _EventExpenseRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<EventExpense>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _EventExpenseService.UpdateEventExpenseAsync(1, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal("Updated Description", result.Description);
        }

        [Fact]
        public async Task DeleteEventExpenseAsync_ShouldReturnTrue_WhenExists()
        {
            // Arrange
            var item = new EventExpense { Id = 1, Name = "Test Item", Description = "Test Description" };

            _EventExpenseRepositoryMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(item);

            _EventExpenseRepositoryMock.Setup(repo => repo.DeleteByIdAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _EventExpenseService.DeleteEventExpenseAsync(1);

            // Assert
            Assert.True(result);
        }
    }
}
