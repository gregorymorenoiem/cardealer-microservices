using AuditService.Application.DTOs;
using AuditService.Shared;
using MediatR;

namespace AuditService.Application.Features.Audit.Queries.GetAuditLogs;

public class GetAuditLogsQuery : IRequest<ApiResponse<PaginatedResult<AuditLogDto>>>
{
    public string? UserId { get; set; }
    public string? Action { get; set; }
    public string? Resource { get; set; }
    public string? ServiceName { get; set; }
    public string? Severity { get; set; }
    public bool? Success { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
    public string? SearchText { get; set; }

    public GetAuditLogsQuery() { }

    public GetAuditLogsQuery(
        string? userId = null,
        string? action = null,
        string? resource = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 50)
    {
        UserId = userId;
        Action = action;
        Resource = resource;
        FromDate = fromDate;
        ToDate = toDate;
        Page = page;
        PageSize = pageSize;
    }
}