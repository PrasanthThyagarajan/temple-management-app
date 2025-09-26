using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services.Interfaces;
using Microsoft.Extensions.Logging;
using TempleApi.Enums;
using Microsoft.EntityFrameworkCore;

namespace TempleApi.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly IRepository<Event> _eventRepository;
        private readonly IRepository<Expense> _ExpenseRepository;
        private readonly IRepository<EventExpense> _eventExpenseRepository;
        private readonly IRepository<TempleApi.Domain.Entities.ExpenseService> _expenseServiceRepository;
        private readonly ILogger<VoucherService> _logger;

        public VoucherService(
            IRepository<Event> eventRepository,
            IRepository<Expense> ExpenseRepository,
            IRepository<EventExpense> eventExpenseRepository,
            IRepository<TempleApi.Domain.Entities.ExpenseService> expenseServiceRepository,
            ILogger<VoucherService> logger)
        {
            _eventRepository = eventRepository;
            _ExpenseRepository = ExpenseRepository;
            _eventExpenseRepository = eventExpenseRepository;
            _expenseServiceRepository = expenseServiceRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<VoucherDto>> GetVouchersByEventAsync(int eventId)
        {
            var vouchers = await _ExpenseRepository.FindAsync(v => v.EventId == eventId && v.IsApprovalNeed);
            return await GetVoucherAsync(vouchers);
        }

        public async Task<IEnumerable<VoucherDto>> GetVouchersByExpenseAsync(int ExpenseId)
        {
            var vouchers = await _ExpenseRepository.FindAsync(v => v.Id == ExpenseId && v.IsApprovalNeed);
            return await GetVoucherAsync(vouchers);
        }

        public async Task<IEnumerable<VoucherDto>> GetAllVouchersAsync()
        {
            var vouchers = await _ExpenseRepository.FindAsync(v => v.IsApprovalNeed);
            return await GetVoucherAsync(vouchers);
        }

        private async Task<IEnumerable<VoucherDto>> GetVoucherAsync(IEnumerable<Expense> expenses)
        {
            var voucherDtos = new List<VoucherDto>();

            foreach (var v in expenses)
            {
                var voucherType = "";
                var approvalRoleId = 0;

                if (v.EventExpenseId > 0)
                {
                    voucherType = "Event";
                    // Load the EventExpense if not already loaded
                    if (v.EventExpense == null && v.EventExpenseId.HasValue)
                    {
                        var eventExpense = await _eventExpenseRepository.GetByIdAsync(v.EventExpenseId.Value);
                        approvalRoleId = eventExpense?.ApprovalRoleId ?? 0;
                    }
                    else
                    {
                        approvalRoleId = v.EventExpense?.ApprovalRoleId ?? 0;
                    }
                }
                else if (v.ExpenseServiceId > 0)
                {
                    voucherType = "Service";
                    // Load the ExpenseService if not already loaded
                    if (v.ExpenseService == null && v.ExpenseServiceId.HasValue)
                    {
                        var expenseService = await _expenseServiceRepository.GetByIdAsync(v.ExpenseServiceId.Value);
                        approvalRoleId = expenseService?.ApprovalRoleId ?? 0;
                    }
                    else
                    {
                        approvalRoleId = v.ExpenseService?.ApprovalRoleId ?? 0;
                    }
                }
                voucherDtos.Add(new VoucherDto
                {
                    Id = v.Id,
                    EventId = v.EventId,
                    ExpenseId = v.EventExpenseId ?? 0,
                    ServiceId = v.ExpenseServiceId ?? 0,
                    ApprovalRoleId = approvalRoleId,
                    ApprovedUserId = v.ApprovedBy,
                    IsApproved = v.IsApproved,
                    CreatedAt = v.CreatedAt,
                    VoucherType = voucherType
                });
            }

            return voucherDtos;
        }
    }
}
