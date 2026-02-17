using System.Threading;
using System.Threading.Tasks;

namespace StaffService.Application.Clients;

/// <summary>
/// Client for communicating with AuthService for staff authentication management.
/// </summary>
public interface IAuthServiceClient
{
    /// <summary>
    /// Creates an admin user in AuthService with the specified role.
    /// </summary>
    Task<CreateAuthUserResult> CreateStaffUserAsync(CreateStaffUserRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// Disables a user account in AuthService.
    /// </summary>
    Task<bool> DisableUserAsync(Guid authUserId, CancellationToken ct = default);
    
    /// <summary>
    /// Re-enables a previously disabled user account.
    /// </summary>
    Task<bool> EnableUserAsync(Guid authUserId, CancellationToken ct = default);
    
    /// <summary>
    /// Deletes a user account from AuthService (hard delete).
    /// </summary>
    Task<bool> DeleteUserAsync(Guid authUserId, CancellationToken ct = default);
    
    /// <summary>
    /// Gets security status (for checking if default admin exists).
    /// </summary>
    Task<SecurityStatusResult> GetSecurityStatusAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Deletes the default admin account after a real admin is created.
    /// </summary>
    Task<bool> DeleteDefaultAdminAsync(CancellationToken ct = default);
}

public record CreateStaffUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string Role // "SuperAdmin", "Admin", "Moderator", etc.
);

public record CreateAuthUserResult(
    bool Success,
    Guid? UserId,
    string? Error
);

public record SecurityStatusResult(
    bool DefaultAdminExists,
    string? DefaultAdminEmail,
    int TotalAdminUsers
);
