using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Notification;

/// <summary>
/// Event published when notification delivery fails.
/// </summary>
public class NotificationFailedEvent : EventBase
{
    public override string EventType => "notification.failed";

    public Guid NotificationId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }
}
