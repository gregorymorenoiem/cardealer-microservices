// EscrowService - Command Handlers

namespace EscrowService.Application.Handlers;

using MediatR;
using EscrowService.Application.Commands;
using EscrowService.Application.DTOs;
using EscrowService.Domain.Entities;
using EscrowService.Domain.Interfaces;

#region EscrowAccount Handlers

public class CreateEscrowAccountHandler : IRequestHandler<CreateEscrowAccountCommand, CreateEscrowResponse>
{
    private readonly IEscrowAccountRepository _accountRepository;
    private readonly IReleaseConditionRepository _conditionRepository;
    private readonly IEscrowFeeConfigurationRepository _feeRepository;
    private readonly IEscrowAuditLogRepository _auditRepository;

    public CreateEscrowAccountHandler(
        IEscrowAccountRepository accountRepository,
        IReleaseConditionRepository conditionRepository,
        IEscrowFeeConfigurationRepository feeRepository,
        IEscrowAuditLogRepository auditRepository)
    {
        _accountRepository = accountRepository;
        _conditionRepository = conditionRepository;
        _feeRepository = feeRepository;
        _auditRepository = auditRepository;
    }

    public async Task<CreateEscrowResponse> Handle(CreateEscrowAccountCommand request, CancellationToken ct)
    {
        // Calculate fee
        var feeConfig = await _feeRepository.GetActiveForAmountAsync(request.TransactionType, request.TotalAmount, ct);
        decimal feeAmount = 0;
        decimal feePercentage = 0;
        
        if (feeConfig != null)
        {
            feePercentage = feeConfig.FeePercentage;
            feeAmount = Math.Max(feeConfig.MinFee, Math.Min(feeConfig.MaxFee, request.TotalAmount * feeConfig.FeePercentage / 100));
        }

        var account = new EscrowAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = GenerateAccountNumber(),
            TransactionType = request.TransactionType,
            Status = EscrowStatus.Pending,
            BuyerId = request.BuyerId,
            BuyerName = request.BuyerName,
            BuyerEmail = request.BuyerEmail,
            BuyerPhone = request.BuyerPhone,
            SellerId = request.SellerId,
            SellerName = request.SellerName,
            SellerEmail = request.SellerEmail,
            SellerPhone = request.SellerPhone,
            SubjectType = request.SubjectType,
            SubjectId = request.SubjectId,
            SubjectDescription = request.SubjectDescription,
            ContractId = request.ContractId,
            TotalAmount = request.TotalAmount,
            PendingAmount = request.TotalAmount,
            FeeAmount = feeAmount,
            FeePercentage = feePercentage,
            Currency = request.Currency ?? "DOP",
            ExpiresAt = request.ExpirationDays.HasValue 
                ? DateTime.UtcNow.AddDays(request.ExpirationDays.Value) 
                : DateTime.UtcNow.AddDays(30),
            ReleaseDelayDays = request.ReleaseDelayDays ?? 3,
            AutoReleaseEnabled = request.AutoReleaseEnabled,
            RequiresBothApproval = request.RequiresBothApproval,
            AllowPartialRelease = request.AllowPartialRelease,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        await _accountRepository.AddAsync(account, ct);

        // Add conditions
        if (request.Conditions != null)
        {
            foreach (var conditionDto in request.Conditions)
            {
                var condition = new ReleaseCondition
                {
                    Id = Guid.NewGuid(),
                    EscrowAccountId = account.Id,
                    Type = conditionDto.Type,
                    Name = conditionDto.Name,
                    Description = conditionDto.Description,
                    IsMandatory = conditionDto.IsMandatory,
                    Order = conditionDto.Order,
                    RequiresEvidence = conditionDto.RequiresEvidence,
                    DueDate = conditionDto.DueDate,
                    Status = ConditionStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                await _conditionRepository.AddAsync(condition, ct);
            }
        }

        // Audit log
        await _auditRepository.AddAsync(new EscrowAuditLog
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = account.Id,
            EventType = EscrowAuditEventType.Created,
            Description = $"Cuenta escrow creada por ${request.TotalAmount:N2} {request.Currency}",
            AmountInvolved = request.TotalAmount,
            PerformedBy = request.CreatedBy,
            PerformedAt = DateTime.UtcNow
        }, ct);

        return new CreateEscrowResponse(account.Id, account.AccountNumber, account.TotalAmount, feeAmount, account.ExpiresAt);
    }

