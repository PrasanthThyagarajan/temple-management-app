using Xunit;
using Moq;
using TempleApi.Services;
using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using Microsoft.Extensions.Logging;
using TempleApi.Enums;

namespace TempleApi.Tests
{
    public class VoucherServiceTests
    {
        private readonly Mock<IRepository<Event>> _eventRepositoryMock;
        private readonly Mock<IRepository<Expense>> _ExpenseRepositoryMock;
        private readonly Mock<IRepository<EventExpense>> _eventExpenseRepositoryMock;
        private readonly Mock<IRepository<TempleApi.Domain.Entities.ExpenseService>> _expenseServiceRepositoryMock;
        private readonly Mock<ILogger<VoucherService>> _loggerMock;
        private readonly VoucherService _voucherService;

        public VoucherServiceTests()
        {
            _eventRepositoryMock = new Mock<IRepository<Event>>();
            _ExpenseRepositoryMock = new Mock<IRepository<Expense>>();
            _eventExpenseRepositoryMock = new Mock<IRepository<EventExpense>>();
            _expenseServiceRepositoryMock = new Mock<IRepository<TempleApi.Domain.Entities.ExpenseService>>();
            _loggerMock = new Mock<ILogger<VoucherService>>();
            _voucherService = new VoucherService(
                _eventRepositoryMock.Object,
                _ExpenseRepositoryMock.Object,
                _eventExpenseRepositoryMock.Object,
                _expenseServiceRepositoryMock.Object,
                _loggerMock.Object
            );
        }

        // Note: Voucher creation/approval is now handled via expenses approval.

        [Fact]
        public async Task GetVouchersByEventAsync_ShouldReturnVouchers_ForEvent()
        {
            // Arrange
            var expenses = new List<Expense>
            {
                new Expense { Id = 1, EventId = 1, EventExpenseId = 1, IsApprovalNeed = true },
                new Expense { Id = 2, EventId = 1, EventExpenseId = 2, IsApprovalNeed = true }
            };

            _ExpenseRepositoryMock.Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Expense, bool>>>() ))
                .ReturnsAsync(expenses.Where(e => e.EventId == 1 && e.IsApprovalNeed));

            // Act
            var result = await _voucherService.GetVouchersByEventAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, v => Assert.Equal(1, v.EventId));
        }

        [Fact]
        public async Task GetVouchersByExpenseAsync_ShouldReturnVouchers_ForExpense()
        {
            // Arrange
            var expenses = new List<Expense>
            {
                new Expense { Id = 1, EventId = 1, EventExpenseId = 1, IsApprovalNeed = true }
            };

            _ExpenseRepositoryMock.Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Expense, bool>>>() ))
                .ReturnsAsync(expenses.Where(e => e.Id == 1 && e.IsApprovalNeed));

            // Act
            var result = await _voucherService.GetVouchersByExpenseAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result.First().ExpenseId);
        }

        // Approval is validated via IExpenseService.ApproveExpenseAsync
    }
}
