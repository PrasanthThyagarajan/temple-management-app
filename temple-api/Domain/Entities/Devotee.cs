using System.ComponentModel.DataAnnotations;

namespace TempleApi.Domain.Entities
{
    public class Devotee : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string State { get; set; } = string.Empty;
        
        [MaxLength(10)]
        public string PostalCode { get; set; } = string.Empty;
        
        public DateTime? DateOfBirth { get; set; }
        
        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;
        
        public int TempleId { get; set; }
        
        public int UserId { get; set; }
        
        // Navigation properties
        public virtual Temple Temple { get; set; } = null!;
        public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public virtual ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();
    }
}
