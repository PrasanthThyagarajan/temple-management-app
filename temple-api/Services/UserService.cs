using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using TempleApi.Models.DTOs;
using TempleApi.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;

namespace TempleApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            // Fallbacks for tests or manual construction
            _configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
            _logger = NullLogger<UserService>.Instance;
        }

        public UserService(IUserRepository userRepository, IConfiguration configuration, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
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
                Gender = createUserDto.Gender,
                PhoneNumber = createUserDto.PhoneNumber,
                Address = createUserDto.Address,
                PasswordHash = HashPassword(createUserDto.Password),
                IsActive = true,
                IsVerified = true, // Admin-created users are pre-verified
                Nakshatra = createUserDto.Nakshatra,
                DateOfBirth = createUserDto.DateOfBirth
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
            user.Gender = updateUserDto.Gender;
            user.PhoneNumber = updateUserDto.PhoneNumber;
            user.Address = updateUserDto.Address;
            user.Nakshatra = updateUserDto.Nakshatra;
            user.DateOfBirth = updateUserDto.DateOfBirth;
            
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

        public async Task<string> ResetPasswordAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException($"User with ID {userId} not found.");

            var newPassword = GenerateRandomPassword(8);
            user.PasswordHash = HashPassword(newPassword);
            await _userRepository.UpdateAsync(user);

            try
            {
                var emailSvc = new EmailService(_configuration, _logger);
                var body = $"Dear {user.FullName},\n\nYour password has been reset.\n\nUsername: {user.Email}\nNew Password: {newPassword}\n\nPlease login and change your password.";
                await emailSvc.SendAsync(user.Email, "Your password has been reset", body);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to send reset password email to {Email}", user.Email);
            }

            return newPassword;
        }

        private string HashPassword(string password)
        {
            // Store as Base64 of the plain password per requirement
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

        private static string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789@#$";
            var data = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(data);
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[data[i] % chars.Length];
            }
            return new string(result);
        }

        private UserDto MapToDto(User user)
        {
            return new UserDto
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
                Roles = new List<string>() // Will be populated by AuthService
            };
        }
    }
}