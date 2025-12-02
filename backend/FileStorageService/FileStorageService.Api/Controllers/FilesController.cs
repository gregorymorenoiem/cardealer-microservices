using FileStorageService.Core.Interfaces;
using FileStorageService.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace FileStorageService.Api.Controllers;

/// <summary>
/// File storage API controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        IFileStorageService fileStorageService,
        ILogger<FilesController> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Initialize a file upload with presigned URL
    /// </summary>
    [HttpPost("init-upload")]
    [ProducesResponseType(typeof(UploadInitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadInitResponse>> InitializeUpload(
        [FromBody] PresignedUrlRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _fileStorageService.InitializeUploadAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid upload request");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Finalize upload after client completes upload to presigned URL
    /// </summary>
    [HttpPost("{fileId}/finalize")]
    [ProducesResponseType(typeof(UploadResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UploadResult>> FinalizeUpload(
        string fileId,
        CancellationToken cancellationToken)
    {
        var result = await _fileStorageService.FinalizeUploadAsync(fileId, cancellationToken);

        if (!result.Success)
        {
            if (result.ErrorCode == "FILE_NOT_FOUND")
            {
                return NotFound(new ProblemDetails
                {
                    Title = "File Not Found",
                    Detail = result.ErrorMessage,
                    Status = StatusCodes.Status404NotFound
                });
            }

            if (result.ErrorCode == "VIRUS_DETECTED")
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Virus Detected",
                    Detail = result.ErrorMessage,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            return BadRequest(new ProblemDetails
            {
                Title = "Upload Failed",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Upload a file directly (server-side)
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(104857600)] // 100MB
    [ProducesResponseType(typeof(UploadResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadResult>> Upload(
        IFormFile file,
        [FromQuery] string ownerId,
        [FromQuery] string? context = null,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid File",
                Detail = "No file was provided",
                Status = StatusCodes.Status400BadRequest
            });
        }

        await using var stream = file.OpenReadStream();
        var result = await _fileStorageService.UploadAsync(
            stream,
            file.FileName,
            file.ContentType,
            ownerId,
            context,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Upload Failed",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Upload multiple files
    /// </summary>
    [HttpPost("upload-batch")]
    [RequestSizeLimit(524288000)] // 500MB
    [ProducesResponseType(typeof(BatchUploadResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<BatchUploadResult>> UploadBatch(
        List<IFormFile> files,
        [FromQuery] string ownerId,
        [FromQuery] string? context = null,
        CancellationToken cancellationToken = default)
    {
        var batchResult = new BatchUploadResult();

        foreach (var file in files)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await using var stream = file.OpenReadStream();
            var result = await _fileStorageService.UploadAsync(
                stream,
                file.FileName,
                file.ContentType,
                ownerId,
                context,
                cancellationToken);

            batchResult.AddResult(result);
        }

        batchResult.Complete();
        return Ok(batchResult);
    }

    /// <summary>
    /// Download a file
    /// </summary>
    [HttpGet("{fileId}/download")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(string fileId, CancellationToken cancellationToken)
    {
        var result = await _fileStorageService.DownloadAsync(fileId, cancellationToken);

        if (!result.Success)
        {
            return NotFound(new ProblemDetails
            {
                Title = "File Not Found",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status404NotFound
            });
        }

        return File(result.Stream!, result.ContentType!, result.FileName);
    }

    /// <summary>
    /// Get download URL for a file
    /// </summary>
    [HttpGet("{fileId}/download-url")]
    [ProducesResponseType(typeof(PresignedUrl), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PresignedUrl>> GetDownloadUrl(
        string fileId,
        [FromQuery] int expirationMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = await _fileStorageService.GetDownloadUrlAsync(fileId, expirationMinutes, cancellationToken);
            return Ok(url);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "File Not Found",
                Detail = $"File {fileId} not found",
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    [HttpDelete("{fileId}")]
    [ProducesResponseType(typeof(DeleteResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteResult>> Delete(string fileId, CancellationToken cancellationToken)
    {
        var result = await _fileStorageService.DeleteAsync(fileId, cancellationToken);

        if (!result.Success)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Delete Failed",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get file information
    /// </summary>
    [HttpGet("{fileId}")]
    [ProducesResponseType(typeof(StoredFile), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StoredFile>> GetFileInfo(string fileId, CancellationToken cancellationToken)
    {
        var file = await _fileStorageService.GetFileInfoAsync(fileId, cancellationToken);

        if (file == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "File Not Found",
                Detail = $"File {fileId} not found",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(file);
    }

    /// <summary>
    /// List files for an owner
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StoredFile>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StoredFile>>> ListFiles(
        [FromQuery] string ownerId,
        [FromQuery] string? context = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var files = await _fileStorageService.ListFilesAsync(ownerId, context, skip, take, cancellationToken);
        return Ok(files);
    }

    /// <summary>
    /// Copy a file
    /// </summary>
    [HttpPost("{fileId}/copy")]
    [ProducesResponseType(typeof(StoredFile), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StoredFile>> CopyFile(
        string fileId,
        [FromQuery] string? newOwnerId = null,
        [FromQuery] string? newContext = null,
        CancellationToken cancellationToken = default)
    {
        var newFile = await _fileStorageService.CopyFileAsync(fileId, newOwnerId, newContext, cancellationToken);

        if (newFile == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "File Not Found",
                Detail = $"Source file {fileId} not found",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(newFile);
    }

    /// <summary>
    /// Rescan a file for viruses
    /// </summary>
    [HttpPost("{fileId}/rescan")]
    [ProducesResponseType(typeof(ScanResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScanResult>> RescanFile(string fileId, CancellationToken cancellationToken)
    {
        var result = await _fileStorageService.RescanFileAsync(fileId, cancellationToken);

        if (result.Status == ScanStatus.Failed && result.ErrorMessage?.Contains("not found") == true)
        {
            return NotFound(new ProblemDetails
            {
                Title = "File Not Found",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Refresh file metadata
    /// </summary>
    [HttpPost("{fileId}/refresh-metadata")]
    [ProducesResponseType(typeof(FileMetadata), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FileMetadata>> RefreshMetadata(string fileId, CancellationToken cancellationToken)
    {
        var metadata = await _fileStorageService.RefreshMetadataAsync(fileId, cancellationToken);

        if (metadata == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "File Not Found",
                Detail = $"File {fileId} not found",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(metadata);
    }

    /// <summary>
    /// Generate image variants
    /// </summary>
    [HttpPost("{fileId}/generate-variants")]
    [ProducesResponseType(typeof(IEnumerable<FileVariant>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<FileVariant>>> GenerateVariants(
        string fileId,
        [FromBody] List<VariantConfig> variants,
        CancellationToken cancellationToken)
    {
        var result = await _fileStorageService.GenerateVariantsAsync(fileId, variants, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update file tags
    /// </summary>
    [HttpPut("{fileId}/tags")]
    [ProducesResponseType(typeof(StoredFile), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StoredFile>> UpdateTags(
        string fileId,
        [FromBody] Dictionary<string, string> tags,
        CancellationToken cancellationToken)
    {
        var file = await _fileStorageService.UpdateTagsAsync(fileId, tags, cancellationToken);

        if (file == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "File Not Found",
                Detail = $"File {fileId} not found",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(file);
    }

    /// <summary>
    /// Get storage statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(StorageStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<StorageStatistics>> GetStatistics(
        [FromQuery] string? ownerId = null,
        CancellationToken cancellationToken = default)
    {
        var stats = await _fileStorageService.GetStatisticsAsync(ownerId, cancellationToken);
        return Ok(stats);
    }
}
