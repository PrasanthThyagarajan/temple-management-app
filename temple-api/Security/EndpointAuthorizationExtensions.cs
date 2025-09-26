using Microsoft.AspNetCore.Authorization;
using TempleApi.Enums;

namespace TempleApi.Security
{
    public static class EndpointAuthorizationExtensions
    {
        public static RouteHandlerBuilder RequirePermission(this RouteHandlerBuilder builder, Permission permission, string pageUrl)
        {
            var policyName = $"{permission}_{pageUrl}".Replace("/", "_").Replace(" ", "_");
            
            builder.WithMetadata(new AuthorizeAttribute(policyName));
            
            // Store the policy configuration for later registration
            if (!PermissionPolicies.Policies.ContainsKey(policyName))
            {
                PermissionPolicies.Policies[policyName] = new PermissionRequirement(permission, pageUrl);
            }
            
            return builder;
        }
    }

    // Static class to store permission policies that need to be registered
    public static class PermissionPolicies
    {
        public static Dictionary<string, PermissionRequirement> Policies { get; } = new Dictionary<string, PermissionRequirement>();
    }
}
