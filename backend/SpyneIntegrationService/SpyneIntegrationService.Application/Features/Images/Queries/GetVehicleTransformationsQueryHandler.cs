using MediatR;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Application.Features.Images.Queries;

public class GetVehicleTransformationsQueryHandler : IRequestHandler<GetVehicleTransformationsQuery, List<ImageTransformationDto>>
{
    private readonly IImageTransformationRepository _repository;

    public GetVehicleTransformationsQueryHandler(IImageTransformationRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ImageTransformationDto>> Handle(GetVehicleTransformationsQuery request, CancellationToken cancellationToken)
    {
        var transformations = await _repository.GetByVehicleIdAsync(request.VehicleId, cancellationToken);
        return transformations.Select(MapToDto).ToList();
    }

    private static ImageTransformationDto MapToDto(ImageTransformation t) => new()
    {
        Id = t.Id,
        VehicleId = t.VehicleId,
        DealerId = t.DealerId,
        OriginalImageUrl = t.OriginalImageUrl,
        TransformedImageUrl = t.TransformedImageUrl,
        TransformedImageUrlHd = t.TransformedImageUrlHd,
        SpyneJobId = t.SpyneJobId,
        BackgroundPreset = t.BackgroundPreset,
        Status = t.Status,
        LicensePlateMasked = t.LicensePlateMasked,
        QualityEnhanced = t.QualityEnhanced,
        ErrorMessage = t.ErrorMessage,
        ProcessingTimeMs = t.ProcessingTimeMs,
        CreatedAt = t.CreatedAt,
        CompletedAt = t.CompletedAt
    };
}
