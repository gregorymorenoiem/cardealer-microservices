using ListingAgent.Domain.Models;

namespace ListingAgent.Application.DTOs;

public sealed class ListingOptimizationRequest
{
    public required string VehicleId { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required string Make { get; init; }
    public required string Model { get; init; }
    public required int Year { get; init; }
    public string? Trim { get; init; }
    public decimal? Price { get; init; }
    public string? Currency { get; init; } = "DOP";
    public int? Mileage { get; init; }
    public string? FuelType { get; init; }
    public string? Transmission { get; init; }
    public string? Condition { get; init; }
    public int PhotoCount { get; init; }
    public IReadOnlyList<string>? Features { get; init; }
    public string? SellerType { get; init; }
    public string? Province { get; init; }
}

public sealed class ListingOptimizationResponse
{
    public required int PuntajeCalidad { get; init; }
    public required string TituloOptimizado { get; init; }
    public required string DescripcionOptimizada { get; init; }
    public required string DescripcionWhatsApp { get; init; }
    public required string MetaDescription { get; init; }
    public required IReadOnlyList<string> Tags { get; init; }
    public required IReadOnlyList<ListingTip> Mejoras { get; init; }
    public string? VentajaCompetitiva { get; init; }
    public required int BoostEstimadoPorcentaje { get; init; }
    public required string ModelUsed { get; init; }
    public required int FallbackLevel { get; init; }
    public required double LatencyMs { get; init; }
    /// <summary>True if the result was served from Redis cache — no LLM API call was made.</summary>
    public bool FromCache { get; init; }
    /// <summary>Semantic cache key (make:model:year:trim) for observability/debugging.</summary>
    public string? CacheKey { get; init; }
}
