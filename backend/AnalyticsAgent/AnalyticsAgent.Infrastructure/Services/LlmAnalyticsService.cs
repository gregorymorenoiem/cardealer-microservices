using System.Text.Json;
using CarDealer.Shared.LlmGateway.Abstractions;
using CarDealer.Shared.LlmGateway.Services;
using Microsoft.Extensions.Logging;
using AnalyticsAgent.Domain.Interfaces;
using AnalyticsAgent.Domain.Models;

namespace AnalyticsAgent.Infrastructure.Services;

/// <summary>
/// LLM-powered analytics engine for OKLA marketplace.
/// Transforms raw business metrics into natural language insights + actionable recommendations.
/// </summary>
public sealed class LlmAnalyticsService : ILlmAnalyticsService
{
    private readonly ILlmGateway _gateway;
    private readonly ILogger<LlmAnalyticsService> _logger;

    private const string SystemPrompt = """
        Eres un analista de datos senior especializado en marketplaces automotrices en el Caribe.
        Trabajas para OKLA, el marketplace automotriz digital líder en República Dominicana.
        
        CONTEXTO DEL NEGOCIO:
        - Competidores: SuperCarros (líder actual), Facebook Marketplace (volumen alto, baja calidad)
        - Modelo de ingresos: Suscripciones (Libre $0, Visible $29, Pro $89, Elite $199 DOP/mes)
        - Métricas clave: MRR, Churn, CAC, LTV, Conversión Free→Paid, DAU/MAU
        - Mercado: ~3,000 dealers activos en RD, ~50,000 listados de vehículos
        
        INSTRUCCIONES:
        1. Analiza los datos proporcionados e identifica patrones, tendencias y anomalías.
        2. Genera un resumen ejecutivo conciso (2-3 oraciones).
        3. Provee insights accionables con prioridad y impacto esperado.
        4. Detecta anomalías (spikes, drops, outliers) y explica posibles causas.
        5. Compara con benchmarks de la industria cuando sea posible.
        6. Escribe en español dominicano profesional.
        
        RESPONDE EXCLUSIVAMENTE en JSON:
        {
          "titulo": "string",
          "resumen_ejecutivo": "string",
          "metricas_clave": [{"nombre": "string", "valor": "string", "cambio": "string", "interpretacion": "string", "tendencia": "subiendo|estable|bajando"}],
          "tendencias": [{"titulo": "string", "descripcion": "string", "impacto": "positivo|neutro|negativo", "recomendacion_asociada": "string?"}],
          "recomendaciones": [{"titulo": "string", "descripcion": "string", "prioridad": "alta|media|baja", "impacto_esperado": "string", "metrica_objetivo": "string?"}],
          "anomalias": [{"tipo": "spike|drop|outlier|pattern_break", "descripcion": "string", "severidad": "info|warning|critical", "metrica_afectada": "string?"}],
          "contexto_competitivo": "string?",
          "pronostico": "string?"
        }
        """;

    public LlmAnalyticsService(ILlmGateway gateway, ILogger<LlmAnalyticsService> logger)
    {
        _gateway = gateway;
        _logger = logger;
    }

    public async Task<AnalyticsInsight> GenerateInsightsAsync(AnalyticsInput input, CancellationToken ct = default)
    {
        var userMessage = BuildUserMessage(input);

        var request = new LlmRequest
        {
            SystemPrompt = SystemPrompt,
            UserMessage = userMessage,
            CallerAgent = "AnalyticsAgent",
            MaxTokens = 4096,
            Temperature = 0.3,
            ResponseFormat = "analytics_insight",
            CacheKey = $"analytics:{input.ReportType}:{input.Period}:{input.DealerId ?? "global"}"
        };

        var response = await _gateway.CompleteAsync(request, ct);

        _logger.LogInformation(
            "AnalyticsAgent: {ReportType}/{Period} — Provider: {Provider}, Fallback: {Level}",
            input.ReportType, input.Period, response.Provider, response.FallbackLevel);

        return ParseInsight(response.Content, input);
    }

