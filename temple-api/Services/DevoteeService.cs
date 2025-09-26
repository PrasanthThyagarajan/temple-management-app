using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TempleApi.Data;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services.Interfaces;

namespace TempleApi.Services
{
    public class DevoteeService : IDevoteeService
    {
        private readonly TempleDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DevoteeService> _logger;

        public DevoteeService(TempleDbContext context, IConfiguration configuration, ILogger<DevoteeService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IEnumerable<Devotee>> GetAllDevoteesAsync()
        {
            return await _context.Devotees
                .Include(d => d.Temple)
                .Where(d => d.IsActive)
                .OrderByDescending(d => d.Id)
                .ToListAsync();
        }

        public async Task<Devotee?> GetDevoteeByIdAsync(int id)
        {
            return await _context.Devotees
                .Include(d => d.Temple)
                .Include(d => d.Donations.Where(don => don.IsActive))
                .Include(d => d.EventRegistrations.Where(er => er.IsActive))
                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);
        }

        public async Task<IEnumerable<Devotee>> GetDevoteesByTempleAsync(int templeId)
        {
            return await _context.Devotees
                .Where(d => d.TempleId == templeId && d.IsActive)
                .OrderByDescending(d => d.Id)
                .ToListAsync();
        }

        public async Task<Devotee> CreateDevoteeAsync(CreateDevoteeDto createDto)
        {

            var devotee = new Devotee
            {
                FullName = createDto.Name,
                Email = createDto.Email ?? string.Empty,
                Phone = createDto.Phone ?? string.Empty,
                Address = createDto.Address ?? string.Empty,
                City = createDto.City ?? string.Empty,
                State = createDto.State ?? string.Empty,
                PostalCode = createDto.PostalCode ?? string.Empty,
                DateOfBirth = createDto.DateOfBirth,
                Gender = createDto.Gender ?? string.Empty,
                TempleId = createDto.TempleId,
                UserId = createDto.UserId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Devotees.Add(devotee);
            await _context.SaveChangesAsync();

            return devotee;
        }

        public async Task<(Devotee Devotee, string? GeneratedPassword)> CreateDevoteeWithUserAsync(CreateDevoteeDto createDto)
        {
            Devotee devotee;
            var password = null as string;

            // Only proceed to create a user if email is present
            if (!string.IsNullOrWhiteSpace(createDto.Email))
            {
                // If a user with this email already exists, skip user creation
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == createDto.Email);
                if (existingUser == null)
                {
                    password = GenerateRandomPassword(8);
                    var user = new User
                    {
                        Username = createDto.Email,
                        Email = createDto.Email,
                        FullName = createDto.Name,
                        Gender = string.IsNullOrWhiteSpace(createDto.Gender) ? null : createDto.Gender,
                        PhoneNumber = string.IsNullOrWhiteSpace(createDto.Phone) ? null : createDto.Phone,
                        Address = string.IsNullOrWhiteSpace(createDto.Address) ? null : createDto.Address,
                        PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password)),
                        IsActive = true,
                        IsVerified = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                    createDto.UserId = user.UserId;

                    // Assign General role if exists
                    var generalRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "General");
                    if (generalRole != null)
                    {
                        _context.UserRoles.Add(new UserRole
                        {
                            UserId = user.UserId,
                            RoleId = generalRole.RoleId,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        });
                        await _context.SaveChangesAsync();
                    }
                    
                    // Send email with generated password
                    try
                    {
                        var emailSvc = new EmailService(_configuration, _logger);
                        var body = $"Dear {user.FullName},\n\nYour account has been created.\n\nUsername: {user.Email}\nPassword: {password}\n\nPlease login and change your password.";
                        await emailSvc.SendAsync(user.Email, "Your account details", body);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Failed to send generated password email to {Email}", createDto.Email);
                    }
                }
                else
                {
                    // User already exists, use their ID
                    createDto.UserId = existingUser.UserId;
                }
            }

            // Always create the devotee (with or without user account)
            devotee = await CreateDevoteeAsync(createDto);
            return (devotee, password);
        }

        private static string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789@#$";
            var data = new byte[length];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(data);
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[data[i] % chars.Length];
            }
            return new string(result);
        }

        public async Task<Devotee?> UpdateDevoteeAsync(int id, CreateDevoteeDto updateDto)
        {
            var devotee = await _context.Devotees.FindAsync(id);
            if (devotee == null || !devotee.IsActive)
                return null;

            devotee.FullName = updateDto.Name;
            devotee.Email = updateDto.Email ?? string.Empty;
            devotee.Phone = updateDto.Phone ?? string.Empty;
            devotee.Address = updateDto.Address ?? string.Empty;
            devotee.City = updateDto.City ?? string.Empty;
            devotee.State = updateDto.State ?? string.Empty;
            devotee.PostalCode = updateDto.PostalCode ?? string.Empty;
            devotee.DateOfBirth = updateDto.DateOfBirth;
            devotee.Gender = updateDto.Gender ?? string.Empty;
            devotee.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return devotee;
        }

        public async Task<bool> DeleteDevoteeAsync(int id)
        {
            var devotee = await _context.Devotees.FindAsync(id);
            if (devotee == null || !devotee.IsActive)
                return false;

            devotee.IsActive = false;
            devotee.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Devotee>> SearchDevoteesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllDevoteesAsync();

            var normalizedSearchTerm = searchTerm.ToLower();
            return await _context.Devotees
                .Include(d => d.Temple)
                .Where(d => d.IsActive && (
                    d.FullName.ToLower().Contains(normalizedSearchTerm) ||
                    d.Email.ToLower().Contains(normalizedSearchTerm) ||
                    d.Phone.ToLower().Contains(normalizedSearchTerm) ||
                    d.City.ToLower().Contains(normalizedSearchTerm) ||
                    d.State.ToLower().Contains(normalizedSearchTerm)
                ))
                .OrderByDescending(d => d.Id)
                .ToListAsync();
        }
    }
}
