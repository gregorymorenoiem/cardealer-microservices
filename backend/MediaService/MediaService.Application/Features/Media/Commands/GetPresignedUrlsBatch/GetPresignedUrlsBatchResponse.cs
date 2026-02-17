namespace MediaService.Application.Features.Media.Commands.GetPresignedUrlsBatch;

public class GetPresignedUrlsBatchResponse
{
    public List<PresignedUrlItem> Items { get; set; } = new();
}

public class PresignedUrlItem
{
    public string MediaId { get; set; } = string.Empty;
    public string PresignedUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
}
