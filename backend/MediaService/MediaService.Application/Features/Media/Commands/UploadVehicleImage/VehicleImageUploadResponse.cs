namespace MediaService.Application.Features.Media.Commands.UploadVehicleImage;

/// <summary>
/// Response for a vehicle image upload operation.
/// </summary>
public class VehicleImageUploadResponse
{
    public string MediaId { get; set; } = string.Empty;
    public string OriginalUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public long FileSize { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string ProcessingStatus { get; set; } = "Queued";

    public VehicleImageUploadResponse() { }

    public VehicleImageUploadResponse(
        string mediaId, string originalUrl, string? thumbnailUrl,
        long fileSize, int width, int height, string contentType,
        string processingStatus = "Queued")
    {
        MediaId = mediaId;
        OriginalUrl = originalUrl;
        ThumbnailUrl = thumbnailUrl;
        FileSize = fileSize;
        Width = width;
        Height = height;
        ContentType = contentType;
        ProcessingStatus = processingStatus;
    }
}
