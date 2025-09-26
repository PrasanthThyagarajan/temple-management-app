using TempleApi.Models.DTOs;

namespace TempleApi.Services.Interfaces
{
    public interface IExpenseService
    {
        Task<ExpenseDto> CreateExpenseAsync(CreateExpenseDto createDto);
        Task<IEnumerable<ExpenseDto>> GetAllExpensesAsync();
        Task<ExpenseDto?> GetExpenseByIdAsync(int id);
        Task<ExpenseDto> UpdateExpenseAsync(int id, UpdateExpenseDto updateDto);
        Task<ExpenseDto> ApproveExpenseAsync(int id, int approvedByUserId);
        Task<bool> DeleteExpenseAsync(int id);
    }
}
