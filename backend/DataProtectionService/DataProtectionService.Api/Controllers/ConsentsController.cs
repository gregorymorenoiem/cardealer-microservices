using Microsoft.AspNetCore.Mvc;
using MediatR;
using DataProtectionService.Application.DTOs;
using DataProtectionService.Application.Commands;
using DataProtectionService.Application.Queries;

namespace DataProtectionService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ConsentsController> _logger;

    public ConsentsController(IMediator mediator, ILogger<ConsentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtener todos los consentimientos de un usuario
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(List<UserConsentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserConsentDto>>> GetUserConsents(
        Guid userId,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetUserConsentsQuery(userId, activeOnly), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtener un consentimiento específico
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserConsentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserConsentDto>> GetConsent(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetConsentByIdQuery(id), cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Verificar si un usuario tiene un consentimiento activo
    /// </summary>
    [HttpGet("check")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> CheckConsent(
        [FromQuery] Guid userId,
        [FromQuery] string type,
        CancellationToken cancellationToken = default)
    {
        var hasConsent = await _mediator.Send(new CheckConsentQuery(userId, type), cancellationToken);
        return Ok(hasConsent);
    }

    /// <summary>
    /// Registrar un nuevo consentimiento (Ley 172-13)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserConsentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserConsentDto>> CreateConsent(
        [FromBody] CreateConsentRequest request,
        CancellationToken cancellationToken = default)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var command = new CreateConsentCommand(
            request.UserId,
            request.Type,
            request.Version,
            request.DocumentHash,
            request.Granted,
            ipAddress,
            userAgent,
            request.CollectionMethod
        );

        var result = await _mediator.Send(command, cancellationToken);
        _logger.LogInformation("Consent created for user {UserId}, type {Type}", request.UserId, request.Type);
        
        return CreatedAtAction(nameof(GetConsent), new { id = result.Id }, result);
    }

    /// <summary>
    /// Revocar un consentimiento (derecho Ley 172-13)
    /// </summary>
    [HttpPost("{id}/revoke")]
    [ProducesResponseType(typeof(UserConsentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserConsentDto>> RevokeConsent(
        Guid id,
        [FromBody] RevokeConsentRequest request,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken = default)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var command = new RevokeConsentCommand(id, userId, request.Reason, ipAddress);
        
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("Consent {ConsentId} revoked by user {UserId}", id, userId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Revocar múltiples consentimientos a la vez
    /// </summary>
    [HttpPost("bulk-revoke")]
    [ProducesResponseType(typeof(List<UserConsentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserConsentDto>>> BulkRevokeConsents(
        [FromBody] BulkRevokeRequest request,
        CancellationToken cancellationToken = default)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var command = new BulkRevokeConsentsCommand(
            request.UserId,
            request.ConsentTypes,
            request.Reason,
            ipAddress
        );

        var result = await _mediator.Send(command, cancellationToken);
        _logger.LogInformation("Bulk revoke: {Count} consents revoked for user {UserId}", result.Count, request.UserId);
        
        return Ok(result);
    }

    /// <summary>
    /// Aceptar política de privacidad actual
    /// </summary>
    [HttpPost("accept-policy")]
    [ProducesResponseType(typeof(UserConsentDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<UserConsentDto>> AcceptPrivacyPolicy(
        [FromBody] AcceptPolicyRequest request,
        CancellationToken cancellationToken = default)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var command = new AcceptPrivacyPolicyCommand(
            request.UserId,
            request.PolicyId,
            ipAddress ?? "unknown",
            userAgent
        );

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetConsent), new { id = result.Id }, result);
    }
}

public record BulkRevokeRequest(Guid UserId, List<string> ConsentTypes, string Reason);
public record AcceptPolicyRequest(Guid UserId, Guid PolicyId);
