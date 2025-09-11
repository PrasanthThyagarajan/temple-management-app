using System.ComponentModel.DataAnnotations;

namespace TempleApi.Models.DTOs
{
    public class UpdateRolePermissionsDto
    {
        [Required]
        public int RoleId { get; set; }

        [Required]
        public List<int> PermissionIds { get; set; } = new();
    }
}


