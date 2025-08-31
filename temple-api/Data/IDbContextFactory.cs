using Microsoft.EntityFrameworkCore;
using TempleApi.Enums;

namespace TempleApi.Data
{
    public interface IDbContextFactory
    {
        DbContext CreateDbContext(DatabaseProvider provider, string connectionString);
        TempleDbContext CreateTempleDbContext();
    }
}
