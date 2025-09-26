using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TempleApi.Data;
using TempleApi.Enums;

namespace TempleApi.Security
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public Permission RequiredPermission { get; }
        public string PageUrl { get; }

        public PermissionRequirement(Permission permission, string pageUrl)
        {
            RequiredPermission = permission;
            PageUrl = pageUrl;
        }
    }

    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PermissionAuthorizationHandler> _logger;

        public PermissionAuthorizationHandler(IServiceProvider serviceProvider, ILogger<PermissionAuthorizationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // Get user ID from claims
            var userIdClaim = context.User.FindFirst("userid") ?? context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogWarning("User ID not found in claims");
                return;
            }

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TempleDbContext>();

            try
            {
                // Check if user has the required permission for the page
                var hasPermission = await dbContext.UserRoles
                    .Where(ur => ur.UserId == userId && ur.IsActive)
                    .Join(dbContext.RolePermissions,
                        ur => ur.RoleId,
                        rp => rp.RoleId,
                        (ur, rp) => new { ur, rp })
                    .Where(x => x.rp.IsActive)
                    .Join(dbContext.PagePermissions,
                        x => x.rp.PagePermissionId,
                        pp => pp.PagePermissionId,
                        (x, pp) => new { x.ur, x.rp, pp })
                    .Where(x => x.pp.IsActive && 
                               x.pp.PageUrl == requirement.PageUrl && 
                               x.pp.PermissionId == (int)requirement.RequiredPermission)
                    .AnyAsync();

                if (hasPermission)
                {
                    context.Succeed(requirement);
                    _logger.LogInformation("User {UserId} granted {Permission} access to {PageUrl}", 
                        userId, requirement.RequiredPermission, requirement.PageUrl);
                }
                else
                {
                    _logger.LogWarning("User {UserId} denied {Permission} access to {PageUrl}", 
                        userId, requirement.RequiredPermission, requirement.PageUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permissions for user {UserId}", userId);
            }
        }
    }

    // Helper attribute for minimal APIs
    public class RequirePermissionAttribute : Attribute
    {
        public Permission Permission { get; }
        public string PageUrl { get; }

        public RequirePermissionAttribute(Permission permission, string pageUrl)
        {
            Permission = permission;
            PageUrl = pageUrl;
        }
    }
}
