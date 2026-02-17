namespace AdminService.Application.Interfaces;

/// <summary>
/// Client interface for communicating with AuthService
/// Used for admin user management and security operations
/// </summary>
public interface IAuthServiceClient
{
    /// <summary>
    /// Create a new admin user in AuthService
    /// </summary>
    Task<CreateAdminUserResult> CreateAdminUserAsync(CreateAdminUserRequest request);

    /// <summary>
    /// Get security status (default admin exists, number of real admins, etc.)
    /// </summary>
    Task<AuthSecurityStatus> GetSecurityStatusAsync();

    /// <summary>
    /// Delete the default admin account (admin@okla.local)
    /// </summary>
    Task<DeleteAdminResult> DeleteDefaultAdminAsync(Guid requestedBy);
}

/// <summary>
/// Request to create an admin user
/// </summary>
public class CreateAdminUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = "Admin";
}

/// <summary>
/// Result of creating an admin user
/// </summary>
public class CreateAdminUserResult
{
    public bool Success { get; set; }
    public Guid UserId { get; set; }
    public string? AccessToken { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Security status from AuthService
/// </summary>
public class AuthSecurityStatus
{
    public bool DefaultAdminExists { get; set; }
    public int RealSuperAdminCount { get; set; }
    public int TotalAdminCount { get; set; }
}

/// <summary>
/// Result of deleting an admin
/// </summary>
public class DeleteAdminResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
