using ListingAgent.Domain.Models;

namespace ListingAgent.Domain.Interfaces;

/// <summary>
/// LLM-powered listing optimization service.
/// </summary>
public interface ILlmListingService
{
    /// <summary>
    /// Analyze and optimize a vehicle listing using LLM.
    /// </summary>
    Task<ListingOptimization> OptimizeListingAsync(ListingInput listing, CancellationToken ct = default);
}
