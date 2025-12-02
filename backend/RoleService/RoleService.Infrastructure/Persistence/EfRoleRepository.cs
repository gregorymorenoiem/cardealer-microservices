using RoleService.Infrastructure.Repositories;
using RoleService.Infrastructure.Persistence;

namespace RoleService.Infrastructure.Persistence
{
    public class EfRoleRepository : Repositories.EfRoleRepository
    {
        public EfRoleRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
