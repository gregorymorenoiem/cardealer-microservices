using MediaService.Application.DTOs;
using MediaService.Shared;

namespace MediaService.Application.Features.Media.Queries.ListMedia;

public class ListMediaResponse
{
    public PaginatedResult<MediaDto> Media { get; set; } = new();

    public ListMediaResponse() { }

    public ListMediaResponse(PaginatedResult<MediaDto> media)
    {
        Media = media;
    }
}