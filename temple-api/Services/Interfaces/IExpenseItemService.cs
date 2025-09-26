using TempleApi.Models.DTOs;

namespace TempleApi.Services.Interfaces
{
    public interface IEventExpenseService
    {
        Task<EventExpenseDto> CreateEventExpenseAsync(CreateEventExpenseDto createDto);
        Task<IEnumerable<EventExpenseDto>> GetAllEventExpensesAsync();
        Task<EventExpenseDto?> GetEventExpenseByIdAsync(int id);
        Task<EventExpenseDto> UpdateEventExpenseAsync(int id, UpdateEventExpenseDto updateDto);
        Task<bool> DeleteEventExpenseAsync(int id);
    }
}
