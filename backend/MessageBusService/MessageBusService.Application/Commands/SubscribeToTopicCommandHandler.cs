using MediatR;
using MessageBusService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MessageBusService.Application.Commands;

public class SubscribeToTopicCommandHandler : IRequestHandler<SubscribeToTopicCommand, Guid>
{
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly ILogger<SubscribeToTopicCommandHandler> _logger;

    public SubscribeToTopicCommandHandler(
        IMessageSubscriber messageSubscriber,
        ILogger<SubscribeToTopicCommandHandler> logger)
    {
        _messageSubscriber = messageSubscriber;
        _logger = logger;
    }

    public async Task<Guid> Handle(SubscribeToTopicCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating subscription for topic: {Topic}, consumer: {Consumer}", 
                request.Topic, request.ConsumerName);

            var subscription = await _messageSubscriber.SubscribeAsync(request.Topic, request.ConsumerName);

            _logger.LogInformation("Subscription created: {SubscriptionId}", subscription.Id);

            return subscription.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription for topic: {Topic}", request.Topic);
            throw;
        }
    }
}
