using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KYCService.Application.Commands;
using KYCService.Application.Queries;
using KYCService.Application.DTOs;
using KYCService.Domain.Entities;

namespace KYCService.Api.Controllers;

/// <summary>
/// Controlador para gestión de perfiles KYC
/// Según Ley 155-17 de Prevención de Lavado de Activos
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class KYCProfilesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<KYCProfilesController> _logger;

    public KYCProfilesController(IMediator mediator, ILogger<KYCProfilesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtener todos los perfiles KYC con paginación y filtros
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<PaginatedResult<KYCProfileSummaryDto>>> GetProfiles(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] KYCStatus? status = null,
        [FromQuery] RiskLevel? riskLevel = null,
        [FromQuery] bool? isPEP = null)
    {
        var query = new GetKYCProfilesQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            RiskLevel = riskLevel,
            IsPEP = isPEP
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Obtener perfil KYC por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<KYCProfileDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetKYCProfileByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Obtener perfil KYC por ID de usuario
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<KYCProfileDto>> GetByUserId(Guid userId)
    {
        var result = await _mediator.Send(new GetKYCProfileByUserIdQuery(userId));
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Obtener perfil KYC por número de documento
    /// </summary>
    [HttpGet("document/{documentNumber}")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<KYCProfileDto>> GetByDocument(string documentNumber)
    {
        var result = await _mediator.Send(new GetKYCProfileByDocumentQuery(documentNumber));
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Crear nuevo perfil KYC
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<KYCProfileDto>> Create([FromBody] CreateKYCProfileCommand command)
    {
        var result = await _mediator.Send(command);
        _logger.LogInformation("KYC Profile created for user {UserId}", command.UserId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Actualizar perfil KYC
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<KYCProfileDto>> Update(Guid id, [FromBody] UpdateKYCProfileCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Aprobar perfil KYC
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<KYCProfileDto>> Approve(Guid id, [FromBody] ApproveKYCProfileCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        var result = await _mediator.Send(command);
        _logger.LogInformation("KYC Profile {Id} approved by {ApprovedBy}", id, command.ApprovedBy);
        return Ok(result);
    }

    /// <summary>
    /// Rechazar perfil KYC
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<KYCProfileDto>> Reject(Guid id, [FromBody] RejectKYCProfileCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        var result = await _mediator.Send(command);
        _logger.LogInformation("KYC Profile {Id} rejected: {Reason}", id, command.RejectionReason);
        return Ok(result);
    }

    /// <summary>
    /// Obtener perfiles pendientes de revisión
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<PaginatedResult<KYCProfileSummaryDto>>> GetPending(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetPendingKYCProfilesQuery { Page = page, PageSize = pageSize });
        return Ok(result);
    }

    /// <summary>
    /// Obtener perfiles próximos a expirar
    /// </summary>
    [HttpGet("expiring")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<PaginatedResult<KYCProfileSummaryDto>>> GetExpiring(
        [FromQuery] int daysUntilExpiry = 30,
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetExpiringKYCProfilesQuery 
        { 
            DaysUntilExpiry = daysUntilExpiry, 
            Page = page, 
            PageSize = pageSize 
        });
        return Ok(result);
    }

    /// <summary>
    /// Obtener estadísticas KYC
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<KYCStatisticsDto>> GetStatistics()
    {
        var result = await _mediator.Send(new GetKYCStatisticsQuery());
        return Ok(result);
    }
}
