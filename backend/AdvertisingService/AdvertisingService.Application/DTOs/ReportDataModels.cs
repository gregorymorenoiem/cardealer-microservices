namespace AdvertisingService.Application.DTOs;

/// <summary>
/// Report data for a single campaign.
/// </summary>
public class CampaignReportData
{
    public Guid CampaignId { get; set; }
    public int TotalImpressions { get; set; }
    public int TotalClicks { get; set; }
    public double Ctr { get; set; }
    public decimal SpentBudget { get; set; }
    public decimal RemainingBudget { get; set; }
    public List<DailyDataPoint> DailyImpressions { get; set; } = new();
    public List<DailyDataPoint> DailyClicks { get; set; } = new();
}

/// <summary>
/// Report data for a single owner (Individual or Dealer).
/// </summary>
public class OwnerReportData
{
    public Guid OwnerId { get; set; }
    public string OwnerType { get; set; } = string.Empty;
    public int ActiveCampaigns { get; set; }
    public int TotalCampaigns { get; set; }
    public int TotalImpressions { get; set; }
    public int TotalClicks { get; set; }
    public double OverallCtr { get; set; }
    public decimal TotalSpent { get; set; }
    public List<DailyDataPoint> DailyImpressions { get; set; } = new();
    public List<DailyDataPoint> DailyClicks { get; set; } = new();
}

/// <summary>
/// Platform-wide report data.
/// </summary>
public class PlatformReportData
{
    public int TotalActiveCampaigns { get; set; }
    public int TotalCampaigns { get; set; }
    public int TotalImpressions { get; set; }
    public int TotalClicks { get; set; }
    public double OverallCtr { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ActiveAdvertisers { get; set; }
    public DateTime ReportPeriodStart { get; set; }
    public DateTime ReportPeriodEnd { get; set; }
}

/// <summary>
/// A single data point for daily time series.
/// </summary>
public class DailyDataPoint
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}
