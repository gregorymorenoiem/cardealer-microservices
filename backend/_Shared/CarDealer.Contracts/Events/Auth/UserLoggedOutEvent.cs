using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Auth;

/// <summary>
/// Event published when a user logs out of the system.
/// </summary>
public class UserLoggedOutEvent : EventBase
{
    public override string EventType => "auth.user.loggedout";

    /// <summary>
    /// Unique identifier of the user who logged out.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Email address of the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the user logged out.
    /// </summary>
    public DateTime LoggedOutAt { get; set; }
}
