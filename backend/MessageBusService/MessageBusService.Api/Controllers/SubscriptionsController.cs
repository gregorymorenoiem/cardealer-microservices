using MediatR;
using MessageBusService.Application.Commands;
using MessageBusService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MessageBusService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(
        IMediator mediator,
        IMessageSubscriber messageSubscriber,
        ILogger<SubscriptionsController> logger)
    {
        _mediator = mediator;
        _messageSubscriber = messageSubscriber;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeToTopicCommand command)
    {
        try
        {
            var subscriptionId = await _mediator.Send(command);
            return Ok(new { success = true, subscriptionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpDelete("{subscriptionId}")]
    public async Task<IActionResult> Unsubscribe(Guid subscriptionId)
    {
        try
        {
            var result = await _messageSubscriber.UnsubscribeAsync(subscriptionId);
            return result ? Ok(new { success = true, message = "Unsubscribed successfully" })
                         : NotFound(new { success = false, message = "Subscription not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetSubscriptions([FromQuery] string? topic = null)
    {
        try
        {
            var subscriptions = await _messageSubscriber.GetSubscriptionsAsync(topic);
            return Ok(subscriptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscriptions");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}
