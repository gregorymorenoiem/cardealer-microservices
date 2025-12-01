using MediatR;
using RoleService.Application.DTOs.Roles;
using RoleService.Shared.Models;

namespace RoleService.Application.UseCases.Roles.GetRoles;

public record GetRolesQuery(
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 50
) : IRequest<PaginatedResult<RoleListItemDto>>;
