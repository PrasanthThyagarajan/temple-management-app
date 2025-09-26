using System.ComponentModel.DataAnnotations;

namespace TempleApi.Models.DTOs
{
    public class VoucherDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int ExpenseId { get; set; }
        public int ServiceId { get; set; }
        public int ApprovalRoleId { get; set; }
        public int? ApprovedUserId { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
        public string VoucherType { get; set; } = string.Empty;
    }

    public class CreateVoucherDto
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        public int ExpenseId { get; set; }

        [Required]
        public int ApprovalRoleId { get; set; }

        public int? ApprovedUserId { get; set; }
    }
}
