using MediatR;
using VehicleIntelligenceService.Application.DTOs;
using VehicleIntelligenceService.Domain.Interfaces;

namespace VehicleIntelligenceService.Application.Features.Pricing.Queries;

public record GetPriceSuggestionQuery(PriceSuggestionRequestDto Request) : IRequest<PriceSuggestionDto>;

public class GetPriceSuggestionHandler : IRequestHandler<GetPriceSuggestionQuery, PriceSuggestionDto>
{
    private readonly IPricingEngine _pricingEngine;

    public GetPriceSuggestionHandler(IPricingEngine pricingEngine)
    {
        _pricingEngine = pricingEngine;
    }

    public async Task<PriceSuggestionDto> Handle(GetPriceSuggestionQuery request, CancellationToken cancellationToken)
    {
        var input = new VehiclePricingInput(
            VehicleId: Guid.NewGuid(),
            Make: request.Request.Make,
            Model: request.Request.Model,
            Year: request.Request.Year,
            Mileage: request.Request.Mileage,
            Condition: "Good",
            FuelType: "Gasoline",
            Transmission: "Automatic",
            CurrentPrice: request.Request.AskingPrice ?? 0,
            PhotoCount: 10,
            ViewCount: 0,
            DaysListed: 0
        );

        var analysis = await _pricingEngine.AnalyzePriceAsync(input, cancellationToken);

        var tips = new List<string>();
        if (analysis.PriceVsMarket > 1.1m)
            tips.Add("Tu precio está por encima del mercado. Considera reducirlo para vender más rápido.");
        if (analysis.PriceVsMarket < 0.9m)
            tips.Add("Tu precio está por debajo del mercado. Podrías aumentarlo ligeramente.");
        if (request.Request.Mileage > 100000)
            tips.Add("El kilometraje alto puede afectar el valor. Destaca el buen mantenimiento.");

        return new PriceSuggestionDto(
            MarketPrice: analysis.MarketAvgPrice,
            SuggestedPrice: analysis.SuggestedPrice,
            DeltaPercent: (analysis.PriceVsMarket - 1) * 100,
            DemandScore: 75,
            EstimatedDaysToSell: analysis.PredictedDaysToSaleAtSuggestedPrice,
            Confidence: analysis.ConfidenceScore,
            ModelVersion: "v1.0-ML-Lite",
            Tips: tips
        );
    }
}
