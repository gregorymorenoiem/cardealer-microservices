using MediaService.Shared;
using MediatR;

namespace MediaService.Application.Features.Media.Commands.FinalizeUpload;

public class FinalizeUploadCommand : IRequest<ApiResponse<FinalizeUploadResponse>>
{
    public string MediaId { get; set; } = string.Empty;

    public FinalizeUploadCommand(string mediaId)
    {
        MediaId = mediaId;
    }
}
