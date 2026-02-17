using System;

namespace StaffService.Domain.Entities;

/// <summary>
/// Represents a specific permission granted to a staff member.
/// </summary>
public class StaffPermission
{
    public Guid Id { get; set; }
    
    public Guid StaffId { get; set; }
    public virtual Staff Staff { get; set; } = null!;
    
    /// <summary>
    /// Permission code (e.g., "users.view", "vehicles.approve").
    /// </summary>
    public string Permission { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this is an additional grant (+) or explicit deny (-).
    /// </summary>
    public bool IsGranted { get; set; } = true;
    
    /// <summary>
    /// Reason for granting/denying this permission.
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// When this permission expires (null = never).
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Who granted this permission.
    /// </summary>
    public Guid GrantedBy { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Computed
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    public bool IsActive => IsGranted && !IsExpired;
}
