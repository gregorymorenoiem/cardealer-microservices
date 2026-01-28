using MediatR;

namespace AdminService.Application.UseCases.PlatformEmployees;

// ============================================================================
// DTOs
// ============================================================================

public class PlatformEmployeeDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Role { get; set; } = string.Empty;
    public string[] Permissions { get; set; } = Array.Empty<string>();
    public string? Department { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime HireDate { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public string? AvatarUrl { get; set; }
}

public class PlatformEmployeeDetailDto : PlatformEmployeeDto
{
    public string? Notes { get; set; }
    public Guid AssignedBy { get; set; }
    public string AssignedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int TotalActionsThisMonth { get; set; }
    public Dictionary<string, int> ActionsByType { get; set; } = new();
}

public class PlatformEmployeeInvitationDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string[] Permissions { get; set; } = Array.Empty<string>();
    public string? Department { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime InvitationDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string InvitedByName { get; set; } = string.Empty;
}

public class EmployeeActivityDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? TargetType { get; set; }
    public string? TargetId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
}

// ============================================================================
// QUERIES
// ============================================================================

public record GetPlatformEmployeesQuery(
    string? Status,
    string? Role,
    string? Department,
    int Page,
    int PageSize
) : IRequest<PaginatedResult<PlatformEmployeeDto>>;

public record GetPlatformEmployeeQuery(Guid EmployeeId) : IRequest<PlatformEmployeeDetailDto?>;

public record GetPlatformInvitationsQuery(string? Status) : IRequest<IEnumerable<PlatformEmployeeInvitationDto>>;

public record GetPlatformEmployeeActivityQuery(
    Guid EmployeeId,
    DateTime? From,
    DateTime? To,
    int Page,
    int PageSize
) : IRequest<PaginatedResult<EmployeeActivityDto>>;

// ============================================================================
// COMMANDS
// ============================================================================

public record InvitePlatformEmployeeCommand(
    string Email,
    string Role,
    string[]? Permissions,
    string? Department,
    string? Notes,
    Guid InvitedBy
) : IRequest<PlatformEmployeeInvitationDto>;

public record UpdatePlatformEmployeeCommand(
    Guid EmployeeId,
    string? Role,
    string[]? Permissions,
    string? Department,
    string? Notes,
    string? Status
) : IRequest<Unit>;

public record SuspendPlatformEmployeeCommand(
    Guid EmployeeId,
    string? Reason
) : IRequest<Unit>;

public record ReactivatePlatformEmployeeCommand(Guid EmployeeId) : IRequest<Unit>;

public record RemovePlatformEmployeeCommand(Guid EmployeeId) : IRequest<Unit>;

public record CancelPlatformInvitationCommand(Guid InvitationId) : IRequest<Unit>;

public record ResendPlatformInvitationCommand(
    Guid InvitationId,
    Guid ResendBy
) : IRequest<PlatformEmployeeInvitationDto>;

// ============================================================================
// HELPER CLASSES (duplicated for AdminService self-containment)
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
