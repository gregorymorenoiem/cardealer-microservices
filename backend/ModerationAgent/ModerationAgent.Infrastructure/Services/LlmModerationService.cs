using CarDealer.Shared.LlmGateway.Abstractions;
using CarDealer.Shared.LlmGateway.Services;
using Microsoft.Extensions.Logging;
using ModerationAgent.Domain.Interfaces;
using ModerationAgent.Domain.Models;

namespace ModerationAgent.Infrastructure.Services;

/// <summary>
/// LLM-powered content moderation for the Dominican Republic automotive marketplace.
/// Detects scams, inappropriate content, fake listings, price manipulation, and PII exposure.
/// </summary>
public sealed class LlmModerationService : ILlmModerationService
{
    private readonly ILlmGateway _gateway;
    private readonly ILogger<LlmModerationService> _logger;

    private const string SystemPrompt = """
        Eres un moderador de contenido experto para OKLA, el marketplace automotriz digital de República Dominicana.
        
        Tu trabajo es proteger a compradores y vendedores detectando:
        1. ESTAFAS: Precios irrealmente bajos, solicitud de pagos adelantados, urgencia artificial,
           números de teléfono sospechosos, vehículos que no existen (stock photos).
        2. CONTENIDO INAPROPIADO: Lenguaje ofensivo, discriminación, contenido sexual, amenazas.
        3. INFORMACIÓN FALSA: Kilometraje alterado (clocking), año incorrecto, modelo incorrecto,
           fotos de otro vehículo, claims sin evidencia ("0 accidentes" sin Carfax).
        4. PRECIOS IRREALES: Vehículos de lujo a precios de desgüace, inflación artificial de precios.
        5. SPAM: Publicaciones duplicadas, publicidad encubierta, links externos, SEO stuffing.
        6. PII EXPUESTO: Números de cédula, tarjetas de crédito, direcciones exactas que deben estar protegidas.
        
        ESTAFAS COMUNES EN RD:
        - "Vehículo importado de zona franca" con precio 50% debajo del mercado
        - Solicitar transferencia bancaria antes de ver el vehículo
        - Fotos genéricas de internet en vez de fotos reales
        - Seller en "el exterior" que no puede mostrar el vehículo
        - Precios en USD cuando el mercado local es en DOP
        - Números de WhatsApp con prefijos extranjeros (+1, +44)
        
        RESPONDE EXCLUSIVAMENTE en JSON:
        {
          "accion": "aprobar" | "revisar_manual" | "rechazar" | "suspender_cuenta",
          "confianza": number (0.0-1.0),
          "puntaje_riesgo": number (0-100),
          "violaciones": [{"tipo": "string", "severidad": "baja|media|alta|critica", "descripcion": "string", "ubicacion": "string?", "evidencia": "string?"}],
          "posible_estafa": boolean,
          "indicadores_estafa": ["string"],
          "contiene_pii": boolean,
          "campos_pii": ["string"],
          "contenido_corregido": "string?",
          "explicacion": "string",
          "mensaje_para_usuario": "string?"
        }
        """;

    public LlmModerationService(ILlmGateway gateway, ILogger<LlmModerationService> logger)
    {
        _gateway = gateway;
        _logger = logger;
    }

    public async Task<ModerationVerdict> ModerateContentAsync(ModerationInput input, CancellationToken ct = default)
    {
        // Pre-filter: check for obvious rule-based violations first
        var preFilter = RunRuleBasedChecks(input);
        if (preFilter is not null && preFilter.Accion == "rechazar")
        {
            _logger.LogWarning("ModerationAgent: {ContentId} rejected by rule-based pre-filter", input.ContentId);
            return preFilter;
        }

        var userMessage = BuildUserMessage(input);
        var request = new LlmRequest
        {
            SystemPrompt = SystemPrompt,
            UserMessage = userMessage,
            CallerAgent = "ModerationAgent",
            MaxTokens = 2048,
            Temperature = 0.1, // Low temperature for consistent moderation decisions
            ResponseFormat = "moderation_verdict",
            CacheKey = $"moderation:{input.ContentId}:{input.ContentType}"
        };

        var response = await _gateway.CompleteAsync(request, ct);

        _logger.LogInformation(
            "ModerationAgent: {ContentId} — Provider: {Provider}, Fallback: {Level}",
            input.ContentId, response.Provider, response.FallbackLevel);

        return ParseVerdict(response.Content, input);
    }

