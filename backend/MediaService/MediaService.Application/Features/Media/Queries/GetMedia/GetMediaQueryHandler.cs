using MediaService.Domain.Interfaces.Repositories;
using MediaService.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MediaService.Application.Features.Media.Queries.GetMedia;

public class GetMediaQueryHandler : IRequestHandler<GetMediaQuery, ApiResponse<GetMediaResponse>>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly ILogger<GetMediaQueryHandler> _logger;

    public GetMediaQueryHandler(
        IMediaRepository mediaRepository,
        ILogger<GetMediaQueryHandler> logger)
    {
        _mediaRepository = mediaRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<GetMediaResponse>> Handle(GetMediaQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var mediaAsset = await _mediaRepository.GetByIdAsync(request.MediaId, cancellationToken);
            if (mediaAsset == null)
                return ApiResponse<GetMediaResponse>.Fail("Media not found");

            var mediaDto = DTOs.MediaDto.FromEntity(mediaAsset);
            var response = new GetMediaResponse(mediaDto);

            return ApiResponse<GetMediaResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving media");
            return ApiResponse<GetMediaResponse>.Fail("Error retrieving media");
        }
    }
}