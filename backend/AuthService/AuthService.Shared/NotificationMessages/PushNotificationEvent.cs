
namespace AuthService.Shared.NotificationMessages;

public class PushNotificationEvent : NotificationEvent
{
    public PushNotificationEvent()
    {
        Type = "Push";
    }

    public string DeviceToken { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Dictionary<string, string> CustomData { get; set; } = new();
}
