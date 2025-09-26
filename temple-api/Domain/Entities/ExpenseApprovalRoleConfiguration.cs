using System.ComponentModel.DataAnnotations;

namespace TempleApi.Domain.Entities
{
    public class ExpenseApprovalRoleConfiguration : BaseEntity
    {
        public int UserRoleId { get; set; }
        public int EventExpenseId { get; set; }

        // Navigation properties
        public virtual UserRole UserRole { get; set; } = null!;
        public virtual EventExpense EventExpense { get; set; } = null!;
    }
}
