using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using UserService.Application.DTOs.UserRoles;
using UserService.Application.UseCases.UserRoles.AssignRole;
using UserService.Application.UseCases.UserRoles.GetUserRoles;
using UserService.Application.UseCases.UserRoles.RevokeRole;

namespace UserService.Api.Controllers
{
    [Authorize(Policy = "UserServiceAccess")]
    [ApiController]
    [Route("api/users/{userId}/roles")]
    public class UserRolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserRolesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtener todos los roles de un usuario
        /// GET /api/users/{userId}/roles
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<UserRolesResponse>> GetUserRoles(Guid userId)
        {
            var query = new GetUserRolesQuery(userId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Asignar un rol a un usuario
        /// POST /api/users/{userId}/roles
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> AssignRole(
            Guid userId,
            [FromBody] AssignRoleRequest request)
        {
            var command = new AssignRoleToUserCommand(
                userId,
                request.RoleId,
                User.Identity?.Name ?? "system");

            var assignmentId = await _mediator.Send(command);

            return CreatedAtAction(
                nameof(GetUserRoles),
                new { userId },
                new { assignmentId, message = "Role assigned successfully" });
        }

        /// <summary>
        /// Revocar un rol de un usuario
        /// DELETE /api/users/{userId}/roles/{roleId}
        /// </summary>
        [HttpDelete("{roleId}")]
        public async Task<ActionResult> RevokeRole(Guid userId, Guid roleId)
        {
            var command = new RevokeRoleFromUserCommand(
                userId,
                roleId,
                User.Identity?.Name ?? "system");

            await _mediator.Send(command);

            return NoContent();
        }

        /// <summary>
        /// Verificar si un usuario tiene un permiso específico
        /// GET /api/users/{userId}/permissions/check?resource=users&action=create
        /// </summary>
        [HttpGet("~/api/users/{userId}/permissions/check")]
        public async Task<ActionResult<CheckPermissionResponse>> CheckPermission(
            Guid userId,
            [FromQuery] string resource,
            [FromQuery] string action)
        {
            var query = new CheckUserPermissionQuery(userId, resource, action);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }

    public class AssignRoleRequest
    {
        public Guid RoleId { get; set; }
    }

    public class CheckPermissionResponse
    {
        public bool HasPermission { get; set; }
        public List<string> GrantedByRoles { get; set; } = new();
    }
}
