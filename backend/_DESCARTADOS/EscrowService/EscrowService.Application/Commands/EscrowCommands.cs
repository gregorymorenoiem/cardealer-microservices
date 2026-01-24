// EscrowService - Commands

namespace EscrowService.Application.Commands;

using MediatR;
using EscrowService.Application.DTOs;
using EscrowService.Domain.Entities;

#region EscrowAccount Commands

public record CreateEscrowAccountCommand(
    EscrowTransactionType TransactionType,
    Guid BuyerId,
    string BuyerName,
    string BuyerEmail,
    string? BuyerPhone,
    Guid SellerId,
    string SellerName,
    string SellerEmail,
    string? SellerPhone,
    string? SubjectType,
    Guid? SubjectId,
    string? SubjectDescription,
    Guid? ContractId,
    decimal TotalAmount,
    string Currency,
    int? ExpirationDays,
    int? ReleaseDelayDays,
    bool AutoReleaseEnabled,
    bool RequiresBothApproval,
    bool AllowPartialRelease,
    string? Notes,
    List<CreateReleaseConditionDto>? Conditions,
    string CreatedBy
) : IRequest<CreateEscrowResponse>;

public record FundEscrowCommand(
    Guid EscrowAccountId,
    decimal Amount,
    PaymentMethod PaymentMethod,
    string? SourceAccount,
    string? BankName,
    string? BankReference,
    string FundedBy
) : IRequest<FundEscrowResponse>;

public record ApproveReleaseCommand(
    Guid EscrowAccountId,
    string ApprovedBy,
    string ApproverType  // "Buyer" or "Seller"
) : IRequest<bool>;

public record ReleaseEscrowCommand(
    Guid EscrowAccountId,
    decimal? Amount,  // null = release all
    string? DestinationAccount,
    string? BankName,
    string ReleasedBy,
    string? Notes
) : IRequest<ReleaseEscrowResponse>;

public record RefundEscrowCommand(
    Guid EscrowAccountId,
    decimal? Amount,  // null = refund all
    string Reason,
    string RefundedBy,
    string? Notes
) : IRequest<RefundEscrowResponse>;

public record CancelEscrowCommand(
    Guid EscrowAccountId,
    string Reason,
    string CancelledBy
) : IRequest<bool>;

public record ExtendEscrowExpirationCommand(
    Guid EscrowAccountId,
    int AdditionalDays,
    string ExtendedBy,
    string? Reason
) : IRequest<bool>;

#endregion

#region Condition Commands

public record AddConditionCommand(
    Guid EscrowAccountId,
    ReleaseConditionType Type,
    string Name,
    string? Description,
    bool IsMandatory,
    int Order,
    bool RequiresEvidence,
    DateTime? DueDate,
    string AddedBy
) : IRequest<Guid>;

public record MarkConditionMetCommand(
    Guid ConditionId,
    string? ActualValue,
    Guid? EvidenceDocumentId,
    string VerifiedBy,
    string? VerificationNotes
) : IRequest<bool>;

public record MarkConditionFailedCommand(
    Guid ConditionId,
    string Reason,
    string MarkedBy
) : IRequest<bool>;

public record WaiveConditionCommand(
    Guid ConditionId,
    string Reason,
    string WaivedBy
) : IRequest<bool>;

#endregion

#region Document Commands

public record UploadDocumentCommand(
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
    bool VisibleToSeller,
    string UploadedBy
) : IRequest<Guid>;

public record VerifyDocumentCommand(
    Guid DocumentId,
    string VerifiedBy
) : IRequest<bool>;

public record DeleteDocumentCommand(
    Guid DocumentId,
    string DeletedBy
) : IRequest<bool>;

#endregion

#region Dispute Commands

public record FileDisputeCommand(
    Guid EscrowAccountId,
    Guid FiledById,
    string FiledByName,
    string FiledByType,
    string Reason,
    string Description,
    decimal? DisputedAmount,
    string? Category
) : IRequest<Guid>;

public record AssignDisputeCommand(
    Guid DisputeId,
    string AssignedTo
) : IRequest<bool>;

public record EscalateDisputeCommand(
    Guid DisputeId,
    string Reason,
    string EscalatedBy
) : IRequest<bool>;

public record ResolveDisputeCommand(
    Guid DisputeId,
    string Resolution,
    string? ResolutionNotes,
    decimal ResolvedBuyerAmount,
    decimal ResolvedSellerAmount,
    string ResolvedBy
) : IRequest<bool>;

public record CloseDisputeCommand(
    Guid DisputeId,
    string ClosedBy
) : IRequest<bool>;

#endregion

#region Fee Configuration Commands

public record CreateFeeConfigurationCommand(
    string Name,
    EscrowTransactionType TransactionType,
    decimal MinAmount,
    decimal MaxAmount,
    decimal FeePercentage,
    decimal MinFee,
    decimal MaxFee,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    string CreatedBy
) : IRequest<Guid>;

public record UpdateFeeConfigurationCommand(
    Guid Id,
    string Name,
    decimal FeePercentage,
    decimal MinFee,
    decimal MaxFee,
    DateTime? EffectiveTo,
    bool IsActive,
    string UpdatedBy
) : IRequest<bool>;

#endregion
