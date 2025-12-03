using Gateway.Domain.Interfaces;

namespace Gateway.Application.UseCases;

/// <summary>
/// Use case for recording HTTP request metrics
/// </summary>
public class RecordRequestMetricsUseCase
{
    private readonly IMetricsService _metricsService;

    public RecordRequestMetricsUseCase(IMetricsService metricsService)
    {
        _metricsService = metricsService;
    }

    public void Execute(string route, string method, int statusCode, double durationSeconds)
    {
        _metricsService.RecordRequest(route, method, statusCode, durationSeconds);
    }
}

/// <summary>
/// Use case for recording downstream service call metrics
/// </summary>
public class RecordDownstreamCallMetricsUseCase
{
    private readonly IMetricsService _metricsService;

    public RecordDownstreamCallMetricsUseCase(IMetricsService metricsService)
    {
        _metricsService = metricsService;
    }

    public void Execute(string service, double latencySeconds, bool success)
    {
        _metricsService.RecordDownstreamCall(service, latencySeconds, success);
    }
}
