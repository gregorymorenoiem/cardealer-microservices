using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Notification;

/// <summary>
/// Event published when a push notification is requested.
/// This event is consumed by NotificationService to send the push notification.
/// </summary>
public class PushNotificationRequestedEvent : EventBase
{
    public override string EventType => "notification.push.requested";

    /// <summary>
    /// Device token to send push notification to
    /// </summary>
    public string DeviceToken { get; set; } = string.Empty;

    /// <summary>
    /// Push notification title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Push notification body
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Optional data payload for the push notification
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }

    /// <summary>
    /// Optional image URL for rich notifications
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Optional action to perform when notification is tapped
    /// </summary>
    public string? ClickAction { get; set; }
}
