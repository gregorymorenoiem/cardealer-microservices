namespace MediaService.Api.Controllers;

/// <summary>
/// Request body for the batch pre-signed URLs endpoint.
/// </summary>
public class GetPresignedUrlsBatchRequest
{
    public List<PresignedUrlFileInfo> Files { get; set; } = new();
    public Guid? VehicleId { get; set; }
    public string? Category { get; set; } = "vehicles";
}

public class PresignedUrlFileInfo
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
}
