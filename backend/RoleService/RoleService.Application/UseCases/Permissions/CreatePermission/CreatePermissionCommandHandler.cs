using MediatR;
using RoleService.Application.DTOs.Permissions;
using RoleService.Domain.Entities;
using RoleService.Domain.Enums;
using RoleService.Domain.Interfaces;

namespace RoleService.Application.UseCases.Permissions.CreatePermission;

public class CreatePermissionCommandHandler : IRequestHandler<CreatePermissionCommand, CreatePermissionResponse>
{
    private readonly IPermissionRepository _permissionRepository;

    public CreatePermissionCommandHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<CreatePermissionResponse> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PermissionAction>(request.Request.Action, out var action))
        {
            throw new ArgumentException($"Invalid action: {request.Request.Action}");
        }

        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Name = request.Request.Name,
            Description = request.Request.Description,
            Resource = request.Request.Resource,
            Action = action,
            Module = request.Request.Module,
            IsActive = true,
            IsSystemPermission = request.Request.IsSystemPermission,
            CreatedAt = DateTime.UtcNow
        };

        await _permissionRepository.AddAsync(permission, cancellationToken);

        return new CreatePermissionResponse(
            permission.Id,
            permission.Name,
            permission.Description,
            permission.Resource,
            permission.Action.ToString(),
            permission.Module,
            permission.IsSystemPermission,
            permission.CreatedAt
        );
    }
}
