using CarDealer.Shared.LlmGateway.Abstractions;
using CarDealer.Shared.LlmGateway.Services;
using Microsoft.Extensions.Logging;
using ListingAgent.Domain.Interfaces;
using ListingAgent.Domain.Models;
using ListingAgent.Domain.Utilities;

namespace ListingAgent.Infrastructure.Services;

/// <summary>
/// LLM-powered listing optimization using shared LlmGateway cascade.
/// Generates SEO titles, descriptions, quality scores, and improvement tips for the DR market.
/// </summary>
public sealed class LlmListingService : ILlmListingService
{
    private readonly ILlmGateway _gateway;
    private readonly ILogger<LlmListingService> _logger;
    private readonly IListingCacheMetrics _cacheMetrics;

    private const string SystemPrompt = """
        Eres un experto en marketing digital automotriz y SEO para el mercado de República Dominicana.
        Trabajas para OKLA, el marketplace automotriz digital líder en RD.
        
        Tu objetivo es optimizar listados de vehículos para maximizar:
        1. Visibilidad en búsquedas (SEO dentro de OKLA y Google)
        2. Tasa de clics (CTR) — títulos atractivos y descripción persuasiva
        3. Tasa de contacto — descripciones que generan confianza y urgencia
        4. Compartibilidad en WhatsApp y redes sociales
        
        REGLAS:
        - Escribe en español dominicano profesional (no coloquial extremo)
        - Incluye keywords relevantes: marca, modelo, año, ciudad, "en venta"
        - Destaca puntos fuertes del vehículo primero
        - Menciona financiamiento disponible si aplica
        - No inventes features que no estén en los datos
        - Sé honesto pero persuasivo
        
        RESPONDE EXCLUSIVAMENTE en JSON:
        {
          "puntaje_calidad": number (0-100),
          "titulo_optimizado": "string (max 80 chars)",
          "descripcion_optimizada": "string (300-500 chars)",
          "descripcion_whatsapp": "string (max 160 chars)",
          "meta_description": "string (max 155 chars)",
          "tags": ["string"],
          "mejoras": [{"categoria": "titulo|descripcion|fotos|precio|completitud", "prioridad": "alta|media|baja", "mensaje": "string", "impacto_estimado": 1-10}],
          "ventaja_competitiva": "string",
          "boost_estimado_porcentaje": number (0-100)
        }
        """;

    public LlmListingService(ILlmGateway gateway, ILogger<LlmListingService> logger, IListingCacheMetrics cacheMetrics)
    {
        _gateway = gateway;
        _logger = logger;
        _cacheMetrics = cacheMetrics;
    }

    /// <summary>
    /// Builds a semantic cache key based on Make/Model/Year/Trim (normalized, case-insensitive).
    /// Delegates to the domain utility <see cref="ListingCacheKeyBuilder"/> for testability.
    /// </summary>
    internal static string BuildSemanticCacheKey(ListingInput listing) =>
        ListingCacheKeyBuilder.Build(listing.Make, listing.Model, listing.Year, listing.Trim);

    public async Task<ListingOptimization> OptimizeListingAsync(ListingInput listing, CancellationToken ct = default)
    {
        var userMessage = BuildUserMessage(listing);
        var semanticKey = BuildSemanticCacheKey(listing);

        var request = new LlmRequest
        {
            SystemPrompt = SystemPrompt,
            UserMessage = userMessage,
            CallerAgent = "ListingAgent",
            MaxTokens = 2048,
            Temperature = 0.4,
            ResponseFormat = "listing_optimization",
            // Semantic key: shared across ALL vehicles of same Make/Model/Year/Trim.
            // This is the key fix: a new VehicleId of Toyota Camry 2022 LE will hit
            // the same cache entry as any other Toyota Camry 2022 LE processed before.
            CacheKey = semanticKey,
            // 30-day TTL for listing optimizations — vehicle model descriptions rarely change.
            CacheTtlOverride = TimeSpan.FromDays(30)
        };

        var response = await _gateway.CompleteAsync(request, ct);

        // Record hit/miss for cache hit rate monitoring (target: ≥50% after first month)
        if (response.FromCache)
            _ = _cacheMetrics.RecordHitAsync(semanticKey, ct);
        else
            _ = _cacheMetrics.RecordMissAsync(semanticKey, ct);

        _logger.LogInformation(
            "ListingAgent: {VehicleId} — Provider: {Provider}, Fallback: {Level}, Latency: {Latency}ms, FromCache: {FromCache}, CacheKey: {CacheKey}",
            listing.VehicleId, response.Provider, response.FallbackLevel,
            response.TotalLatency.TotalMilliseconds, response.FromCache, semanticKey);

        var result = ParseOptimization(response.Content, listing);
        result.FromCache = response.FromCache;
        result.CacheKey = semanticKey;
        return result;
    }

