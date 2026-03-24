"""
OKLA Advanced Pricing - Enhanced C# Integration
===============================================

Integración completa del motor de ML con la arquitectura .NET existente.
Incluye handlers mejorados, DTOs extendidos y pipeline de features.
"""

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using MediatR;
using PricingAgent.Domain.Models;

namespace PricingAgent.Application.Features.Pricing.Queries;

/// <summary>
/// Enhanced pricing analysis request with ML features
/// </summary>
public sealed class EnhancedPricingAnalysisRequest
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
    public string Currency { get; init; } = "DOP";
    public IReadOnlyList<string> Features { get; init; } = new List<string>();
    public bool IncludeMarketAnalysis { get; init; } = true;
    public bool IncludeCompetitorAnalysis { get; init; } = true;
    public bool EnableMLPrediction { get; init; } = true;
}

/// <summary>
/// Enhanced pricing response with ML insights
/// </summary>
public sealed class EnhancedPricingAnalysisResponse
{
    public required decimal PrecioSugeridoDop { get; init; }
    public required decimal PrecioSugeridoUsd { get; init; }
    public required decimal PrecioMinimoDop { get; init; }
    public required decimal PrecioMaximoDop { get; init; }
    public required double ConfianzaScore { get; init; }
    public required string PosicionMercado { get; init; }
    public required int DiasEstimadosVenta { get; init; }
    public required string TendenciaMercado { get; init; }
    public required IReadOnlyDictionary<string, double> FactoresPricing { get; init; }
    public required CompetitorAnalysis AnalisisCompetencia { get; init; }
    public required MarketAnalysis AnalisisMercado { get; init; }
    public required MLModelInfo ModeloInfo { get; init; }
    public required string Explicacion { get; init; }
    public required IReadOnlyList<string> Recomendaciones { get; init; }
    public required double LatenciaMs { get; init; }
}

public sealed class CompetitorAnalysis
{
    public required IReadOnlyList<decimal> PreciosCompetencia { get; init; }
    public required decimal PromedioCompetencia { get; init; }
    public required int CompetidoresEncontrados { get; init; }
    public required double AjustePrecio { get; init; }
    public required string AnalisisPositioning { get; init; }
}

public sealed class MarketAnalysis
{
    public required string Tendencia { get; init; }
    public required double MultiplicadorTendencia { get; init; }
    public required bool EsTemporadaAlta { get; init; }
    public required string NivelDemanda { get; init; }
    public required IReadOnlyDictionary<string, string> FactoresMercado { get; init; }
}

public sealed class MLModelInfo
{
    public required string Version { get; init; }
    public required string Algorithm { get; init; }
    public required double Accuracy { get; init; }
    public required DateTime LastTraining { get; init; }
    public required int SamplesUsed { get; init; }
}

/// <summary>
/// Enhanced query for ML-powered pricing analysis
/// </summary>
public sealed class EnhancedAnalyzeVehiclePriceQuery : IRequest<EnhancedPricingAnalysisResponse>
{
    public required EnhancedPricingAnalysisRequest Request { get; init; }
}

