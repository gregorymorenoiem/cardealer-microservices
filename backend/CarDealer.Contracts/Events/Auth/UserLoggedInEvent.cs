using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Auth;

/// <summary>
/// Event published when a user successfully logs into the system.
/// </summary>
public class UserLoggedInEvent : EventBase
{
    public override string EventType => "auth.user.loggedin";
    
    /// <summary>
    /// Unique identifier of the user who logged in.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Email address of the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// IP address from which the user logged in.
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent string of the client.
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Timestamp when the user logged in.
    /// </summary>
    public DateTime LoggedInAt { get; set; }
}
