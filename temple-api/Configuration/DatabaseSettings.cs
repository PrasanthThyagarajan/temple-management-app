namespace TempleApi.Configuration
{
    public class DatabaseSettings
    {
        public const string SectionName = "Database";
        
        public string Provider { get; set; } = "PostgreSQL";
        public string ConnectionString { get; set; } = string.Empty;
        public bool EnableMigrations { get; set; } = true;
        public bool EnableSensitiveDataLogging { get; set; } = false;
        public int CommandTimeout { get; set; } = 30;
    }
}
