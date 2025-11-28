using MediaService.Application.DTOs;
using MediaService.Shared;
using MediatR;

namespace MediaService.Application.Features.Media.Queries.GetMedia;

public class GetMediaQuery : IRequest<ApiResponse<GetMediaResponse>>
{
    public string MediaId { get; set; } = string.Empty;
    public bool IncludeVariants { get; set; } = true;

    public GetMediaQuery(string mediaId, bool includeVariants = true)
    {
        MediaId = mediaId;
        IncludeVariants = includeVariants;
    }
}
