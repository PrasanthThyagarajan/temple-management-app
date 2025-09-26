using System.ComponentModel.DataAnnotations;

namespace TempleApi.Domain.Entities
{
    public class Voucher : BaseEntity
    {
        [Key]
        public int VoucherId { get; set; }

        [Required]
        [MaxLength(50)]
        public string VoucherNumber { get; set; } = string.Empty;

        [Required]
        public DateTime VoucherDate { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public int? ExpenseId { get; set; }
        public virtual Expense? Expense { get; set; }
    }
}
