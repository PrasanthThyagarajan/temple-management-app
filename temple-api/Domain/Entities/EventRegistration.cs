using System.ComponentModel.DataAnnotations;

namespace TempleApi.Domain.Entities
{
    public class EventRegistration : BaseEntity
    {
        public int EventId { get; set; }
        
        public int DevoteeId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string AttendeeName { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string Status { get; set; } = "Registered";
        
        public DateTime RegistrationDate { get; set; }
        
        [MaxLength(500)]
        public string SpecialRequirements { get; set; } = string.Empty;
        
        public decimal? AmountPaid { get; set; }
        
        [MaxLength(200)]
        public string PaymentMethod { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual Event Event { get; set; } = null!;
        public virtual Devotee Devotee { get; set; } = null!;
    }
}
