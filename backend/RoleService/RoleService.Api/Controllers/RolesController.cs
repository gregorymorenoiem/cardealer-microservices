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

namespace RoleService.Api.Controllers;

[Authorize(Policy = "RoleServiceAccess")]
[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Crea un nuevo role
    /// </summary>
    [HttpPost]
    [RateLimit(maxRequests: 100, windowSeconds: 60)]
    public async Task<ActionResult<ApiResponse<CreateRoleResponse>>> CreateRole([FromBody] CreateRoleRequest request)
    {
        var command = new CreateRoleCommand(request);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<CreateRoleResponse>.Ok(result));
    }

    /// <summary>
    /// Obtiene todos los roles con paginación
    /// </summary>
    [HttpGet]
    [RateLimit(maxRequests: 150, windowSeconds: 60)]
    public async Task<ActionResult<ApiResponse<PaginatedResult<RoleListItemDto>>>> GetRoles(
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = new GetRolesQuery(isActive, page, pageSize);
        var result = await _mediator.Send(query);
        return Ok(ApiResponse<PaginatedResult<RoleListItemDto>>.Ok(result));
    }

    /// <summary>
    /// Obtiene un role por ID con sus permisos
    /// </summary>
    [HttpGet("{id:guid}")]
    [RateLimit(maxRequests: 200, windowSeconds: 60)]
    public async Task<ActionResult<ApiResponse<RoleDetailsDto>>> GetRole(Guid id)
    {
        var query = new GetRoleByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(ApiResponse<object>.Fail("Role not found"));

        return Ok(ApiResponse<RoleDetailsDto>.Ok(result));
    }

    /// <summary>
    /// Actualiza un role existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [RateLimit(maxRequests: 100, windowSeconds: 60)]
    public async Task<ActionResult<ApiResponse<UpdateRoleResponse>>> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request)
    {
        var command = new UpdateRoleCommand(id, request);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<UpdateRoleResponse>.Ok(result));
    }

    /// <summary>
    /// Elimina un role
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RateLimit(maxRequests: 50, windowSeconds: 60)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRole(Guid id)
    {
        var command = new DeleteRoleCommand(id);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<bool>.Ok(result));
    }
}
