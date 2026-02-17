using MediatR;
using Microsoft.Extensions.Logging;
using RoleService.Application.DTOs.RolePermissions;
using RoleService.Domain.Interfaces;
using RoleService.Shared.Exceptions;
using RoleService.Application.Interfaces;

namespace RoleService.Application.UseCases.RolePermissions.RemovePermission;

/// <summary>
/// Handler para remover un permiso de un rol.
/// Valida que el rol no sea del sistema y que la relación exista.
/// </summary>
public class RemovePermissionCommandHandler : IRequestHandler<RemovePermissionCommand, RemovePermissionResponse>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IAuditServiceClient _auditClient;
    private readonly IUserContextService _userContext;
    private readonly IPermissionCacheService _cacheService;
    private readonly ILogger<RemovePermissionCommandHandler> _logger;

    public RemovePermissionCommandHandler(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IRolePermissionRepository rolePermissionRepository,
        IAuditServiceClient auditClient,
        IUserContextService userContext,
        IPermissionCacheService cacheService,
        ILogger<RemovePermissionCommandHandler> logger)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _auditClient = auditClient;
        _userContext = userContext;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<RemovePermissionResponse> Handle(RemovePermissionCommand request, CancellationToken cancellationToken)
    {
        // Paso 1: Verificar que el rol existe
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException($"Role with ID '{request.RoleId}' not found", "ROLE_NOT_FOUND");
        }

        // Paso 2: Verificar que el rol puede ser modificado
        if (!role.CanBeModified())
        {
            throw new ForbiddenException(
                $"Cannot modify permissions for system role '{role.Name}'",
                "SYSTEM_ROLE_IMMUTABLE");
        }

        // Paso 3: Verificar que el permiso existe
        var permission = await _permissionRepository.GetByIdAsync(request.PermissionId, cancellationToken);
        if (permission == null)
        {
            throw new NotFoundException($"Permission with ID '{request.PermissionId}' not found", "PERMISSION_NOT_FOUND");
        }

        // Paso 4: Verificar que la relación existe
        var hasPermission = await _rolePermissionRepository.HasPermissionAsync(
            request.RoleId,
            request.PermissionId,
            cancellationToken);

        if (!hasPermission)
        {
            throw new NotFoundException(
                $"Role '{role.Name}' does not have permission '{permission.Name}'",
                "PERMISSION_NOT_ASSIGNED");
        }

        // Paso 5: Remover el permiso
        var currentUserId = _userContext.GetCurrentUserId();
        await _rolePermissionRepository.RemovePermissionFromRoleAsync(
            request.RoleId,
            request.PermissionId,
            cancellationToken);

        _logger.LogInformation(
            "Permission {PermissionId} ({PermissionName}) removed from role {RoleId} ({RoleName}) by user {UserId}",
            permission.Id, permission.Name, role.Id, role.Name, currentUserId);

        // Paso 6: Invalidar cache para este rol
        await _cacheService.InvalidateRolePermissionsAsync(request.RoleId, cancellationToken);

        // Paso 7: Auditoría (fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await _auditClient.LogPermissionRemovedAsync(
                    request.RoleId,
                    request.PermissionId,
                    currentUserId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for permission removal");
            }
        }, cancellationToken);

        return new RemovePermissionResponse
        {
            Success = true,
            RoleId = role.Id,
            RoleName = role.Name,
            PermissionId = permission.Id,
            PermissionName = permission.Name,
            RemovedAt = DateTime.UtcNow,
            RemovedBy = currentUserId
        };
    }
}
