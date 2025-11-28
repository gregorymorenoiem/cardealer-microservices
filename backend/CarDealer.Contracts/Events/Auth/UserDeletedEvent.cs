using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Auth;

/// <summary>
/// Event published when a user account is deleted from the system.
/// </summary>
public class UserDeletedEvent : EventBase
{
    public override string EventType => "auth.user.deleted";
    
    /// <summary>
    /// Unique identifier of the deleted user.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Email address of the deleted user.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp when the user was deleted.
    /// </summary>
    public DateTime DeletedAt { get; set; }
    
    /// <summary>
    /// Reason for deletion (optional).
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// User ID of the administrator who performed the deletion (if applicable).
    /// </summary>
    public Guid? DeletedBy { get; set; }
}
