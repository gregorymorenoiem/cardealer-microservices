using MediatR;
using MessageBusService.Application.Commands;
using MessageBusService.Application.Queries;
using MessageBusService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MessageBusService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeadLetterController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IDeadLetterManager _deadLetterManager;
    private readonly ILogger<DeadLetterController> _logger;

    public DeadLetterController(
        IMediator mediator,
        IDeadLetterManager deadLetterManager,
        ILogger<DeadLetterController> logger)
    {
        _mediator = mediator;
        _deadLetterManager = deadLetterManager;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetDeadLetters([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = new GetDeadLettersQuery { PageNumber = pageNumber, PageSize = pageSize };
            var deadLetters = await _mediator.Send(query);
            return Ok(deadLetters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dead letters");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("{messageId}/retry")]
    public async Task<IActionResult> Retry(Guid messageId)
    {
        try
        {
            var command = new RetryDeadLetterCommand { DeadLetterMessageId = messageId };
            var result = await _mediator.Send(command);
            return result ? Ok(new { success = true, message = "Message retried successfully" })
                         : BadRequest(new { success = false, message = "Failed to retry message" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying dead letter");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpDelete("{messageId}")]
    public async Task<IActionResult> Discard(Guid messageId)
    {
        try
        {
            var result = await _deadLetterManager.DiscardAsync(messageId);
            return result ? Ok(new { success = true, message = "Message discarded successfully" })
                         : NotFound(new { success = false, message = "Message not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discarding dead letter");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("{messageId}")]
    public async Task<IActionResult> GetById(Guid messageId)
    {
        try
        {
            var deadLetter = await _deadLetterManager.GetByIdAsync(messageId);
            return deadLetter != null ? Ok(deadLetter) : NotFound(new { message = "Message not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dead letter");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}
