using MediatR;

namespace RoleService.Application.UseCases.RolePermissions.RemovePermission;

public record RemovePermissionCommand(Guid RoleId, Guid PermissionId) : IRequest<bool>;
