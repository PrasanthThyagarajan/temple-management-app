using System.ComponentModel.DataAnnotations;

namespace TempleApi.Models.DTOs
{
    public class CreateEventDto
    {
        public int? AreaId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        [StringLength(100)]
        public string? Location { get; set; }
        
        [Required]
        public int EventTypeId { get; set; }
        
        public int? MaxAttendees { get; set; }
        
        public decimal? RegistrationFee { get; set; }

        public bool IsApprovalNeeded { get; set; } = false;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;
    }
}
