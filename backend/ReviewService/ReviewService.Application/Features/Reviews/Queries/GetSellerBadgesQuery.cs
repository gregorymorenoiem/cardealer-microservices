using MediatR;
using Microsoft.Extensions.Logging;
using ReviewService.Application.DTOs;
using ReviewService.Domain.Base;
using ReviewService.Domain.Interfaces;

namespace ReviewService.Application.Features.Reviews.Queries;

/// <summary>
/// Sprint 15 - Obtener badges de un vendedor
/// </summary>
public record GetSellerBadgesQuery : IRequest<Result<List<SellerBadgeDto>>>
{
    public Guid SellerId { get; init; }
    public bool OnlyActive { get; init; } = true;
}

/// <summary>
/// Handler para obtener badges de vendedor
/// </summary>
public class GetSellerBadgesQueryHandler : IRequestHandler<GetSellerBadgesQuery, Result<List<SellerBadgeDto>>>
{
    private readonly ISellerBadgeRepository _badgeRepository;
    private readonly ILogger<GetSellerBadgesQueryHandler> _logger;

    public GetSellerBadgesQueryHandler(ISellerBadgeRepository badgeRepository, ILogger<GetSellerBadgesQueryHandler> logger)
    {
        _badgeRepository = badgeRepository;
        _logger = logger;
    }

    public async Task<Result<List<SellerBadgeDto>>> Handle(GetSellerBadgesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var badges = await _badgeRepository.GetActiveBySellerIdAsync(request.SellerId, cancellationToken);

            if (!request.OnlyActive)
            {
                // Si necesita incluir badges inactivos, podría agregarse otro método al repositorio
            }

            var badgeDtos = badges
                .Where(b => !request.OnlyActive || (b.IsActive && (b.ExpiresAt == null || b.ExpiresAt > DateTime.UtcNow)))
                .OrderByDescending(b => b.EarnedAt)
                .Select(b => new SellerBadgeDto
                {
                    Id = b.Id,
                    BadgeType = b.BadgeType.ToString(),
                    Title = b.Title ?? "",
                    Description = b.Description ?? "",
                    Icon = b.Icon ?? "",
                    Color = b.Color ?? "",
                    IsActive = b.IsActive,
                    EarnedAt = b.EarnedAt,
                    ExpiresAt = b.ExpiresAt
                })
                .ToList();

            return Result<List<SellerBadgeDto>>.Success(badgeDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting badges for seller {SellerId}", request.SellerId);
            return Result<List<SellerBadgeDto>>.Failure($"Error al obtener badges: {ex.Message}");
        }
    }
}
