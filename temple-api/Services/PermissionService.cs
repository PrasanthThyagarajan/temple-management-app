using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TempleApi.Configuration;
using TempleApi.Data;
using TempleApi.Enums;

namespace TempleApi.Services
{
    public interface IPermissionService
    {
        Task<bool> CheckUserPermissionAsync(int userId, string pageUrl, Permission permission);
        Task<List<string>> GetUserPermissionsForPageAsync(int userId, string pageUrl);
        Task<Dictionary<string, List<Permission>>> GetAllUserPermissionsAsync(int userId);
    }

    public class PermissionService : IPermissionService
    {
        private readonly TempleDbContext _context;
        private readonly ILogger<PermissionService> _logger;
        private readonly AuthorizationSettings _authSettings;

        public PermissionService(
            TempleDbContext context,
            ILogger<PermissionService> logger,
            IOptions<AuthorizationSettings> authSettings)
        {
            _context = context;
            _logger = logger;
            _authSettings = authSettings.Value;
        }

        public async Task<bool> CheckUserPermissionAsync(int userId, string pageUrl, Permission permission)
        {
            try
            {
                return await _context.UserRoles
                    .Where(ur => ur.UserId == userId && ur.IsActive)
                    .Join(_context.RolePermissions,
                        ur => ur.RoleId,
                        rp => rp.RoleId,
                        (ur, rp) => new { ur, rp })
                    .Where(x => x.rp.IsActive)
                    .Join(_context.PagePermissions,
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
                _logger.LogError(ex, "Error checking permission for user {UserId} on page {PageUrl}", userId, pageUrl);
                return false;
            }
        }

        public async Task<List<string>> GetUserPermissionsForPageAsync(int userId, string pageUrl)
        {
            try
            {
                var permissions = await _context.UserRoles
                    .Where(ur => ur.UserId == userId && ur.IsActive)
                    .Join(_context.RolePermissions,
                        ur => ur.RoleId,
                        rp => rp.RoleId,
                        (ur, rp) => new { ur, rp })
                    .Where(x => x.rp.IsActive)
                    .Join(_context.PagePermissions,
                        x => x.rp.PagePermissionId,
                        pp => pp.PagePermissionId,
                        (x, pp) => new { x.ur, x.rp, pp })
                    .Where(x => x.pp.IsActive && x.pp.PageUrl == pageUrl)
                    .Select(x => x.pp.PermissionId)
                    .Distinct()
                    .ToListAsync();

                return permissions
                    .Select(p => ((Permission)p).ToString())
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions for user {UserId} on page {PageUrl}", userId, pageUrl);
                return new List<string>();
            }
        }

        public async Task<Dictionary<string, List<Permission>>> GetAllUserPermissionsAsync(int userId)
        {
            try
            {
                var permissions = await _context.UserRoles
                    .Where(ur => ur.UserId == userId && ur.IsActive)
                    .Join(_context.RolePermissions,
                        ur => ur.RoleId,
                        rp => rp.RoleId,
                        (ur, rp) => new { ur, rp })
                    .Where(x => x.rp.IsActive)
                    .Join(_context.PagePermissions,
                        x => x.rp.PagePermissionId,
                        pp => pp.PagePermissionId,
                        (x, pp) => new { x.ur, x.rp, pp })
                    .Where(x => x.pp.IsActive)
                    .Select(x => new { x.pp.PageUrl, x.pp.PermissionId })
                    .ToListAsync();

                var result = new Dictionary<string, List<Permission>>();
                
                foreach (var perm in permissions)
                {
                    if (!result.ContainsKey(perm.PageUrl))
                    {
                        result[perm.PageUrl] = new List<Permission>();
                    }
                    
                    result[perm.PageUrl].Add((Permission)perm.PermissionId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all permissions for user {UserId}", userId);
                return new Dictionary<string, List<Permission>>();
            }
        }
    }
}
