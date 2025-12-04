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
    /// Verifica si un usuario/roles tiene un permiso específico.
    /// Preferible usar RoleIds (del JWT) en lugar de UserId para mejor rendimiento.
    /// </summary>
    /// <remarks>
    /// Ejemplo con RoleIds (preferido):
    /// ```json
    /// {
    ///   "roleIds": ["guid1", "guid2"],
    ///   "resource": "vehicles",
    ///   "action": "create"
    /// }
    /// ```
    /// 
    /// Ejemplo con UserId (requiere lookup adicional - deprecated):
    /// ```json
    /// {
    ///   "userId": "user-guid",
    ///   "resource": "vehicles",
    ///   "action": "create"
    /// }
    /// ```
    /// </remarks>
    [HttpPost("check")]
    [RateLimit(maxRequests: 200, windowSeconds: 60)]
    public async Task<ActionResult<ApiResponse<CheckPermissionResponse>>> CheckPermission([FromBody] CheckPermissionRequest request)
    {
        // Validate request - must have either RoleIds or UserId
        if ((request.RoleIds == null || !request.RoleIds.Any()) && request.UserId == null)
        {
            throw new BadRequestException("Either RoleIds or UserId must be provided");
        }

        IEnumerable<Guid> roleIds;

        if (request.RoleIds != null && request.RoleIds.Any())
        {
            // Preferred path: use RoleIds directly
            roleIds = request.RoleIds;
        }
        else
        {
            // Legacy path: UserId provided - for now, return error
            // In a full implementation, this would call UserService to get roles
            throw new BadRequestException(
                "UserId-based permission check is deprecated. " +
                "Please provide RoleIds directly (typically from JWT claims). " +
                "See API documentation for examples.");
        }

        var query = new CheckPermissionQuery(roleIds, request.Resource, request.Action);
        var result = await _mediator.Send(query);
        return Ok(ApiResponse<CheckPermissionResponse>.Ok(result));
    }
}
