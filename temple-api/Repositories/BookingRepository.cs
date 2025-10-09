using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Repositories.Interfaces;

namespace TempleApi.Repositories
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(TempleDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Booking>> GetAllWithDetailsAsync()
        {
            return await GetAllAsync();
        }

        public async Task<Booking?> GetWithDetailsAsync(int id)
        {
            return await GetByIdAsync(id);
        }
    }
}


