using TempleApi.Data;
using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;

namespace TempleApi.Repositories
{
    public class SaleRepository : Repository<Sale>, ISaleRepository
    {
        public SaleRepository(TempleDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Sale>> GetByCustomerAsync(int customerId)
        {
            return await FindAsync(s => s.UserId == customerId && s.IsActive, 
                s => s.Customer, s => s.Staff, s => s.SaleItems);
        }

        public async Task<IEnumerable<Sale>> GetByStaffAsync(int staffId)
        {
            return await FindAsync(s => s.StaffId == staffId && s.IsActive,
                s => s.Customer, s => s.Staff, s => s.SaleItems);
        }

        public async Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await FindAsync(s => s.SaleDate >= startDate && s.SaleDate <= endDate && s.IsActive,
                s => s.Customer, s => s.Staff, s => s.SaleItems);
        }

        public async Task<Sale?> GetWithDetailsAsync(int id)
        {
            return await GetByIdAsync(id, s => s.Customer, s => s.Staff, s => s.SaleItems);
        }

        public async Task<IEnumerable<Sale>> GetAllWithDetailsAsync()
        {
            return await GetAllAsync(s => s.Customer, s => s.Staff, s => s.SaleItems);
        }
    }
}
