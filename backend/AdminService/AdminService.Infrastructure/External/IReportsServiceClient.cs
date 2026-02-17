using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AdminService.Infrastructure.External
{
    /// <summary>
    /// HTTP client for communicating with ReportsService (content moderation reports).
    /// </summary>
    public interface IReportsServiceClient
    {
        Task<ContentReportSearchResponse?> GetReportsAsync(
            ContentReportSearchFilters filters, CancellationToken ct = default);

        Task<ContentReportDetailResponse?> GetReportByIdAsync(
            Guid reportId, CancellationToken ct = default);

        Task<ContentReportStatsResponse> GetReportStatsAsync(CancellationToken ct = default);

        Task<bool> UpdateReportStatusAsync(
            Guid reportId, string status, string? resolution = null,
            string? resolvedById = null, CancellationToken ct = default);
    }

    // ── Request / Response DTOs ──────────────────────────────────────

    public class ContentReportSearchFilters
    {
        public string? Type { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ContentReportSearchResponse
    {
        public List<ContentReportItemDto> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class ContentReportItemDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string TargetId { get; set; } = string.Empty;
        public string TargetTitle { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ReportedById { get; set; } = string.Empty;
        public string ReportedByEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedById { get; set; }
        public string? Resolution { get; set; }
        public int ReportCount { get; set; }
    }

    public class ContentReportDetailResponse
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string TargetId { get; set; } = string.Empty;
        public string TargetTitle { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ReportedById { get; set; } = string.Empty;
        public string ReportedByEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedById { get; set; }
        public string? Resolution { get; set; }
        public int ReportCount { get; set; }
    }

    public class ContentReportStatsResponse
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Investigating { get; set; }
        public int Resolved { get; set; }
        public int Dismissed { get; set; }
        public int HighPriority { get; set; }
    }
}
