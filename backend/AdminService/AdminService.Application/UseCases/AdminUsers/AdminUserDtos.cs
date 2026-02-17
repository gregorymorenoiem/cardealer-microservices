using MediatR;

namespace AdminService.Application.UseCases.AdminUsers;

// ============================================================================
// DTOs
// ============================================================================

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Role { get; set; } = string.Empty;
    public string[] Permissions { get; set; } = Array.Empty<string>();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? AvatarUrl { get; set; }
}

public class AdminUserDetailDto : AdminUserDto
{
    public string? Phone { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int TotalActionsThisMonth { get; set; }
    public Dictionary<string, int> ActionsByType { get; set; } = new();
    public List<RecentActionDto> RecentActions { get; set; } = new();
}

public class RecentActionDto
{
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class AdminDashboardDto
{
    public DashboardStatsDto Stats { get; set; } = new();
    public List<PendingItemSummaryDto> PendingItems { get; set; } = new();
    public List<RecentActionDto> RecentActivity { get; set; } = new();
    public List<AlertDto> Alerts { get; set; } = new();
}

public class DashboardStatsDto
{
    public int TotalDealers { get; set; }
    public int ActiveDealers { get; set; }
    public int PendingDealers { get; set; }
    public int TotalUsers { get; set; }
    public int TotalListings { get; set; }
    public int PendingListings { get; set; }
    public int TotalTransactions { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int OpenTickets { get; set; }
}

public class PendingItemSummaryDto
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
    public string Priority { get; set; } = "Normal";
    public string Link { get; set; } = string.Empty;
}

public class AlertDto
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "Info";
    public DateTime Timestamp { get; set; }
}

public class PendingItemsDto
{
    public List<PendingDealerDto> PendingDealers { get; set; } = new();
    public List<PendingListingDto> PendingListings { get; set; } = new();
    public List<PendingReportDto> PendingReports { get; set; } = new();
    public List<PendingTicketDto> PendingTickets { get; set; } = new();
}

public class PendingDealerDto
{
    public Guid Id { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public int DaysPending { get; set; }
}

public class PendingListingDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DealerName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}

public class PendingReportDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public DateTime ReportedAt { get; set; }
    public int ReportCount { get; set; }
}

public class PendingTicketDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AdminActivityDto
{
    public Guid Id { get; set; }
    public Guid AdminId { get; set; }
    public string AdminName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? TargetType { get; set; }
    public string? TargetId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
}

public class PlatformSettingsDto
{
    public bool MaintenanceMode { get; set; }
    public string? MaintenanceMessage { get; set; }
    public bool RegistrationEnabled { get; set; }
    public bool DealerRegistrationEnabled { get; set; }
    public bool EarlyBirdActive { get; set; }
    public DateTime? EarlyBirdEndDate { get; set; }
    public decimal EarlyBirdDiscount { get; set; }
    public Dictionary<string, object> FeatureFlags { get; set; } = new();
    public Dictionary<string, object> Limits { get; set; } = new();
}

// ============================================================================
// QUERIES
// ============================================================================

public record GetAdminUsersQuery(
    string? Role,
    bool? IsActive,
    int Page,
    int PageSize
) : IRequest<PaginatedResult<AdminUserDto>>;

public record GetAdminUserQuery(Guid UserId) : IRequest<AdminUserDetailDto?>;

public record GetAdminDashboardQuery() : IRequest<AdminDashboardDto>;

public record GetPendingItemsQuery() : IRequest<PendingItemsDto>;

public record GetAdminActivityQuery(
    Guid? AdminId,
    string? Action,
    DateTime? From,
    DateTime? To,
    int Page,
    int PageSize
) : IRequest<PaginatedResult<AdminActivityDto>>;

public record GetPlatformSettingsQuery() : IRequest<PlatformSettingsDto>;

// ============================================================================
// COMMANDS
// ============================================================================

public record UpdateAdminRoleCommand(
    Guid UserId,
    string Role,
    string[]? Permissions
) : IRequest<Unit>;

public record SuspendAdminUserCommand(
    Guid UserId,
    string? Reason
) : IRequest<Unit>;

public record ReactivateAdminUserCommand(Guid UserId) : IRequest<Unit>;

public record UpdatePlatformSettingsCommand(
    Dictionary<string, object> Settings
) : IRequest<Unit>;

// ============================================================================
// HELPER CLASSES
// ============================================================================

public class PaginatedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
