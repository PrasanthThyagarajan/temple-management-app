using Microsoft.EntityFrameworkCore;
using Serilog;
using TempleApi.Data;
using TempleApi.Services.Interfaces;
using TempleApi.Services;
using TempleApi.Models.DTOs;
using TempleApi.Domain.Entities;
using TempleApi.Enums;

namespace TempleApi.Endpoints;

public static class EditApis
{
    public static void MapEditEndpoints(this WebApplication app)
    {
        #region Role Management Edit Endpoints

        app.MapPut("/api/roles/{id:int}", async (int id, Role role, IRoleService roleService) =>
        {
            if (id != role.RoleId) return Results.BadRequest();
            var updated = await roleService.UpdateRoleAsync(role);
            return Results.Ok(updated);
        }).RequireAuthorization("UserRoleConfiguration");

        app.MapPut("/api/userroles/{id:int}", async (int id, TempleApi.Domain.Entities.UserRole userRole, IUserRoleService userRoleService) =>
        {
            if (id != userRole.UserRoleId)
            {
                return Results.BadRequest();
            }

            var updatedUserRole = await userRoleService.UpdateUserRoleAsync(userRole);
            return Results.Ok(updatedUserRole);
        }).RequireAuthorization("UserRoleConfiguration");

        #endregion

        #region User Management Edit Endpoints

        app.MapPut("/api/users/{id}", async (int id, CreateUserDto updateDto, IUserService userService) =>
        {
            try
            {
                var user = await userService.UpdateUserAsync(id, updateDto);
                return Results.Ok(user);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating user with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization("UserRoleConfiguration");

        app.MapPut("/api/users/me", async (HttpContext httpContext, CreateUserDto updateDto, TempleApi.Data.TempleDbContext db, IAuthService authService) =>
        {
            // Identify user via Basic auth context
            var identityName = httpContext.User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(identityName)) return Results.Unauthorized();

            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == identityName || u.Email == identityName);
            if (user == null) return Results.NotFound();

            // Update selected profile fields
            user.Email = updateDto.Email;
            user.FullName = updateDto.Name;
            user.Gender = updateDto.Gender;
            user.PhoneNumber = updateDto.PhoneNumber;
            user.Address = updateDto.Address;
            user.Nakshatra = updateDto.Nakshatra;
            user.DateOfBirth = updateDto.DateOfBirth;

            // Optional: if password provided, update
            if (!string.IsNullOrWhiteSpace(updateDto.Password))
            {
                user.PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(updateDto.Password));
            }

            await db.SaveChangesAsync();

            var roles = await authService.GetUserRolesAsync(user.UserId);
            var result = new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Nakshatra = user.Nakshatra,
                DateOfBirth = user.DateOfBirth,
                Roles = roles
            };

            return Results.Ok(result);
        }).RequireAuthorization();

        #endregion

        #region Temple Management Edit Endpoints

