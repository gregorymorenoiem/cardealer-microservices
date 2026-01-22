using MediatR;
using Microsoft.Extensions.Logging;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Application.Features.Videos.Commands;

public class GenerateVideoCommandHandler : IRequestHandler<GenerateVideoCommand, GenerateVideoResponse>
{
    private readonly IVideoGenerationRepository _repository;
    private readonly ISpyneApiClient _spyneClient;
    private readonly ILogger<GenerateVideoCommandHandler> _logger;

    public GenerateVideoCommandHandler(
        IVideoGenerationRepository repository,
        ISpyneApiClient spyneClient,
        ILogger<GenerateVideoCommandHandler> logger)
    {
        _repository = repository;
        _spyneClient = spyneClient;
        _logger = logger;
    }

    public async Task<GenerateVideoResponse> Handle(GenerateVideoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting video generation for vehicle {VehicleId} with style {Style}", 
            request.VehicleId, request.Style);

        var video = new VideoGeneration
        {
            VehicleId = request.VehicleId,
            DealerId = request.DealerId,
            SourceImageUrls = request.ImageUrls,
            Style = request.Style,
            OutputFormat = request.OutputFormat,
            BackgroundPreset = request.BackgroundPreset,
            CustomBackgroundId = request.CustomBackgroundId,
            IncludeMusic = request.IncludeMusic,
            MusicTrackId = request.MusicTrackId,
            RequestedDuration = request.DurationSeconds,
            Status = TransformationStatus.Pending
        };

        await _repository.AddAsync(video, cancellationToken);

        try
        {
            var spyneRequest = new SpyneVideoRequest
            {
                ImageUrls = request.ImageUrls,
                Style = request.Style.ToString().ToLower(),
                Format = GetFormatString(request.OutputFormat),
                IncludeMusic = request.IncludeMusic,
                MusicTrackId = request.MusicTrackId,
                DurationSeconds = request.DurationSeconds ?? 30,
                WebhookUrl = GetWebhookUrl()
            };

            var response = await _spyneClient.GenerateVideoAsync(spyneRequest, cancellationToken);
            
            video.MarkAsProcessing(response.JobId);
            await _repository.UpdateAsync(video, cancellationToken);

            _logger.LogInformation("Video generation job {JobId} created for vehicle {VehicleId}", 
                response.JobId, request.VehicleId);

            return new GenerateVideoResponse
            {
                VideoId = video.Id,
                Status = TransformationStatus.Processing,
                EstimatedCompletionMinutes = response.EstimatedMinutes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start video generation for vehicle {VehicleId}", request.VehicleId);
            video.MarkAsFailed(ex.Message);
            await _repository.UpdateAsync(video, cancellationToken);

            return new GenerateVideoResponse
            {
                VideoId = video.Id,
                Status = TransformationStatus.Failed,
                EstimatedCompletionMinutes = 0
            };
        }
    }

    private static string GetFormatString(VideoFormat format) => format switch
    {
        VideoFormat.Mp4_720p => "mp4_720p",
        VideoFormat.Mp4_1080p => "mp4_1080p",
        VideoFormat.Mp4_4K => "mp4_4k",
        VideoFormat.Webm_720p => "webm_720p",
        VideoFormat.Webm_1080p => "webm_1080p",
        VideoFormat.Horizontal => "horizontal",
        VideoFormat.Vertical => "vertical",
        VideoFormat.Square => "square",
        _ => "mp4_1080p"
    };

    private static string GetWebhookUrl() => 
        Environment.GetEnvironmentVariable("SPYNE_WEBHOOK_URL") ?? "https://api.okla.com.do/api/spyne/webhooks";
}
