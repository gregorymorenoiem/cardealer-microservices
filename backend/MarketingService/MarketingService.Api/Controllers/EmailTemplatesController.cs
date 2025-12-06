using Microsoft.AspNetCore.Mvc;
using MarketingService.Application.DTOs;
using MarketingService.Domain.Entities;
using MarketingService.Domain.Interfaces;

namespace MarketingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailTemplatesController : ControllerBase
{
    private readonly IEmailTemplateRepository _templateRepository;

    public EmailTemplatesController(IEmailTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmailTemplateDto>>> GetAll(CancellationToken cancellationToken)
    {
        var templates = await _templateRepository.GetAllAsync(cancellationToken);
        return Ok(templates.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmailTemplateDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetByIdAsync(id, cancellationToken);
        if (template == null)
            return NotFound();
        return Ok(MapToDto(template));
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<EmailTemplateDto>>> GetActive(CancellationToken cancellationToken)
    {
        var templates = await _templateRepository.GetActiveAsync(cancellationToken);
        return Ok(templates.Select(MapToDto));
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<EmailTemplateDto>>> GetByType(string type, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TemplateType>(type, true, out var templateType))
            return BadRequest($"Invalid template type: {type}");

        var templates = await _templateRepository.GetByTypeAsync(templateType, cancellationToken);
        return Ok(templates.Select(MapToDto));
    }

    [HttpGet("default/{type}")]
    public async Task<ActionResult<EmailTemplateDto>> GetDefault(string type, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TemplateType>(type, true, out var templateType))
            return BadRequest($"Invalid template type: {type}");

        var template = await _templateRepository.GetDefaultByTypeAsync(templateType, cancellationToken);
        if (template == null)
            return NotFound($"No default template found for type: {type}");
        return Ok(MapToDto(template));
    }

    [HttpPost]
    public async Task<ActionResult<EmailTemplateDto>> Create([FromBody] CreateEmailTemplateRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TemplateType>(request.Type, true, out var type))
            return BadRequest($"Invalid template type: {request.Type}");

        var dealerId = GetCurrentDealerId();
        var userId = GetCurrentUserId();

        var template = new EmailTemplate(dealerId, request.Name, type, request.Subject, request.Body, userId, request.Description);

        await _templateRepository.AddAsync(template, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = template.Id }, MapToDto(template));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EmailTemplateDto>> Update(Guid id, [FromBody] UpdateEmailTemplateRequest request, CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetByIdAsync(id, cancellationToken);
        if (template == null)
            return NotFound();

        template.Update(request.Name, request.Subject, request.Body, request.HtmlBody);
        await _templateRepository.UpdateAsync(template, cancellationToken);
        return Ok(MapToDto(template));
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<EmailTemplateDto>> Activate(Guid id, CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetByIdAsync(id, cancellationToken);
        if (template == null)
            return NotFound();

        template.Activate();
        await _templateRepository.UpdateAsync(template, cancellationToken);
        return Ok(MapToDto(template));
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<EmailTemplateDto>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetByIdAsync(id, cancellationToken);
        if (template == null)
            return NotFound();

        template.Deactivate();
        await _templateRepository.UpdateAsync(template, cancellationToken);
        return Ok(MapToDto(template));
    }

    [HttpPost("{id:guid}/set-default")]
    public async Task<ActionResult<EmailTemplateDto>> SetDefault(Guid id, CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetByIdAsync(id, cancellationToken);
        if (template == null)
            return NotFound();

        template.SetAsDefault();
        await _templateRepository.UpdateAsync(template, cancellationToken);
        return Ok(MapToDto(template));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!await _templateRepository.ExistsAsync(id, cancellationToken))
            return NotFound();

        await _templateRepository.DeleteAsync(id, cancellationToken);
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

    private static EmailTemplateDto MapToDto(EmailTemplate t) => new(
        t.Id,
        t.Name,
        t.Description,
        t.Type.ToString(),
        t.Subject,
        t.Body,
        t.HtmlBody,
        t.PreheaderText,
        t.FromName,
        t.FromEmail,
        t.ReplyToEmail,
        t.IsActive,
        t.IsDefault,
        t.Category,
        t.Tags,
        t.CreatedAt
    );
}