        app.MapPut("/api/temples/{id}", async (int id, CreateTempleDto updateDto, ITempleService templeService) =>
        {
            try
            {
                var temple = await templeService.UpdateTempleAsync(id, updateDto);
                if (temple == null)
                    return Results.NotFound();
                
                return Results.Ok(temple);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating temple with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Devotee Management Edit Endpoints

        app.MapPut("/api/devotees/{id}", async (int id, CreateDevoteeDto updateDto, IDevoteeService devoteeService) =>
        {
            try
            {
                var devotee = await devoteeService.UpdateDevoteeAsync(id, updateDto);
                if (devotee == null)
                    return Results.NotFound();
                
                return Results.Ok(devotee);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating devotee with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Donation Management Edit Endpoints

        app.MapPut("/api/donations/{id}/status", async (int id, string status, IDonationService donationService) =>
        {
            try
            {
                var donation = await donationService.UpdateDonationStatusAsync(id, status);
                if (donation == null)
                    return Results.NotFound();
                
                return Results.Ok(donation);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating donation status for id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Event Management Edit Endpoints

        app.MapPut("/api/events/{id}", async (int id, CreateEventDto updateDto, IEventService eventService) =>
        {
            try
            {
                var eventEntity = await eventService.UpdateEventAsync(id, updateDto);
                if (eventEntity == null)
                    return Results.NotFound();
                
                return Results.Ok(eventEntity);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating event with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        app.MapPut("/api/events/{id}/status", async (int id, string status, IEventService eventService) =>
        {
            try
            {
                var result = await eventService.UpdateEventStatusAsync(id, status);
                if (!result)
                    return Results.NotFound();
                
                return Results.Ok(new { message = "Event status updated successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating event status for id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Area Management Edit Endpoints

        app.MapPut("/api/areas/{id}", async (int id, CreateAreaDto updateDto, IAreaService areaService) =>
        {
            try
            {
                var area = await areaService.UpdateAreaAsync(id, updateDto);
                if (area == null)
                    return Results.NotFound();
                
                return Results.Ok(area);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating area with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Expense Management Edit Endpoints

        app.MapPut("/api/expense-items/{id}", async (int id, UpdateEventExpenseDto dto, IEventExpenseService svc) =>
        {
            var item = await svc.UpdateEventExpenseAsync(id, dto);
            return Results.Ok(item);
        });

        app.MapPut("/api/expense-services/{id}", async (int id, UpdateExpenseServiceDto dto, IExpenseServiceService svc) =>
        {
            var updated = await svc.UpdateExpenseServiceAsync(id, dto);
            return Results.Ok(updated);
        });

        app.MapPut("/api/event-expenses/{id}", async (int id, UpdateExpenseDto dto, IExpenseService svc) =>
        {
            var updated = await svc.UpdateExpenseAsync(id, dto);
            return Results.Ok(updated);
        });

        app.MapPut("/api/event-expenses/{id}/approve", async (int id, IExpenseService svc) =>
        {
            try
            {
                // For now, using a default user ID of 1. In a real app, this would come from authentication
                var approved = await svc.ApproveExpenseAsync(id, 1);
                return Results.Ok(approved);
            }
            catch (ArgumentException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error approving expense with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Product Management Edit Endpoints

        app.MapPut("/api/products/{id}", async (int id, CreateProductDto updateDto, IProductService productService) =>
        {
            try
            {
                var product = await productService.UpdateProductAsync(id, updateDto);
                return Results.Ok(product);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating product with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        app.MapPut("/api/products/{id}/quantity", async (int id, int quantity, IProductService productService) =>
        {
            try
            {
                var result = await productService.UpdateProductQuantityAsync(id, quantity);
                if (!result)
                    return Results.NotFound();
                
                return Results.Ok(new { message = "Product quantity updated successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating product quantity for id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        app.MapPut("/api/products/{id}/status", async (int id, string status, IProductService productService) =>
        {
            try
            {
                var result = await productService.UpdateProductStatusAsync(id, status);
                if (!result)
                    return Results.NotFound();
                
                return Results.Ok(new { message = "Product status updated successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating product status for id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Sales Management Edit Endpoints

        app.MapPut("/api/sales/{id}", async (int id, CreateSaleDto updateDto, ISaleService saleService) =>
        {
            try
            {
                var sale = await saleService.UpdateSaleAsync(id, updateDto);
                return Results.Ok(sale);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating sale with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        app.MapPut("/api/sales/{id}/status", async (int id, string status, ISaleService saleService) =>
        {
            try
            {
                var result = await saleService.UpdateSaleStatusAsync(id, status);
                if (!result)
                    return Results.NotFound();
                
                return Results.Ok(new { message = "Sale status updated successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating sale status for id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Pooja Management Edit Endpoints

        app.MapPut("/api/poojas/{id}", async (int id, CreatePoojaDto updateDto, IPoojaService poojaService) =>
        {
            try
            {
                var pooja = await poojaService.UpdatePoojaAsync(id, updateDto);
                return Results.Ok(pooja);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating pooja with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Pooja Booking Edit Endpoints

        app.MapPut("/api/pooja-bookings/{id}/status", async (int id, TempleApi.Enums.BookingStatus status, IPoojaBookingService bookingService) =>
        {
            try
            {
                var booking = await bookingService.UpdateBookingStatusAsync(id, status);
                return Results.Ok(booking);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating pooja booking status for id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        app.MapPut("/api/pooja-bookings/{id}/assign-staff", async (int id, int staffId, IPoojaBookingService bookingService) =>
        {
            try
            {
                var booking = await bookingService.AssignStaffToBookingAsync(id, staffId);
                return Results.Ok(booking);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error assigning staff to pooja booking for id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Category Management Edit Endpoints

        app.MapPut("/api/categories/{id}", async (int id, CreateCategoryDto updateDto, ICategoryService categoryService) =>
        {
            try
            {
                var category = await categoryService.UpdateCategoryAsync(id, updateDto);
                if (category == null)
                    return Results.NotFound();
                
                return Results.Ok(category);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating category with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        app.MapPut("/api/categories/{id}/toggle-status", async (int id, ICategoryService categoryService) =>
        {
            try
            {
                var result = await categoryService.ToggleCategoryStatusAsync(id);
                if (!result)
                    return Results.NotFound();
                
                return Results.Ok(new { message = "Category status updated successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error toggling category status for id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Contribution Settings Edit Endpoints

        app.MapPut("/api/contribution-settings/{id}", async (int id, CreateContributionSettingDto updateDto, IContributionSettingService contributionSettingService) =>
        {
            try
            {
                var setting = await contributionSettingService.UpdateAsync(id, updateDto);
                if (setting == null)
                    return Results.NotFound();
                
                return Results.Ok(setting);
            }
            catch (InvalidOperationException ex)
            {
                Log.Warning(ex, "Validation error updating contribution setting {Id}", id);
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating contribution setting {Id}", id);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        #endregion

        #region Contribution Edit Endpoints

        app.MapPut("/api/contributions/{id}", async (int id, CreateContributionDto updateDto, IContributionService contributionService) =>
        {
            try
            {
                var contribution = await contributionService.UpdateAsync(id, updateDto);
                if (contribution == null)
                    return Results.NotFound();
                
                return Results.Ok(contribution);
            }
            catch (InvalidOperationException ex)
            {
                Log.Warning(ex, "Validation error updating contribution {Id}", id);
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating contribution {Id}", id);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        #endregion
    }
}
