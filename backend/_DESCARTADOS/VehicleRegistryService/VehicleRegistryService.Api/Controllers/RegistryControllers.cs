// =====================================================
// VehicleRegistryService - Controllers
// Ley 63-17 Movilidad, Transporte y Tránsito (INTRANT)
// =====================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using VehicleRegistryService.Application.Commands;
using VehicleRegistryService.Application.Queries;
using VehicleRegistryService.Application.DTOs;

namespace VehicleRegistryService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RegistrationsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Consulta un registro por placa
    /// </summary>
    [HttpGet("plate/{plateNumber}")]
    public async Task<ActionResult<VehicleRegistrationDto>> GetByPlate(string plateNumber)
    {
        var result = await _mediator.Send(new GetRegistrationByPlateQuery(plateNumber));
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Consulta un registro por VIN
    /// </summary>
    [HttpGet("vin/{vin}")]
    public async Task<ActionResult<VehicleRegistrationDto>> GetByVin(string vin)
    {
        var result = await _mediator.Send(new GetRegistrationByVinQuery(vin));
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Consulta registros por propietario
    /// </summary>
    [HttpGet("owner/{identification}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<VehicleRegistrationDto>>> GetByOwner(string identification)
    {
        var result = await _mediator.Send(new GetRegistrationsByOwnerQuery(identification));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene registros expirados
    /// </summary>
    [HttpGet("expired")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<VehicleRegistrationDto>>> GetExpired()
    {
        var result = await _mediator.Send(new GetExpiredRegistrationsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo registro vehicular
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<VehicleRegistrationDto>> Create([FromBody] CreateRegistrationDto dto)
    {
        var result = await _mediator.Send(new CreateRegistrationCommand(dto));
        return CreatedAtAction(nameof(GetByPlate), new { plateNumber = result.PlateNumber }, result);
    }

    /// <summary>
    /// Renueva un registro vehicular
    /// </summary>
    [HttpPost("{id:guid}/renew")]
    [Authorize]
    public async Task<ActionResult<VehicleRegistrationDto>> Renew(Guid id)
    {
        var result = await _mediator.Send(new RenewRegistrationCommand(id));
        return Ok(result);
    }

    /// <summary>
    /// Suspende un registro
    /// </summary>
    [HttpPost("{id:guid}/suspend")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Suspend(Guid id, [FromBody] SuspendRequest request)
    {
        var result = await _mediator.Send(new SuspendRegistrationCommand(id, request.Reason));
        if (!result) return NotFound();
        return NoContent();
    }
}

public record SuspendRequest(string Reason);

[ApiController]
[Route("api/[controller]")]
public class TransfersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransfersController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtiene transferencias de un vehículo
    /// </summary>
    [HttpGet("vehicle/{vehicleId:guid}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<OwnershipTransferDto>>> GetByVehicle(Guid vehicleId)
    {
        var result = await _mediator.Send(new GetTransfersByVehicleQuery(vehicleId));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene transferencias pendientes
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<OwnershipTransferDto>>> GetPending()
    {
        var result = await _mediator.Send(new GetPendingTransfersQuery());
        return Ok(result);
    }

    /// <summary>
    /// Inicia una transferencia de propiedad
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<OwnershipTransferDto>> Create([FromBody] CreateTransferDto dto)
    {
        var result = await _mediator.Send(new CreateTransferCommand(dto));
        return Ok(result);
    }

    /// <summary>
    /// Completa una transferencia
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Complete(Guid id, [FromBody] CompleteTransferDto dto)
    {
        var result = await _mediator.Send(new CompleteTransferCommand(id, dto));
        if (!result) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Rechaza una transferencia
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Reject(Guid id, [FromBody] RejectRequest request)
    {
        var result = await _mediator.Send(new RejectTransferCommand(id, request.Reason));
        if (!result) return NotFound();
        return NoContent();
    }
}

public record RejectRequest(string Reason);

[ApiController]
[Route("api/[controller]")]
public class LiensController : ControllerBase
{
    private readonly IMediator _mediator;

    public LiensController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtiene gravámenes de un vehículo
    /// </summary>
    [HttpGet("vehicle/{vehicleId:guid}")]
    public async Task<ActionResult<IEnumerable<LienRecordDto>>> GetByVehicle(Guid vehicleId)
    {
        var result = await _mediator.Send(new GetLiensByVehicleQuery(vehicleId));
        return Ok(result);
    }

    /// <summary>
    /// Verifica si un vehículo tiene gravámenes activos
    /// </summary>
    [HttpGet("vehicle/{vehicleId:guid}/check")]
    public async Task<ActionResult<bool>> CheckHasLiens(Guid vehicleId)
    {
        var result = await _mediator.Send(new CheckVehicleHasLiensQuery(vehicleId));
        return Ok(result);
    }

    /// <summary>
    /// Registra un gravamen
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LienRecordDto>> Create([FromBody] CreateLienDto dto)
    {
        var result = await _mediator.Send(new CreateLienCommand(dto));
        return Ok(result);
    }

    /// <summary>
    /// Libera un gravamen
    /// </summary>
    [HttpPost("{id:guid}/release")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Release(Guid id, [FromBody] ReleaseLienDto dto)
    {
        var result = await _mediator.Send(new ReleaseLienCommand(id, dto));
        if (!result) return NotFound();
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
public class VinValidationController : ControllerBase
{
    private readonly IMediator _mediator;

    public VinValidationController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Valida un VIN
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<VinValidationDto>> Validate([FromBody] ValidateVinDto dto)
    {
        var result = await _mediator.Send(new ValidateVinCommand(dto));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene última validación de un VIN
    /// </summary>
    [HttpGet("{vin}")]
    public async Task<ActionResult<VinValidationDto>> GetValidation(string vin)
    {
        var result = await _mediator.Send(new GetVinValidationQuery(vin));
        if (result == null) return NotFound();
        return Ok(result);
    }
}

[ApiController]
[Route("api/[controller]")]
public class RegistryStatisticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RegistryStatisticsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtiene estadísticas del registro
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RegistryStatisticsDto>> GetStatistics()
    {
        var result = await _mediator.Send(new GetRegistryStatisticsQuery());
        return Ok(result);
    }
}
