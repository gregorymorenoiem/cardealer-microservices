using System;
using System.Collections.Generic;

namespace StaffService.Domain.Entities;

/// <summary>
/// Represents an internal staff member (employee) of OKLA.
/// Synchronized with AuthService for authentication.
/// </summary>
public class Staff
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Reference to the AuthService user ID for authentication sync.
    /// </summary>
    public Guid AuthUserId { get; set; }
    
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    
    /// <summary>
    /// Employee code or badge number.
    /// </summary>
    public string? EmployeeCode { get; set; }
    
    /// <summary>
    /// Department the staff member belongs to.
    /// </summary>
    public Guid? DepartmentId { get; set; }
    public virtual Department? Department { get; set; }
    
    /// <summary>
    /// Job position/title.
    /// </summary>
    public Guid? PositionId { get; set; }
    public virtual Position? Position { get; set; }
    
    /// <summary>
    /// Direct supervisor/manager.
    /// </summary>
    public Guid? SupervisorId { get; set; }
    public virtual Staff? Supervisor { get; set; }
    
    /// <summary>
    /// Staff members reporting to this person.
    /// </summary>
    public virtual ICollection<Staff> DirectReports { get; set; } = new List<Staff>();
    
    /// <summary>
    /// Current employment status.
    /// </summary>
    public StaffStatus Status { get; set; } = StaffStatus.Active;
    
    /// <summary>
    /// Role in the admin system (SuperAdmin, Admin, Moderator, etc.)
    /// </summary>
    public StaffRole Role { get; set; } = StaffRole.Support;
    
    /// <summary>
    /// Additional permissions beyond the role defaults.
    /// </summary>
    public virtual ICollection<StaffPermission> Permissions { get; set; } = new List<StaffPermission>();
    
    /// <summary>
    /// Invitation used to onboard this staff member.
    /// </summary>
    public Guid? InvitationId { get; set; }
    public virtual StaffInvitation? Invitation { get; set; }
    
    /// <summary>
    /// Employment start date.
    /// </summary>
    public DateTime HireDate { get; set; }
    
    /// <summary>
    /// Employment end date (if terminated).
    /// </summary>
    public DateTime? TerminationDate { get; set; }
    
    /// <summary>
    /// Reason for termination.
    /// </summary>
    public string? TerminationReason { get; set; }
    
    /// <summary>
    /// Last login timestamp from AuthService events.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// Two-factor authentication enabled status.
    /// </summary>
    public bool TwoFactorEnabled { get; set; }
    
    /// <summary>
    /// Notes about the staff member.
    /// </summary>
    public string? Notes { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsActive => Status == StaffStatus.Active;
}

/// <summary>
/// Staff employment status.
/// </summary>
public enum StaffStatus
{
    /// <summary>
    /// Pending invitation acceptance.
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Currently employed and active.
    /// </summary>
    Active = 1,
    
    /// <summary>
    /// Temporarily suspended.
    /// </summary>
    Suspended = 2,
    
    /// <summary>
    /// On leave (vacation, sick, etc.)
    /// </summary>
    OnLeave = 3,
    
    /// <summary>
    /// Employment terminated.
    /// </summary>
    Terminated = 4
}

/// <summary>
/// Staff role in the admin system.
/// </summary>
public enum StaffRole
{
    /// <summary>
    /// Full system access, can manage all staff.
    /// </summary>
    SuperAdmin = 0,
    
    /// <summary>
    /// High-level access, can manage most operations.
    /// </summary>
    Admin = 1,
    
    /// <summary>
    /// Regulatory and compliance management.
    /// </summary>
    Compliance = 2,
    
    /// <summary>
    /// Content moderation and user management.
    /// </summary>
    Moderator = 3,
    
    /// <summary>
    /// Customer support access.
    /// </summary>
    Support = 4,
    
    /// <summary>
    /// Read-only analytics access.
    /// </summary>
    Analyst = 5
}
