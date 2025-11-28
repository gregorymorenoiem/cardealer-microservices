using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Auth;

/// <summary>
/// Event published when a new user successfully registers in the system.
/// </summary>
public class UserRegisteredEvent : EventBase
{
    public override string EventType => "auth.user.registered";

    /// <summary>
    /// Unique identifier of the registered user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Email address of the registered user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Full name of the registered user.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the user registered.
    /// </summary>
    public DateTime RegisteredAt { get; set; }

    /// <summary>
    /// Optional metadata associated with the registration.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
