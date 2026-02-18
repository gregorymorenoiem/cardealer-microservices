using MediatR;
using Microsoft.Extensions.Logging;
using RoleService.Domain.Interfaces;
using RoleService.Shared.Exceptions;
using RoleService.Application.Interfaces;

namespace RoleService.Application.UseCases.Roles.DeleteRole;

/// <summary>
/// Handler para eliminar un rol del sistema.
/// Los roles del sistema (SuperAdmin, Admin) no pueden ser eliminados.
/// </summary>
public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, bool>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IAuditServiceClient _auditClient;
    private readonly INotificationServiceClient _notificationClient;
    private readonly IUserContextService _userContext;
    private readonly ILogger<DeleteRoleCommandHandler> _logger;

    public DeleteRoleCommandHandler(
        IRoleRepository roleRepository,
        IAuditServiceClient auditClient,
        INotificationServiceClient notificationClient,
        IUserContextService userContext,
        ILogger<DeleteRoleCommandHandler> logger)
    {
        _roleRepository = roleRepository;
        _auditClient = auditClient;
        _notificationClient = notificationClient;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        // Paso 1: Verificar que el rol existe
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            _logger.LogWarning("Attempted to delete non-existent role: {RoleId}", request.RoleId);
            throw new NotFoundException($"Role with ID '{request.RoleId}' not found", "ROLE_NOT_FOUND");
        }

        // Paso 2: Verificar que no es un rol del sistema
        if (role.IsSystemRole)
        {
            _logger.LogWarning("Attempted to delete system role: {RoleName}", role.Name);
            throw new BadRequestException(
                $"System role '{role.Name}' cannot be deleted. System roles are protected.",
                "ROLE_IS_SYSTEM");
        }

        // TODO: Paso 3 - Verificar que no tiene usuarios asignados (requiere UserService)
        // Por ahora, se permite eliminar aunque tenga usuarios

        var currentUserId = _userContext.GetCurrentUserId();
        var roleName = role.Name;

        // Paso 4: Eliminar el rol
        await _roleRepository.DeleteAsync(request.RoleId, cancellationToken);
        _logger.LogInformation("Role deleted: {RoleId} - {RoleName} by {UserId}",
            request.RoleId, roleName, currentUserId);

        // Paso 5: Auditoría (fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await _auditClient.LogRoleDeletedAsync(request.RoleId, roleName, currentUserId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for role deletion");
            }
        }, cancellationToken);

        // Paso 6: Notificación crítica a admins (fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await _notificationClient.SendCriticalRoleChangedNotificationAsync(
                    "admin@okla.com.do",
                    roleName,
                    $"Role '{roleName}' was deleted by user {currentUserId}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send notification for role deletion");
            }
        }, cancellationToken);

        return true;
    }
}
