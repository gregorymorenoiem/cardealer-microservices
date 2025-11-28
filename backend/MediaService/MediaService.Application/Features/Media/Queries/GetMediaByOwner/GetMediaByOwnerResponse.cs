using MediaService.Application.DTOs;
using MediaService.Shared;

namespace MediaService.Application.Features.Media.Queries.GetMediaByOwner;

public class GetMediaByOwnerResponse
{
    public string OwnerId { get; set; } = string.Empty;
    public PaginatedResult<MediaDto> Media { get; set; } = new();

    public GetMediaByOwnerResponse() { }

    public GetMediaByOwnerResponse(string ownerId, PaginatedResult<MediaDto> media)
    {
        OwnerId = ownerId;
        Media = media;
    }
}