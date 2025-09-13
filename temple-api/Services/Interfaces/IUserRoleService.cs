using TempleApi.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TempleApi.Services.Interfaces
{
    public interface IUserRoleService
    {
        Task<IEnumerable<UserRole>> GetAllUserRolesAsync();
        Task<UserRole> CreateUserRoleAsync(UserRole userRole);
        Task<UserRole> UpdateUserRoleAsync(UserRole userRole);
        Task<bool> DeleteUserRoleAsync(int userRoleId);
    }
}
