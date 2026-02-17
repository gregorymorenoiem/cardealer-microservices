using MediatR;

namespace AdminService.Application.UseCases.Dealers;

// ============================================================================
// DTOs - Match frontend AdminDealer interface
// ============================================================================

/// <summary>
/// Dealer DTO for list views - matches frontend AdminDealer interface
/// </summary>
public class AdminDealerDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Status { get; set; } = "pending"; // active, pending, suspended, rejected
    public bool Verified { get; set; }
    public string Plan { get; set; } = "none"; // starter, pro, enterprise, none
    public int VehiclesCount { get; set; }
    public int SalesCount { get; set; }
    public double Rating { get; set; }
    public int ReviewsCount { get; set; }
    public string Location { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public decimal Mrr { get; set; }
    public int DocumentsCount { get; set; }
    public int PendingDocuments { get; set; }
}

/// <summary>
/// Dealer statistics - matches frontend getDealerStats response
/// </summary>
public class DealerStatsDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Pending { get; set; }
    public int Suspended { get; set; }
    public decimal TotalMrr { get; set; }
    public DealerPlanBreakdown ByPlan { get; set; } = new();
}

public class DealerPlanBreakdown
{
    public int Starter { get; set; }
    public int Pro { get; set; }
    public int Enterprise { get; set; }
}

/// <summary>
/// Paginated result for dealers
/// </summary>
public class PaginatedDealerResult
{
    public List<AdminDealerDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)Total / PageSize) : 0;
}

// ============================================================================
// QUERIES
// ============================================================================

public record GetDealersQuery(
    string? Search,
    string? Status,
    string? Plan,
    bool? Verified,
    int Page = 1,
    int PageSize = 10
) : IRequest<PaginatedDealerResult>;

public record GetDealerStatsQuery() : IRequest<DealerStatsDto>;

public record GetDealerByIdQuery(Guid DealerId) : IRequest<AdminDealerDto?>;

// ============================================================================
// COMMANDS
// ============================================================================

public record VerifyDealerCommand(Guid DealerId) : IRequest<Unit>;

public record SuspendDealerCommand(Guid DealerId, string Reason) : IRequest<Unit>;

public record ReactivateDealerCommand(Guid DealerId) : IRequest<Unit>;

public record DeleteDealerCommand(Guid DealerId) : IRequest<Unit>;
