using MediaService.Application.DTOs;

namespace MediaService.Application.Features.Media.Queries.GetMediaVariants;

public class GetMediaVariantsResponse
{
    public string MediaId { get; set; } = string.Empty;
    public List<MediaVariantDto> Variants { get; set; } = new();

    public GetMediaVariantsResponse() { }

    public GetMediaVariantsResponse(string mediaId, List<MediaVariantDto> variants)
    {
        MediaId = mediaId;
        Variants = variants;
    }
}