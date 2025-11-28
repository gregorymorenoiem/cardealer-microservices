using AuditService.Application.DTOs;
using AuditService.Shared;
using MediatR;

namespace AuditService.Application.Features.Audit.Queries.GetAuditStats;

public class GetAuditStatsQuery : IRequest<ApiResponse<AuditStatsDto>>
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? ServiceName { get; set; }
    public string? UserId { get; set; }
    public string? Action { get; set; }

    public GetAuditStatsQuery() { }

    public GetAuditStatsQuery(DateTime? fromDate = null, DateTime? toDate = null)
    {
        FromDate = fromDate;
        ToDate = toDate;
    }
}