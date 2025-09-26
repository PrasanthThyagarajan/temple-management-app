using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TempleApi.Domain.Entities
{
    public class Expense : BaseEntity
    {
        public int? EventExpenseId { get; set; }
        public int? ExpenseServiceId { get; set; }
        public int EventId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public bool IsApprovalNeed { get; set; } = false;
        public bool IsApproved { get; set; } = false;
        public int? RequestedBy { get; set; }
        public int? ApprovedBy { get; set; }
        
        [Column("ApprovedOn")]
        public DateTime? ApprovedOn { get; set; }

        // Navigation properties
        public virtual EventExpense? EventExpense { get; set; }
        public virtual ExpenseService? ExpenseService { get; set; }
        public virtual Event Event { get; set; } = null!;
        public virtual User? RequestedByUser { get; set; }
        public virtual User? ApprovedByUser { get; set; }
    }
}
