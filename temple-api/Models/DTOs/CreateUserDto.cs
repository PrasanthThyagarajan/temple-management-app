namespace TempleApi.Models.DTOs
{
    public class CreateUserDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string Password { get; set; } = string.Empty;
        public string? Nakshatra { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
