using MediatR;
using Microsoft.Extensions.Logging;
using ReviewService.Application.DTOs;
using ReviewService.Domain.Services;

namespace ReviewService.Application.Features.Badges.Commands;

/// <summary>
/// Handler para calcular badges
/// </summary>
public class CalculateBadgesCommandHandler : IRequestHandler<CalculateBadgesCommand, BadgeCalculationResultDto>
{
    private readonly IBadgeCalculationService _badgeCalculationService;
    private readonly ILogger<CalculateBadgesCommandHandler> _logger;

    public CalculateBadgesCommandHandler(
        IBadgeCalculationService badgeCalculationService,
        ILogger<CalculateBadgesCommandHandler> logger)
    {
        _badgeCalculationService = badgeCalculationService;
        _logger = logger;
    }

    public async Task<BadgeCalculationResultDto> Handle(CalculateBadgesCommand request, CancellationToken cancellationToken)
    {
        if (request.SellerId.HasValue)
        {
            // Calcular badges para un vendedor específico
            var grantedBadges = await _badgeCalculationService.CalculateAndGrantBadgesAsync(request.SellerId.Value, cancellationToken);
            
            _logger.LogInformation("Calculated badges for seller {SellerId}: {BadgeCount} badges granted", 
                request.SellerId.Value, grantedBadges.Count);

            return new BadgeCalculationResultDto
            {
                ProcessedSellers = 1,
                TotalBadgesGranted = grantedBadges.Count,
                Results = new Dictionary<Guid, List<string>>
                {
                    { request.SellerId.Value, grantedBadges.Select(b => b.ToString()).ToList() }
                }
            };
        }
        else
        {
            // Calcular badges para todos los vendedores
            var allResults = await _badgeCalculationService.CalculateBadgesForAllSellersAsync(cancellationToken);
            
            var totalBadges = allResults.Values.Sum(badges => badges.Count);
            
            _logger.LogInformation("Calculated badges for all sellers: {SellerCount} sellers processed, {BadgeCount} badges granted", 
                allResults.Count, totalBadges);

            return new BadgeCalculationResultDto
            {
                ProcessedSellers = allResults.Count,
                TotalBadgesGranted = totalBadges,
                Results = allResults.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => kvp.Value.Select(b => b.ToString()).ToList())
            };
        }
    }
}