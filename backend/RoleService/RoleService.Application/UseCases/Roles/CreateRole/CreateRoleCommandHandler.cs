using MediatR;
using RoleService.Application.DTOs.Roles;
using RoleService.Domain.Entities;
using RoleService.Domain.Interfaces;
using RoleService.Shared.Exceptions;
using RoleService.Application.Interfaces;

namespace RoleService.Application.UseCases.Roles.CreateRole;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, CreateRoleResponse>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IAuditServiceClient _auditClient;
    private readonly INotificationServiceClient _notificationClient;

    public CreateRoleCommandHandler(
        IRoleRepository roleRepository,
        IAuditServiceClient auditClient,
        INotificationServiceClient notificationClient)
    {
        _roleRepository = roleRepository;
        _auditClient = auditClient;
        _notificationClient = notificationClient;
    }

    public async Task<CreateRoleResponse> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // Verificar que no exista un role con el mismo nombre
        var existingRole = await _roleRepository.GetByNameAsync(request.Request.Name, cancellationToken);
        if (existingRole != null)
        {
            throw new ConflictException($"Role with name '{request.Request.Name}' already exists");
        }

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = request.Request.Name,
            Description = request.Request.Description,
            Priority = request.Request.Priority,
            IsActive = true,
            IsSystemRole = request.Request.IsSystemRole,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system" // TODO: Get from JWT claims
        };

        await _roleRepository.AddAsync(role, cancellationToken);

        // Auditoría
        _ = _auditClient.LogRoleCreatedAsync(role.Id, role.Name, "system");

        // Notificación a admins
        _ = _notificationClient.SendRoleCreatedNotificationAsync(
            "admin@cardealer.com",
            role.Name);

        return new CreateRoleResponse(
            role.Id,
            role.Name,
            role.Description ?? string.Empty,
            role.Priority,
            role.IsSystemRole,
            role.CreatedAt
        );
    }
}
