using DealerAnalyticsService.Domain.Entities;

namespace DealerAnalyticsService.Application.DTOs;

// ============================================
// Analytics DTOs
// ============================================

public record AnalyticsDashboardDto(
    AnalyticsSummaryDto Summary,
    List<TimeseriesDataPoint> ViewsTrend,
    List<ContactMethodStats> ContactMethodBreakdown,
    List<DeviceStats> DeviceBreakdown,
    List<TopReferrer> TopReferrers,
    LiveStatsDto LiveStats
);

public record AnalyticsSummaryDto(
    int TotalViews,
    int UniqueVisitors,
    double AverageViewDuration,
    int TotalContacts,
    double ContactConversionRate,
    double InquiryConversionRate,
    double BounceRate,
    double EngagementRate,
    int ComparedToLastPeriod // % change
);

public record TimeseriesDataPoint(
    DateTime Date,
    int Views,
    int Contacts,
    int UniqueVisitors
);

public record ContactMethodStats(
    ContactType Type,
    string Label,
    int Count,
    double Percentage,
    int ConvertedCount,
    double ConversionRate
);

public record DeviceStats(
    string DeviceType,
    int Count,
    double Percentage
);

public record TopReferrer(
    string Source,
    int Views,
    double Percentage
);

public record LiveStatsDto(
    int CurrentViewers,
    ProfileViewDto? MostRecentView,
    int ViewsToday,
    int ContactsToday
);

public record ProfileViewDto(
    Guid Id,
    DateTime ViewedAt,
    string? DeviceType,
    string? City,
    string? Country,
    int DurationSeconds,
    bool IsBounce
);

public record ContactEventDto(
    Guid Id,
    DateTime ClickedAt,
    ContactType ContactType,
    string Source,
    string? DeviceType,
    bool ConvertedToInquiry,
    TimeSpan? TimeToConversion
);

public record TrackProfileViewRequest(
    Guid DealerId,
    string? ViewerIpAddress,
    string? ViewerUserAgent,
    Guid? ViewerUserId,
    string? ReferrerUrl,
    string? ViewedPage,
    int DurationSeconds
);

public record TrackContactEventRequest(
    Guid DealerId,
    ContactType ContactType,
    string? ViewerIpAddress,
    Guid? ViewerUserId,
    string? ContactValue,
    Guid? VehicleId,
    string? Source
);

public record AnalyticsDateRangeRequest(
    DateTime StartDate,
    DateTime EndDate
);

// ============================================
// Comparison DTOs
// ============================================

public record PeriodComparisonDto(
    AnalyticsSummaryDto CurrentPeriod,
    AnalyticsSummaryDto PreviousPeriod,
    Dictionary<string, double> PercentageChanges
);

// ============================================
// Export DTOs
// ============================================

public record AnalyticsExportDto(
    Guid DealerId,
    string DealerName,
    DateTime StartDate,
    DateTime EndDate,
    AnalyticsSummaryDto Summary,
    List<DailyAnalyticsSummary> DailyData
);
