using MediatR;
using RoleService.Application.DTOs.RolePermissions;

namespace RoleService.Application.UseCases.RolePermissions.CheckPermission;

/// <summary>
/// Query to check if any of the given roles has permission to perform an action on a resource.
/// </summary>
/// <param name="RoleIds">List of role IDs to check (typically from user's JWT claims)</param>
/// <param name="Resource">The resource to check permission for (e.g., "vehicles", "users")</param>
/// <param name="Action">The action to check (e.g., "read", "create", "update", "delete")</param>
public record CheckPermissionQuery(IEnumerable<Guid> RoleIds, string Resource, string Action) : IRequest<CheckPermissionResponse>;

/// <summary>
/// Legacy query that uses UserId - requires integration with UserService to resolve roles.
/// Consider using CheckPermissionQuery with RoleIds instead for better performance.
/// </summary>
[Obsolete("Use CheckPermissionQuery with RoleIds for better performance. This requires UserService integration.")]
public record CheckPermissionByUserQuery(Guid UserId, string Resource, string Action) : IRequest<CheckPermissionResponse>;
