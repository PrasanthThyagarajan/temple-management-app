using Microsoft.EntityFrameworkCore;
using Serilog;
using TempleApi.Data;
using TempleApi.Services.Interfaces;
using TempleApi.Services;
using TempleApi.Models.DTOs;
using TempleApi.Domain.Entities;
using TempleApi.Enums;
using System.Security.Claims;
using System.Text;

namespace TempleApi.Endpoints;

public static class CreateApis
{
    public static void MapCreateEndpoints(this WebApplication app)
    {
        #region Authentication Create Endpoints

        app.MapPost("/api/auth/login", async (HttpContext httpContext, IAuthService authService, ILogger<Program> logger) =>
        {
            try
            {
                // If middleware already authenticated the user, use that
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    {
                        return Results.Json(new AuthResponse { Success = false, Message = "Authentication failed" }, statusCode: 401);
                    }

                    var userDto = await authService.GetUserByIdAsync(userId);
                    if (userDto == null)
                    {
                        return Results.Json(new AuthResponse { Success = false, Message = "User not found" }, statusCode: 404);
                    }

                    var roles = await authService.GetUserRolesAsync(userId);
                    var permissions = await authService.GetUserPermissionsAsync(userId);

                    return Results.Ok(new AuthResponse
                    {
                        Success = true,
                        Message = "Login successful",
                        Token = string.Empty,
                        User = userDto,
                        Roles = roles,
                        Permissions = permissions
                    });
                }

                // Fallback: manually handle Basic header (useful for tests)
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

                return Results.Ok(resp);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in login endpoint");
                return Results.Json(new AuthResponse
                {
                    Success = false,
                    Message = "An internal error occurred"
                }, statusCode: 500);
            }
        });

        app.MapPost("/api/auth/register", async (RegisterRequest request, IAuthService authService, ILogger<Program> logger) =>
        {
            try
            {
                var result = await authService.RegisterAsync(request);
                
                if (!result.Success)
                {
                    return Results.BadRequest(result);
                }

                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in register endpoint");
                return Results.Json(new AuthResponse
                {
                    Success = false,
                    Message = "An internal error occurred"
                }, statusCode: 500);
            }
        });

        #endregion

        #region Role Management Create Endpoints

        app.MapPost("/api/roles", async (Role role, IRoleService roleService) =>
        {
            var created = await roleService.CreateRoleAsync(role);
            return Results.Created($"/api/roles/{created.RoleId}", created);
        }).RequireAuthorization("UserRoleConfiguration");

        app.MapPost("/api/userroles", async (TempleApi.Domain.Entities.UserRole userRole, IUserRoleService userRoleService) =>
        {
            var createdUserRole = await userRoleService.CreateUserRoleAsync(userRole);
            return Results.Created($"/api/userroles/{createdUserRole.UserRoleId}", createdUserRole);
        }).RequireAuthorization("UserRoleConfiguration");

