using System.ComponentModel.DataAnnotations;

namespace TempleApi.Models.DTOs
{
    public class CreateEventDto
    {
        [Required]
        public int TempleId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        [StringLength(100)]
        public string? Location { get; set; }
        
        [Required]
        [StringLength(100)]
        public string EventType { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? SpecialInstructions { get; set; }
        
        public int? MaxAttendees { get; set; }
        
        public decimal? RegistrationFee { get; set; }
    }
}
