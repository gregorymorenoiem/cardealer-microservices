using System.Text.Json;
using CarDealer.Shared.LlmGateway.Abstractions;
using CarDealer.Shared.LlmGateway.Services;
using Microsoft.Extensions.Logging;
using PricingAgent.Domain.Interfaces;
using PricingAgent.Domain.Models;

namespace PricingAgent.Infrastructure.Services;

/// <summary>
/// LLM-powered pricing service using the shared LlmGateway cascade (Claude → Gemini → Llama).
/// Specialized for the Dominican Republic automotive market.
/// </summary>
public sealed class LlmPricingService : ILlmPricingService
{
    private readonly ILlmGateway _gateway;
    private readonly ILogger<LlmPricingService> _logger;

    private const string SystemPrompt = """
        Eres un experto tasador de vehículos especializado en el mercado automotriz de República Dominicana.
        
        CONTEXTO:
        - Moneda principal: Peso Dominicano (DOP). Tasa aproximada: 1 USD = 58.50 DOP.
        - Plataforma: OKLA (marketplace digital automotriz dominicano).
        - Competidores: SuperCarros, Facebook Marketplace.
        - Factores locales: importación desde EEUU/Corea, impuestos DGII (ITF, matrícula), 
          seguros obligatorios, costo de mantenimiento local, disponibilidad de repuestos.
        
        INSTRUCCIONES:
        1. Analiza el vehículo proporcionado y los comparables del mercado.
        2. Genera un rango de precio justo en DOP considerando:
           - Año, marca, modelo, trim, kilometraje
           - Condición (nuevo/usado/certificado)
           - Provincia (Santo Domingo vs provincias tiene diferencia de 5-15%)
           - Tipo de combustible (gasolina vs diésel - diésel suele valer más en RD)
           - Transmisión (automática suele valer 10-20% más que manual)
           - Historial de precios y tendencia del mercado
        3. Explica tu razonamiento en español dominicano profesional.
        4. Provee recomendaciones tanto para compradores como vendedores.
        
        RESPONDE EXCLUSIVAMENTE en JSON con esta estructura:
        {
          "precio_minimo_dop": number,
          "precio_maximo_dop": number,
          "precio_sugerido_dop": number,
          "precio_sugerido_usd": number,
          "confianza": number (0.0-1.0),
          "nivel_precio": "bajo" | "justo" | "alto" | "muy_alto",
          "desviacion_porcentaje": number,
          "explicacion": "string",
          "factores_positivos": ["string"],
          "factores_negativos": ["string"],
          "recomendacion_comprador": "string",
          "recomendacion_vendedor": "string",
          "dias_estimados_venta": number,
          "tendencia_mercado": "subiendo" | "estable" | "bajando"
        }
        """;

    public LlmPricingService(ILlmGateway gateway, ILogger<LlmPricingService> logger)
    {
        _gateway = gateway;
        _logger = logger;
    }

    public async Task<PricingAnalysis> AnalyzePriceAsync(
        VehiclePricingInput vehicle,
        IReadOnlyList<MarketComparable> comparables,
        CancellationToken ct = default)
    {
        var userMessage = BuildUserMessage(vehicle, comparables);

        var request = new LlmRequest
        {
            SystemPrompt = SystemPrompt,
            UserMessage = userMessage,
            CallerAgent = "PricingAgent",
            MaxTokens = 2048,
            Temperature = 0.2,
            ResponseFormat = "pricing_analysis",
            CacheKey = $"pricing:{vehicle.Make}:{vehicle.Model}:{vehicle.Year}:{vehicle.Mileage}:{vehicle.Province}"
        };

        var response = await _gateway.CompleteAsync(request, ct);

        _logger.LogInformation(
            "PricingAgent: {Make} {Model} {Year} — Provider: {Provider}, Fallback: {Level}, Latency: {Latency}ms",
            vehicle.Make, vehicle.Model, vehicle.Year,
            response.Provider, response.FallbackLevel, response.TotalLatency.TotalMilliseconds);

        return ParseAnalysis(response.Content, vehicle);
    }

