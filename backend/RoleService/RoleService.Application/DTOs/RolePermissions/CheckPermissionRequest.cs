namespace RoleService.Application.DTOs.RolePermissions;

/// <summary>
/// Request to check if a user/roles has permission to perform an action on a resource.
/// Either RoleIds OR UserId should be provided (RoleIds is preferred for performance).
/// </summary>
public record CheckPermissionRequest
{
    /// <summary>
    /// List of role IDs to check. Preferred method - typically extracted from JWT claims.
    /// </summary>
    public IEnumerable<Guid>? RoleIds { get; init; }

    /// <summary>
    /// [Deprecated] User ID - requires additional lookup to resolve roles.
    /// Use RoleIds instead when possible.
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// The resource to check permission for (e.g., "vehicles", "users", "orders")
    /// </summary>
    public required string Resource { get; init; }

    /// <summary>
    /// The action to check (e.g., "read", "create", "update", "delete", "manage")
    /// </summary>
    public required string Action { get; init; }
}
