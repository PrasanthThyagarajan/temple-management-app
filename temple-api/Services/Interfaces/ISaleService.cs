using TempleApi.Models.DTOs;

namespace TempleApi.Services.Interfaces
{
    public interface ISaleService
    {
        Task<SaleDto> CreateSaleAsync(CreateSaleDto createSaleDto);
        Task<SaleDto?> GetSaleByIdAsync(int id);
        Task<IEnumerable<SaleDto>> GetAllSalesAsync();
        Task<IEnumerable<SaleDto>> GetSalesByCustomerAsync(int customerId);
        Task<IEnumerable<SaleDto>> GetSalesByStaffAsync(int staffId);
        Task<IEnumerable<SaleDto>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<SaleDto>> SearchSalesAsync(string searchTerm);
        Task<SaleDto> UpdateSaleAsync(int id, CreateSaleDto updateSaleDto);
        Task<bool> UpdateSaleStatusAsync(int id, string status);
        Task<bool> DeleteSaleAsync(int id);
    }
}
