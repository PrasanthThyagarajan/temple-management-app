using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using TempleApi.Data;
using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using TempleApi.Services;
using TempleApi.Services.Interfaces;
using TempleApi.Models.DTOs;
using TempleApi.Configuration;
using TempleApi.Enums;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/temple-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configure database settings
var databaseSettings = builder.Configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>();
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(DatabaseSettings.SectionName));

// Configure Jyotisham API settings
builder.Services.Configure<JyotishamApiSettings>(builder.Configuration.GetSection(JyotishamApiSettings.SectionName));

// Register database context factory
builder.Services.AddScoped<IDbContextFactory, DbContextFactory>();

// Configure database context based on provider
var provider = Enum.Parse<DatabaseProvider>(databaseSettings?.Provider ?? "PostgreSQL");
var connectionString = databaseSettings?.ConnectionString ?? 
    builder.Configuration.GetConnectionString(provider.ToString()) ??
    builder.Configuration.GetConnectionString("PostgreSQL");

switch (provider)
{
    case DatabaseProvider.PostgreSQL:
        builder.Services.AddDbContext<TempleDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.CommandTimeout(databaseSettings?.CommandTimeout ?? 30);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            }));
        break;

    case DatabaseProvider.SQLite:
        builder.Services.AddDbContext<TempleDbContext>(options =>
            options.UseSqlite(connectionString, sqliteOptions =>
            {
                sqliteOptions.CommandTimeout(databaseSettings?.CommandTimeout ?? 30);
            }));
        break;

    case DatabaseProvider.SQLServer:
        builder.Services.AddDbContext<TempleDbContext>(options =>
            options.UseSqlServer(connectionString, sqlServerOptions =>
            {
                sqlServerOptions.CommandTimeout(databaseSettings?.CommandTimeout ?? 30);
                sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            }));
        break;

    case DatabaseProvider.MySQL:
        builder.Services.AddDbContext<TempleDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mySqlOptions =>
            {
                mySqlOptions.CommandTimeout(databaseSettings?.CommandTimeout ?? 30);
            }));
        break;

    default:
        throw new ArgumentException($"Unsupported database provider: {provider}");
}

// Register repositories
builder.Services.AddScoped<IRepository<Temple>, TempleApi.Repositories.Repository<Temple>>();
builder.Services.AddScoped<IRepository<Devotee>, TempleApi.Repositories.Repository<Devotee>>();
builder.Services.AddScoped<IRepository<Donation>, TempleApi.Repositories.Repository<Donation>>();
builder.Services.AddScoped<IRepository<Event>, TempleApi.Repositories.Repository<Event>>();
builder.Services.AddScoped<IRepository<EventRegistration>, TempleApi.Repositories.Repository<EventRegistration>>();
builder.Services.AddScoped<IRepository<Service>, TempleApi.Repositories.Repository<Service>>();

// Shop Management repositories
builder.Services.AddScoped<IUserRepository, TempleApi.Repositories.UserRepository>();
builder.Services.AddScoped<ICategoryRepository, TempleApi.Repositories.CategoryRepository>();
builder.Services.AddScoped<IProductRepository, TempleApi.Repositories.ProductRepository>();
builder.Services.AddScoped<ISaleRepository, TempleApi.Repositories.SaleRepository>();
builder.Services.AddScoped<IRepository<SaleItem>, TempleApi.Repositories.Repository<SaleItem>>();
builder.Services.AddScoped<IRepository<Pooja>, TempleApi.Repositories.Repository<Pooja>>();
builder.Services.AddScoped<IPoojaBookingRepository, TempleApi.Repositories.PoojaBookingRepository>();

// Register services
builder.Services.AddScoped<ITempleService, TempleService>();
builder.Services.AddScoped<IDevoteeService, DevoteeService>();
builder.Services.AddScoped<IDonationService, DonationService>();
builder.Services.AddScoped<IEventService, EventService>();

// Authentication services
builder.Services.AddScoped<IAuthService, AuthService>();

