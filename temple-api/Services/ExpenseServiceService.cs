using TempleApi.Repositories.Interfaces;
using TempleApi.Models.DTOs;
using TempleApi.Services.Interfaces;
using TempleApi.Domain.Entities;
using ExpenseServiceEntity = TempleApi.Domain.Entities.ExpenseService;

namespace TempleApi.Services
{
	public class ExpenseServiceService : IExpenseServiceService
	{
		private readonly IRepository<ExpenseServiceEntity> _expenseServiceRepository;
		private readonly IRepository<Role> _roleRepository;

		public ExpenseServiceService(IRepository<ExpenseServiceEntity> expenseServiceRepository, IRepository<Role> roleRepository)
		{
			_expenseServiceRepository = expenseServiceRepository;
			_roleRepository = roleRepository;
		}

		public async Task<ExpenseServiceDto> CreateExpenseServiceAsync(CreateExpenseServiceDto createDto)
		{
			var entity = new ExpenseServiceEntity
			{
				Name = createDto.Name,
				Description = createDto.Description,
				CreatedAt = DateTime.UtcNow,
				IsActive = createDto.IsActive,
				IsApprovalNeeded = createDto.IsApprovalNeeded,
				ApprovalRoleId = createDto.ApprovalRoleId
			};
			var created = await _expenseServiceRepository.AddAsync(entity);
			
			// Get role name if role is assigned
			string? roleName = null;
			if (created.ApprovalRoleId.HasValue)
			{
				var role = await _roleRepository.GetByIdAsync(created.ApprovalRoleId.Value);
				roleName = role?.RoleName;
			}
			
			return new ExpenseServiceDto
			{
				Id = created.Id,
				Name = created.Name,
				Description = created.Description,
				CreatedAt = created.CreatedAt,
				IsActive = created.IsActive,
				IsApprovalNeeded = created.IsApprovalNeeded,
				ApprovalRoleId = created.ApprovalRoleId,
				ApprovalRoleName = roleName
			};
		}

		public async Task<IEnumerable<ExpenseServiceDto>> GetAllExpenseServicesAsync()
		{
			var list = await _expenseServiceRepository.GetAllAsync();
			var roles = await _roleRepository.GetAllAsync();
			
			return list.Select(s => new ExpenseServiceDto
			{
				Id = s.Id,
				Name = s.Name,
				Description = s.Description,
				CreatedAt = s.CreatedAt,
				IsActive = s.IsActive,
				IsApprovalNeeded = s.IsApprovalNeeded,
				ApprovalRoleId = s.ApprovalRoleId,
				ApprovalRoleName = s.ApprovalRoleId.HasValue 
					? roles.FirstOrDefault(r => r.RoleId == s.ApprovalRoleId.Value)?.RoleName 
					: null
			});
		}

		public async Task<ExpenseServiceDto?> GetExpenseServiceByIdAsync(int id)
		{
			var s = await _expenseServiceRepository.GetByIdAsync(id);
			if (s == null) return null;
			
			// Get role name if role is assigned
			string? roleName = null;
			if (s.ApprovalRoleId.HasValue)
			{
				var role = await _roleRepository.GetByIdAsync(s.ApprovalRoleId.Value);
				roleName = role?.RoleName;
			}
			
			return new ExpenseServiceDto
			{
				Id = s.Id,
				Name = s.Name,
				Description = s.Description,
				CreatedAt = s.CreatedAt,
				IsActive = s.IsActive,
				IsApprovalNeeded = s.IsApprovalNeeded,
				ApprovalRoleId = s.ApprovalRoleId,
				ApprovalRoleName = roleName
			};
		}

		public async Task<ExpenseServiceDto> UpdateExpenseServiceAsync(int id, UpdateExpenseServiceDto updateDto)
		{
			var s = await _expenseServiceRepository.GetByIdAsync(id);
			if (s == null) throw new ArgumentException("Expense service not found.");
			s.Name = updateDto.Name;
			s.Description = updateDto.Description;
			s.IsActive = updateDto.IsActive;
			s.IsApprovalNeeded = updateDto.IsApprovalNeeded;
			s.ApprovalRoleId = updateDto.ApprovalRoleId;
			s.UpdatedAt = DateTime.UtcNow;
			await _expenseServiceRepository.UpdateAsync(s);
			// Get role name if role is assigned
			string? roleName = null;
			if (s.ApprovalRoleId.HasValue)
			{
				var role = await _roleRepository.GetByIdAsync(s.ApprovalRoleId.Value);
				roleName = role?.RoleName;
			}
			
			return new ExpenseServiceDto
			{
				Id = s.Id,
				Name = s.Name,
				Description = s.Description,
				CreatedAt = s.CreatedAt,
				IsActive = s.IsActive,
				IsApprovalNeeded = s.IsApprovalNeeded,
				ApprovalRoleId = s.ApprovalRoleId,
				ApprovalRoleName = roleName
			};
		}

		public Task<bool> DeleteExpenseServiceAsync(int id)
		{
			return _expenseServiceRepository.DeleteByIdAsync(id);
		}
	}
}


