using TempleApi.Domain.Entities;
using TempleApi.Repositories.Interfaces;
using TempleApi.Data;

namespace TempleApi.Repositories
{
	public class RoleRepository : Repository<Role>, IRoleRepository
	{
		public RoleRepository(TempleDbContext context) : base(context)
		{
		}
	}
}
