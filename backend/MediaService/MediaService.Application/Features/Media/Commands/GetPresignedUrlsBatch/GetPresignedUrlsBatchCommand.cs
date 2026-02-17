using MediaService.Shared;
using MediatR;

namespace MediaService.Application.Features.Media.Commands.GetPresignedUrlsBatch;

/// <summary>
/// Command to generate multiple pre-signed S3 upload URLs in a single request.
/// Enables direct browser-to-S3 uploads for maximum performance.
/// </summary>
public class GetPresignedUrlsBatchCommand : IRequest<ApiResponse<GetPresignedUrlsBatchResponse>>
{
    public List<FileUploadInfo> Files { get; set; } = new();
    public Guid? VehicleId { get; set; }
    public string Category { get; set; } = "vehicles";
    public string OwnerId { get; set; } = string.Empty;
    public Guid DealerId { get; set; }
}

public class FileUploadInfo
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
}
