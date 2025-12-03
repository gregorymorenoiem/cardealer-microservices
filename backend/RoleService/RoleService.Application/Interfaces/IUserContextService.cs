namespace RoleService.Application.Interfaces;

/// <summary>
/// Service for accessing current user context from JWT claims
/// </summary>
public interface IUserContextService
{
    /// <summary>
    /// Gets the current user ID from JWT claims
    /// </summary>
    /// <returns>User ID or "anonymous" if not authenticated</returns>
    string GetCurrentUserId();

    /// <summary>
    /// Gets the current username from JWT claims
    /// </summary>
    /// <returns>Username or "anonymous" if not authenticated</returns>
    string GetCurrentUserName();

    /// <summary>
    /// Gets the current user roles from JWT claims
    /// </summary>
    /// <returns>Collection of role names</returns>
    IEnumerable<string> GetCurrentUserRoles();

    /// <summary>
    /// Checks if the current user is authenticated
    /// </summary>
    /// <returns>True if authenticated, false otherwise</returns>
    bool IsAuthenticated();

    /// <summary>
    /// Gets the current user email from JWT claims
    /// </summary>
    /// <returns>Email or null if not present</returns>
    string? GetCurrentUserEmail();
}
