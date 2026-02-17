using MediatR;
using Microsoft.Extensions.Logging;
using RoleService.Application.DTOs.RolePermissions;
using RoleService.Domain.Interfaces;
using RoleService.Shared.Exceptions;
using RoleService.Application.Interfaces;

namespace RoleService.Application.UseCases.RolePermissions.AssignPermission;

/// <summary>
/// Handler para asignar un permiso a un rol.
/// Valida existencia de rol y permiso, y previene duplicados.
/// </summary>
public class AssignPermissionCommandHandler : IRequestHandler<AssignPermissionCommand, AssignPermissionResponse>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IAuditServiceClient _auditClient;
    private readonly IUserContextService _userContext;
    private readonly IPermissionCacheService _cacheService;
    private readonly ILogger<AssignPermissionCommandHandler> _logger;

    public AssignPermissionCommandHandler(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IRolePermissionRepository rolePermissionRepository,
        IAuditServiceClient auditClient,
        IUserContextService userContext,
        IPermissionCacheService cacheService,
        ILogger<AssignPermissionCommandHandler> logger)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _auditClient = auditClient;
        _userContext = userContext;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<AssignPermissionResponse> Handle(AssignPermissionCommand request, CancellationToken cancellationToken)
    {
        // Paso 1: Verificar que el rol existe
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException($"Role with ID '{request.RoleId}' not found", "ROLE_NOT_FOUND");
        }

        // Paso 2: Verificar que el rol puede ser modificado (no es sistema)
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

        // Paso 4: Verificar que el permiso está activo
        if (!permission.IsActive)
        {
            throw new BadRequestException(
                $"Cannot assign inactive permission '{permission.Name}'",
                "PERMISSION_INACTIVE");
        }

        // Paso 5: Verificar si ya tiene el permiso
        var hasPermission = await _rolePermissionRepository.HasPermissionAsync(
            request.RoleId,
            request.PermissionId,
            cancellationToken);

        if (hasPermission)
        {
            throw new ConflictException(
                $"Role '{role.Name}' already has permission '{permission.Name}'",
                "PERMISSION_ALREADY_ASSIGNED");
        }

        // Paso 6: Asignar el permiso
        var currentUserId = _userContext.GetCurrentUserId();
        await _rolePermissionRepository.AssignPermissionToRoleAsync(
            request.RoleId,
            request.PermissionId,
            currentUserId,
            cancellationToken);

        _logger.LogInformation(
            "Permission {PermissionId} ({PermissionName}) assigned to role {RoleId} ({RoleName}) by user {UserId}",
            permission.Id, permission.Name, role.Id, role.Name, currentUserId);

        // Paso 7: Invalidar cache para este rol
        await _cacheService.InvalidateRolePermissionsAsync(request.RoleId, cancellationToken);

        // Paso 8: Auditoría (fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await _auditClient.LogPermissionAssignedAsync(
                    request.RoleId,
                    request.PermissionId,
                    currentUserId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for permission assignment");
            }
        }, cancellationToken);

        return new AssignPermissionResponse
        {
            Success = true,
            RoleId = role.Id,
            RoleName = role.Name,
            PermissionId = permission.Id,
            PermissionName = permission.Name,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = currentUserId
        };
    }
}