    private string GenerateAccountNumber()
    {
        return $"ESC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }
}

public class FundEscrowHandler : IRequestHandler<FundEscrowCommand, FundEscrowResponse>
{
    private readonly IEscrowAccountRepository _accountRepository;
    private readonly IFundMovementRepository _movementRepository;
    private readonly IEscrowAuditLogRepository _auditRepository;

    public FundEscrowHandler(
        IEscrowAccountRepository accountRepository,
        IFundMovementRepository movementRepository,
        IEscrowAuditLogRepository auditRepository)
    {
        _accountRepository = accountRepository;
        _movementRepository = movementRepository;
        _auditRepository = auditRepository;
    }

    public async Task<FundEscrowResponse> Handle(FundEscrowCommand request, CancellationToken ct)
    {
        var account = await _accountRepository.GetByIdAsync(request.EscrowAccountId, ct);
        if (account == null)
            return new FundEscrowResponse(false, Guid.Empty, "", 0, 0, "Cuenta escrow no encontrada");

        if (account.Status != EscrowStatus.Pending && account.Status != EscrowStatus.Funded)
            return new FundEscrowResponse(false, Guid.Empty, "", 0, 0, "La cuenta no acepta fondos en su estado actual");

        var movement = new FundMovement
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = account.Id,
            TransactionNumber = GenerateTransactionNumber(),
            Type = account.FundedAmount == 0 ? FundMovementType.Deposit : FundMovementType.AdditionalDeposit,
            PaymentMethod = request.PaymentMethod,
            Amount = request.Amount,
            Currency = account.Currency,
            SourceAccount = request.SourceAccount,
            BankName = request.BankName,
            BankReference = request.BankReference,
            Status = "Completed",
            PartyId = account.BuyerId,
            PartyName = account.BuyerName,
            PartyType = "Buyer",
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            InitiatedBy = request.FundedBy
        };

        await _movementRepository.AddAsync(movement, ct);

        // Update account
        account.FundedAmount += request.Amount;
        account.PendingAmount = account.TotalAmount - account.FundedAmount;
        
        if (account.FundedAmount >= account.TotalAmount)
        {
            account.Status = EscrowStatus.Funded;
            account.FundedAt = DateTime.UtcNow;
        }
        
        account.UpdatedAt = DateTime.UtcNow;
        await _accountRepository.UpdateAsync(account, ct);

        // Audit
        await _auditRepository.AddAsync(new EscrowAuditLog
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = account.Id,
            EventType = EscrowAuditEventType.Funded,
            Description = $"Fondos recibidos: ${request.Amount:N2}",
            AmountInvolved = request.Amount,
            PerformedBy = request.FundedBy,
            PerformedAt = DateTime.UtcNow
        }, ct);

        return new FundEscrowResponse(true, movement.Id, movement.TransactionNumber, account.FundedAmount, account.PendingAmount, "Fondos recibidos exitosamente");
    }

    private string GenerateTransactionNumber()
    {
        return $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
    }
}

public class ApproveReleaseHandler : IRequestHandler<ApproveReleaseCommand, bool>
{
    private readonly IEscrowAccountRepository _accountRepository;
    private readonly IEscrowAuditLogRepository _auditRepository;

    public ApproveReleaseHandler(
        IEscrowAccountRepository accountRepository,
        IEscrowAuditLogRepository auditRepository)
    {
        _accountRepository = accountRepository;
        _auditRepository = auditRepository;
    }

