using MediatR;
using Microsoft.Extensions.Logging;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Application.Features.Spins.Commands;

public class GenerateSpinCommandHandler : IRequestHandler<GenerateSpinCommand, GenerateSpinResponse>
{
    private readonly ISpinGenerationRepository _repository;
    private readonly ISpyneApiClient _spyneClient;
    private readonly ILogger<GenerateSpinCommandHandler> _logger;

    public GenerateSpinCommandHandler(
        ISpinGenerationRepository repository,
        ISpyneApiClient spyneClient,
        ILogger<GenerateSpinCommandHandler> logger)
    {
        _repository = repository;
        _spyneClient = spyneClient;
        _logger = logger;
    }

    public async Task<GenerateSpinResponse> Handle(GenerateSpinCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting 360 spin generation for vehicle {VehicleId} with {Count} images", 
            request.VehicleId, request.ImageUrls.Count);

        var spin = new SpinGeneration
        {
            VehicleId = request.VehicleId,
            DealerId = request.DealerId,
            SourceImageUrls = request.ImageUrls,
            BackgroundPreset = request.BackgroundPreset,
            CustomBackgroundId = request.CustomBackgroundId,
            ReceivedImageCount = request.ImageUrls.Count,
            RequiredImageCount = 36, // Standard 10° increments
            Status = TransformationStatus.Pending
        };

        await _repository.AddAsync(spin, cancellationToken);

        if (!spin.HasSufficientImages())
        {
            _logger.LogWarning("Insufficient images for spin: {Received}/{Required}", 
                spin.ReceivedImageCount, spin.RequiredImageCount);
            
            return new GenerateSpinResponse
            {
                SpinId = spin.Id,
                Status = TransformationStatus.Pending,
                RequiredImageCount = spin.RequiredImageCount,
                ReceivedImageCount = spin.ReceivedImageCount,
                HasSufficientImages = false,
                EstimatedCompletionMinutes = 0
            };
        }

        try
        {
            var spyneRequest = new SpyneSpinRequest
            {
                ImageUrls = request.ImageUrls,
                BackgroundId = request.CustomBackgroundId ?? GetBackgroundId(request.BackgroundPreset),
                OutputFormat = "interactive",
                WebhookUrl = GetWebhookUrl()
            };

            var response = await _spyneClient.GenerateSpinAsync(spyneRequest, cancellationToken);
            
            spin.MarkAsProcessing(response.JobId);
            await _repository.UpdateAsync(spin, cancellationToken);

            _logger.LogInformation("Spin generation job {JobId} created for vehicle {VehicleId}", 
                response.JobId, request.VehicleId);

            return new GenerateSpinResponse
            {
                SpinId = spin.Id,
                Status = TransformationStatus.Processing,
                RequiredImageCount = spin.RequiredImageCount,
                ReceivedImageCount = spin.ReceivedImageCount,
                HasSufficientImages = true,
                EstimatedCompletionMinutes = response.EstimatedMinutes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start spin generation for vehicle {VehicleId}", request.VehicleId);
            spin.MarkAsFailed(ex.Message);
            await _repository.UpdateAsync(spin, cancellationToken);

            return new GenerateSpinResponse
            {
                SpinId = spin.Id,
                Status = TransformationStatus.Failed,
                RequiredImageCount = spin.RequiredImageCount,
                ReceivedImageCount = spin.ReceivedImageCount,
                HasSufficientImages = true,
                EstimatedCompletionMinutes = 0
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
        _ => "studio_1"
    };

    private static string GetWebhookUrl() => 
        Environment.GetEnvironmentVariable("SPYNE_WEBHOOK_URL") ?? "https://api.okla.com.do/api/spyne/webhooks";
}
