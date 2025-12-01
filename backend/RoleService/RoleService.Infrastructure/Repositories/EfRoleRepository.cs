using Microsoft.EntityFrameworkCore;
using RoleService.Domain.Entities;
using RoleService.Domain.Interfaces;
using RoleService.Infrastructure.Persistence;

namespace RoleService.Infrastructure.Repositories;

public class EfRoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _context;

    public EfRoleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .OrderBy(r => r.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetActiveRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Where(r => r.IsActive)
            .OrderBy(r => r.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        await _context.Roles.AddAsync(role, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        _context.Roles.Update(role);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await GetByIdAsync(id, cancellationToken);
        if (role != null)
        {
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
