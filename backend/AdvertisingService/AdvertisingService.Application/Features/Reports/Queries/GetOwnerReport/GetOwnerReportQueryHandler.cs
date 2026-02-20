using AdvertisingService.Application.DTOs;
using AdvertisingService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdvertisingService.Application.Features.Reports.Queries.GetOwnerReport;

public class GetOwnerReportQueryHandler : IRequestHandler<GetOwnerReportQuery, OwnerReportDto?>
{
    private readonly IAdReportingService _reportingService;
    private readonly ILogger<GetOwnerReportQueryHandler> _logger;

    public GetOwnerReportQueryHandler(
        IAdReportingService reportingService,
        ILogger<GetOwnerReportQueryHandler> logger)
    {
        _reportingService = reportingService;
        _logger = logger;
    }

    public async Task<OwnerReportDto?> Handle(GetOwnerReportQuery request, CancellationToken ct)
    {
        var since = DateTime.UtcNow.AddDays(-request.DaysBack).Date;

        _logger.LogDebug("Generating owner report for {OwnerId} ({OwnerType}), since {Since}",
            request.OwnerId, request.OwnerType, since);

        var data = await _reportingService.GetOwnerReportAsync(
            request.OwnerId, request.OwnerType, since, ct);

        return new OwnerReportDto(
            data.OwnerId,
            data.OwnerType,
            data.ActiveCampaigns,
            data.TotalCampaigns,
            data.TotalImpressions,
            data.TotalClicks,
            data.OverallCtr,
            data.TotalSpent,
            data.DailyImpressions.Select(d => new DailyDataPointDto(d.Date, d.Count)).ToList(),
            data.DailyClicks.Select(d => new DailyDataPointDto(d.Date, d.Count)).ToList()
        );
    }
}
