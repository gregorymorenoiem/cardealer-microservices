namespace MediaService.Application.Features.Media.Commands.ProcessMedia;

public class ProcessMediaResponse
{
    public string MediaId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int VariantsGenerated { get; set; }
    public string? Message { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public ProcessMediaResponse() { }

    public ProcessMediaResponse(string mediaId, string status, int variantsGenerated, string? message = null, DateTime? processedAt = null)
    {
        MediaId = mediaId;
        Status = status;
        VariantsGenerated = variantsGenerated;
        Message = message;
        ProcessedAt = processedAt;
    }
}