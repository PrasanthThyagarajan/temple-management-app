using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TempleApi.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }
        
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
        
        [StringLength(100)]
        public string EventType { get; set; } = string.Empty; // Puja, Festival, Ceremony, etc.
        
        [StringLength(50)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Active, Completed, Cancelled
        
        [StringLength(500)]
        public string? SpecialInstructions { get; set; }
        
        public int? MaxAttendees { get; set; }
        
        public decimal? RegistrationFee { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        [ForeignKey("TempleId")]
        public virtual Temple Temple { get; set; } = null!;
        
        public virtual ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
    }
}
