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
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [Required]
        [StringLength(100)]
        public string DonationType { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Purpose { get; set; }
        
        [StringLength(50)]
        public string? Status { get; set; } = "Pending";
        
        public DateTime? DonationDate { get; set; }
        
        [StringLength(200)]
        public string? ReceiptNumber { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
