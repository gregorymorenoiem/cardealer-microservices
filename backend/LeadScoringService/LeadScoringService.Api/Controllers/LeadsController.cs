using LeadScoringService.Application.DTOs;
using LeadScoringService.Application.Features.Leads.Commands;
using LeadScoringService.Application.Features.Leads.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeadScoringService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeadsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LeadsController> _logger;

    public LeadsController(IMediator mediator, ILogger<LeadsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtener leads de un dealer con paginación y filtros
    /// </summary>
    [HttpGet("dealer/{dealerId}")]
    [Authorize]
    [ProducesResponseType(typeof(PagedLeadsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeadsByDealer(
        Guid dealerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? temperature = null,
        [FromQuery] string? status = null,
        [FromQuery] string? searchTerm = null)
    {
        try
        {
            var query = new GetLeadsByDealerQuery(dealerId, page, pageSize, temperature, status, searchTerm);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leads for dealer {DealerId}", dealerId);
            return StatusCode(500, new { message = "Error retrieving leads" });
        }
    }

    /// <summary>
    /// Obtener un lead por ID con todo su historial
    /// </summary>
    [HttpGet("{leadId}")]
    [Authorize]
    [ProducesResponseType(typeof(LeadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLeadById(Guid leadId)
    {
        try
        {
            var query = new GetLeadByIdQuery(leadId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = $"Lead {leadId} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lead {LeadId}", leadId);
            return StatusCode(500, new { message = "Error retrieving lead" });
        }
    }

    /// <summary>
    /// Obtener estadísticas de leads de un dealer
    /// </summary>
    [HttpGet("dealer/{dealerId}/statistics")]
    [Authorize]
    [ProducesResponseType(typeof(LeadStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeadStatistics(Guid dealerId)
    {
        try
        {
            var query = new GetLeadStatisticsQuery(dealerId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics for dealer {DealerId}", dealerId);
            return StatusCode(500, new { message = "Error retrieving statistics" });
        }
    }

    /// <summary>
    /// Crear o actualizar un lead
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(LeadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrUpdateLead([FromBody] CreateLeadDto dto)
    {
        try
        {
            var command = new CreateOrUpdateLeadCommand(dto);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating lead for user {UserId} and vehicle {VehicleId}", dto.UserId, dto.VehicleId);
            return StatusCode(500, new { message = "Error creating/updating lead" });
        }
    }

    /// <summary>
    /// Registrar una acción de un lead (view, contact, etc.)
    /// </summary>
    [HttpPost("actions")]
    [Authorize]
    [ProducesResponseType(typeof(LeadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordLeadAction([FromBody] RecordLeadActionDto dto)
    {
        try
        {
            var command = new RecordLeadActionCommand(dto);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording action for lead {LeadId}", dto.LeadId);
            return StatusCode(500, new { message = "Error recording action" });
        }
    }

    /// <summary>
    /// Actualizar el estado de un lead
    /// </summary>
    [HttpPut("{leadId}/status")]
    [Authorize]
    [ProducesResponseType(typeof(LeadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLeadStatus(Guid leadId, [FromBody] UpdateLeadStatusDto dto)
    {
        try
        {
            var command = new UpdateLeadStatusCommand(leadId, dto);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = $"Lead {leadId} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for lead {LeadId}", leadId);
            return StatusCode(500, new { message = "Error updating lead status" });
        }
    }
}
