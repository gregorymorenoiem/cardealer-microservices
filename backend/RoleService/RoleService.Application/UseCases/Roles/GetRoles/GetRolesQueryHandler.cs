using MediatR;
using RoleService.Application.DTOs.Roles;
using RoleService.Domain.Interfaces;
using RoleService.Shared.Models;

namespace RoleService.Application.UseCases.Roles.GetRoles;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, PaginatedResult<RoleListItemDto>>
{
    private readonly IRoleRepository _roleRepository;

    public GetRolesQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<PaginatedResult<RoleListItemDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = request.IsActive.HasValue
            ? await _roleRepository.GetActiveRolesAsync(cancellationToken)
            : await _roleRepository.GetAllAsync(cancellationToken);

        var rolesList = roles.ToList();
        var totalCount = rolesList.Count;

        // Paginación
        var paginatedRoles = rolesList
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var items = new List<RoleListItemDto>();
        foreach (var role in paginatedRoles)
        {
            var permissions = await _roleRepository.GetRolePermissionsAsync(role.Id, cancellationToken);
            items.Add(new RoleListItemDto(
                role.Id,
                role.Name,
                role.Description,
                role.Priority,
                role.IsActive,
                role.IsSystemRole,
                permissions.Count(),
                role.CreatedAt
            ));
        }

        return new PaginatedResult<RoleListItemDto>(
            items,
            totalCount,
            request.Page,
            request.PageSize
        );
    }
}
