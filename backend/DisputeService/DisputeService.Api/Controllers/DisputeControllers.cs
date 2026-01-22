// DisputeService - API Controllers

namespace DisputeService.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using DisputeService.Application.Commands;
using DisputeService.Application.Queries;
using DisputeService.Application.DTOs;
using DisputeService.Domain.Entities;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DisputesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DisputesController(IMediator mediator) => _mediator = mediator;

    #region Dispute Endpoints

    /// <summary>
    /// Registrar nueva disputa (Pro-Consumidor Art. 102)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> FileDispute([FromBody] FileDisputeCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Obtener disputa por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DisputeDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDisputeByIdQuery(id), ct);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Obtener disputa por número de caso
    /// </summary>
    [HttpGet("case/{caseNumber}")]
    public async Task<ActionResult<DisputeDto>> GetByCaseNumber(string caseNumber, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDisputeByCaseNumberQuery(caseNumber), ct);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Listar disputas con paginación
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<DisputePagedResultDto>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DisputeStatus? status = null,
        [FromQuery] DisputeType? type = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetDisputesPagedQuery(page, pageSize, status, type), ct);
        return Ok(result);
    }

    /// <summary>
    /// Disputas del reclamante
    /// </summary>
    [HttpGet("complainant/{complainantId:guid}")]
    public async Task<ActionResult<List<DisputeSummaryDto>>> GetByComplainant(Guid complainantId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDisputesByComplainantQuery(complainantId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Disputas del demandado
    /// </summary>
    [HttpGet("respondent/{respondentId:guid}")]
    public async Task<ActionResult<List<DisputeSummaryDto>>> GetByRespondent(Guid respondentId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDisputesByRespondentQuery(respondentId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Disputas asignadas a un mediador
    /// </summary>
    [HttpGet("mediator/{mediatorId}")]
    [Authorize(Roles = "Admin,Mediator")]
    public async Task<ActionResult<List<DisputeSummaryDto>>> GetByMediator(string mediatorId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDisputesByMediatorQuery(mediatorId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Disputas pendientes
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Mediator,Compliance")]
    public async Task<ActionResult<List<DisputeSummaryDto>>> GetPending(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPendingDisputesQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Disputas vencidas (fuera de SLA)
    /// </summary>
    [HttpGet("overdue")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<List<DisputeSummaryDto>>> GetOverdue(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetOverdueDisputesQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Disputas escaladas
    /// </summary>
    [HttpGet("escalated")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<List<DisputeSummaryDto>>> GetEscalated(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetEscalatedDisputesQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Disputas referidas a Pro-Consumidor
    /// </summary>
    [HttpGet("pro-consumidor")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<List<DisputeSummaryDto>>> GetProConsumidorReferrals(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProConsumidorReferralsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Reconocer disputa
    /// </summary>
    [HttpPost("{id:guid}/acknowledge")]
    public async Task<ActionResult> Acknowledge(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new AcknowledgeDisputeCommand(id), ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Asignar mediador
    /// </summary>
    [HttpPost("{id:guid}/assign-mediator")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> AssignMediator(Guid id, [FromBody] AssignMediatorRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AssignMediatorCommand(id, request.MediatorId, request.MediatorName), ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Escalar disputa
    /// </summary>
    [HttpPost("{id:guid}/escalate")]
    [Authorize(Roles = "Admin,Mediator")]
    public async Task<ActionResult> Escalate(Guid id, [FromBody] EscalateRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new EscalateDisputeCommand(id, request.Reason), ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Resolver disputa
    /// </summary>
    [HttpPost("{id:guid}/resolve")]
    [Authorize(Roles = "Admin,Mediator")]
    public async Task<ActionResult> Resolve(Guid id, [FromBody] ResolveRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ResolveDisputeCommand(id, request.Resolution, request.ResolutionSummary, request.ResolvedBy), ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Referir a Pro-Consumidor
    /// </summary>
    [HttpPost("{id:guid}/refer-pro-consumidor")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult> ReferToProConsumidor(Guid id, [FromBody] ReferProConsumidorRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ReferToProConsumidorCommand(id, request.Reason), ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Cerrar disputa
    /// </summary>
    [HttpPost("{id:guid}/close")]
    [Authorize(Roles = "Admin,Mediator")]
    public async Task<ActionResult> Close(Guid id, [FromBody] CloseRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CloseDisputeCommand(id, request.Reason), ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Estadísticas de disputas
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<DisputeStatisticsDto>> GetStatistics(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = to ?? DateTime.UtcNow;
        var result = await _mediator.Send(new GetDisputeStatisticsQuery(fromDate, toDate), ct);
        return Ok(result);
    }

    #endregion
}

[ApiController]
[Route("api/disputes/{disputeId:guid}/[controller]")]
[Authorize]
public class EvidenceController : ControllerBase
{
    private readonly IMediator _mediator;

    public EvidenceController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Presentar evidencia
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> SubmitEvidence(Guid disputeId, [FromBody] SubmitEvidenceRequest request, CancellationToken ct)
    {
        var command = new SubmitEvidenceCommand(
            disputeId, request.Name, request.Description, request.EvidenceType,
            request.FileName, request.ContentType, request.FileSize, request.StoragePath,
            request.SubmittedById, request.SubmittedByName, request.SubmitterRole);

        var id = await _mediator.Send(command, ct);
        return Created($"api/disputes/{disputeId}/evidence/{id}", id);
    }

    /// <summary>
    /// Listar evidencias de una disputa
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<DisputeEvidenceDto>>> GetByDispute(Guid disputeId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetEvidenceByDisputeQuery(disputeId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener evidencia por ID
    /// </summary>
    [HttpGet("{evidenceId:guid}")]
    public async Task<ActionResult<DisputeEvidenceDto>> GetById(Guid disputeId, Guid evidenceId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetEvidenceByIdQuery(evidenceId), ct);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Revisar evidencia
    /// </summary>
    [HttpPost("{evidenceId:guid}/review")]
    [Authorize(Roles = "Admin,Mediator")]
    public async Task<ActionResult> ReviewEvidence(Guid disputeId, Guid evidenceId, [FromBody] ReviewEvidenceRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ReviewEvidenceCommand(evidenceId, request.NewStatus, request.ReviewedBy, request.ReviewNotes), ct);
        return result ? NoContent() : NotFound();
    }
}

[ApiController]
[Route("api/disputes/{disputeId:guid}/[controller]")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CommentsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Agregar comentario
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> AddComment(Guid disputeId, [FromBody] AddCommentRequest request, CancellationToken ct)
    {
        var command = new AddCommentCommand(
            disputeId, request.AuthorId, request.AuthorName, request.AuthorRole,
            request.Content, request.IsInternal, request.IsOfficial);

        var id = await _mediator.Send(command, ct);
        return Created($"api/disputes/{disputeId}/comments/{id}", id);
    }

    /// <summary>
    /// Listar comentarios de una disputa
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<DisputeCommentDto>>> GetByDispute(
        Guid disputeId,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetCommentsByDisputeQuery(disputeId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Editar comentario
    /// </summary>
    [HttpPut("{commentId:guid}")]
    public async Task<ActionResult> EditComment(Guid disputeId, Guid commentId, [FromBody] EditCommentRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new EditCommentCommand(commentId, request.NewContent), ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Eliminar comentario
    /// </summary>
    [HttpDelete("{commentId:guid}")]
    public async Task<ActionResult> DeleteComment(Guid disputeId, Guid commentId, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteCommentCommand(commentId), ct);
        return result ? NoContent() : NotFound();
    }
}

[ApiController]
[Route("api/disputes/{disputeId:guid}/[controller]")]
[Authorize]
public class MediationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MediationsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Programar sesión de mediación
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Mediator")]
    public async Task<ActionResult<Guid>> ScheduleMediation(Guid disputeId, [FromBody] ScheduleMediationRequest request, CancellationToken ct)
    {
        var command = new ScheduleMediationCommand(
            disputeId, request.ScheduledAt, request.DurationMinutes, request.Channel,
            request.MeetingLink, request.Location, request.MediatorId, request.MediatorName);

        var id = await _mediator.Send(command, ct);
        return Created($"api/disputes/{disputeId}/mediations/{id}", id);
    }

    /// <summary>
    /// Listar sesiones de una disputa
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<MediationSessionDto>>> GetByDispute(Guid disputeId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMediationsByDisputeQuery(disputeId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener sesión por ID
    /// </summary>
    [HttpGet("{sessionId:guid}")]
    public async Task<ActionResult<MediationSessionDto>> GetById(Guid disputeId, Guid sessionId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMediationSessionByIdQuery(sessionId), ct);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Iniciar sesión de mediación
    /// </summary>
    [HttpPost("{sessionId:guid}/start")]
    [Authorize(Roles = "Admin,Mediator")]
    public async Task<ActionResult> StartMediation(Guid disputeId, Guid sessionId, CancellationToken ct)
    {
        var result = await _mediator.Send(new StartMediationCommand(sessionId), ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Completar sesión de mediación
    /// </summary>
    [HttpPost("{sessionId:guid}/complete")]
    [Authorize(Roles = "Admin,Mediator")]
    public async Task<ActionResult> CompleteMediation(Guid disputeId, Guid sessionId, [FromBody] CompleteMediationRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CompleteMediationCommand(
            sessionId, request.Summary, request.Notes, request.PartiesAgreed,
            request.ComplainantAttended, request.RespondentAttended), ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Cancelar sesión de mediación
    /// </summary>
    [HttpPost("{sessionId:guid}/cancel")]
    [Authorize(Roles = "Admin,Mediator")]
    public async Task<ActionResult> CancelMediation(Guid disputeId, Guid sessionId, [FromBody] CancelMediationRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelMediationCommand(sessionId, request.Reason), ct);
        return result ? NoContent() : NotFound();
    }
}

[ApiController]
[Route("api/disputes/{disputeId:guid}/[controller]")]
[Authorize]
public class TimelineController : ControllerBase
{
    private readonly IMediator _mediator;

    public TimelineController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtener timeline de una disputa
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<DisputeTimelineEventDto>>> GetTimeline(Guid disputeId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTimelineByDisputeQuery(disputeId), ct);
        return Ok(result);
    }
}

[ApiController]
[Route("api/disputes/{disputeId:guid}/[controller]")]
[Authorize]
public class ParticipantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ParticipantsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Agregar participante
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Mediator")]
    public async Task<ActionResult<Guid>> AddParticipant(Guid disputeId, [FromBody] AddParticipantRequest request, CancellationToken ct)
    {
        var command = new AddParticipantCommand(disputeId, request.UserId, request.UserName, request.UserEmail, request.Role);
        var id = await _mediator.Send(command, ct);
        return Created($"api/disputes/{disputeId}/participants/{id}", id);
    }

    /// <summary>
    /// Listar participantes
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<DisputeParticipantDto>>> GetByDispute(Guid disputeId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetParticipantsByDisputeQuery(disputeId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Remover participante
    /// </summary>
    [HttpDelete("{participantId:guid}")]
    [Authorize(Roles = "Admin,Mediator")]
    public async Task<ActionResult> RemoveParticipant(Guid disputeId, Guid participantId, CancellationToken ct)
    {
        var result = await _mediator.Send(new RemoveParticipantCommand(participantId), ct);
        return result ? NoContent() : NotFound();
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ResolutionTemplatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ResolutionTemplatesController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Crear plantilla de resolución
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateTemplate([FromBody] CreateResolutionTemplateCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return Created($"api/resolution-templates/{id}", id);
    }

    /// <summary>
    /// Listar plantillas activas
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ResolutionTemplateDto>>> GetTemplates([FromQuery] DisputeType? disputeType, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetResolutionTemplatesQuery(disputeType), ct);
        return Ok(result);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SlaConfigurationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SlaConfigurationsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Crear configuración SLA
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateSlaConfiguration([FromBody] CreateSlaConfigurationCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return Created($"api/sla-configurations/{id}", id);
    }

    /// <summary>
    /// Listar configuraciones SLA
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<DisputeSlaConfigurationDto>>> GetConfigurations(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSlaConfigurationsQuery(), ct);
        return Ok(result);
    }
}

#region Request DTOs

public record AssignMediatorRequest(string MediatorId, string MediatorName);
public record EscalateRequest(string Reason);
public record ResolveRequest(ResolutionType Resolution, string ResolutionSummary, string ResolvedBy);
public record ReferProConsumidorRequest(string Reason);
public record CloseRequest(string Reason);

public record SubmitEvidenceRequest(
    string Name,
    string? Description,
    string EvidenceType,
    string FileName,
    string ContentType,
    long FileSize,
    string StoragePath,
    Guid SubmittedById,
    string SubmittedByName,
    ParticipantRole SubmitterRole);

public record ReviewEvidenceRequest(EvidenceStatus NewStatus, string ReviewedBy, string? ReviewNotes);

public record AddCommentRequest(
    Guid AuthorId,
    string AuthorName,
    ParticipantRole AuthorRole,
    string Content,
    bool IsInternal,
    bool IsOfficial);

public record EditCommentRequest(string NewContent);

public record ScheduleMediationRequest(
    DateTime ScheduledAt,
    int DurationMinutes,
    CommunicationChannel Channel,
    string? MeetingLink,
    string? Location,
    string MediatorId,
    string MediatorName);

public record CompleteMediationRequest(
    string Summary,
    string? Notes,
    bool PartiesAgreed,
    bool ComplainantAttended,
    bool RespondentAttended);

public record CancelMediationRequest(string Reason);

public record AddParticipantRequest(Guid UserId, string UserName, string UserEmail, ParticipantRole Role);

#endregion
