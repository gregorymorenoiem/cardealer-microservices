using MediaService.Domain.Entities;
using MediaService.Domain.Interfaces.Repositories;
using MediaService.Domain.Interfaces.Services;
using MediaService.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MediaService.Application.Features.Media.Commands.UploadVehicleImage;

public class UploadVehicleImageCommandHandler : IRequestHandler<UploadVehicleImageCommand, ApiResponse<VehicleImageUploadResponse>>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IImageProcessor _imageProcessor;
    private readonly IMediaStorageService _storageService;
    private readonly ILogger<UploadVehicleImageCommandHandler> _logger;

    private const int MaxOptimizedWidth = 1920;
    private const int OptimizedQuality = 85;

    public UploadVehicleImageCommandHandler(
        IMediaRepository mediaRepository,
        IImageProcessor imageProcessor,
        IMediaStorageService storageService,
        ILogger<UploadVehicleImageCommandHandler> logger)
    {
        _mediaRepository = mediaRepository;
        _imageProcessor = imageProcessor;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<ApiResponse<VehicleImageUploadResponse>> Handle(
        UploadVehicleImageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var file = request.File;
            using var stream = file.OpenReadStream();

            // Get image info
            var imageInfo = await _imageProcessor.GetImageInfoAsync(stream);
            stream.Position = 0;

            // Optionally compress server-side if not done client-side
            Stream uploadStream = stream;
            long finalSize = file.Length;
            int finalWidth = imageInfo.Width;
            int finalHeight = imageInfo.Height;
            var shouldCompress = request.Compress && imageInfo.Width > MaxOptimizedWidth;

            if (shouldCompress)
            {
                var optimized = await _imageProcessor.OptimizeImageAsync(stream, file.ContentType, OptimizedQuality);
                uploadStream = optimized;
                finalSize = optimized.Length;

                // Recalculate dimensions for optimized image
                optimized.Position = 0;
                var optimizedInfo = await _imageProcessor.GetImageInfoAsync(optimized);
                finalWidth = optimizedInfo.Width;
                finalHeight = optimizedInfo.Height;
                optimized.Position = 0;
            }

            // Build storage key
            var vehiclePart = request.VehicleId.HasValue ? request.VehicleId.Value.ToString("N") : "pending";
            var imageTypePart = request.ImageType?.ToLowerInvariant() ?? "general";
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? ".jpg";
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var storageKey = $"vehicles/{vehiclePart}/{imageTypePart}/{fileName}";

            // Upload to S3
            await _storageService.UploadFileAsync(storageKey, uploadStream, file.ContentType);

            // Get public URL
            var url = await _storageService.GetFileUrlAsync(storageKey);

            // Create MediaAsset in DB
            var imageMedia = new ImageMedia(
                request.DealerId,
                request.OwnerId,
                "vehicles",
                file.FileName,
                file.ContentType,
                finalSize,
                storageKey,
                finalWidth,
                finalHeight);

            await _mediaRepository.AddAsync(imageMedia, cancellationToken);

            // Generate inline thumbnail for immediate use
            string? thumbnailUrl = null;
            try
            {
                uploadStream.Position = 0;
                using var thumbStream = await _imageProcessor.CreateThumbnailAsync(uploadStream, 200, 200);
                var thumbKey = $"vehicles/{vehiclePart}/{imageTypePart}/thumb_{fileName}";
                await _storageService.UploadFileAsync(thumbKey, thumbStream, "image/jpeg");
                thumbnailUrl = await _storageService.GetFileUrlAsync(thumbKey);

                var thumbVariant = new MediaVariant(
                    imageMedia.Id, "thumb", thumbKey, 200, 200, thumbStream.Length, ".jpg", 80);
                thumbVariant.SetCdnUrl(thumbnailUrl);
                imageMedia.AddVariant(thumbVariant);
                await _mediaRepository.UpdateAsync(imageMedia, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate inline thumbnail for {MediaId}", imageMedia.Id);
            }

            // Dispose optimized stream if we created one
            if (shouldCompress && uploadStream != stream)
            {
                await uploadStream.DisposeAsync();
            }

            var response = new VehicleImageUploadResponse(
                imageMedia.Id,
                url,
                thumbnailUrl,
                finalSize,
                finalWidth,
                finalHeight,
                file.ContentType,
                "Queued" // Remaining variants will be processed asynchronously
            );

            _logger.LogInformation(
                "Vehicle image uploaded: {MediaId}, Size: {Size}KB, Dimensions: {W}x{H}, Compressed: {Compressed}",
                imageMedia.Id, finalSize / 1024, finalWidth, finalHeight, shouldCompress);

            return ApiResponse<VehicleImageUploadResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading vehicle image");
            return ApiResponse<VehicleImageUploadResponse>.Fail($"Error al subir la imagen: {ex.Message}");
        }
    }
}
