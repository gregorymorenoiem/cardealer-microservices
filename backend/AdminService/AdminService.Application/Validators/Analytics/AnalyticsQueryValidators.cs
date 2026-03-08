using FluentValidation;
using AdminService.Application.UseCases.Analytics;

namespace AdminService.Application.Validators.Analytics;

/// <summary>
/// Validators for all analytics queries.
/// All queries accept a Period string that must be validated against NoSqlInjection/NoXss.
/// </summary>
public class GetAnalyticsOverviewQueryValidator : AbstractValidator<GetAnalyticsOverviewQuery>
{
    private static readonly string[] ValidPeriods = { "1d", "7d", "14d", "30d", "90d", "6m", "1y" };

    public GetAnalyticsOverviewQueryValidator()
    {
        RuleFor(x => x.Period)
            .Must(p => ValidPeriods.Contains(p, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Period must be one of: {string.Join(", ", ValidPeriods)}.")
            .NoSqlInjection()
            .NoXss();
    }
}

public class GetWeeklyDataQueryValidator : AbstractValidator<GetWeeklyDataQuery>
{
    private static readonly string[] ValidPeriods = { "1d", "7d", "14d", "30d", "90d", "6m", "1y" };

    public GetWeeklyDataQueryValidator()
    {
        RuleFor(x => x.Period)
            .Must(p => ValidPeriods.Contains(p, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Period must be one of: {string.Join(", ", ValidPeriods)}.")
            .NoSqlInjection()
            .NoXss();
    }
}

public class GetTrafficSourcesQueryValidator : AbstractValidator<GetTrafficSourcesQuery>
{
    private static readonly string[] ValidPeriods = { "1d", "7d", "14d", "30d", "90d", "6m", "1y" };

    public GetTrafficSourcesQueryValidator()
    {
        RuleFor(x => x.Period)
            .Must(p => ValidPeriods.Contains(p, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Period must be one of: {string.Join(", ", ValidPeriods)}.")
            .NoSqlInjection()
            .NoXss();
    }
}

public class GetDeviceBreakdownQueryValidator : AbstractValidator<GetDeviceBreakdownQuery>
{
    private static readonly string[] ValidPeriods = { "1d", "7d", "14d", "30d", "90d", "6m", "1y" };

    public GetDeviceBreakdownQueryValidator()
    {
        RuleFor(x => x.Period)
            .Must(p => ValidPeriods.Contains(p, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Period must be one of: {string.Join(", ", ValidPeriods)}.")
            .NoSqlInjection()
            .NoXss();
    }
}

public class GetConversionRatesQueryValidator : AbstractValidator<GetConversionRatesQuery>
{
    private static readonly string[] ValidPeriods = { "1d", "7d", "14d", "30d", "90d", "6m", "1y" };

    public GetConversionRatesQueryValidator()
    {
        RuleFor(x => x.Period)
            .Must(p => ValidPeriods.Contains(p, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Period must be one of: {string.Join(", ", ValidPeriods)}.")
            .NoSqlInjection()
            .NoXss();
    }
}

public class GetRevenueByChannelQueryValidator : AbstractValidator<GetRevenueByChannelQuery>
{
    private static readonly string[] ValidPeriods = { "1d", "7d", "14d", "30d", "90d", "6m", "1y" };

    public GetRevenueByChannelQueryValidator()
    {
        RuleFor(x => x.Period)
            .Must(p => ValidPeriods.Contains(p, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Period must be one of: {string.Join(", ", ValidPeriods)}.")
            .NoSqlInjection()
            .NoXss();
    }
}

public class GetPlatformAnalyticsQueryValidator : AbstractValidator<GetPlatformAnalyticsQuery>
{
    private static readonly string[] ValidPeriods = { "1d", "7d", "14d", "30d", "90d", "6m", "1y" };

    public GetPlatformAnalyticsQueryValidator()
    {
        RuleFor(x => x.Period)
            .Must(p => ValidPeriods.Contains(p, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Period must be one of: {string.Join(", ", ValidPeriods)}.")
            .NoSqlInjection()
            .NoXss();
    }
}
