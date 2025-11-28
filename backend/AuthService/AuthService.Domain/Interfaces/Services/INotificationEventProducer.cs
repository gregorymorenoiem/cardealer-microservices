using AuthService.Shared.NotificationMessages;

namespace AuthService.Domain.Interfaces.Services;

public interface INotificationEventProducer
{
    Task PublishNotificationAsync(NotificationEvent notification);
    Task PublishEmailAsync(string to, string subject, string body, Dictionary<string, object>? data = null);
    Task PublishSmsAsync(string to, string message, Dictionary<string, object>? data = null);
}

