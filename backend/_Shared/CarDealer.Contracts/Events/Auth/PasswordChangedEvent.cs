using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Auth;

/// <summary>
/// Event published when a user successfully changes their password.
/// </summary>
public class PasswordChangedEvent : EventBase
{
    public override string EventType => "auth.password.changed";

    /// <summary>
    /// Unique identifier of the user who changed their password.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Email address of the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the password was changed.
    /// </summary>
    public DateTime ChangedAt { get; set; }

    /// <summary>
    /// IP address from which the password was changed.
    /// </summary>
    public string? IpAddress { get; set; }
}
