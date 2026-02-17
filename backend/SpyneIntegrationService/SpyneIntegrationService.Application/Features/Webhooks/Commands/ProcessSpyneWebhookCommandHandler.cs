using MediatR;
using Microsoft.Extensions.Logging;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Application.Features.Webhooks.Commands;

public class ProcessSpyneWebhookCommandHandler : IRequestHandler<ProcessSpyneWebhookCommand, WebhookProcessingResult>
{
    private readonly IWebhookEventRepository _webhookRepository;
    private readonly IImageTransformationRepository _imageRepository;
    private readonly ISpinGenerationRepository _spinRepository;
    private readonly IVideoGenerationRepository _videoRepository;
    private readonly ILogger<ProcessSpyneWebhookCommandHandler> _logger;

    public ProcessSpyneWebhookCommandHandler(
        IWebhookEventRepository webhookRepository,
        IImageTransformationRepository imageRepository,
        ISpinGenerationRepository spinRepository,
        IVideoGenerationRepository videoRepository,
        ILogger<ProcessSpyneWebhookCommandHandler> logger)
    {
        _webhookRepository = webhookRepository;
        _imageRepository = imageRepository;
        _spinRepository = spinRepository;
        _videoRepository = videoRepository;
        _logger = logger;
    }

    public async Task<WebhookProcessingResult> Handle(ProcessSpyneWebhookCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing Spyne webhook: Type={Type}, JobId={JobId}", 
            request.TransformationType, request.SpyneJobId);

        var webhookEvent = new SpyneWebhookEvent
        {
            TransformationType = request.TransformationType,
            SpyneJobId = request.SpyneJobId,
            EventType = request.EventType,
            RawPayload = request.RawPayload
        };

        await _webhookRepository.AddAsync(webhookEvent, cancellationToken);

        try
        {
            var result = request.TransformationType switch
            {
                TransformationType.Background => await ProcessImageWebhookAsync(request, cancellationToken),
                TransformationType.Spin => await ProcessSpinWebhookAsync(request, cancellationToken),
                TransformationType.Video => await ProcessVideoWebhookAsync(request, cancellationToken),
                _ => new WebhookProcessingResult { Success = false, Message = "Unknown transformation type" }
            };

            if (result.Success)
            {
                webhookEvent.MarkAsProcessed();
            }
            else
            {
                webhookEvent.MarkAsFailed(result.Message ?? "Processing failed");
            }

            await _webhookRepository.UpdateAsync(webhookEvent, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook for job {JobId}", request.SpyneJobId);
            webhookEvent.MarkAsFailed(ex.Message);
            await _webhookRepository.UpdateAsync(webhookEvent, cancellationToken);

            return new WebhookProcessingResult
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    private async Task<WebhookProcessingResult> ProcessImageWebhookAsync(ProcessSpyneWebhookCommand request, CancellationToken cancellationToken)
    {
        var image = await _imageRepository.GetBySpyneJobIdAsync(request.SpyneJobId, cancellationToken);
        if (image == null)
        {
            return new WebhookProcessingResult { Success = false, Message = "Image not found" };
        }

        if (request.EventType == "completed" && request.ResultUrl != null)
        {
            image.MarkAsCompleted(request.ResultUrl, null, (int)(request.ProcessingTimeMs ?? 0));
        }
        else if (request.EventType == "failed")
        {
            image.MarkAsFailed(request.ErrorMessage ?? "Unknown error");
        }

        await _imageRepository.UpdateAsync(image, cancellationToken);

        return new WebhookProcessingResult
        {
            Success = true,
            TransformationId = image.Id,
            Message = $"Image {request.EventType}"
        };
    }

    private async Task<WebhookProcessingResult> ProcessSpinWebhookAsync(ProcessSpyneWebhookCommand request, CancellationToken cancellationToken)
    {
        var spin = await _spinRepository.GetBySpyneJobIdAsync(request.SpyneJobId, cancellationToken);
        if (spin == null)
        {
            return new WebhookProcessingResult { Success = false, Message = "Spin not found" };
        }

        if (request.EventType == "completed" && request.ResultUrl != null)
        {
            spin.MarkAsCompleted(
                request.ResultUrl, 
                request.EmbedCode ?? "",
                request.ThumbnailUrl,
                (int)(request.ProcessingTimeMs ?? 0));
        }
        else if (request.EventType == "failed")
        {
            spin.MarkAsFailed(request.ErrorMessage ?? "Unknown error");
        }

        await _spinRepository.UpdateAsync(spin, cancellationToken);

        return new WebhookProcessingResult
        {
            Success = true,
            TransformationId = spin.Id,
            Message = $"Spin {request.EventType}"
        };
    }

    private async Task<WebhookProcessingResult> ProcessVideoWebhookAsync(ProcessSpyneWebhookCommand request, CancellationToken cancellationToken)
    {
        var video = await _videoRepository.GetBySpyneJobIdAsync(request.SpyneJobId, cancellationToken);
        if (video == null)
        {
            return new WebhookProcessingResult { Success = false, Message = "Video not found" };
        }

        if (request.EventType == "completed" && request.ResultUrl != null)
        {
            video.MarkAsCompleted(
                request.ResultUrl, 
                request.ThumbnailUrl ?? "",
                request.FileSizeBytes ?? 0, 
                (int)(request.ProcessingTimeMs ?? 0));
        }
        else if (request.EventType == "failed")
        {
            video.MarkAsFailed(request.ErrorMessage ?? "Unknown error");
        }

        await _videoRepository.UpdateAsync(video, cancellationToken);

        return new WebhookProcessingResult
        {
            Success = true,
            TransformationId = video.Id,
            Message = $"Video {request.EventType}"
        };
    }
}