    private static ModerationVerdict? RunRuleBasedChecks(ModerationInput input)
    {
        var violations = new List<Violation>();

        // Check for suspicious phone patterns in body
        if (input.Body is not null)
        {
            // WhatsApp with foreign prefix
            if (System.Text.RegularExpressions.Regex.IsMatch(input.Body, @"\+(?!1809|\+1829|\+1849)\d{10,}"))
            {
                violations.Add(new Violation
                {
                    Tipo = "estafa",
                    Severidad = "alta",
                    Descripcion = "Número de teléfono con prefijo no dominicano detectado.",
                    Ubicacion = "body"
                });
            }

            // Credit card pattern
            if (System.Text.RegularExpressions.Regex.IsMatch(input.Body, @"\b\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b"))
            {
                violations.Add(new Violation
                {
                    Tipo = "pii_expuesto",
                    Severidad = "critica",
                    Descripcion = "Posible número de tarjeta de crédito detectado.",
                    Ubicacion = "body"
                });
            }

            // Cédula pattern (RD: 001-0000000-0)
            if (System.Text.RegularExpressions.Regex.IsMatch(input.Body, @"\b\d{3}-?\d{7}-?\d\b"))
            {
                violations.Add(new Violation
                {
                    Tipo = "pii_expuesto",
                    Severidad = "alta",
                    Descripcion = "Posible número de cédula dominicana detectado.",
                    Ubicacion = "body"
                });
            }
        }

        // Price sanity check (vehicle can't cost less than DOP 50,000 ≈ $850 USD)
        if (input.Price.HasValue && input.Currency == "DOP" && input.Price < 50_000 && input.ContentType == "listing")
        {
            violations.Add(new Violation
            {
                Tipo = "precio_irreal",
                Severidad = "alta",
                Descripcion = $"Precio irreal: {input.Price:N0} DOP. Mínimo esperado: 50,000 DOP.",
                Evidencia = input.Price.ToString()
            });
        }

        if (violations.Count == 0) return null;

        var hasCritical = violations.Any(v => v.Severidad == "critica");
        return new ModerationVerdict
        {
            Accion = hasCritical ? "rechazar" : "revisar_manual",
            Confianza = 0.9,
            PuntajeRiesgo = hasCritical ? 95 : 70,
            Violaciones = violations,
            PosibleEstafa = violations.Any(v => v.Tipo == "estafa"),
            IndicadoresEstafa = violations.Where(v => v.Tipo == "estafa").Select(v => v.Descripcion).ToList(),
            ContienePii = violations.Any(v => v.Tipo == "pii_expuesto"),
            CamposPii = violations.Where(v => v.Tipo == "pii_expuesto").Select(v => v.Descripcion).ToList(),
            Explicacion = "Detectado por filtro automático pre-LLM."
        };
    }

    private static string BuildUserMessage(ModerationInput input)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"## Contenido a moderar (Tipo: {input.ContentType}):");
        if (input.Title is not null) sb.AppendLine($"- Título: {input.Title}");
        if (input.Body is not null) sb.AppendLine($"- Cuerpo: {input.Body}");
        if (input.SellerName is not null) sb.AppendLine($"- Vendedor: {input.SellerName}");
        if (input.Price.HasValue) sb.AppendLine($"- Precio: {input.Price:N0} {input.Currency}");
        if (input.PhotoCount.HasValue) sb.AppendLine($"- Fotos: {input.PhotoCount}");
        if (input.ImageLabels?.Count > 0) sb.AppendLine($"- Etiquetas de imagen: {string.Join(", ", input.ImageLabels)}");
        if (input.IpCountry is not null) sb.AppendLine($"- País IP: {input.IpCountry}");
        return sb.ToString();
    }

    private ModerationVerdict ParseVerdict(string content, ModerationInput input)
    {
        try
        {
            var parsed = LlmResponseParser.ParseJsonResponse<ModerationVerdictJson>(content);
            if (parsed is not null)
            {
                return new ModerationVerdict
                {
                    Accion = parsed.Accion ?? "revisar_manual",
                    Confianza = parsed.Confianza,
                    PuntajeRiesgo = parsed.PuntajeRiesgo,
                    Violaciones = parsed.Violaciones?.Select(v => new Violation
                    {
                        Tipo = v.Tipo ?? "desconocido",
                        Severidad = v.Severidad ?? "media",
                        Descripcion = v.Descripcion ?? "",
                        Ubicacion = v.Ubicacion,
                        Evidencia = v.Evidencia
                    }).ToList() ?? [],
                    PosibleEstafa = parsed.PosibleEstafa,
                    IndicadoresEstafa = parsed.IndicadoresEstafa ?? [],
                    ContienePii = parsed.ContienePii,
                    CamposPii = parsed.CamposPii ?? [],
                    ContenidoCorregido = parsed.ContenidoCorregido,
                    Explicacion = parsed.Explicacion ?? "Sin explicación.",
                    MensajeParaUsuario = parsed.MensajeParaUsuario
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ModerationAgent: Failed to parse LLM response for {ContentId}", input.ContentId);
        }

        // Safe fallback: send to manual review
        return new ModerationVerdict
        {
            Accion = "revisar_manual",
            Confianza = 0.2,
            PuntajeRiesgo = 50,
            Violaciones = [],
            PosibleEstafa = false,
            IndicadoresEstafa = [],
            ContienePii = false,
            CamposPii = [],
            Explicacion = "No se pudo analizar con LLM. Enviado a revisión manual."
        };
    }

    private sealed class ModerationVerdictJson
    {
        public string? Accion { get; set; }
        public double Confianza { get; set; }
        public int PuntajeRiesgo { get; set; }
        public List<ViolationJson>? Violaciones { get; set; }
        public bool PosibleEstafa { get; set; }
        public List<string>? IndicadoresEstafa { get; set; }
        public bool ContienePii { get; set; }
        public List<string>? CamposPii { get; set; }
        public string? ContenidoCorregido { get; set; }
        public string? Explicacion { get; set; }
        public string? MensajeParaUsuario { get; set; }
    }

    private sealed class ViolationJson
    {
        public string? Tipo { get; set; }
        public string? Severidad { get; set; }
        public string? Descripcion { get; set; }
        public string? Ubicacion { get; set; }
        public string? Evidencia { get; set; }
    }
}
