using System.ComponentModel.DataAnnotations;

namespace TempleApi.Domain.Entities
{
    public class Donation : BaseEntity
    {
        public int TempleId { get; set; }
        
        public int? DevoteeId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string DonorName { get; set; } = string.Empty;
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string DonationType { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Purpose { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";
        
        public DateTime DonationDate { get; set; }
        
        [MaxLength(200)]
        public string ReceiptNumber { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual Temple Temple { get; set; } = null!;
        public virtual Devotee? Devotee { get; set; }
    }
}
