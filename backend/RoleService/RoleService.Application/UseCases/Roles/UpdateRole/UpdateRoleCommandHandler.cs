using MediatR;
using System.Text;
using RoleService.Application.DTOs.Roles;
using RoleService.Domain.Interfaces;
using RoleService.Shared.Exceptions;
using RoleService.Application.Interfaces;

namespace RoleService.Application.UseCases.Roles.UpdateRole;

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, UpdateRoleResponse>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IAuditServiceClient _auditClient;
    private readonly IUserContextService _userContext;

    public UpdateRoleCommandHandler(
        IRoleRepository roleRepository,
        IAuditServiceClient auditClient,
        IUserContextService userContext)
    {
        _roleRepository = roleRepository;
        _auditClient = auditClient;
        _userContext = userContext;
    }

    public async Task<UpdateRoleResponse> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException($"Role with ID '{request.RoleId}' not found");
        }

        if (role.IsSystemRole)
        {
            throw new ForbiddenException("System roles cannot be modified");
        }

        // Verificar si el nuevo nombre ya existe (si cambió)
        var changes = new StringBuilder();

        if (role.Name != request.Request.Name)
        {
            var existingRole = await _roleRepository.GetByNameAsync(request.Request.Name, cancellationToken);
            if (existingRole != null && existingRole.Id != request.RoleId)
            {
                throw new ConflictException($"Role with name '{request.Request.Name}' already exists");
            }
            changes.Append($"Name: {role.Name} → {request.Request.Name}; ");
            role.Name = request.Request.Name;
        }

        if (role.Description != request.Request.Description)
        {
            changes.Append($"Description: {role.Description} → {request.Request.Description}; ");
            role.Description = request.Request.Description;
        }

        if (role.Priority != request.Request.Priority)
        {
            changes.Append($"Priority: {role.Priority} → {request.Request.Priority}; ");
            role.Priority = request.Request.Priority;
        }

        if (role.IsActive != request.Request.IsActive)
        {
            changes.Append($"IsActive: {role.IsActive} → {request.Request.IsActive}; ");
            role.IsActive = request.Request.IsActive;
        }

        role.UpdatedAt = DateTime.UtcNow;
        role.UpdatedBy = _userContext.GetCurrentUserId();

        await _roleRepository.UpdateAsync(role, cancellationToken);

        // Auditoría
        if (changes.Length > 0)
        {
            _ = _auditClient.LogRoleUpdatedAsync(role.Id, changes.ToString(), _userContext.GetCurrentUserId());
        }

        return new UpdateRoleResponse(
            role.Id,
            role.Name,
            role.Description,
            role.Priority,
            role.IsActive,
            role.UpdatedAt.Value
        );
    }
}
