using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using LegalDocumentService.Application.DTOs;
using LegalDocumentService.Application.Features.Documents.Commands;
using LegalDocumentService.Application.Features.Documents.Queries;
using LegalDocumentService.Domain.Enums;

namespace LegalDocumentService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LegalDocumentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LegalDocumentsController> _logger;

    public LegalDocumentsController(IMediator mediator, ILogger<LegalDocumentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // ===== DOCUMENTS =====

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LegalDocumentDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLegalDocumentByIdQuery(id), ct);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<LegalDocumentDto>> GetBySlug(string slug, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLegalDocumentBySlugQuery(slug), ct);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<LegalDocumentSummaryDto>>> GetActive(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetActiveDocumentsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<List<LegalDocumentSummaryDto>>> GetByType(LegalDocumentType type, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDocumentsByTypeQuery(type), ct);
        return Ok(result);
    }

    [HttpGet("requiring-acceptance")]
    public async Task<ActionResult<List<LegalDocumentSummaryDto>>> GetRequiringAcceptance(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDocumentsRequiringAcceptanceQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<LegalDocumentDto>> Create([FromBody] CreateLegalDocumentDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateLegalDocumentCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<LegalDocumentDto>> Update(Guid id, [FromBody] UpdateLegalDocumentDto dto, CancellationToken ct)
    {
        if (id != dto.Id)
            return BadRequest("ID mismatch");

        var result = await _mediator.Send(new UpdateLegalDocumentCommand(dto), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<LegalDocumentDto>> Publish(Guid id, [FromBody] PublishDocumentDto dto, CancellationToken ct)
    {
        if (id != dto.DocumentId)
            return BadRequest("ID mismatch");

        var result = await _mediator.Send(new PublishDocumentCommand(dto), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/archive")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult> Archive(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ArchiveDocumentCommand(id), ct);
        return NoContent();
    }

    // ===== ACCEPTANCES =====

    [HttpGet("acceptances/user/{userId}")]
    [Authorize]
    public async Task<ActionResult<List<UserAcceptanceDto>>> GetUserAcceptances(string userId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserAcceptancesQuery(userId), ct);
        return Ok(result);
    }

    [HttpGet("acceptances/check")]
    [Authorize]
    public async Task<ActionResult<UserAcceptanceStatusDto>> CheckAcceptance(
        [FromQuery] string userId,
        [FromQuery] Guid documentId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new CheckUserAcceptanceQuery(userId, documentId), ct);
        return Ok(result);
    }

    [HttpGet("acceptances/pending/{userId}")]
    [Authorize]
    public async Task<ActionResult<List<LegalDocumentSummaryDto>>> GetPendingAcceptances(string userId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPendingAcceptancesQuery(userId), ct);
        return Ok(result);
    }

    [HttpPost("acceptances")]
    [Authorize]
    public async Task<ActionResult<UserAcceptanceDto>> RecordAcceptance([FromBody] CreateAcceptanceDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new RecordUserAcceptanceCommand(dto), ct);
        return CreatedAtAction(nameof(GetUserAcceptances), new { userId = result.UserId }, result);
    }

    [HttpPost("acceptances/{id:guid}/revoke")]
    [Authorize]
    public async Task<ActionResult> RevokeAcceptance(Guid id, [FromBody] RevokeAcceptanceDto dto, CancellationToken ct)
    {
        if (id != dto.AcceptanceId)
            return BadRequest("ID mismatch");

        await _mediator.Send(new RevokeAcceptanceCommand(dto), ct);
        return NoContent();
    }

    // ===== TEMPLATES =====

    [HttpGet("templates")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<List<DocumentTemplateDto>>> GetTemplates(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetActiveTemplatesQuery(), ct);
        return Ok(result);
    }

    [HttpPost("templates")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<DocumentTemplateDto>> CreateTemplate([FromBody] CreateTemplateDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateTemplateCommand(dto), ct);
        return CreatedAtAction(nameof(GetTemplates), new { id = result.Id }, result);
    }

    // ===== COMPLIANCE REQUIREMENTS =====

    [HttpGet("compliance-requirements")]
    public async Task<ActionResult<List<ComplianceRequirementDto>>> GetComplianceRequirements(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetActiveComplianceRequirementsQuery(), ct);
        return Ok(result);
    }

    [HttpPost("compliance-requirements")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<ComplianceRequirementDto>> CreateComplianceRequirement(
        [FromBody] CreateComplianceRequirementDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateComplianceRequirementCommand(dto), ct);
        return CreatedAtAction(nameof(GetComplianceRequirements), new { id = result.Id }, result);
    }

    // ===== STATISTICS =====

    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<LegalStatisticsDto>> GetStatistics(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLegalStatisticsQuery(), ct);
        return Ok(result);
    }
}
