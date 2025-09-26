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
            if (!await _context.PagePermissions.AnyAsync())
            {
                var pagePermissions = new List<PagePermission>();
                
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
                    new { Name = "Categories", Url = "/categories" },
                    new { Name = "Products", Url = "/products" },
                    new { Name = "Sales", Url = "/sales" },
                    
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
                    
                    // Additional Routes
                    new { Name = "Verify", Url = "/verify" },
                    new { Name = "ApiTest", Url = "/api-test" },
                    new { Name = "CreateManageEvent", Url = "/create-manage-event" },
                    new { Name = "AddEvents", Url = "/add-events" }
                };
                
                // Create permissions for each page and permission type
                foreach (var page in pages)
                {
                    foreach (var permissionType in Enum.GetValues<TempleApi.Enums.Permission>())
                    {
                        pagePermissions.Add(new PagePermission
                        {
                            PageName = page.Name,
                            PageUrl = page.Url,
                            PermissionId = (int)permissionType
                        });
                    }
                }

                foreach (var pagePermission in pagePermissions)
                {
                    pagePermission.CreatedAt = DateTime.UtcNow;
                    pagePermission.IsActive = true;
                }

                _context.PagePermissions.AddRange(pagePermissions);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedRolePermissionsAsync()
        {
            if (!await _context.RolePermissions.AnyAsync())
            {
                var adminRole = await _context.Roles.FirstAsync(r => r.RoleName == "Admin");
                var generalRole = await _context.Roles.FirstAsync(r => r.RoleName == "General");

                var allPagePermissions = await _context.PagePermissions.ToListAsync();
                var rolePermissions = new List<RolePermission>();

                // Admin gets ALL permissions for ALL pages
                foreach (var pagePermission in allPagePermissions)
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = adminRole.RoleId,
                        PagePermissionId = pagePermission.PagePermissionId,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                }

                // General gets specific permissions for specific pages
                // Read permissions for basic pages
                var generalReadPermissions = allPagePermissions.Where(p => 
                    p.PermissionId == (int)TempleApi.Enums.Permission.Read && (
                        p.PageName == "Home" ||
                        p.PageName == "Dashboard" ||
                        p.PageName == "Devotees" ||
                        p.PageName == "Donations" ||
                        p.PageName == "Events" ||
                        p.PageName == "Temples" ||
                        p.PageName == "Areas" ||
                        p.PageName == "Categories" ||
                        p.PageName == "Products" ||
                        p.PageName == "Sales" ||
                        p.PageName == "EventExpenses" ||
                        p.PageName == "EventExpenseItems" ||
                        p.PageName == "EventExpenseServices"
                    )
                ).ToList();

                // Create permissions for donations and basic temple management
                var generalCreatePermissions = allPagePermissions.Where(p => 
                    p.PermissionId == (int)TempleApi.Enums.Permission.Create && (
                        p.PageName == "Donations" ||
                        p.PageName == "Devotees" ||
                        p.PageName == "EventExpenses" ||
                        p.PageName == "EventExpenseItems"
                    )
                ).ToList();

                // Update permissions for donations and expenses (general users can update their own records)
                var generalUpdatePermissions = allPagePermissions.Where(p => 
                    p.PermissionId == (int)TempleApi.Enums.Permission.Update && (
                        p.PageName == "Donations" ||
                        p.PageName == "EventExpenses" ||
                        p.PageName == "EventExpenseItems"
                    )
                ).ToList();

                // Add all general permissions
                var generalPermissions = generalReadPermissions
                    .Concat(generalCreatePermissions)
                    .Concat(generalUpdatePermissions);

                foreach (var pagePermission in generalPermissions)
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = generalRole.RoleId,
                        PagePermissionId = pagePermission.PagePermissionId,
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
