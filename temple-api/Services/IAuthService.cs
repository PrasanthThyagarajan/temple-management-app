using TempleApi.Models.DTOs;

namespace TempleApi.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<List<string>> GetUserRolesAsync(int userId);
        Task<List<string>> GetUserPermissionsAsync(int userId);
        Task<(List<string> Roles, List<string> Permissions)> GetUserRolesAndPermissionsAsync(int userId);
        Task<bool> ValidateTokenAsync(string token);
        Task<UserDto?> GetUserFromTokenAsync(string token);
        Task<bool> VerifyAsync(string code);
        Task<AuthResponse> RefreshTokenAsync(string token);
        int? GetUserIdFromToken(HttpContext httpContext);
    }
}