    public async Task<bool> Handle(ApproveReleaseCommand request, CancellationToken ct)
    {
        var account = await _accountRepository.GetByIdAsync(request.EscrowAccountId, ct);
        if (account == null) return false;

        if (request.ApproverType == "Buyer")
        {
            account.BuyerApproved = true;
            account.BuyerApprovedAt = DateTime.UtcNow;
        }
        else if (request.ApproverType == "Seller")
        {
            account.SellerApproved = true;
            account.SellerApprovedAt = DateTime.UtcNow;
        }
        else
        {
            return false;
        }

        // Check if ready for release
        if (account.RequiresBothApproval && account.BuyerApproved && account.SellerApproved)
        {
            account.Status = EscrowStatus.PendingRelease;
        }
        else if (!account.RequiresBothApproval && (account.BuyerApproved || account.SellerApproved))
        {
            account.Status = EscrowStatus.PendingRelease;
        }

        account.UpdatedAt = DateTime.UtcNow;
        await _accountRepository.UpdateAsync(account, ct);

        await _auditRepository.AddAsync(new EscrowAuditLog
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = account.Id,
            EventType = EscrowAuditEventType.ReleaseRequested,
            Description = $"Liberación aprobada por {request.ApproverType}",
            PerformedBy = request.ApprovedBy,
            PerformedAt = DateTime.UtcNow
        }, ct);

        return true;
    }
}

public class ReleaseEscrowHandler : IRequestHandler<ReleaseEscrowCommand, ReleaseEscrowResponse>
{
    private readonly IEscrowAccountRepository _accountRepository;
    private readonly IFundMovementRepository _movementRepository;
    private readonly IReleaseConditionRepository _conditionRepository;
    private readonly IEscrowAuditLogRepository _auditRepository;

    public ReleaseEscrowHandler(
        IEscrowAccountRepository accountRepository,
        IFundMovementRepository movementRepository,
        IReleaseConditionRepository conditionRepository,
        IEscrowAuditLogRepository auditRepository)
    {
        _accountRepository = accountRepository;
        _movementRepository = movementRepository;
        _conditionRepository = conditionRepository;
        _auditRepository = auditRepository;
    }

    public async Task<ReleaseEscrowResponse> Handle(ReleaseEscrowCommand request, CancellationToken ct)
    {
        var account = await _accountRepository.GetByIdAsync(request.EscrowAccountId, ct);
        if (account == null)
            return new ReleaseEscrowResponse(false, null, 0, "Cuenta escrow no encontrada");

        // Check conditions
        var allConditionsMet = await _conditionRepository.AllMandatoryConditionsMetAsync(account.Id, ct);
        if (!allConditionsMet)
            return new ReleaseEscrowResponse(false, null, 0, "No todas las condiciones obligatorias han sido cumplidas");

        // Check approvals
        if (account.RequiresBothApproval && (!account.BuyerApproved || !account.SellerApproved))
            return new ReleaseEscrowResponse(false, null, 0, "Se requiere aprobación de ambas partes");

        var releaseAmount = request.Amount ?? (account.FundedAmount - account.ReleasedAmount - account.FeeAmount);
        
        if (releaseAmount <= 0)
            return new ReleaseEscrowResponse(false, null, 0, "No hay fondos disponibles para liberar");

        if (!account.AllowPartialRelease && request.Amount.HasValue && request.Amount.Value < releaseAmount)
            return new ReleaseEscrowResponse(false, null, 0, "Esta cuenta no permite liberaciones parciales");

        var movement = new FundMovement
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = account.Id,
            TransactionNumber = $"REL-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            Type = request.Amount.HasValue ? FundMovementType.PartialRelease : FundMovementType.Release,
            PaymentMethod = PaymentMethod.BankTransfer,
            Amount = releaseAmount,
            Currency = account.Currency,
            DestinationAccount = request.DestinationAccount,
            BankName = request.BankName,
            Status = "Completed",
            PartyId = account.SellerId,
            PartyName = account.SellerName,
            PartyType = "Seller",
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            InitiatedBy = request.ReleasedBy,
            Notes = request.Notes
        };

