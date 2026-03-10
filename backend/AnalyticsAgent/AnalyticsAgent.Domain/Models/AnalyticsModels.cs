// =============================================================================
// AnalyticsAgent — Domain Models
// LLM-powered business analytics and insights for OKLA marketplace.
// Transforms raw data into natural language insights + actionable recommendations.
// =============================================================================

namespace AnalyticsAgent.Domain.Models;

/// <summary>
/// Raw metrics input for analytics generation.
/// </summary>
public sealed class AnalyticsInput
{
    public required string ReportType { get; init; } // "daily_summary", "dealer_performance", "market_trends", "cost_analysis", "user_behavior"
    public required string Period { get; init; } // "today", "week", "month", "quarter"
    public string? DealerId { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public required Dictionary<string, object> Metrics { get; init; }
}

/// <summary>
/// LLM-generated analytics insight.
/// </summary>
public sealed class AnalyticsInsight
{
    /// <summary>Report title in Spanish.</summary>
    public string Titulo { get; init; } = string.Empty;

    /// <summary>Executive summary (2-3 sentences).</summary>
    public string ResumenEjecutivo { get; init; } = string.Empty;

    /// <summary>Key metrics with context and change indicators.</summary>
    public IReadOnlyList<MetricInsight> MetricasClave { get; init; } = [];

    /// <summary>Detected trends.</summary>
    public IReadOnlyList<TrendInsight> Tendencias { get; init; } = [];

    /// <summary>Actionable recommendations.</summary>
    public IReadOnlyList<Recommendation> Recomendaciones { get; init; } = [];

    /// <summary>Anomalies or alerts detected in the data.</summary>
    public IReadOnlyList<Anomaly> Anomalias { get; init; } = [];

    /// <summary>Competitive context (vs SuperCarros, Facebook Marketplace).</summary>
    public string? ContextoCompetitivo { get; init; }

    /// <summary>Forecasted metrics for next period.</summary>
    public string? Pronostico { get; init; }
}

public sealed class MetricInsight
{
    public required string Nombre { get; init; }
    public required string Valor { get; init; }
    public required string Cambio { get; init; } // "+15%", "-3%", "sin cambio"
    public required string Interpretacion { get; init; }
    public string Tendencia { get; init; } = "estable"; // "subiendo", "estable", "bajando"
}

public sealed class TrendInsight
{
    public required string Titulo { get; init; }
    public required string Descripcion { get; init; }
    public required string Impacto { get; init; } // "positivo", "neutro", "negativo"
    public string? RecomendacionAsociada { get; init; }
}

public sealed class Recommendation
{
    public required string Titulo { get; init; }
    public required string Descripcion { get; init; }
    public required string Prioridad { get; init; } // "alta", "media", "baja"
    public required string ImpactoEsperado { get; init; }
    public string? MetricaObjetivo { get; init; }
}

public sealed class Anomaly
{
    public required string Tipo { get; init; } // "spike", "drop", "outlier", "pattern_break"
    public required string Descripcion { get; init; }
    public required string Severidad { get; init; } // "info", "warning", "critical"
    public string? MetricaAfectada { get; init; }
}
