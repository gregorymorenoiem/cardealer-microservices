// =====================================================
// DigitalSignatureService - Controllers
// Ley 339-22 Firma Digital de República Dominicana
// =====================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using DigitalSignatureService.Application.Commands;
using DigitalSignatureService.Application.Queries;
using DigitalSignatureService.Application.DTOs;

namespace DigitalSignatureService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CertificatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CertificatesController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtiene un certificado por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<DigitalCertificateDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetCertificateByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene certificados por usuario
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<DigitalCertificateDto>>> GetByUser(Guid userId)
    {
        var result = await _mediator.Send(new GetCertificatesByUserQuery(userId));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene certificados activos de un usuario
    /// </summary>
    [HttpGet("user/{userId:guid}/active")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<DigitalCertificateDto>>> GetActiveByUser(Guid userId)
    {
        var result = await _mediator.Send(new GetActiveCertificatesByUserQuery(userId));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene certificados próximos a expirar
    /// </summary>
    [HttpGet("expiring")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<DigitalCertificateDto>>> GetExpiring([FromQuery] int daysAhead = 30)
    {
        var result = await _mediator.Send(new GetExpiringCertificatesQuery(daysAhead));
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo certificado digital (OGTIC)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<DigitalCertificateDto>> Create([FromBody] CreateCertificateDto dto)
    {
        var result = await _mediator.Send(new CreateCertificateCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Revoca un certificado
    /// </summary>
    [HttpPost("{id:guid}/revoke")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Revoke(Guid id, [FromBody] RevokeCertificateDto dto)
    {
        var result = await _mediator.Send(new RevokeCertificateCommand(id, dto));
        if (!result) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Suspende un certificado
    /// </summary>
    [HttpPost("{id:guid}/suspend")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Suspend(Guid id, [FromBody] SuspendRequest request)
    {
        var result = await _mediator.Send(new SuspendCertificateCommand(id, request.Reason));
        if (!result) return NotFound();
        return NoContent();
    }
}

public record SuspendRequest(string Reason);

[ApiController]
[Route("api/[controller]")]
public class SignaturesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SignaturesController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtiene firmas de un documento
    /// </summary>
    [HttpGet("document/{documentId:guid}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<DigitalSignatureDto>>> GetByDocument(Guid documentId)
    {
        var result = await _mediator.Send(new GetSignaturesByDocumentQuery(documentId));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene firmas de un firmante
    /// </summary>
    [HttpGet("signer/{identification}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<DigitalSignatureDto>>> GetBySigner(string identification)
    {
        var result = await _mediator.Send(new GetSignaturesBySignerQuery(identification));
        return Ok(result);
    }

    /// <summary>
    /// Firma un documento digitalmente
    /// </summary>
    [HttpPost("sign")]
    [Authorize]
    public async Task<ActionResult<DigitalSignatureDto>> SignDocument([FromBody] SignDocumentDto dto)
    {
        var result = await _mediator.Send(new SignDocumentCommand(dto));
        return Ok(result);
    }

    /// <summary>
    /// Verifica la validez de una firma
    /// </summary>
    [HttpPost("verify")]
    public async Task<ActionResult<VerificationResultDto>> Verify([FromBody] VerifySignatureDto dto)
    {
        var result = await _mediator.Send(new VerifySignatureCommand(dto));
        return Ok(result);
    }

    /// <summary>
    /// Verifica todas las firmas de un documento
    /// </summary>
    [HttpPost("verify-document/{documentId:guid}")]
    public async Task<ActionResult<IEnumerable<VerificationResultDto>>> VerifyDocument(Guid documentId)
    {
        var result = await _mediator.Send(new VerifyDocumentSignaturesCommand(documentId));
        return Ok(result);
    }
}

[ApiController]
[Route("api/[controller]")]
public class TimeStampsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TimeStampsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtiene el sello de tiempo de una firma
    /// </summary>
    [HttpGet("signature/{signatureId:guid}")]
    [Authorize]
    public async Task<ActionResult<TimeStampDto>> GetBySignature(Guid signatureId)
    {
        var result = await _mediator.Send(new GetTimeStampBySignatureQuery(signatureId));
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Agrega un sello de tiempo certificado
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<TimeStampDto>> AddTimeStamp([FromBody] AddTimeStampRequest request)
    {
        var result = await _mediator.Send(new AddTimeStampCommand(request.SignatureId));
        return Ok(result);
    }
}

public record AddTimeStampRequest(Guid SignatureId);

[ApiController]
[Route("api/[controller]")]
public class SignatureStatisticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SignatureStatisticsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Obtiene estadísticas de firma digital
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SignatureStatisticsDto>> GetStatistics()
    {
        var result = await _mediator.Send(new GetSignatureStatisticsQuery());
        return Ok(result);
    }
}
