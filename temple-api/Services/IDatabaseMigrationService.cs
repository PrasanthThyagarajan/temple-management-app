namespace TempleApi.Services
{
    public interface IDatabaseMigrationService
    {
        Task MigrateAsync();
        Task EnsureCreatedAsync();
        Task<bool> CanConnectAsync();
        string GetDatabaseProvider();
        string GetConnectionString();
        Task EnsureContributionTablesAsync();
    }
}
