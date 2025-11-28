using MediaService.Application.DTOs;

namespace MediaService.Application.Features.Media.Queries.GetMedia;

public class GetMediaResponse
{
    public MediaDto Media { get; set; } = new();

    public GetMediaResponse() { }

    public GetMediaResponse(MediaDto media)
    {
        Media = media;
    }
}