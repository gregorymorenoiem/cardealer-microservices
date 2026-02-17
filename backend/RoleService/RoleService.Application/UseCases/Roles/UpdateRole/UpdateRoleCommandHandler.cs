using MediatR;
using Microsoft.Extensions.Logging;
using System.Text;
using RoleService.Application.DTOs.Roles;
using RoleService.Domain.Interfaces;
using RoleService.Shared.Exceptions;
using RoleService.Application.Interfaces;

namespace RoleService.Application.UseCases.Roles.UpdateRole;

/// <summary>
/// Handler para actualizar un rol existente.
/// Los roles del sistema (SuperAdmin, Admin) no pueden ser modificados.
/// El campo Name es inmutable.
/// </summary>
public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, UpdateRoleResponse>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IAuditServiceClient _auditClient;
    private readonly IUserContextService _userContext;
    private readonly ILogger<UpdateRoleCommandHandler> _logger;

    public UpdateRoleCommandHandler(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IRolePermissionRepository rolePermissionRepository,
        IAuditServiceClient auditClient,
        IUserContextService userContext,
        ILogger<UpdateRoleCommandHandler> logger)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _auditClient = auditClient;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<UpdateRoleResponse> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        
        // Paso 1: Verificar que el rol existe
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            _logger.LogWarning("Attempted to update non-existent role: {RoleId}", request.RoleId);
            throw new NotFoundException($"Role with ID '{request.RoleId}' not found", "ROLE_NOT_FOUND");
        }

        // Paso 2: Verificar que no es un rol del sistema
        if (role.IsSystemRole)
        {
            _logger.LogWarning("Attempted to modify system role: {RoleName}", role.Name);
            throw new BadRequestException(
                $"System role '{role.Name}' cannot be modified. System roles are immutable.", 
                "ROLE_IS_SYSTEM");
        }

        var changes = new StringBuilder();
        var currentUserId = _userContext.GetCurrentUserId();

        // Paso 3: Actualizar DisplayName (si se proporciona)
        if (!string.IsNullOrEmpty(req.DisplayName) && role.DisplayName != req.DisplayName)
        {
            changes.Append($"DisplayName: '{role.DisplayName}' → '{req.DisplayName}'; ");
            role.DisplayName = req.DisplayName;
        }

        // Paso 4: Actualizar Description (si se proporciona)
        if (req.Description != null && role.Description != req.Description)
        {
            changes.Append($"Description changed; ");
            role.Description = req.Description;
        }

        // Paso 5: Actualizar IsActive (si se proporciona)
        if (req.IsActive.HasValue && role.IsActive != req.IsActive.Value)
        {
            changes.Append($"IsActive: {role.IsActive} → {req.IsActive.Value}; ");
            role.IsActive = req.IsActive.Value;
        }

        // Paso 6: Sincronizar permisos (si se proporcionan)
        int permissionCount = 0;
        if (req.PermissionIds != null)
        {
            // Validar que todos los permisos existen
            foreach (var permissionId in req.PermissionIds)
            {
                var permission = await _permissionRepository.GetByIdAsync(permissionId, cancellationToken);
                if (permission == null)
                {
                    throw new BadRequestException($"Permission with ID '{permissionId}' not found", "INVALID_PERMISSION");
                }
            }

            // Obtener permisos actuales
            var currentPermissions = await _roleRepository.GetRolePermissionsAsync(role.Id, cancellationToken);
            var currentPermissionIds = currentPermissions.Select(p => p.Id).ToHashSet();
            var newPermissionIds = req.PermissionIds.ToHashSet();

            // Remover permisos que ya no están
            var toRemove = currentPermissionIds.Except(newPermissionIds);
            foreach (var permissionId in toRemove)
            {
                await _rolePermissionRepository.RemovePermissionFromRoleAsync(role.Id, permissionId, cancellationToken);
            }

            // Agregar nuevos permisos
            var toAdd = newPermissionIds.Except(currentPermissionIds);
            foreach (var permissionId in toAdd)
            {
                await _rolePermissionRepository.AssignPermissionToRoleAsync(role.Id, permissionId, currentUserId, cancellationToken);
            }

            if (toRemove.Any() || toAdd.Any())
            {
                changes.Append($"Permissions: -{toRemove.Count()}/+{toAdd.Count()}; ");
            }

            permissionCount = req.PermissionIds.Count;
        }
        else
        {
            var currentPermissions = await _roleRepository.GetRolePermissionsAsync(role.Id, cancellationToken);
            permissionCount = currentPermissions.Count();
        }

        // Paso 7: Actualizar timestamps
        role.UpdatedAt = DateTime.UtcNow;
        role.UpdatedBy = currentUserId;

        await _roleRepository.UpdateAsync(role, cancellationToken);
        _logger.LogInformation("Role updated: {RoleId} - Changes: {Changes}", role.Id, changes.ToString());

        // Paso 8: Auditoría (fire-and-forget)
        if (changes.Length > 0)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _auditClient.LogRoleUpdatedAsync(role.Id, changes.ToString(), currentUserId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send audit log for role update");
                }
            }, cancellationToken);
        }

        return new UpdateRoleResponse
        {
            Success = true,
            Data = new RoleUpdatedData
            {
                Id = role.Id,
                Name = role.Name,
                DisplayName = role.DisplayName,
                Description = role.Description,
                IsActive = role.IsActive,
                IsSystemRole = role.IsSystemRole,
                PermissionCount = permissionCount,
                UpdatedAt = role.UpdatedAt.Value
            }
        };
    }
}
