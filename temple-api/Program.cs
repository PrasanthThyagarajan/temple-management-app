using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using TempleApi.Security;
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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Claims;

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

// Ensure SQLite database directory exists when using a relative path like "Database/..."
if (provider == DatabaseProvider.SQLite)
{
    var dbDir = System.IO.Path.Combine(builder.Environment.ContentRootPath, "Database");
    if (!System.IO.Directory.Exists(dbDir))
    {
        System.IO.Directory.CreateDirectory(dbDir);
    }
}

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
builder.Services.AddScoped(typeof(IRepository<>), typeof(TempleApi.Repositories.Repository<>));
builder.Services.AddScoped<IRepository<Temple>, TempleApi.Repositories.Repository<Temple>>();
builder.Services.AddScoped<IRepository<Devotee>, TempleApi.Repositories.Repository<Devotee>>();
builder.Services.AddScoped<IRepository<Donation>, TempleApi.Repositories.Repository<Donation>>();
builder.Services.AddScoped<IRepository<Event>, TempleApi.Repositories.Repository<Event>>();
builder.Services.AddScoped<IRepository<EventRegistration>, TempleApi.Repositories.Repository<EventRegistration>>();
builder.Services.AddScoped<IRepository<Service>, TempleApi.Repositories.Repository<Service>>();
builder.Services.AddScoped<TempleApi.Repositories.Interfaces.IRepository<TempleApi.Domain.Entities.ExpenseService>, TempleApi.Repositories.Repository<TempleApi.Domain.Entities.ExpenseService>>();
builder.Services.AddScoped<IRepository<User>, TempleApi.Repositories.Repository<User>>();
builder.Services.AddScoped<IRepository<TempleApi.Domain.Entities.UserRole>, TempleApi.Repositories.Repository<TempleApi.Domain.Entities.UserRole>>();
builder.Services.AddScoped<IRepository<Role>, TempleApi.Repositories.Repository<Role>>();

// Shop Management repositories
builder.Services.AddScoped<IUserRepository, TempleApi.Repositories.UserRepository>();
builder.Services.AddScoped<ICategoryRepository, TempleApi.Repositories.CategoryRepository>();
builder.Services.AddScoped<IProductRepository, TempleApi.Repositories.ProductRepository>();
builder.Services.AddScoped<ISaleRepository, TempleApi.Repositories.SaleRepository>();
builder.Services.AddScoped<IRepository<SaleItem>, TempleApi.Repositories.Repository<SaleItem>>();
builder.Services.AddScoped<IRepository<Pooja>, TempleApi.Repositories.Repository<Pooja>>();
builder.Services.AddScoped<IPoojaBookingRepository, TempleApi.Repositories.PoojaBookingRepository>();

// Role & Permission management repositories
builder.Services.AddScoped<IRoleRepository, TempleApi.Repositories.RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();

// Register services
builder.Services.AddScoped<ITempleService, TempleService>();
builder.Services.AddScoped<IDevoteeService, DevoteeService>();
builder.Services.AddScoped<IDonationService, DonationService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IAreaService, AreaService>();
builder.Services.AddScoped<IEventTypeService, EventTypeService>();

// Authentication services
builder.Services.AddScoped<IAuthService, AuthService>();

// Shop Management services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<IPoojaService, PoojaService>();
builder.Services.AddScoped<IPoojaBookingService, PoojaBookingService>();
builder.Services.AddScoped<IExpenseService, TempleApi.Services.ExpenseService>();
builder.Services.AddScoped<IEventExpenseService, EventExpenseService>();
builder.Services.AddScoped<IExpenseServiceService, TempleApi.Services.ExpenseServiceService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();

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

// Replace JWT with Basic Authentication
builder.Services.AddAuthentication("BasicAuthentication")
	.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserRoleConfiguration", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") ||
            context.User.HasClaim("permission", "UserRoleConfiguration")
        );
    });
});

