using System.ComponentModel.DataAnnotations;

namespace TempleApi.Models.DTOs
{
    public class ExpenseDto
    {
        public int Id { get; set; }
        public int? EventExpenseId { get; set; }
        public int? ExpenseServiceId { get; set; }
        public int EventId { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsApprovalNeed { get; set; } = false;
        public bool IsApproved { get; set; } = false;
        public int? RequestedBy { get; set; }
        public string? RequestedByUserName { get; set; }
        public string? RequestedByUserRole { get; set; }
        public int? ApprovedBy { get; set; }
        public string? ApprovedByUserName { get; set; }
        public string? ApprovedByUserRole { get; set; }
        public DateTime? ApprovedOn { get; set; }
        
        // Navigation properties for approval info
        public EventExpenseDto? EventExpense { get; set; }
        public ExpenseServiceDto? ExpenseService { get; set; }
    }

    public class CreateExpenseDto
    {
        public int? EventExpenseId { get; set; }
        public int? ExpenseServiceId { get; set; }
        [Required]
        public int EventId { get; set; }
        [Required]
        public decimal Price { get; set; }
        public int? RequestedBy { get; set; }
    }

    public class UpdateExpenseDto
    {
        [Required]
        public decimal Price { get; set; }
    }
}
