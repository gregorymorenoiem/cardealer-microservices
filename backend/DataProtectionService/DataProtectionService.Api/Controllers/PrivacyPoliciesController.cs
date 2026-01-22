using Microsoft.AspNetCore.Mvc;
using MediatR;
using DataProtectionService.Application.DTOs;
using DataProtectionService.Application.Commands;
using DataProtectionService.Application.Queries;

namespace DataProtectionService.Api.Controllers;

/// <summary>
/// Controller para políticas de privacidad y términos de servicio
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PrivacyPoliciesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PrivacyPoliciesController> _logger;

    public PrivacyPoliciesController(IMediator mediator, ILogger<PrivacyPoliciesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtener la política de privacidad actual/vigente
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(typeof(PrivacyPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrivacyPolicyDto>> GetCurrentPolicy(
        [FromQuery] string language = "es",
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetCurrentPrivacyPolicyQuery(language), cancellationToken);
        if (result == null) return NotFound("No active privacy policy found");
        return Ok(result);
    }

    /// <summary>
    /// Obtener todas las políticas de privacidad
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PrivacyPolicyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PrivacyPolicyDto>>> GetAll(
        [FromQuery] bool activeOnly = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAllPrivacyPoliciesQuery(activeOnly), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Crear una nueva versión de política de privacidad (Admin)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PrivacyPolicyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PrivacyPolicyDto>> Create(
        [FromBody] CreatePrivacyPolicyRequest request,
        [FromHeader(Name = "X-User-Id")] Guid createdById,
        CancellationToken cancellationToken = default)
    {
        var command = new CreatePrivacyPolicyCommand(
            request.Version,
            request.DocumentType,
            request.Content,
            request.ChangesSummary,
            request.Language,
            request.EffectiveDate,
            request.RequiresReAcceptance,
            createdById
        );

        var result = await _mediator.Send(command, cancellationToken);
        
        _logger.LogInformation(
            "Privacy Policy created: v{Version} - Type: {Type} - Effective: {EffectiveDate}", 
            request.Version, request.DocumentType, request.EffectiveDate);
        
        return CreatedAtAction(nameof(GetCurrentPolicy), result);
    }

    /// <summary>
    /// Actualizar una política de privacidad (Admin)
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PrivacyPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrivacyPolicyDto>> Update(
        Guid id,
        [FromBody] UpdatePolicyRequest request,
        [FromHeader(Name = "X-User-Id")] Guid updatedById,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdatePrivacyPolicyCommand(
            id,
            request.Content,
            request.ChangesSummary,
            request.EffectiveDate,
            request.IsActive,
            updatedById
        );

        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}

public record UpdatePolicyRequest(
    string? Content,
    string? ChangesSummary,
    DateTime? EffectiveDate,
    bool? IsActive
);
