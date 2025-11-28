using MediaService.Application.DTOs;
using MediaService.Domain.Interfaces.Repositories;
using MediaService.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MediaService.Application.Features.Media.Queries.GetMediaByOwner;

public class GetMediaByOwnerQueryHandler : IRequestHandler<GetMediaByOwnerQuery, ApiResponse<GetMediaByOwnerResponse>>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly ILogger<GetMediaByOwnerQueryHandler> _logger;

    public GetMediaByOwnerQueryHandler(
        IMediaRepository mediaRepository,
        ILogger<GetMediaByOwnerQueryHandler> logger)
    {
        _mediaRepository = mediaRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<GetMediaByOwnerResponse>> Handle(GetMediaByOwnerQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Domain.Enums.MediaType? mediaType = null;
            if (!string.IsNullOrEmpty(request.MediaType) &&
                Enum.TryParse<Domain.Enums.MediaType>(request.MediaType, true, out var parsedType))
            {
                mediaType = parsedType;
            }

            var (items, totalCount) = await _mediaRepository.GetPaginatedAsync(
                ownerId: request.OwnerId,
                context: request.Context,
                type: mediaType,
                fromDate: request.FromDate,
                toDate: request.ToDate,
                page: request.Page,
                pageSize: request.PageSize);

            var mediaDtos = items.Select(DTOs.MediaDto.FromEntity).ToList();
            var paginatedResult = new PaginatedResult<MediaDto>
            {
                Items = mediaDtos,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };

            var response = new GetMediaByOwnerResponse(request.OwnerId, paginatedResult);
            return ApiResponse<GetMediaByOwnerResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving media for owner");
            return ApiResponse<GetMediaByOwnerResponse>.Fail("Error retrieving media for owner");
        }
    }
}