        await _movementRepository.AddAsync(movement, ct);

        account.ReleasedAmount += releaseAmount;
        account.ReleasedAt = DateTime.UtcNow;
        
        if (account.ReleasedAmount >= account.FundedAmount - account.FeeAmount)
        {
            account.Status = EscrowStatus.Released;
        }
        else
        {
            account.Status = EscrowStatus.PartialRelease;
        }

        account.UpdatedAt = DateTime.UtcNow;
        await _accountRepository.UpdateAsync(account, ct);

        await _auditRepository.AddAsync(new EscrowAuditLog
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = account.Id,
            EventType = EscrowAuditEventType.Released,
            Description = $"Fondos liberados al vendedor: ${releaseAmount:N2}",
            AmountInvolved = releaseAmount,
            PerformedBy = request.ReleasedBy,
            PerformedAt = DateTime.UtcNow
        }, ct);

        return new ReleaseEscrowResponse(true, movement.Id, releaseAmount, "Fondos liberados exitosamente");
    }
}

public class RefundEscrowHandler : IRequestHandler<RefundEscrowCommand, RefundEscrowResponse>
{
    private readonly IEscrowAccountRepository _accountRepository;
    private readonly IFundMovementRepository _movementRepository;
    private readonly IEscrowAuditLogRepository _auditRepository;

    public RefundEscrowHandler(
        IEscrowAccountRepository accountRepository,
        IFundMovementRepository movementRepository,
        IEscrowAuditLogRepository auditRepository)
    {
        _accountRepository = accountRepository;
        _movementRepository = movementRepository;
        _auditRepository = auditRepository;
    }

    public async Task<RefundEscrowResponse> Handle(RefundEscrowCommand request, CancellationToken ct)
    {
        var account = await _accountRepository.GetByIdAsync(request.EscrowAccountId, ct);
        if (account == null)
            return new RefundEscrowResponse(false, null, 0, "Cuenta escrow no encontrada");

        var availableForRefund = account.FundedAmount - account.ReleasedAmount - account.RefundedAmount;
        var refundAmount = request.Amount ?? availableForRefund;

        if (refundAmount <= 0 || refundAmount > availableForRefund)
            return new RefundEscrowResponse(false, null, 0, "Monto de reembolso inválido");

        var movement = new FundMovement
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = account.Id,
            TransactionNumber = $"REF-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            Type = request.Amount.HasValue ? FundMovementType.PartialRefund : FundMovementType.Refund,
            PaymentMethod = PaymentMethod.BankTransfer,
            Amount = refundAmount,
            Currency = account.Currency,
            Status = "Completed",
            PartyId = account.BuyerId,
            PartyName = account.BuyerName,
            PartyType = "Buyer",
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            InitiatedBy = request.RefundedBy,
            Notes = $"{request.Reason}. {request.Notes}"
        };

        await _movementRepository.AddAsync(movement, ct);

        account.RefundedAmount += refundAmount;
        account.RefundedAt = DateTime.UtcNow;
        account.Status = EscrowStatus.Refunded;
        account.UpdatedAt = DateTime.UtcNow;

        await _accountRepository.UpdateAsync(account, ct);

        await _auditRepository.AddAsync(new EscrowAuditLog
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = account.Id,
            EventType = EscrowAuditEventType.Refunded,
            Description = $"Reembolso al comprador: ${refundAmount:N2}. Razón: {request.Reason}",
            AmountInvolved = refundAmount,
            PerformedBy = request.RefundedBy,
            PerformedAt = DateTime.UtcNow
        }, ct);

        return new RefundEscrowResponse(true, movement.Id, refundAmount, "Reembolso procesado exitosamente");
    }
}

public class CancelEscrowHandler : IRequestHandler<CancelEscrowCommand, bool>
{
    private readonly IEscrowAccountRepository _accountRepository;
    private readonly IEscrowAuditLogRepository _auditRepository;

