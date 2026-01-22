// ContractService - REST API Controllers

namespace ContractService.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ContractService.Domain.Entities;
using ContractService.Application.Commands;
using ContractService.Application.Queries;
using ContractService.Application.DTOs;

#region Template Controller

[ApiController]
[Route("api/contract-templates")]
[Authorize]
public class ContractTemplatesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ContractTemplatesController> _logger;

    public ContractTemplatesController(IMediator mediator, ILogger<ContractTemplatesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtener plantilla por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContractTemplateDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTemplateByIdQuery(id), ct);
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Obtener plantilla por código
    /// </summary>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(ContractTemplateDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTemplateByCodeQuery(code), ct);
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Obtener todas las plantillas activas
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<ContractTemplateDto>), 200)]
    public async Task<IActionResult> GetActiveTemplates(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetActiveTemplatesQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener plantillas por tipo de contrato
    /// </summary>
    [HttpGet("by-type/{type}")]
    [ProducesResponseType(typeof(List<ContractTemplateDto>), 200)]
    public async Task<IActionResult> GetByType(ContractType type, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTemplatesByTypeQuery(type), ct);
        return Ok(result);
    }

    /// <summary>
    /// Crear nueva plantilla de contrato
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Compliance")]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateTemplateCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Actualizar plantilla
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Compliance")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTemplateCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        var result = await _mediator.Send(command, ct);
        return result ? NoContent() : NotFound();
    }
}

#endregion

#region Contract Controller

