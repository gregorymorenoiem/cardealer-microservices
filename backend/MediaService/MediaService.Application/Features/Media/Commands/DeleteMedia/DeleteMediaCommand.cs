using MediaService.Shared;
using MediatR;

namespace MediaService.Application.Features.Media.Commands.DeleteMedia;

public class DeleteMediaCommand : IRequest<ApiResponse<DeleteMediaResponse>>
{
    public string MediaId { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public string? Reason { get; set; }

    public DeleteMediaCommand(string mediaId, string requestedBy, string? reason = null)
    {
        MediaId = mediaId;
        RequestedBy = requestedBy;
        Reason = reason;
    }
}