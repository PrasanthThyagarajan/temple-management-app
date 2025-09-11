using System.ComponentModel.DataAnnotations;

namespace TempleApi.Domain.Entities
{
    public class Permission : BaseEntity
    {
        [Key]
        public int PermissionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PermissionName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        // Navigation properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
