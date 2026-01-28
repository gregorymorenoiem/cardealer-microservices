using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChatbotService.Application.DTOs;
using ChatbotService.Domain.Interfaces;
using ChatbotService.Domain.Enums;

namespace ChatbotService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Dealer")]
public class LeadsController : ControllerBase
{
    private readonly ILogger<LeadsController> _logger;
    private readonly IChatLeadRepository _leadRepository;
    private readonly IChatSessionRepository _sessionRepository;

    public LeadsController(
        ILogger<LeadsController> logger,
        IChatLeadRepository leadRepository,
        IChatSessionRepository sessionRepository)
    {
        _logger = logger;
        _leadRepository = leadRepository;
        _sessionRepository = sessionRepository;
    }

    /// <summary>
    /// Get lead by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ChatLeadDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetLead(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var lead = await _leadRepository.GetByIdAsync(id, cancellationToken);
            if (lead == null)
            {
                return NotFound(new { error = "Lead not found" });
            }

            var dto = MapToDto(lead);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lead {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get leads by status
    /// </summary>
    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<ChatLeadDto>), 200)]
    public async Task<IActionResult> GetLeadsByStatus(
        LeadStatus status, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var leads = await _leadRepository.GetByStatusAsync(status, page, pageSize, cancellationToken);
            var dtos = leads.Select(MapToDto);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leads by status {Status}", status);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get unassigned leads
    /// </summary>
    [HttpGet("unassigned")]
    [ProducesResponseType(typeof(IEnumerable<ChatLeadDto>), 200)]
    public async Task<IActionResult> GetUnassignedLeads(CancellationToken cancellationToken)
    {
        try
        {
            var leads = await _leadRepository.GetUnassignedLeadsAsync(cancellationToken);
            var dtos = leads.Select(MapToDto);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unassigned leads");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get lead by session
    /// </summary>
    [HttpGet("session/{sessionId:guid}")]
    [ProducesResponseType(typeof(ChatLeadDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetLeadBySession(Guid sessionId, CancellationToken cancellationToken)
    {
        try
        {
            var lead = await _leadRepository.GetBySessionIdAsync(sessionId, cancellationToken);
            if (lead == null)
            {
                return NotFound(new { error = "No lead found for session" });
            }

            var dto = MapToDto(lead);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lead for session {SessionId}", sessionId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Update lead status
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateLeadStatus(Guid id, [FromBody] UpdateLeadStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var lead = await _leadRepository.GetByIdAsync(id, cancellationToken);
            if (lead == null)
            {
                return NotFound(new { error = "Lead not found" });
            }

            lead.Status = request.Status;
            lead.UpdatedAt = DateTime.UtcNow;

            await _leadRepository.UpdateAsync(lead, cancellationToken);

            return Ok(new { message = "Lead status updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lead status {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Assign lead to user
    /// </summary>
    [HttpPut("{id:guid}/assign")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AssignLead(Guid id, [FromBody] AssignLeadRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var lead = await _leadRepository.GetByIdAsync(id, cancellationToken);
            if (lead == null)
            {
                return NotFound(new { error = "Lead not found" });
            }

            lead.AssignedToUserId = request.UserId;
            lead.Status = LeadStatus.InProgress;
            lead.UpdatedAt = DateTime.UtcNow;

            await _leadRepository.UpdateAsync(lead, cancellationToken);

            return Ok(new { message = "Lead assigned" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning lead {Id}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get today's lead count
    /// </summary>
    [HttpGet("count/today/{configurationId:guid}")]
    [ProducesResponseType(typeof(int), 200)]
    public async Task<IActionResult> GetTodayLeadCount(Guid configurationId, CancellationToken cancellationToken)
    {
        try
        {
            var count = await _leadRepository.GetTodayLeadCountAsync(configurationId, cancellationToken);
            return Ok(new { count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today's lead count");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    private static ChatLeadDto MapToDto(Domain.Entities.ChatLead lead)
    {
        return new ChatLeadDto
        {
            Id = lead.Id,
            SessionId = lead.SessionId,
            FullName = lead.FullName,
            Email = lead.Email,
            Phone = lead.Phone,
            InterestedVehicleId = lead.InterestedVehicleId,
            InterestedVehicleName = lead.InterestedVehicleName,
            BudgetMin = lead.BudgetMin,
            BudgetMax = lead.BudgetMax,
            InterestedInFinancing = lead.InterestedInFinancing,
            InterestedInTradeIn = lead.InterestedInTradeIn,
            Status = lead.Status,
            Temperature = lead.Temperature,
            QualificationScore = lead.QualificationScore,
            AssignedToUserId = lead.AssignedToUserId,
            CreatedAt = lead.CreatedAt,
            UpdatedAt = lead.UpdatedAt
        };
    }
}

public record UpdateLeadStatusRequest(LeadStatus Status);
public record AssignLeadRequest(Guid UserId);
