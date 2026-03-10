using PricingAgent.Domain.Models;

namespace PricingAgent.Application.DTOs;

/// <summary>
/// Request to analyze a vehicle's price.
/// </summary>
public sealed class PricingAnalysisRequest
{
    public required string Make { get; init; }
    public required string Model { get; init; }
    public required int Year { get; init; }
    public string? Trim { get; init; }
    public int? Mileage { get; init; }
    public string? FuelType { get; init; }
    public string? Transmission { get; init; }
    public string? Condition { get; init; }
    public string? Province { get; init; }
    public decimal? AskingPrice { get; init; }
    public string? Currency { get; init; } = "DOP";
    public IReadOnlyList<string>? Features { get; init; }
}

/// <summary>
/// Response with pricing analysis.
/// </summary>
public sealed class PricingAnalysisResponse
{
    public required decimal PrecioMinimoDop { get; init; }
    public required decimal PrecioMaximoDop { get; init; }
    public required decimal PrecioSugeridoDop { get; init; }
    public required decimal PrecioSugeridoUsd { get; init; }
    public required double Confianza { get; init; }
    public required string NivelPrecio { get; init; }
    public required double DesviacionPorcentaje { get; init; }
    public required string Explicacion { get; init; }
    public required IReadOnlyList<string> FactoresPositivos { get; init; }
    public required IReadOnlyList<string> FactoresNegativos { get; init; }
    public string? RecomendacionComprador { get; init; }
    public string? RecomendacionVendedor { get; init; }
    public required int DiasEstimadosVenta { get; init; }
    public required string TendenciaMercado { get; init; }
    public required string ModelUsed { get; init; }
    public required int FallbackLevel { get; init; }
    public required double LatencyMs { get; init; }
}

/// <summary>
/// Batch pricing request for multiple vehicles.
/// </summary>
public sealed class BatchPricingRequest
{
    public required IReadOnlyList<PricingAnalysisRequest> Vehicles { get; init; }
}

/// <summary>
/// Batch pricing response.
/// </summary>
public sealed class BatchPricingResponse
{
    public required IReadOnlyList<PricingAnalysisResponse> Results { get; init; }
    public required int TotalProcessed { get; init; }
    public required double TotalLatencyMs { get; init; }
}
