using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoleService.Application.DTOs;
using RoleService.Application.UseCases.LogError;
using RoleService.Shared;
using RoleService.Shared.Models;

namespace RoleService.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ErrorsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ErrorsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("log")]
        public async Task<ActionResult<ApiResponse<LogErrorResponse>>> LogError([FromBody] LogErrorRequest request)
        {
            var command = new LogErrorCommand(request);
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<LogErrorResponse>.Ok(result));
        }
    }
}
