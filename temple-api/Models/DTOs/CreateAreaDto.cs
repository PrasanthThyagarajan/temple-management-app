using System.ComponentModel.DataAnnotations;

namespace TempleApi.Models.DTOs
{
    public class CreateAreaDto
    {
        [Required]
        public int TempleId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}


