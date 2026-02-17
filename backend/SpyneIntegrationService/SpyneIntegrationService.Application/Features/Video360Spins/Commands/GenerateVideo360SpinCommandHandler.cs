using MediatR;
using Microsoft.Extensions.Logging;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Application.Features.Video360Spins.Commands;

/// <summary>
/// Handler for GenerateVideo360SpinCommand.
/// Sends the video URL to Spyne API to extract frames and generate 360° spin.
/// </summary>
public class GenerateVideo360SpinCommandHandler : IRequestHandler<GenerateVideo360SpinCommand, GenerateVideo360SpinResponse>
{
    private readonly IVideo360SpinRepository _repository;
    private readonly ISpyneApiClient _spyneClient;
    private readonly ILogger<GenerateVideo360SpinCommandHandler> _logger;

    public GenerateVideo360SpinCommandHandler(
        IVideo360SpinRepository repository,
        ISpyneApiClient spyneClient,
        ILogger<GenerateVideo360SpinCommandHandler> logger)
    {
        _repository = repository;
        _spyneClient = spyneClient;
        _logger = logger;
    }

    public async Task<GenerateVideo360SpinResponse> Handle(
        GenerateVideo360SpinCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting video-based 360° spin generation for vehicle {VehicleId} from video: {VideoUrl}", 
            request.VehicleId, 
            request.VideoUrl);

        // Create entity to track the process
        var video360Spin = new Video360Spin
        {
            VehicleId = request.VehicleId,
            DealerId = request.DealerId,
            UserId = request.UserId,
            SourceVideoUrl = request.VideoUrl,
            VideoDurationSeconds = request.VideoDurationSeconds,
            VideoFileSizeBytes = request.VideoFileSizeBytes,
            VideoFormat = request.VideoFormat,
            VideoResolution = request.VideoResolution,
            Type = request.Type,
            BackgroundPreset = request.BackgroundPreset,
            CustomBackgroundId = request.CustomBackgroundId,
            FrameCount = request.FrameCount,
            EnableHotspots = request.EnableHotspots,
            MaskLicensePlate = request.MaskLicensePlate,
            Status = TransformationStatus.Pending
        };

        // Validate the video
        var validationError = video360Spin.GetValidationError();
        if (validationError != null)
        {
            _logger.LogWarning("Video validation failed: {Error}", validationError);
            video360Spin.MarkAsFailed(validationError);
            await _repository.AddAsync(video360Spin, cancellationToken);
            
            return new GenerateVideo360SpinResponse
            {
                SpinId = video360Spin.Id,
                Status = TransformationStatus.Failed,
                Message = validationError,
                EstimatedCompletionMinutes = 0
            };
        }

        // Save initial record
        await _repository.AddAsync(video360Spin, cancellationToken);

        try
        {
            // Build Spyne API request with video URL
            // Using the unified merchandise/process API with spin=true and videoData
            var spyneRequest = new SpyneMerchandiseRequest
            {
                Vin = request.Vin,
                StockNumber = request.StockNumber ?? $"okla-{video360Spin.Id:N}",
                DealerId = request.DealerId?.ToString(),
                
                // We want to process the video and generate a 360° spin
                ProcessImages = false, // We're sending video, not images
                Process360Spin = true,  // Generate 360° spin from video frames
                ProcessFeatureVideo = false,
                
                // Video input
                VideoUrls = new List<string> { request.VideoUrl },
                ImageUrls = new List<string>(), // Empty - we're using video
                
                // Processing options
                BackgroundId = GetBackgroundId(request.BackgroundPreset, request.CustomBackgroundId),
                MaskLicensePlate = request.MaskLicensePlate,
                
                // 360 Spin options
                EnableHotspots = request.EnableHotspots,
                SpinFrameCount = request.FrameCount
            };

            _logger.LogDebug(
                "Sending video to Spyne API for 360° extraction. FrameCount: {FrameCount}, Background: {BgId}", 
                request.FrameCount,
                spyneRequest.BackgroundId);

            // Send to Spyne API
            var response = await _spyneClient.TransformVehicleAsync(spyneRequest, cancellationToken);
            
            // Update entity with Spyne job ID
            video360Spin.MarkAsProcessing(response.JobId);
            await _repository.UpdateAsync(video360Spin, cancellationToken);

            _logger.LogInformation(
                "Video 360° spin job created. SpinId: {SpinId}, SpyneJobId: {JobId}", 
                video360Spin.Id, 
                response.JobId);

            return new GenerateVideo360SpinResponse
            {
                SpinId = video360Spin.Id,
                Status = TransformationStatus.Processing,
                EstimatedCompletionMinutes = CalculateEstimatedMinutes(request.FrameCount),
                Message = "Video enviado a Spyne. Extrayendo frames y generando vista 360°...",
                SpyneJobId = response.JobId,
                StatusCheckUrl = $"/api/video360spins/{video360Spin.Id}/status"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to send video to Spyne API for vehicle {VehicleId}", 
                request.VehicleId);
            
            video360Spin.MarkAsFailed($"Error al enviar video a Spyne: {ex.Message}");
            await _repository.UpdateAsync(video360Spin, cancellationToken);

            return new GenerateVideo360SpinResponse
            {
                SpinId = video360Spin.Id,
                Status = TransformationStatus.Failed,
                Message = $"Error: {ex.Message}",
                EstimatedCompletionMinutes = 0
            };
        }
    }

    private static string GetBackgroundId(BackgroundPreset preset, string? customId)
    {
        if (!string.IsNullOrEmpty(customId))
            return customId;
            
        return preset switch
        {
            BackgroundPreset.Showroom => "20883",    // Showroom gris (dealers)
            BackgroundPreset.White => "16570",       // Blanco infinito
            BackgroundPreset.Studio => "20883",      // Default studio
            BackgroundPreset.Outdoor => "outdoor_1",
            BackgroundPreset.Urban => "urban_1",
            BackgroundPreset.Luxury => "luxury_1",
            _ => "20883"
        };
    }

    private static int CalculateEstimatedMinutes(int frameCount)
    {
        // Estimated processing time based on frame count
        return frameCount switch
        {
            <= 24 => 3,
            <= 36 => 5,
            <= 72 => 8,
            _ => 10
        };
    }
}
