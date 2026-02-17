// NOTE: Report queries/commands are now handled directly by the ReportsController
// via IReportsServiceClient HTTP client calls to ReportsService.
// These MediatR handlers are kept as no-op stubs to prevent MediatR registration errors.
// The actual data flows through: AdminService → HTTP → ReportsService → PostgreSQL.

using AdminService.Application.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdminService.Application.UseCases.Reports;

// Stub handlers — not actively used, but registered by MediatR assembly scanning.

public class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, PaginatedReportResponse>
{
    public Task<PaginatedReportResponse> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new PaginatedReportResponse(
            new List<ReportDto>(), 0, request.Page, request.PageSize, 0));
    }
}

public class GetReportByIdQueryHandler : IRequestHandler<GetReportByIdQuery, ReportDto?>
{
    public Task<ReportDto?> Handle(GetReportByIdQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult<ReportDto?>(null);
    }
}

public class GetReportStatsQueryHandler : IRequestHandler<GetReportStatsQuery, ReportStatsDto>
{
    public Task<ReportStatsDto> Handle(GetReportStatsQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ReportStatsDto(0, 0, 0, 0, 0, 0));
    }
}

public class UpdateReportStatusCommandHandler : IRequestHandler<UpdateReportStatusCommand, bool>
{
    public Task<bool> Handle(UpdateReportStatusCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }
}
