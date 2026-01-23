using MediatR;
using Microsoft.Extensions.Logging;
using RoleService.Application.DTOs.Permissions;
using RoleService.Domain.Interfaces;

namespace RoleService.Application.UseCases.Permissions.GetPermissions;

/// <summary>
/// Handler para obtener lista de permisos con filtros opcionales.
/// Soporta filtrado por módulo, recurso, o lista completa.
/// </summary>
public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, List<PermissionListItemDto>>
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly ILogger<GetPermissionsQueryHandler> _logger;

    public GetPermissionsQueryHandler(
        IPermissionRepository permissionRepository,
        ILogger<GetPermissionsQueryHandler> logger)
    {
        _permissionRepository = permissionRepository;
        _logger = logger;
    }

    public async Task<List<PermissionListItemDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.Permission> permissions;

        // Paso 1: Obtener permisos según filtro
        if (!string.IsNullOrEmpty(request.Module))
        {
            _logger.LogDebug("Fetching permissions for module: {Module}", request.Module);
            permissions = await _permissionRepository.GetByModuleAsync(request.Module, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(request.Resource))
        {
            _logger.LogDebug("Fetching permissions for resource: {Resource}", request.Resource);
            permissions = await _permissionRepository.GetByResourceAsync(request.Resource, cancellationToken);
        }
        else
        {
            _logger.LogDebug("Fetching all permissions");
            permissions = await _permissionRepository.GetAllAsync(cancellationToken);
        }

        // Paso 2: Filtrar solo permisos activos si no se especifica lo contrario
        if (request.ActiveOnly)
        {
            permissions = permissions.Where(p => p.IsActive);
        }

        // Paso 3: Mapear a DTOs
        var result = permissions
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Resource)
            .ThenBy(p => p.Action)
            .Select(p => new PermissionListItemDto
            {
                Id = p.Id,
                Name = p.Name,
                DisplayName = p.DisplayName,
                Module = p.Module,
                IsActive = p.IsActive,
                IsSystemPermission = p.IsSystemPermission
            })
            .ToList();

        _logger.LogDebug("Returning {Count} permissions", result.Count);
        return result;
    }
}
