using MediatR;
using RoleService.Application.DTOs.RolePermissions;

namespace RoleService.Application.UseCases.RolePermissions.AssignPermission;

public record AssignPermissionCommand(Guid RoleId, Guid PermissionId) : IRequest<bool>;
