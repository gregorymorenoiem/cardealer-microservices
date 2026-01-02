using AuthService.Domain.Interfaces.Services;
using AuthService.Shared.NotificationMessages;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Services.Messaging;

/// <summary>
/// No-operation implementation of INotificationEventProducer for when RabbitMQ is disabled.
/// </summary>
public class NoOpNotificationProducer : INotificationEventProducer
{
    private readonly ILogger<NoOpNotificationProducer> _logger;

    public NoOpNotificationProducer(ILogger<NoOpNotificationProducer> logger)
    {
        _logger = logger;
        _logger.LogInformation("NoOpNotificationProducer initialized - RabbitMQ is disabled");
    }

    public Task PublishNotificationAsync(NotificationEvent notification)
    {
        _logger.LogDebug("NoOp: Would publish notification to {To}, Type: {Type}",
            notification.To, notification.Type);
        return Task.CompletedTask;
    }

    public Task PublishEmailAsync(string to, string subject, string body, Dictionary<string, object>? data = null)
    {
        _logger.LogDebug("NoOp: Would send email to {To} with subject: {Subject}", to, subject);
        return Task.CompletedTask;
    }

    public Task PublishSmsAsync(string to, string message, Dictionary<string, object>? data = null)
    {
        _logger.LogDebug("NoOp: Would send SMS to {To}: {Message}", to, message);
        return Task.CompletedTask;
    }
}
