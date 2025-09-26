using System.ComponentModel.DataAnnotations;

namespace TempleApi.Domain.Entities
{
	public class ExpenseService : BaseEntity
	{
		[Required]
		[MaxLength(200)]
		public string Name { get; set; } = string.Empty;

		[MaxLength(500)]
		public string Description { get; set; } = string.Empty;

		[Required]
		public bool IsApprovalNeeded { get; set; } = false;

		public int? ApprovalRoleId { get; set; }

		// Navigation properties
		public virtual Role? ApprovalRole { get; set; }
	}
}


