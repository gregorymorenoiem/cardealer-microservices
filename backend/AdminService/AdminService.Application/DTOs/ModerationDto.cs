namespace AdminService.Application.DTOs;

/// <summary>
/// DTO for moderation queue items
/// </summary>
public record ModerationItemDto(
    Guid Id,
    string Type,
    string TargetId,
    string Title,
    string Description,
    decimal? Price,
    List<string> Images,
    string SellerName,
    string SellerType,
    string SellerId,
    DateTime SubmittedAt,
    string? FlagReason,
    string Priority,
    string Status,
    DateTime? ReviewedAt,
    string? ReviewedBy,
    string? ReviewerNotes,
    string? RejectionReason
);

/// <summary>
/// DTO for moderation statistics
/// </summary>
public record ModerationStatsDto(
    int InQueue,
    int HighPriority,
    int ReviewedToday,
    int RejectedToday,
    int ApprovedToday,
    int EscalatedCount,
    double AvgReviewTimeMinutes
);

/// <summary>
/// Paginated response for moderation queue
/// </summary>
public record PaginatedModerationResponse(
    IReadOnlyList<ModerationItemDto> Items,
    int Total,
    int Page,
    int PageSize,
    int TotalPages
);

/// <summary>
/// Request to take an action on a moderation item
/// </summary>
public record ModerationActionDto(
    string Action,  // approve, reject, escalate, skip
    string? Reason,
    string? Notes
);

/// <summary>
/// Response after processing a moderation action
/// </summary>
public record ModerationActionResponseDto(
    bool Success,
    string Message,
    Guid? NextItemId
);
