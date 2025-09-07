using TempleApi.Domain.Entities;
using TempleApi.Enums;

namespace TempleApi.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetByRoleAsync(UserRole role);
        Task<bool> ValidateCredentialsAsync(string email, string passwordHash);
    }
}
