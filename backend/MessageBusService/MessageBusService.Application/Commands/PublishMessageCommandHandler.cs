using MediatR;
using MessageBusService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MessageBusService.Application.Commands;

public class PublishMessageCommandHandler : IRequestHandler<PublishMessageCommand, bool>
{
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<PublishMessageCommandHandler> _logger;

    public PublishMessageCommandHandler(
        IMessagePublisher messagePublisher,
        ILogger<PublishMessageCommandHandler> logger)
    {
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task<bool> Handle(PublishMessageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Publishing message to topic: {Topic}", request.Topic);
            
            var result = await _messagePublisher.PublishAsync(
                request.Topic,
                request.Payload,
                request.Priority,
                request.Headers);

            if (result)
            {
                _logger.LogInformation("Message published successfully to topic: {Topic}", request.Topic);
            }
            else
            {
                _logger.LogWarning("Failed to publish message to topic: {Topic}", request.Topic);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to topic: {Topic}", request.Topic);
            throw;
        }
    }
}
