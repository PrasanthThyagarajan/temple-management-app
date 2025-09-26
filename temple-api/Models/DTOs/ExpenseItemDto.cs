using System.ComponentModel.DataAnnotations;

namespace TempleApi.Models.DTOs
{
    public class EventExpenseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsApprovalNeeded { get; set; } = false;
        public int? ApprovalRoleId { get; set; }
        public string? ApprovalRoleName { get; set; }
    }

    public class CreateEventExpenseDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsApprovalNeeded { get; set; } = false;
        public int? ApprovalRoleId { get; set; }
    }

    public class UpdateEventExpenseDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsApprovalNeeded { get; set; } = false;
        public int? ApprovalRoleId { get; set; }
    }
}
