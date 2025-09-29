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

        public async Task EnsureContributionTablesAsync()
        {
            try
            {
                _logger.LogInformation("Ensuring Contribution tables exist");
                
                // Create ContributionSettings table if it doesn't exist
                var createContributionSettingsTable = @"
                    CREATE TABLE IF NOT EXISTS ""ContributionSettings"" (
                        ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        ""Name"" TEXT NOT NULL,
                        ""Description"" TEXT,
                        ""EventId"" INTEGER NOT NULL,
                        ""ContributionType"" TEXT NOT NULL,
                        ""Amount"" DECIMAL(10,2) NOT NULL,
                        ""RecurringDay"" INTEGER,
                        ""RecurringFrequency"" TEXT,
                        ""CreatedAt"" TEXT NOT NULL,
                        ""UpdatedAt"" TEXT,
                        ""IsActive"" INTEGER NOT NULL DEFAULT 1,
                        CONSTRAINT ""FK_ContributionSettings_Events_EventId"" FOREIGN KEY (""EventId"") REFERENCES ""Events"" (""Id"") ON DELETE CASCADE
                    );";

                await _context.Database.ExecuteSqlRawAsync(createContributionSettingsTable);
                _logger.LogInformation("ContributionSettings table created or already exists");

                // Create Contributions table if it doesn't exist
                var createContributionsTable = @"
                    CREATE TABLE IF NOT EXISTS ""Contributions"" (
                        ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        ""EventId"" INTEGER NOT NULL,
                        ""DevoteeId"" INTEGER NOT NULL,
                        ""ContributionSettingId"" INTEGER NOT NULL,
                        ""Amount"" DECIMAL(10,2) NOT NULL,
                        ""ContributionDate"" TEXT NOT NULL,
                        ""Notes"" TEXT,
                        ""PaymentMethod"" TEXT NOT NULL,
                        ""TransactionReference"" TEXT,
                        ""CreatedAt"" TEXT NOT NULL,
                        ""UpdatedAt"" TEXT,
                        ""IsActive"" INTEGER NOT NULL DEFAULT 1,
                        CONSTRAINT ""FK_Contributions_Events_EventId"" FOREIGN KEY (""EventId"") REFERENCES ""Events"" (""Id"") ON DELETE CASCADE,
                        CONSTRAINT ""FK_Contributions_Devotees_DevoteeId"" FOREIGN KEY (""DevoteeId"") REFERENCES ""Devotees"" (""Id"") ON DELETE CASCADE,
                        CONSTRAINT ""FK_Contributions_ContributionSettings_ContributionSettingId"" FOREIGN KEY (""ContributionSettingId"") REFERENCES ""ContributionSettings"" (""Id"") ON DELETE CASCADE
                    );";

                await _context.Database.ExecuteSqlRawAsync(createContributionsTable);
                _logger.LogInformation("Contributions table created or already exists");

                // Create indexes
                var indexes = new[]
                {
                    @"CREATE INDEX IF NOT EXISTS ""IX_ContributionSettings_EventId"" ON ""ContributionSettings"" (""EventId"");",
                    @"CREATE INDEX IF NOT EXISTS ""IX_Contributions_EventId"" ON ""Contributions"" (""EventId"");",
                    @"CREATE INDEX IF NOT EXISTS ""IX_Contributions_DevoteeId"" ON ""Contributions"" (""DevoteeId"");",
                    @"CREATE INDEX IF NOT EXISTS ""IX_Contributions_ContributionSettingId"" ON ""Contributions"" (""ContributionSettingId"");"
                };

                foreach (var indexSql in indexes)
                {
                    await _context.Database.ExecuteSqlRawAsync(indexSql);
                }
                _logger.LogInformation("Contribution table indexes created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring Contribution tables exist");
                throw;
            }
        }
    }
}
