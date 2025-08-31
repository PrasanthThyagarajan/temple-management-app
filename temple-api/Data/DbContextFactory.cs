using Microsoft.EntityFrameworkCore;
using TempleApi.Configuration;
using TempleApi.Enums;
using Microsoft.Extensions.Options;

namespace TempleApi.Data
{
    public class DbContextFactory : IDbContextFactory
    {
        private readonly DatabaseSettings _databaseSettings;
        private readonly ILogger<DbContextFactory> _logger;

        public DbContextFactory(IOptions<DatabaseSettings> databaseSettings, ILogger<DbContextFactory> logger)
        {
            _databaseSettings = databaseSettings.Value;
            _logger = logger;
        }

        public DbContext CreateDbContext(DatabaseProvider provider, string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TempleDbContext>();
            
            switch (provider)
            {
                case DatabaseProvider.PostgreSQL:
                    optionsBuilder.UseNpgsql(connectionString, options =>
                    {
                        options.CommandTimeout(_databaseSettings.CommandTimeout);
                        options.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorCodesToAdd: null);
                    });
                    break;

                case DatabaseProvider.SQLite:
                    optionsBuilder.UseSqlite(connectionString, options =>
                    {
                        options.CommandTimeout(_databaseSettings.CommandTimeout);
                    });
                    break;

                case DatabaseProvider.SQLServer:
                    optionsBuilder.UseSqlServer(connectionString, options =>
                    {
                        options.CommandTimeout(_databaseSettings.CommandTimeout);
                        options.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
                    break;

                case DatabaseProvider.MySQL:
                    optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), options =>
                    {
                        options.CommandTimeout(_databaseSettings.CommandTimeout);
                    });
                    break;

                default:
                    throw new ArgumentException($"Unsupported database provider: {provider}");
            }

            if (_databaseSettings.EnableSensitiveDataLogging)
            {
                optionsBuilder.EnableSensitiveDataLogging();
            }

            _logger.LogInformation("Creating DbContext for provider: {Provider}", provider);
            return new TempleDbContext(optionsBuilder.Options);
        }

        public TempleDbContext CreateTempleDbContext()
        {
            var provider = Enum.Parse<DatabaseProvider>(_databaseSettings.Provider);
            return (TempleDbContext)CreateDbContext(provider, _databaseSettings.ConnectionString);
        }
    }
}
