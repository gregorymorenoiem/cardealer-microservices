using AdvertisingService.Application.DTOs;
using AdvertisingService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdvertisingService.Application.Features.HomepageConfig.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryImageConfigDto>
{
    private readonly ICategoryConfigRepository _categoryRepo;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;

    public UpdateCategoryCommandHandler(ICategoryConfigRepository categoryRepo, ILogger<UpdateCategoryCommandHandler> logger)
    {
        _categoryRepo = categoryRepo;
        _logger = logger;
    }

    public async Task<CategoryImageConfigDto> Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var config = await _categoryRepo.GetByKeyAsync(request.CategoryKey, ct)
            ?? throw new KeyNotFoundException($"Category '{request.CategoryKey}' not found");

        if (request.DisplayName != null) config.DisplayName = request.DisplayName;
        if (request.Description != null) config.Description = request.Description;
        if (request.ImageUrl != null) config.ImageUrl = request.ImageUrl;
        if (request.IconUrl != null) config.IconUrl = request.IconUrl;
        if (request.Gradient != null) config.Gradient = request.Gradient;
        if (request.VehicleCount.HasValue) config.VehicleCount = request.VehicleCount.Value;
        if (request.IsTrending.HasValue) config.IsTrending = request.IsTrending.Value;
        if (request.DisplayOrder.HasValue) config.DisplayOrder = request.DisplayOrder.Value;
        if (request.IsVisible.HasValue) config.IsVisible = request.IsVisible.Value;
        if (request.Route != null) config.Route = request.Route;

        await _categoryRepo.UpdateAsync(config, ct);

        _logger.LogInformation("Category {CategoryKey} updated", request.CategoryKey);

        return new CategoryImageConfigDto(
            config.Id, config.CategoryKey, config.DisplayName, config.Description,
            config.ImageUrl, config.IconUrl, config.Gradient, config.VehicleCount,
            config.IsTrending, config.DisplayOrder, config.IsVisible, config.Route
        );
    }
}
