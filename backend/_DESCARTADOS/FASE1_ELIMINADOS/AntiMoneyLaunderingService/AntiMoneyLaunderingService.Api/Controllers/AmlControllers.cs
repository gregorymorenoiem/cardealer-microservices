// =====================================================
// AntiMoneyLaunderingService - Controllers
// Ley 155-17 Prevención de Lavado de Activos (PLD)
// =====================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using AntiMoneyLaunderingService.Application.Commands;
using AntiMoneyLaunderingService.Application.Queries;
using AntiMoneyLaunderingService.Application.DTOs;

namespace AntiMoneyLaunderingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene todos los clientes con paginación
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAllCustomersQuery(page, pageSize));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un cliente por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un cliente por User ID
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<CustomerDto>> GetByUserId(Guid userId)
    {
        var result = await _mediator.Send(new GetCustomerByUserIdQuery(userId));
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene clientes de alto riesgo
    /// </summary>
    [HttpGet("high-risk")]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetHighRisk()
    {
        var result = await _mediator.Send(new GetHighRiskCustomersQuery());
        return Ok(result);
    }

    /// <summary>
    /// Obtiene Personas Políticamente Expuestas (PEP)
    /// </summary>
    [HttpGet("peps")]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetPeps()
    {
        var result = await _mediator.Send(new GetPepCustomersQuery());
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo cliente con información KYC
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CustomerDto>> Create([FromBody] CreateCustomerDto dto)
    {
        var result = await _mediator.Send(new CreateCustomerCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Actualiza el nivel de riesgo de un cliente
    /// </summary>
    [HttpPut("{id:guid}/risk-level")]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    public async Task<ActionResult> UpdateRiskLevel(Guid id, [FromBody] UpdateRiskLevelRequest request)
    {
        var result = await _mediator.Send(new UpdateRiskLevelCommand(id, request.RiskLevel, request.Reason));
        if (!result) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Realiza revisión KYC de un cliente
    /// </summary>
    [HttpPost("{id:guid}/kyc-review")]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    public async Task<ActionResult> PerformKycReview(Guid id, [FromBody] KycReviewRequest request)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var result = await _mediator.Send(new PerformKycReviewCommand(id, userId, request.Notes, request.Approved));
        if (!result) return NotFound();
        return NoContent();
    }
}

public record UpdateRiskLevelRequest(string RiskLevel, string Reason);
public record KycReviewRequest(string Notes, bool Approved);

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene transacciones por cliente
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetByCustomer(Guid customerId)
    {
        var result = await _mediator.Send(new GetTransactionsByCustomerQuery(customerId));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene transacciones sospechosas
    /// </summary>
    [HttpGet("suspicious")]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetSuspicious()
    {
        var result = await _mediator.Send(new GetSuspiciousTransactionsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Obtiene transacciones sobre el umbral (USD 10,000 por defecto según GAFI)
    /// </summary>
    [HttpGet("above-threshold")]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAboveThreshold([FromQuery] decimal threshold = 50000)
    {
        var result = await _mediator.Send(new GetAboveThresholdTransactionsQuery(threshold));
        return Ok(result);
    }

    /// <summary>
    /// Registra una nueva transacción
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<TransactionDto>> Create([FromBody] CreateTransactionDto dto)
    {
        var result = await _mediator.Send(new CreateTransactionCommand(dto));
        return Ok(result);
    }

    /// <summary>
    /// Marca una transacción como sospechosa
    /// </summary>
    [HttpPost("{id:guid}/flag-suspicious")]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    public async Task<ActionResult> FlagAsSuspicious(Guid id, [FromBody] FlagSuspiciousRequest request)
    {
        var result = await _mediator.Send(new FlagTransactionAsSuspiciousCommand(id, request.Reason));
        if (!result) return NotFound();
        return NoContent();
    }
}

public record FlagSuspiciousRequest(string Reason);

[ApiController]
[Route("api/[controller]")]
public class SuspiciousActivityReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SuspiciousActivityReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene todos los Reportes de Operación Sospechosa (ROS)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    public async Task<ActionResult<IEnumerable<SuspiciousActivityReportDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAllSuspiciousActivityReportsQuery(page, pageSize));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un ROS por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    public async Task<ActionResult<SuspiciousActivityReportDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetSuspiciousActivityReportByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene ROS pendientes de envío a UAF
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    public async Task<ActionResult<IEnumerable<SuspiciousActivityReportDto>>> GetPending()
    {
        var result = await _mediator.Send(new GetPendingReportsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo ROS (Reporte de Operación Sospechosa)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    public async Task<ActionResult<SuspiciousActivityReportDto>> Create([FromBody] CreateSuspiciousActivityReportDto dto)
    {
        var result = await _mediator.Send(new CreateSuspiciousActivityReportCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Envía un ROS a la Unidad de Análisis Financiero (UAF)
    /// </summary>
    [HttpPost("{id:guid}/submit-to-uaf")]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    public async Task<ActionResult> SubmitToUaf(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var result = await _mediator.Send(new SubmitReportToUafCommand(id, userId));
        if (!result) return NotFound();
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AlertsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene alertas activas
    /// </summary>
    [HttpGet("active")]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    public async Task<ActionResult<IEnumerable<AmlAlertDto>>> GetActive()
    {
        var result = await _mediator.Send(new GetActiveAlertsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Obtiene alertas por cliente
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    public async Task<ActionResult<IEnumerable<AmlAlertDto>>> GetByCustomer(Guid customerId)
    {
        var result = await _mediator.Send(new GetCustomerAlertsQuery(customerId));
        return Ok(result);
    }

    /// <summary>
    /// Reconoce/cierra una alerta
    /// </summary>
    [HttpPost("{id:guid}/acknowledge")]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    public async Task<ActionResult> Acknowledge(Guid id, [FromBody] AcknowledgeAlertRequest request)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var result = await _mediator.Send(new AcknowledgeAlertCommand(id, userId, request.Notes));
        if (!result) return NotFound();
        return NoContent();
    }
}

public record AcknowledgeAlertRequest(string Notes);

[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatisticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene estadísticas AML del sistema
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    public async Task<ActionResult<AmlStatisticsDto>> GetStatistics()
    {
        var result = await _mediator.Send(new GetAmlStatisticsQuery());
        return Ok(result);
    }
}
