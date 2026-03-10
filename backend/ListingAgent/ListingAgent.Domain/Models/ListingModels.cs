// =============================================================================
// ListingAgent — Domain Models
// LLM-powered listing quality optimization for OKLA marketplace.
// Generates SEO titles, descriptions, quality scores, and improvement tips.
// =============================================================================

namespace ListingAgent.Domain.Models;

/// <summary>
/// Input listing data for quality analysis.
/// </summary>
public sealed class ListingInput
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
    public IReadOnlyList<string>? PhotoUrls { get; init; }
    public IReadOnlyList<string>? Features { get; init; }
    public string? SellerType { get; init; } // "dealer" | "private"
    public string? Province { get; init; }
}

/// <summary>
/// LLM-generated listing optimization result.
/// </summary>
public sealed class ListingOptimization
{
    /// <summary>Quality score (0-100). Below 50 = poor, 50-75 = average, 75+ = good.</summary>
    public int PuntajeCalidad { get; init; }

    /// <summary>SEO-optimized title in Spanish.</summary>
    public string TituloOptimizado { get; init; } = string.Empty;

    /// <summary>SEO-optimized description in Spanish.</summary>
    public string DescripcionOptimizada { get; init; } = string.Empty;

    /// <summary>Short WhatsApp-friendly description (≤160 chars).</summary>
    public string DescripcionWhatsApp { get; init; } = string.Empty;

    /// <summary>Meta description for SEO (≤155 chars).</summary>
    public string MetaDescription { get; init; } = string.Empty;

    /// <summary>Suggested SEO tags/keywords.</summary>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>Specific improvement suggestions.</summary>
    public IReadOnlyList<ListingTip> Mejoras { get; init; } = [];

    /// <summary>Photo quality assessment.</summary>
    public PhotoAssessment? EvaluacionFotos { get; init; }

    /// <summary>Competitive advantage summary.</summary>
    public string? VentajaCompetitiva { get; init; }

    /// <summary>Estimated visibility boost if improvements applied (percentage).</summary>
    public int BoostEstimadoPorcentaje { get; init; }

    /// <summary>Whether this result was served from Redis cache (true = no LLM API call was made).</summary>
    public bool FromCache { get; set; }

    /// <summary>The semantic cache key used (make:model:year:trim). Useful for debugging and monitoring.</summary>
    public string? CacheKey { get; set; }
}

/// <summary>
/// A single improvement tip.
/// </summary>
public sealed class ListingTip
{
    public required string Categoria { get; init; } // "titulo", "descripcion", "fotos", "precio", "completitud"
    public required string Prioridad { get; init; } // "alta", "media", "baja"
    public required string Mensaje { get; init; }
    public int ImpactoEstimado { get; init; } // 1-10
}

/// <summary>
/// Photo quality assessment.
/// </summary>
public sealed class PhotoAssessment
{
    public int CantidadFotos { get; init; }
    public int MinimoRecomendado { get; init; } = 8;
    public bool TieneFotoExterior { get; init; }
    public bool TieneFotoInterior { get; init; }
    public bool TieneFotoMotor { get; init; }
    public bool TieneFotoTablero { get; init; }
    public string Veredicto { get; init; } = string.Empty;
}
