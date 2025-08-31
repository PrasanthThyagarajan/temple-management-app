using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TempleApi.Models
{
    public class Service
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
        [StringLength(100)]
        public string ServiceType { get; set; } = string.Empty; // Puja, Abhishek, Archana, etc.
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [StringLength(100)]
        public string? Duration { get; set; }
        
        [StringLength(500)]
        public string? Requirements { get; set; }
        
        [StringLength(500)]
        public string? Benefits { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation property
        [ForeignKey("TempleId")]
        public virtual Temple Temple { get; set; } = null!;
    }
}
