using System.ComponentModel.DataAnnotations;

namespace TempleApi.Models.DTOs
{
    public class CreateTempleDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? City { get; set; }
        
        [StringLength(100)]
        public string? State { get; set; }
        
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        [StringLength(200)]
        public string? Email { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(100)]
        public string? Deity { get; set; }
        
        public DateTime? EstablishedDate { get; set; }
    }
}
