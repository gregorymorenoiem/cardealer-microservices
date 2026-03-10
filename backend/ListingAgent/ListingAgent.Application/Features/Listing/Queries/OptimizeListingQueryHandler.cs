using MediatR;
using Microsoft.Extensions.Logging;
using ListingAgent.Application.DTOs;
using ListingAgent.Domain.Interfaces;
using ListingAgent.Domain.Models;

namespace ListingAgent.Application.Features.Listing.Queries;

public sealed class OptimizeListingQueryHandler
    : IRequestHandler<OptimizeListingQuery, ListingOptimizationResponse>
{
    private readonly ILlmListingService _llmListingService;
    private readonly ILogger<OptimizeListingQueryHandler> _logger;

    public OptimizeListingQueryHandler(ILlmListingService llmListingService, ILogger<OptimizeListingQueryHandler> logger)
    {
        _llmListingService = llmListingService;
        _logger = logger;
    }

    public async Task<ListingOptimizationResponse> Handle(OptimizeListingQuery query, CancellationToken ct)
    {
        var req = query.Request;
        _logger.LogInformation("ListingAgent: Optimizing listing for {Make} {Model} {Year}", req.Make, req.Model, req.Year);

        var input = new ListingInput
        {
            VehicleId = req.VehicleId,
            Title = req.Title,
            Description = req.Description,
            Make = req.Make,
            Model = req.Model,
            Year = req.Year,
            Trim = req.Trim,
            Price = req.Price,
            Currency = req.Currency,
            Mileage = req.Mileage,
            FuelType = req.FuelType,
            Transmission = req.Transmission,
            Condition = req.Condition,
            PhotoCount = req.PhotoCount,
            Features = req.Features,
            SellerType = req.SellerType,
            Province = req.Province
        };

        var result = await _llmListingService.OptimizeListingAsync(input, ct);

        return new ListingOptimizationResponse
        {
            PuntajeCalidad = result.PuntajeCalidad,
            TituloOptimizado = result.TituloOptimizado,
            DescripcionOptimizada = result.DescripcionOptimizada,
            DescripcionWhatsApp = result.DescripcionWhatsApp,
            MetaDescription = result.MetaDescription,
            Tags = result.Tags,
            Mejoras = result.Mejoras,
            VentajaCompetitiva = result.VentajaCompetitiva,
            BoostEstimadoPorcentaje = result.BoostEstimadoPorcentaje,
            ModelUsed = "cascade",
            FallbackLevel = 0,
            LatencyMs = 0,
            FromCache = result.FromCache,
            CacheKey = result.CacheKey
        };
    }
}
