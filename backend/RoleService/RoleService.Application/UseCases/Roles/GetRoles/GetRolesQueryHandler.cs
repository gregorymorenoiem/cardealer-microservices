using MediatR;
using Microsoft.Extensions.Logging;
using RoleService.Application.DTOs.Roles;
using RoleService.Domain.Interfaces;
using RoleService.Shared.Models;

namespace RoleService.Application.UseCases.Roles.GetRoles;

/// <summary>
/// Handler para obtener roles paginados con filtros opcionales.
/// </summary>
public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, PaginatedResult<RoleListItemDto>>
{
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<GetRolesQueryHandler> _logger;

    public GetRolesQueryHandler(
        IRoleRepository roleRepository,
        ILogger<GetRolesQueryHandler> logger)
    {
        _roleRepository = roleRepository;
        _logger = logger;
    }

    public async Task<PaginatedResult<RoleListItemDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting roles with filter IsActive={IsActive}, Page={Page}, PageSize={PageSize}",
            request.IsActive, request.Page, request.PageSize);

        // Obtener roles según filtro
        var roles = request.IsActive.HasValue && request.IsActive.Value
            ? await _roleRepository.GetActiveRolesAsync(cancellationToken)
            : await _roleRepository.GetAllAsync(cancellationToken);

        var rolesList = roles.ToList();
        var totalCount = rolesList.Count;

        // Paginación
        var paginatedRoles = rolesList
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Mapear a DTOs
        var items = new List<RoleListItemDto>();
        foreach (var role in paginatedRoles)
        {
            var permissions = await _roleRepository.GetRolePermissionsAsync(role.Id, cancellationToken);
            items.Add(new RoleListItemDto
            {
                Id = role.Id,
                Name = role.Name,
                DisplayName = role.DisplayName,
                IsActive = role.IsActive,
                IsSystemRole = role.IsSystemRole,
                UserCount = 0, // TODO: Obtener de UserService cuando esté integrado
                PermissionCount = permissions.Count(),
                CreatedAt = role.CreatedAt
            });
        }

        _logger.LogDebug("Returning {Count} of {Total} roles", items.Count, totalCount);

        return new PaginatedResult<RoleListItemDto>(
            items,
            totalCount,
            request.Page,
            request.PageSize
        );
    }
}
