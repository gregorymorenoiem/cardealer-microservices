using MediaService.Domain.Entities;
using MediaService.Domain.Interfaces.Repositories;
using MediaService.Domain.Interfaces.Services;
using MediaService.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MediaService.Application.Features.Media.Commands.GetPresignedUrlsBatch;

public class GetPresignedUrlsBatchCommandHandler
    : IRequestHandler<GetPresignedUrlsBatchCommand, ApiResponse<GetPresignedUrlsBatchResponse>>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IMediaStorageService _storageService;
    private readonly ILogger<GetPresignedUrlsBatchCommandHandler> _logger;

    private static readonly TimeSpan PresignedUrlExpiry = TimeSpan.FromMinutes(15);

    public GetPresignedUrlsBatchCommandHandler(
        IMediaRepository mediaRepository,
        IMediaStorageService storageService,
        ILogger<GetPresignedUrlsBatchCommandHandler> logger)
    {
        _mediaRepository = mediaRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<ApiResponse<GetPresignedUrlsBatchResponse>> Handle(
        GetPresignedUrlsBatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var response = new GetPresignedUrlsBatchResponse();

            foreach (var fileInfo in request.Files)
            {
                var vehiclePart = request.VehicleId?.ToString("N") ?? "pending";
                var extension = Path.GetExtension(fileInfo.FileName)?.ToLowerInvariant() ?? ".jpg";
                var fileName = $"{Guid.NewGuid():N}{extension}";
                var storageKey = $"{request.Category}/{vehiclePart}/{fileName}";

                // Create MediaAsset record (status: Uploaded — awaiting finalization)
                var imageMedia = new ImageMedia(
                    request.DealerId,
                    request.OwnerId,
                    request.Category,
                    fileInfo.FileName,
                    fileInfo.ContentType,
                    fileInfo.Size,
                    storageKey,
                    1, 1); // Placeholder dimensions — updated on finalize

                await _mediaRepository.AddAsync(imageMedia, cancellationToken);

                // Generate pre-signed upload URL
                var uploadInfo = await _storageService.GenerateUploadUrlAsync(
                    storageKey, fileInfo.ContentType, PresignedUrlExpiry);

                response.Items.Add(new PresignedUrlItem
                {
                    MediaId = imageMedia.Id,
                    PresignedUrl = uploadInfo.UploadUrl,
                    ExpiresAt = uploadInfo.ExpiresAt,
                    StorageKey = storageKey,
                    Headers = uploadInfo.Headers
                });
            }

            _logger.LogInformation(
                "Generated {Count} pre-signed URLs for owner {OwnerId}",
                response.Items.Count, request.OwnerId);

            return ApiResponse<GetPresignedUrlsBatchResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating pre-signed URLs batch");
            return ApiResponse<GetPresignedUrlsBatchResponse>.Fail("Error generando URLs de carga");
        }
    }
}
