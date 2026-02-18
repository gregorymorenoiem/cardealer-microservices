using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoleService.Application.DTOs.RolePermissions;
using RoleService.Application.UseCases.RolePermissions.AssignPermission;
using RoleService.Application.UseCases.RolePermissions.RemovePermission;
using RoleService.Application.UseCases.RolePermissions.CheckPermission;
using RoleService.Shared.Models;
using RoleService.Shared.RateLimiting;
using RoleService.Shared.Exceptions;

namespace RoleService.Api.Controllers;

/// <summary>
/// Controller para gestión de asignación de permisos a roles.
/// Este es el corazón del sistema de autorización RBAC.
/// </summary>
[ApiController]
[Route("api/role-permissions")]
[Produces("application/json")]
public class RolePermissionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RolePermissionsController> _logger;

    public RolePermissionsController(IMediator mediator, ILogger<RolePermissionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Asigna un permiso a un rol.
    /// Requiere permiso: admin:manage-roles
    /// </summary>
    /// <param name="request">IDs del rol y permiso</param>
    /// <returns>Resultado de la asignación</returns>
    /// <response code="200">Permiso asignado exitosamente</response>
    /// <response code="404">Rol o permiso no existe</response>
    /// <response code="409">Permiso ya está asignado al rol</response>
    /// <response code="403">No tiene permiso para modificar este rol (rol del sistema)</response>
    [HttpPost("assign")]
    [Authorize(Policy = "ManageRoles")]
    [RateLimit(maxRequests: 100, windowSeconds: 60)]
    [ProducesResponseType(typeof(ApiResponse<AssignPermissionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<AssignPermissionResponse>>> AssignPermission([FromBody] AssignPermissionRequest request)
    {
        _logger.LogInformation("Assigning permission {PermissionId} to role {RoleId}",
            request.PermissionId, request.RoleId);

        var command = new AssignPermissionCommand(request.RoleId, request.PermissionId);
        var result = await _mediator.Send(command);

        _logger.LogInformation("Permission {PermissionName} assigned to role {RoleName} successfully",
            result.PermissionName, result.RoleName);

        return Ok(ApiResponse<AssignPermissionResponse>.Ok(result));
    }

    /// <summary>
    /// Remueve un permiso de un rol.
    /// Requiere permiso: admin:manage-roles
    /// </summary>
    /// <param name="request">IDs del rol y permiso</param>
    /// <returns>Resultado de la remoción</returns>
    /// <response code="200">Permiso removido exitosamente</response>
    /// <response code="404">Asignación no existe</response>
    /// <response code="403">No tiene permiso para modificar este rol (rol del sistema)</response>
    [HttpPost("remove")]
    [Authorize(Policy = "ManageRoles")]
    [RateLimit(maxRequests: 100, windowSeconds: 60)]
    [ProducesResponseType(typeof(ApiResponse<RemovePermissionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<RemovePermissionResponse>>> RemovePermission([FromBody] AssignPermissionRequest request)
    {
        _logger.LogInformation("Removing permission {PermissionId} from role {RoleId}",
            request.PermissionId, request.RoleId);

        var command = new RemovePermissionCommand(request.RoleId, request.PermissionId);
        var result = await _mediator.Send(command);

        _logger.LogInformation("Permission {PermissionName} removed from role {RoleName} successfully",
            result.PermissionName, result.RoleName);

        return Ok(ApiResponse<RemovePermissionResponse>.Ok(result));
    }

    /// <summary>
    /// Verifica si un usuario/roles tiene un permiso específico.
    /// Este endpoint es usado por el Gateway y otros servicios para autorización.
    /// Preferible usar RoleIds (del JWT) en lugar de UserId para mejor rendimiento.
    /// </summary>
    /// <remarks>
    /// Ejemplo con RoleIds (preferido - más rápido, evita lookup):
    /// ```json
    /// {
    ///   "roleIds": ["guid1", "guid2"],
    ///   "resource": "vehicles",
    ///   "action": "create"
    /// }
    /// ```
    /// 
    /// El endpoint verifica en cache primero (TTL 5 min) y luego en DB si es necesario.
    /// La respuesta incluye el campo "cached" para indicar si vino del cache.
    /// </remarks>
    /// <param name="request">RoleIds o UserId, resource y action</param>
    /// <returns>Resultado de la verificación</returns>
    /// <response code="200">Verificación completada (hasPermission indica el resultado)</response>
    /// <response code="400">Request inválido</response>
    [HttpPost("check")]
    [Authorize(Policy = "RoleServiceAccess")]
    [RateLimit(maxRequests: 500, windowSeconds: 60)] // Alto rate limit por uso interno frecuente
    [ProducesResponseType(typeof(ApiResponse<CheckPermissionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CheckPermissionResponse>>> CheckPermission([FromBody] CheckPermissionRequest request)
    {
        // Validar request - debe tener RoleIds o UserId
        if ((request.RoleIds == null || !request.RoleIds.Any()) && request.UserId == null)
        {
            throw new BadRequestException(
                "Either RoleIds or UserId must be provided",
                "MISSING_ROLE_OR_USER");
        }

        // Validar resource y action
        if (string.IsNullOrWhiteSpace(request.Resource))
        {
            throw new BadRequestException("Resource is required", "MISSING_RESOURCE");
        }

        if (string.IsNullOrWhiteSpace(request.Action))
        {
            throw new BadRequestException("Action is required", "MISSING_ACTION");
        }

        IEnumerable<Guid> roleIds;

        if (request.RoleIds != null && request.RoleIds.Any())
        {
            // Preferred path: usar RoleIds directamente (del JWT)
            roleIds = request.RoleIds;
        }
        else
        {
            // Legacy path: UserId provided - deprecado
            _logger.LogWarning("Deprecated UserId-based permission check attempted. Use RoleIds instead.");
            throw new BadRequestException(
                "UserId-based permission check is deprecated. " +
                "Please provide RoleIds directly (typically from JWT claims). " +
                "See API documentation for examples.",
                "DEPRECATED_USER_ID_CHECK");
        }

        var query = new CheckPermissionQuery(roleIds, request.Resource, request.Action);
        var result = await _mediator.Send(query);

        _logger.LogDebug("Permission check: {Resource}:{Action} = {HasPermission} (cached: {Cached})",
            request.Resource, request.Action, result.HasPermission, result.Cached);

        return Ok(ApiResponse<CheckPermissionResponse>.Ok(result));
    }
}
