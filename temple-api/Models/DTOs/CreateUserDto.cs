using TempleApi.Enums;

namespace TempleApi.Models.DTOs
{
    public class CreateUserDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}
