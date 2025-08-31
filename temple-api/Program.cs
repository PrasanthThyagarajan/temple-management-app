using Microsoft.EntityFrameworkCore;
using Serilog;
using TempleApi.Data;
using TempleApi.Services;
using TempleApi.Services.Interfaces;
using TempleApi.Models.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/temple-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddDbContext<TempleDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<ITempleService, TempleService>();
builder.Services.AddScoped<IDevoteeService, DevoteeService>();
builder.Services.AddScoped<IDonationService, DonationService>();
builder.Services.AddScoped<IEventService, EventService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
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
}

app.UseHttpsRedirection();
app.UseCors("AllowVueApp");

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

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

app.Run();
