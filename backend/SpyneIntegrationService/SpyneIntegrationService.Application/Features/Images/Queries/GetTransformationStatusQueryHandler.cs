using MediatR;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Application.Features.Images.Queries;

public class GetTransformationStatusQueryHandler : IRequestHandler<GetTransformationStatusQuery, ImageTransformationDto?>
{
    private readonly IImageTransformationRepository _repository;

    public GetTransformationStatusQueryHandler(IImageTransformationRepository repository)
    {
        _repository = repository;
    }

    public async Task<ImageTransformationDto?> Handle(GetTransformationStatusQuery request, CancellationToken cancellationToken)
    {
        var transformation = await _repository.GetByIdAsync(request.TransformationId, cancellationToken);
        
        if (transformation == null)
            return null;

        return MapToDto(transformation);
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
