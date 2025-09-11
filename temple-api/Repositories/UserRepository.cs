using Microsoft.EntityFrameworkCore;
using TempleApi.Data;
using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;

namespace TempleApi.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(TempleDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == roleName))
                .ToListAsync();
        }

        public async Task<bool> ValidateCredentialsAsync(string email, string passwordHash)
        {
            return await ExistsAsync(u => u.Email == email && u.PasswordHash == passwordHash && u.IsActive);
        }
    }
}
