namespace ReportsService.Application.DTOs;

// ── Response DTOs ────────────────────────────────────

public record ContentReportDto(
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
    int ReportCount,
    string? ReportCategory = null);

public record ContentReportStatsDto(
    int Total,
    int Pending,
    int Investigating,
    int Resolved,
    int Dismissed,
    int HighPriority);

public record PaginatedContentReportResponse(
    IReadOnlyList<ContentReportDto> Items,
    int Total,
    int Page,
    int PageSize,
    int TotalPages);

// ── Request DTOs ────────────────────────────────────

public record CreateContentReportRequest(
    string Type,
    string TargetId,
    string TargetTitle,
    string Reason,
    string Description,
    string ReportedById,
    string ReportedByEmail,
    string? Priority = null,
    string? ReportCategory = null);

/// <summary>
/// Anonymous report request — no auth required.
/// Buyer can report a vehicle listing without registering.
/// </summary>
public record CreateAnonymousVehicleReportRequest(
    string VehicleId,
    string VehicleTitle,
    string ReportCategory,
    string? Description = null,
    string? ContactEmail = null);

public record UpdateContentReportStatusRequest(
    string Status,
    string? Resolution = null,
    string? ResolvedById = null);
