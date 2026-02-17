using MediatR;
using Microsoft.Extensions.Logging;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Application.Features.Images.Commands;

public class TransformBatchImagesCommandHandler : IRequestHandler<TransformBatchImagesCommand, TransformBatchResponse>
{
    private readonly IImageTransformationRepository _repository;
    private readonly ISpyneApiClient _spyneClient;
    private readonly ILogger<TransformBatchImagesCommandHandler> _logger;

    public TransformBatchImagesCommandHandler(
        IImageTransformationRepository repository,
        ISpyneApiClient spyneClient,
        ILogger<TransformBatchImagesCommandHandler> logger)
    {
        _repository = repository;
        _spyneClient = spyneClient;
        _logger = logger;
    }

    public async Task<TransformBatchResponse> Handle(TransformBatchImagesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting batch transformation of {Count} images for vehicle {VehicleId}", 
            request.ImageUrls.Count, request.VehicleId);

        var transformations = new List<ImageTransformation>();

        foreach (var imageUrl in request.ImageUrls)
        {
            var transformation = new ImageTransformation
            {
                VehicleId = request.VehicleId,
                DealerId = request.DealerId,
                OriginalImageUrl = imageUrl,
                BackgroundPreset = request.BackgroundPreset,
                CustomBackgroundId = request.CustomBackgroundId,
                LicensePlateMasked = request.MaskLicensePlate,
                QualityEnhanced = request.EnhanceQuality,
                Status = TransformationStatus.Pending
            };

            await _repository.AddAsync(transformation, cancellationToken);
            transformations.Add(transformation);
        }

        try
        {
            var spyneRequests = request.ImageUrls.Select(url => new SpyneTransformRequest
            {
                ImageUrl = url,
                BackgroundId = request.CustomBackgroundId ?? GetBackgroundId(request.BackgroundPreset),
                MaskLicensePlate = request.MaskLicensePlate,
                EnhanceQuality = request.EnhanceQuality,
                WebhookUrl = GetWebhookUrl()
            }).ToList();

            var response = await _spyneClient.TransformBatchAsync(spyneRequests, cancellationToken);

            // Update all transformations with job ID
            foreach (var transformation in transformations)
            {
                transformation.MarkAsProcessing(response.JobId);
                await _repository.UpdateAsync(transformation, cancellationToken);
            }

            _logger.LogInformation("Batch transformation job {JobId} created with {Count} images", 
                response.JobId, request.ImageUrls.Count);

            return new TransformBatchResponse
            {
                Transformations = transformations.Select(MapToDto).ToList(),
                TotalImages = request.ImageUrls.Count,
                EstimatedCompletionSeconds = response.EstimatedSeconds ?? 60
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start batch transformation for vehicle {VehicleId}", request.VehicleId);
            
            foreach (var transformation in transformations)
            {
                transformation.MarkAsFailed(ex.Message);
                await _repository.UpdateAsync(transformation, cancellationToken);
            }

            return new TransformBatchResponse
            {
                Transformations = transformations.Select(MapToDto).ToList(),
                TotalImages = request.ImageUrls.Count,
                EstimatedCompletionSeconds = 0
            };
        }
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
