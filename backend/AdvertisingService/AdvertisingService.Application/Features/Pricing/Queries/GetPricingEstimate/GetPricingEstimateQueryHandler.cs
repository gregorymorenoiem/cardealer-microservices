using AdvertisingService.Application.DTOs;
using AdvertisingService.Domain.Enums;
using MediatR;

namespace AdvertisingService.Application.Features.Pricing.Queries.GetPricingEstimate;

public class GetPricingEstimateQueryHandler : IRequestHandler<GetPricingEstimateQuery, PricingEstimateDto>
{
    public Task<PricingEstimateDto> Handle(GetPricingEstimateQuery request, CancellationToken ct)
    {
        var (ppv, daily, weekly, monthly, estViews) = request.PlacementType switch
        {
            AdPlacementType.FeaturedSpot => (0.50m, 150m, 900m, 3200m, 300),
            AdPlacementType.PremiumSpot => (1.00m, 300m, 1800m, 6500m, 200),
            _ => (0.50m, 150m, 900m, 3200m, 300)
        };

        var result = new PricingEstimateDto(
            request.PlacementType,
            CampaignPricingModel.PerView,
            ppv, daily, weekly, monthly, estViews, "DOP"
        );

        return Task.FromResult(result);
    }
}
