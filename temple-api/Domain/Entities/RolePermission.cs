using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TempleApi.Domain.Entities
{
    public class RolePermission : BaseEntity
    {
        [Key]
        public int RolePermissionId { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        public int PagePermissionId { get; set; }

        // Navigation properties
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;

        [ForeignKey("PagePermissionId")]
        public virtual PagePermission PagePermission { get; set; } = null!;
    }
}
