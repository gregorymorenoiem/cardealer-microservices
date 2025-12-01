using MediatR;
using MessageBusService.Application.Commands;
using MessageBusService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MessageBusService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(
        IMediator mediator,
        IMessagePublisher messagePublisher,
        ILogger<MessagesController> logger)
    {
        _mediator = mediator;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> PublishMessage([FromBody] PublishMessageCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return result ? Ok(new { success = true, message = "Message published successfully" }) 
                         : BadRequest(new { success = false, message = "Failed to publish message" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("batch")]
    public async Task<IActionResult> PublishBatch([FromBody] PublishBatchRequest request)
    {
        try
        {
            var result = await _messagePublisher.PublishBatchAsync(request.Topic, request.Payloads, request.Priority);
            return result ? Ok(new { success = true, message = $"{request.Payloads.Count} messages published" })
                         : BadRequest(new { success = false, message = "Failed to publish batch" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing batch");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("{messageId}")]
    public async Task<IActionResult> GetMessageStatus(Guid messageId)
    {
        try
        {
            var message = await _messagePublisher.GetMessageStatusAsync(messageId);
            return message != null ? Ok(message) : NotFound(new { message = "Message not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting message status");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

public class PublishBatchRequest
{
    public string Topic { get; set; } = string.Empty;
    public List<string> Payloads { get; set; } = new();
    public Domain.Enums.MessagePriority Priority { get; set; }
}
