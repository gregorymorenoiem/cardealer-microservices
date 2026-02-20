using AdvertisingService.Application.DTOs;
using AdvertisingService.Application.Interfaces;
using AdvertisingService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdvertisingService.Application.Features.Rotation.Commands.UpdateRotationConfig;

public class UpdateRotationConfigCommandHandler : IRequestHandler<UpdateRotationConfigCommand, RotationConfigDto>
{
    private readonly IRotationConfigRepository _configRepo;
    private readonly IHomepageRotationCacheService _cacheService;
    private readonly ILogger<UpdateRotationConfigCommandHandler> _logger;

    public UpdateRotationConfigCommandHandler(
        IRotationConfigRepository configRepo,
        IHomepageRotationCacheService cacheService,
        ILogger<UpdateRotationConfigCommandHandler> logger)
    {
        _configRepo = configRepo;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<RotationConfigDto> Handle(UpdateRotationConfigCommand request, CancellationToken ct)
    {
        var config = await _configRepo.GetBySectionAsync(request.Section, ct)
            ?? throw new KeyNotFoundException($"RotationConfig for {request.Section} not found");

        if (request.AlgorithmType.HasValue) config.AlgorithmType = request.AlgorithmType.Value;
        if (request.RefreshIntervalMinutes.HasValue) config.RefreshIntervalMinutes = request.RefreshIntervalMinutes.Value;
        if (request.MaxVehiclesShown.HasValue) config.MaxVehiclesShown = request.MaxVehiclesShown.Value;
        if (request.WeightRemainingBudget.HasValue) config.WeightRemainingBudget = request.WeightRemainingBudget.Value;
        if (request.WeightCtr.HasValue) config.WeightCtr = request.WeightCtr.Value;
        if (request.WeightQualityScore.HasValue) config.WeightQualityScore = request.WeightQualityScore.Value;
        if (request.WeightRecency.HasValue) config.WeightRecency = request.WeightRecency.Value;
        if (request.IsActive.HasValue) config.IsActive = request.IsActive.Value;

        // Post-merge validation: if any weight was changed, the resulting sum must equal 1.0
        var anyWeightChanged = request.WeightRemainingBudget.HasValue ||
                               request.WeightCtr.HasValue ||
                               request.WeightQualityScore.HasValue ||
                               request.WeightRecency.HasValue;

        if (anyWeightChanged)
        {
            var sum = config.WeightRemainingBudget + config.WeightCtr +
                      config.WeightQualityScore + config.WeightRecency;

            if (Math.Abs(sum - 1.0m) >= 0.01m)
            {
                throw new FluentValidation.ValidationException(
                    $"Los pesos deben sumar 100% (1.0). Suma actual tras aplicar cambios: {sum:F4}. " +
                    $"Pesos: Budget={config.WeightRemainingBudget}, CTR={config.WeightCtr}, " +
                    $"Quality={config.WeightQualityScore}, Recency={config.WeightRecency}");
            }
        }

        await _configRepo.UpdateAsync(config, ct);
        await _cacheService.InvalidateAsync(request.Section);

        _logger.LogInformation("RotationConfig updated for {Section}", request.Section);

        return new RotationConfigDto(
            config.Id, config.Section, config.AlgorithmType, config.RefreshIntervalMinutes,
            config.MaxVehiclesShown, config.WeightRemainingBudget, config.WeightCtr,
            config.WeightQualityScore, config.WeightRecency, config.IsActive, config.UpdatedAt
        );
    }
}