    private static string BuildUserMessage(VehiclePricingInput vehicle, IReadOnlyList<MarketComparable> comparables)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("## Vehículo a tasar:");
        sb.AppendLine($"- Marca: {vehicle.Make}");
        sb.AppendLine($"- Modelo: {vehicle.Model}");
        sb.AppendLine($"- Año: {vehicle.Year}");
        if (vehicle.Trim is not null) sb.AppendLine($"- Trim: {vehicle.Trim}");
        if (vehicle.Mileage.HasValue) sb.AppendLine($"- Kilometraje: {vehicle.Mileage:N0} km");
        if (vehicle.FuelType is not null) sb.AppendLine($"- Combustible: {vehicle.FuelType}");
        if (vehicle.Transmission is not null) sb.AppendLine($"- Transmisión: {vehicle.Transmission}");
        if (vehicle.Condition is not null) sb.AppendLine($"- Condición: {vehicle.Condition}");
        if (vehicle.Province is not null) sb.AppendLine($"- Provincia: {vehicle.Province}");
        if (vehicle.AskingPrice.HasValue)
            sb.AppendLine($"- Precio pedido: {vehicle.AskingPrice:N0} {vehicle.Currency}");
        if (vehicle.Features?.Count > 0)
            sb.AppendLine($"- Extras: {string.Join(", ", vehicle.Features)}");

        if (comparables.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Comparables del mercado:");
            foreach (var comp in comparables.Take(10))
            {
                sb.AppendLine($"- {comp.Make} {comp.Model} {comp.Year}: {comp.Price:N0} {comp.Currency} " +
                              $"({comp.Mileage:N0} km, {comp.Condition}, fuente: {comp.Source}, " +
                              $"días en mercado: {comp.DaysOnMarket})");
            }
        }

        return sb.ToString();
    }

    private PricingAnalysis ParseAnalysis(string content, VehiclePricingInput vehicle)
    {
        try
        {
            var parsed = LlmResponseParser.ParseJsonResponse<PricingAnalysisJson>(content);
            if (parsed is not null)
            {
                return new PricingAnalysis
                {
                    PrecioMinimoDop = parsed.PrecioMinimoDop,
                    PrecioMaximoDop = parsed.PrecioMaximoDop,
                    PrecioSugeridoDop = parsed.PrecioSugeridoDop,
                    PrecioSugeridoUsd = parsed.PrecioSugeridoUsd,
                    Confianza = parsed.Confianza,
                    NivelPrecio = parsed.NivelPrecio ?? "justo",
                    DesviacionPorcentaje = parsed.DesviacionPorcentaje,
                    Explicacion = parsed.Explicacion ?? "Análisis no disponible.",
                    FactoresPositivos = parsed.FactoresPositivos ?? [],
                    FactoresNegativos = parsed.FactoresNegativos ?? [],
                    RecomendacionComprador = parsed.RecomendacionComprador,
                    RecomendacionVendedor = parsed.RecomendacionVendedor,
                    DiasEstimadosVenta = parsed.DiasEstimadosVenta,
                    TendenciaMercado = parsed.TendenciaMercado ?? "estable"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PricingAgent: Failed to parse LLM response for {Make} {Model}", vehicle.Make, vehicle.Model);
        }

        // Fallback: basic heuristic pricing
        return GetFallbackAnalysis(vehicle);
    }

    private static PricingAnalysis GetFallbackAnalysis(VehiclePricingInput vehicle)
    {
        // Basic depreciation model: 15% per year from average new car price
        var basePrice = 1_500_000m; // Average new car price in DOP
        var age = DateTime.UtcNow.Year - vehicle.Year;
        var depreciation = Math.Pow(0.85, Math.Min(age, 15));
        var estimatedPrice = basePrice * (decimal)depreciation;

        return new PricingAnalysis
        {
            PrecioMinimoDop = estimatedPrice * 0.85m,
            PrecioMaximoDop = estimatedPrice * 1.15m,
            PrecioSugeridoDop = estimatedPrice,
            PrecioSugeridoUsd = Math.Round(estimatedPrice / 58.50m, 2),
            Confianza = 0.3,
            NivelPrecio = "justo",
            Explicacion = "Estimación basada en depreciación estándar. No se pudo obtener análisis detallado del LLM.",
            FactoresPositivos = [],
            FactoresNegativos = ["Análisis limitado — basado en heurística"],
            DiasEstimadosVenta = 30,
            TendenciaMercado = "estable"
        };
    }

    /// <summary>JSON DTO matching the LLM snake_case output.</summary>
    private sealed class PricingAnalysisJson
    {
        public decimal PrecioMinimoDop { get; set; }
        public decimal PrecioMaximoDop { get; set; }
        public decimal PrecioSugeridoDop { get; set; }
        public decimal PrecioSugeridoUsd { get; set; }
        public double Confianza { get; set; }
        public string? NivelPrecio { get; set; }
        public double DesviacionPorcentaje { get; set; }
        public string? Explicacion { get; set; }
        public List<string>? FactoresPositivos { get; set; }
        public List<string>? FactoresNegativos { get; set; }
        public string? RecomendacionComprador { get; set; }
        public string? RecomendacionVendedor { get; set; }
        public int DiasEstimadosVenta { get; set; }
        public string? TendenciaMercado { get; set; }
    }
}
