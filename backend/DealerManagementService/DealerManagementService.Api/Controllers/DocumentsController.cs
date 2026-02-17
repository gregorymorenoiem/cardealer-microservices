using DealerManagementService.Application.DTOs;
using DealerManagementService.Application.Features.Documents.Commands;
using DealerManagementService.Application.Features.Documents.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DealerManagementService.Api.Controllers;

/// <summary>
/// Manage dealer verification documents
/// </summary>
[ApiController]
[Route("api/dealers/{dealerId:guid}/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(IMediator mediator, ILogger<DocumentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all documents for a dealer
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<DealerDocumentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DealerDocumentDto>>> GetDocuments(Guid dealerId)
    {
        var query = new GetDealerDocumentsQuery(dealerId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Upload a document for a dealer
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DealerDocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB max
    public async Task<ActionResult<DealerDocumentDto>> UploadDocument(
        Guid dealerId,
        [FromForm] IFormFile file,
        [FromForm] string type,
        [FromForm] string? expiryDate = null)
    {
        try
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file provided" });
            }

            var allowedTypes = new[] { "application/pdf", "image/jpeg", "image/png" };
            if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return BadRequest(new { message = "Invalid file type. Allowed: PDF, JPG, PNG" });
            }

            if (file.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new { message = "File too large. Maximum: 10MB" });
            }

            // Save file to disk (dealer-documents folder)
            var uploadsDir = Path.Combine("uploads", "dealer-documents", dealerId.ToString());
            Directory.CreateDirectory(uploadsDir);

            var fileExtension = Path.GetExtension(file.FileName);
            var safeFileName = $"{type}_{Guid.NewGuid():N}{fileExtension}";
            var filePath = Path.Combine(uploadsDir, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Build URL for the file
            var fileUrl = $"/uploads/dealer-documents/{dealerId}/{safeFileName}";

            // Parse expiry date if provided
            DateTime? parsedExpiry = null;
            if (!string.IsNullOrWhiteSpace(expiryDate) && DateTime.TryParse(expiryDate, out var expiry))
            {
                parsedExpiry = expiry;
            }

            var command = new UploadDocumentCommand(
                DealerId: dealerId,
                Type: type,
                FileName: file.FileName,
                FileUrl: fileUrl,
                FileKey: filePath,
                FileSizeBytes: file.Length,
                MimeType: file.ContentType,
                ExpiryDate: parsedExpiry
            );

            var result = await _mediator.Send(command);

            return CreatedAtAction(
                nameof(GetDocuments),
                new { dealerId },
                result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document for dealer {DealerId}", dealerId);
            return StatusCode(500, new { message = "Error uploading document" });
        }
    }

    /// <summary>
    /// Delete a document
    /// </summary>
    [HttpDelete("{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteDocument(Guid dealerId, Guid documentId)
    {
        try
        {
            var command = new DeleteDocumentCommand(dealerId, documentId);
            await _mediator.Send(command);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId} for dealer {DealerId}", documentId, dealerId);
            return StatusCode(500, new { message = "Error deleting document" });
        }
    }
}
