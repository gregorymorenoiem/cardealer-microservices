using MediaService.Domain.Entities;
using MediaService.Domain.Interfaces.Repositories;
using MediaService.Domain.Interfaces.Services;
using MediaService.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MediaService.Application.Features.Media.Commands.InitUpload;

public class InitUploadCommandHandler : IRequestHandler<InitUploadCommand, ApiResponse<InitUploadResponse>>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IMediaStorageService _storageService;
    private readonly ILogger<InitUploadCommandHandler> _logger;

    public InitUploadCommandHandler(
        IMediaRepository mediaRepository,
        IMediaStorageService storageService,
        ILogger<InitUploadCommandHandler> logger)
    {
        _mediaRepository = mediaRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<ApiResponse<InitUploadResponse>> Handle(InitUploadCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var mediaType = DetermineMediaType(request.ContentType);
            var storageKey = await _storageService.GenerateStorageKeyAsync(
                request.OwnerId, request.Context, request.FileName);

            MediaAsset mediaAsset = mediaType switch
            {
                // Note: Width and Height are set to 1 as placeholder values.
                // They will be updated when the image is actually processed after upload.
                Domain.Enums.MediaType.Image => new ImageMedia(
                    request.DealerId, request.OwnerId, request.Context, request.FileName,
                    request.ContentType, request.FileSize, storageKey, 1, 1),
                Domain.Enums.MediaType.Video => new VideoMedia(
                    request.DealerId, request.OwnerId, request.Context, request.FileName,
                    request.ContentType, request.FileSize, storageKey),
                Domain.Enums.MediaType.Document => new DocumentMedia(
                    request.DealerId, request.OwnerId, request.Context, request.FileName,
                    request.ContentType, request.FileSize, storageKey),
                _ => new MediaAsset(
                    request.DealerId, request.OwnerId, request.Context, mediaType,
                    request.FileName, request.ContentType, request.FileSize, storageKey)
            };

            var uploadInfo = await _storageService.GenerateUploadUrlAsync(storageKey, request.ContentType);
            await _mediaRepository.AddAsync(mediaAsset, cancellationToken);

            var response = new InitUploadResponse(
                mediaAsset.Id,
                uploadInfo.UploadUrl,
                uploadInfo.ExpiresAt,
                uploadInfo.Headers,
                storageKey
            );

            return ApiResponse<InitUploadResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing upload");
            return ApiResponse<InitUploadResponse>.Fail("Error initializing upload");
        }
    }

    private static Domain.Enums.MediaType DetermineMediaType(string contentType)
    {
        return contentType.ToLower() switch
        {
            var ct when ct.StartsWith("image/") => Domain.Enums.MediaType.Image,
            var ct when ct.StartsWith("video/") => Domain.Enums.MediaType.Video,
            var ct when ct.StartsWith("audio/") => Domain.Enums.MediaType.Audio,
            _ => Domain.Enums.MediaType.Document
        };
    }
}