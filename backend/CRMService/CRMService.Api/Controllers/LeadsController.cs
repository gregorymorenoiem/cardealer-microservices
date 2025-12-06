using CRMService.Application.DTOs;
using CRMService.Domain.Entities;
using CRMService.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeadsController : ControllerBase
{
    private readonly ILeadRepository _leadRepository;
    private readonly ILogger<LeadsController> _logger;

    public LeadsController(ILeadRepository leadRepository, ILogger<LeadsController> logger)
    {
        _leadRepository = leadRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeadDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        var leads = await _leadRepository.GetAllAsync(cancellationToken);
        return Ok(leads.Select(LeadDto.FromEntity));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LeadDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var lead = await _leadRepository.GetByIdAsync(id, cancellationToken);
        if (lead == null)
            return NotFound();

        return Ok(LeadDto.FromEntity(lead));
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<LeadDto>>> GetByStatus(string status, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<LeadStatus>(status, out var leadStatus))
            return BadRequest("Invalid status");

        var leads = await _leadRepository.GetByStatusAsync(leadStatus, cancellationToken);
        return Ok(leads.Select(LeadDto.FromEntity));
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<LeadDto>>> Search(
        [FromQuery] string query,
        CancellationToken cancellationToken = default)
    {
        var leads = await _leadRepository.SearchAsync(query, cancellationToken);
        return Ok(leads.Select(LeadDto.FromEntity));
    }

    [HttpGet("assigned/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<LeadDto>>> GetByAssignedUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var leads = await _leadRepository.GetByAssignedUserAsync(userId, cancellationToken);
        return Ok(leads.Select(LeadDto.FromEntity));
    }

    [HttpGet("recent/{count:int}")]
    public async Task<ActionResult<IEnumerable<LeadDto>>> GetRecent(
        int count,
        CancellationToken cancellationToken = default)
    {
        var leads = await _leadRepository.GetRecentAsync(count, cancellationToken);
        return Ok(leads.Select(LeadDto.FromEntity));
    }

    [HttpPost]
    public async Task<ActionResult<LeadDto>> Create(
        [FromBody] CreateLeadRequest request,
        [FromHeader(Name = "X-Dealer-Id")] Guid dealerId,
        CancellationToken cancellationToken = default)
    {
        var source = Enum.TryParse<LeadSource>(request.Source, out var s) ? s : LeadSource.Website;

        var lead = new Lead(
            dealerId,
            request.FirstName,
            request.LastName,
            request.Email,
            source,
            request.Phone,
            request.Company
        );

        await _leadRepository.AddAsync(lead, cancellationToken);

        _logger.LogInformation("Lead {LeadId} created for dealer {DealerId}", lead.Id, dealerId);

        return CreatedAtAction(nameof(GetById), new { id = lead.Id }, LeadDto.FromEntity(lead));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LeadDto>> Update(
        Guid id,
        [FromBody] UpdateLeadRequest request,
        CancellationToken cancellationToken = default)
    {
        var lead = await _leadRepository.GetByIdAsync(id, cancellationToken);
        if (lead == null)
            return NotFound();

        lead.UpdateContactInfo(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Company,
            request.JobTitle
        );

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<LeadStatus>(request.Status, out var status))
        {
            lead.UpdateStatus(status);
        }

        if (request.Score.HasValue)
        {
            lead.UpdateScore(request.Score.Value);
        }

        await _leadRepository.UpdateAsync(lead, cancellationToken);

        _logger.LogInformation("Lead {LeadId} updated", id);

        return Ok(LeadDto.FromEntity(lead));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var lead = await _leadRepository.GetByIdAsync(id, cancellationToken);
        if (lead == null)
            return NotFound();

        await _leadRepository.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Lead {LeadId} deleted", id);

        return NoContent();
    }
}
