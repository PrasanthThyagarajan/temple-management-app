using Serilog;
using TempleApi.Services.Interfaces;

namespace TempleApi.Endpoints;

public static class DeleteApis
{
    public static void MapDeleteEndpoints(this WebApplication app)
    {
        #region Role Management Delete Endpoints

        app.MapDelete("/api/roles/{id:int}", async (int id, IRoleService roleService) =>
        {
            var ok = await roleService.DeleteRoleAsync(id);
            if (!ok) return Results.NotFound();
            return Results.NoContent();
        }).RequireAuthorization("UserRoleConfiguration");

        app.MapDelete("/api/userroles/{id:int}", async (int id, IUserRoleService userRoleService) =>
        {
            var success = await userRoleService.DeleteUserRoleAsync(id);
            if (!success)
            {
                return Results.NotFound();
            }

            return Results.NoContent();
        }).RequireAuthorization("UserRoleConfiguration");

        #endregion

        #region User Management Delete Endpoints

        app.MapDelete("/api/users/{id}", async (int id, IUserService userService) =>
        {
            try
            {
                var result = await userService.DeleteUserAsync(id);
                if (!result)
                    return Results.NotFound();
                
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting user with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization("UserRoleConfiguration");

        #endregion

        #region Temple Management Delete Endpoints

        app.MapDelete("/api/temples/{id}", async (int id, ITempleService templeService) =>
        {
            try
            {
                var result = await templeService.DeleteTempleAsync(id);
                if (!result)
                    return Results.NotFound();
                
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting temple with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Devotee Management Delete Endpoints

        app.MapDelete("/api/devotees/{id}", async (int id, IDevoteeService devoteeService) =>
        {
            try
            {
                var result = await devoteeService.DeleteDevoteeAsync(id);
                if (!result)
                    return Results.NotFound();
                
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting devotee with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Donation Management Delete Endpoints

        app.MapDelete("/api/donations/{id}", async (int id, IDonationService donationService) =>
        {
            try
            {
                var result = await donationService.DeleteDonationAsync(id);
                if (!result)
                    return Results.NotFound();
                
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting donation with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Event Management Delete Endpoints

        app.MapDelete("/api/events/{id}", async (int id, IEventService eventService) =>
        {
            try
            {
                var result = await eventService.DeleteEventAsync(id);
                if (!result)
                    return Results.NotFound();
                
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting event with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Expense Management Delete Endpoints

        app.MapDelete("/api/expense-items/{id}", async (int id, IEventExpenseService svc) =>
        {
            var ok = await svc.DeleteEventExpenseAsync(id);
            return ok ? Results.NoContent() : Results.NotFound();
        });

        app.MapDelete("/api/expense-services/{id}", async (int id, IExpenseServiceService svc) =>
        {
            var ok = await svc.DeleteExpenseServiceAsync(id);
            return ok ? Results.NoContent() : Results.NotFound();
        });

        app.MapDelete("/api/event-expenses/{id}", async (int id, IExpenseService svc) =>
        {
            var ok = await svc.DeleteExpenseAsync(id);
            return ok ? Results.NoContent() : Results.NotFound();
        });

        #endregion

        #region Product Management Delete Endpoints

        app.MapDelete("/api/products/{id}", async (int id, IProductService productService) =>
        {
            try
            {
                var result = await productService.DeleteProductAsync(id);
                if (!result)
                    return Results.NotFound();
                
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting product with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Sales Management Delete Endpoints

        app.MapDelete("/api/sales/{id}", async (int id, ISaleService saleService) =>
        {
            try
            {
                var result = await saleService.DeleteSaleAsync(id);
                if (!result)
                    return Results.NotFound();
                
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting sale with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Booking Delete Endpoints

        app.MapDelete("/api/bookings/{id}", async (int id, IBookingService bookingService) =>
        {
            try
            {
                var ok = await bookingService.DeleteAsync(id);
                return ok ? Results.NoContent() : Results.NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting booking with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Pooja Management Delete Endpoints

        app.MapDelete("/api/poojas/{id}", async (int id, IPoojaService poojaService) =>
        {
            try
            {
                var result = await poojaService.DeletePoojaAsync(id);
                if (!result)
                    return Results.NotFound();
                
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting pooja with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Pooja Booking Delete Endpoints

        app.MapDelete("/api/pooja-bookings/{id}", async (int id, IPoojaBookingService bookingService) =>
        {
            try
            {
                var result = await bookingService.DeleteBookingAsync(id);
                if (!result)
                    return Results.NotFound();
                
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting pooja booking with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Category Management Delete Endpoints

        app.MapDelete("/api/categories/{id}", async (int id, ICategoryService categoryService) =>
        {
            try
            {
                var result = await categoryService.DeleteCategoryAsync(id);
                if (!result)
                    return Results.NotFound();
                
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting category with id {Id}", id);
                return Results.Problem("Internal server error");
            }
        });

        #endregion

        #region Contribution Settings Delete Endpoints

        app.MapDelete("/api/contribution-settings/{id}", async (int id, IContributionSettingService contributionSettingService) =>
        {
            try
            {
                var result = await contributionSettingService.DeleteAsync(id);
                if (!result)
                    return Results.NotFound();
                
                return Results.Ok(new { message = "Contribution setting deleted successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting contribution setting {Id}", id);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        #endregion

        #region Contribution Delete Endpoints

        app.MapDelete("/api/contributions/{id}", async (int id, IContributionService contributionService) =>
        {
            try
            {
                var result = await contributionService.DeleteAsync(id);
                if (!result)
                    return Results.NotFound();
                
                return Results.Ok(new { message = "Contribution deleted successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting contribution {Id}", id);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        #endregion

        #region Inventory Management Delete Endpoints

        app.MapDelete("/api/inventories/{id}", async (int id, IInventoryService inventoryService) =>
        {
            try
            {
                var result = await inventoryService.DeleteAsync(id);
                if (!result)
                    return Results.NotFound();
                
                return Results.Ok(new { message = "Inventory item deleted successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting inventory {Id}", id);
                return Results.Problem("Internal server error");
            }
        }).RequireAuthorization();

        #endregion
    }
}
