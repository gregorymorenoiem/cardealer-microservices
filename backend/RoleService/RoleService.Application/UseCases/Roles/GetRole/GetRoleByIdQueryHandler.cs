using MediatR;
using Microsoft.Extensions.Logging;
using RoleService.Application.DTOs.Roles;
using RoleService.Domain.Interfaces;

namespace RoleService.Application.UseCases.Roles.GetRole;

/// <summary>
/// Handler para obtener un rol por ID con todos sus permisos.
/// </summary>
public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleDetailsDto?>
{
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<GetRoleByIdQueryHandler> _logger;

    public GetRoleByIdQueryHandler(
        IRoleRepository roleRepository,
        ILogger<GetRoleByIdQueryHandler> logger)
    {
        _roleRepository = roleRepository;
        _logger = logger;
    }

    public async Task<RoleDetailsDto?> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting role by ID: {RoleId}", request.RoleId);
        
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            _logger.LogDebug("Role not found: {RoleId}", request.RoleId);
            return null;
        }

        var permissions = await _roleRepository.GetRolePermissionsAsync(request.RoleId, cancellationToken);

        _logger.LogDebug("Found role {RoleName} with {PermissionCount} permissions", 
            role.Name, permissions.Count());

        return new RoleDetailsDto
        {
            Id = role.Id,
            Name = role.Name,
            DisplayName = role.DisplayName,
            Description = role.Description,
            IsActive = role.IsActive,
            IsSystemRole = role.IsSystemRole,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt,
            CreatedBy = role.CreatedBy,
            UpdatedBy = role.UpdatedBy,
            Permissions = permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                DisplayName = p.DisplayName,
                Description = p.Description,
                Resource = p.Resource,
                Action = p.Action.ToString().ToLowerInvariant(),
                Module = p.Module
            }).ToList()
        };
    }
}
