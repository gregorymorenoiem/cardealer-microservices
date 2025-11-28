using MediaService.Shared;
using MediatR;

namespace MediaService.Application.Features.Media.Commands.ProcessMedia;

public class ProcessMediaCommand : IRequest<ApiResponse<ProcessMediaResponse>>
{
    public string MediaId { get; set; } = string.Empty;
    public string? ProcessingType { get; set; }
    public Dictionary<string, object>? ProcessingOptions { get; set; }

    public ProcessMediaCommand(string mediaId, string? processingType = null, Dictionary<string, object>? processingOptions = null)
    {
        MediaId = mediaId;
        ProcessingType = processingType;
        ProcessingOptions = processingOptions;
    }
}