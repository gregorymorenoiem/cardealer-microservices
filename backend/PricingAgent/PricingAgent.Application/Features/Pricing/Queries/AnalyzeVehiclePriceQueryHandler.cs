using MediatR;
using Microsoft.Extensions.Logging;
using PricingAgent.Application.DTOs;
using PricingAgent.Domain.Interfaces;
using PricingAgent.Domain.Models;

namespace PricingAgent.Application.Features.Pricing.Queries;

public sealed class AnalyzeVehiclePriceQueryHandler
    : IRequestHandler<AnalyzeVehiclePriceQuery, PricingAnalysisResponse>
{
    private readonly ILlmPricingService _llmPricingService;
    private readonly ILogger<AnalyzeVehiclePriceQueryHandler> _logger;

    public AnalyzeVehiclePriceQueryHandler(
        ILlmPricingService llmPricingService,
        ILogger<AnalyzeVehiclePriceQueryHandler> logger)
    {
        _llmPricingService = llmPricingService;
        _logger = logger;
    }

    public async Task<PricingAnalysisResponse> Handle(
        AnalyzeVehiclePriceQuery query,
        CancellationToken cancellationToken)
    {
        var req = query.Request;

        _logger.LogInformation(
            "PricingAgent: Analyzing {Make} {Model} {Year} — Province: {Province}",
            req.Make, req.Model, req.Year, req.Province ?? "N/A");

        var vehicleInput = new VehiclePricingInput
        {
            Make = req.Make,
            Model = req.Model,
            Year = req.Year,
            Trim = req.Trim,
            Mileage = req.Mileage,
            FuelType = req.FuelType,
            Transmission = req.Transmission,
            Condition = req.Condition,
            Province = req.Province,
            AskingPrice = req.AskingPrice,
            Currency = req.Currency,
            Features = req.Features
        };

        // TODO: Fetch real comparables from VehiclesSaleService or external sources
        var comparables = GetMockComparables(req);

        var analysis = await _llmPricingService.AnalyzePriceAsync(
            vehicleInput, comparables, cancellationToken);

        return new PricingAnalysisResponse
        {
            PrecioMinimoDop = analysis.PrecioMinimoDop,
            PrecioMaximoDop = analysis.PrecioMaximoDop,
            PrecioSugeridoDop = analysis.PrecioSugeridoDop,
            PrecioSugeridoUsd = analysis.PrecioSugeridoUsd,
            Confianza = analysis.Confianza,
            NivelPrecio = analysis.NivelPrecio,
            DesviacionPorcentaje = analysis.DesviacionPorcentaje,
            Explicacion = analysis.Explicacion,
            FactoresPositivos = analysis.FactoresPositivos,
            FactoresNegativos = analysis.FactoresNegativos,
            RecomendacionComprador = analysis.RecomendacionComprador,
            RecomendacionVendedor = analysis.RecomendacionVendedor,
            DiasEstimadosVenta = analysis.DiasEstimadosVenta,
            TendenciaMercado = analysis.TendenciaMercado,
            ModelUsed = "cascade",
            FallbackLevel = 0,
            LatencyMs = 0
        };
    }

    private static List<MarketComparable> GetMockComparables(PricingAnalysisRequest req)
    {
        // Placeholder — in production, fetch from DB or VehiclesSaleService
        return
        [
            new MarketComparable
            {
                Make = req.Make,
                Model = req.Model,
                Year = req.Year,
                Price = 1_200_000m,
                Currency = "DOP",
                Mileage = 45000,
                Condition = "used",
                Source = "okla",
                DaysOnMarket = 15
            }
        ];
    }
}
