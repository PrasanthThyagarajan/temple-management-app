using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using TempleApi.Data;
using TempleApi.Services.Interfaces;
using TempleApi.Services;
using TempleApi.Configuration;
using TempleApi.Models.DTOs;
using TempleApi.Enums;
using System.Security.Claims;
using System.Text;

namespace TempleApi.Endpoints;

public static class ReadApis
{
    public static void MapReadEndpoints(this WebApplication app)
    {
        #region Authentication Read Endpoints

        app.MapGet("/api/auth/me", async (HttpContext httpContext, IAuthService authService, ILogger<Program> logger) =>
        {
            try
            {
                // Try claims first
                var userIdClaim = httpContext.User.FindFirst("userid");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userIdFromClaims))
                {
                    var userDto = await authService.GetUserByIdAsync(userIdFromClaims);
                    return userDto != null ? Results.Ok(userDto) : Results.NotFound();
                }

                // Fallback: Basic header
                var authHeader = httpContext.Request.Headers["Authorization"].ToString();
                if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                {
                    return Results.Unauthorized();
                }
                var encoded = authHeader.Substring("Basic ".Length).Trim();
                var bytes = Convert.FromBase64String(encoded);
                var parts = Encoding.UTF8.GetString(bytes).Split(':', 2);
                if (parts.Length != 2) return Results.Unauthorized();

                var resp = await authService.LoginAsync(new LoginRequest { Username = parts[0], Password = parts[1] });
                if (!resp.Success || resp.User == null) return Results.Unauthorized();
                return Results.Ok(resp.User);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting current user");
                return Results.Problem("An internal error occurred", statusCode: 500);
            }
        }).RequireAuthorization();

        app.MapGet("/api/auth/roles", async (HttpContext httpContext, IAuthService authService, ILogger<Program> logger) =>
        {
            try
            {
                // Try claims first
                var userIdClaim = httpContext.User.FindFirst("userid");
                int userId;
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var parsed))
                {
                    userId = parsed;
                }
                else
                {
                    // Fallback: Basic header
                    var authHeader = httpContext.Request.Headers["Authorization"].ToString();
                    if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                    {
                        return Results.Unauthorized();
                    }
                    var encoded = authHeader.Substring("Basic ".Length).Trim();
                    var bytes = Convert.FromBase64String(encoded);
                    var parts = Encoding.UTF8.GetString(bytes).Split(':', 2);
                    if (parts.Length != 2) return Results.Unauthorized();
                    var resp = await authService.LoginAsync(new LoginRequest { Username = parts[0], Password = parts[1] });
                    if (!resp.Success || resp.User == null) return Results.Unauthorized();
                    userId = resp.User.UserId;
                }

                var roles = await authService.GetUserRolesAsync(userId);
                return Results.Ok(roles);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting user roles");
                return Results.Problem("An internal error occurred", statusCode: 500);
            }
        }).RequireAuthorization();

        app.MapGet("/api/auth/permissions", async (HttpContext httpContext, IAuthService authService, ILogger<Program> logger) =>
        {
            try
            {
                // Try claims first
                var userIdClaim = httpContext.User.FindFirst("userid");
                int userId;
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var parsed))
                {
                    userId = parsed;
                }
                else
                {
                    // Fallback: Basic header
                    var authHeader = httpContext.Request.Headers["Authorization"].ToString();
                    if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                    {
                        return Results.Unauthorized();
                    }
                    var encoded = authHeader.Substring("Basic ".Length).Trim();
                    var bytes = Convert.FromBase64String(encoded);
                    var parts = Encoding.UTF8.GetString(bytes).Split(':', 2);
                    if (parts.Length != 2) return Results.Unauthorized();
                    var resp = await authService.LoginAsync(new LoginRequest { Username = parts[0], Password = parts[1] });
                    if (!resp.Success || resp.User == null) return Results.Unauthorized();
                    userId = resp.User.UserId;
                }

                var permissions = await authService.GetUserPermissionsAsync(userId);
                return Results.Ok(permissions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting user permissions");
                return Results.Problem("An internal error occurred", statusCode: 500);
            }
        }).RequireAuthorization();

        app.MapGet("/api/auth/verify", async (string code, IAuthService authService, ILogger<Program> logger) =>
        {
            try
            {
                var ok = await authService.VerifyAsync(code);
                if (!ok) return Results.BadRequest("Invalid or expired verification code");
                return Results.Ok(new { message = "Account verified successfully" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in verify endpoint");
                return Results.Problem("An internal error occurred", statusCode: 500);
            }
        });

        #endregion

        #region Role Management Read Endpoints

        app.MapGet("/api/roles", async (IRoleService roleService) =>
        {
            var roles = await roleService.GetAllRolesAsync();
            return Results.Ok(roles);
        }).RequireAuthorization("UserRoleConfiguration");

        app.MapGet("/api/roles/{id:int}", async (int id, IRoleService roleService) =>
        {
            var role = await roleService.GetRoleByIdAsync(id);
            if (role == null) return Results.NotFound();
            return Results.Ok(role);
        }).RequireAuthorization("UserRoleConfiguration");

        app.MapGet("/api/roles/{roleId}/permissions", async (int roleId, TempleDbContext db) =>
        {
            var role = await db.Roles.FindAsync(roleId);
            if (role == null) return Results.NotFound();

            var pagePermissionIds = await db.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PagePermissionId)
                .ToListAsync();

            return Results.Ok(new { roleId, pagePermissionIds });
        }).RequireAuthorization("UserRoleConfiguration");

        app.MapGet("/api/userroles", async (IUserRoleService userRoleService) =>
        {
            var userRoles = await userRoleService.GetAllUserRolesAsync();
            return Results.Ok(userRoles);
        }).RequireAuthorization("UserRoleConfiguration");

        app.MapGet("/api/permissions", async (TempleDbContext db) =>
        {
            var pagePermissions = await db.PagePermissions
                .Where(p => p.IsActive)
                .Select(p => new { p.PagePermissionId, p.PageName, p.PageUrl, p.PermissionId })
                .ToListAsync();
            return Results.Ok(pagePermissions);
        }).RequireAuthorization("AdminOnly");

        app.MapGet("/api/admin/role-permissions", async (TempleDbContext context, IAuthService authService, HttpContext httpContext) =>
        {
            try 
            {
                // Try claims first
                var userIdClaim = httpContext.User.FindFirst("userid");
                int userId;
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var parsed))
                {
                    userId = parsed;
                }
                else
                {
                    // Fallback: Basic header
                    var authHeader = httpContext.Request.Headers["Authorization"].ToString();
                    if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                    {
                        return Results.Unauthorized();
                    }
                    var encoded = authHeader.Substring("Basic ".Length).Trim();
                    var bytes = Convert.FromBase64String(encoded);
                    var parts = Encoding.UTF8.GetString(bytes).Split(':', 2);
                    if (parts.Length != 2) return Results.Unauthorized();
                    var resp = await authService.LoginAsync(new LoginRequest { Username = parts[0], Password = parts[1] });
                    if (!resp.Success || resp.User == null) return Results.Unauthorized();
                    userId = resp.User.UserId;
                }

                // Get user's roles and permissions
                var (userRoles, userPermissions) = await authService.GetUserRolesAndPermissionsAsync(userId);
                
                // Get all role permissions with page details for user's roles
                var rolePermissions = await context.RolePermissions
                    .Include(rp => rp.Role)
                    .Include(rp => rp.PagePermission)
                    .Where(rp => userRoles.Contains(rp.Role.RoleName))
                    .Select(rp => new {
                        RoleName = rp.Role.RoleName,
                        PageName = rp.PagePermission.PageName,
                        PageUrl = rp.PagePermission.PageUrl,
                        PermissionName = ((TempleApi.Enums.Permission)rp.PagePermission.PermissionId).ToString(),
                        PermissionId = rp.PagePermission.PermissionId
                    })
                    .ToListAsync();

                Log.Information("Retrieved {Count} role permissions for user {UserId} with roles {Roles}", 
                    rolePermissions.Count, userId, string.Join(",", userRoles));
                    
                return Results.Ok(rolePermissions);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving role permissions");
                return Results.Problem("Failed to retrieve role permissions");
            }
        }).RequireAuthorization();

        #endregion

        #region User Management Read Endpoints

        app.MapGet("/api/users", async (IUserService userService) =>
        {
            try
            {
                var users = await userService.GetAllUsersAsync();
                return Results.Ok(users);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting all users");
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        app.MapGet("/api/users/{id}", async (int id, IUserService userService) =>
        {
            try
            {
                var user = await userService.GetUserByIdAsync(id);
                if (user == null)
                    return Results.NotFound();
                
                return Results.Ok(user);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting user with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization("UserRoleConfiguration");

        app.MapGet("/api/users/email/{email}", async (string email, IUserService userService) =>
        {
            try
            {
                var user = await userService.GetUserByEmailAsync(email);
                if (user == null)
                    return Results.NotFound();
                
                return Results.Ok(user);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting user with email {Email}", email);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization("UserRoleConfiguration");

        app.MapGet("/api/users/{userId:int}/roles-permissions", async (int userId, IAuthService authService) =>
        {
            var (roles, permissions) = await authService.GetUserRolesAndPermissionsAsync(userId);
            return Results.Ok(new { userId, roles, permissions });
        }).RequireAuthorization("UserRoleConfiguration");

        app.MapGet("/api/users/role/{role}", async (TempleApi.Enums.UserRole role, IUserService userService) =>
        {
            try
            {
                var users = await userService.GetUsersByRoleAsync(role.ToString());
                return Results.Ok(users);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting users by role {Role}", role);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization("UserRoleConfiguration");

        app.MapGet("/api/users/me", async (HttpContext httpContext, IAuthService authService) =>
        {
            // Try claims first
            var userIdClaim = httpContext.User.FindFirst("userid");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userIdFromClaims))
            {
                var userDto = await authService.GetUserByIdAsync(userIdFromClaims);
                return userDto != null ? Results.Ok(userDto) : Results.NotFound();
            }

            // Fallback: Basic header
            var authHeader = httpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Unauthorized();
            }
            var encoded = authHeader.Substring("Basic ".Length).Trim();
            var bytes = Convert.FromBase64String(encoded);
            var parts = Encoding.UTF8.GetString(bytes).Split(':', 2);
            if (parts.Length != 2) return Results.Unauthorized();

            var resp = await authService.LoginAsync(new LoginRequest { Username = parts[0], Password = parts[1] });
            if (!resp.Success || resp.User == null) return Results.Unauthorized();
            return Results.Ok(resp.User);
        }).RequireAuthorization();

        app.MapGet("/api/users/me/permissions", async (HttpContext httpContext, IPermissionService permissionService) =>
        {
            var userIdClaim = httpContext.User.FindFirst("userid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Results.Unauthorized();
            }

            var permissions = await permissionService.GetAllUserPermissionsAsync(userId);
            return Results.Ok(permissions);
        }).RequireAuthorization();

        app.MapGet("/api/users/without-devotees", async (IUserService userService) =>
        {
            try
            {
                var users = await userService.GetUsersWithoutDevoteesAsync();
                return Results.Ok(users);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting users without devotees");
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        #endregion

        #region Temple Management Read Endpoints

        app.MapGet("/api/temples", async (ITempleService templeService) =>
        {
            try
            {
                var temples = await templeService.GetAllTemplesAsync();
                return Results.Ok(temples);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting all temples");
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/temples/{id}", async (int id, ITempleService templeService) =>
        {
            try
            {
                var temple = await templeService.GetTempleByIdAsync(id);
                if (temple == null)
                    return Results.NotFound();
                
                return Results.Ok(temple);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting temple with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/temples/search/{searchTerm}", async (string searchTerm, ITempleService templeService) =>
        {
            try
            {
                var temples = await templeService.SearchTemplesAsync(searchTerm);
                return Results.Ok(temples);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error searching temples with term {SearchTerm}", searchTerm);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/temples/location/{city}", async (string city, string? state, ITempleService templeService) =>
        {
            try
            {
                var temples = await templeService.GetTemplesByLocationAsync(city, state);
                return Results.Ok(temples);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting temples by location {City}", city);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/temples/{templeId}/devotees", async (int templeId, IDevoteeService devoteeService) =>
        {
            try
            {
                var devotees = await devoteeService.GetDevoteesByTempleAsync(templeId);
                return Results.Ok(devotees);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting devotees for temple {TempleId}", templeId);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/temples/{templeId}/donations", async (int templeId, IDonationService donationService) =>
        {
            try
            {
                var donations = await donationService.GetDonationsByTempleAsync(templeId);
                return Results.Ok(donations);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting donations for temple {TempleId}", templeId);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/temples/{templeId}/donations/total", async (int templeId, IDonationService donationService) =>
        {
            try
            {
                var total = await donationService.GetTotalDonationsByTempleAsync(templeId);
                return Results.Ok(new { total });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting total donations for temple {TempleId}", templeId);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/temples/{templeId}/events", async (int templeId, IEventService eventService) =>
        {
            try
            {
                var events = await eventService.GetEventsByTempleAsync(templeId);
                return Results.Ok(events);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting events for temple {TempleId}", templeId);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/temples/{templeId}/events/upcoming", async (int templeId, IEventService eventService) =>
        {
            try
            {
                var events = await eventService.GetUpcomingEventsAsync(templeId);
                return Results.Ok(events);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting upcoming events for temple {TempleId}", templeId);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/temples/{templeId}/areas", async (int templeId, IAreaService areaService) =>
        {
            try
            {
                var areas = await areaService.GetAreasByTempleAsync(templeId);
                return Results.Ok(areas);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting areas for temple {TempleId}", templeId);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Devotee Management Read Endpoints

        app.MapGet("/api/devotees", async (IDevoteeService devoteeService) =>
        {
            try
            {
                var devotees = await devoteeService.GetAllDevoteesAsync();
                return Results.Ok(devotees);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting all devotees");
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/devotees/{id}", async (int id, IDevoteeService devoteeService) =>
        {
            try
            {
                var devotee = await devoteeService.GetDevoteeByIdAsync(id);
                if (devotee == null)
                    return Results.NotFound();
                
                return Results.Ok(devotee);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting devotee with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/devotees/available-users", async (IUserService userService) =>
        {
            try
            {
                var users = await userService.GetUsersWithoutDevoteesAsync();
                return Results.Ok(users);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting available users for devotees");
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        app.MapGet("/api/devotees/search/{searchTerm}", async (string searchTerm, IDevoteeService devoteeService) =>
        {
            try
            {
                var devotees = await devoteeService.SearchDevoteesAsync(searchTerm);
                return Results.Ok(devotees);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error searching devotees with term {SearchTerm}", searchTerm);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/devotees/{devoteeId}/donations", async (int devoteeId, IDonationService donationService) =>
        {
            try
            {
                var donations = await donationService.GetDonationsByDevoteeAsync(devoteeId);
                return Results.Ok(donations);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting donations for devotee {DevoteeId}", devoteeId);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Donation Management Read Endpoints

        app.MapGet("/api/donations", async (IDonationService donationService) =>
        {
            try
            {
                var donations = await donationService.GetAllDonationsAsync();
                return Results.Ok(donations);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting all donations");
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/donations/{id}", async (int id, IDonationService donationService) =>
        {
            try
            {
                var donation = await donationService.GetDonationByIdAsync(id);
                if (donation == null)
                    return Results.NotFound();
                
                return Results.Ok(donation);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting donation with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Event Management Read Endpoints

        app.MapGet("/api/events", async (IEventService eventService) =>
        {
            try
            {
                var events = await eventService.GetAllEventsAsync();
                return Results.Ok(events);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting all events");
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/events/{id}", async (int id, IEventService eventService) =>
        {
            try
            {
                var eventEntity = await eventService.GetEventByIdAsync(id);
                if (eventEntity == null)
                    return Results.NotFound();
                
                return Results.Ok(eventEntity);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting event with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/events/search/{searchTerm}", async (string searchTerm, IEventService eventService) =>
        {
            try
            {
                var events = await eventService.SearchEventsAsync(searchTerm);
                return Results.Ok(events);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error searching events with term {SearchTerm}", searchTerm);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/events/{eventId}/vouchers", async (int eventId, IVoucherService svc) =>
        {
            var vouchers = await svc.GetVouchersByEventAsync(eventId);
            return Results.Ok(vouchers);
        });

        #endregion

        #region Area Management Read Endpoints

        app.MapGet("/api/areas", async (int? templeId, IAreaService areaService) =>
        {
            try
            {
                if (templeId.HasValue)
                {
                    var filtered = await areaService.GetAreasByTempleAsync(templeId.Value);
                    return Results.Ok(filtered);
                }
                var areas = await areaService.GetAllAreasAsync();
                return Results.Ok(areas);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting all areas");
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/areas/{id}", async (int id, IAreaService areaService) =>
        {
            try
            {
                var area = await areaService.GetAreaByIdAsync(id);
                if (area == null)
                    return Results.NotFound();
                return Results.Ok(area);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting area with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Event Type Read Endpoints

        app.MapGet("/api/event-types", async (IEventTypeService eventTypeService) =>
        {
            try
            {
                var types = await eventTypeService.GetAllAsync();
                return Results.Ok(types);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting event types");
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Expense Management Read Endpoints

        app.MapGet("/api/expense-items", async (IEventExpenseService svc) =>
        {
            var items = await svc.GetAllEventExpensesAsync();
            return Results.Ok(items);
        });

        app.MapGet("/api/expense-services", async (IExpenseServiceService svc) =>
        {
            var list = await svc.GetAllExpenseServicesAsync();
            return Results.Ok(list);
        });

        app.MapGet("/api/event-expenses", async (IExpenseService svc) =>
        {
            var expenses = await svc.GetAllExpensesAsync();
            return Results.Ok(expenses);
        });

        app.MapGet("/api/event-expenses/{id}", async (int id, IExpenseService svc) =>
        {
            var p = await svc.GetExpenseByIdAsync(id);
            if (p is null) return Results.NotFound();
            return Results.Ok(p);
        });

        app.MapGet("/api/event-expenses/{eventExpenseId}/vouchers", async (int eventExpenseId, IVoucherService svc) =>
        {
            var vouchers = await svc.GetVouchersByExpenseAsync(eventExpenseId);
            return Results.Ok(vouchers);
        });

        #endregion

        #region Voucher Management Read Endpoints

        app.MapGet("/api/vouchers", async (IVoucherService svc) =>
        {
            var vouchers = await svc.GetAllVouchersAsync();
            return Results.Ok(vouchers);
        });

        #endregion

        #region Product Management Read Endpoints

        app.MapGet("/api/products", async (IProductService productService) =>
        {
            try
            {
                var products = await productService.GetAllProductsAsync();
                return Results.Ok(products);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting all products");
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/products/{id}", async (int id, IProductService productService) =>
        {
            try
            {
                var product = await productService.GetProductByIdAsync(id);
                if (product == null)
                    return Results.NotFound();
                
                return Results.Ok(product);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting product with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/products/category/{category}", async (string category, IProductService productService) =>
        {
            try
            {
                var products = await productService.GetProductsByCategoryAsync(category);
                return Results.Ok(products);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting products by category {Category}", category);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/products/search/{searchTerm}", async (string searchTerm, IProductService productService) =>
        {
            try
            {
                var products = await productService.SearchProductsAsync(searchTerm);
                return Results.Ok(products);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error searching products with term {SearchTerm}", searchTerm);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Sales Management Read Endpoints

        app.MapGet("/api/sales", async (ISaleService saleService) =>
        {
            try
            {
                var sales = await saleService.GetAllSalesAsync();
                return Results.Ok(sales);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting all sales");
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/sales/{id}", async (int id, ISaleService saleService) =>
        {
            try
            {
                var sale = await saleService.GetSaleByIdAsync(id);
                if (sale == null)
                    return Results.NotFound();
                
                return Results.Ok(sale);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting sale with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/sales/customer/{customerId}", async (int customerId, ISaleService saleService) =>
        {
            try
            {
                var sales = await saleService.GetSalesByCustomerAsync(customerId);
                return Results.Ok(sales);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting sales for customer {CustomerId}", customerId);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/sales/staff/{staffId}", async (int staffId, ISaleService saleService) =>
        {
            try
            {
                var sales = await saleService.GetSalesByStaffAsync(staffId);
                return Results.Ok(sales);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting sales for staff {StaffId}", staffId);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/sales/date-range", async (DateTime startDate, DateTime endDate, ISaleService saleService) =>
        {
            try
            {
                var sales = await saleService.GetSalesByDateRangeAsync(startDate, endDate);
                return Results.Ok(sales);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting sales by date range");
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/sales/search/{searchTerm}", async (string searchTerm, ISaleService saleService) =>
        {
            try
            {
                var sales = await saleService.SearchSalesAsync(searchTerm);
                return Results.Ok(sales);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error searching sales with term {SearchTerm}", searchTerm);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Pooja Management Read Endpoints

        app.MapGet("/api/poojas", async (IPoojaService poojaService) =>
        {
            try
            {
                var poojas = await poojaService.GetAllPoojasAsync();
                return Results.Ok(poojas);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting all poojas");
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/poojas/{id}", async (int id, IPoojaService poojaService) =>
        {
            try
            {
                var pooja = await poojaService.GetPoojaByIdAsync(id);
                if (pooja == null)
                    return Results.NotFound();
                
                return Results.Ok(pooja);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting pooja with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Pooja Booking Read Endpoints

        app.MapGet("/api/pooja-bookings", async (IPoojaBookingService bookingService) =>
        {
            try
            {
                var bookings = await bookingService.GetAllBookingsAsync();
                return Results.Ok(bookings);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting all pooja bookings");
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/pooja-bookings/{id}", async (int id, IPoojaBookingService bookingService) =>
        {
            try
            {
                var booking = await bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                    return Results.NotFound();
                
                return Results.Ok(booking);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting pooja booking with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/pooja-bookings/customer/{customerId}", async (int customerId, IPoojaBookingService bookingService) =>
        {
            try
            {
                var bookings = await bookingService.GetBookingsByCustomerAsync(customerId);
                return Results.Ok(bookings);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting pooja bookings for customer {CustomerId}", customerId);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/pooja-bookings/staff/{staffId}", async (int staffId, IPoojaBookingService bookingService) =>
        {
            try
            {
                var bookings = await bookingService.GetBookingsByStaffAsync(staffId);
                return Results.Ok(bookings);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting pooja bookings for staff {StaffId}", staffId);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/pooja-bookings/status/{status}", async (TempleApi.Enums.BookingStatus status, IPoojaBookingService bookingService) =>
        {
            try
            {
                var bookings = await bookingService.GetBookingsByStatusAsync(status);
                return Results.Ok(bookings);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting pooja bookings by status {Status}", status);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/pooja-bookings/date-range", async (DateTime startDate, DateTime endDate, IPoojaBookingService bookingService) =>
        {
            try
            {
                var bookings = await bookingService.GetBookingsByDateRangeAsync(startDate, endDate);
                return Results.Ok(bookings);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting pooja bookings by date range");
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Category Management Read Endpoints

        app.MapGet("/api/categories", async (ICategoryService categoryService) =>
        {
            try
            {
                var categories = await categoryService.GetAllCategoriesAsync();
                return Results.Ok(categories);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting all categories");
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/categories/{id}", async (int id, ICategoryService categoryService) =>
        {
            try
            {
                var category = await categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                    return Results.NotFound();
                
                return Results.Ok(category);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting category with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        app.MapGet("/api/categories/active", async (ICategoryService categoryService) =>
        {
            try
            {
                var categories = await categoryService.GetActiveCategoriesAsync();
                return Results.Ok(categories);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting active categories");
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Configuration and Debug Read Endpoints

        app.MapGet("/api/database/info", (IOptions<DatabaseSettings> dbSettings) =>
        {
            var settings = dbSettings.Value;
            return Results.Ok(new
            {
                provider = settings.Provider,
                connectionString = settings.ConnectionString.Substring(0, Math.Min(50, settings.ConnectionString.Length)) + "...",
                enableMigrations = settings.EnableMigrations,
                commandTimeout = settings.CommandTimeout
            });
        });

        app.MapGet("/api/config/pages", (IConfiguration config) =>
        {
            var pages = config.GetSection("App:Pages").GetChildren()
                .Select(s => new { key = s["key"], name = s["name"] })
                .Where(p => !string.IsNullOrWhiteSpace(p.key) && !string.IsNullOrWhiteSpace(p.name))
                .ToList();
            return Results.Ok(pages);
        }).RequireAuthorization("UserRoleConfiguration");

        app.MapGet("/api/debug/seeding-status", async (TempleDbContext context) =>
        {
            var pagePermissionsCount = await context.PagePermissions.CountAsync();
            var uniquePages = await context.PagePermissions.Select(p => p.PageName).Distinct().CountAsync();
            var samplePages = await context.PagePermissions
                .Select(p => new { p.PageName, p.PageUrl })
                .Distinct()
                .Take(15)
                .ToListAsync();
            
            var rolesCount = await context.Roles.CountAsync();
            var roles = await context.Roles.Select(r => r.RoleName).ToListAsync();
            
            var usersCount = await context.Users.CountAsync();
            var adminExists = await context.Users.AnyAsync(u => u.Username == "admin");
            
            var templesCount = await context.Temples.CountAsync();
            
            return Results.Ok(new {
                PagePermissions = new {
                    Total = pagePermissionsCount,
                    UniquePages = uniquePages,
                    SamplePages = samplePages
                },
                Roles = new {
                    Total = rolesCount,
                    Names = roles
                },
                Users = new {
                    Total = usersCount,
                    AdminExists = adminExists
                },
                Temples = new {
                    Total = templesCount
                },
                Status = "✅ Seeding verification completed successfully!"
            });
        });

        app.MapGet("/api/debug/admin", async (TempleDbContext context) =>
        {
            var admin = await context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == "admin");

            if (admin == null)
            {
                return Results.NotFound(new { message = "Admin user not found" });
            }

            return Results.Ok(new
            {
                admin.UserId,
                admin.Username,
                admin.Email,
                admin.IsActive,
                admin.IsVerified,
                PasswordDecoded = SafeDecode(admin.PasswordHash),
                Roles = admin.UserRoles.Select(ur => ur.Role.RoleName).ToList()
            });

            static string SafeDecode(string base64)
            {
                try
                {
                    return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                }
                catch
                {
                    return "<invalid base64>";
                }
            }
        });

        #endregion

        #region Contribution Settings Read Endpoints

        app.MapGet("/api/contribution-settings", async (IContributionSettingService contributionSettingService) =>
        {
            try
            {
                var settings = await contributionSettingService.GetAllAsync();
                return Results.Ok(settings);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting all contribution settings");
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        app.MapGet("/api/contribution-settings/{id}", async (int id, IContributionSettingService contributionSettingService) =>
        {
            try
            {
                var setting = await contributionSettingService.GetByIdAsync(id);
                if (setting == null)
                    return Results.NotFound();
                
                return Results.Ok(setting);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting contribution setting with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        app.MapGet("/api/contribution-settings/event/{eventId}", async (int eventId, IContributionSettingService contributionSettingService) =>
        {
            try
            {
                var settings = await contributionSettingService.GetByEventIdAsync(eventId);
                return Results.Ok(settings);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting contribution settings for event {EventId}", eventId);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        app.MapGet("/api/contribution-settings/active", async (IContributionSettingService contributionSettingService) =>
        {
            try
            {
                var settings = await contributionSettingService.GetActiveContributionsAsync();
                return Results.Ok(settings);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting active contribution settings");
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        #endregion

        #region Contribution Read Endpoints

        app.MapGet("/api/contributions", async (IContributionService contributionService) =>
        {
            try
            {
                var contributions = await contributionService.GetAllAsync();
                return Results.Ok(contributions);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting all contributions");
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        app.MapGet("/api/contributions/{id}", async (int id, IContributionService contributionService) =>
        {
            try
            {
                var contribution = await contributionService.GetByIdAsync(id);
                if (contribution == null)
                    return Results.NotFound();
                
                return Results.Ok(contribution);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting contribution with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        app.MapGet("/api/contributions/event/{eventId}", async (int eventId, IContributionService contributionService) =>
        {
            try
            {
                var contributions = await contributionService.GetByEventIdAsync(eventId);
                return Results.Ok(contributions);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting contributions for event {EventId}", eventId);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        app.MapGet("/api/contributions/devotee/{devoteeId}", async (int devoteeId, IContributionService contributionService) =>
        {
            try
            {
                var contributions = await contributionService.GetByDevoteeIdAsync(devoteeId);
                return Results.Ok(contributions);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting contributions for devotee {DevoteeId}", devoteeId);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        app.MapGet("/api/contributions/summary", async (IContributionService contributionService) =>
        {
            try
            {
                var summary = await contributionService.GetContributionSummaryByEventAsync();
                return Results.Ok(summary);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting contribution summary");
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        app.MapGet("/api/contributions/total/{eventId}", async (int eventId, IContributionService contributionService) =>
        {
            try
            {
                var total = await contributionService.GetTotalContributionsByEventAsync(eventId);
                return Results.Ok(new { eventId, totalAmount = total });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting total contributions for event {EventId}", eventId);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        #endregion

        #region Inventory Management Read Endpoints

        app.MapGet("/api/inventories", async (IInventoryService inventoryService) =>
        {
            try
            {
                var inventories = await inventoryService.GetAllAsync();
                return Results.Ok(inventories);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting all inventories");
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        app.MapGet("/api/inventories/{id}", async (int id, IInventoryService inventoryService) =>
        {
            try
            {
                var inventory = await inventoryService.GetByIdAsync(id);
                if (inventory == null)
                    return Results.NotFound();
                
                return Results.Ok(inventory);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting inventory with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        app.MapGet("/api/inventories/temple/{templeId}", async (int templeId, IInventoryService inventoryService) =>
        {
            try
            {
                var inventories = await inventoryService.GetByTempleIdAsync(templeId);
                return Results.Ok(inventories);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting inventories for temple {TempleId}", templeId);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        app.MapGet("/api/inventories/area/{areaId}", async (int areaId, IInventoryService inventoryService) =>
        {
            try
            {
                var inventories = await inventoryService.GetByAreaIdAsync(areaId);
                return Results.Ok(inventories);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting inventories for area {AreaId}", areaId);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        app.MapGet("/api/inventories/worth/{itemWorth}", async (ItemWorth itemWorth, IInventoryService inventoryService) =>
        {
            try
            {
                var inventories = await inventoryService.GetByItemWorthAsync(itemWorth);
                return Results.Ok(inventories);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting inventories by worth {ItemWorth}", itemWorth);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        app.MapGet("/api/inventories/active", async (IInventoryService inventoryService) =>
        {
            try
            {
                var inventories = await inventoryService.GetActiveItemsAsync();
                return Results.Ok(inventories);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting active inventories");
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        app.MapGet("/api/inventories/area/{areaId}/value", async (int areaId, IInventoryService inventoryService) =>
        {
            try
            {
                var totalValue = await inventoryService.GetTotalValueByAreaAsync(areaId);
                return Results.Ok(new { areaId, totalValue });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting total value for area {AreaId}", areaId);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        app.MapGet("/api/inventories/temple/{templeId}/quantity", async (int templeId, IInventoryService inventoryService) =>
        {
            try
            {
                var totalQuantity = await inventoryService.GetTotalQuantityByTempleAsync(templeId);
                return Results.Ok(new { templeId, totalQuantity });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting total quantity for temple {TempleId}", templeId);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        #endregion

        #region Health Check

        app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

        #endregion
    }
}
