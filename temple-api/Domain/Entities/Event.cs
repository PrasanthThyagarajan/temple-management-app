using System.ComponentModel.DataAnnotations;

namespace TempleApi.Domain.Entities
{
    public class Event : BaseEntity
    {
        public int? AreaId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public int EventTypeId { get; set; }
        
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Organizer { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;
        
        public int? MaxAttendees { get; set; }
        
        public decimal? EntryFee { get; set; }
        
        public bool IsApprovalNeeded { get; set; } = false;
        
        // Navigation properties
        public virtual Area? Area { get; set; }
        public virtual EventType EventType { get; set; } = null!;
        public virtual ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
    }
}
