using MediatR;
using Microsoft.Extensions.Logging;
using AnalyticsAgent.Application.DTOs;
using AnalyticsAgent.Domain.Interfaces;
using AnalyticsAgent.Domain.Models;

namespace AnalyticsAgent.Application.Features.Analytics.Queries;

public sealed class GenerateInsightsQueryHandler
    : IRequestHandler<GenerateInsightsQuery, AnalyticsInsightResponse>
{
    private readonly ILlmAnalyticsService _analyticsService;
    private readonly ILogger<GenerateInsightsQueryHandler> _logger;

    public GenerateInsightsQueryHandler(ILlmAnalyticsService analyticsService, ILogger<GenerateInsightsQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<AnalyticsInsightResponse> Handle(GenerateInsightsQuery query, CancellationToken ct)
    {
        var req = query.Request;
        _logger.LogInformation("AnalyticsAgent: Generating {ReportType} insights for {Period}", req.ReportType, req.Period);

        var input = new AnalyticsInput
        {
            ReportType = req.ReportType,
            Period = req.Period,
            DealerId = req.DealerId,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            Metrics = req.Metrics
        };

        var insight = await _analyticsService.GenerateInsightsAsync(input, ct);

        return new AnalyticsInsightResponse
        {
            Titulo = insight.Titulo,
            ResumenEjecutivo = insight.ResumenEjecutivo,
            MetricasClave = insight.MetricasClave,
            Tendencias = insight.Tendencias,
            Recomendaciones = insight.Recomendaciones,
            Anomalias = insight.Anomalias,
            ContextoCompetitivo = insight.ContextoCompetitivo,
            Pronostico = insight.Pronostico,
            ModelUsed = "cascade",
            FallbackLevel = 0,
            LatencyMs = 0
        };
    }
}
