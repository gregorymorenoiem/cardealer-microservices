using MediaService.Shared;
using MediatR;

namespace MediaService.Application.Features.Media.Commands.InitUpload;

public class InitUploadCommand : IRequest<ApiResponse<InitUploadResponse>>
{
    public Guid DealerId { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public string? Context { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }

    public InitUploadCommand(Guid dealerId, string ownerId, string? context, string fileName, string contentType, long fileSize)
    {
        DealerId = dealerId;
        OwnerId = ownerId;
        Context = context;
        FileName = fileName;
        ContentType = contentType;
        FileSize = fileSize;
    }
}

