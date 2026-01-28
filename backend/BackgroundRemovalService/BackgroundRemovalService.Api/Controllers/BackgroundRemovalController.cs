using System.Security.Claims;
using BackgroundRemovalService.Application.DTOs;
using BackgroundRemovalService.Application.Features.Commands;
using BackgroundRemovalService.Application.Features.Queries;
using BackgroundRemovalService.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundRemovalService.Api.Controllers;

/// <summary>
/// API para remoción de fondos de imágenes.
/// Por defecto usa ClipDrop (Stability AI): https://clipdrop.co/apis/docs/remove-background
/// También soporta: Remove.bg, Photoroom, Slazzer.
/// El proveedor se configura en appsettings.json (BackgroundRemoval:DefaultProvider).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BackgroundRemovalController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateRemovalJobRequest> _validator;
    private readonly ILogger<BackgroundRemovalController> _logger;

    public BackgroundRemovalController(
        IMediator mediator,
        IValidator<CreateRemovalJobRequest> validator,
        ILogger<BackgroundRemovalController> logger)
    {
        _mediator = mediator;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Remueve el fondo de una imagen
    /// </summary>
    /// <param name="request">Imagen a procesar (URL o Base64) y opciones</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado del procesamiento con URL de la imagen sin fondo</returns>
    /// <response code="200">Procesamiento exitoso</response>
    /// <response code="400">Request inválido</response>
    /// <response code="500">Error interno</response>
    [HttpPost("remove")]
    [ProducesResponseType(typeof(RemovalJobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RemovalJobResponse>> RemoveBackground(
        [FromBody] CreateRemovalJobRequest request,
        CancellationToken cancellationToken)
    {
        // Validar request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ValidationProblemDetails(
                validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray())));
        }

        var userId = GetUserIdFromClaims();
        var tenantId = GetTenantIdFromClaims();
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var command = new CreateRemovalJobCommand
        {
            Request = request,
            UserId = userId,
            TenantId = tenantId,
            ClientIpAddress = clientIp,
            UserAgent = userAgent
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.Status == ProcessingStatus.Failed)
        {
            return StatusCode(500, new ProblemDetails
            {
                Title = "Background removal failed",
                Detail = result.ErrorMessage,
                Status = 500,
                Extensions = { ["errorCode"] = result.ErrorCode }
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Obtiene el estado de un job por ID
    /// </summary>
    [HttpGet("jobs/{jobId:guid}")]
    [ProducesResponseType(typeof(RemovalJobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RemovalJobResponse>> GetJob(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var query = new GetRemovalJobByIdQuery { JobId = jobId };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Obtiene un job por CorrelationId
    /// </summary>
    [HttpGet("jobs/correlation/{correlationId}")]
    [ProducesResponseType(typeof(RemovalJobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RemovalJobResponse>> GetJobByCorrelationId(
        string correlationId,
        CancellationToken cancellationToken)
    {
        var query = new GetRemovalJobByCorrelationIdQuery { CorrelationId = correlationId };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Lista los jobs del usuario autenticado
    /// </summary>
    [HttpGet("jobs")]
    [Authorize]
    [ProducesResponseType(typeof(RemovalJobListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RemovalJobListResponse>> GetUserJobs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] ProcessingStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserIdFromClaims();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var query = new GetUserRemovalJobsQuery
        {
            UserId = userId.Value,
            Page = page,
            PageSize = Math.Clamp(pageSize, 1, 100),
            StatusFilter = status
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Reintenta un job fallido
    /// </summary>
    [HttpPost("jobs/{jobId:guid}/retry")]
    [ProducesResponseType(typeof(RemovalJobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RemovalJobResponse>> RetryJob(
        Guid jobId,
        [FromQuery] BackgroundRemovalProvider? alternateProvider = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new RetryRemovalJobCommand
            {
                JobId = jobId,
                AlternateProvider = alternateProvider
            };

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new ProblemDetails { Title = "Job not found", Detail = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Cannot retry", Detail = ex.Message });
        }
    }

    /// <summary>
    /// Cancela un job pendiente
    /// </summary>
    [HttpPost("jobs/{jobId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelJob(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        var command = new CancelRemovalJobCommand
        {
            JobId = jobId,
            UserId = userId
        };

        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Cannot cancel job",
                Detail = "Job not found or cannot be cancelled"
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Obtiene información de los proveedores disponibles
    /// </summary>
    [HttpGet("providers")]
    [ProducesResponseType(typeof(IEnumerable<ProviderInfoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProviderInfoResponse>>> GetProviders(
        [FromQuery] bool onlyEnabled = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProvidersInfoQuery { OnlyEnabled = onlyEnabled };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Health check de los proveedores
    /// </summary>
    [HttpGet("providers/health")]
    [ProducesResponseType(typeof(IEnumerable<ProviderHealthResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProviderHealthResponse>>> GetProvidersHealth(
        CancellationToken cancellationToken)
    {
        var query = new GetProvidersHealthQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene estadísticas de uso
    /// </summary>
    [HttpGet("usage")]
    [Authorize]
    [ProducesResponseType(typeof(UsageStatisticsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UsageStatisticsResponse>> GetUsageStatistics(
        [FromQuery] int? billingPeriod = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserIdFromClaims();
        var query = new GetUsageStatisticsQuery
        {
            UserId = userId,
            BillingPeriod = billingPeriod
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    // === Helper Methods ===

    private Guid? GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }

    private string? GetTenantIdFromClaims()
    {
        return User.FindFirst("tenant_id")?.Value
            ?? User.FindFirst("tenantId")?.Value;
    }
}
