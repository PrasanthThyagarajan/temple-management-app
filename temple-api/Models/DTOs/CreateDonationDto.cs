using System.ComponentModel.DataAnnotations;

namespace TempleApi.Models.DTOs
{
    public class CreateDonationDto
    {
        [Required]
        public int TempleId { get; set; }
        
        public int? DevoteeId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string DonorName { get; set; } = string.Empty;
        
        [StringLength(100)]
        [EmailAddress]
        public string? DonorEmail { get; set; }
        
        [StringLength(20)]
        public string? DonorPhone { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        
        [Required]
        [StringLength(100)]
        public string DonationType { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Purpose { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        [StringLength(50)]
        public string? PaymentMethod { get; set; }
        
        public DateTime? DonationDate { get; set; }
    }
}
