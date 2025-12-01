using Microsoft.AspNetCore.Mvc;
using MediatR;
using System;
using System.Threading.Tasks;
using UserService.Application.DTOs;
using UserService.Application.UseCases.Users.CreateUser;
using UserService.Application.UseCases.Users.UpdateUser;
using UserService.Application.UseCases.Users.DeleteUser;
using UserService.Application.UseCases.Users.GetUser;
using UserService.Application.UseCases.Users.GetUsers;

namespace UserService.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Listar usuarios con paginación
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedUsersResponse>> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetUsersQuery(page, pageSize);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Obtener usuario por ID
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<ActionResult<UserDto>> GetUser(Guid userId)
        {
            var query = new GetUserQuery(userId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Crear nuevo usuario
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var command = new CreateUserCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.PhoneNumber
            );

            var userId = await _mediator.Send(command);

            return CreatedAtAction(
                nameof(GetUser),
                new { userId },
                new { id = userId, message = "User created successfully" }
            );
        }

        /// <summary>
        /// Actualizar usuario
        /// </summary>
        [HttpPut("{userId}")]
        public async Task<ActionResult> UpdateUser(
            Guid userId,
            [FromBody] UpdateUserRequest request)
        {
            var command = new UpdateUserCommand(
                userId,
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                request.IsActive
            );

            await _mediator.Send(command);

            return NoContent();
        }

        /// <summary>
        /// Eliminar usuario
        /// </summary>
        [HttpDelete("{userId}")]
        public async Task<ActionResult> DeleteUser(Guid userId)
        {
            var command = new DeleteUserCommand(userId);
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
