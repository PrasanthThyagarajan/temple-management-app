using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using TempleApi.Security;
using Serilog;
using TempleApi.Data;
using TempleApi.Repositories.Interfaces;
using TempleApi.Repositories;
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
using TempleApi.Endpoints;
using TempleApi.Middleware;

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

// Configure Authorization settings
builder.Services.Configure<AuthorizationSettings>(builder.Configuration.GetSection(AuthorizationSettings.SectionName));

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
builder.Services.AddScoped<IPermissionService, PermissionService>();

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
builder.Services.AddScoped<IContributionSettingRepository, ContributionSettingRepository>();
builder.Services.AddScoped<IContributionSettingService, ContributionSettingService>();
builder.Services.AddScoped<IContributionRepository, ContributionRepository>();
builder.Services.AddScoped<IContributionService, ContributionService>();

// Register Inventory services
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IRepository<Inventory>, TempleApi.Repositories.Repository<Inventory>>();

// Jyotisham API service
builder.Services.AddHttpClient<IJyotishamApiService, JyotishamApiService>();

// Data seeding service
builder.Services.AddScoped<DataSeedingService>();

// Database migration service
builder.Services.AddScoped<IDatabaseMigrationService, DatabaseMigrationService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "http://localhost:5173", "http://localhost:5051", 
                          "http://127.0.0.1:5173", "http://127.0.0.1:5000", "http://127.0.0.1:5051")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Replace JWT with Basic Authentication
builder.Services.AddAuthentication("BasicAuthentication")
	.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

// Permission authorization is now handled by middleware

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

// Add permission-based authorization middleware
app.UsePermissionAuthorization();

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
        
        // Ensure Contribution tables exist
        var migrationService = scope.ServiceProvider.GetRequiredService<IDatabaseMigrationService>();
        await migrationService.EnsureContributionTablesAsync();
        logger.LogInformation("Contribution tables ensured");
        
        // Ensure Inventory tables exist
        await migrationService.EnsureInventoryTablesAsync();
        logger.LogInformation("Inventory tables ensured");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database");
        throw;
    }
}

// Map controllers
app.MapControllers();

// Map API endpoints using extension methods
app.MapReadEndpoints();
app.MapCreateEndpoints();
app.MapEditEndpoints();
app.MapDeleteEndpoints();

// Permission-based authorization is now handled by middleware using configuration

app.Run();

public partial class Program { }