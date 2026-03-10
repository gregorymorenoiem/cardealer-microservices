using PricingAgent.Domain.Models;

namespace PricingAgent.Domain.Interfaces;

/// <summary>
/// LLM-powered pricing service that analyzes vehicle value using market data + AI.
/// </summary>
public interface ILlmPricingService
{
    /// <summary>
    /// Generate a pricing analysis for a vehicle using LLM + market comparables.
    /// </summary>
    Task<PricingAnalysis> AnalyzePriceAsync(
        VehiclePricingInput vehicle,
        IReadOnlyList<MarketComparable> comparables,
        CancellationToken ct = default);
}

/// <summary>
/// Repository for persisting pricing analyses.
/// </summary>
public interface IPricingAnalysisRepository
{
    Task<PricingAnalysis?> GetCachedAnalysisAsync(string vehicleFingerprint, CancellationToken ct = default);
    Task SaveAnalysisAsync(string vehicleFingerprint, PricingAnalysis analysis, CancellationToken ct = default);
}
