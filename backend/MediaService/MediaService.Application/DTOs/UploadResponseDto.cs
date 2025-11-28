namespace MediaService.Application.DTOs;

public class UploadResponseDto
{
    public string MediaId { get; set; } = string.Empty;
    public string UploadUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public Dictionary<string, string> UploadHeaders { get; set; } = new();
    public string StorageKey { get; set; } = string.Empty;
}