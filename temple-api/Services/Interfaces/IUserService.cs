using TempleApi.Models.DTOs;

namespace TempleApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string roleName);
        Task<UserDto> UpdateUserAsync(int id, CreateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> ValidateUserCredentialsAsync(string email, string password);
        Task<string> ResetPasswordAsync(int userId);
    }
}
