using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TempleApi.Models
{
    public class Devotee
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [StringLength(100)]
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
        
        public int TempleId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation property
        [ForeignKey("TempleId")]
        public virtual Temple Temple { get; set; } = null!;
        
        // Navigation properties
        public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public virtual ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();
    }
}
