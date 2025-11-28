using MediaService.Application.DTOs;
using MediaService.Shared;
using MediatR;

namespace MediaService.Application.Features.Media.Queries.GetMediaVariants;

public class GetMediaVariantsQuery : IRequest<ApiResponse<GetMediaVariantsResponse>>
{
    public string MediaId { get; set; } = string.Empty;
    public string? VariantName { get; set; }

    public GetMediaVariantsQuery(string mediaId, string? variantName = null)
    {
        MediaId = mediaId;
        VariantName = variantName;
    }
}