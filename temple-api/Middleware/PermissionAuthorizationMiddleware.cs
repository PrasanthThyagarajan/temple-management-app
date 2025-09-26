using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using TempleApi.Configuration;
using TempleApi.Data;
using TempleApi.Enums;

namespace TempleApi.Middleware
{
    public class PermissionAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PermissionAuthorizationMiddleware> _logger;
        private readonly AuthorizationSettings _authSettings;

        public PermissionAuthorizationMiddleware(
            RequestDelegate next,
            ILogger<PermissionAuthorizationMiddleware> logger,
            IOptions<AuthorizationSettings> authSettings)
        {
            _next = next;
            _logger = logger;
            _authSettings = authSettings.Value;
        }

        public async Task InvokeAsync(HttpContext context, TempleDbContext dbContext)
        {
            // Skip if permission-based auth is disabled
            if (!_authSettings.EnablePermissionBasedAuth)
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
            var method = context.Request.Method;

            // Check if endpoint is public
            if (_authSettings.PublicEndpoints.Any(ep => path.StartsWith(ep.ToLowerInvariant())))
            {
                await _next(context);
                return;
            }

            // Check if user is authenticated
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                if (_authSettings.DefaultRequireAuthentication)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Authentication required");
                    return;
                }
                
                await _next(context);
                return;
            }

            // Find matching endpoint configuration
            var matchingEndpoint = FindMatchingEndpoint(path);
            if (matchingEndpoint == null)
            {
                // No specific permission configured, proceed
                await _next(context);
                return;
            }

            // Get required permission for the HTTP method
            if (!_authSettings.EndpointPermissions[matchingEndpoint].TryGetValue(method, out var requiredPermission))
            {
                // No permission configured for this method
                await _next(context);
                return;
            }

            // Parse the permission enum
            if (!Enum.TryParse<Permission>(requiredPermission, out var permission))
            {
                _logger.LogWarning("Invalid permission configured: {Permission}", requiredPermission);
                await _next(context);
                return;
            }

            // Get user ID from claims
            var userIdClaim = context.User.FindFirst("userid") ?? context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("User ID not found in claims");
                return;
            }

            // Determine the page URL for permission check
            var pageUrl = DeterminePageUrl(matchingEndpoint);

            // Check if user has the required permission
            var hasPermission = await CheckUserPermission(dbContext, userId, pageUrl, permission);

            if (!hasPermission)
            {
                _logger.LogWarning("User {UserId} denied {Permission} access to {Path}", userId, permission, path);
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync($"Access denied. Required permission: {permission} for {pageUrl}");
                return;
            }

            _logger.LogInformation("User {UserId} granted {Permission} access to {Path}", userId, permission, path);
            await _next(context);
        }

        private string? FindMatchingEndpoint(string path)
        {
            // Find the most specific matching endpoint
            return _authSettings.EndpointPermissions.Keys
                .Where(ep => path.StartsWith(ep.ToLowerInvariant()))
                .OrderByDescending(ep => ep.Length)
                .FirstOrDefault();
        }

        private string DeterminePageUrl(string endpoint)
        {
            // Map API endpoint to page URL in PagePermissions table
            // The PagePermissions table stores frontend routes without /api prefix
            
            // Handle admin endpoints specially
            if (endpoint.StartsWith("/api/admin/"))
            {
                return endpoint.Replace("/api", "");
            }
            
            // For regular API endpoints, map to frontend routes
            return endpoint switch
            {
                var e when e.StartsWith("/api/users") => "/admin/users",
                var e when e.StartsWith("/api/roles") => "/roles",
                var e when e.StartsWith("/api/devotees") => "/devotees",
                var e when e.StartsWith("/api/donations") => "/donations",
                var e when e.StartsWith("/api/events") => "/events",
                var e when e.StartsWith("/api/expense-items") => "/event-expense-items",
                var e when e.StartsWith("/api/expense-services") => "/event-expense-services",
                var e when e.StartsWith("/api/event-expenses") => "/event-expenses",
                var e when e.StartsWith("/api/vouchers") => "/vouchers",
                var e when e.StartsWith("/api/products") => "/products",
                var e when e.StartsWith("/api/sales") => "/sales",
                var e when e.StartsWith("/api/poojas") => "/poojas",
                var e when e.StartsWith("/api/bookings") => "/bookings",
                var e when e.StartsWith("/api/temples") => "/temples",
                var e when e.StartsWith("/api/categories") => "/categories",
                var e when e.StartsWith("/api/areas") => "/areas",
                var e when e.StartsWith("/api/user-roles") => "/user-roles",
                var e when e.StartsWith("/api/auth") => "/", // Auth endpoints map to home
                _ => endpoint.Replace("/api", "")
            };
        }

        private async Task<bool> CheckUserPermission(TempleDbContext dbContext, int userId, string pageUrl, Permission permission)
        {
            try
            {
                return await dbContext.UserRoles
                    .Where(ur => ur.UserId == userId && ur.IsActive)
                    .AsNoTracking()
                    .Join(dbContext.RolePermissions.AsNoTracking(),
                        ur => ur.RoleId,
                        rp => rp.RoleId,
                        (ur, rp) => new { ur, rp })
                    .Where(x => x.rp.IsActive)
                    .Join(dbContext.PagePermissions.AsNoTracking(),
                        x => x.rp.PagePermissionId,
                        pp => pp.PagePermissionId,
                        (x, pp) => new { x.ur, x.rp, pp })
                    .Where(x => x.pp.IsActive && 
                               x.pp.PageUrl == pageUrl && 
                               x.pp.PermissionId == (int)permission)
                    .AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permissions for user {UserId}", userId);
                return false;
            }
        }
    }

    public static class PermissionAuthorizationMiddlewareExtensions
    {
        public static IApplicationBuilder UsePermissionAuthorization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PermissionAuthorizationMiddleware>();
        }
    }
}
