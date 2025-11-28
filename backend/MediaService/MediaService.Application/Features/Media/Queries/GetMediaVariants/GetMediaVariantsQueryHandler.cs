using MediaService.Domain.Interfaces.Repositories;
using MediaService.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MediaService.Application.Features.Media.Queries.GetMediaVariants;

public class GetMediaVariantsQueryHandler : IRequestHandler<GetMediaVariantsQuery, ApiResponse<GetMediaVariantsResponse>>
{
    private readonly IMediaVariantRepository _variantRepository;
    private readonly IMediaRepository _mediaRepository;
    private readonly ILogger<GetMediaVariantsQueryHandler> _logger;

    public GetMediaVariantsQueryHandler(
        IMediaVariantRepository variantRepository,
        IMediaRepository mediaRepository,
        ILogger<GetMediaVariantsQueryHandler> logger)
    {
        _variantRepository = variantRepository;
        _mediaRepository = mediaRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<GetMediaVariantsResponse>> Handle(GetMediaVariantsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var mediaExists = await _mediaRepository.ExistsAsync(request.MediaId, cancellationToken);
            if (!mediaExists)
                return ApiResponse<GetMediaVariantsResponse>.Fail("Media not found");

            List<Domain.Entities.MediaVariant> variants;

            if (!string.IsNullOrEmpty(request.VariantName))
            {
                var variant = await _variantRepository.GetByMediaIdAndNameAsync(request.MediaId, request.VariantName, cancellationToken);
                variants = variant != null ? new List<Domain.Entities.MediaVariant> { variant } : new List<Domain.Entities.MediaVariant>();
            }
            else
            {
                var variantList = await _variantRepository.GetByMediaIdAsync(request.MediaId, cancellationToken);
                variants = variantList.ToList();
            }

            var variantDtos = variants.Select(DTOs.MediaVariantDto.FromEntity).ToList();
            var response = new GetMediaVariantsResponse(request.MediaId, variantDtos);

            return ApiResponse<GetMediaVariantsResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving media variants");
            return ApiResponse<GetMediaVariantsResponse>.Fail("Error retrieving media variants");
        }
    }
}