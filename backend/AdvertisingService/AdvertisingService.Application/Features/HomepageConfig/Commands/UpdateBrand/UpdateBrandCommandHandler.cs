using AdvertisingService.Application.DTOs;
using AdvertisingService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdvertisingService.Application.Features.HomepageConfig.Commands.UpdateBrand;

public class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand, BrandConfigDto>
{
    private readonly IBrandConfigRepository _brandRepo;
    private readonly ILogger<UpdateBrandCommandHandler> _logger;

    public UpdateBrandCommandHandler(IBrandConfigRepository brandRepo, ILogger<UpdateBrandCommandHandler> logger)
    {
        _brandRepo = brandRepo;
        _logger = logger;
    }

    public async Task<BrandConfigDto> Handle(UpdateBrandCommand request, CancellationToken ct)
    {
        var config = await _brandRepo.GetByKeyAsync(request.BrandKey, ct)
            ?? throw new KeyNotFoundException($"Brand '{request.BrandKey}' not found");

        if (request.DisplayName != null) config.DisplayName = request.DisplayName;
        if (request.LogoUrl != null) config.LogoUrl = request.LogoUrl;
        if (request.LogoInitials != null) config.LogoInitials = request.LogoInitials;
        if (request.VehicleCount.HasValue) config.VehicleCount = request.VehicleCount.Value;
        if (request.DisplayOrder.HasValue) config.DisplayOrder = request.DisplayOrder.Value;
        if (request.IsVisible.HasValue) config.IsVisible = request.IsVisible.Value;
        if (request.Route != null) config.Route = request.Route;

        await _brandRepo.UpdateAsync(config, ct);

        _logger.LogInformation("Brand {BrandKey} updated", request.BrandKey);

        return new BrandConfigDto(
            config.Id, config.BrandKey, config.DisplayName, config.LogoUrl,
            config.LogoInitials, config.VehicleCount, config.DisplayOrder,
            config.IsVisible, config.Route
        );
    }
}