// Add Controllers with JSON options to prevent reference loops
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Avoid $id/$ref payloads that break simple clients; ignore cycles instead
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Configure JSON options for minimal APIs
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Avoid redirecting to HTTPS in dev to keep proxy/simple http working
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
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
        var isRelational = context.Database.IsRelational();
        var hasAnyMigrations = isRelational && context.Database.GetMigrations().Any();

        if (databaseSettings?.EnableMigrations == true && hasAnyMigrations)
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

            try
            {
                if (!isRelational)
                {
                    // Skip relational-only operations for InMemory provider
                    goto SeedData;
                }
                var databaseCreator = (IRelationalDatabaseCreator)context.Database.GetService<IDatabaseCreator>();

                if (!databaseCreator.Exists())
                {
                    databaseCreator.Create();
                    logger.LogInformation("Database created successfully");
                }

                // Some providers may not implement HasTables; guard via try/catch
                var relationalCreator = databaseCreator as RelationalDatabaseCreator;
                var hasTables = false;
                try
                {
                    hasTables = relationalCreator != null && relationalCreator.HasTables();
                }
                catch
                {
                    // ignore
                }

                if (!hasTables)
                {
                    databaseCreator.CreateTables();
                    logger.LogInformation("Database tables created successfully");
                }
            }
            catch (Exception ex)
            {
                // Tables may already exist; log at debug level
                logger.LogDebug(ex, "CreateTables skipped or failed (may already exist)");
            }
        }
        
SeedData:
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

// Auth API endpoints
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

// Remove token validation endpoint in Basic auth mode

// Remove refresh token endpoint in Basic auth mode

// Role API endpoints
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

app.MapPost("/api/roles", async (Role role, IRoleService roleService) =>
{
    var created = await roleService.CreateRoleAsync(role);
    return Results.Created($"/api/roles/{created.RoleId}", created);
}).RequireAuthorization("UserRoleConfiguration");

app.MapPut("/api/roles/{id:int}", async (int id, Role role, IRoleService roleService) =>
{
    if (id != role.RoleId) return Results.BadRequest();
    var updated = await roleService.UpdateRoleAsync(role);
    return Results.Ok(updated);
}).RequireAuthorization("UserRoleConfiguration");

app.MapDelete("/api/roles/{id:int}", async (int id, IRoleService roleService) =>
{
    var ok = await roleService.DeleteRoleAsync(id);
    if (!ok) return Results.NotFound();
    return Results.NoContent();
}).RequireAuthorization("UserRoleConfiguration");

// UserRole API endpoints
app.MapGet("/api/userroles", async (IUserRoleService userRoleService) =>
{
    var userRoles = await userRoleService.GetAllUserRolesAsync();
    return Results.Ok(userRoles);
}).RequireAuthorization("UserRoleConfiguration");

