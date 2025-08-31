using System.ComponentModel.DataAnnotations;

namespace TempleApi.Models.DTOs
{
    public class CreateDevoteeDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }
        
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        [StringLength(200)]
        public string? Address { get; set; }
        
        [StringLength(100)]
        public string? City { get; set; }
        
        [StringLength(100)]
        public string? State { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        
        [StringLength(50)]
        public string? Gender { get; set; }
        
        [Required]
        public int TempleId { get; set; }
    }
}
