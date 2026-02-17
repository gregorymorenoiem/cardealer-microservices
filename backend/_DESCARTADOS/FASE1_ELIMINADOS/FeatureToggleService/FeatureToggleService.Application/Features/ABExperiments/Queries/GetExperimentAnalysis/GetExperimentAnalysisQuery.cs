using FeatureToggleService.Domain.Entities;
using MediatR;

namespace FeatureToggleService.Application.Features.ABExperiments.Queries.GetExperimentAnalysis;

public class GetExperimentAnalysisQuery : IRequest<ExperimentAnalysisResponse>
{
    public Guid ExperimentId { get; set; }
}

public class ExperimentAnalysisResponse
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public int DaysRunning { get; set; }
    public int TotalParticipants { get; set; }
    public int TotalExposures { get; set; }

    public List<VariantResultDto> VariantResults { get; set; } = new();

    public Guid? RecommendedWinnerId { get; set; }
    public string? RecommendedWinnerKey { get; set; }
    public double? WinnerConfidence { get; set; }
    public string Recommendation { get; set; } = string.Empty;

    public List<string> Warnings { get; set; } = new();
    public List<string> Insights { get; set; } = new();
}

public class VariantResultDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsControl { get; set; }

    public int Participants { get; set; }
    public int Exposures { get; set; }
    public int Conversions { get; set; }

    public double ConversionRate { get; set; }
    public double StandardError { get; set; }
    public double ConfidenceIntervalLower { get; set; }
    public double ConfidenceIntervalUpper { get; set; }

    public double? PValue { get; set; }
    public double? ZScore { get; set; }
    public double? RelativeLift { get; set; }
    public double? AbsoluteDifference { get; set; }
    public bool? IsStatisticallySignificant { get; set; }

    public double TotalRevenue { get; set; }
    public double AverageRevenuePerUser { get; set; }
}
