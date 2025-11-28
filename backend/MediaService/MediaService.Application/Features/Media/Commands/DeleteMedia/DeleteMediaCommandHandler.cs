using MediaService.Domain.Entities;
using MediaService.Domain.Interfaces.Repositories;
using MediaService.Domain.Interfaces.Services;
using MediaService.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MediaService.Application.Features.Media.Commands.DeleteMedia;

public class DeleteMediaCommandHandler : IRequestHandler<DeleteMediaCommand, ApiResponse<DeleteMediaResponse>>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IMediaVariantRepository _variantRepository;
    private readonly IMediaStorageService _storageService;
    private readonly ILogger<DeleteMediaCommandHandler> _logger;

    public DeleteMediaCommandHandler(
        IMediaRepository mediaRepository,
        IMediaVariantRepository variantRepository,
        IMediaStorageService storageService,
        ILogger<DeleteMediaCommandHandler> logger)
    {
        _mediaRepository = mediaRepository;
        _variantRepository = variantRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<ApiResponse<DeleteMediaResponse>> Handle(DeleteMediaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var mediaAsset = await _mediaRepository.GetByIdAsync(request.MediaId, cancellationToken);
            if (mediaAsset == null)
                return ApiResponse<DeleteMediaResponse>.Fail("Media not found");

            var storageKeys = mediaAsset.GetAllStorageKeys();
            var deletedFiles = 0;

            foreach (var storageKey in storageKeys)
            {
                try
                {
                    await _storageService.DeleteFileAsync(storageKey);
                    deletedFiles++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete file from storage: {StorageKey}", storageKey);
                }
            }

            await _variantRepository.DeleteByMediaIdAsync(request.MediaId, cancellationToken);
            await _mediaRepository.DeleteAsync(mediaAsset, cancellationToken);

            var response = new DeleteMediaResponse(
                request.MediaId,
                true,
                deletedFiles,
                $"Successfully deleted media and {deletedFiles} associated files"
            );

            return ApiResponse<DeleteMediaResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting media");
            return ApiResponse<DeleteMediaResponse>.Fail("Error deleting media");
        }
    }
}