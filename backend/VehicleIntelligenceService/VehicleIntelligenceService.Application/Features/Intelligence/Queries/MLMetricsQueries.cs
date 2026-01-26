using MediatR;
using VehicleIntelligenceService.Application.DTOs;

namespace VehicleIntelligenceService.Application.Features.Intelligence.Queries;

public record GetMLStatisticsQuery : IRequest<MLStatisticsDto>;

public class GetMLStatisticsHandler : IRequestHandler<GetMLStatisticsQuery, MLStatisticsDto>
{
    public Task<MLStatisticsDto> Handle(GetMLStatisticsQuery request, CancellationToken cancellationToken)
    {
        // En producción, obtendría datos reales de Prometheus/métricas del servicio
        var result = new MLStatisticsDto(
            TotalInferences: Random.Shared.Next(100000, 500000),
            SuccessRate: 99.85m,
            ErrorsLast24h: Random.Shared.Next(0, 50),
            LastUpdated: DateTime.UtcNow
        );

        return Task.FromResult(result);
    }
}

public record GetModelPerformanceQuery : IRequest<List<ModelPerformanceDto>>;

public class GetModelPerformanceHandler : IRequestHandler<GetModelPerformanceQuery, List<ModelPerformanceDto>>
{
    public Task<List<ModelPerformanceDto>> Handle(GetModelPerformanceQuery request, CancellationToken cancellationToken)
    {
        // En producción, obtendría datos reales de monitoreo de modelos
        var result = new List<ModelPerformanceDto>
        {
            new(
                ModelName: "Pricing Model v2.3.1",
                Accuracy: 0.89m,
                Mae: 45000,
                Rmse: 67500,
                LastTrained: DateTime.UtcNow.AddHours(-10),
                NextTraining: DateTime.UtcNow.AddHours(14),
                Status: "healthy"
            ),
            new(
                ModelName: "Demand Model v1.5.0",
                Accuracy: 0.87m,
                Mae: 0,
                Rmse: 0,
                LastTrained: DateTime.UtcNow.AddHours(-16),
                NextTraining: DateTime.UtcNow.AddHours(8),
                Status: "healthy"
            ),
            new(
                ModelName: "Time-to-Sale v1.2.0",
                Accuracy: 0.82m,
                Mae: 0,
                Rmse: 0,
                LastTrained: DateTime.UtcNow.AddDays(-2),
                NextTraining: DateTime.UtcNow.AddDays(2),
                Status: "warning"
            )
        };

        return Task.FromResult(result);
    }
}

public record GetInferenceMetricsQuery : IRequest<InferenceMetricsDto>;

public class GetInferenceMetricsHandler : IRequestHandler<GetInferenceMetricsQuery, InferenceMetricsDto>
{
    public Task<InferenceMetricsDto> Handle(GetInferenceMetricsQuery request, CancellationToken cancellationToken)
    {
        // En producción, obtendría datos reales de Prometheus
        var result = new InferenceMetricsDto(
            TotalInferences: Random.Shared.Next(100000, 500000),
            SuccessRate: 99.85m,
            AvgLatencyMs: 45.5,
            P95LatencyMs: 120.3,
            P99LatencyMs: 285.7,
            ErrorsLast24h: Random.Shared.Next(0, 50),
            LastUpdated: DateTime.UtcNow
        );

        return Task.FromResult(result);
    }
}
