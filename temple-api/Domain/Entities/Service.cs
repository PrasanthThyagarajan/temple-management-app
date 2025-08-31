using System.ComponentModel.DataAnnotations;

namespace TempleApi.Domain.Entities
{
    public class Service : BaseEntity
    {
        public int TempleId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string ServiceType { get; set; } = string.Empty;
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public bool IsAvailable { get; set; } = true;
        
        [MaxLength(100)]
        public string Duration { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string Requirements { get; set; } = string.Empty;
        
        public int? MaxBookingsPerDay { get; set; }
        
        public TimeSpan? StartTime { get; set; }
        
        public TimeSpan? EndTime { get; set; }
        
        // Navigation properties
        public virtual Temple Temple { get; set; } = null!;
    }
}
