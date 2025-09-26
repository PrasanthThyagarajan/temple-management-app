using System.ComponentModel.DataAnnotations;
using TempleApi.Enums;

namespace TempleApi.Domain.Entities
{
    public class PagePermission : BaseEntity
    {
        [Key]
        public int PagePermissionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PageName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string PageUrl { get; set; } = string.Empty;

        public int PermissionId { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public TempleApi.Enums.Permission Permission
        {
            get => (TempleApi.Enums.Permission)PermissionId;
            set => PermissionId = (int)value;
        }
        
        // Navigation properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}