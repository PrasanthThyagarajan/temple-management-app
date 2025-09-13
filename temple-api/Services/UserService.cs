using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace TempleApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            // Check if user with email already exists
            var existingUser = await _userRepository.GetByEmailAsync(createUserDto.Email);
            
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            var user = new User
            {
                Username = createUserDto.Email.Split('@')[0], // Use email prefix as username
                Email = createUserDto.Email,
                FullName = createUserDto.Name,
                PasswordHash = HashPassword(createUserDto.Password),
                IsActive = true
            };

            await _userRepository.AddAsync(user);

            return MapToDto(user);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            
            return user != null ? MapToDto(user) : null;
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            
            return user != null ? MapToDto(user) : null;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            // Materialize to list to avoid deferred execution logging like SelectListIterator
            var userList = users.ToList();
            return userList.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string roleName)
        {
            var users = await _userRepository.GetUsersByRoleAsync(roleName);
            var userList = users.ToList();
            return userList.Select(MapToDto).ToList();
        }

        public async Task<UserDto> UpdateUserAsync(int id, CreateUserDto updateUserDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }

            // Check if email is being changed and if new email already exists
            if (user.Email != updateUserDto.Email)
            {
                var existingUser = await _userRepository.GetByEmailAsync(updateUserDto.Email);
                if (existingUser != null)
                {
                    throw new InvalidOperationException("User with this email already exists.");
                }
            }

            user.Email = updateUserDto.Email;
            user.FullName = updateUserDto.Name;
            
            // Only update password if provided
            if (!string.IsNullOrEmpty(updateUserDto.Password))
            {
                user.PasswordHash = HashPassword(updateUserDto.Password);
            }

            await _userRepository.UpdateAsync(user);

            return MapToDto(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            
            if (user == null)
            {
                return false;
            }

            // Soft delete by setting IsActive to false
            user.IsActive = false;
            await _userRepository.UpdateAsync(user);

            return true;
        }

        public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            
            if (user == null)
            {
                return false;
            }

            return VerifyPassword(password, user.PasswordHash);
        }

        public async Task<UserDto> RegisterUserAsync(RegisterUserDto registerUserDto)
        {
            // Validate email format and length
            if (!IsValidEmail(registerUserDto.Email) || registerUserDto.Email.Length > 30)
            {
                throw new ArgumentException("Invalid email format or length.");
            }

            // Check if user with email already exists
            var existingUser = await _userRepository.GetByEmailAsync(registerUserDto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            // Create user entity
            var user = new User
            {
                Username = registerUserDto.Email.Split('@')[0],
                Email = registerUserDto.Email,
                FullName = registerUserDto.Name,
                PasswordHash = HashPassword(registerUserDto.Password),
                IsActive = false, // Inactive until email verification
                IsVerified = false, // New field for email verification
                VerificationCode = GenerateVerificationCode(),
                CreatedAt = DateTime.UtcNow
            };

            // Save user to database
            await _userRepository.AddAsync(user);

            // Send verification email
            SendVerificationEmail(user.Email, user.VerificationCode);

            return MapToDto(user);
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

        private bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }

        private string GenerateVerificationCode()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[16];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        private void SendVerificationEmail(string email, string verificationCode)
        {
            // Logic to send email using configured SMTP settings
            // Use email: pallipurammutharamman@gmail.com, password: AmmanShakhi@2025
        }

        private UserDto MapToDto(User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Roles = new List<string>() // Will be populated by AuthService
            };
        }
    }
}