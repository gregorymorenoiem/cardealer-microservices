using MediaService.Application.DTOs;
using MediaService.Domain.Interfaces.Repositories;
using MediaService.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MediaService.Application.Features.Media.Queries.ListMedia;

public class ListMediaQueryHandler : IRequestHandler<ListMediaQuery, ApiResponse<ListMediaResponse>>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly ILogger<ListMediaQueryHandler> _logger;

    public ListMediaQueryHandler(
        IMediaRepository mediaRepository,
        ILogger<ListMediaQueryHandler> logger)
    {
        _mediaRepository = mediaRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<ListMediaResponse>> Handle(ListMediaQuery request, CancellationToken cancellationToken)
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
            var result = new PaginatedResult<MediaDto>
            {
                Items = mediaDtos,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };

            var response = new ListMediaResponse(result);
            return ApiResponse<ListMediaResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving media list");
            return ApiResponse<ListMediaResponse>.Fail("Error retrieving media list");
        }
    }
}