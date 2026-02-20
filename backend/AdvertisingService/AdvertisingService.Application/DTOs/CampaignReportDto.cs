namespace AdvertisingService.Application.DTOs;

public record CampaignReportDto(
    Guid CampaignId,
    int TotalImpressions,
    int TotalClicks,
    double Ctr,
    decimal SpentBudget,
    decimal RemainingBudget,
    List<DailyDataPointDto> DailyImpressions,
    List<DailyDataPointDto> DailyClicks
);

public record OwnerReportDto(
    Guid OwnerId,
    string OwnerType,
    int ActiveCampaigns,
    int TotalCampaigns,
    int TotalImpressions,
    int TotalClicks,
    double OverallCtr,
    decimal TotalSpent,
    List<DailyDataPointDto> DailyImpressions,
    List<DailyDataPointDto> DailyClicks
);

public record PlatformReportDto(
    int TotalActiveCampaigns,
    int TotalCampaigns,
    int TotalImpressions,
    int TotalClicks,
    double OverallCtr,
    decimal TotalRevenue,
    int ActiveAdvertisers,
    DateTime ReportPeriodStart,
    DateTime ReportPeriodEnd
);

public record DailyDataPointDto(
    DateTime Date,
    int Count
);
