using Xunit;
using Moq;
using TempleApi.Services;
using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace TempleApi.Tests
{
    public class ExpenseServiceTests
    {
        private readonly Mock<IRepository<Expense>> _ExpenseRepositoryMock;
        private readonly Mock<IRepository<EventExpense>> _EventExpenseRepositoryMock;
        private readonly Mock<IRepository<TempleApi.Domain.Entities.ExpenseService>> _ExpenseServiceRepositoryMock;
        private readonly Mock<IRepository<Event>> _EventRepositoryMock;
        private readonly Mock<IRepository<User>> _UserRepositoryMock;
        private readonly Mock<IRepository<UserRole>> _UserRoleRepositoryMock;
        private readonly Mock<IRepository<Role>> _RoleRepositoryMock;
        private readonly Mock<IRepository<EventApprovalRoleConfiguration>> _eventRoleCfgRepoMock;
        private readonly Mock<IRepository<ExpenseApprovalRoleConfiguration>> _ExpenseRoleCfgRepoMock;
        private readonly Mock<ILogger<TempleApi.Services.ExpenseService>> _loggerMock;
        private readonly TempleApi.Services.ExpenseService _ExpenseService;

        public ExpenseServiceTests()
        {
            _ExpenseRepositoryMock = new Mock<IRepository<Expense>>();
            _EventExpenseRepositoryMock = new Mock<IRepository<EventExpense>>();
            _ExpenseServiceRepositoryMock = new Mock<IRepository<TempleApi.Domain.Entities.ExpenseService>>();
            _EventRepositoryMock = new Mock<IRepository<Event>>();
            _UserRepositoryMock = new Mock<IRepository<User>>();
            _UserRoleRepositoryMock = new Mock<IRepository<UserRole>>();
            _RoleRepositoryMock = new Mock<IRepository<Role>>();
            _eventRoleCfgRepoMock = new Mock<IRepository<EventApprovalRoleConfiguration>>();
            _ExpenseRoleCfgRepoMock = new Mock<IRepository<ExpenseApprovalRoleConfiguration>>();
            _loggerMock = new Mock<ILogger<TempleApi.Services.ExpenseService>>();
            _ExpenseService = new TempleApi.Services.ExpenseService(
                _ExpenseRepositoryMock.Object,
                _EventExpenseRepositoryMock.Object,
                _ExpenseServiceRepositoryMock.Object,
                _EventRepositoryMock.Object,
                _UserRepositoryMock.Object,
                _UserRoleRepositoryMock.Object,
                _RoleRepositoryMock.Object,
                _eventRoleCfgRepoMock.Object,
                _ExpenseRoleCfgRepoMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task CreateExpenseAsync_ShouldCreateExpense_WhenValidData()
        {
            // Arrange
            var EventExpense = new EventExpense { Id = 1, Name = "Test Item" };
            var createDto = new CreateExpenseDto
            {
                EventExpenseId = 1,
                EventId = 1,
                Price = 100.00m
            };

            _EventExpenseRepositoryMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(EventExpense);

            var createdExpense = new Expense
            {
                Id = 1,
                EventExpenseId = 1,
                EventId = 1,
                Price = 100.00m,
                IsApprovalNeed = true,
                CreatedAt = DateTime.UtcNow
            };

            _ExpenseRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Expense>()))
                .ReturnsAsync(createdExpense);

            // Approval role mapping for item
            _ExpenseServiceRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((TempleApi.Domain.Entities.ExpenseService?)null);
            _EventRepositoryMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new Event { Id = 1, Name = "Test Event", IsApprovalNeeded = true });

            // Act
            var result = await _ExpenseService.CreateExpenseAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.EventExpenseId);
            Assert.Equal(1, result.EventId);
            Assert.Equal(100.00m, result.Price);
            Assert.True(result.IsApprovalNeed);
            _ExpenseRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Expense>()), Times.Once);
        }

        [Fact]
        public async Task CreateExpenseAsync_ShouldThrowException_WhenEventExpenseNotFound()
        {
            // Arrange
            var createDto = new CreateExpenseDto
            {
                EventExpenseId = 999,
                EventId = 1,
                Price = 50.00m
            };

            _EventExpenseRepositoryMock.Setup(repo => repo.GetByIdAsync(999))
                .ReturnsAsync((EventExpense?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _ExpenseService.CreateExpenseAsync(createDto));
        }

        [Fact]
        public async Task GetAllExpensesAsync_ShouldReturnAllExpenses()
        {
            // Arrange
            var Expenses = new List<Expense>
            {
                new Expense { Id = 1, EventExpenseId = 1, IsApprovalNeed = true },
                new Expense { Id = 2, EventExpenseId = 2, IsApprovalNeed = false }
            };

            _ExpenseRepositoryMock.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(Expenses);

            // Act
            var result = await _ExpenseService.GetAllExpensesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetExpenseByIdAsync_ShouldReturnExpense_WhenExists()
        {
            // Arrange
            var Expense = new Expense { Id = 1, EventExpenseId = 1, IsApprovalNeed = true };

            _ExpenseRepositoryMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(Expense);

            // Act
            var result = await _ExpenseService.GetExpenseByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.True(result.IsApprovalNeed);
        }

        [Fact]
        public async Task GetExpenseByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            _ExpenseRepositoryMock.Setup(repo => repo.GetByIdAsync(999))
                .ReturnsAsync((Expense?)null);

            // Act
            var result = await _ExpenseService.GetExpenseByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
    }
}
