// ComplianceService - Controllers (All in One)
// Controladores simplificados para gestión de compliance

namespace ComplianceService.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using ComplianceService.Application.Commands;
using ComplianceService.Application.Queries;
using ComplianceService.Application.DTOs;
using ComplianceService.Domain.Entities;

#region Frameworks Controller

/// <summary>
/// API para gestión de marcos regulatorios
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FrameworksController : ControllerBase
{
    private readonly IMediator _mediator;
    public FrameworksController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtener todos los marcos regulatorios
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RegulatoryFrameworkDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RegulatoryFrameworkDto>>> GetAll(
        [FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAllFrameworksQuery(includeInactive), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener un marco por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RegulatoryFrameworkDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegulatoryFrameworkDetailDto>> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetFrameworkByIdQuery(id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Obtener marcos por tipo de regulación
    /// </summary>
    [HttpGet("by-type/{type}")]
    [ProducesResponseType(typeof(IEnumerable<RegulatoryFrameworkDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RegulatoryFrameworkDto>>> GetByType(
        RegulationType type, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetFrameworksByTypeQuery(type), ct);
        return Ok(result);
    }

    /// <summary>
    /// Crear un nuevo marco regulatorio
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Compliance")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateFrameworkCommand command, CancellationToken ct = default)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Actualizar un marco regulatorio
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Compliance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFrameworkCommand command, CancellationToken ct = default)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        var result = await _mediator.Send(command, ct);
        if (!result) return NotFound();
        return Ok();
    }
}

#endregion

#region Requirements Controller

/// <summary>
/// API para gestión de requerimientos de compliance
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RequirementsController : ControllerBase
{
    private readonly IMediator _mediator;
    public RequirementsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtener requerimientos por framework
    /// </summary>
    [HttpGet("by-framework/{frameworkId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ComplianceRequirementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ComplianceRequirementDto>>> GetByFramework(
        Guid frameworkId, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetRequirementsByFrameworkQuery(frameworkId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener un requerimiento por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ComplianceRequirementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ComplianceRequirementDto>> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetRequirementByIdQuery(id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Obtener requerimientos próximos a vencer
    /// </summary>
    [HttpGet("upcoming-deadlines")]
    [ProducesResponseType(typeof(IEnumerable<ComplianceRequirementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ComplianceRequirementDto>>> GetUpcomingDeadlines(
        [FromQuery] int daysAhead = 30, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetUpcomingRequirementDeadlinesQuery(daysAhead), ct);
        return Ok(result);
    }

    /// <summary>
    /// Crear un nuevo requerimiento
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Compliance")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateRequirementCommand command, CancellationToken ct = default)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }
}

#endregion

#region Controls Controller

/// <summary>
/// API para gestión de controles de compliance
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ControlsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ControlsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtener controles por framework
    /// </summary>
    [HttpGet("by-framework/{frameworkId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ComplianceControlDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ComplianceControlDto>>> GetByFramework(
        Guid frameworkId, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetControlsByFrameworkQuery(frameworkId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener un control por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ComplianceControlDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ComplianceControlDto>> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetControlByIdQuery(id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Obtener controles que necesitan prueba
    /// </summary>
    [HttpGet("due-for-testing")]
    [ProducesResponseType(typeof(IEnumerable<ComplianceControlDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ComplianceControlDto>>> GetDueForTesting(
        [FromQuery] int daysAhead = 30, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetControlsDueForTestingQuery(daysAhead), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener estadísticas de controles
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ControlStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ControlStatisticsDto>> GetStatistics(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetControlStatisticsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Crear un nuevo control
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Compliance")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateControlCommand command, CancellationToken ct = default)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Registrar prueba de control
    /// </summary>
    [HttpPost("{id:guid}/test")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> RecordTest(Guid id, [FromBody] RecordControlTestRequest request, CancellationToken ct = default)
    {
        var command = new RecordControlTestCommand(
            id, 
            request.TestProcedure,
            request.TestResults,
            request.IsPassed,
            request.EffectivenessScore,
            request.Findings,
            request.Recommendations,
            request.EvidenceDocuments,
            User.Identity?.Name ?? "system"
        );
        var testId = await _mediator.Send(command, ct);
        return Created($"/api/controls/{id}/tests/{testId}", testId);
    }
}

public record RecordControlTestRequest(
    string TestProcedure,
    string? TestResults,
    bool IsPassed,
    int? EffectivenessScore,
    string? Findings,
    string? Recommendations,
    List<string>? EvidenceDocuments
);

#endregion

#region Assessments Controller

/// <summary>
/// API para gestión de evaluaciones de compliance
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssessmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AssessmentsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtener evaluaciones paginadas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<ComplianceAssessmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<ComplianceAssessmentDto>>> GetPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] ComplianceStatus? status = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAssessmentsPaginatedQuery(page, pageSize, status), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener una evaluación por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ComplianceAssessmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ComplianceAssessmentDto>> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAssessmentByIdQuery(id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Obtener evaluaciones vencidas
    /// </summary>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(IEnumerable<ComplianceAssessmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ComplianceAssessmentDto>>> GetOverdue(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetOverdueAssessmentsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener estadísticas de evaluaciones
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(AssessmentStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AssessmentStatisticsDto>> GetStatistics(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAssessmentStatisticsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Crear una nueva evaluación
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Compliance")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateAssessmentCommand command, CancellationToken ct = default)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }
}

#endregion

#region Findings Controller

/// <summary>
/// API para gestión de hallazgos de compliance
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FindingsController : ControllerBase
{
    private readonly IMediator _mediator;
    public FindingsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtener hallazgos paginados
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<ComplianceFindingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<ComplianceFindingDto>>> GetPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] FindingStatus? status = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetFindingsPaginatedQuery(page, pageSize, status), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener un hallazgo por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ComplianceFindingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ComplianceFindingDto>> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetFindingByIdQuery(id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Obtener hallazgos críticos
    /// </summary>
    [HttpGet("critical")]
    [ProducesResponseType(typeof(IEnumerable<ComplianceFindingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ComplianceFindingDto>>> GetCritical(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetCriticalFindingsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener estadísticas de hallazgos
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(FindingStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<FindingStatisticsDto>> GetStatistics(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetFindingStatisticsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Crear un nuevo hallazgo
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Compliance")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateFindingCommand command, CancellationToken ct = default)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }
}

#endregion

#region Remediations Controller

/// <summary>
/// API para gestión de acciones de remediación
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RemediationsController : ControllerBase
{
    private readonly IMediator _mediator;
    public RemediationsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtener remediaciones por hallazgo
    /// </summary>
    [HttpGet("by-finding/{findingId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<RemediationActionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RemediationActionDto>>> GetByFinding(
        Guid findingId, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetRemediationsByFindingQuery(findingId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener una remediación por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RemediationActionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RemediationActionDto>> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetRemediationByIdQuery(id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Obtener remediaciones vencidas
    /// </summary>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(IEnumerable<RemediationActionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RemediationActionDto>>> GetOverdue(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetOverdueRemediationsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Crear una nueva acción de remediación
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Compliance")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateRemediationCommand command, CancellationToken ct = default)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Completar una remediación
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteRemediationRequest request, CancellationToken ct = default)
    {
        var command = new CompleteRemediationCommand(id, request.Notes ?? "", User.Identity?.Name ?? "system");
        var result = await _mediator.Send(command, ct);
        if (!result) return NotFound();
        return Ok();
    }
}

public record CompleteRemediationRequest(string? Notes);

#endregion

#region Reports Controller

/// <summary>
/// API para gestión de reportes regulatorios
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ReportsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtener reportes paginados
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<RegulatoryReportDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<RegulatoryReportDto>>> GetPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] ReportStatus? status = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetReportsPaginatedQuery(page, pageSize, status), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener un reporte por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RegulatoryReportDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegulatoryReportDetailDto>> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetReportByIdQuery(id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Obtener reportes pendientes
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(IEnumerable<RegulatoryReportDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RegulatoryReportDto>>> GetPending(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetPendingReportsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Crear un nuevo reporte
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Compliance")]
    [ProducesResponseType(typeof(CreateReportResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateReportResponse>> Create([FromBody] CreateReportCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Enviar un reporte al regulador
    /// </summary>
    [HttpPost("{id:guid}/submit")]
    [Authorize(Roles = "Admin,Compliance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitReportRequest request, CancellationToken ct = default)
    {
        var command = new SubmitReportCommand(id, request.SubmissionReference, User.Identity?.Name ?? "system");
        var result = await _mediator.Send(command, ct);
        if (!result) return NotFound();
        return Ok();
    }
}

public record SubmitReportRequest(string? SubmissionReference);

#endregion

#region Calendar Controller

/// <summary>
/// API para gestión del calendario de compliance
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalendarController : ControllerBase
{
    private readonly IMediator _mediator;
    public CalendarController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtener eventos próximos
    /// </summary>
    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(IEnumerable<ComplianceCalendarDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ComplianceCalendarDto>>> GetUpcoming(
        [FromQuery] int daysAhead = 30, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetUpcomingCalendarItemsQuery(daysAhead), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener un evento por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ComplianceCalendarDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ComplianceCalendarDto>> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetCalendarItemByIdQuery(id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Obtener eventos vencidos
    /// </summary>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(IEnumerable<ComplianceCalendarDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ComplianceCalendarDto>>> GetOverdue(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetOverdueCalendarItemsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Crear un nuevo evento de calendario
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Compliance")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateCalendarEventCommand command, CancellationToken ct = default)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Completar un evento
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteCalendarEventRequest request, CancellationToken ct = default)
    {
        var command = new CompleteCalendarEventCommand(id, request.Notes, User.Identity?.Name ?? "system");
        var result = await _mediator.Send(command, ct);
        if (!result) return NotFound();
        return Ok();
    }
}

public record CompleteCalendarEventRequest(string? Notes);

#endregion

#region Training Controller

/// <summary>
/// API para gestión de capacitaciones de compliance
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TrainingController : ControllerBase
{
    private readonly IMediator _mediator;
    public TrainingController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtener todas las capacitaciones activas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ComplianceTrainingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ComplianceTrainingDto>>> GetAll(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAllActiveTrainingsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener una capacitación por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ComplianceTrainingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ComplianceTrainingDto>> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetTrainingByIdQuery(id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Obtener capacitaciones obligatorias
    /// </summary>
    [HttpGet("mandatory")]
    [ProducesResponseType(typeof(IEnumerable<ComplianceTrainingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ComplianceTrainingDto>>> GetMandatory(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetMandatoryTrainingsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener estadísticas de capacitaciones
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(TrainingStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TrainingStatisticsDto>> GetStatistics(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetTrainingStatisticsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Crear una nueva capacitación
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Compliance")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateTrainingCommand command, CancellationToken ct = default)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Registrar completado de capacitación
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> RecordCompletion(Guid id, [FromBody] RecordTrainingCompletionRequest request, CancellationToken ct = default)
    {
        var command = new RecordTrainingCompletionCommand(
            id,
            request.UserId,
            request.Score,
            request.Passed,
            request.CertificateUrl
        );
        var completionId = await _mediator.Send(command, ct);
        return Created($"/api/training/{id}/completions/{completionId}", completionId);
    }
}

public record RecordTrainingCompletionRequest(
    Guid UserId,
    decimal? Score,
    bool Passed,
    string? Notes,
    string? CertificateUrl
);

#endregion

#region Dashboard Controller

/// <summary>
/// API para dashboard de compliance
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    public DashboardController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtener dashboard completo de compliance
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ComplianceDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ComplianceDashboardDto>> GetDashboard(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetComplianceDashboardQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener métricas de compliance
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(IEnumerable<ComplianceMetricDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ComplianceMetricDto>>> GetMetrics(
        [FromQuery] RegulationType? type = null,
        CancellationToken ct = default)
    {
        if (type.HasValue)
        {
            var result = await _mediator.Send(new GetMetricsByRegulationQuery(type.Value), ct);
            return Ok(result);
        }
        var outOfTarget = await _mediator.Send(new GetOutOfTargetMetricsQuery(), ct);
        return Ok(outOfTarget);
    }

    /// <summary>
    /// Registrar una métrica
    /// </summary>
    [HttpPost("metrics")]
    [Authorize(Roles = "Admin,Compliance,System")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> RecordMetric([FromBody] RecordMetricCommand command, CancellationToken ct = default)
    {
        var id = await _mediator.Send(command, ct);
        return Created($"/api/dashboard/metrics/{id}", id);
    }
}

#endregion
