using TempleApi.Domain.Entities;
using TempleApi.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace TempleApi.Services
{
    public class DataSeedingService
    {
        private readonly TempleDbContext _context;

        public DataSeedingService(TempleDbContext context)
        {
            _context = context;
        }

        public async Task SeedDataAsync()
        {
            // Seed Roles
            await SeedRolesAsync();
            
            // Seed Permissions
            await SeedPermissionsAsync();
            
            // Seed Role-Permission mappings
            await SeedRolePermissionsAsync();
            
            // Seed default admin user
            await SeedDefaultAdminAsync();
        }

        private async Task SeedRolesAsync()
        {
            if (!await _context.Roles.AnyAsync())
            {
                var roles = new List<Role>
                {
                    new Role
                    {
                        RoleName = "Admin",
                        Description = "System Administrator with full access",
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Role
                    {
                        RoleName = "General",
                        Description = "General user with basic access",
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    }
                };

                _context.Roles.AddRange(roles);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedPermissionsAsync()
        {
            if (!await _context.Permissions.AnyAsync())
            {
                var permissions = new List<Permission>
                {
                    // Admin Console composite permission
                    new Permission { PermissionName = "UserRoleConfiguration", Description = "Access to user/role/permission admin" },
                    // User Management
                    new Permission { PermissionName = "users.view", Description = "View users" },
                    new Permission { PermissionName = "users.create", Description = "Create users" },
                    new Permission { PermissionName = "users.edit", Description = "Edit users" },
                    new Permission { PermissionName = "users.delete", Description = "Delete users" },
                    
                    // Temple Management
                    new Permission { PermissionName = "temples.view", Description = "View temples" },
                    new Permission { PermissionName = "temples.create", Description = "Create temples" },
                    new Permission { PermissionName = "temples.edit", Description = "Edit temples" },
                    new Permission { PermissionName = "temples.delete", Description = "Delete temples" },
                    
                    // Devotee Management
                    new Permission { PermissionName = "devotees.view", Description = "View devotees" },
                    new Permission { PermissionName = "devotees.create", Description = "Create devotees" },
                    new Permission { PermissionName = "devotees.edit", Description = "Edit devotees" },
                    new Permission { PermissionName = "devotees.delete", Description = "Delete devotees" },
                    
                    // Donation Management
                    new Permission { PermissionName = "donations.view", Description = "View donations" },
                    new Permission { PermissionName = "donations.create", Description = "Create donations" },
                    new Permission { PermissionName = "donations.edit", Description = "Edit donations" },
                    new Permission { PermissionName = "donations.delete", Description = "Delete donations" },
                    
                    // Event Management
                    new Permission { PermissionName = "events.view", Description = "View events" },
                    new Permission { PermissionName = "events.create", Description = "Create events" },
                    new Permission { PermissionName = "events.edit", Description = "Edit events" },
                    new Permission { PermissionName = "events.delete", Description = "Delete events" },
                    
                    // Product Management
                    new Permission { PermissionName = "products.view", Description = "View products" },
                    new Permission { PermissionName = "products.create", Description = "Create products" },
                    new Permission { PermissionName = "products.edit", Description = "Edit products" },
                    new Permission { PermissionName = "products.delete", Description = "Delete products" },
                    
                    // Sale Management
                    new Permission { PermissionName = "sales.view", Description = "View sales" },
                    new Permission { PermissionName = "sales.create", Description = "Create sales" },
                    new Permission { PermissionName = "sales.edit", Description = "Edit sales" },
                    new Permission { PermissionName = "sales.delete", Description = "Delete sales" },
                    
                    // Pooja Management
                    new Permission { PermissionName = "poojas.view", Description = "View poojas" },
                    new Permission { PermissionName = "poojas.create", Description = "Create poojas" },
                    new Permission { PermissionName = "poojas.edit", Description = "Edit poojas" },
                    new Permission { PermissionName = "poojas.delete", Description = "Delete poojas" },
                    
                    // Booking Management
                    new Permission { PermissionName = "bookings.view", Description = "View bookings" },
                    new Permission { PermissionName = "bookings.create", Description = "Create bookings" },
                    new Permission { PermissionName = "bookings.edit", Description = "Edit bookings" },
                    new Permission { PermissionName = "bookings.delete", Description = "Delete bookings" }
                };

                foreach (var permission in permissions)
                {
                    permission.CreatedAt = DateTime.UtcNow;
                    permission.IsActive = true;
                }

                _context.Permissions.AddRange(permissions);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedRolePermissionsAsync()
        {
            if (!await _context.RolePermissions.AnyAsync())
            {
                var adminRole = await _context.Roles.FirstAsync(r => r.RoleName == "Admin");
                var generalRole = await _context.Roles.FirstAsync(r => r.RoleName == "General");

                var allPermissions = await _context.Permissions.ToListAsync();

                var rolePermissions = new List<RolePermission>();

                // Admin gets all permissions
                foreach (var permission in allPermissions)
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = adminRole.RoleId,
                        PermissionId = permission.PermissionId,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                }

                // General gets basic view permissions and create for bookings/donations
                var generalPermissions = allPermissions.Where(p => 
                    p.PermissionName.Contains("view") ||
                    p.PermissionName.StartsWith("bookings.create") ||
                    p.PermissionName.StartsWith("donations.create")
                ).ToList();

                foreach (var permission in generalPermissions)
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = generalRole.RoleId,
                        PermissionId = permission.PermissionId,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                }

                _context.RolePermissions.AddRange(rolePermissions);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedDefaultAdminAsync()
        {
            if (!await _context.Users.AnyAsync())
            {
                var adminRole = await _context.Roles.FirstAsync(r => r.RoleName == "Admin");
                
                // Hash password for "admin123" using PBKDF2 to match AuthService verification
                var passwordHash = HashPassword("admin123");
                
                var adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@temple.com",
                    FullName = "System Administrator",
                    PasswordHash = passwordHash,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();

                // Assign admin role
                var userRole = new UserRole
                {
                    UserId = adminUser.UserId,
                    RoleId = adminRole.RoleId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();
            }
        }

        private string HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var saltBytes = new byte[32];
            rng.GetBytes(saltBytes);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
            var hashBytes = pbkdf2.GetBytes(32);

            var hashWithSalt = new byte[64];
            Array.Copy(saltBytes, 0, hashWithSalt, 0, 32);
            Array.Copy(hashBytes, 0, hashWithSalt, 32, 32);

            return Convert.ToBase64String(hashWithSalt);
        }
    }
}
