using MediatR;
using RoleService.Application.DTOs.Permissions;

namespace RoleService.Application.UseCases.Permissions.GetPermissions;

public record GetPermissionsQuery(
    string? Module = null,
    string? Resource = null
) : IRequest<List<PermissionDetailsDto>>;
