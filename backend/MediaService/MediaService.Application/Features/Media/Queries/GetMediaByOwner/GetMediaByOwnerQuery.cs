using MediaService.Application.DTOs;
using MediaService.Shared;
using MediatR;

namespace MediaService.Application.Features.Media.Queries.GetMediaByOwner;

public class GetMediaByOwnerQuery : IRequest<ApiResponse<GetMediaByOwnerResponse>>
{
    public string OwnerId { get; set; } = string.Empty;
    public string? Context { get; set; }
    public string? MediaType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public bool IncludeVariants { get; set; } = false;

    public GetMediaByOwnerQuery(string ownerId, string? context = null)
    {
        OwnerId = ownerId;
        Context = context;
    }
}
