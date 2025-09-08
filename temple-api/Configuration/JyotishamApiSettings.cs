namespace TempleApi.Configuration
{
    public class JyotishamApiSettings
    {
        public const string SectionName = "JyotishamApi";
        
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public int Timeout { get; set; } = 30;
    }
}

