using MediatR;
using RoleService.Application.DTOs.RolePermissions;

namespace RoleService.Application.UseCases.RolePermissions.RemovePermission;

/// <summary>
/// Comando para remover un permiso de un rol.
/// </summary>
/// <param name="RoleId">ID del rol del que se removerá el permiso</param>
/// <param name="PermissionId">ID del permiso a remover</param>
public record RemovePermissionCommand(Guid RoleId, Guid PermissionId) : IRequest<RemovePermissionResponse>;
