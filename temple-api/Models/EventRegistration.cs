using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TempleApi.Models
{
    public class EventRegistration
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int EventId { get; set; }
        
        [Required]
        public int DevoteeId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string AttendeeName { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? AttendeeEmail { get; set; }
        
        [StringLength(20)]
        public string? AttendeePhone { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Registered"; // Registered, Confirmed, Cancelled, Attended
        
        [StringLength(500)]
        public string? SpecialRequirements { get; set; }
        
        public decimal? AmountPaid { get; set; }
        
        [StringLength(50)]
        public string? PaymentStatus { get; set; } // Pending, Paid, Refunded
        
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; } = null!;
        
        [ForeignKey("DevoteeId")]
        public virtual Devotee Devotee { get; set; } = null!;
    }
}
