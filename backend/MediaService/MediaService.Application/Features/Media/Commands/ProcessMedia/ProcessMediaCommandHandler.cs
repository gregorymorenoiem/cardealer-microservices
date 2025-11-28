using MediaService.Domain.Entities;
using MediaService.Domain.Interfaces.Repositories;
using MediaService.Domain.Interfaces.Services;
using MediaService.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MediaService.Application.Features.Media.Commands.ProcessMedia;

public class ProcessMediaCommandHandler : IRequestHandler<ProcessMediaCommand, ApiResponse<ProcessMediaResponse>>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IImageProcessor _imageProcessor;
    private readonly IVideoProcessor _videoProcessor;
    private readonly IMediaStorageService _storageService;
    private readonly ILogger<ProcessMediaCommandHandler> _logger;

    public ProcessMediaCommandHandler(
        IMediaRepository mediaRepository,
        IImageProcessor imageProcessor,
        IVideoProcessor videoProcessor,
        IMediaStorageService storageService,
        ILogger<ProcessMediaCommandHandler> logger)
    {
        _mediaRepository = mediaRepository;
        _imageProcessor = imageProcessor;
        _videoProcessor = videoProcessor;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<ApiResponse<ProcessMediaResponse>> Handle(ProcessMediaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var mediaAsset = await _mediaRepository.GetByIdAsync(request.MediaId, cancellationToken);
            if (mediaAsset == null)
                return ApiResponse<ProcessMediaResponse>.Fail("Media not found");

            mediaAsset.MarkAsProcessing();
            await _mediaRepository.UpdateAsync(mediaAsset, cancellationToken);

            int variantsGenerated = 0;
            string status = "processed";
            string? message = null;

            try
            {
                switch (mediaAsset.Type)
                {
                    case Domain.Enums.MediaType.Image:
                        variantsGenerated = await ProcessImageAsync((ImageMedia)mediaAsset, cancellationToken);
                        message = $"Generated {variantsGenerated} image variants";
                        break;
                    case Domain.Enums.MediaType.Video:
                        variantsGenerated = await ProcessVideoAsync((VideoMedia)mediaAsset, cancellationToken);
                        message = $"Generated {variantsGenerated} video variants";
                        break;
                    default:
                        message = "No processing required for this media type";
                        break;
                }

                var cdnUrl = await _storageService.GetFileUrlAsync(mediaAsset.StorageKey);
                mediaAsset.MarkAsProcessed(cdnUrl);
            }
            catch (Exception ex)
            {
                mediaAsset.MarkAsFailed($"Processing failed: {ex.Message}");
                status = "failed";
                message = ex.Message;
            }

            await _mediaRepository.UpdateAsync(mediaAsset, cancellationToken);

            var response = new ProcessMediaResponse(
                mediaAsset.Id,
                status,
                variantsGenerated,
                message,
                mediaAsset.ProcessedAt
            );

            return ApiResponse<ProcessMediaResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing media");
            return ApiResponse<ProcessMediaResponse>.Fail("Error processing media");
        }
    }

    private async Task<int> ProcessImageAsync(ImageMedia image, CancellationToken cancellationToken)
    {
        using var originalStream = await _storageService.DownloadFileAsync(image.StorageKey);
        var imageInfo = await _imageProcessor.GetImageInfoAsync(originalStream);
        image.SetDimensions(imageInfo.Width, imageInfo.Height);

        var variants = new[]
        {
            new { Name = "thumb", Width = 200, Height = 200, Quality = 80 },
            new { Name = "small", Width = 400, Height = 400, Quality = 80 },
            new { Name = "medium", Width = 800, Height = 800, Quality = 85 }
        };

        var variantsGenerated = 0;

        foreach (var variant in variants)
        {
            try
            {
                originalStream.Position = 0;
                using var variantStream = await _imageProcessor.CreateThumbnailAsync(
                    originalStream, variant.Width, variant.Height);

                var variantStorageKey = $"{image.StorageKey}_{variant.Name}";
                await _storageService.UploadFileAsync(variantStorageKey, variantStream, image.ContentType);

                var mediaVariant = new MediaVariant(
                    image.Id, variant.Name, variantStorageKey,
                    variant.Width, variant.Height, variantStream.Length,
                    ".jpg", variant.Quality);

                image.AddVariant(mediaVariant);
                variantsGenerated++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create variant {VariantName}", variant.Name);
            }
        }

        return variantsGenerated;
    }

    private async Task<int> ProcessVideoAsync(VideoMedia video, CancellationToken cancellationToken)
    {
        using var originalStream = await _storageService.DownloadFileAsync(video.StorageKey);
        var videoInfo = await _videoProcessor.GetVideoInfoAsync(originalStream);
        video.SetDuration(videoInfo.Duration);
        video.SetDimensions(videoInfo.Width, videoInfo.Height);

        var variantsGenerated = 0;

        try
        {
            originalStream.Position = 0;
            var thumbnails = await _videoProcessor.GenerateThumbnailsAsync(originalStream, 3, 320, 180);

            for (int i = 0; i < thumbnails.Count; i++)
            {
                var thumbnailStorageKey = $"{video.StorageKey}_thumb_{i + 1}.jpg";
                await _storageService.UploadFileAsync(thumbnailStorageKey, thumbnails[i], "image/jpeg");

                var thumbnailVariant = new MediaVariant(
                    video.Id, $"thumb_{i + 1}", thumbnailStorageKey,
                    320, 180, thumbnails[i].Length, ".jpg", 85);

                video.AddVariant(thumbnailVariant);
                variantsGenerated++;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate thumbnails for video");
        }

        return variantsGenerated;
    }
}