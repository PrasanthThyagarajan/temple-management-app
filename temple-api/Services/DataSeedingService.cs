using TempleApi.Domain.Entities;
using TempleApi.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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
            
            // Seed Page Permissions
            await SeedPagePermissionsAsync();
            
            // Seed Role-Permission mappings
            await SeedRolePermissionsAsync();
            
            // Seed default admin user
            await SeedDefaultAdminAsync();

            // Seed Event Types
            await SeedEventTypesAsync();

            await SeedDefaultTempleAsync();
        }

        private async Task SeedRolesAsync()
        {
            var existingRoles = await _context.Roles.Select(r => r.RoleName).ToListAsync();

            var rolesToAdd = new List<Role>
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

            var newRoles = rolesToAdd.Where(r => !existingRoles.Contains(r.RoleName)).ToList();

            if (newRoles.Any())
            {
                _context.Roles.AddRange(newRoles);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedPagePermissionsAsync()
        {
            var pagePermissionsToInsert = new List<PagePermission>();

            // Define pages and their permissions based on router/index.js paths
            var pages = new[]
            {
                // Home and Dashboard
                new { Name = "Home", Url = "/" },
                new { Name = "Dashboard", Url = "/dashboard" },

                // Core Temple Management
                new { Name = "Areas", Url = "/areas" },
                new { Name = "Temples", Url = "/temples" },
                new { Name = "Devotees", Url = "/devotees" },
                new { Name = "Donations", Url = "/donations" },
                new { Name = "Events", Url = "/events" },
                new { Name = "Contributions", Url = "/contributions" },
                new { Name = "Categories", Url = "/categories" },
                new { Name = "Products", Url = "/products" },
                new { Name = "Sales", Url = "/sales" },
                new { Name = "Inventory", Url = "/inventories" },

                // Event Expense Management
                new { Name = "EventExpenses", Url = "/event-expenses" },
                new { Name = "EventExpenseItems", Url = "/event-expense-items" },
                new { Name = "EventExpenseServices", Url = "/event-expense-services" },
                new { Name = "Vouchers", Url = "/vouchers" },

                // Admin & User Management
                new { Name = "RolePermissions", Url = "/admin/role-permissions" },
                new { Name = "RoleManagement", Url = "/roles" },
                new { Name = "UserRoleConfiguration", Url = "/user-roles" },
                new { Name = "UserRegistration", Url = "/register" },
                new { Name = "AdminUserManagement", Url = "/admin/users" },
                new { Name = "ContributionSettings", Url = "/contribution-settings" },

                // Additional Routes
                new { Name = "Verify", Url = "/verify" },
                new { Name = "ApiTest", Url = "/api-test" }
            };

            var desiredUrls = pages.Select(p => p.Url).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var existing = await _context.PagePermissions.ToListAsync();
            var now = DateTime.UtcNow;
            var madeChanges = false;

            // Ensure each desired page has all four permissions
            foreach (var page in pages)
            {
                foreach (var permissionType in Enum.GetValues<TempleApi.Enums.Permission>())
                {
                    var match = existing.FirstOrDefault(pp =>
                        pp.PageUrl.Equals(page.Url, StringComparison.OrdinalIgnoreCase) &&
                        pp.PermissionId == (int)permissionType);

                    if (match == null)
                    {
                        pagePermissionsToInsert.Add(new PagePermission
                        {
                            PageName = page.Name,
                            PageUrl = page.Url,
                            PermissionId = (int)permissionType,
                            CreatedAt = now,
                            IsActive = true
                        });
                        madeChanges = true;
                    }
                    else
                    {
                        // Keep name in sync and ensure active
                        if (!string.Equals(match.PageName, page.Name, StringComparison.Ordinal))
                        {
                            match.PageName = page.Name;
                            madeChanges = true;
                        }
                        if (!match.IsActive)
                        {
                            match.IsActive = true;
                            madeChanges = true;
                        }
                    }
                }
            }

            // Deactivate obsolete page permissions not in desired set
            foreach (var pp in existing)
            {
                if (!desiredUrls.Contains(pp.PageUrl) && pp.IsActive)
                {
                    pp.IsActive = false;
                    madeChanges = true;
                }
            }

            if (pagePermissionsToInsert.Count > 0)
            {
                _context.PagePermissions.AddRange(pagePermissionsToInsert);
            }

            if (madeChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedRolePermissionsAsync()
        {
            var adminRole = await _context.Roles.FirstAsync(r => r.RoleName == "Admin");
            var generalRole = await _context.Roles.FirstAsync(r => r.RoleName == "General");

            var allPagePermissions = await _context.PagePermissions.ToListAsync();
            var now = DateTime.UtcNow;
            var madeChanges = false;

            // Ensure Admin has ALL permissions for ALL pages (active)
            foreach (var pagePermission in allPagePermissions)
            {
                var existingAdminRp = await _context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == adminRole.RoleId && rp.PagePermissionId == pagePermission.PagePermissionId);
                if (existingAdminRp == null)
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = adminRole.RoleId,
                        PagePermissionId = pagePermission.PagePermissionId,
                        CreatedAt = now,
                        IsActive = true
                    });
                    madeChanges = true;
                }
                else if (!existingAdminRp.IsActive)
                {
                    existingAdminRp.IsActive = true;
                    madeChanges = true;
                }
            }

            // General gets specific permissions for specific pages
            var generalReadTargets = new HashSet<string>(new[]
            {
                "Home","Dashboard","Devotees","Donations","Events","Temples","Areas","Categories","Products","Sales","EventExpenses","EventExpenseItems","EventExpenseServices"
            });

            var generalCreateTargets = new HashSet<string>(new[]
            {
                "Donations","Devotees","EventExpenses","EventExpenseItems"
            });

            var generalUpdateTargets = new HashSet<string>(new[]
            {
                "Donations","EventExpenses","EventExpenseItems"
            });

            var generalDesired = allPagePermissions.Where(p =>
                (p.PermissionId == (int)TempleApi.Enums.Permission.Read && generalReadTargets.Contains(p.PageName)) ||
                (p.PermissionId == (int)TempleApi.Enums.Permission.Create && generalCreateTargets.Contains(p.PageName)) ||
                (p.PermissionId == (int)TempleApi.Enums.Permission.Update && generalUpdateTargets.Contains(p.PageName))
            ).ToList();

            foreach (var pagePermission in generalDesired)
            {
                var existingGeneralRp = await _context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == generalRole.RoleId && rp.PagePermissionId == pagePermission.PagePermissionId);
                if (existingGeneralRp == null)
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = generalRole.RoleId,
                        PagePermissionId = pagePermission.PagePermissionId,
                        CreatedAt = now,
                        IsActive = true
                    });
                    madeChanges = true;
                }
                else if (!existingGeneralRp.IsActive)
                {
                    existingGeneralRp.IsActive = true;
                    madeChanges = true;
                }
            }

            if (madeChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedDefaultAdminAsync()
        {
            if (!await _context.Users.AnyAsync())
            {
                var adminRole = await _context.Roles.FirstAsync(r => r.RoleName == "Admin");
                
                // Store password as Base64 per BasicAuth requirement
                var passwordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("admin123"));
                
                var adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@temple.com",
                    FullName = "System Administrator",
                    PasswordHash = passwordHash,
                    IsActive = true,
                    IsVerified = true,
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
            else
            {
                // Ensure existing admin has Base64 password and is active
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
                if (adminUser != null)
                {
                    var expected = Convert.ToBase64String(Encoding.UTF8.GetBytes("admin123"));
                    var changed = false;
                    try
                    {
                        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(adminUser.PasswordHash));
                        if (!string.Equals(decoded, "admin123"))
                        {
                            adminUser.PasswordHash = expected;
                            changed = true;
                        }
                    }
                    catch
                    {
                        adminUser.PasswordHash = expected;
                        changed = true;
                    }

                    if (!adminUser.IsActive)
                    {
                        adminUser.IsActive = true;
                        changed = true;
                    }

                    if (!adminUser.IsVerified)
                    {
                        adminUser.IsVerified = true;
                        changed = true;
                    }

                    if (changed)
                    {
                        _context.Users.Update(adminUser);
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }

        private async Task SeedDefaultAreasAsync(int templeId)
        {
            var existingAreas = await _context.Areas.Where(a => a.TempleId == templeId).Select(a => a.Name).ToListAsync();

            var defaultAreas = new[]
            {
                "Sree Kovil",
                "Maadan Kovil",
                "Ganpathi Madapam",
                "Sarpakaavu",
                "Auditorium",
                "Guest House",
                "Rental Area A",
                "Rental Area B"
            };

            var areasToAdd = defaultAreas.Where(areaName => !existingAreas.Contains(areaName)).ToList();

            foreach (var areaName in areasToAdd)
            {
                _context.Areas.Add(new Area
                {
                    TempleId = templeId,
                    Name = areaName,
                    Description = string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }

            if (areasToAdd.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedEventTypesAsync()
        {
            if (!await _context.EventTypes.AnyAsync())
            {
                var types = new List<EventType>
                {
                    new EventType { Name = "Daily Ritual", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new EventType { Name = "Monthly Ritual", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new EventType { Name = "Annual Festival", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new EventType { Name = "Season Festival", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new EventType { Name = "Kumba Abhishekam", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new EventType { Name = "Special Pooja", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new EventType { Name = "Auditorium Events", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new EventType { Name = "Monthly Rental", IsActive = true, CreatedAt = DateTime.UtcNow }
                };

                _context.EventTypes.AddRange(types);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedDefaultTempleAsync()
        {
            var existingTemple = await _context.Temples.FirstOrDefaultAsync(t => t.Name == "Pallipuram Pariyathara shri Mutharamman Temple");
            if (existingTemple == null)
            {
                var newTemple = new Temple
                {
                    Name = "Pallipuram Pariyathara shri Mutharamman Temple",
                    City = "Thiruvananthapuram",
                    State = "Kerala",
                    Address = "Vetturod, Kaniyapuram",
                    EstablishedDate = new DateTime(1900, 1, 1),
                    Deity = "Mutharamman Devi",
                    Description = "Pallipuram Pariyathara shri Mutharamman Temple",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Temples.Add(newTemple);
                await _context.SaveChangesAsync();

                // Seed default areas for the new temple
                await SeedDefaultAreasAsync(newTemple.Id);
            }
            else
            {
                // Ensure default areas exist for the existing temple
                var hasAreas = await _context.Areas.AnyAsync(a => a.TempleId == existingTemple.Id);
                if (!hasAreas)
                {
                    await SeedDefaultAreasAsync(existingTemple.Id);
                }
            }
        }

        private string HashPassword(string password)
        {
            // Not used anymore; keep for compatibility if needed
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
        }
    }
}
