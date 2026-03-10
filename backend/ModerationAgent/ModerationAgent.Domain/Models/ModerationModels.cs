// =============================================================================
// ModerationAgent — Domain Models
// LLM-powered content moderation for OKLA marketplace.
// Detects scams, inappropriate content, fake listings, policy violations.
// =============================================================================

namespace ModerationAgent.Domain.Models;

/// <summary>
/// Content to be moderated.
/// </summary>
public sealed class ModerationInput
{
    public required string ContentId { get; init; }
    public required string ContentType { get; init; } // "listing", "review", "message", "profile"
    public string? Title { get; init; }
    public string? Body { get; init; }
    public string? SellerName { get; init; }
    public decimal? Price { get; init; }
    public string? Currency { get; init; }
    public int? PhotoCount { get; init; }
    public IReadOnlyList<string>? ImageLabels { get; init; } // From image recognition
    public string? UserHistoryJson { get; init; } // Previous violations
    public string? IpCountry { get; init; }
}

/// <summary>
/// Moderation verdict from LLM analysis.
/// </summary>
public sealed class ModerationVerdict
{
    /// <summary>Overall action: "aprobar", "revisar_manual", "rechazar", "suspender_cuenta".</summary>
    public required string Accion { get; init; }

    /// <summary>Confidence in the verdict (0.0-1.0).</summary>
    public double Confianza { get; init; }

    /// <summary>Risk score (0-100). Above 70 = high risk.</summary>
    public int PuntajeRiesgo { get; init; }

    /// <summary>Detected violations.</summary>
    public IReadOnlyList<Violation> Violaciones { get; init; } = [];

    /// <summary>Whether the content is likely a scam.</summary>
    public bool PosibleEstafa { get; init; }

    /// <summary>Scam indicators detected.</summary>
    public IReadOnlyList<string> IndicadoresEstafa { get; init; } = [];

    /// <summary>Whether the content contains PII that should be redacted.</summary>
    public bool ContienePii { get; init; }

    /// <summary>PII fields detected (phone, email, etc.).</summary>
    public IReadOnlyList<string> CamposPii { get; init; } = [];

    /// <summary>Auto-corrected version of the content (if applicable).</summary>
    public string? ContenidoCorregido { get; init; }

    /// <summary>Human-readable explanation in Spanish.</summary>
    public string Explicacion { get; init; } = string.Empty;

    /// <summary>Recommended notification to user.</summary>
    public string? MensajeParaUsuario { get; init; }
}

/// <summary>
/// A specific policy violation detected.
/// </summary>
public sealed class Violation
{
    public required string Tipo { get; init; } // "contenido_sexual", "spam", "estafa", "info_falsa", "precio_irreal", "ofensivo", "pii_expuesto"
    public required string Severidad { get; init; } // "baja", "media", "alta", "critica"
    public required string Descripcion { get; init; }
    public string? Ubicacion { get; init; } // Where in the content
    public string? Evidencia { get; init; } // The specific text/element
}
