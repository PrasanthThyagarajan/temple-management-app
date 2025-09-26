using TempleApi.Models.DTOs;

namespace TempleApi.Services.Interfaces
{
    public interface IVoucherService
    {
        Task<IEnumerable<VoucherDto>> GetVouchersByEventAsync(int eventId);
        Task<IEnumerable<VoucherDto>> GetVouchersByExpenseAsync(int ExpenseId);
        Task<IEnumerable<VoucherDto>> GetAllVouchersAsync();
    }
}
