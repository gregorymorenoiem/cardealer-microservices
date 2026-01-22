// =====================================================
// ConsumerProtectionService - Controllers
// Ley 358-05 Derechos del Consumidor de RD
// =====================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using ConsumerProtectionService.Application.Commands;
using ConsumerProtectionService.Application.Queries;
using ConsumerProtectionService.Application.DTOs;

namespace ConsumerProtectionService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarrantiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public WarrantiesController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtiene una garantía por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WarrantyDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetWarrantyByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene garantías por producto
    /// </summary>
    [HttpGet("product/{productId:guid}")]
    public async Task<ActionResult<IEnumerable<WarrantyDto>>> GetByProduct(Guid productId)
    {
        var result = await _mediator.Send(new GetWarrantiesByProductQuery(productId));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene garantías por consumidor
    /// </summary>
    [HttpGet("consumer/{consumerId:guid}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<WarrantyDto>>> GetByConsumer(Guid consumerId)
    {
        var result = await _mediator.Send(new GetWarrantiesByConsumerQuery(consumerId));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene garantías activas
    /// </summary>
    [HttpGet("active")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<WarrantyDto>>> GetActive()
    {
        var result = await _mediator.Send(new GetActiveWarrantiesQuery());
        return Ok(result);
    }

    /// <summary>
    /// Obtiene garantías próximas a vencer
    /// </summary>
    [HttpGet("expiring")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<WarrantyDto>>> GetExpiring([FromQuery] int daysAhead = 30)
    {
        var result = await _mediator.Send(new GetExpiringWarrantiesQuery(daysAhead));
        return Ok(result);
    }

    /// <summary>
    /// Crea una nueva garantía (mínimo 6 meses según Ley 358-05)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<ActionResult<WarrantyDto>> Create([FromBody] CreateWarrantyDto dto)
    {
        var result = await _mediator.Send(new CreateWarrantyCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Activa una garantía para un consumidor
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [Authorize]
    public async Task<ActionResult> Activate(Guid id, [FromBody] ActivateWarrantyRequest request)
    {
        var result = await _mediator.Send(new ActivateWarrantyCommand(id, request.ConsumerId));
        if (!result) return NotFound();
        return NoContent();
    }
}

public record ActivateWarrantyRequest(Guid ConsumerId);

[ApiController]
[Route("api/[controller]")]
public class WarrantyClaimsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WarrantyClaimsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtiene reclamaciones por garantía
    /// </summary>
    [HttpGet("warranty/{warrantyId:guid}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<WarrantyClaimDto>>> GetByWarranty(Guid warrantyId)
    {
        var result = await _mediator.Send(new GetWarrantyClaimsByWarrantyQuery(warrantyId));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene reclamaciones pendientes
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<ActionResult<IEnumerable<WarrantyClaimDto>>> GetPending()
    {
        var result = await _mediator.Send(new GetPendingWarrantyClaimsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Crea una reclamación de garantía
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<WarrantyClaimDto>> Create([FromBody] CreateWarrantyClaimDto dto)
    {
        var result = await _mediator.Send(new CreateWarrantyClaimCommand(dto));
        return Ok(result);
    }

    /// <summary>
    /// Resuelve una reclamación de garantía
    /// </summary>
    [HttpPost("{id:guid}/resolve")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<ActionResult> Resolve(Guid id, [FromBody] ResolveClaimDto dto)
    {
        var result = await _mediator.Send(new ResolveWarrantyClaimCommand(id, dto));
        if (!result) return NotFound();
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
public class ComplaintsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ComplaintsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtiene una reclamación por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ComplaintDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetComplaintByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene reclamaciones por consumidor
    /// </summary>
    [HttpGet("consumer/{consumerId:guid}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ComplaintDto>>> GetByConsumer(Guid consumerId)
    {
        var result = await _mediator.Send(new GetComplaintsByConsumerQuery(consumerId));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene reclamaciones por vendedor
    /// </summary>
    [HttpGet("seller/{sellerId:guid}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ComplaintDto>>> GetBySeller(Guid sellerId)
    {
        var result = await _mediator.Send(new GetComplaintsBySellerQuery(sellerId));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene reclamaciones pendientes
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<ComplaintDto>>> GetPending()
    {
        var result = await _mediator.Send(new GetPendingComplaintsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Obtiene reclamaciones escaladas a Pro Consumidor
    /// </summary>
    [HttpGet("escalated")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<ComplaintDto>>> GetEscalated()
    {
        var result = await _mediator.Send(new GetEscalatedComplaintsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Crea una reclamación (15 días para responder según Ley 358-05)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ComplaintDto>> Create([FromBody] CreateComplaintDto dto)
    {
        var result = await _mediator.Send(new CreateComplaintCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Responde a una reclamación
    /// </summary>
    [HttpPost("{id:guid}/respond")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<ActionResult> Respond(Guid id, [FromBody] RespondToComplaintDto dto)
    {
        var result = await _mediator.Send(new RespondToComplaintCommand(id, dto));
        if (!result) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Escala una reclamación a Pro Consumidor
    /// </summary>
    [HttpPost("{id:guid}/escalate")]
    [Authorize]
    public async Task<ActionResult> Escalate(Guid id, [FromBody] EscalateToProConsumidorDto dto)
    {
        var result = await _mediator.Send(new EscalateToProConsumidorCommand(id, dto));
        if (!result) return NotFound();
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
public class MediationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MediationsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtiene mediaciones programadas
    /// </summary>
    [HttpGet("scheduled")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<MediationDto>>> GetScheduled()
    {
        var result = await _mediator.Send(new GetScheduledMediationsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Programa una mediación
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MediationDto>> Schedule([FromBody] CreateMediationDto dto)
    {
        var result = await _mediator.Send(new ScheduleMediationCommand(dto));
        return Ok(result);
    }

    /// <summary>
    /// Completa una mediación
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Complete(Guid id, [FromBody] CompleteMediationRequest request)
    {
        var result = await _mediator.Send(new CompleteMediationCommand(id, request.Outcome, request.AgreementSummary));
        if (!result) return NotFound();
        return NoContent();
    }
}

public record CompleteMediationRequest(string Outcome, string? AgreementSummary);

[ApiController]
[Route("api/[controller]")]
public class ConsumerStatisticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConsumerStatisticsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtiene estadísticas de protección al consumidor
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ConsumerProtectionStatisticsDto>> GetStatistics()
    {
        var result = await _mediator.Send(new GetConsumerProtectionStatisticsQuery());
        return Ok(result);
    }
}
