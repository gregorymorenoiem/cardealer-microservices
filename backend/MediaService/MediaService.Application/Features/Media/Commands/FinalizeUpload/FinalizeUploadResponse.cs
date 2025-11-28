namespace MediaService.Application.Features.Media.Commands.FinalizeUpload;

public class FinalizeUploadResponse
{
    public string MediaId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? CdnUrl { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public FinalizeUploadResponse() { }

    public FinalizeUploadResponse(string mediaId, string status, string? cdnUrl = null, DateTime? processedAt = null)
    {
        MediaId = mediaId;
        Status = status;
        CdnUrl = cdnUrl;
        ProcessedAt = processedAt;
    }
}