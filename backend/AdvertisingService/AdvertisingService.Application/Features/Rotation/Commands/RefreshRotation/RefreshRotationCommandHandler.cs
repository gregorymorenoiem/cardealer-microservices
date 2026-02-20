using AdvertisingService.Application.Interfaces;
using AdvertisingService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdvertisingService.Application.Features.Rotation.Commands.RefreshRotation;

public class RefreshRotationCommandHandler : IRequestHandler<RefreshRotationCommand, bool>
{
    private readonly IHomepageRotationCacheService _cacheService;
    private readonly ILogger<RefreshRotationCommandHandler> _logger;

    public RefreshRotationCommandHandler(
        IHomepageRotationCacheService cacheService,
        ILogger<RefreshRotationCommandHandler> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<bool> Handle(RefreshRotationCommand request, CancellationToken ct)
    {
        if (request.Section.HasValue)
        {
            await _cacheService.RefreshRotationAsync(request.Section.Value, ct);
        }
        else
        {
            foreach (var section in Enum.GetValues<AdPlacementType>())
            {
                await _cacheService.RefreshRotationAsync(section, ct);
            }
        }

        _logger.LogInformation("Rotation manually refreshed for {Section}", request.Section?.ToString() ?? "ALL");
        return true;
    }
}
