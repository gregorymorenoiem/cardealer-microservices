using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoleService.Application.DTOs.RolePermissions;
using RoleService.Application.UseCases.RolePermissions.AssignPermission;
using RoleService.Application.UseCases.RolePermissions.RemovePermission;
using RoleService.Application.UseCases.RolePermissions.CheckPermission;
using RoleService.Shared.Models;
using RoleService.Shared.RateLimiting;

namespace RoleService.Api.Controllers;

[Authorize(Policy = "RoleServiceAccess")]
[ApiController]
[Route("api/role-permissions")]
public class RolePermissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolePermissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Asigna un permiso a un role
    /// </summary>
    [HttpPost("assign")]
    [RateLimit(maxRequests: 100, windowSeconds: 60)]
    public async Task<ActionResult<ApiResponse<bool>>> AssignPermission([FromBody] AssignPermissionRequest request)
    {
        var command = new AssignPermissionCommand(request.RoleId, request.PermissionId);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<bool>.Ok(result));
    }

    /// <summary>
    /// Remueve un permiso de un role
    /// </summary>
    [HttpPost("remove")]
    [RateLimit(maxRequests: 100, windowSeconds: 60)]
    public async Task<ActionResult<ApiResponse<bool>>> RemovePermission([FromBody] AssignPermissionRequest request)
    {
        var command = new RemovePermissionCommand(request.RoleId, request.PermissionId);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<bool>.Ok(result));
    }

    /// <summary>
    /// Verifica si un usuario tiene un permiso específico
    /// </summary>
    [HttpPost("check")]
    [RateLimit(maxRequests: 200, windowSeconds: 60)]
    public async Task<ActionResult<ApiResponse<CheckPermissionResponse>>> CheckPermission([FromBody] CheckPermissionRequest request)
    {
        var query = new CheckPermissionQuery(request.UserId, request.Resource, request.Action);
        var result = await _mediator.Send(query);
        return Ok(ApiResponse<CheckPermissionResponse>.Ok(result));
    }
}
