using TempleApi.Domain.Entities;

namespace TempleApi.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> ValidateCredentialsAsync(string email, string passwordHash);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);
    }
}
