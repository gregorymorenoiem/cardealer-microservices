using System;
using System.Collections.Generic;

namespace StaffService.Domain.Entities;

/// <summary>
/// Represents a job position/title.
/// </summary>
public class Position
{
    public Guid Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    /// <summary>
    /// Position code for identification.
    /// </summary>
    public string? Code { get; set; }
    
    /// <summary>
    /// Department this position belongs to.
    /// </summary>
    public Guid? DepartmentId { get; set; }
    public virtual Department? Department { get; set; }
    
    /// <summary>
    /// Default role for this position.
    /// </summary>
    public StaffRole DefaultRole { get; set; } = StaffRole.Support;
    
    /// <summary>
    /// Seniority level (1=Junior, 2=Mid, 3=Senior, 4=Lead, 5=Director).
    /// </summary>
    public int Level { get; set; } = 1;
    
    /// <summary>
    /// Staff members holding this position.
    /// </summary>
    public virtual ICollection<Staff> StaffMembers { get; set; } = new List<Staff>();
    
    public bool IsActive { get; set; } = true;
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
