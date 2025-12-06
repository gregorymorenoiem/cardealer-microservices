using Microsoft.AspNetCore.Mvc;
using MarketingService.Application.DTOs;
using MarketingService.Domain.Entities;
using MarketingService.Domain.Interfaces;

namespace MarketingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AudiencesController : ControllerBase
{
    private readonly IAudienceRepository _audienceRepository;

    public AudiencesController(IAudienceRepository audienceRepository)
    {
        _audienceRepository = audienceRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AudienceDto>>> GetAll(CancellationToken cancellationToken)
    {
        var audiences = await _audienceRepository.GetAllAsync(cancellationToken);
        return Ok(audiences.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AudienceDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var audience = await _audienceRepository.GetByIdAsync(id, cancellationToken);
        if (audience == null)
            return NotFound();
        return Ok(MapToDto(audience));
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<AudienceDto>>> GetActive(CancellationToken cancellationToken)
    {
        var audiences = await _audienceRepository.GetActiveAsync(cancellationToken);
        return Ok(audiences.Select(MapToDto));
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<AudienceDto>>> GetByType(string type, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AudienceType>(type, true, out var audienceType))
            return BadRequest($"Invalid audience type: {type}");

        var audiences = await _audienceRepository.GetByTypeAsync(audienceType, cancellationToken);
        return Ok(audiences.Select(MapToDto));
    }

    [HttpPost]
    public async Task<ActionResult<AudienceDto>> Create([FromBody] CreateAudienceRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AudienceType>(request.Type, true, out var type))
            return BadRequest($"Invalid audience type: {request.Type}");

        var dealerId = GetCurrentDealerId();
        var userId = GetCurrentUserId();

        var audience = new Audience(dealerId, request.Name, type, userId, request.Description);

        if (!string.IsNullOrEmpty(request.FilterCriteria))
            audience.SetFilterCriteria(request.FilterCriteria);

        await _audienceRepository.AddAsync(audience, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = audience.Id }, MapToDto(audience));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AudienceDto>> Update(Guid id, [FromBody] UpdateAudienceRequest request, CancellationToken cancellationToken)
    {
        var audience = await _audienceRepository.GetByIdAsync(id, cancellationToken);
        if (audience == null)
            return NotFound();

        audience.Update(request.Name, request.Description);

        if (!string.IsNullOrEmpty(request.FilterCriteria))
            audience.SetFilterCriteria(request.FilterCriteria);

        await _audienceRepository.UpdateAsync(audience, cancellationToken);
        return Ok(MapToDto(audience));
    }

    [HttpPut("{id:guid}/member-count")]
    public async Task<ActionResult<AudienceDto>> UpdateMemberCount(Guid id, [FromQuery] int count, CancellationToken cancellationToken)
    {
        var audience = await _audienceRepository.GetByIdAsync(id, cancellationToken);
        if (audience == null)
            return NotFound();

        audience.UpdateMemberCount(count);
        await _audienceRepository.UpdateAsync(audience, cancellationToken);
        return Ok(MapToDto(audience));
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<AudienceDto>> Activate(Guid id, CancellationToken cancellationToken)
    {
        var audience = await _audienceRepository.GetByIdAsync(id, cancellationToken);
        if (audience == null)
            return NotFound();

        audience.Activate();
        await _audienceRepository.UpdateAsync(audience, cancellationToken);
        return Ok(MapToDto(audience));
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<AudienceDto>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var audience = await _audienceRepository.GetByIdAsync(id, cancellationToken);
        if (audience == null)
            return NotFound();

        audience.Deactivate();
        await _audienceRepository.UpdateAsync(audience, cancellationToken);
        return Ok(MapToDto(audience));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!await _audienceRepository.ExistsAsync(id, cancellationToken))
            return NotFound();

        await _audienceRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private Guid GetCurrentDealerId()
    {
        var dealerIdClaim = User.FindFirst("dealer_id")?.Value;
        return dealerIdClaim != null ? Guid.Parse(dealerIdClaim) : Guid.Empty;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("user_id")?.Value;
        return userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;
    }

    private static AudienceDto MapToDto(Audience a) => new(
        a.Id,
        a.Name,
        a.Description,
        a.Type.ToString(),
        a.FilterCriteria,
        a.MemberCount,
        a.IsActive,
        a.LastSyncedAt,
        a.CreatedAt
    );
}
