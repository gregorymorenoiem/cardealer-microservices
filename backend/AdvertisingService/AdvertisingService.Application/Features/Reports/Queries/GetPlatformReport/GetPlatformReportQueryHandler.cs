using AdvertisingService.Application.DTOs;
using AdvertisingService.Application.Interfaces;
using MediatR;

namespace AdvertisingService.Application.Features.Reports.Queries.GetPlatformReport;

public class GetPlatformReportQueryHandler : IRequestHandler<GetPlatformReportQuery, PlatformReportDto>
{
    private readonly IAdReportingService _reportingService;

    public GetPlatformReportQueryHandler(IAdReportingService reportingService)
        => _reportingService = reportingService;

    public async Task<PlatformReportDto> Handle(GetPlatformReportQuery request, CancellationToken ct)
    {
        var since = DateTime.UtcNow.AddDays(-request.DaysBack);
        var report = await _reportingService.GetPlatformReportAsync(since, ct);

        return new PlatformReportDto(
            report.TotalActiveCampaigns, report.TotalCampaigns,
            report.TotalImpressions, report.TotalClicks, report.OverallCtr,
            report.TotalRevenue, report.ActiveAdvertisers,
            report.ReportPeriodStart, report.ReportPeriodEnd
        );
    }
}