    public CancelEscrowHandler(
        IEscrowAccountRepository accountRepository,
        IEscrowAuditLogRepository auditRepository)
    {
        _accountRepository = accountRepository;
        _auditRepository = auditRepository;
    }

    public async Task<bool> Handle(CancelEscrowCommand request, CancellationToken ct)
    {
        var account = await _accountRepository.GetByIdAsync(request.EscrowAccountId, ct);
        if (account == null) return false;

        if (account.Status == EscrowStatus.Released || account.Status == EscrowStatus.Refunded)
            return false;

        account.Status = EscrowStatus.Cancelled;
        account.CancelledAt = DateTime.UtcNow;
        account.UpdatedAt = DateTime.UtcNow;
        account.UpdatedBy = request.CancelledBy;

        await _accountRepository.UpdateAsync(account, ct);

        await _auditRepository.AddAsync(new EscrowAuditLog
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = account.Id,
            EventType = EscrowAuditEventType.Cancelled,
            Description = $"Escrow cancelado. Razón: {request.Reason}",
            PerformedBy = request.CancelledBy,
            PerformedAt = DateTime.UtcNow
        }, ct);

        return true;
    }
}

#endregion

#region Condition Handlers

public class AddConditionHandler : IRequestHandler<AddConditionCommand, Guid>
{
    private readonly IReleaseConditionRepository _conditionRepository;

    public AddConditionHandler(IReleaseConditionRepository conditionRepository)
    {
        _conditionRepository = conditionRepository;
    }

    public async Task<Guid> Handle(AddConditionCommand request, CancellationToken ct)
    {
        var condition = new ReleaseCondition
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = request.EscrowAccountId,
            Type = request.Type,
            Name = request.Name,
            Description = request.Description,
            IsMandatory = request.IsMandatory,
            Order = request.Order,
            RequiresEvidence = request.RequiresEvidence,
            DueDate = request.DueDate,
            Status = ConditionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _conditionRepository.AddAsync(condition, ct);
        return condition.Id;
    }
}

public class MarkConditionMetHandler : IRequestHandler<MarkConditionMetCommand, bool>
{
    private readonly IReleaseConditionRepository _conditionRepository;
    private readonly IEscrowAccountRepository _accountRepository;
    private readonly IEscrowAuditLogRepository _auditRepository;

    public MarkConditionMetHandler(
        IReleaseConditionRepository conditionRepository,
        IEscrowAccountRepository accountRepository,
        IEscrowAuditLogRepository auditRepository)
    {
        _conditionRepository = conditionRepository;
        _accountRepository = accountRepository;
        _auditRepository = auditRepository;
    }

    public async Task<bool> Handle(MarkConditionMetCommand request, CancellationToken ct)
    {
        var condition = await _conditionRepository.GetByIdAsync(request.ConditionId, ct);
        if (condition == null) return false;

        condition.Status = ConditionStatus.Met;
        condition.ActualValue = request.ActualValue;
        condition.EvidenceDocumentId = request.EvidenceDocumentId;
        condition.VerifiedBy = request.VerifiedBy;
        condition.VerificationNotes = request.VerificationNotes;
        condition.MetAt = DateTime.UtcNow;
        condition.UpdatedAt = DateTime.UtcNow;

        await _conditionRepository.UpdateAsync(condition, ct);

        // Check if all conditions met and update account status
        var allMet = await _conditionRepository.AllMandatoryConditionsMetAsync(condition.EscrowAccountId, ct);
        if (allMet)
        {
            var account = await _accountRepository.GetByIdAsync(condition.EscrowAccountId, ct);
            if (account != null && account.Status == EscrowStatus.InProgress)
            {
                account.Status = EscrowStatus.ConditionsMet;
                account.UpdatedAt = DateTime.UtcNow;
                await _accountRepository.UpdateAsync(account, ct);
            }
        }

        await _auditRepository.AddAsync(new EscrowAuditLog
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = condition.EscrowAccountId,
            EventType = EscrowAuditEventType.ConditionMet,
            Description = $"Condición cumplida: {condition.Name}",
            PerformedBy = request.VerifiedBy,
            PerformedAt = DateTime.UtcNow
        }, ct);

