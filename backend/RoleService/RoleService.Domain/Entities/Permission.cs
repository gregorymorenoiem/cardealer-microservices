using RoleService.Domain.Enums;

namespace RoleService.Domain.Entities;

public class Permission
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public PermissionAction Action { get; set; }
    public string Module { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsSystemPermission { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
