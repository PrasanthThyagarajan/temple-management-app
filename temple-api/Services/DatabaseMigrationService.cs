using TempleApi.Data;
using TempleApi.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace TempleApi.Services
{
    public class DatabaseMigrationService : IDatabaseMigrationService
    {
        private readonly TempleDbContext _context;
        private readonly DatabaseSettings _databaseSettings;
        private readonly ILogger<DatabaseMigrationService> _logger;

        public DatabaseMigrationService(
            TempleDbContext context,
            IOptions<DatabaseSettings> databaseSettings,
            ILogger<DatabaseMigrationService> logger)
        {
            _context = context;
            _databaseSettings = databaseSettings.Value;
            _logger = logger;
        }

        public async Task MigrateAsync()
        {
            try
            {
                _logger.LogInformation("Starting database migration for provider: {Provider}", _databaseSettings.Provider);
                
                if (_context.Database.GetPendingMigrations().Any())
                {
                    await _context.Database.MigrateAsync();
                    _logger.LogInformation("Database migration completed successfully");
                }
                else
                {
                    _logger.LogInformation("No pending migrations found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database migration");
                throw;
            }
        }

        public async Task EnsureCreatedAsync()
        {
            try
            {
                _logger.LogInformation("Ensuring database exists for provider: {Provider}", _databaseSettings.Provider);
                
                if (!await _context.Database.CanConnectAsync())
                {
                    await _context.Database.EnsureCreatedAsync();
                    _logger.LogInformation("Database created successfully");
                }
                else
                {
                    _logger.LogInformation("Database already exists");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring database exists");
                throw;
            }
        }

        public async Task<bool> CanConnectAsync()
        {
            try
            {
                return await _context.Database.CanConnectAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking database connection");
                return false;
            }
        }

        public string GetDatabaseProvider()
        {
            return _databaseSettings.Provider;
        }

        public string GetConnectionString()
        {
            return _databaseSettings.ConnectionString;
        }
    }
}
