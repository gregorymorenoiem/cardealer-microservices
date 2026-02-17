using MediatR;
using Microsoft.Extensions.Logging;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Application.Features.Images.Commands;

public class TransformImageCommandHandler : IRequestHandler<TransformImageCommand, ImageTransformationDto>
{
    private readonly IImageTransformationRepository _repository;
    private readonly ISpyneApiClient _spyneClient;
    private readonly ILogger<TransformImageCommandHandler> _logger;

    public TransformImageCommandHandler(
        IImageTransformationRepository repository,
        ISpyneApiClient spyneClient,
        ILogger<TransformImageCommandHandler> logger)
    {
        _repository = repository;
        _spyneClient = spyneClient;
        _logger = logger;
    }

    public async Task<ImageTransformationDto> Handle(TransformImageCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting image transformation for vehicle {VehicleId}", request.VehicleId);

        // Create transformation record
        var transformation = new ImageTransformation
        {
            VehicleId = request.VehicleId,
            DealerId = request.DealerId,
            OriginalImageUrl = request.OriginalImageUrl,
            BackgroundPreset = request.BackgroundPreset,
            CustomBackgroundId = request.CustomBackgroundId,
            LicensePlateMasked = request.MaskLicensePlate,
            QualityEnhanced = request.EnhanceQuality,
            Status = TransformationStatus.Pending
        };

        await _repository.AddAsync(transformation, cancellationToken);

        try
        {
            // Call Spyne API
            var spyneRequest = new SpyneTransformRequest
            {
                ImageUrl = request.OriginalImageUrl,
                BackgroundId = request.CustomBackgroundId ?? GetBackgroundId(request.BackgroundPreset),
                MaskLicensePlate = request.MaskLicensePlate,
                EnhanceQuality = request.EnhanceQuality,
                WebhookUrl = GetWebhookUrl()
            };

            var response = await _spyneClient.TransformImageAsync(spyneRequest, cancellationToken);
            
            transformation.MarkAsProcessing(response.JobId);
            await _repository.UpdateAsync(transformation, cancellationToken);

            _logger.LogInformation("Image transformation job {JobId} created for vehicle {VehicleId}", 
                response.JobId, request.VehicleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start image transformation for vehicle {VehicleId}", request.VehicleId);
            transformation.MarkAsFailed(ex.Message);
            await _repository.UpdateAsync(transformation, cancellationToken);
        }

        return MapToDto(transformation);
    }

    private static string GetBackgroundId(BackgroundPreset preset) => preset switch
    {
        BackgroundPreset.Showroom => "showroom_1",
        BackgroundPreset.Outdoor => "outdoor_1",
        BackgroundPreset.Studio => "studio_1",
        BackgroundPreset.White => "white_1",
        BackgroundPreset.Urban => "urban_1",
        BackgroundPreset.Luxury => "luxury_1",
        _ => "showroom_1"
    };

    private static string GetWebhookUrl() => 
        Environment.GetEnvironmentVariable("SPYNE_WEBHOOK_URL") ?? "https://api.okla.com.do/api/spyne/webhooks";

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
