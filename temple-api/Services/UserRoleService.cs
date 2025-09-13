using TempleApi.Domain.Entities;
using TempleApi.Services.Interfaces;
using TempleApi.Data;
using Microsoft.EntityFrameworkCore;

namespace TempleApi.Services
{
	public class UserRoleService : IUserRoleService
	{
		private readonly TempleDbContext _db;

		public UserRoleService(TempleDbContext db)
		{
			_db = db;
		}

		public async Task<IEnumerable<UserRole>> GetAllUserRolesAsync()
		{
			return await _db.UserRoles
				.Include(ur => ur.User)
				.Include(ur => ur.Role)
				.ToListAsync();
		}

		public async Task<UserRole> CreateUserRoleAsync(UserRole userRole)
		{
			_db.UserRoles.Add(userRole);
			await _db.SaveChangesAsync();
			return userRole;
		}

		public async Task<UserRole> UpdateUserRoleAsync(UserRole userRole)
		{
			_db.UserRoles.Update(userRole);
			await _db.SaveChangesAsync();
			return userRole;
		}

		public async Task<bool> DeleteUserRoleAsync(int userRoleId)
		{
			var existing = await _db.UserRoles.FindAsync(userRoleId);
			if (existing == null) return false;
			_db.UserRoles.Remove(existing);
			await _db.SaveChangesAsync();
			return true;
		}
	}
}
