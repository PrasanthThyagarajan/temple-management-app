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
            // Validate input
            if (string.IsNullOrWhiteSpace(createDto.Name))
            {
                throw new InvalidOperationException("Devotee name is required.");
            }

            if (createDto.TempleId <= 0)
            {
                throw new InvalidOperationException("Valid temple selection is required.");
            }

            // Check if temple exists
            var templeExists = await _context.Temples.AnyAsync(t => t.Id == createDto.TempleId && t.IsActive);
            if (!templeExists)
            {
                throw new InvalidOperationException("The specified temple does not exist or is inactive.");
            }

            Devotee devotee;
            var password = null as string;

            // If userId is provided, validate the user and email
            if (createDto.UserId > 0)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == createDto.UserId);
                if (existingUser == null)
                {
                    throw new InvalidOperationException("The specified user does not exist.");
                }

                // Check if user is active
                if (!existingUser.IsActive)
                {
                    throw new InvalidOperationException("The specified user is not active.");
                }

                // Check if user is an admin
                var isAdmin = await _context.UserRoles
                    .Include(ur => ur.Role)
                    .AnyAsync(ur => ur.UserId == createDto.UserId && 
                                   ur.Role.RoleName == "Admin" && 
                                   ur.IsActive);
                if (isAdmin)
                {
                    throw new InvalidOperationException("Admin users cannot be registered as devotees. Admins have full control without being devotees.");
                }

                // Check if user is already linked to another devotee
                var existingDevotee = await _context.Devotees.FirstOrDefaultAsync(d => d.UserId == createDto.UserId && d.IsActive);
                if (existingDevotee != null)
                {
                    throw new InvalidOperationException($"This user is already registered as a devotee with ID {existingDevotee.Id}.");
                }

                // Validate email matches if both are provided
                if (!string.IsNullOrWhiteSpace(createDto.Email) && 
                    !string.Equals(existingUser.Email, createDto.Email, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("The provided email does not match the selected user's email.");
                }

                // Use the user's email if not provided
                if (string.IsNullOrWhiteSpace(createDto.Email))
                {
                    createDto.Email = existingUser.Email;
                }
            }
            // Only proceed to create a user if email is present and no userId provided
            else if (!string.IsNullOrWhiteSpace(createDto.Email))
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
                    // User already exists, check if they are an admin
                    var isAdmin = await _context.UserRoles
                        .Include(ur => ur.Role)
                        .AnyAsync(ur => ur.UserId == existingUser.UserId && 
                                       ur.Role.RoleName == "Admin" && 
                                       ur.IsActive);
                    if (isAdmin)
                    {
                        throw new InvalidOperationException($"The user with email {createDto.Email} is an admin. Admin users cannot be registered as devotees.");
                    }

                    // Check if already linked to a devotee
                    var existingDevotee = await _context.Devotees.FirstOrDefaultAsync(d => d.UserId == existingUser.UserId && d.IsActive);
                    if (existingDevotee != null)
                    {
                        throw new InvalidOperationException($"A user with email {createDto.Email} is already registered as a devotee.");
                    }
                    
                    // Check if user is active
                    if (!existingUser.IsActive)
                    {
                        throw new InvalidOperationException("The user associated with this email is not active.");
                    }
                    
                    // User exists and is not linked to any devotee, use their ID
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
            // Validate input
            if (string.IsNullOrWhiteSpace(updateDto.Name))
            {
                throw new InvalidOperationException("Devotee name is required.");
            }

            if (updateDto.TempleId <= 0)
            {
                throw new InvalidOperationException("Valid temple selection is required.");
            }

            var devotee = await _context.Devotees.FindAsync(id);
            if (devotee == null || !devotee.IsActive)
                return null;

            // Check if temple exists
            var templeExists = await _context.Temples.AnyAsync(t => t.Id == updateDto.TempleId && t.IsActive);
            if (!templeExists)
            {
                throw new InvalidOperationException("The specified temple does not exist or is inactive.");
            }

            // If email is being changed, validate it
            if (!string.IsNullOrWhiteSpace(updateDto.Email) && 
                !string.Equals(devotee.Email, updateDto.Email, StringComparison.OrdinalIgnoreCase))
            {
                // Check if the new email belongs to an active user
                var userWithEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == updateDto.Email);
                if (userWithEmail != null)
                {
                    // Check if this user is already linked to another devotee
                    var otherDevotee = await _context.Devotees.FirstOrDefaultAsync(d => 
                        d.UserId == userWithEmail.UserId && 
                        d.IsActive && 
                        d.Id != id);
                    
                    if (otherDevotee != null)
                    {
                        throw new InvalidOperationException($"A user with email {updateDto.Email} is already registered as a devotee.");
                    }

                    // Check if user is active
                    if (!userWithEmail.IsActive)
                    {
                        throw new InvalidOperationException("The user associated with this email is not active.");
                    }

                    // Check if user is an admin
                    var isAdmin = await _context.UserRoles
                        .Include(ur => ur.Role)
                        .AnyAsync(ur => ur.UserId == userWithEmail.UserId && 
                                       ur.Role.RoleName == "Admin" && 
                                       ur.IsActive);
                    if (isAdmin)
                    {
                        throw new InvalidOperationException($"The user with email {updateDto.Email} is an admin. Admin users cannot be registered as devotees.");
                    }
                }
            }

            devotee.FullName = updateDto.Name;
            devotee.Email = updateDto.Email ?? string.Empty;
            devotee.Phone = updateDto.Phone ?? string.Empty;
            devotee.Address = updateDto.Address ?? string.Empty;
            devotee.City = updateDto.City ?? string.Empty;
            devotee.State = updateDto.State ?? string.Empty;
            devotee.PostalCode = updateDto.PostalCode ?? string.Empty;
            devotee.DateOfBirth = updateDto.DateOfBirth;
            devotee.Gender = updateDto.Gender ?? string.Empty;
            devotee.TempleId = updateDto.TempleId;
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