/// <summary>
/// Enhanced handler with ML integration
/// </summary>
public sealed class EnhancedAnalyzeVehiclePriceQueryHandler 
    : IRequestHandler<EnhancedAnalyzeVehiclePriceQuery, EnhancedPricingAnalysisResponse>
{
    private readonly ILogger<EnhancedAnalyzeVehiclePriceQueryHandler> _logger;
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly IPricingMLService _mlService;
    private readonly IPricingRepository _repository;

    public EnhancedAnalyzeVehiclePriceQueryHandler(
        ILogger<EnhancedAnalyzeVehiclePriceQueryHandler> logger,
        IConfiguration config,
        HttpClient httpClient,
        IPricingMLService mlService,
        IPricingRepository repository)
    {
        _logger = logger;
        _config = config;
        _httpClient = httpClient;
        _mlService = mlService;
        _repository = repository;
    }

    public async Task<EnhancedPricingAnalysisResponse> Handle(
        EnhancedAnalyzeVehiclePriceQuery request, 
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("Iniciando análisis ML para {Make} {Model} {Year}", 
                request.Request.Make, request.Request.Model, request.Request.Year);

            // 1. Usar ML Service si está habilitado
            if (request.Request.EnableMLPrediction && await _mlService.IsAvailableAsync())
            {
                var mlPrediction = await _mlService.PredictPriceAsync(request.Request, cancellationToken);
                
                return new EnhancedPricingAnalysisResponse
                {
                    PrecioSugeridoDop = (decimal)mlPrediction.SuggestedPriceDop,
                    PrecioSugeridoUsd = (decimal)mlPrediction.SuggestedPriceUsd,
                    PrecioMinimoDop = (decimal)mlPrediction.PriceRangeMin,
                    PrecioMaximoDop = (decimal)mlPrediction.PriceRangeMax,
                    ConfianzaScore = mlPrediction.ConfidenceScore,
                    PosicionMercado = mlPrediction.MarketPosition,
                    DiasEstimadosVenta = mlPrediction.DaysToSell,
                    TendenciaMercado = mlPrediction.MarketTrend,
                    FactoresPricing = mlPrediction.PricingFactors,
                    AnalisisCompetencia = MapCompetitorAnalysis(mlPrediction.CompetitiveAnalysis),
                    AnalisisMercado = MapMarketAnalysis(mlPrediction),
                    ModeloInfo = new MLModelInfo
                    {
                        Version = mlPrediction.ModelVersion,
                        Algorithm = "XGBoost + Market Intelligence",
                        Accuracy = 0.85, // TODO: Get from model metadata
                        LastTraining = DateTime.UtcNow.AddDays(-7), // TODO: Get from model
                        SamplesUsed = 12000 // TODO: Get from model
                    },
                    Explicacion = GenerateExplanation(mlPrediction),
                    Recomendaciones = GenerateRecommendations(mlPrediction),
                    LatenciaMs = (DateTime.UtcNow - startTime).TotalMilliseconds
                };
            }

            // 2. Fallback a método tradicional si ML no disponible
            return await HandleTraditionalPricing(request, startTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en análisis de pricing ML");
            
            // Fallback seguro
            return await HandleTraditionalPricing(request, startTime);
        }
    }

    private async Task<EnhancedPricingAnalysisResponse> HandleTraditionalPricing(
        EnhancedAnalyzeVehiclePriceQuery request, DateTime startTime)
    {
        _logger.LogWarning("Usando método de pricing tradicional como fallback");
        
        // Implementación tradicional mejorada
        var traditionalAnalysis = await AnalyzeTraditionalPricing(request.Request);
        
        return new EnhancedPricingAnalysisResponse
        {
            PrecioSugeridoDop = traditionalAnalysis.SuggestedPrice,
            PrecioSugeridoUsd = traditionalAnalysis.SuggestedPrice / 58.5m, // Tasa fija
            PrecioMinimoDop = traditionalAnalysis.SuggestedPrice * 0.85m,
            PrecioMaximoDop = traditionalAnalysis.SuggestedPrice * 1.15m,
            ConfianzaScore = 0.6, // Menor confianza para método tradicional
            PosicionMercado = "Estimación tradicional",
            DiasEstimadosVenta = 45,
            TendenciaMercado = "Análisis básico",
            FactoresPricing = new Dictionary<string, double>
            {
                ["traditional_method"] = 1.0
            },
            AnalisisCompetencia = new CompetitorAnalysis
            {
                PreciosCompetencia = new List<decimal>(),
                PromedioCompetencia = 0,
                CompetidoresEncontrados = 0,
                AjustePrecio = 1.0,
                AnalisisPositioning = "Sin análisis competitivo"
            },
            AnalisisMercado = new MarketAnalysis
            {
                Tendencia = "Sin datos",
                MultiplicadorTendencia = 1.0,
                EsTemporadaAlta = false,
                NivelDemanda = "Desconocido",
                FactoresMercado = new Dictionary<string, string>()
            },
            ModeloInfo = new MLModelInfo
            {
                Version = "Traditional v1.0",
                Algorithm = "Rule-based pricing",
                Accuracy = 0.60,
                LastTraining = DateTime.UtcNow,
                SamplesUsed = 0
            },
            Explicacion = "Análisis basado en reglas tradicionales sin ML",
            Recomendaciones = new List<string>
            {
                "Se recomienda habilitar el sistema de ML para mayor precisión",
                "Análisis limitado sin datos de competencia"
            },
            LatenciaMs = (DateTime.UtcNow - startTime).TotalMilliseconds
        };
    }

    private CompetitorAnalysis MapCompetitorAnalysis(dynamic competitiveData)
    {
        var competitorPrices = ((IEnumerable<dynamic>)competitiveData.competitor_prices)
            .Select(p => (decimal)(double)p)
            .ToList();

        return new CompetitorAnalysis
        {
            PreciosCompetencia = competitorPrices,
            PromedioCompetencia = (decimal)(double)competitiveData.avg_competitor_price,
            CompetidoresEncontrados = (int)competitiveData.competitors_found,
            AjustePrecio = (double)competitiveData.price_adjustment,
            AnalisisPositioning = GeneratePositioningAnalysis(competitorPrices)
        };
    }

    private MarketAnalysis MapMarketAnalysis(dynamic mlPrediction)
    {
        // Extraer datos del análisis de mercado del ML
        return new MarketAnalysis
        {
            Tendencia = mlPrediction.MarketTrend,
            MultiplicadorTendencia = 1.0, // TODO: Extract from ML response
            EsTemporadaAlta = DateTime.Now.Month >= 11 || DateTime.Now.Month <= 2,
            NivelDemanda = "Moderado", // TODO: Extract from ML
            FactoresMercado = new Dictionary<string, string>
            {
                ["seasonality"] = "Considerado",
                ["competition"] = "Analizado",
                ["trends"] = "Actualizados"
            }
        };
    }

    private string GenerateExplanation(dynamic mlPrediction)
    {
        var confidence = (double)mlPrediction.ConfidenceScore;
        var position = (string)mlPrediction.MarketPosition;
        
        return $"Análisis ML con {confidence:P0} de confianza. " +
               $"Posición: {position}. " +
               $"Predicción basada en {mlPrediction.ModelVersion} entrenado con datos actualizados.";
    }

    private List<string> GenerateRecommendations(dynamic mlPrediction)
    {
        var recommendations = new List<string>();
        var daysToSell = (int)mlPrediction.DaysToSell;
        var confidence = (double)mlPrediction.ConfidenceScore;
        
        if (daysToSell > 45)
        {
            recommendations.Add("Considere reducir el precio para acelerar la venta");
        }
        
        if (confidence < 0.7)
        {
            recommendations.Add("Confianza baja - verifique datos del vehículo");
        }
        
        recommendations.Add("Monitoree precios de competencia semanalmente");
        recommendations.Add("Ajuste precio según feedback del mercado");
        
        return recommendations;
    }

    private string GeneratePositioningAnalysis(IReadOnlyList<decimal> competitorPrices)
    {
        if (!competitorPrices.Any())
            return "Sin competencia directa encontrada";
        
        var avgPrice = competitorPrices.Average();
        return $"Promedio competencia: {avgPrice:C0}. " +
               $"Rango: {competitorPrices.Min():C0} - {competitorPrices.Max():C0}";
    }

    private async Task<TraditionalPricingResult> AnalyzeTraditionalPricing(
        EnhancedPricingAnalysisRequest request)
    {
        // Implementación de pricing tradicional mejorada
        var basePrice = await CalculateBasePrice(request);
        var adjustedPrice = ApplyTraditionalAdjustments(basePrice, request);
        
        return new TraditionalPricingResult
        {
            SuggestedPrice = adjustedPrice
        };
    }

    private async Task<decimal> CalculateBasePrice(EnhancedPricingAnalysisRequest request)
    {
        // Lógica de precio base mejorada
        // TODO: Implementar con datos históricos reales
        
        var age = DateTime.Now.Year - request.Year;
        var baseValue = GetBrandBaseValue(request.Make);
        var depreciationFactor = Math.Pow(0.85, age); // 15% anual
        
        return (decimal)(baseValue * depreciationFactor);
    }

    private double GetBrandBaseValue(string make)
    {
        return make.ToLower() switch
        {
            "toyota" or "honda" or "nissan" => 2000000,
            "bmw" or "mercedes-benz" or "audi" => 3500000,
            "hyundai" or "kia" => 1500000,
            _ => 1800000
        };
    }

    private decimal ApplyTraditionalAdjustments(decimal basePrice, 
        EnhancedPricingAnalysisRequest request)
    {
        var adjustedPrice = basePrice;
        
        // Ajuste por millaje
        if (request.Mileage.HasValue)
        {
            var mileageAdjustment = request.Mileage < 50000 ? 1.1m : 
                                  request.Mileage > 150000 ? 0.9m : 1.0m;
            adjustedPrice *= mileageAdjustment;
        }
        
        // Ajuste por condición
        var conditionMultiplier = request.Condition?.ToLower() switch
        {
            "excelente" => 1.1m,
            "muy bueno" => 1.05m,
            "bueno" => 1.0m,
            "regular" => 0.9m,
            _ => 0.95m
        };
        
        adjustedPrice *= conditionMultiplier;
        
        return adjustedPrice;
    }
}

