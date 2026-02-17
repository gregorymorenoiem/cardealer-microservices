using FeatureToggleService.Application.Interfaces;
using MediatR;

namespace FeatureToggleService.Application.Features.ABExperiments.Queries.GetExperimentAnalysis;

public class GetExperimentAnalysisHandler : IRequestHandler<GetExperimentAnalysisQuery, ExperimentAnalysisResponse>
{
    private readonly IABTestingService _abTestingService;

    public GetExperimentAnalysisHandler(IABTestingService abTestingService)
    {
        _abTestingService = abTestingService;
    }

    public async Task<ExperimentAnalysisResponse> Handle(GetExperimentAnalysisQuery request, CancellationToken cancellationToken)
    {
        var experiment = await _abTestingService.GetExperimentAsync(request.ExperimentId, cancellationToken);

        if (experiment == null)
        {
            throw new KeyNotFoundException($"Experiment with ID {request.ExperimentId} not found");
        }

        var analysis = await _abTestingService.AnalyzeExperimentAsync(request.ExperimentId, cancellationToken);

        var recommendedWinnerKey = analysis.RecommendedWinnerId.HasValue
            ? experiment.Variants.FirstOrDefault(v => v.Id == analysis.RecommendedWinnerId.Value)?.Key
            : null;

        return new ExperimentAnalysisResponse
        {
            Id = experiment.Id,
            Key = experiment.Key,
            Name = experiment.Name,
            Status = experiment.Status.ToString(),
            StartedAt = experiment.StartedAt ?? DateTime.UtcNow,
            DaysRunning = analysis.DaysRunning,
            TotalParticipants = analysis.TotalParticipants,
            TotalExposures = analysis.TotalExposures,
            VariantResults = analysis.VariantResults.Select(vr => new VariantResultDto
            {
                Id = vr.VariantId,
                Key = vr.VariantKey,
                Name = vr.VariantName,
                IsControl = vr.IsControl,
                Participants = vr.TotalAssignments,
                Exposures = vr.TotalExposures,
                Conversions = vr.TotalConversions,
                ConversionRate = vr.ConversionRate,
                StandardError = vr.StandardError,
                ConfidenceIntervalLower = vr.ConfidenceIntervalLower,
                ConfidenceIntervalUpper = vr.ConfidenceIntervalUpper,
                PValue = vr.PValue ?? 0,
                ZScore = vr.ZScore ?? 0,
                RelativeLift = vr.RelativeLift,
                AbsoluteDifference = vr.AbsoluteDifference,
                IsStatisticallySignificant = vr.IsStatisticallySignificant ?? false,
                TotalRevenue = vr.TotalRevenue ?? 0,
                AverageRevenuePerUser = vr.AverageRevenuePerUser ?? 0
            }).ToList(),
            RecommendedWinnerId = analysis.RecommendedWinnerId,
            RecommendedWinnerKey = recommendedWinnerKey,
            WinnerConfidence = analysis.WinnerConfidence,
            Recommendation = analysis.Recommendation,
            Warnings = analysis.Warnings,
            Insights = analysis.Insights
        };
    }
}
