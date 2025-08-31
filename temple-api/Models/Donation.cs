using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TempleApi.Models
{
    public class Donation
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int TempleId { get; set; }
        
        public int? DevoteeId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string DonorName { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? DonorEmail { get; set; }
        
        [StringLength(20)]
        public string? DonorPhone { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        [StringLength(100)]
        public string DonationType { get; set; } = string.Empty; // Cash, Online, Check, etc.
        
        [StringLength(200)]
        public string? Purpose { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed
        
        [StringLength(50)]
        public string? PaymentMethod { get; set; }
        
        public DateTime DonationDate { get; set; } = DateTime.UtcNow;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("TempleId")]
        public virtual Temple Temple { get; set; } = null!;
        
        [ForeignKey("DevoteeId")]
        public virtual Devotee? Devotee { get; set; }
    }
}
