using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TempleApi.Domain.Entities
{
    public class Temple : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string State { get; set; } = string.Empty;
        
        [MaxLength(10)]
        public string PostalCode { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Deity { get; set; } = string.Empty;
        
        public DateTime EstablishedDate { get; set; }
        
        // Navigation properties
        public virtual ICollection<Devotee> Devotees { get; set; } = new List<Devotee>();
        public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public virtual ICollection<Event> Events { get; set; } = new List<Event>();
        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
        [JsonIgnore]
        public virtual ICollection<Area> Areas { get; set; } = new List<Area>();
    }
}
