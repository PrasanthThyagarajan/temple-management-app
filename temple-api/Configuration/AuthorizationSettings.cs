namespace TempleApi.Configuration
{
    public class AuthorizationSettings
    {
        public const string SectionName = "Authorization";
        
        public bool EnablePermissionBasedAuth { get; set; } = true;
        public bool DefaultRequireAuthentication { get; set; } = true;
        public List<string> PublicEndpoints { get; set; } = new List<string>();
        public Dictionary<string, Dictionary<string, string>> EndpointPermissions { get; set; } = new Dictionary<string, Dictionary<string, string>>();
    }
}
