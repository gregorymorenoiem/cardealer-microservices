using AdvertisingService.Application.DTOs;
using AdvertisingService.Application.Interfaces;
using MediatR;

namespace AdvertisingService.Application.Features.Reports.Queries.GetCampaignReport;

public class GetCampaignReportQueryHandler : IRequestHandler<GetCampaignReportQuery, CampaignReportDto?>
{
    private readonly IAdReportingService _reportingService;

    public GetCampaignReportQueryHandler(IAdReportingService reportingService)
        => _reportingService = reportingService;

    public async Task<CampaignReportDto?> Handle(GetCampaignReportQuery request, CancellationToken ct)
    {
        var since = DateTime.UtcNow.AddDays(-request.DaysBack);
        var report = await _reportingService.GetCampaignReportAsync(request.CampaignId, since, ct);

        if (report.CampaignId == Guid.Empty) return null;

        return new CampaignReportDto(
            report.CampaignId, report.TotalImpressions, report.TotalClicks, report.Ctr,
            report.SpentBudget, report.RemainingBudget,
            report.DailyImpressions.Select(d => new DailyDataPointDto(d.Date, d.Count)).ToList(),
            report.DailyClicks.Select(d => new DailyDataPointDto(d.Date, d.Count)).ToList()
        );
    }
}
