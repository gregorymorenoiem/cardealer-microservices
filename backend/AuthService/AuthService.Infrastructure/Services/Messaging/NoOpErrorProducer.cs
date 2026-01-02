using AuthService.Shared.ErrorMessages;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Services.Messaging;

/// <summary>
/// No-operation implementation of IErrorEventProducer for when RabbitMQ is disabled.
/// </summary>
public class NoOpErrorProducer : IErrorEventProducer
{
    private readonly ILogger<NoOpErrorProducer> _logger;

    public NoOpErrorProducer(ILogger<NoOpErrorProducer> logger)
    {
        _logger = logger;
        _logger.LogInformation("NoOpErrorProducer initialized - RabbitMQ is disabled");
    }

    public Task PublishErrorAsync(RabbitMQErrorEvent errorEvent)
    {
        _logger.LogDebug("NoOp: Would publish error event {ErrorCode}: {ErrorMessage}",
            errorEvent.ErrorCode, errorEvent.ErrorMessage);
        return Task.CompletedTask;
    }

    public Task PublishErrorAsync(string errorCode, string errorMessage, string? stackTrace = null,
        string? userId = null, Dictionary<string, object>? metadata = null)
    {
        _logger.LogDebug("NoOp: Would publish error {ErrorCode}: {ErrorMessage}",
            errorCode, errorMessage);
        return Task.CompletedTask;
    }
}
