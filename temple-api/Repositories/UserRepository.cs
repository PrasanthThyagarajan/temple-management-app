using TempleApi.Data;
using TempleApi.Repositories.Interfaces;
using TempleApi.Domain.Entities;
using TempleApi.Enums;

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

        public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
        {
            return await FindAsync(u => u.Role == role && u.IsActive);
        }

        public async Task<bool> ValidateCredentialsAsync(string email, string passwordHash)
        {
            return await ExistsAsync(u => u.Email == email && u.PasswordHash == passwordHash && u.IsActive);
        }
    }
}
