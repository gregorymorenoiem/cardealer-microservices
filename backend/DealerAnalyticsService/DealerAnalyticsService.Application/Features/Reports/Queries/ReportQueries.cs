using DealerAnalyticsService.Application.DTOs;
using MediatR;

namespace DealerAnalyticsService.Application.Features.Reports.Queries;

/// <summary>
/// Query para generar reporte diario
/// </summary>
public record GetDailyReportQuery(
    Guid DealerId,
    DateTime Date
) : IRequest<AnalyticsReportDto>;

/// <summary>
/// Query para generar reporte semanal
/// </summary>
public record GetWeeklyReportQuery(
    Guid DealerId,
    DateTime WeekStartDate
) : IRequest<AnalyticsReportDto>;

/// <summary>
/// Query para generar reporte mensual
/// </summary>
public record GetMonthlyReportQuery(
    Guid DealerId,
    int Year,
    int Month
) : IRequest<AnalyticsReportDto>;

/// <summary>
/// Query para generar reporte personalizado
/// </summary>
public record GetCustomReportQuery(
    Guid DealerId,
    DateTime FromDate,
    DateTime ToDate,
    List<string>? IncludeSections = null
) : IRequest<AnalyticsReportDto>;

/// <summary>
/// Query para exportar reporte en formato específico
/// </summary>
public record ExportReportQuery(
    Guid DealerId,
    DateTime FromDate,
    DateTime ToDate,
    string Format // pdf, excel, csv
) : IRequest<ReportExportDto>;

public record ReportExportDto
{
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public byte[] Content { get; init; } = Array.Empty<byte>();
    public DateTime GeneratedAt { get; init; }
}
