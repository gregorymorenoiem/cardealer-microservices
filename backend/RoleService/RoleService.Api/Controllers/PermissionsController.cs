using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoleService.Application.DTOs.Permissions;
using RoleService.Application.UseCases.Permissions.CreatePermission;
using RoleService.Application.UseCases.Permissions.GetPermissions;
using RoleService.Shared.Models;
using RoleService.Shared.RateLimiting;

namespace RoleService.Api.Controllers;

[Authorize(Policy = "RoleServiceAccess")]
[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PermissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Crea un nuevo permiso
    /// </summary>
    [HttpPost]
    [RateLimit(maxRequests: 100, windowSeconds: 60)]
    public async Task<ActionResult<ApiResponse<CreatePermissionResponse>>> CreatePermission([FromBody] CreatePermissionRequest request)
    {
        var command = new CreatePermissionCommand(request);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<CreatePermissionResponse>.Ok(result));
    }

    /// <summary>
    /// Obtiene todos los permisos con filtros opcionales
    /// </summary>
    [HttpGet]
    [RateLimit(maxRequests: 150, windowSeconds: 60)]
    public async Task<ActionResult<ApiResponse<List<PermissionDetailsDto>>>> GetPermissions(
        [FromQuery] string? module = null,
        [FromQuery] string? resource = null)
    {
        var query = new GetPermissionsQuery(module, resource);
        var result = await _mediator.Send(query);
        return Ok(ApiResponse<List<PermissionDetailsDto>>.Ok(result));
    }
}