    private static string BuildUserMessage(AnalyticsInput input)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"## Reporte: {input.ReportType} — Período: {input.Period}");
        if (input.DealerId is not null) sb.AppendLine($"- Dealer: {input.DealerId}");
        if (input.StartDate.HasValue) sb.AppendLine($"- Desde: {input.StartDate:yyyy-MM-dd}");
        if (input.EndDate.HasValue) sb.AppendLine($"- Hasta: {input.EndDate:yyyy-MM-dd}");

        sb.AppendLine();
        sb.AppendLine("## Métricas:");
        foreach (var (key, value) in input.Metrics)
        {
            sb.AppendLine($"- {key}: {value}");
        }

        return sb.ToString();
    }

    private AnalyticsInsight ParseInsight(string content, AnalyticsInput input)
    {
        try
        {
            var parsed = LlmResponseParser.ParseJsonResponse<AnalyticsInsightJson>(content);
            if (parsed is not null)
            {
                return new AnalyticsInsight
                {
                    Titulo = parsed.Titulo ?? $"Reporte {input.ReportType}",
                    ResumenEjecutivo = parsed.ResumenEjecutivo ?? "Sin resumen disponible.",
                    MetricasClave = parsed.MetricasClave?.Select(m => new MetricInsight
                    {
                        Nombre = m.Nombre ?? "",
                        Valor = m.Valor ?? "N/A",
                        Cambio = m.Cambio ?? "sin cambio",
                        Interpretacion = m.Interpretacion ?? "",
                        Tendencia = m.Tendencia ?? "estable"
                    }).ToList() ?? [],
                    Tendencias = parsed.Tendencias?.Select(t => new TrendInsight
                    {
                        Titulo = t.Titulo ?? "",
                        Descripcion = t.Descripcion ?? "",
                        Impacto = t.Impacto ?? "neutro",
                        RecomendacionAsociada = t.RecomendacionAsociada
                    }).ToList() ?? [],
                    Recomendaciones = parsed.Recomendaciones?.Select(r => new Recommendation
                    {
                        Titulo = r.Titulo ?? "",
                        Descripcion = r.Descripcion ?? "",
                        Prioridad = r.Prioridad ?? "media",
                        ImpactoEsperado = r.ImpactoEsperado ?? "",
                        MetricaObjetivo = r.MetricaObjetivo
                    }).ToList() ?? [],
                    Anomalias = parsed.Anomalias?.Select(a => new Anomaly
                    {
                        Tipo = a.Tipo ?? "outlier",
                        Descripcion = a.Descripcion ?? "",
                        Severidad = a.Severidad ?? "info",
                        MetricaAfectada = a.MetricaAfectada
                    }).ToList() ?? [],
                    ContextoCompetitivo = parsed.ContextoCompetitivo,
                    Pronostico = parsed.Pronostico
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AnalyticsAgent: Failed to parse LLM response for {ReportType}", input.ReportType);
        }

        return new AnalyticsInsight
        {
            Titulo = $"Reporte {input.ReportType} — {input.Period}",
            ResumenEjecutivo = "No se pudo generar análisis con LLM. Los datos crudos están disponibles.",
            MetricasClave = [],
            Tendencias = [],
            Recomendaciones = [],
            Anomalias = []
        };
    }

    // JSON DTOs matching snake_case LLM output
    private sealed class AnalyticsInsightJson
    {
        public string? Titulo { get; set; }
        public string? ResumenEjecutivo { get; set; }
        public List<MetricInsightJson>? MetricasClave { get; set; }
        public List<TrendInsightJson>? Tendencias { get; set; }
        public List<RecommendationJson>? Recomendaciones { get; set; }
        public List<AnomalyJson>? Anomalias { get; set; }
        public string? ContextoCompetitivo { get; set; }
        public string? Pronostico { get; set; }
    }

    private sealed class MetricInsightJson
    {
        public string? Nombre { get; set; }
        public string? Valor { get; set; }
        public string? Cambio { get; set; }
        public string? Interpretacion { get; set; }
        public string? Tendencia { get; set; }
    }

    private sealed class TrendInsightJson
    {
        public string? Titulo { get; set; }
        public string? Descripcion { get; set; }
        public string? Impacto { get; set; }
        public string? RecomendacionAsociada { get; set; }
    }

    private sealed class RecommendationJson
    {
        public string? Titulo { get; set; }
        public string? Descripcion { get; set; }
        public string? Prioridad { get; set; }
        public string? ImpactoEsperado { get; set; }
        public string? MetricaObjetivo { get; set; }
    }

    private sealed class AnomalyJson
    {
        public string? Tipo { get; set; }
        public string? Descripcion { get; set; }
        public string? Severidad { get; set; }
        public string? MetricaAfectada { get; set; }
    }
}
