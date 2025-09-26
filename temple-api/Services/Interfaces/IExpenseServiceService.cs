using TempleApi.Models.DTOs;

namespace TempleApi.Services.Interfaces
{
	public interface IExpenseServiceService
	{
		Task<ExpenseServiceDto> CreateExpenseServiceAsync(CreateExpenseServiceDto createDto);
		Task<IEnumerable<ExpenseServiceDto>> GetAllExpenseServicesAsync();
		Task<ExpenseServiceDto?> GetExpenseServiceByIdAsync(int id);
		Task<ExpenseServiceDto> UpdateExpenseServiceAsync(int id, UpdateExpenseServiceDto updateDto);
		Task<bool> DeleteExpenseServiceAsync(int id);
	}
}