    private static string BuildUserMessage(ListingInput listing)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("## Listado actual:");
        sb.AppendLine($"- Título: {listing.Title}");
        if (!string.IsNullOrEmpty(listing.Description))
            sb.AppendLine($"- Descripción: {listing.Description}");
        sb.AppendLine($"- Vehículo: {listing.Year} {listing.Make} {listing.Model}");
        if (listing.Trim is not null) sb.AppendLine($"- Trim: {listing.Trim}");
        if (listing.Price.HasValue) sb.AppendLine($"- Precio: {listing.Price:N0} {listing.Currency}");
        if (listing.Mileage.HasValue) sb.AppendLine($"- Kilometraje: {listing.Mileage:N0} km");
        if (listing.FuelType is not null) sb.AppendLine($"- Combustible: {listing.FuelType}");
        if (listing.Transmission is not null) sb.AppendLine($"- Transmisión: {listing.Transmission}");
        if (listing.Condition is not null) sb.AppendLine($"- Condición: {listing.Condition}");
        sb.AppendLine($"- Fotos: {listing.PhotoCount}");
        if (listing.Features?.Count > 0) sb.AppendLine($"- Features: {string.Join(", ", listing.Features)}");
        sb.AppendLine($"- Tipo vendedor: {listing.SellerType ?? "N/A"}");
        if (listing.Province is not null) sb.AppendLine($"- Provincia: {listing.Province}");
        return sb.ToString();
    }

    private ListingOptimization ParseOptimization(string content, ListingInput listing)
    {
        try
        {
            var parsed = LlmResponseParser.ParseJsonResponse<ListingOptimizationJson>(content);
            if (parsed is not null)
            {
                return new ListingOptimization
                {
                    PuntajeCalidad = parsed.PuntajeCalidad,
                    TituloOptimizado = parsed.TituloOptimizado ?? listing.Title,
                    DescripcionOptimizada = parsed.DescripcionOptimizada ?? listing.Description ?? "",
                    DescripcionWhatsApp = parsed.DescripcionWhatsApp ?? "",
                    MetaDescription = parsed.MetaDescription ?? "",
                    Tags = parsed.Tags ?? [],
                    Mejoras = parsed.Mejoras?.Select(m => new ListingTip
                    {
                        Categoria = m.Categoria ?? "general",
                        Prioridad = m.Prioridad ?? "media",
                        Mensaje = m.Mensaje ?? "",
                        ImpactoEstimado = m.ImpactoEstimado
                    }).ToList() ?? [],
                    VentajaCompetitiva = parsed.VentajaCompetitiva,
                    BoostEstimadoPorcentaje = parsed.BoostEstimadoPorcentaje
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ListingAgent: Failed to parse LLM response for {VehicleId}", listing.VehicleId);
        }

        return GetFallbackOptimization(listing);
    }

    private static ListingOptimization GetFallbackOptimization(ListingInput listing)
    {
        var tips = new List<ListingTip>();

        if (listing.PhotoCount < 5)
            tips.Add(new ListingTip { Categoria = "fotos", Prioridad = "alta", Mensaje = "Sube al menos 8 fotos (exterior, interior, motor, tablero).", ImpactoEstimado = 9 });

        if (string.IsNullOrEmpty(listing.Description) || listing.Description.Length < 100)
            tips.Add(new ListingTip { Categoria = "descripcion", Prioridad = "alta", Mensaje = "Agrega una descripción detallada de al menos 200 caracteres.", ImpactoEstimado = 8 });

        if (listing.Title.Length < 20)
            tips.Add(new ListingTip { Categoria = "titulo", Prioridad = "media", Mensaje = "El título debe incluir año, marca, modelo y un diferenciador.", ImpactoEstimado = 7 });

        return new ListingOptimization
        {
            PuntajeCalidad = 40,
            TituloOptimizado = $"{listing.Year} {listing.Make} {listing.Model} en Venta - OKLA",
            DescripcionOptimizada = listing.Description ?? $"Vehículo {listing.Year} {listing.Make} {listing.Model} disponible en OKLA.",
            DescripcionWhatsApp = $"🚗 {listing.Year} {listing.Make} {listing.Model} - Ver en OKLA",
            MetaDescription = $"Compra {listing.Year} {listing.Make} {listing.Model} en República Dominicana. Precio y fotos en OKLA.",
            Tags = [listing.Make, listing.Model, listing.Year.ToString(), "en venta", "república dominicana"],
            Mejoras = tips,
            BoostEstimadoPorcentaje = 0
        };
    }

    private sealed class ListingOptimizationJson
    {
        public int PuntajeCalidad { get; set; }
        public string? TituloOptimizado { get; set; }
        public string? DescripcionOptimizada { get; set; }
        public string? DescripcionWhatsApp { get; set; }
        public string? MetaDescription { get; set; }
        public List<string>? Tags { get; set; }
        public List<ListingTipJson>? Mejoras { get; set; }
        public string? VentajaCompetitiva { get; set; }
        public int BoostEstimadoPorcentaje { get; set; }
    }

    private sealed class ListingTipJson
    {
        public string? Categoria { get; set; }
        public string? Prioridad { get; set; }
        public string? Mensaje { get; set; }
        public int ImpactoEstimado { get; set; }
    }
}
