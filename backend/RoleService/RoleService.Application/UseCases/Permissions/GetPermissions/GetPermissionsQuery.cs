using MediatR;
using RoleService.Application.DTOs.Permissions;

namespace RoleService.Application.UseCases.Permissions.GetPermissions;

/// <summary>
/// Query para obtener lista de permisos.
/// Soporta filtrado por módulo, recurso, y estado activo.
/// </summary>
public record GetPermissionsQuery(
    string? Module = null,
    string? Resource = null,
    bool ActiveOnly = true
) : IRequest<List<PermissionListItemDto>>;
