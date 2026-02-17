using MediatR;
using RoleService.Application.DTOs.RolePermissions;

namespace RoleService.Application.UseCases.RolePermissions.AssignPermission;

/// <summary>
/// Comando para asignar un permiso a un rol.
/// </summary>
/// <param name="RoleId">ID del rol al que se asignará el permiso</param>
/// <param name="PermissionId">ID del permiso a asignar</param>
public record AssignPermissionCommand(Guid RoleId, Guid PermissionId) : IRequest<AssignPermissionResponse>;
