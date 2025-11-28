namespace MediaService.Application.Features.Media.Commands.DeleteMedia;

public class DeleteMediaResponse
{
    public string MediaId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int DeletedFiles { get; set; }
    public string? Message { get; set; }

    public DeleteMediaResponse() { }

    public DeleteMediaResponse(string mediaId, bool success, int deletedFiles, string? message = null)
    {
        MediaId = mediaId;
        Success = success;
        DeletedFiles = deletedFiles;
        Message = message;
    }
}