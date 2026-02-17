using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using InventoryManagementService.Application.DTOs;
using InventoryManagementService.Application.Features.Inventory.Commands;
using InventoryManagementService.Application.Features.Inventory.Queries;
using InventoryManagementService.Domain.Entities;

namespace InventoryManagementService.Api.Controllers;

[ApiController]
[Route("api/inventory/[controller]")]
[Authorize]
public class BulkImportController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BulkImportController> _logger;

    public BulkImportController(IMediator mediator, ILogger<BulkImportController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all bulk import jobs for a dealer
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<BulkImportJobDto>>> GetJobs([FromQuery] Guid dealerId, [FromQuery] int limit = 20)
    {
        try
        {
            var query = new GetBulkImportJobsQuery
            {
                DealerId = dealerId,
                Limit = limit
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bulk import jobs for dealer {DealerId}", dealerId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get a single bulk import job by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<BulkImportJobDto>> GetJob(Guid id)
    {
        try
        {
            var query = new GetBulkImportJobQuery { JobId = id };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound(new { message = "Import job not found" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bulk import job {JobId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Start a new bulk import job
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BulkImportJobDto>> StartImport([FromBody] StartBulkImportRequest request)
    {
        try
        {
            if (!Enum.TryParse<ImportFileType>(request.FileType, out var fileType))
            {
                return BadRequest(new { message = "Invalid file type. Allowed values: CSV, Excel, JSON" });
            }

            var command = new StartBulkImportCommand
            {
                DealerId = request.DealerId,
                UserId = request.UserId,
                FileName = request.FileName,
                FileUrl = request.FileUrl,
                FileSizeBytes = request.FileSizeBytes,
                FileType = fileType
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetJob), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting bulk import");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Upload a CSV file for import
    /// </summary>
    [HttpPost("upload")]
    public async Task<ActionResult<BulkImportJobDto>> UploadFile(
        [FromQuery] Guid dealerId,
        [FromQuery] Guid userId,
        IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded" });
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!new[] { ".csv", ".xlsx", ".xls", ".json" }.Contains(extension))
            {
                return BadRequest(new { message = "Invalid file type. Allowed: CSV, Excel, JSON" });
            }

            var fileType = extension switch
            {
                ".csv" => ImportFileType.CSV,
                ".xlsx" or ".xls" => ImportFileType.Excel,
                ".json" => ImportFileType.JSON,
                _ => ImportFileType.CSV
            };

            // Save file temporarily (in production, upload to S3)
            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var command = new StartBulkImportCommand
            {
                DealerId = dealerId,
                UserId = userId,
                FileName = file.FileName,
                FileUrl = tempPath, // In production, this would be S3 URL
                FileSizeBytes = file.Length,
                FileType = fileType
            };

            var result = await _mediator.Send(command);
            
            // TODO: Trigger background processing job
            // _backgroundJobClient.Enqueue<IBulkImportProcessor>(x => x.ProcessAsync(result.Id));

            return CreatedAtAction(nameof(GetJob), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading import file");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Download CSV template for importing vehicles
    /// </summary>
    [HttpGet("template")]
    [AllowAnonymous]
    public IActionResult DownloadTemplate()
    {
        var csvContent = @"VIN,StockNumber,Make,Model,Year,ListPrice,CostPrice,Location,InternalNotes,AcquisitionSource,AcquisitionDetails
1HGBH41JXMN109186,STK001,Honda,Accord,2021,1500000,1200000,Showroom A,Clean title,DirectPurchase,Purchased from private seller
5YJSA1E21MF123456,STK002,Tesla,Model 3,2022,2500000,2100000,Showroom B,Minor scratch on rear bumper,TradeIn,Trade-in from 2024 Camry purchase
WVWZZZ3CZWE123456,STK003,Volkswagen,Golf,2020,1200000,950000,Lot B,Needs oil change,Auction,Purchased at Manheim auction";

        var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
        return File(bytes, "text/csv", "inventory_import_template.csv");
    }

    /// <summary>
    /// Cancel a pending import job
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelImport(Guid id)
    {
        try
        {
            var query = new GetBulkImportJobQuery { JobId = id };
            var job = await _mediator.Send(query);

            if (job == null)
                return NotFound(new { message = "Import job not found" });

            if (job.Status != "Pending" && job.Status != "Processing")
                return BadRequest(new { message = "Only pending or processing jobs can be cancelled" });

            // TODO: Implement CancelBulkImportCommand
            // await _mediator.Send(new CancelBulkImportCommand { JobId = id });

            return Ok(new { message = "Import job cancellation requested" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling import job {JobId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}

/// <summary>
/// Request to start a bulk import
/// </summary>
public class StartBulkImportRequest
{
    public Guid DealerId { get; set; }
    public Guid UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FileType { get; set; } = "CSV";
}
