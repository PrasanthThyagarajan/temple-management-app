using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TempleApi.Data;
using TempleApi.Models.DTOs;
using TempleApi.Services.Interfaces;
using System.Linq;

namespace TempleApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly TempleDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(TempleDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var input = (request.Username ?? string.Empty).Trim();

                // EF Core InMemory provider can behave differently for case-sensitive queries.
                // Fetch users and compare in-memory using case-insensitive equality to be robust across providers.
                var users = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .ToListAsync();

                var user = users.FirstOrDefault(u =>
                    string.Equals(u.Username, input, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(u.Email, input, StringComparison.OrdinalIgnoreCase));

                if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    };
                }

                // Check if the user's email is verified
                if (!user.IsVerified)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Please verify your email before logging in. Check your email for the verification link."
                    };
                }

                // Check if the user is active
                if (!user.IsActive)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Your account has been deactivated. Please contact support."
                    };
                }

                var roles = await GetUserRolesAsync(user.UserId);
                var permissions = await GetUserPermissionsAsync(user.UserId);

                // For BasicAuth flow, no JWT token is returned
                return new AuthResponse
                {
                    Success = true,
                    Message = "Login successful",
                    Token = string.Empty,
                    User = new UserDto
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        Email = user.Email,
                        FullName = user.FullName,
                        Gender = user.Gender,
                        PhoneNumber = user.PhoneNumber,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
                        Nakshatra = user.Nakshatra,
                        DateOfBirth = user.DateOfBirth,
                        Roles = roles
                    },
                    Roles = roles,
                    Permissions = permissions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", request.Username);
                return new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during login"
                };
            }
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // Enforce: username must be email and <= 30 chars
                if (string.IsNullOrWhiteSpace(request.Email) || request.Email.Length > 30)
                {
                    return new AuthResponse { Success = false, Message = "Email must be <= 30 characters" };
                }

                // Normalize username as email
                var username = request.Email.Trim();

                // Optional phone number validation
                var phone = (request.PhoneNumber ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    if (phone.Length > 20)
                    {
                        return new AuthResponse { Success = false, Message = "Phone number must be <= 20 characters" };
                    }
                    if (!IsValidPhoneNumber(phone))
                    {
                        return new AuthResponse { Success = false, Message = "Invalid phone number format" };
                    }
                }

                // Check duplicates (handle case-insensitive for In-Memory database in tests)
                var existingUsers = await _context.Users.ToListAsync();
                if (existingUsers.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)))
                {
                    return new AuthResponse { Success = false, Message = "Username already exists" };
                }
                if (existingUsers.Any(u => string.Equals(u.Email, request.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    return new AuthResponse { Success = false, Message = "Email already exists" };
                }

                // Create new user (inactive until email verification)
                var verificationCode = Guid.NewGuid().ToString("N");
                var user = new Domain.Entities.User
                {
                    Username = username,
                    Email = request.Email,
                    FullName = request.FullName,
                    Gender = request.Gender,
                    PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone,
                    Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address,
                    // Store password as Base64 encoded string per requirement
                    PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(request.Password)),
                    IsActive = false,
                    IsVerified = false,
                    VerificationCode = verificationCode,
                    Nakshatra = request.Nakshatra,
                    DateOfBirth = request.DateOfBirth
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Assign default role (General)
                var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "General");
                if (defaultRole != null)
                {
                    var userRole = new Domain.Entities.UserRole
                    {
                        UserId = user.UserId,
                        RoleId = defaultRole.RoleId,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync();
                }

                // Send verification email (common verify URL convention)
                try
                {
                    var baseUrl = _configuration["App:BaseUrl"] ?? string.Empty;
                    var verifyUrl = string.IsNullOrWhiteSpace(baseUrl)
                        ? $"/api/auth/verify?code={verificationCode}"
                        : $"{baseUrl.TrimEnd('/')}/verify?code={verificationCode}";
                    var emailSvc = new EmailService(_configuration, _logger);
                    await emailSvc.SendAsync(user.Email, "Verify your account", $"Please verify your account by clicking: {verifyUrl}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);
                }

                return new AuthResponse
                {
                    Success = true,
                    Message = "Registration successful. Please check your email to verify your account.",
                    Token = string.Empty,
                    User = new UserDto
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
                        Roles = new List<string> { "General" }
                    },
                    Roles = new List<string> { "General" },
                    Permissions = new List<string>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Username}", request.Username);
                return new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during registration"
                };
            }
        }

        private bool IsValidPhoneNumber(string phone)
        {
            // Allow digits, spaces, +, -, (, ); 7-20 digits overall
            var allowed = System.Text.RegularExpressions.Regex.IsMatch(phone, @"^[0-9+\-()\s]+$");
            if (!allowed) return false;
            var digitCount = phone.Count(char.IsDigit);
            return digitCount >= 7 && digitCount <= 20;
        }

        public async Task<bool> VerifyAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationCode == code && !u.IsVerified);
            if (user == null) return false;
            user.IsVerified = true;
            user.IsActive = true;
            user.VerificationCode = string.Empty;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return null;

            var roles = await GetUserRolesAsync(userId);

            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Nakshatra = user.Nakshatra,
                DateOfBirth = user.DateOfBirth,
                Roles = roles
            };
        }

        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync();
        }

        public async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.PagePermission)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.PagePermission.PageName)
                .Distinct()
                .ToListAsync();
        }

        public async Task<(List<string> Roles, List<string> Permissions)> GetUserRolesAndPermissionsAsync(int userId)
        {
            var rolesTask = GetUserRolesAsync(userId);
            var permissionsTask = GetUserPermissionsAsync(userId);
            await Task.WhenAll(rolesTask, permissionsTask);
            return (rolesTask.Result, permissionsTask.Result);
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public async Task<UserDto?> GetUserFromTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "userid");

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return null;

                return await GetUserByIdAsync(userId);
            }
            catch
            {
                return null;
            }
        }

        public async Task<AuthResponse> RefreshTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));

                // First try to read the token even if it's expired (ignore lifetime validation for refresh)
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "userid");

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid token"
                    };
                }

                // Check if token is not too old (allow refresh up to 7 days after expiration)
                var expClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "exp");
                if (expClaim != null && long.TryParse(expClaim.Value, out long exp))
                {
                    var expiry = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                    var maxRefreshTime = expiry.AddDays(7); // Allow refresh up to 7 days after expiration
                    
                    if (DateTime.UtcNow > maxRefreshTime)
                    {
                        return new AuthResponse
                        {
                            Success = false,
                            Message = "Token is too old for refresh. Please login again."
                        };
                    }
                }

                // Get the current user to ensure they still exist and are active
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);

                if (user == null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "User not found or inactive"
                    };
                }

                // Generate new token with current user data
                var roles = await GetUserRolesAsync(user.UserId);
                var permissions = await GetUserPermissionsAsync(user.UserId);
                var newToken = GenerateJwtToken(user, roles, permissions);

                return new AuthResponse
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    Token = newToken,
                    User = new UserDto
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
                    },
                    Roles = roles,
                    Permissions = permissions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return new AuthResponse
                {
                    Success = false,
                    Message = "Failed to refresh token"
                };
            }
        }

        // Replace hashed password with simple Base64 storage per requirement
        private string HashPassword(string password)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(storedHash));
                return string.Equals(password, decoded);
            }
            catch
            {
                return false;
            }
        }

        // JWT generation is no longer used in BasicAuth mode; method kept for compatibility
        private string GenerateJwtToken(Domain.Entities.User user, List<string> roles, List<string> permissions) => string.Empty;

        public int? GetUserIdFromToken(HttpContext httpContext)
        {
            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting user ID from token");
                return null;
            }
        }
    }
}
