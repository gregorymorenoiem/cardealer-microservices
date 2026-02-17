using AdminService.Application.UseCases.PlatformUsers;

namespace AdminService.Application.Interfaces;

/// <summary>
/// Interface for platform user operations
/// This service calls UserService via HTTP to manage platform users
/// </summary>
public interface IPlatformUserService
{
    /// <summary>
    /// Get paginated list of platform users with filtering
    /// </summary>
    Task<PaginatedUserResult> GetUsersAsync(
        string? search = null,
        string? type = null,
        string? status = null,
        bool? verified = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get platform user statistics
    /// </summary>
    Task<PlatformUserStatsDto> GetUserStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a platform user by ID
    /// </summary>
    Task<PlatformUserDetailDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update platform user status
    /// </summary>
    Task UpdateUserStatusAsync(Guid userId, string status, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify a platform user
    /// </summary>
    Task VerifyUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a platform user (soft delete)
    /// </summary>
    Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
