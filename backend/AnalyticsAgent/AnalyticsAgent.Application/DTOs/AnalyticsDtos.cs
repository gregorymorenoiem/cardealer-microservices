using AnalyticsAgent.Domain.Models;

namespace AnalyticsAgent.Application.DTOs;

public sealed class AnalyticsInsightRequest
{
    public required string ReportType { get; init; }
    public required string Period { get; init; }
    public string? DealerId { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public required Dictionary<string, object> Metrics { get; init; }
}

public sealed class AnalyticsInsightResponse
{
    public required string Titulo { get; init; }
    public required string ResumenEjecutivo { get; init; }
    public required IReadOnlyList<MetricInsight> MetricasClave { get; init; }
    public required IReadOnlyList<TrendInsight> Tendencias { get; init; }
    public required IReadOnlyList<Recommendation> Recomendaciones { get; init; }
    public required IReadOnlyList<Anomaly> Anomalias { get; init; }
    public string? ContextoCompetitivo { get; init; }
    public string? Pronostico { get; init; }
    public required string ModelUsed { get; init; }
    public required int FallbackLevel { get; init; }
    public required double LatencyMs { get; init; }
}
