using System;
using System.Collections.Generic;

namespace StaffService.Domain.Entities;

/// <summary>
/// Represents an organizational department.
/// </summary>
public class Department
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    /// <summary>
    /// Department code for identification.
    /// </summary>
    public string? Code { get; set; }
    
    /// <summary>
    /// Parent department for hierarchical structure.
    /// </summary>
    public Guid? ParentDepartmentId { get; set; }
    public virtual Department? ParentDepartment { get; set; }
    
    /// <summary>
    /// Child departments.
    /// </summary>
    public virtual ICollection<Department> ChildDepartments { get; set; } = new List<Department>();
    
    /// <summary>
    /// Department head/manager.
    /// </summary>
    public Guid? HeadId { get; set; }
    public virtual Staff? Head { get; set; }
    
    /// <summary>
    /// Staff members in this department.
    /// </summary>
    public virtual ICollection<Staff> StaffMembers { get; set; } = new List<Staff>();
    
    /// <summary>
    /// Positions available in this department.
    /// </summary>
    public virtual ICollection<Position> Positions { get; set; } = new List<Position>();
    
    public bool IsActive { get; set; } = true;
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
