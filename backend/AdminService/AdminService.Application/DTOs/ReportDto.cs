namespace AdminService.Application.DTOs;

public record ReportDto(
    Guid Id,
    string Type,
    string TargetId,
    string TargetTitle,
    string Reason,
    string Description,
    string ReportedById,
    string ReportedByEmail,
    string Status,
    string Priority,
    DateTime CreatedAt,
    DateTime? ResolvedAt,
    string? ResolvedById,
    string? Resolution,
    int ReportCount);

public record ReportStatsDto(
    int Total,
    int Pending,
    int Investigating,
    int Resolved,
    int Dismissed,
    int HighPriority);

public record PaginatedReportResponse(
    IReadOnlyList<ReportDto> Items,
    int Total,
    int Page,
    int PageSize,
    int TotalPages);
