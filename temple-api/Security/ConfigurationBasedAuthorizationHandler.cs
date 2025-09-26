using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using TempleApi.Configuration;
using TempleApi.Data;
using TempleApi.Enums;

namespace TempleApi.Security
{
    /// <summary>
    /// Authorization handler that uses configuration to determine permissions
    /// </summary>
    public class ConfigurationBasedAuthorizationHandler : IAuthorizationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ConfigurationBasedAuthorizationHandler> _logger;
        private readonly AuthorizationSettings _authSettings;

        public ConfigurationBasedAuthorizationHandler(
            IServiceProvider serviceProvider,
            ILogger<ConfigurationBasedAuthorizationHandler> logger,
            IOptions<AuthorizationSettings> authSettings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _authSettings = authSettings.Value;
        }

        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            // Skip if permission-based auth is disabled
            if (!_authSettings.EnablePermissionBasedAuth)
            {
                return Task.CompletedTask;
            }

            var httpContext = context.Resource as HttpContext;
            if (httpContext == null)
            {
                return Task.CompletedTask;
            }

            var path = httpContext.Request.Path.Value?.ToLowerInvariant() ?? "";
            var method = httpContext.Request.Method;

            // Check if endpoint is public
            if (_authSettings.PublicEndpoints.Any(ep => path.StartsWith(ep.ToLowerInvariant())))
            {
                // Public endpoint, no authorization needed
                foreach (var requirement in context.PendingRequirements.ToList())
                {
                    context.Succeed(requirement);
                }
                return Task.CompletedTask;
            }

            // For authenticated user requirement
            var authRequirement = context.PendingRequirements.OfType<DenyAnonymousAuthorizationRequirement>().FirstOrDefault();
            if (authRequirement != null && context.User.Identity?.IsAuthenticated == true)
            {
                context.Succeed(authRequirement);
            }

            return Task.CompletedTask;
        }
    }
}
