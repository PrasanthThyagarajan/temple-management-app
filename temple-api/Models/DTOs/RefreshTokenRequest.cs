using System.ComponentModel.DataAnnotations;

namespace TempleApi.Models.DTOs
{
    public class RefreshTokenRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;
    }
}
