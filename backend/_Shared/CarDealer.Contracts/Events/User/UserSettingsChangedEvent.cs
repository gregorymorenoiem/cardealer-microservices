using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.User;

/// <summary>
/// Published by UserService when a user updates their communication/notification preferences
/// or app settings (locale, currency, theme). Consumed by NotificationService to create
/// an in-app confirmation notification.
/// </summary>
public class UserSettingsChangedEvent : EventBase
{
    public override string EventType => "user.settings.changed";

    /// <summary>The user who changed their settings.</summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Which settings section was changed: "preferences", "locale", "all".
    /// </summary>
    public string ChangeType { get; set; } = "preferences";
}
