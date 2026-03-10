using AnalyticsAgent.Domain.Models;

namespace AnalyticsAgent.Domain.Interfaces;

public interface ILlmAnalyticsService
{
    Task<AnalyticsInsight> GenerateInsightsAsync(AnalyticsInput input, CancellationToken ct = default);
}
