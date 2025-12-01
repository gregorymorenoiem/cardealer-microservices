using MediatR;
using RoleService.Application.DTOs.RolePermissions;

namespace RoleService.Application.UseCases.RolePermissions.CheckPermission;

public record CheckPermissionQuery(Guid UserId, string Resource, string Action) : IRequest<CheckPermissionResponse>;
