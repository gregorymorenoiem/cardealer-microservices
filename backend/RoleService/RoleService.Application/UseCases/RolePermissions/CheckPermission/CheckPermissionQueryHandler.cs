using MediatR;
using RoleService.Application.DTOs.RolePermissions;
using RoleService.Application.Interfaces;
using RoleService.Domain.Enums;
using RoleService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace RoleService.Application.UseCases.RolePermissions.CheckPermission;

/// <summary>
/// Handler para verificar si un conjunto de roles tiene un permiso específico.
/// Implementa caching con TTL de 5 minutos para optimizar rendimiento.
/// Este endpoint es crítico para performance ya que se llama en cada request autenticado.
/// </summary>
public class CheckPermissionQueryHandler : IRequestHandler<CheckPermissionQuery, CheckPermissionResponse>
{
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionCacheService _cacheService;
    private readonly ILogger<CheckPermissionQueryHandler> _logger;

    public CheckPermissionQueryHandler(
        IRolePermissionRepository rolePermissionRepository,
        IRoleRepository roleRepository,
        IPermissionCacheService cacheService,
        ILogger<CheckPermissionQueryHandler> logger)
    {
        _rolePermissionRepository = rolePermissionRepository;
        _roleRepository = roleRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<CheckPermissionResponse> Handle(CheckPermissionQuery request, CancellationToken cancellationToken)
    {
        // Paso 1: Validar acción
        if (!Enum.TryParse<PermissionAction>(request.Action, true, out var action))
        {
            _logger.LogWarning("Invalid permission action requested: {Action}", request.Action);
            return new CheckPermissionResponse
            {
                HasPermission = false,
                Reason = $"Invalid action: {request.Action}",
                Cached = false
            };
        }

        // Paso 2: Validar que hay roles
        var roleIds = request.RoleIds?.ToList() ?? new List<Guid>();
        if (!roleIds.Any())
        {
            _logger.LogWarning("No role IDs provided for permission check");
            return new CheckPermissionResponse
            {
                HasPermission = false,
                Reason = "No roles provided",
                Cached = false
            };
        }

        // Paso 3: Verificar cada rol (con cache primero)
        foreach (var roleId in roleIds)
        {
            try
            {
                // Paso 3.1: Intentar obtener del cache
                var cachedResult = await _cacheService.GetCachedPermissionCheckAsync(
                    roleId,
                    request.Resource,
                    request.Action,
                    cancellationToken);

                if (cachedResult.HasValue)
                {
                    if (cachedResult.Value)
                    {
                        // Obtener nombre del rol para la respuesta
                        var roleName = await GetRoleNameAsync(roleId, cancellationToken);

                        _logger.LogDebug(
                            "Permission GRANTED (cached) for role {RoleId} on resource {Resource} with action {Action}",
                            roleId, request.Resource, request.Action);

                        return new CheckPermissionResponse
                        {
                            HasPermission = true,
                            Reason = $"Permission granted via role {roleName}",
                            Permission = $"{request.Resource}:{request.Action}".ToLowerInvariant(),
                            GrantedByRole = roleName,
                            Cached = true
                        };
                    }
                    // Si está en cache como false, continuar con el siguiente rol
                    continue;
                }

                // Paso 3.2: Verificar en base de datos
                var hasPermission = await _rolePermissionRepository.RoleHasPermissionAsync(
                    roleId,
                    request.Resource,
                    action,
                    cancellationToken);

                // Paso 3.3: Guardar en cache
                await _cacheService.SetCachedPermissionCheckAsync(
                    roleId,
                    request.Resource,
                    request.Action,
                    hasPermission,
                    cancellationToken);

                if (hasPermission)
                {
                    var roleName = await GetRoleNameAsync(roleId, cancellationToken);

                    _logger.LogDebug(
                        "Permission GRANTED for role {RoleId} on resource {Resource} with action {Action}",
                        roleId, request.Resource, request.Action);

                    return new CheckPermissionResponse
                    {
                        HasPermission = true,
                        Reason = $"Permission granted via role {roleName}",
                        Permission = $"{request.Resource}:{request.Action}".ToLowerInvariant(),
                        GrantedByRole = roleName,
                        Cached = false
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error checking permission for role {RoleId} on resource {Resource}",
                    roleId, request.Resource);
                // Continuar verificando otros roles
            }
        }

        // Paso 4: Ningún rol tiene el permiso
        _logger.LogDebug(
            "Permission DENIED for roles [{RoleIds}] on resource {Resource} with action {Action}",
            string.Join(", ", roleIds), request.Resource, request.Action);

        return new CheckPermissionResponse
        {
            HasPermission = false,
            Reason = "User does not have the required permission",
            Permission = $"{request.Resource}:{request.Action}".ToLowerInvariant(),
            Cached = false
        };
    }

    /// <summary>
    /// Obtiene el nombre de un rol para incluir en la respuesta.
    /// </summary>
    private async Task<string> GetRoleNameAsync(Guid roleId, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
            return role?.Name ?? roleId.ToString();
        }
        catch
        {
            return roleId.ToString();
        }
    }
}
