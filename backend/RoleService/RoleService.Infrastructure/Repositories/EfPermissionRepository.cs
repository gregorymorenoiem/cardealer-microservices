using Microsoft.EntityFrameworkCore;
using RoleService.Domain.Entities;
using RoleService.Domain.Enums;
using RoleService.Domain.Interfaces;
using RoleService.Infrastructure.Persistence;

namespace RoleService.Infrastructure.Repositories;

public class EfPermissionRepository : IPermissionRepository
{
    private readonly ApplicationDbContext _context;

    public EfPermissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetByModuleAsync(string module, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .Where(p => p.Module == module)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetByResourceAsync(string resource, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .Where(p => p.Resource == resource)
            .OrderBy(p => p.Action)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetByActionAsync(PermissionAction action, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .Where(p => p.Action == action)
            .OrderBy(p => p.Module)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        await _context.Permissions.AddAsync(permission, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        _context.Permissions.Update(permission);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var permission = await GetByIdAsync(id, cancellationToken);
        if (permission != null)
        {
            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
