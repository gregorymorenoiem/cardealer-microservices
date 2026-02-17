// =====================================================
// TaxComplianceService - Controllers
// Ley 11-92 Código Tributario de República Dominicana
// =====================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using TaxComplianceService.Application.Commands;
using TaxComplianceService.Application.Queries;
using TaxComplianceService.Application.DTOs;

namespace TaxComplianceService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaxpayersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TaxpayersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene todos los contribuyentes
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<TaxpayerDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAllTaxpayersQuery(page, pageSize));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un contribuyente por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<TaxpayerDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetTaxpayerByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un contribuyente por RNC
    /// </summary>
    [HttpGet("rnc/{rnc}")]
    [Authorize]
    public async Task<ActionResult<TaxpayerDto>> GetByRnc(string rnc)
    {
        var result = await _mediator.Send(new GetTaxpayerByRncQuery(rnc));
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo contribuyente
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<TaxpayerDto>> Create([FromBody] CreateTaxpayerDto dto)
    {
        var result = await _mediator.Send(new CreateTaxpayerCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}

[ApiController]
[Route("api/[controller]")]
public class DeclarationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DeclarationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene una declaración por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<TaxDeclarationDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetTaxDeclarationByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene declaraciones por contribuyente
    /// </summary>
    [HttpGet("taxpayer/{taxpayerId:guid}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<TaxDeclarationDto>>> GetByTaxpayer(Guid taxpayerId)
    {
        var result = await _mediator.Send(new GetDeclarationsByTaxpayerQuery(taxpayerId));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene declaraciones por período
    /// </summary>
    [HttpGet("period/{period}")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<IEnumerable<TaxDeclarationDto>>> GetByPeriod(string period)
    {
        var result = await _mediator.Send(new GetDeclarationsByPeriodQuery(period));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene declaraciones pendientes
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<IEnumerable<TaxDeclarationDto>>> GetPending()
    {
        var result = await _mediator.Send(new GetPendingDeclarationsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Obtiene declaraciones vencidas
    /// </summary>
    [HttpGet("overdue")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<IEnumerable<TaxDeclarationDto>>> GetOverdue()
    {
        var result = await _mediator.Send(new GetOverdueDeclarationsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Crea una nueva declaración tributaria
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<TaxDeclarationDto>> Create([FromBody] CreateTaxDeclarationDto dto)
    {
        var result = await _mediator.Send(new CreateTaxDeclarationCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Envía declaración a DGII
    /// </summary>
    [HttpPost("{id:guid}/submit-to-dgii")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<DgiiSubmissionResultDto>> SubmitToDgii(Guid id)
    {
        var result = await _mediator.Send(new SubmitDeclarationToDgiiCommand(id));
        return Ok(result);
    }

    /// <summary>
    /// Genera declaración ITBIS automáticamente
    /// </summary>
    [HttpPost("generate-itbis")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<TaxDeclarationDto>> GenerateITBIS([FromBody] GenerateITBISRequest request)
    {
        var result = await _mediator.Send(new GenerateITBISDeclarationCommand(request.TaxpayerId, request.Period));
        return Ok(result);
    }
}

public record GenerateITBISRequest(Guid TaxpayerId, string Period);

[ApiController]
[Route("api/[controller]")]
public class NcfController : ControllerBase
{
    private readonly IMediator _mediator;

    public NcfController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene secuencias NCF de un contribuyente
    /// </summary>
    [HttpGet("taxpayer/{taxpayerId:guid}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<NcfSequenceDto>>> GetByTaxpayer(Guid taxpayerId)
    {
        var result = await _mediator.Send(new GetNcfSequencesByTaxpayerQuery(taxpayerId));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene siguiente NCF disponible
    /// </summary>
    [HttpGet("next/{taxpayerId:guid}/{ncfType}")]
    [Authorize]
    public async Task<ActionResult<string>> GetNextNcf(Guid taxpayerId, string ncfType)
    {
        var result = await _mediator.Send(new GetNextNcfCommand(taxpayerId, ncfType));
        return Ok(new { ncf = result });
    }

    /// <summary>
    /// Crea nueva secuencia NCF (autorizada por DGII)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<NcfSequenceDto>> Create([FromBody] CreateNcfSequenceDto dto)
    {
        var result = await _mediator.Send(new CreateNcfSequenceCommand(dto));
        return Ok(result);
    }
}

[ApiController]
[Route("api/[controller]")]
public class Reportes606Controller : ControllerBase
{
    private readonly IMediator _mediator;

    public Reportes606Controller(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene items del Reporte 606 (Compras)
    /// </summary>
    [HttpGet("declaration/{declarationId:guid}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Reporte606ItemDto>>> GetByDeclaration(Guid declarationId)
    {
        var result = await _mediator.Send(new GetReporte606ItemsQuery(declarationId));
        return Ok(result);
    }

    /// <summary>
    /// Agrega items al Reporte 606
    /// </summary>
    [HttpPost("declaration/{declarationId:guid}")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult> AddItems(Guid declarationId, [FromBody] List<Reporte606ItemDto> items)
    {
        await _mediator.Send(new AddReporte606ItemsCommand(declarationId, items));
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
public class Reportes607Controller : ControllerBase
{
    private readonly IMediator _mediator;

    public Reportes607Controller(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene items del Reporte 607 (Ventas)
    /// </summary>
    [HttpGet("declaration/{declarationId:guid}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Reporte607ItemDto>>> GetByDeclaration(Guid declarationId)
    {
        var result = await _mediator.Send(new GetReporte607ItemsQuery(declarationId));
        return Ok(result);
    }

    /// <summary>
    /// Agrega items al Reporte 607
    /// </summary>
    [HttpPost("declaration/{declarationId:guid}")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult> AddItems(Guid declarationId, [FromBody] List<Reporte607ItemDto> items)
    {
        await _mediator.Send(new AddReporte607ItemsCommand(declarationId, items));
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
public class TaxStatisticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TaxStatisticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene estadísticas tributarias
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<TaxStatisticsDto>> GetStatistics()
    {
        var result = await _mediator.Send(new GetTaxStatisticsQuery());
        return Ok(result);
    }
}