app.MapPost("/api/userroles", async (TempleApi.Domain.Entities.UserRole userRole, IUserRoleService userRoleService) =>
{
    var createdUserRole = await userRoleService.CreateUserRoleAsync(userRole);
    return Results.Created($"/api/userroles/{createdUserRole.UserRoleId}", createdUserRole);
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

app.MapDelete("/api/userroles/{id:int}", async (int id, IUserRoleService userRoleService) =>
{
    var success = await userRoleService.DeleteUserRoleAsync(id);
    if (!success)
    {
        return Results.NotFound();
    }

    return Results.NoContent();
}).RequireAuthorization("UserRoleConfiguration");

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

// Area endpoints
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

// (Removed duplicate category toggle-status and delete routes to avoid conflicts)

// Event Type endpoints
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

// Legacy Expense-items endpoints removed; use /api/expense-items instead

// Expense Item alias endpoints
app.MapGet("/api/expense-items", async (IEventExpenseService svc) =>
{
    var items = await svc.GetAllEventExpensesAsync();
    return Results.Ok(items);
});

app.MapPost("/api/expense-items", async (CreateEventExpenseDto dto, IEventExpenseService svc) =>
{
    var item = await svc.CreateEventExpenseAsync(dto);
    return Results.Created($"/api/expense-items/{item.Id}", item);
});

app.MapPut("/api/expense-items/{id}", async (int id, UpdateEventExpenseDto dto, IEventExpenseService svc) =>
{
    var item = await svc.UpdateEventExpenseAsync(id, dto);
    return Results.Ok(item);
});

app.MapDelete("/api/expense-items/{id}", async (int id, IEventExpenseService svc) =>
{
    var ok = await svc.DeleteEventExpenseAsync(id);
    return ok ? Results.NoContent() : Results.NotFound();
});

// Expense Services endpoints
app.MapGet("/api/expense-services", async (IExpenseServiceService svc) =>
{
    var list = await svc.GetAllExpenseServicesAsync();
    return Results.Ok(list);
});

app.MapPost("/api/expense-services", async (CreateExpenseServiceDto dto, IExpenseServiceService svc) =>
{
    var created = await svc.CreateExpenseServiceAsync(dto);
    return Results.Created($"/api/expense-services/{created.Id}", created);
});

app.MapPut("/api/expense-services/{id}", async (int id, UpdateExpenseServiceDto dto, IExpenseServiceService svc) =>
{
    var updated = await svc.UpdateExpenseServiceAsync(id, dto);
    return Results.Ok(updated);
});

app.MapDelete("/api/expense-services/{id}", async (int id, IExpenseServiceService svc) =>
{
    var ok = await svc.DeleteExpenseServiceAsync(id);
    return ok ? Results.NoContent() : Results.NotFound();
});

// Event Expense endpoints (renamed from Expenses)
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

app.MapPost("/api/event-expenses", async (CreateExpenseDto dto, IExpenseService svc) =>
{
    var created = await svc.CreateExpenseAsync(dto);
    return Results.Created($"/api/event-expenses/{created.Id}", created);
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

app.MapDelete("/api/event-expenses/{id}", async (int id, IExpenseService svc) =>
{
    var ok = await svc.DeleteExpenseAsync(id);
    return ok ? Results.NoContent() : Results.NotFound();
});

// Alias for vouchers by event expense
app.MapGet("/api/event-expenses/{eventExpenseId}/vouchers", async (int eventExpenseId, IVoucherService svc) =>
{
    var vouchers = await svc.GetVouchersByExpenseAsync(eventExpenseId);
    return Results.Ok(vouchers);
});

// Event Expense alias endpoints removed; use /api/event-expenses

// Voucher endpoints
app.MapGet("/api/vouchers", async (IVoucherService svc) =>
{
    var vouchers = await svc.GetAllVouchersAsync();
    return Results.Ok(vouchers);
});

app.MapGet("/api/events/{eventId}/vouchers", async (int eventId, IVoucherService svc) =>
{
    var vouchers = await svc.GetVouchersByEventAsync(eventId);
    return Results.Ok(vouchers);
});


// Voucher approval is handled through expense approval endpoints
// Use PUT /api/event-expenses/{id}/approve instead

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
// NOTE: GET /api/roles is handled by RoleController. Avoid duplicating here to prevent ambiguous route.

app.MapGet("/api/permissions", async (TempleDbContext db) =>
{
    var pagePermissions = await db.PagePermissions
        .Select(p => new { p.PagePermissionId, p.PageName, p.PageUrl, p.PermissionId })
        .ToListAsync();
    return Results.Ok(pagePermissions);
}).RequireAuthorization("AdminOnly");

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
}).RequireAuthorization("UserRoleConfiguration");

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

// Minimal endpoint: get roles and permissions for a userId
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

// Debug endpoint to verify seeding
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

// Debug endpoint: inspect admin user record (development use only)
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

// Role permissions endpoint for UI permission checking
app.MapGet("/api/admin/role-permissions", async (TempleDbContext context, IAuthService authService, HttpContext httpContext) =>
{
    try 
    {
        // Get current user from JWT token
        var userId = authService.GetUserIdFromToken(httpContext);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        // Get user's roles and permissions
        var (userRoles, userPermissions) = await authService.GetUserRolesAndPermissionsAsync(userId.Value);
        
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

// App pages config endpoint
app.MapGet("/api/config/pages", (IConfiguration config) =>
{
    var pages = config.GetSection("App:Pages").GetChildren()
        .Select(s => new { key = s["key"], name = s["name"] })
        .Where(p => !string.IsNullOrWhiteSpace(p.key) && !string.IsNullOrWhiteSpace(p.name))
        .ToList();
    return Results.Ok(pages);
}).RequireAuthorization("UserRoleConfiguration");

app.MapGet("/api/users/me", async (HttpContext httpContext, IAuthService authService) =>
{
    var userId = authService.GetUserIdFromToken(httpContext);
    if (userId == null)
    {
        // Try Basic auth user from /api/auth/me behavior
        var resp = await authService.LoginAsync(new LoginRequest { Username = httpContext.User.Identity?.Name ?? string.Empty, Password = string.Empty });
        if (resp.User == null) return Results.Unauthorized();
        return Results.Ok(resp.User);
    }

    var userDto = await authService.GetUserByIdAsync(userId.Value);
    return userDto != null ? Results.Ok(userDto) : Results.NotFound();
}).RequireAuthorization();

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

// TEMPORARY: Send email for testing SMTP configuration
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

app.Run();

public partial class Program { }

public record EmailSendRequest(string To, string Subject, string Body);
