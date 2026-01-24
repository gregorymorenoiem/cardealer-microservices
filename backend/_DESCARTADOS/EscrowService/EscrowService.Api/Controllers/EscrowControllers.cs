// EscrowService - REST API Controllers
// Sistema de depósito en garantía para transacciones seguras

namespace EscrowService.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using EscrowService.Application.Commands;
using EscrowService.Application.Queries;
using EscrowService.Application.DTOs;
using EscrowService.Domain.Entities;

#region EscrowAccounts Controller

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EscrowAccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EscrowAccountsController> _logger;

    public EscrowAccountsController(IMediator mediator, ILogger<EscrowAccountsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Listar cuentas escrow con filtros y paginación
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<PagedResult<EscrowAccountSummaryDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] EscrowStatus? status = null,
        [FromQuery] Guid? buyerId = null,
        [FromQuery] Guid? sellerId = null,
        CancellationToken ct = default)
    {
        var query = new GetPagedEscrowAccountsQuery(page, pageSize, status, null);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener cuenta escrow por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EscrowAccountDto>> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetEscrowAccountByIdQuery(id);
        var result = await _mediator.Send(query, ct);
        
        if (result == null)
            return NotFound(new { message = "Cuenta escrow no encontrada" });
            
        return Ok(result);
    }

    /// <summary>
    /// Obtener cuenta escrow por número de cuenta
    /// </summary>
    [HttpGet("by-number/{accountNumber}")]
    public async Task<ActionResult<EscrowAccountDto>> GetByNumber(string accountNumber, CancellationToken ct)
    {
        var query = new GetEscrowAccountByNumberQuery(accountNumber);
        var result = await _mediator.Send(query, ct);
        
        if (result == null)
            return NotFound(new { message = "Cuenta escrow no encontrada" });
            
        return Ok(result);
    }

    /// <summary>
    /// Crear nueva cuenta escrow
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CreateEscrowResponse>> Create(
        [FromBody] CreateEscrowAccountDto dto,
        CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? "System";
        
        var command = new CreateEscrowAccountCommand(
            dto.TransactionType,
            dto.BuyerId,
            dto.BuyerName,
            dto.BuyerEmail,
            dto.BuyerPhone,
            dto.SellerId,
            dto.SellerName,
            dto.SellerEmail,
            dto.SellerPhone,
            dto.SubjectType,
            dto.SubjectId,
            dto.SubjectDescription,
            dto.ContractId,
            dto.TotalAmount,
            dto.Currency,
            dto.ExpirationDays,
            dto.ReleaseDelayDays,
            dto.AutoReleaseEnabled,
            dto.RequiresBothApproval,
            dto.AllowPartialRelease,
            dto.Notes,
            dto.Conditions,
            userId
        );

        var result = await _mediator.Send(command, ct);
        
        return CreatedAtAction(
            nameof(GetById), 
            new { id = result.EscrowAccountId }, 
            result
        );
    }

    /// <summary>
    /// Depositar fondos en cuenta escrow
    /// </summary>
    [HttpPost("{id:guid}/fund")]
    public async Task<ActionResult<FundEscrowResponse>> Fund(
        Guid id,
        [FromBody] FundEscrowDto dto,
        CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? "System";
        
        var command = new FundEscrowCommand(
            id,
            dto.Amount,
            dto.PaymentMethod,
            dto.SourceAccount,
            dto.BankName,
            dto.BankReference,
            userId
        );

        var result = await _mediator.Send(command, ct);
        
        if (!result.Success)
            return BadRequest(new { message = result.Message });
            
        return Ok(result);
    }

    /// <summary>
    /// Aprobar liberación de fondos
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult> ApproveRelease(
        Guid id,
        [FromBody] ApproveReleaseDto dto,
        CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? "System";
        
        var command = new ApproveReleaseCommand(id, userId, dto.ApproverType);
        var result = await _mediator.Send(command, ct);
        
        if (!result)
            return BadRequest(new { message = "No se pudo aprobar la liberación" });
            
        return Ok(new { message = "Liberación aprobada exitosamente" });
    }

    /// <summary>
    /// Liberar fondos al vendedor
    /// </summary>
    [HttpPost("{id:guid}/release")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<ReleaseEscrowResponse>> Release(
        Guid id,
        [FromBody] ReleaseEscrowDto dto,
        CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? "System";
        
        var command = new ReleaseEscrowCommand(
            id,
            dto.Amount,
            dto.DestinationAccount,
            dto.BankName,
            userId,
            dto.Notes
        );

        var result = await _mediator.Send(command, ct);
        
        if (!result.Success)
            return BadRequest(new { message = result.Message });
            
        return Ok(result);
    }

    /// <summary>
    /// Reembolsar fondos al comprador
    /// </summary>
    [HttpPost("{id:guid}/refund")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<RefundEscrowResponse>> Refund(
        Guid id,
        [FromBody] RefundEscrowDto dto,
        CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? "System";
        
        var command = new RefundEscrowCommand(
            id,
            dto.Amount,
            dto.Reason,
            userId,
            dto.Notes
        );

        var result = await _mediator.Send(command, ct);
        
        if (!result.Success)
            return BadRequest(new { message = result.Message });
            
        return Ok(result);
    }

    /// <summary>
    /// Cancelar cuenta escrow
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult> Cancel(
        Guid id,
        [FromBody] CancelEscrowDto dto,
        CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? "System";
        
        var command = new CancelEscrowCommand(id, dto.Reason, userId);
        var result = await _mediator.Send(command, ct);
        
        if (!result)
            return BadRequest(new { message = "No se pudo cancelar la cuenta escrow" });
            
        return Ok(new { message = "Cuenta escrow cancelada" });
    }

    /// <summary>
    /// Extender fecha de expiración
    /// </summary>
    [HttpPost("{id:guid}/extend")]
    public async Task<ActionResult> ExtendExpiration(
        Guid id,
        [FromBody] ExtendExpirationDto dto,
        CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? "System";
        
        var command = new ExtendEscrowExpirationCommand(id, dto.AdditionalDays, userId, dto.Reason);
        var result = await _mediator.Send(command, ct);
        
        if (!result)
            return BadRequest(new { message = "No se pudo extender la expiración" });
            
        return Ok(new { message = "Fecha de expiración extendida" });
    }

    /// <summary>
    /// Obtener estadísticas de escrow
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<EscrowStatisticsDto>> GetStatistics(CancellationToken ct)
    {
        var query = new GetEscrowStatisticsQuery();
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }
}

#endregion

#region Conditions Controller

[ApiController]
[Route("api/escrow-conditions")]
[Authorize]
public class EscrowConditionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EscrowConditionsController> _logger;

    public EscrowConditionsController(IMediator mediator, ILogger<EscrowConditionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Agregar condición a cuenta escrow
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AddConditionResponse>> AddCondition(
        [FromBody] AddConditionDto dto,
        CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? "System";
        
        var command = new AddConditionCommand(
            dto.EscrowAccountId,
            dto.Type,
            dto.Name,
            dto.Description,
            dto.IsMandatory,
            dto.Order,
            dto.RequiresEvidence,
            dto.DueDate,
            userId
        );

        var conditionId = await _mediator.Send(command, ct);
        
        return Created(
            $"/api/escrow-conditions/{conditionId}",
            new AddConditionResponse(conditionId, dto.Name, dto.Type, dto.IsMandatory)
        );
    }

    /// <summary>
    /// Marcar condición como cumplida
    /// </summary>
    [HttpPost("{id:guid}/met")]
    public async Task<ActionResult> MarkConditionMet(
        Guid id,
        [FromBody] MarkConditionMetDto dto,
        CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? "System";
        
        var command = new MarkConditionMetCommand(
            id,
            dto.ActualValue,
            dto.EvidenceDocumentId,
            userId,
            dto.VerificationNotes
        );
        
        var result = await _mediator.Send(command, ct);
        
        if (!result)
            return BadRequest(new { message = "No se pudo marcar la condición como cumplida" });
            
        return Ok(new { message = "Condición marcada como cumplida" });
    }

    /// <summary>
    /// Marcar condición como fallida
    /// </summary>
    [HttpPost("{id:guid}/failed")]
    public async Task<ActionResult> MarkConditionFailed(
        Guid id,
        [FromBody] MarkConditionFailedDto dto,
        CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? "System";
        
        var command = new MarkConditionFailedCommand(id, dto.Reason, userId);
        var result = await _mediator.Send(command, ct);
        
        if (!result)
            return BadRequest(new { message = "No se pudo marcar la condición como fallida" });
            
        return Ok(new { message = "Condición marcada como fallida" });
    }

    /// <summary>
    /// Exonerar condición
    /// </summary>
    [HttpPost("{id:guid}/waive")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult> WaiveCondition(
        Guid id,
        [FromBody] WaiveConditionDto dto,
        CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? "System";
        
        var command = new WaiveConditionCommand(id, dto.Reason, userId);
        var result = await _mediator.Send(command, ct);
        
        if (!result)
            return BadRequest(new { message = "No se pudo exonerar la condición" });
            
        return Ok(new { message = "Condición exonerada" });
    }

    /// <summary>
    /// Obtener condiciones de una cuenta escrow
    /// </summary>
    [HttpGet("by-account/{accountId:guid}")]
    public async Task<ActionResult<List<ReleaseConditionDto>>> GetByAccount(
        Guid accountId,
        CancellationToken ct)
    {
        var query = new GetConditionsByEscrowAccountQuery(accountId);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }
}

#endregion

#region Documents Controller

[ApiController]
[Route("api/escrow-documents")]
[Authorize]
public class EscrowDocumentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EscrowDocumentsController> _logger;

    public EscrowDocumentsController(IMediator mediator, ILogger<EscrowDocumentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Subir documento a cuenta escrow
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UploadDocumentResponse>> Upload(
        [FromBody] UploadDocumentDto dto,
        CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? "System";
        
        var command = new UploadDocumentCommand(
            dto.EscrowAccountId,
            dto.Name,
            dto.Description,
            dto.DocumentType,
            dto.FileName,
            dto.ContentType,
            dto.FileSize,
            dto.StoragePath,
            dto.FileHash,
            dto.VisibleToBuyer,
            dto.VisibleToSeller,
            userId
        );

        var documentId = await _mediator.Send(command, ct);
        
        return Created(
            $"/api/escrow-documents/{documentId}",
            new UploadDocumentResponse(documentId, dto.Name, dto.FileName, dto.FileSize, DateTime.UtcNow)
        );
    }

    /// <summary>
    /// Verificar documento
    /// </summary>
    [HttpPost("{id:guid}/verify")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult> Verify(Guid id, CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? "System";
        
        var command = new VerifyDocumentCommand(id, userId);
        var result = await _mediator.Send(command, ct);
        
        if (!result)
            return BadRequest(new { message = "No se pudo verificar el documento" });
            
        return Ok(new { message = "Documento verificado" });
    }

    /// <summary>
    /// Eliminar documento
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? "System";
        
        var command = new DeleteDocumentCommand(id, userId);
        var result = await _mediator.Send(command, ct);
        
        if (!result)
            return BadRequest(new { message = "No se pudo eliminar el documento" });
            
        return Ok(new { message = "Documento eliminado" });
    }

    /// <summary>
    /// Obtener documentos de una cuenta escrow
    /// </summary>
    [HttpGet("by-account/{accountId:guid}")]
    public async Task<ActionResult<List<EscrowDocumentDto>>> GetByAccount(
        Guid accountId,
        CancellationToken ct)
    {
        var query = new GetDocumentsByEscrowAccountQuery(accountId);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }
}

#endregion

#region Disputes Controller

[ApiController]
[Route("api/escrow-disputes")]
[Authorize]
public class EscrowDisputesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EscrowDisputesController> _logger;

    public EscrowDisputesController(IMediator mediator, ILogger<EscrowDisputesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Presentar disputa
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FileDisputeResponse>> FileDispute(
        [FromBody] FileDisputeDto dto,
        CancellationToken ct)
    {
        var command = new FileDisputeCommand(
            dto.EscrowAccountId,
            dto.FiledById,
            dto.FiledByName,
            dto.FiledByType,
            dto.Reason,
            dto.Description,
            dto.DisputedAmount,
            dto.Category
        );

        var disputeId = await _mediator.Send(command, ct);
        
        return Created(
            $"/api/escrow-disputes/{disputeId}",
            new FileDisputeResponse(disputeId, $"DSP-{DateTime.UtcNow:yyyyMMdd}", EscrowDisputeStatus.Filed, DateTime.UtcNow)
        );
    }

    /// <summary>
    /// Asignar disputa a revisor
    /// </summary>
    [HttpPost("{id:guid}/assign")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult> Assign(
        Guid id,
        [FromBody] AssignDisputeDto dto,
        CancellationToken ct)
    {
        var command = new AssignDisputeCommand(id, dto.AssignedTo);
        var result = await _mediator.Send(command, ct);
        
        if (!result)
            return BadRequest(new { message = "No se pudo asignar la disputa" });
            
        return Ok(new { message = "Disputa asignada" });
    }

    /// <summary>
    /// Escalar disputa
    /// </summary>
    [HttpPost("{id:guid}/escalate")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult> Escalate(
        Guid id,
        [FromBody] EscalateDisputeDto dto,
        CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? "System";
        
        var command = new EscalateDisputeCommand(id, dto.Reason, userId);
        var result = await _mediator.Send(command, ct);
        
        if (!result)
            return BadRequest(new { message = "No se pudo escalar la disputa" });
            
        return Ok(new { message = "Disputa escalada" });
    }

    /// <summary>
    /// Resolver disputa
    /// </summary>
    [HttpPost("{id:guid}/resolve")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<ResolveDisputeResponse>> Resolve(
        Guid id,
        [FromBody] ResolveDisputeDto dto,
        CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? "System";
        
        var command = new ResolveDisputeCommand(
            id,
            dto.Resolution,
            dto.ResolutionNotes,
            dto.ResolvedBuyerAmount,
            dto.ResolvedSellerAmount,
            userId
        );
        
        var result = await _mediator.Send(command, ct);
        
        if (!result)
            return BadRequest(new { message = "No se pudo resolver la disputa" });
            
        return Ok(new ResolveDisputeResponse(
            true,
            id,
            dto.Resolution,
            dto.ResolvedBuyerAmount,
            dto.ResolvedSellerAmount,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Cerrar disputa
    /// </summary>
    [HttpPost("{id:guid}/close")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult> Close(Guid id, CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? "System";
        
        var command = new CloseDisputeCommand(id, userId);
        var result = await _mediator.Send(command, ct);
        
        if (!result)
            return BadRequest(new { message = "No se pudo cerrar la disputa" });
            
        return Ok(new { message = "Disputa cerrada" });
    }

    /// <summary>
    /// Obtener disputas
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<PagedResult<EscrowDisputeDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] EscrowDisputeStatus? status = null,
        CancellationToken ct = default)
    {
        var query = new GetPagedDisputesQuery(page, pageSize, status);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Obtener disputa por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EscrowDisputeDto>> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetDisputeByIdQuery(id);
        var result = await _mediator.Send(query, ct);
        
        if (result == null)
            return NotFound(new { message = "Disputa no encontrada" });
            
        return Ok(result);
    }
}

#endregion

#region Fee Configurations Controller

[ApiController]
[Route("api/escrow-fees")]
[Authorize(Roles = "Admin")]
public class EscrowFeesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EscrowFeesController> _logger;

    public EscrowFeesController(IMediator mediator, ILogger<EscrowFeesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Crear configuración de tarifas
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CreateFeeConfigurationResponse>> Create(
        [FromBody] CreateFeeConfigurationDto dto,
        CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? "System";
        
        var command = new CreateFeeConfigurationCommand(
            dto.Name,
            dto.TransactionType,
            dto.MinAmount,
            dto.MaxAmount,
            dto.FeePercentage,
            dto.MinFee,
            dto.MaxFee,
            dto.EffectiveFrom,
            dto.EffectiveTo,
            userId
        );

        var configId = await _mediator.Send(command, ct);
        
        return Created(
            $"/api/escrow-fees/{configId}",
            new CreateFeeConfigurationResponse(configId, dto.Name, dto.FeePercentage, dto.EffectiveFrom)
        );
    }

    /// <summary>
    /// Actualizar configuración de tarifas
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        Guid id,
        [FromBody] UpdateFeeConfigurationDto dto,
        CancellationToken ct)
    {
        var userId = User.Identity?.Name ?? "System";
        
        var command = new UpdateFeeConfigurationCommand(
            id,
            dto.Name,
            dto.FeePercentage,
            dto.MinFee,
            dto.MaxFee,
            dto.EffectiveTo,
            dto.IsActive,
            userId
        );
        
        var result = await _mediator.Send(command, ct);
        
        if (!result)
            return BadRequest(new { message = "No se pudo actualizar la configuración" });
            
        return Ok(new { message = "Configuración actualizada" });
    }

    /// <summary>
    /// Obtener configuraciones de tarifas
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<EscrowFeeConfigurationDto>>> GetAll(
        [FromQuery] bool activeOnly = true,
        CancellationToken ct = default)
    {
        var query = new GetFeeConfigurationsQuery();
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }
}

#endregion

#region Audit Controller

[ApiController]
[Route("api/escrow-audit")]
[Authorize(Roles = "Admin,Compliance")]
public class EscrowAuditController : ControllerBase
{
    private readonly IMediator _mediator;

    public EscrowAuditController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtener logs de auditoría de una cuenta escrow
    /// </summary>
    [HttpGet("by-account/{accountId:guid}")]
    public async Task<ActionResult<List<EscrowAuditLogDto>>> GetByAccount(
        Guid accountId,
        CancellationToken ct)
    {
        var query = new GetAuditLogsByEscrowAccountQuery(accountId);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }
}

#endregion

#region Request DTOs

// Fund Escrow
public record FundEscrowDto(
    decimal Amount,
    PaymentMethod PaymentMethod,
    string? SourceAccount,
    string? BankName,
    string? BankReference
);

// Approve Release
public record ApproveReleaseDto(string ApproverType);

// Release Escrow
public record ReleaseEscrowDto(
    decimal? Amount,
    string? DestinationAccount,
    string? BankName,
    string? Notes
);

// Refund Escrow
public record RefundEscrowDto(
    decimal? Amount,
    string Reason,
    string? Notes
);

// Cancel Escrow
public record CancelEscrowDto(string Reason);

// Extend Expiration
public record ExtendExpirationDto(int AdditionalDays, string? Reason);

// Add Condition
public record AddConditionDto(
    Guid EscrowAccountId,
    ReleaseConditionType Type,
    string Name,
    string? Description,
    bool IsMandatory,
    int Order,
    bool RequiresEvidence,
    DateTime? DueDate
);

// Mark Condition Met
public record MarkConditionMetDto(
    string? ActualValue,
    Guid? EvidenceDocumentId,
    string? VerificationNotes
);

// Mark Condition Failed
public record MarkConditionFailedDto(string Reason);

// Waive Condition
public record WaiveConditionDto(string Reason);

// Upload Document
public record UploadDocumentDto(
    Guid EscrowAccountId,
    string Name,
    string? Description,
    string DocumentType,
    string FileName,
    string ContentType,
    long FileSize,
    string StoragePath,
    string? FileHash,
    bool VisibleToBuyer,
    bool VisibleToSeller
);

// File Dispute
public record FileDisputeDto(
    Guid EscrowAccountId,
    Guid FiledById,
    string FiledByName,
    string FiledByType,
    string Reason,
    string Description,
    decimal? DisputedAmount,
    string? Category
);

// Assign Dispute
public record AssignDisputeDto(string AssignedTo);

// Escalate Dispute
public record EscalateDisputeDto(string Reason);

// Create Fee Configuration
public record CreateFeeConfigurationDto(
    string Name,
    EscrowTransactionType TransactionType,
    decimal MinAmount,
    decimal MaxAmount,
    decimal FeePercentage,
    decimal MinFee,
    decimal MaxFee,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo
);

// Update Fee Configuration
public record UpdateFeeConfigurationDto(
    string Name,
    decimal FeePercentage,
    decimal MinFee,
    decimal MaxFee,
    DateTime? EffectiveTo,
    bool IsActive
);

#endregion
