using TempleApi.Domain.Entities;
using TempleApi.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

using TempleApi.Services.Interfaces;

namespace TempleApi.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _roleRepository.GetAllAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await _roleRepository.GetByIdAsync(roleId);
        }

        public async Task<Role> CreateRoleAsync(Role role)
        {
            await _roleRepository.AddAsync(role);
            return role;
        }

        public async Task<Role> UpdateRoleAsync(Role role)
        {
            await _roleRepository.UpdateAsync(role);
            return role;
        }

        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
            {
                return false;
            }

            await _roleRepository.DeleteAsync(role);
            return true;
        }
    }
}
