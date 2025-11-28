using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Interfaces;

public interface IPushNotificationService
{
    // TODO: Uncomment when PushNotificationEvent is migrated to CarDealer.Contracts
    // Task<bool> SendPushNotificationAsync(PushNotificationEvent pushEvent);

    Task<bool> SendToDeviceAsync(string deviceToken, string title, string message, Dictionary<string, string>? customData = null);
    Task<bool> SendToTopicAsync(string topic, string title, string message, Dictionary<string, string>? customData = null);
    Task<bool> SubscribeToTopicAsync(string deviceToken, string topic);
    Task<bool> UnsubscribeFromTopicAsync(string deviceToken, string topic);
}