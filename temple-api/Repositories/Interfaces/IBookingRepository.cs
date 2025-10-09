using TempleApi.Domain.Entities;

namespace TempleApi.Repositories.Interfaces
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetAllWithDetailsAsync();
        Task<Booking?> GetWithDetailsAsync(int id);
    }
}


