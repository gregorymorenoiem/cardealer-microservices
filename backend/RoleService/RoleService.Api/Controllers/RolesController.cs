using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoleService.Application.DTOs.Roles;
using RoleService.Application.UseCases.Roles.CreateRole;
using RoleService.Application.UseCases.Roles.UpdateRole;
using RoleService.Application.UseCases.Roles.DeleteRole;
using RoleService.Application.UseCases.Roles.GetRole;
using RoleService.Application.UseCases.Roles.GetRoles;
using RoleService.Shared.Models;
using RoleService.Shared.RateLimiting;
using RoleService.Shared.Exceptions;

namespace RoleService.Api.Controllers;

/// <summary>
/// Controller para gestión de roles del sistema RBAC.
/// Los roles del sistema (SuperAdmin, Admin) son inmutables.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IMediator mediator, ILogger<RolesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Crea un nuevo rol en el sistema.
    /// Requiere permiso: admin:manage-roles
    /// </summary>
    /// <param name="request">Datos del rol a crear</param>
    /// <returns>El rol creado con su ID</returns>
    /// <response code="200">Rol creado exitosamente</response>
    /// <response code="400">Datos inválidos</response>
    /// <response code="409">Ya existe un rol con ese nombre</response>
    /// <response code="403">Sin permisos para crear roles</response>
    [HttpPost]
    [Authorize(Policy = "ManageRoles")]
    [RateLimit(maxRequests: 100, windowSeconds: 60)]
    [ProducesResponseType(typeof(ApiResponse<CreateRoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<CreateRoleResponse>>> CreateRole([FromBody] CreateRoleRequest request)
    {
        _logger.LogInformation("Creating role: {RoleName}", request.Name);
        
        var command = new CreateRoleCommand(request);
        var result = await _mediator.Send(command);
        
        _logger.LogInformation("Role created successfully: {RoleId}", result.Data.Id);
        return Ok(ApiResponse<CreateRoleResponse>.Ok(result));
    }

    /// <summary>
    /// Obtiene todos los roles con paginación.
    /// </summary>
    /// <param name="isActive">Filtrar por estado activo/inactivo</param>
    /// <param name="page">Número de página (default: 1)</param>
    /// <param name="pageSize">Tamaño de página (default: 50, max: 100)</param>
    /// <returns>Lista paginada de roles</returns>
    [HttpGet]
    [Authorize(Policy = "RoleServiceAccess")]
    [RateLimit(maxRequests: 150, windowSeconds: 60)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<RoleListItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResult<RoleListItemDto>>>> GetRoles(
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        // Validar paginación
        if (pageSize > 100)
            pageSize = 100;
        if (page < 1)
            page = 1;
            
        var query = new GetRolesQuery(isActive, page, pageSize);
        var result = await _mediator.Send(query);
        return Ok(ApiResponse<PaginatedResult<RoleListItemDto>>.Ok(result));
    }

    /// <summary>
    /// Obtiene un rol por ID con todos sus permisos asignados.
    /// </summary>
    /// <param name="id">ID del rol</param>
    /// <returns>Detalle del rol con permisos</returns>
    /// <response code="200">Rol encontrado</response>
    /// <response code="404">Rol no existe</response>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "RoleServiceAccess")]
    [RateLimit(maxRequests: 200, windowSeconds: 60)]
    [ProducesResponseType(typeof(ApiResponse<RoleDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RoleDetailsDto>>> GetRole(Guid id)
    {
        var query = new GetRoleByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            _logger.LogWarning("Role not found: {RoleId}", id);
            throw new NotFoundException($"Role with ID '{id}' not found", "ROLE_NOT_FOUND");
        }

        return Ok(ApiResponse<RoleDetailsDto>.Ok(result));
    }

    /// <summary>
    /// Actualiza un rol existente.
    /// El campo Name es inmutable y no puede ser cambiado.
    /// Los roles del sistema (SuperAdmin, Admin) no pueden ser modificados.
    /// Requiere permiso: admin:manage-roles
    /// </summary>
    /// <param name="id">ID del rol a actualizar</param>
    /// <param name="request">Datos a actualizar</param>
    /// <returns>Rol actualizado</returns>
    /// <response code="200">Rol actualizado exitosamente</response>
    /// <response code="400">No se puede modificar rol del sistema</response>
    /// <response code="404">Rol no existe</response>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ManageRoles")]
    [RateLimit(maxRequests: 100, windowSeconds: 60)]
    [ProducesResponseType(typeof(ApiResponse<UpdateRoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UpdateRoleResponse>>> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request)
    {
        _logger.LogInformation("Updating role: {RoleId}", id);
        
        var command = new UpdateRoleCommand(id, request);
        var result = await _mediator.Send(command);
        
        _logger.LogInformation("Role updated successfully: {RoleId}", id);
        return Ok(ApiResponse<UpdateRoleResponse>.Ok(result));
    }

    /// <summary>
    /// Elimina un rol del sistema.
    /// Los roles del sistema (SuperAdmin, Admin) no pueden ser eliminados.
    /// Los roles con usuarios asignados no pueden ser eliminados.
    /// Requiere permiso: admin:manage-roles
    /// </summary>
    /// <param name="id">ID del rol a eliminar</param>
    /// <returns>Resultado de la eliminación</returns>
    /// <response code="200">Rol eliminado exitosamente</response>
    /// <response code="400">Rol del sistema o tiene usuarios asignados</response>
    /// <response code="404">Rol no existe</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ManageRoles")]
    [RateLimit(maxRequests: 50, windowSeconds: 60)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRole(Guid id)
    {
        _logger.LogInformation("Deleting role: {RoleId}", id);
        
        var command = new DeleteRoleCommand(id);
        var result = await _mediator.Send(command);
        
        _logger.LogInformation("Role deleted successfully: {RoleId}", id);
        return Ok(ApiResponse<bool>.Ok(result));
    }
}
