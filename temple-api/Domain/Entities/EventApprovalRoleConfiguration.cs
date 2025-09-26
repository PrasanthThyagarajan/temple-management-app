using System.ComponentModel.DataAnnotations;

namespace TempleApi.Domain.Entities
{
    public class EventApprovalRoleConfiguration : BaseEntity
    {
        public int EventId { get; set; }
        public int UserRoleId { get; set; }

        public virtual Event Event { get; set; } = null!;
        public virtual UserRole UserRole { get; set; } = null!;
    }
}
