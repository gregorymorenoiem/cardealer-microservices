using MediaService.Domain.Entities;
using MediaService.Domain.Interfaces.Repositories;
using MediaService.Domain.Interfaces.Services;
using MediaService.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MediaService.Application.Features.Media.Commands.FinalizeUpload;

public class FinalizeUploadCommandHandler : IRequestHandler<FinalizeUploadCommand, ApiResponse<FinalizeUploadResponse>>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IMediaStorageService _storageService;
    private readonly ILogger<FinalizeUploadCommandHandler> _logger;

    public FinalizeUploadCommandHandler(
        IMediaRepository mediaRepository,
        IMediaStorageService storageService,
        ILogger<FinalizeUploadCommandHandler> logger)
    {
        _mediaRepository = mediaRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<ApiResponse<FinalizeUploadResponse>> Handle(FinalizeUploadCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var mediaAsset = await _mediaRepository.GetByIdAsync(request.MediaId, cancellationToken);
            if (mediaAsset == null)
                return ApiResponse<FinalizeUploadResponse>.Fail("Media not found");

            var fileExists = await _storageService.FileExistsAsync(mediaAsset.StorageKey);
            if (!fileExists)
            {
                mediaAsset.MarkAsFailed("File not found in storage");
                await _mediaRepository.UpdateAsync(mediaAsset, cancellationToken);
                return ApiResponse<FinalizeUploadResponse>.Fail("Uploaded file not found");
            }

            var cdnUrl = await _storageService.GetFileUrlAsync(mediaAsset.StorageKey);
            mediaAsset.MarkAsProcessed(cdnUrl);
            await _mediaRepository.UpdateAsync(mediaAsset, cancellationToken);

            var response = new FinalizeUploadResponse(
                mediaAsset.Id,
                mediaAsset.Status.ToString(),
                cdnUrl,
                mediaAsset.ProcessedAt
            );

            return ApiResponse<FinalizeUploadResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finalizing upload");
            return ApiResponse<FinalizeUploadResponse>.Fail("Error finalizing upload");
        }
    }
}