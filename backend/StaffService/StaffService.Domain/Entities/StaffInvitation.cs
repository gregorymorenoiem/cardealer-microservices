using System;

namespace StaffService.Domain.Entities;

/// <summary>
/// Represents an invitation to join the staff.
/// </summary>
public class StaffInvitation
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Email address the invitation was sent to.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Secure token for invitation validation.
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// Role assigned upon acceptance.
    /// </summary>
    public StaffRole AssignedRole { get; set; }
    
    /// <summary>
    /// Department to assign upon acceptance.
    /// </summary>
    public Guid? DepartmentId { get; set; }
    public virtual Department? Department { get; set; }
    
    /// <summary>
    /// Position to assign upon acceptance.
    /// </summary>
    public Guid? PositionId { get; set; }
    public virtual Position? Position { get; set; }
    
    /// <summary>
    /// Supervisor to assign upon acceptance.
    /// </summary>
    public Guid? SupervisorId { get; set; }
    public virtual Staff? Supervisor { get; set; }
    
    /// <summary>
    /// Current status of the invitation.
    /// </summary>
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    
    /// <summary>
    /// When the invitation expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// When the invitation was accepted.
    /// </summary>
    public DateTime? AcceptedAt { get; set; }
    
    /// <summary>
    /// Staff member or admin user who sent the invitation.
    /// Can be null if the inviter is a system admin not in the staff table.
    /// </summary>
    public Guid? InvitedBy { get; set; }
    public virtual Staff? InvitedByStaff { get; set; }
    
    /// <summary>
    /// Staff member created from this invitation.
    /// </summary>
    public Guid? StaffId { get; set; }
    public virtual Staff? Staff { get; set; }
    
    /// <summary>
    /// Personalized message included in invitation email.
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// Number of times the invitation email was sent.
    /// </summary>
    public int EmailSentCount { get; set; }
    
    /// <summary>
    /// Last time the invitation email was sent.
    /// </summary>
    public DateTime? LastEmailSentAt { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Computed properties
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsValid => Status == InvitationStatus.Pending && !IsExpired;
}

/// <summary>
/// Status of a staff invitation.
/// </summary>
public enum InvitationStatus
{
    /// <summary>
    /// Invitation sent, awaiting acceptance.
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Invitation accepted, staff created.
    /// </summary>
    Accepted = 1,
    
    /// <summary>
    /// Invitation expired without acceptance.
    /// </summary>
    Expired = 2,
    
    /// <summary>
    /// Invitation revoked by admin.
    /// </summary>
    Revoked = 3
}
