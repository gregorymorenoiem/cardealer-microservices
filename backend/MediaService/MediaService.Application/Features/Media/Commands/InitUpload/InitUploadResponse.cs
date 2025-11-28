namespace MediaService.Application.Features.Media.Commands.InitUpload;

public class InitUploadResponse
{
    public string MediaId { get; set; } = string.Empty;
    public string UploadUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public Dictionary<string, string> UploadHeaders { get; set; } = new();
    public string StorageKey { get; set; } = string.Empty;

    public InitUploadResponse() { }

    public InitUploadResponse(string mediaId, string uploadUrl, DateTime expiresAt, Dictionary<string, string> uploadHeaders, string storageKey)
    {
        MediaId = mediaId;
        UploadUrl = uploadUrl;
        ExpiresAt = expiresAt;
        UploadHeaders = uploadHeaders;
        StorageKey = storageKey;
    }
}