using MediatR;
using VehicleIntelligenceService.Application.DTOs;
using VehicleIntelligenceService.Domain.Interfaces;
using System.Text.Json;

namespace VehicleIntelligenceService.Application.Features.Demand.Commands;

public record PredictDemandCommand(CreateDemandPredictionRequest Request) : IRequest<DemandPredictionDto>;

public class PredictDemandHandler : IRequestHandler<PredictDemandCommand, DemandPredictionDto>
{
    private readonly IPricingEngine _pricingEngine;
    private readonly IDemandPredictionRepository _repository;

    public PredictDemandHandler(IPricingEngine pricingEngine, IDemandPredictionRepository repository)
    {
        _pricingEngine = pricingEngine;
        _repository = repository;
    }

    public async Task<DemandPredictionDto> Handle(PredictDemandCommand request, CancellationToken cancellationToken)
    {
        var input = new DemandPredictionInput(
            request.Request.Make,
            request.Request.Model,
            request.Request.Year,
            request.Request.FuelType,
            request.Request.Transmission
        );

        var prediction = await _pricingEngine.PredictDemandAsync(input, cancellationToken);
        var saved = await _repository.CreateAsync(prediction, cancellationToken);

        var insights = JsonSerializer.Deserialize<List<string>>(saved.Insights) ?? new List<string>();

        return new DemandPredictionDto
        {
            Id = saved.Id,
            Make = saved.Make,
            Model = saved.Model,
            Year = saved.Year,
            CurrentDemand = saved.CurrentDemand.ToString(),
            DemandScore = saved.DemandScore,
            Trend = saved.Trend.ToString(),
            TrendStrength = saved.TrendStrength,
            PredictedDemand30Days = saved.PredictedDemand30Days.ToString(),
            PredictedDemand90Days = saved.PredictedDemand90Days.ToString(),
            SearchesPerDay = saved.SearchesPerDay,
            AvailableInventory = saved.AvailableInventory,
            AvgDaysToSale = saved.AvgDaysToSale,
            BuyRecommendation = saved.BuyRecommendation.ToString(),
            BuyRecommendationReason = saved.BuyRecommendationReason,
            Insights = insights,
            PredictionDate = saved.PredictionDate
        };
    }
}
