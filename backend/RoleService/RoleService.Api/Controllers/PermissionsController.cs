using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoleService.Application.DTOs.Permissions;
using RoleService.Application.UseCases.Permissions.CreatePermission;
using RoleService.Application.UseCases.Permissions.GetPermissions;
using RoleService.Shared.Models;
using RoleService.Shared.RateLimiting;
using RoleService.Domain.Entities;

namespace RoleService.Api.Controllers;

/// <summary>
/// Controller para gestión de permisos del sistema RBAC.
/// Los permisos siguen el formato: resource:action (ej: "vehicles:create")
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PermissionsController> _logger;

    public PermissionsController(IMediator mediator, ILogger<PermissionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Crea un nuevo permiso en el sistema.
    /// Requiere permiso: admin:manage-permissions (Solo SuperAdmin)
    /// </summary>
    /// <param name="request">Datos del permiso a crear</param>
    /// <returns>El permiso creado con su ID</returns>
    /// <response code="200">Permiso creado exitosamente</response>
    /// <response code="400">Datos inválidos o módulo no permitido</response>
    /// <response code="409">Ya existe un permiso con ese nombre</response>
    /// <response code="403">Sin permisos para crear permisos</response>
    [HttpPost]
    [Authorize(Policy = "ManagePermissions")]
    [RateLimit(maxRequests: 100, windowSeconds: 60)]
    [ProducesResponseType(typeof(ApiResponse<CreatePermissionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<CreatePermissionResponse>>> CreatePermission([FromBody] CreatePermissionRequest request)
    {
        // Validar que el módulo es permitido
        if (!Permission.IsValidModule(request.Module))
        {
            _logger.LogWarning("Invalid module attempted: {Module}. Allowed: {AllowedModules}",
                request.Module, string.Join(", ", Permission.AllowedModules));
            return BadRequest(ApiResponse<object>.Fail(
                $"Invalid module '{request.Module}'. Allowed modules: {string.Join(", ", Permission.AllowedModules)}",
                "INVALID_MODULE"));
        }

        _logger.LogInformation("Creating permission: {PermissionName} in module {Module}",
            request.Name, request.Module);

        var command = new CreatePermissionCommand(request);
        var result = await _mediator.Send(command);

        _logger.LogInformation("Permission created successfully: {PermissionId}", result.Data.Id);
        return Ok(ApiResponse<CreatePermissionResponse>.Ok(result));
    }

    /// <summary>
    /// Obtiene todos los permisos del sistema con filtros opcionales.
    /// </summary>
    /// <param name="module">Filtrar por módulo (auth, users, vehicles, etc.)</param>
    /// <param name="resource">Filtrar por recurso (vehicles, users, dealers, etc.)</param>
    /// <returns>Lista de permisos</returns>
    [HttpGet]
    [Authorize(Policy = "RoleServiceAccess")]
    [RateLimit(maxRequests: 150, windowSeconds: 60)]
    [ProducesResponseType(typeof(ApiResponse<List<PermissionListItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PermissionListItemDto>>>> GetPermissions(
        [FromQuery] string? module = null,
        [FromQuery] string? resource = null)
    {
        var query = new GetPermissionsQuery(module, resource);
        var result = await _mediator.Send(query);
        return Ok(ApiResponse<List<PermissionListItemDto>>.Ok(result));
    }

    /// <summary>
    /// Obtiene la lista de módulos permitidos en el sistema.
    /// </summary>
    /// <returns>Lista de módulos permitidos</returns>
    [HttpGet("modules")]
    [Authorize(Policy = "RoleServiceAccess")]
    [RateLimit(maxRequests: 200, windowSeconds: 60)]
    [ProducesResponseType(typeof(ApiResponse<string[]>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<string[]>> GetAllowedModules()
    {
        return Ok(ApiResponse<string[]>.Ok(Permission.AllowedModules));
    }
}
