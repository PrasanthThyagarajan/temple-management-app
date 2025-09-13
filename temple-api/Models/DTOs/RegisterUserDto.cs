using System.ComponentModel.DataAnnotations;

namespace TempleApi.Models.DTOs
{
    public class RegisterUserDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(30)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
