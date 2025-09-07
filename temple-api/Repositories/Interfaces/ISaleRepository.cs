using TempleApi.Domain.Entities;

namespace TempleApi.Repositories.Interfaces
{
    public interface ISaleRepository : IRepository<Sale>
    {
        Task<IEnumerable<Sale>> GetByCustomerAsync(int customerId);
        Task<IEnumerable<Sale>> GetByStaffAsync(int staffId);
        Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Sale?> GetWithDetailsAsync(int id);
        Task<IEnumerable<Sale>> GetAllWithDetailsAsync();
    }
}
