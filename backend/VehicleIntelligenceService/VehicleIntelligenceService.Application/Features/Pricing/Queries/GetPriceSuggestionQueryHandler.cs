using MediatR;
using VehicleIntelligenceService.Application.DTOs;
using VehicleIntelligenceService.Domain.Interfaces;

namespace VehicleIntelligenceService.Application.Features.Pricing.Queries;

public class GetPriceSuggestionQueryHandler(
    IVehiclePricingEngine engine,
    IPriceSuggestionRepository repository,
    ICategoryDemandRepository demandRepository) : IRequestHandler<GetPriceSuggestionQuery, PriceSuggestionDto>
{
    public async Task<PriceSuggestionDto> Handle(GetPriceSuggestionQuery request, CancellationToken cancellationToken)
    {
        var demand = await demandRepository.GetAllAsync(cancellationToken);
        var categoryDemand = demand
            .FirstOrDefault(d => string.Equals(d.Category, request.Request.BodyType ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            ?.DemandScore;

        var record = engine.ComputeSuggestion(
            request.Request.Make,
            request.Request.Model,
            request.Request.Year,
            request.Request.Mileage,
            request.Request.BodyType,
            request.Request.Location,
            request.Request.AskingPrice,
            categoryDemand);

        await repository.AddAsync(record, cancellationToken);

        var tips = engine.BuildSellingTips(record);

        return new PriceSuggestionDto(
            record.MarketPrice,
            record.SuggestedPrice,
            record.DeltaPercent,
            record.DemandScore,
            record.EstimatedDaysToSell,
            record.Confidence,
            record.ModelVersion,
            tips);
    }
}
