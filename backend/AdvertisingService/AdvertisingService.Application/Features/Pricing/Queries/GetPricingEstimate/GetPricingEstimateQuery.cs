using AdvertisingService.Application.DTOs;
using AdvertisingService.Domain.Enums;
using MediatR;

namespace AdvertisingService.Application.Features.Pricing.Queries.GetPricingEstimate;

public record GetPricingEstimateQuery(AdPlacementType PlacementType) : IRequest<PricingEstimateDto>;
