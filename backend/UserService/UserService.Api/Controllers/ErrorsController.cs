using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs;
using UserService.Application.UseCases.LogError;
using UserService.Shared;

namespace UserService.Api.Controllers
{
    [ApiController]
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
