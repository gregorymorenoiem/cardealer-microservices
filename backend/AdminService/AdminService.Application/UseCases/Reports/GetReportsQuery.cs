using AdminService.Application.DTOs;
using MediatR;

namespace AdminService.Application.UseCases.Reports;

public record GetReportsQuery(
    string? Type = null,
    string? Status = null,
    string? Priority = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 10
) : IRequest<PaginatedReportResponse>;

public record GetReportByIdQuery(Guid Id) : IRequest<ReportDto?>;

public record GetReportStatsQuery() : IRequest<ReportStatsDto>;

public record UpdateReportStatusCommand(
    Guid Id,
    string Status,
    string? Resolution = null
) : IRequest<bool>;
