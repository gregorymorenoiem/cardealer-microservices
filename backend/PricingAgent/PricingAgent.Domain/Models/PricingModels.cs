// =============================================================================
// PricingAgent — Domain Models
// LLM-powered vehicle pricing for the Dominican Republic market.
// Uses market data + LLM analysis to generate fair price ranges in DOP/USD.
// =============================================================================

namespace PricingAgent.Domain.Models;

/// <summary>
/// Vehicle data input for pricing analysis.
/// </summary>
public sealed class VehiclePricingInput
{
    public required string Make { get; init; }
    public required string Model { get; init; }
    public required int Year { get; init; }
    public string? Trim { get; init; }
    public int? Mileage { get; init; }
    public string? FuelType { get; init; }
    public string? Transmission { get; init; }
    public string? Condition { get; init; } // "new", "used", "certified"
    public string? Color { get; init; }
    public string? Province { get; init; } // Dominican Republic province
    public decimal? AskingPrice { get; init; }
    public string? Currency { get; init; } = "DOP";
    public IReadOnlyList<string>? Features { get; init; }
    public IReadOnlyList<string>? DamageNotes { get; init; }
}

/// <summary>
/// Market comparables used for pricing context.
/// </summary>
public sealed class MarketComparable
{
    public required string Make { get; init; }
    public required string Model { get; init; }
    public required int Year { get; init; }
    public required decimal Price { get; init; }
    public required string Currency { get; init; }
    public int? Mileage { get; init; }
    public string? Condition { get; init; }
    public string? Source { get; init; } // "okla", "supercarros", "facebook"
    public DateTimeOffset? ListedAt { get; init; }
    public int? DaysOnMarket { get; init; }
}

/// <summary>
/// LLM-generated pricing analysis result.
/// </summary>
public sealed class PricingAnalysis
{
    /// <summary>Suggested fair market price range (low end) in DOP.</summary>
    public decimal PrecioMinimoDop { get; init; }

    /// <summary>Suggested fair market price range (high end) in DOP.</summary>
    public decimal PrecioMaximoDop { get; init; }

    /// <summary>Most likely selling price in DOP.</summary>
    public decimal PrecioSugeridoDop { get; init; }

    /// <summary>USD equivalent of suggested price.</summary>
    public decimal PrecioSugeridoUsd { get; init; }

    /// <summary>Confidence score (0.0 - 1.0).</summary>
    public double Confianza { get; init; }

    /// <summary>How the asking price compares: "bajo", "justo", "alto", "muy_alto".</summary>
    public string NivelPrecio { get; init; } = "justo";

    /// <summary>Percentage above/below fair market value. Negative = below.</summary>
    public double DesviacionPorcentaje { get; init; }

    /// <summary>Natural language explanation in Spanish.</summary>
    public string Explicacion { get; init; } = string.Empty;

    /// <summary>Factors that increase the value.</summary>
    public IReadOnlyList<string> FactoresPositivos { get; init; } = [];

    /// <summary>Factors that decrease the value.</summary>
    public IReadOnlyList<string> FactoresNegativos { get; init; } = [];

    /// <summary>Recommended negotiation range for buyer.</summary>
    public string? RecomendacionComprador { get; init; }

    /// <summary>Recommended listing strategy for seller.</summary>
    public string? RecomendacionVendedor { get; init; }

    /// <summary>Estimated days to sell at suggested price.</summary>
    public int DiasEstimadosVenta { get; init; }

    /// <summary>Market trend: "subiendo", "estable", "bajando".</summary>
    public string TendenciaMercado { get; init; } = "estable";
}
