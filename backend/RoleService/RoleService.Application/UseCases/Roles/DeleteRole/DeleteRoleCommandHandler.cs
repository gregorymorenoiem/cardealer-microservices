using MediatR;
using RoleService.Domain.Interfaces;
using RoleService.Shared.Exceptions;
using RoleService.Application.Interfaces;

namespace RoleService.Application.UseCases.Roles.DeleteRole;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, bool>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IAuditServiceClient _auditClient;
    private readonly INotificationServiceClient _notificationClient;

    public DeleteRoleCommandHandler(
        IRoleRepository roleRepository,
        IAuditServiceClient auditClient,
        INotificationServiceClient notificationClient)
    {
        _roleRepository = roleRepository;
        _auditClient = auditClient;
        _notificationClient = notificationClient;
    }

    public async Task<bool> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException($"Role with ID '{request.RoleId}' not found");
        }

        if (role.IsSystemRole)
        {
            throw new ForbiddenException("System roles cannot be deleted");
        }

        await _roleRepository.DeleteAsync(request.RoleId, cancellationToken);

        // Auditoría
        _ = _auditClient.LogRoleDeletedAsync(role.Id, role.Name, "system");

        // Notificación crítica a admins
        _ = _notificationClient.SendCriticalRoleChangedNotificationAsync(
            "admin@cardealer.com",
            role.Name,
            "Role Deleted");

        return true;
    }
}
