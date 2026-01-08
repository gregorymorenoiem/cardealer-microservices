using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using InventoryManagementService.Application.DTOs;
using InventoryManagementService.Application.Features.Inventory.Commands;
using InventoryManagementService.Application.Features.Inventory.Queries;
using InventoryManagementService.Domain.Entities;

namespace InventoryManagementService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(IMediator mediator, ILogger<InventoryController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated inventory items for a dealer
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<InventoryItemDto>>> GetInventoryItems(
        [FromQuery] Guid dealerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] InventoryStatus? status = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        try
        {
            var query = new GetInventoryItemsQuery
            {
                DealerId = dealerId,
                Page = page,
                PageSize = pageSize,
                Status = status,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory items for dealer {DealerId}", dealerId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get inventory statistics for a dealer
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<InventoryStatsDto>> GetStats([FromQuery] Guid dealerId)
    {
        try
        {
            var query = new GetInventoryStatsQuery { DealerId = dealerId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory stats for dealer {DealerId}", dealerId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get a single inventory item by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<InventoryItemDto>> GetById(Guid id)
    {
        try
        {
            // TODO: Implement GetInventoryItemByIdQuery
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory item {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Create a new inventory item
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<InventoryItemDto>> Create([FromBody] CreateInventoryItemRequest request)
    {
        try
        {
            var command = new CreateInventoryItemCommand
            {
                DealerId = request.DealerId,
                VehicleId = request.VehicleId,
                InternalNotes = request.InternalNotes,
                Location = request.Location,
                StockNumber = request.StockNumber,
                VIN = request.VIN,
                CostPrice = request.CostPrice,
                ListPrice = request.ListPrice,
                TargetPrice = request.TargetPrice,
                MinAcceptablePrice = request.MinAcceptablePrice,
                IsNegotiable = request.IsNegotiable,
                AcquiredDate = request.AcquiredDate,
                AcquisitionSource = request.AcquisitionSource,
                AcquisitionDetails = request.AcquisitionDetails
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inventory item");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update an existing inventory item
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<InventoryItemDto>> Update(Guid id, [FromBody] UpdateInventoryItemRequest request)
    {
        try
        {
            var command = new UpdateInventoryItemCommand
            {
                Id = id,
                InternalNotes = request.InternalNotes,
                Location = request.Location,
                ListPrice = request.ListPrice,
                TargetPrice = request.TargetPrice,
                MinAcceptablePrice = request.MinAcceptablePrice,
                IsNegotiable = request.IsNegotiable,
                IsFeatured = request.IsFeatured,
                Priority = request.Priority
            };

            var result = await _mediator.Send(command);
            if (result == null)
                return NotFound();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating inventory item {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Bulk update status for multiple items
    /// </summary>
    [HttpPost("bulk/status")]
    public async Task<IActionResult> BulkUpdateStatus([FromBody] BulkUpdateStatusRequest request)
    {
        try
        {
            var command = new BulkUpdateStatusCommand
            {
                ItemIds = request.ItemIds,
                Status = request.Status
            };

            await _mediator.Send(command);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating status");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete an inventory item
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            // TODO: Implement DeleteInventoryItemCommand
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting inventory item {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get featured items for a dealer
    /// </summary>
    [HttpGet("featured")]
    [AllowAnonymous]
    public async Task<ActionResult<List<InventoryItemDto>>> GetFeatured([FromQuery] Guid dealerId)
    {
        try
        {
            // TODO: Implement GetFeaturedItemsQuery
            return Ok(new List<InventoryItemDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured items for dealer {DealerId}", dealerId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get hot items (high activity) for a dealer
    /// </summary>
    [HttpGet("hot")]
    public async Task<ActionResult<List<InventoryItemDto>>> GetHotItems([FromQuery] Guid dealerId)
    {
        try
        {
            // TODO: Implement GetHotItemsQuery
            return Ok(new List<InventoryItemDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hot items for dealer {DealerId}", dealerId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get overdue items (90+ days) for a dealer
    /// </summary>
    [HttpGet("overdue")]
    public async Task<ActionResult<List<InventoryItemDto>>> GetOverdue([FromQuery] Guid dealerId)
    {
        try
        {
            // TODO: Implement GetOverdueItemsQuery
            return Ok(new List<InventoryItemDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue items for dealer {DealerId}", dealerId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
