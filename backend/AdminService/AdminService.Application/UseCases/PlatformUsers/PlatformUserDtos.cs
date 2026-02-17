using MediatR;

namespace AdminService.Application.UseCases.PlatformUsers;

// ============================================================================
// DTOs
// ============================================================================

/// <summary>
/// Platform user DTO for list views - matches frontend AdminUser interface
/// </summary>
public class PlatformUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Avatar { get; set; }
    public string Type { get; set; } = "buyer"; // buyer, seller, dealer
    public string UserIntent { get; set; } = "browse"; // browse, buy, sell, buy_and_sell
    public string Status { get; set; } = "active"; // active, suspended, pending, banned
    public bool Verified { get; set; }
    public bool EmailVerified { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string? LastActiveAt { get; set; }
    public int VehiclesCount { get; set; }
    public int FavoritesCount { get; set; }
    public int DealsCount { get; set; }
}

/// <summary>
/// Platform user detail DTO - includes more information
/// </summary>
public class PlatformUserDetailDto : PlatformUserDto
{
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? Country { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Gender { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public List<UserActivityItemDto> RecentActivity { get; set; } = new();
    public List<UserVehicleDto> Vehicles { get; set; } = new();
    public List<UserReportDto> Reports { get; set; } = new();
}

/// <summary>
/// User activity item
/// </summary>
public class UserActivityItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Target { get; set; }
    public string? IpAddress { get; set; }
}

/// <summary>
/// User vehicle summary
/// </summary>
public class UserVehicleDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// User report summary
/// </summary>
public class UserReportDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Platform user statistics DTO - matches frontend getUserStats response
/// </summary>
public class PlatformUserStatsDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Suspended { get; set; }
    public int Pending { get; set; }
    public int NewThisMonth { get; set; }
}

/// <summary>
/// Paginated result for platform users
/// </summary>
public class PaginatedUserResult
{
    public List<PlatformUserDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)Total / PageSize) : 0;
}

// ============================================================================
// QUERIES
// ============================================================================

/// <summary>
/// Query to get platform users with filtering
/// </summary>
public record GetPlatformUsersQuery(
    string? Search,
    string? Type,
    string? Status,
    bool? Verified,
    int Page = 1,
    int PageSize = 20
) : IRequest<PaginatedUserResult>;

/// <summary>
/// Query to get platform user statistics
/// </summary>
public record GetPlatformUserStatsQuery() : IRequest<PlatformUserStatsDto>;

/// <summary>
/// Query to get a single platform user by ID
/// </summary>
public record GetPlatformUserQuery(Guid UserId) : IRequest<PlatformUserDetailDto?>;

// ============================================================================
// COMMANDS
// ============================================================================

/// <summary>
/// Command to update platform user status
/// </summary>
public record UpdatePlatformUserStatusCommand(
    Guid UserId,
    string Status,
    string? Reason = null
) : IRequest<Unit>;

/// <summary>
/// Command to verify a platform user
/// </summary>
public record VerifyPlatformUserCommand(Guid UserId) : IRequest<Unit>;

/// <summary>
/// Command to delete a platform user
/// </summary>
public record DeletePlatformUserCommand(Guid UserId) : IRequest<Unit>;
