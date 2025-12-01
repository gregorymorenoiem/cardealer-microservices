using MediatR;
using RoleService.Application.DTOs.RolePermissions;
using RoleService.Domain.Enums;
using RoleService.Domain.Interfaces;

namespace RoleService.Application.UseCases.RolePermissions.CheckPermission;

public class CheckPermissionQueryHandler : IRequestHandler<CheckPermissionQuery, CheckPermissionResponse>
{
    private readonly IRolePermissionRepository _rolePermissionRepository;

    public CheckPermissionQueryHandler(IRolePermissionRepository rolePermissionRepository)
    {
        _rolePermissionRepository = rolePermissionRepository;
    }

    public async Task<CheckPermissionResponse> Handle(CheckPermissionQuery request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PermissionAction>(request.Action, out var action))
        {
            return new CheckPermissionResponse(false, $"Invalid action: {request.Action}");
        }

        // TODO: En una implementación real, aquí necesitarías:
        // 1. Obtener los roles del usuario desde UserService
        // 2. Para cada rol, verificar si tiene el permiso
        // Por ahora, retornamos un placeholder

        var hasPermission = false; // await _rolePermissionRepository.UserHasPermissionAsync(request.UserId, request.Resource, action, cancellationToken);

        return new CheckPermissionResponse(
            hasPermission,
            hasPermission ? "User has permission" : "User does not have permission"
        );
    }
}
