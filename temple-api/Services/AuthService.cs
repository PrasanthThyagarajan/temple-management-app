using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TempleApi.Data;
using TempleApi.Models.DTOs;
using TempleApi.Services.Interfaces;

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
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

                if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    };
                }

                var roles = await GetUserRolesAsync(user.UserId);
                var permissions = await GetUserPermissionsAsync(user.UserId);
                var token = GenerateJwtToken(user, roles, permissions);

                return new AuthResponse
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    User = new UserDto
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        Email = user.Email,
                        FullName = user.FullName,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
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

                // Check duplicates
                if (await _context.Users.AnyAsync(u => u.Username == username))
                {
                    return new AuthResponse { Success = false, Message = "Username already exists" };
                }
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
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
                    PasswordHash = HashPassword(request.Password),
                    IsActive = false,
                    IsVerified = false,
                    VerificationCode = verificationCode
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

                // Send verification email
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
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
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
                .ThenInclude(rp => rp.Permission)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.PermissionName)
                .Distinct()
                .ToListAsync();
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

        private string HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var saltBytes = new byte[32];
            rng.GetBytes(saltBytes);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
            var hashBytes = pbkdf2.GetBytes(32);

            var hashWithSalt = new byte[64];
            Array.Copy(saltBytes, 0, hashWithSalt, 0, 32);
            Array.Copy(hashBytes, 0, hashWithSalt, 32, 32);

            return Convert.ToBase64String(hashWithSalt);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                // Support BCrypt hashes (legacy) as well as PBKDF2 (current)
                if (!string.IsNullOrWhiteSpace(storedHash) && storedHash.StartsWith("$2"))
                {
                    return BCrypt.Net.BCrypt.Verify(password, storedHash);
                }

                var hashWithSalt = Convert.FromBase64String(storedHash);
                var saltBytes = new byte[32];
                var hashBytes = new byte[32];

                Array.Copy(hashWithSalt, 0, saltBytes, 0, 32);
                Array.Copy(hashWithSalt, 32, hashBytes, 0, 32);

                using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
                var computedHash = pbkdf2.GetBytes(32);

                return computedHash.SequenceEqual(hashBytes);
            }
            catch
            {
                return false;
            }
        }

        private string GenerateJwtToken(Domain.Entities.User user, List<string> roles, List<string> permissions)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new("userid", user.UserId.ToString()),
                new("fullname", user.FullName)
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Add permission claims
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
