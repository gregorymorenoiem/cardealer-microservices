using MediatR;
using VehicleIntelligenceService.Application.DTOs;
using VehicleIntelligenceService.Domain.Interfaces;

namespace VehicleIntelligenceService.Application.Features.Pricing.Commands;

public record AnalyzeVehiclePriceCommand(CreatePriceAnalysisRequest Request) : IRequest<PriceAnalysisDto>;

public class AnalyzeVehiclePriceHandler : IRequestHandler<AnalyzeVehiclePriceCommand, PriceAnalysisDto>
{
    private readonly IPricingEngine _pricingEngine;
    private readonly IPriceAnalysisRepository _repository;

    public AnalyzeVehiclePriceHandler(IPricingEngine pricingEngine, IPriceAnalysisRepository repository)
    {
        _pricingEngine = pricingEngine;
        _repository = repository;
    }

    public async Task<PriceAnalysisDto> Handle(AnalyzeVehiclePriceCommand request, CancellationToken cancellationToken)
    {
        var input = new VehiclePricingInput(
            request.Request.VehicleId,
            request.Request.Make,
            request.Request.Model,
            request.Request.Year,
            request.Request.Mileage,
            request.Request.Condition,
            request.Request.FuelType,
            request.Request.Transmission,
            request.Request.CurrentPrice,
            request.Request.PhotoCount,
            request.Request.ViewCount,
            request.Request.DaysListed
        );

        var analysis = await _pricingEngine.AnalyzePriceAsync(input, cancellationToken);
        var saved = await _repository.CreateAsync(analysis, cancellationToken);

        return new PriceAnalysisDto
        {
            Id = saved.Id,
            VehicleId = saved.VehicleId,
            CurrentPrice = saved.CurrentPrice,
            SuggestedPrice = saved.SuggestedPrice,
            SuggestedPriceMin = saved.SuggestedPriceMin,
            SuggestedPriceMax = saved.SuggestedPriceMax,
            MarketAvgPrice = saved.MarketAvgPrice,
            PriceVsMarket = saved.PriceVsMarket,
            PricePosition = saved.PricePosition,
            PredictedDaysToSaleAtCurrentPrice = saved.PredictedDaysToSaleAtCurrentPrice,
            PredictedDaysToSaleAtSuggestedPrice = saved.PredictedDaysToSaleAtSuggestedPrice,
            ConfidenceScore = saved.ConfidenceScore,
            AnalysisDate = saved.AnalysisDate
        };
    }
}
