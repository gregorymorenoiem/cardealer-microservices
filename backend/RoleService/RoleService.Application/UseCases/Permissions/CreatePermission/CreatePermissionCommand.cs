using MediatR;
using RoleService.Application.DTOs.Permissions;

namespace RoleService.Application.UseCases.Permissions.CreatePermission;

public record CreatePermissionCommand(CreatePermissionRequest Request) : IRequest<CreatePermissionResponse>;
