namespace ContactService.Application.DTOs;

/// <summary>
/// DTO for a single overage conversation detail line.
/// Used in the downloadable overage report.
///
/// CONTRA #5 / OVERAGE BILLING FIX
/// </summary>
public record ConversationOverageDetailDto
{
    public Guid Id { get; init; }
    public Guid ContactRequestId { get; init; }
    public Guid BuyerId { get; init; }
    public Guid? VehicleId { get; init; }
    public string Subject { get; init; } = string.Empty;
    public int ConversationNumber { get; init; }
    public decimal UnitCost { get; init; }
    public DateTime OccurredAtUtc { get; init; }
}

/// <summary>
/// Summary DTO for the overage report header.
/// </summary>
public record ConversationOverageReportDto
{
    public Guid DealerId { get; init; }
    public string BillingPeriod { get; init; } = string.Empty;
    public string DealerPlan { get; init; } = string.Empty;
    public int IncludedLimit { get; init; }
    public int TotalConversations { get; init; }
    public int OverageCount { get; init; }
    public decimal UnitCost { get; init; }
    public decimal TotalOverageCost { get; init; }
    public string Currency { get; init; } = "USD";
    public List<ConversationOverageDetailDto> Details { get; init; } = new();
    public DateTime GeneratedAtUtc { get; init; } = DateTime.UtcNow;
}