[ApiController]
[Route("api/contracts")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ContractsController> _logger;

    public ContractsController(IMediator mediator, ILogger<ContractsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtener contrato por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContractDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetContractByIdQuery(id), ct);
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Obtener contrato por número
    /// </summary>
    [HttpGet("by-number/{contractNumber}")]
    [ProducesResponseType(typeof(ContractDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByNumber(string contractNumber, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetContractByNumberQuery(contractNumber), ct);
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Obtener contratos del usuario actual
    /// </summary>
    [HttpGet("my-contracts")]
    [ProducesResponseType(typeof(List<ContractSummaryDto>), 200)]
    public async Task<IActionResult> GetMyContracts([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new GetUserContractsQuery(userId, page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener contratos por sujeto (vehículo, transacción, etc.)
    /// </summary>
    [HttpGet("by-subject")]
    [ProducesResponseType(typeof(List<ContractSummaryDto>), 200)]
    public async Task<IActionResult> GetBySubject([FromQuery] string subjectType, [FromQuery] Guid subjectId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSubjectContractsQuery(subjectType, subjectId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener contratos por estado
    /// </summary>
    [HttpGet("by-status/{status}")]
    [Authorize(Roles = "Admin,Compliance")]
    [ProducesResponseType(typeof(PagedResult<ContractSummaryDto>), 200)]
    public async Task<IActionResult> GetByStatus(ContractStatus status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetContractsByStatusQuery(status, page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener contratos próximos a expirar
    /// </summary>
    [HttpGet("expiring")]
    [Authorize(Roles = "Admin,Compliance")]
    [ProducesResponseType(typeof(List<ContractSummaryDto>), 200)]
    public async Task<IActionResult> GetExpiring([FromQuery] int daysUntilExpiration = 30, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetExpiringContractsQuery(daysUntilExpiration), ct);
        return Ok(result);
    }

    /// <summary>
    /// Crear nuevo contrato
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateContractResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateContractCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.ContractId }, result);
    }

    /// <summary>
    /// Finalizar contrato (listo para firmas)
    /// </summary>
    [HttpPost("{id:guid}/finalize")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Finalize(Guid id, CancellationToken ct)
    {
        var command = new FinalizeContractCommand(id, GetUserEmail());
        var result = await _mediator.Send(command, ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Terminar contrato
    /// </summary>
    [HttpPost("{id:guid}/terminate")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Terminate(Guid id, [FromBody] TerminateRequest request, CancellationToken ct)
    {
        var command = new TerminateContractCommand(id, request.Reason, GetUserEmail());
        var result = await _mediator.Send(command, ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Aceptar términos y condiciones
    /// </summary>
    [HttpPost("{id:guid}/accept-terms")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AcceptTerms(Guid id, [FromBody] AcceptTermsRequest request, CancellationToken ct)
    {
        var command = new AcceptTermsCommand(id, request.AcceptedTerms, request.AcceptedPrivacyPolicy, GetUserEmail(), GetIpAddress());
        var result = await _mediator.Send(command, ct);
        return result ? NoContent() : NotFound();
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());
    private string GetUserEmail() => User.FindFirst("email")?.Value ?? "unknown";
    private string GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

public record TerminateRequest(string Reason);
public record AcceptTermsRequest(bool AcceptedTerms, bool AcceptedPrivacyPolicy);

#endregion

#region Signature Controller

[ApiController]
[Route("api/contract-signatures")]
[Authorize]
public class ContractSignaturesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ContractSignaturesController> _logger;

    public ContractSignaturesController(IMediator mediator, ILogger<ContractSignaturesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtener firmas de un contrato
    /// </summary>
    [HttpGet("contract/{contractId:guid}")]
    [ProducesResponseType(typeof(List<ContractSignatureDto>), 200)]
    public async Task<IActionResult> GetContractSignatures(Guid contractId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetContractSignaturesQuery(contractId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener firma por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContractSignatureDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSignatureByIdQuery(id), ct);
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Obtener mis firmas pendientes
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(List<ContractSignatureDto>), 200)]
    public async Task<IActionResult> GetPending(CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new GetPendingSignaturesQuery(userId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Solicitar firma de una parte
    /// </summary>
    [HttpPost("request")]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RequestSignature([FromBody] RequestSignatureCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Firmar contrato electrónicamente (Ley 126-02)
    /// </summary>
    [HttpPost("{id:guid}/sign")]
    [ProducesResponseType(typeof(SignContractResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Sign(Guid id, [FromBody] SignRequest request, CancellationToken ct)
    {
        var command = new SignContractCommand(
            id,
            request.SignatureData,
            request.SignatureImage,
            request.CertificateData,
            GetIpAddress(),
            GetUserAgent(),
            request.GeoLocation,
            request.DeviceFingerprint,
            request.BiometricVerified ?? false,
            request.BiometricType
        );
        
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Rechazar firma
    /// </summary>
    [HttpPost("{id:guid}/decline")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Decline(Guid id, [FromBody] DeclineRequest request, CancellationToken ct)
    {
        var command = new DeclineSignatureCommand(id, request.Reason, GetUserEmail());
        var result = await _mediator.Send(command, ct);
        return result ? NoContent() : NotFound();
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());
    private string GetUserEmail() => User.FindFirst("email")?.Value ?? "unknown";
    private string GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    private string GetUserAgent() => Request.Headers["User-Agent"].ToString();
}

public record SignRequest(
    string SignatureData,
    string? SignatureImage,
    string? CertificateData,
    string? GeoLocation,
    string? DeviceFingerprint,
    bool? BiometricVerified,
    string? BiometricType
);

public record DeclineRequest(string Reason);

#endregion

#region Parties Controller

[ApiController]
[Route("api/contract-parties")]
[Authorize]
public class ContractPartiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContractPartiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtener partes de un contrato
    /// </summary>
    [HttpGet("contract/{contractId:guid}")]
    [ProducesResponseType(typeof(List<ContractPartyDto>), 200)]
    public async Task<IActionResult> GetContractParties(Guid contractId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetContractPartiesQuery(contractId), ct);
        return Ok(result);
    }
}

#endregion

#region Clauses Controller

[ApiController]
[Route("api/contract-clauses")]
[Authorize]
public class ContractClausesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContractClausesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtener cláusulas de un contrato
    /// </summary>
    [HttpGet("contract/{contractId:guid}")]
    [ProducesResponseType(typeof(List<ContractClauseDto>), 200)]
    public async Task<IActionResult> GetContractClauses(Guid contractId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetContractClausesQuery(contractId), ct);
        return Ok(result);
    }
}

#endregion

#region Documents Controller

[ApiController]
[Route("api/contract-documents")]
[Authorize]
public class ContractDocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContractDocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtener documentos de un contrato
    /// </summary>
    [HttpGet("contract/{contractId:guid}")]
    [ProducesResponseType(typeof(List<ContractDocumentDto>), 200)]
    public async Task<IActionResult> GetContractDocuments(Guid contractId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetContractDocumentsQuery(contractId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Subir documento a un contrato
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Upload([FromBody] UploadDocumentCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetContractDocuments), new { contractId = command.ContractId }, id);
    }
}

#endregion

#region Audit Log Controller

[ApiController]
[Route("api/contract-audit")]
[Authorize(Roles = "Admin,Compliance")]
public class ContractAuditController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContractAuditController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtener historial de auditoría de un contrato
    /// </summary>
    [HttpGet("contract/{contractId:guid}")]
    [ProducesResponseType(typeof(List<ContractAuditLogDto>), 200)]
    public async Task<IActionResult> GetContractAuditLog(Guid contractId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetContractAuditLogQuery(contractId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener actividad de un usuario en contratos
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(List<ContractAuditLogDto>), 200)]
    public async Task<IActionResult> GetUserActivity(string userId, [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserContractActivityQuery(userId, from, to), ct);
        return Ok(result);
    }
}

#endregion

#region Versions Controller

[ApiController]
[Route("api/contract-versions")]
[Authorize]
public class ContractVersionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContractVersionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtener versiones de un contrato
    /// </summary>
    [HttpGet("contract/{contractId:guid}")]
    [ProducesResponseType(typeof(List<ContractVersionDto>), 200)]
    public async Task<IActionResult> GetContractVersions(Guid contractId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetContractVersionsQuery(contractId), ct);
        return Ok(result);
    }
}

#endregion

#region Certification Authorities Controller

[ApiController]
[Route("api/certification-authorities")]
[Authorize]
public class CertificationAuthoritiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CertificationAuthoritiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtener todas las autoridades certificadoras
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CertificationAuthorityDto>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCertificationAuthoritiesQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener autoridades certificadoras activas
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<CertificationAuthorityDto>), 200)]
    public async Task<IActionResult> GetActive(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetActiveCertificationAuthoritiesQuery(), ct);
        return Ok(result);
    }
}

#endregion