        app.MapPost("/api/roles/{roleId}/permissions", async (int roleId, UpdateRolePermissionsDto request, TempleDbContext db) =>
        {
            if (roleId != request.RoleId) return Results.BadRequest("RoleId mismatch");
            var role = await db.Roles.FindAsync(roleId);
            if (role == null) return Results.NotFound("Role not found");
            if (string.Equals(role.RoleName, "Admin", StringComparison.OrdinalIgnoreCase))
                return Results.BadRequest("Admin role permissions cannot be modified.");

            var validPagePermissionIds = await db.PagePermissions
                .Where(p => request.PagePermissionIds.Contains(p.PagePermissionId))
                .Select(p => p.PagePermissionId)
                .ToListAsync();

            using var tx = await db.Database.BeginTransactionAsync();
            try
            {
                var existing = db.RolePermissions.Where(rp => rp.RoleId == roleId);
                db.RolePermissions.RemoveRange(existing);
                await db.SaveChangesAsync();

                var newMappings = validPagePermissionIds.Select(pid => new RolePermission
                {
                    RoleId = roleId,
                    PagePermissionId = pid,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
                await db.RolePermissions.AddRangeAsync(newMappings);
                await db.SaveChangesAsync();

                await tx.CommitAsync();
                return Results.Ok(new { message = "Role permissions updated" });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                Log.Error(ex, "Error updating permissions for role {RoleId}", roleId);
                return Results.Problem("Failed to update role permissions");
            }
        }).RequireAuthorization("UserRoleConfiguration");

        #endregion

        #region User Management Create Endpoints

        app.MapPost("/api/users", async (CreateUserDto createDto, IUserService userService) =>
        {
            try
            {
                var user = await userService.CreateUserAsync(createDto);
                return Results.Created($"/api/users/{user.UserId}", user);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating user");
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization("UserRoleConfiguration");

        app.MapPost("/api/users/{id:int}/reset-password", async (int id, IUserService userService) =>
        {
            try
            {
                var newPassword = await userService.ResetPasswordAsync(id);
                return Results.Ok(new { newPassword });
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error resetting password for user {Id}", id);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization("UserRoleConfiguration");

        app.MapPost("/api/users/validate", async (LoginRequest request, IUserService userService) =>
        {
            try
            {
                var isValid = await userService.ValidateUserCredentialsAsync(request.Username, request.Password);
                return Results.Ok(new { isValid });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error validating user credentials");
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization("UserRoleConfiguration");

        #endregion

        #region Temple Management Create Endpoints

        app.MapPost("/api/temples", async (CreateTempleDto createDto, ITempleService templeService) =>
        {
            try
            {
                var temple = await templeService.CreateTempleAsync(createDto);
                return Results.Created($"/api/temples/{temple.Id}", temple);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating temple");
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Devotee Management Create Endpoints

        app.MapPost("/api/devotees", async (CreateDevoteeDto createDto, IDevoteeService devoteeService) =>
        {
            try
            {
                var (devotee, generatedPassword) = await devoteeService.CreateDevoteeWithUserAsync(createDto);
                if (devotee == null)
                {
                    return Results.Problem("Failed to create devotee");
                }
                return Results.Created($"/api/devotees/{devotee.Id}", new { devotee, generatedPassword });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating devotee");
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Donation Management Create Endpoints

        app.MapPost("/api/donations", async (CreateDonationDto createDto, IDonationService donationService) =>
        {
            try
            {
                var donation = await donationService.CreateDonationAsync(createDto);
                return Results.Created($"/api/donations/{donation.Id}", donation);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating donation");
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Event Management Create Endpoints

        app.MapPost("/api/events", async (CreateEventDto createDto, IEventService eventService) =>
        {
            try
            {
                var eventEntity = await eventService.CreateEventAsync(createDto);
                return Results.Created($"/api/events/{eventEntity.Id}", eventEntity);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating event");
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Area Management Create Endpoints

        app.MapPost("/api/areas", async (CreateAreaDto createDto, IAreaService areaService) =>
        {
            try
            {
                var area = await areaService.CreateAreaAsync(createDto);
                return Results.Created($"/api/areas/{area.Id}", area);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating area");
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Expense Management Create Endpoints

        app.MapPost("/api/expense-items", async (CreateEventExpenseDto dto, IEventExpenseService svc) =>
        {
            var item = await svc.CreateEventExpenseAsync(dto);
            return Results.Created($"/api/expense-items/{item.Id}", item);
        });

        app.MapPost("/api/expense-services", async (CreateExpenseServiceDto dto, IExpenseServiceService svc) =>
        {
            var created = await svc.CreateExpenseServiceAsync(dto);
            return Results.Created($"/api/expense-services/{created.Id}", created);
        });

        app.MapPost("/api/event-expenses", async (CreateExpenseDto dto, IExpenseService svc) =>
        {
            var created = await svc.CreateExpenseAsync(dto);
            return Results.Created($"/api/event-expenses/{created.Id}", created);
        });

        #endregion

        #region Product Management Create Endpoints

        app.MapPost("/api/products", async (CreateProductDto createDto, IProductService productService) =>
        {
            try
            {
                var product = await productService.CreateProductAsync(createDto);
                return Results.Created($"/api/products/{product.Id}", product);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating product");
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Sales Management Create Endpoints

        app.MapPost("/api/sales", async (CreateSaleDto createDto, ISaleService saleService) =>
        {
            try
            {
                var sale = await saleService.CreateSaleAsync(createDto);
                return Results.Created($"/api/sales/{sale.Id}", sale);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating sale");
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Pooja Management Create Endpoints

        app.MapPost("/api/poojas", async (CreatePoojaDto createDto, IPoojaService poojaService) =>
        {
            try
            {
                var pooja = await poojaService.CreatePoojaAsync(createDto);
                return Results.Created($"/api/poojas/{pooja.Id}", pooja);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating pooja");
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Pooja Booking Create Endpoints

        app.MapPost("/api/pooja-bookings", async (CreatePoojaBookingDto createDto, IPoojaBookingService bookingService) =>
        {
            try
            {
                var booking = await bookingService.CreateBookingAsync(createDto);
                return Results.Created($"/api/pooja-bookings/{booking.Id}", booking);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating pooja booking");
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Category Management Create Endpoints

        app.MapPost("/api/categories", async (CreateCategoryDto createDto, ICategoryService categoryService) =>
        {
            try
            {
                var category = await categoryService.CreateCategoryAsync(createDto);
                return Results.Created($"/api/categories/{category.Id}", category);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating category");
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Jyotisham API Create Endpoints

        app.MapPost("/api/panchang", async (PanchangRequestDto request, IJyotishamApiService jyotishamService) =>
        {
            try
            {
                var result = await jyotishamService.GetPanchangAsync(request);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting Panchang data");
                return Results.Problem("Internal server error");
            }
        });

        app.MapPost("/api/horoscope/daily", async (HoroscopeRequestDto request, IJyotishamApiService jyotishamService) =>
        {
            try
            {
                var result = await jyotishamService.GetDailyHoroscopeAsync(request);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting daily horoscope");
                return Results.Problem("Internal server error");
            }
        });

        app.MapPost("/api/horoscope/weekly", async (HoroscopeRequestDto request, IJyotishamApiService jyotishamService) =>
        {
            try
            {
                var result = await jyotishamService.GetWeeklyHoroscopeAsync(request);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting weekly horoscope");
                return Results.Problem("Internal server error");
            }
        });

        app.MapPost("/api/horoscope/monthly", async (HoroscopeRequestDto request, IJyotishamApiService jyotishamService) =>
        {
            try
            {
                var result = await jyotishamService.GetMonthlyHoroscopeAsync(request);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting monthly horoscope");
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Debug and Testing Create Endpoints

        app.MapPost("/api/debug/send-email", async (EmailSendRequest req, IConfiguration config, ILogger<Program> logger) =>
        {
            try
            {
                var emailSvc = new EmailService(config, logger);
                await emailSvc.SendAsync(req.To, req.Subject, req.Body);
                return Results.Ok(new { message = $"Email sent to {req.To}" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send test email to {To}", req.To);
                return Results.Problem($"Failed to send email: {ex.Message}");
            }
        });

        #endregion
    }

    public record EmailSendRequest(string To, string Subject, string Body);
}
