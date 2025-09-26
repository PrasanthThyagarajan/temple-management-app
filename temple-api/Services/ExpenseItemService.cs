using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace TempleApi.Services
{
    public class EventExpenseService : IEventExpenseService
    {
        private readonly IRepository<EventExpense> _EventExpenseRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly ILogger<EventExpenseService> _logger;

        public EventExpenseService(
            IRepository<EventExpense> EventExpenseRepository,
            IRepository<Role> roleRepository,
            ILogger<EventExpenseService> logger)
        {
            _EventExpenseRepository = EventExpenseRepository;
            _roleRepository = roleRepository;
            _logger = logger;
        }

        public async Task<EventExpenseDto> CreateEventExpenseAsync(CreateEventExpenseDto createDto)
        {
            var EventExpense = new EventExpense
            {
                Name = createDto.Name,
                Description = createDto.Description,
                CreatedAt = DateTime.UtcNow,
                IsActive = createDto.IsActive,
                IsApprovalNeeded = createDto.IsApprovalNeeded,
                ApprovalRoleId = createDto.ApprovalRoleId
            };

            var createdItem = await _EventExpenseRepository.AddAsync(EventExpense);

            // Get role name if role is assigned
            string? roleName = null;
            if (createdItem.ApprovalRoleId.HasValue)
            {
                var role = await _roleRepository.GetByIdAsync(createdItem.ApprovalRoleId.Value);
                roleName = role?.RoleName;
            }

            return new EventExpenseDto
            {
                Id = createdItem.Id,
                Name = createdItem.Name,
                Description = createdItem.Description,
                CreatedAt = createdItem.CreatedAt,
                IsActive = createdItem.IsActive,
                IsApprovalNeeded = createdItem.IsApprovalNeeded,
                ApprovalRoleId = createdItem.ApprovalRoleId,
                ApprovalRoleName = roleName
            };
        }

        public async Task<IEnumerable<EventExpenseDto>> GetAllEventExpensesAsync()
        {
            var items = await _EventExpenseRepository.GetAllAsync();
            var roles = await _roleRepository.GetAllAsync();
            
            return items.Select(item => new EventExpenseDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                CreatedAt = item.CreatedAt,
                IsActive = item.IsActive,
                IsApprovalNeeded = item.IsApprovalNeeded,
                ApprovalRoleId = item.ApprovalRoleId,
                ApprovalRoleName = item.ApprovalRoleId.HasValue 
                    ? roles.FirstOrDefault(r => r.RoleId == item.ApprovalRoleId.Value)?.RoleName 
                    : null
            });
        }

        public async Task<EventExpenseDto?> GetEventExpenseByIdAsync(int id)
        {
            var item = await _EventExpenseRepository.GetByIdAsync(id);
            if (item == null) return null;

            // Get role name if role is assigned
            string? roleName = null;
            if (item.ApprovalRoleId.HasValue)
            {
                var role = await _roleRepository.GetByIdAsync(item.ApprovalRoleId.Value);
                roleName = role?.RoleName;
            }

            return new EventExpenseDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                CreatedAt = item.CreatedAt,
                IsActive = item.IsActive,
                IsApprovalNeeded = item.IsApprovalNeeded,
                ApprovalRoleId = item.ApprovalRoleId,
                ApprovalRoleName = roleName
            };
        }

        public async Task<EventExpenseDto> UpdateEventExpenseAsync(int id, UpdateEventExpenseDto updateDto)
        {
            var item = await _EventExpenseRepository.GetByIdAsync(id);
            if (item == null)
            {
                throw new ArgumentException("Expense item not found.");
            }

            item.Name = updateDto.Name;
            item.Description = updateDto.Description;
            item.IsActive = updateDto.IsActive;
            item.IsApprovalNeeded = updateDto.IsApprovalNeeded;
            item.ApprovalRoleId = updateDto.ApprovalRoleId;
            item.UpdatedAt = DateTime.UtcNow;

            await _EventExpenseRepository.UpdateAsync(item);

            // Get role name if role is assigned
            string? roleName = null;
            if (item.ApprovalRoleId.HasValue)
            {
                var role = await _roleRepository.GetByIdAsync(item.ApprovalRoleId.Value);
                roleName = role?.RoleName;
            }

            return new EventExpenseDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                CreatedAt = item.CreatedAt,
                IsActive = item.IsActive,
                IsApprovalNeeded = item.IsApprovalNeeded,
                ApprovalRoleId = item.ApprovalRoleId,
                ApprovalRoleName = roleName
            };
        }

        public async Task<bool> DeleteEventExpenseAsync(int id)
        {
            return await _EventExpenseRepository.DeleteByIdAsync(id);
        }
    }
}