// Shop Management services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<IPoojaService, PoojaService>();
builder.Services.AddScoped<IPoojaBookingService, PoojaBookingService>();

// Jyotisham API service
builder.Services.AddHttpClient<IJyotishamApiService, JyotishamApiService>();

// Data seeding service
builder.Services.AddScoped<DataSeedingService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "http://localhost:5173", "http://localhost:5051", 
                          "http://127.0.0.1:5173", "http://127.0.0.1:5000", "http://127.0.0.1:5051")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"))),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// Add Controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowVueApp");

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TempleDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        if (databaseSettings?.EnableMigrations == true)
        {
            logger.LogInformation("Applying database migrations for provider: {Provider}", provider);
            context.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully");
        }
        else
        {
            logger.LogInformation("Ensuring database exists for provider: {Provider}", provider);
            context.Database.EnsureCreated();
            logger.LogInformation("Database ensured successfully");
        }
        
        // Seed initial data
        var dataSeedingService = scope.ServiceProvider.GetRequiredService<DataSeedingService>();
        await dataSeedingService.SeedDataAsync();
        logger.LogInformation("Initial data seeded successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database");
        throw;
    }
}

// Map controllers
app.MapControllers();

// Temple endpoints
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

// Devotee endpoints
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

app.MapPost("/api/devotees", async (CreateDevoteeDto createDto, IDevoteeService devoteeService) =>
{
    try
    {
        var devotee = await devoteeService.CreateDevoteeAsync(createDto);
        return Results.Created($"/api/devotees/{devotee.Id}", devotee);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error creating devotee");
        return Results.Problem("Internal server error");
    }
});

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

// Donation endpoints
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

// Event endpoints
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

// Role & Permission management endpoints
app.MapGet("/api/roles", async (TempleDbContext db) =>
{
    var roles = await db.Roles
        .Select(r => new { r.RoleId, r.RoleName, r.Description })
        .ToListAsync();
    return Results.Ok(roles);
}).RequireAuthorization("AdminOnly");

app.MapGet("/api/permissions", async (TempleDbContext db) =>
{
    var permissions = await db.Permissions
        .Select(p => new { p.PermissionId, p.PermissionName, p.Description })
        .ToListAsync();
    return Results.Ok(permissions);
}).RequireAuthorization("AdminOnly");

app.MapGet("/api/roles/{roleId}/permissions", async (int roleId, TempleDbContext db) =>
{
    var role = await db.Roles.FindAsync(roleId);
    if (role == null) return Results.NotFound();

    var permissionIds = await db.RolePermissions
        .Where(rp => rp.RoleId == roleId)
        .Select(rp => rp.PermissionId)
        .ToListAsync();

    return Results.Ok(new { roleId, permissionIds });
}).RequireAuthorization("AdminOnly");

app.MapPost("/api/roles/{roleId}/permissions", async (int roleId, UpdateRolePermissionsDto request, TempleDbContext db) =>
{
    if (roleId != request.RoleId) return Results.BadRequest("RoleId mismatch");
    var role = await db.Roles.FindAsync(roleId);
    if (role == null) return Results.NotFound("Role not found");

    var validPermissionIds = await db.Permissions
        .Where(p => request.PermissionIds.Contains(p.PermissionId))
        .Select(p => p.PermissionId)
        .ToListAsync();

    using var tx = await db.Database.BeginTransactionAsync();
    try
    {
        var existing = db.RolePermissions.Where(rp => rp.RoleId == roleId);
        db.RolePermissions.RemoveRange(existing);
        await db.SaveChangesAsync();

        var newMappings = validPermissionIds.Select(pid => new RolePermission
        {
            RoleId = roleId,
            PermissionId = pid,
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
}).RequireAuthorization("AdminOnly");

// User endpoints
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
});

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
});

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
});

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
});

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
});

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
});

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
});

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
});

// Product endpoints
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

// Sale endpoints
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

// Pooja endpoints
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

// Pooja Booking endpoints
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

// Category endpoints
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

// Jyotisham API endpoints
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

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

// Database info endpoint
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

app.Run();