        return true;
    }
}

#endregion

#region Dispute Handlers

public class FileDisputeHandler : IRequestHandler<FileDisputeCommand, Guid>
{
    private readonly IEscrowDisputeRepository _disputeRepository;
    private readonly IEscrowAccountRepository _accountRepository;
    private readonly IEscrowAuditLogRepository _auditRepository;

    public FileDisputeHandler(
        IEscrowDisputeRepository disputeRepository,
        IEscrowAccountRepository accountRepository,
        IEscrowAuditLogRepository auditRepository)
    {
        _disputeRepository = disputeRepository;
        _accountRepository = accountRepository;
        _auditRepository = auditRepository;
    }

    public async Task<Guid> Handle(FileDisputeCommand request, CancellationToken ct)
    {
        var dispute = new EscrowDispute
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = request.EscrowAccountId,
            DisputeNumber = $"DSP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            Status = EscrowDisputeStatus.Filed,
            FiledById = request.FiledById,
            FiledByName = request.FiledByName,
            FiledByType = request.FiledByType,
            Reason = request.Reason,
            Description = request.Description,
            DisputedAmount = request.DisputedAmount,
            Category = request.Category,
            FiledAt = DateTime.UtcNow
        };

        await _disputeRepository.AddAsync(dispute, ct);

        // Update account status
        var account = await _accountRepository.GetByIdAsync(request.EscrowAccountId, ct);
        if (account != null)
        {
            account.Status = EscrowStatus.Disputed;
            account.UpdatedAt = DateTime.UtcNow;
            await _accountRepository.UpdateAsync(account, ct);
        }

        await _auditRepository.AddAsync(new EscrowAuditLog
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = request.EscrowAccountId,
            EventType = EscrowAuditEventType.Disputed,
            Description = $"Disputa presentada: {request.Reason}",
            AmountInvolved = request.DisputedAmount,
            PerformedBy = request.FiledByName,
            PerformedAt = DateTime.UtcNow
        }, ct);

        return dispute.Id;
    }
}

public class ResolveDisputeHandler : IRequestHandler<ResolveDisputeCommand, bool>
{
    private readonly IEscrowDisputeRepository _disputeRepository;
    private readonly IEscrowAuditLogRepository _auditRepository;

    public ResolveDisputeHandler(
        IEscrowDisputeRepository disputeRepository,
        IEscrowAuditLogRepository auditRepository)
    {
        _disputeRepository = disputeRepository;
        _auditRepository = auditRepository;
    }

    public async Task<bool> Handle(ResolveDisputeCommand request, CancellationToken ct)
    {
        var dispute = await _disputeRepository.GetByIdAsync(request.DisputeId, ct);
        if (dispute == null) return false;

        dispute.Status = EscrowDisputeStatus.Resolved;
        dispute.Resolution = request.Resolution;
        dispute.ResolutionNotes = request.ResolutionNotes;
        dispute.ResolvedBuyerAmount = request.ResolvedBuyerAmount;
        dispute.ResolvedSellerAmount = request.ResolvedSellerAmount;
        dispute.ResolvedAt = DateTime.UtcNow;
        dispute.ResolvedBy = request.ResolvedBy;

        await _disputeRepository.UpdateAsync(dispute, ct);

        await _auditRepository.AddAsync(new EscrowAuditLog
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = dispute.EscrowAccountId,
            EventType = EscrowAuditEventType.DisputeResolved,
            Description = $"Disputa resuelta: {request.Resolution}",
            PerformedBy = request.ResolvedBy,
            PerformedAt = DateTime.UtcNow
        }, ct);

        return true;
    }
}

#endregion
