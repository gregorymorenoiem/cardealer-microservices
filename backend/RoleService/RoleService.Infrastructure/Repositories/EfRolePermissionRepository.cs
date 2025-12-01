using Microsoft.EntityFrameworkCore;
using RoleService.Domain.Entities;
using RoleService.Domain.Enums;
using RoleService.Domain.Interfaces;
using RoleService.Infrastructure.Persistence;

namespace RoleService.Infrastructure.Repositories;

public class EfRolePermissionRepository : IRolePermissionRepository
{
    private readonly ApplicationDbContext _context;

    public EfRolePermissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AssignPermissionToRoleAsync(Guid roleId, Guid permissionId, string assignedBy, CancellationToken cancellationToken = default)
    {
        var rolePermission = new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionId = permissionId,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = assignedBy
        };

        await _context.RolePermissions.AddAsync(rolePermission, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
    {
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

        if (rolePermission != null)
        {
            _context.RolePermissions.Remove(rolePermission);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> HasPermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
    {
        return await _context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);
    }

    public async Task<bool> RoleHasPermissionAsync(Guid roleId, string resource, PermissionAction action, CancellationToken cancellationToken = default)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Join(_context.Permissions,
                rp => rp.PermissionId,
                p => p.Id,
                (rp, p) => p)
            .AnyAsync(p => p.Resource == resource && (p.Action == action || p.Action == PermissionAction.All), cancellationToken);
    }
}