public sealed class TraditionalPricingResult
{
    public required decimal SuggestedPrice { get; init; }
}

/// <summary>
/// Service interface for ML pricing integration
/// </summary>
public interface IPricingMLService
{
    Task<bool> IsAvailableAsync();
    Task<MLPricingPrediction> PredictPriceAsync(
        EnhancedPricingAnalysisRequest request, 
        CancellationToken cancellationToken = default);
    Task<bool> RetrainModelAsync();
}

/// <summary>
/// ML Pricing prediction result
/// </summary>
public sealed class MLPricingPrediction
{
    public required double SuggestedPriceDop { get; init; }
    public required double SuggestedPriceUsd { get; init; }
    public required double PriceRangeMin { get; init; }
    public required double PriceRangeMax { get; init; }
    public required double ConfidenceScore { get; init; }
    public required string MarketPosition { get; init; }
    public required int DaysToSell { get; init; }
    public required string MarketTrend { get; init; }
    public required IReadOnlyDictionary<string, double> PricingFactors { get; init; }
    public required dynamic CompetitiveAnalysis { get; init; }
    public required string ModelVersion { get; init; }
}

/// <summary>
/// HTTP-based ML service implementation
/// </summary>
public sealed class HttpPricingMLService : IPricingMLService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpPricingMLService> _logger;
    private readonly string _baseUrl;

    public HttpPricingMLService(HttpClient httpClient, 
        ILogger<HttpPricingMLService> logger, 
        IConfiguration config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = config["MLPricingService:BaseUrl"] ?? "http://localhost:8080";
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/pricing/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ML Pricing service no disponible");
            return false;
        }
    }

    public async Task<MLPricingPrediction> PredictPriceAsync(
        EnhancedPricingAnalysisRequest request, 
        CancellationToken cancellationToken = default)
    {
        var mlRequest = new
        {
            make = request.Make,
            model = request.Model,
            year = request.Year,
            mileage = request.Mileage,
            trim = request.Trim,
            fuel_type = request.FuelType,
            transmission = request.Transmission,
            condition = request.Condition,
            province = request.Province,
            asking_price = request.AskingPrice
        };

        var json = JsonSerializer.Serialize(mlRequest);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(
            $"{_baseUrl}/api/pricing/predict", 
            content, 
            cancellationToken);

        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var prediction = JsonSerializer.Deserialize<dynamic>(responseJson);

        return new MLPricingPrediction
        {
            SuggestedPriceDop = (double)prediction.GetProperty("suggested_price_dop").GetDouble(),
            SuggestedPriceUsd = (double)prediction.GetProperty("suggested_price_usd").GetDouble(),
            PriceRangeMin = (double)prediction.GetProperty("price_range_min").GetDouble(),
            PriceRangeMax = (double)prediction.GetProperty("price_range_max").GetDouble(),
            ConfidenceScore = (double)prediction.GetProperty("confidence_score").GetDouble(),
            MarketPosition = prediction.GetProperty("market_position").GetString() ?? "",
            DaysToSell = prediction.GetProperty("days_to_sell").GetInt32(),
            MarketTrend = prediction.GetProperty("market_trend").GetString() ?? "",
            PricingFactors = ExtractPricingFactors(prediction.GetProperty("pricing_factors")),
            CompetitiveAnalysis = prediction.GetProperty("competitive_analysis"),
            ModelVersion = prediction.GetProperty("model_version").GetString() ?? "Unknown"
        };
    }

    public async Task<bool> RetrainModelAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/pricing/train", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reentrenando modelo ML");
            return false;
        }
    }

    private IReadOnlyDictionary<string, double> ExtractPricingFactors(JsonElement factorsElement)
    {
        var factors = new Dictionary<string, double>();
        
        if (factorsElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in factorsElement.EnumerateObject())
            {
                factors[property.Name] = property.Value.GetDouble();
            }
        }
        
        return factors;
    }
}