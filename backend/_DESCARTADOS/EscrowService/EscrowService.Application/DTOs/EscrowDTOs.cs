// EscrowService - DTOs

namespace EscrowService.Application.DTOs;

using EscrowService.Domain.Entities;

#region EscrowAccount DTOs

public record EscrowAccountDto(
    Guid Id,
    string AccountNumber,
    EscrowTransactionType TransactionType,
    EscrowStatus Status,
    Guid BuyerId,
    string BuyerName,
    string BuyerEmail,
    Guid SellerId,
    string SellerName,
    string SellerEmail,
    string? SubjectType,
    Guid? SubjectId,
    string? SubjectDescription,
    Guid? ContractId,
    decimal TotalAmount,
    decimal FundedAmount,
    decimal ReleasedAmount,
    decimal RefundedAmount,
    decimal PendingAmount,
    decimal FeeAmount,
    string Currency,
    DateTime CreatedAt,
    DateTime? FundedAt,
    DateTime? ExpiresAt,
    DateTime? ReleasedAt,
    bool BuyerApproved,
    bool SellerApproved,
    int ConditionsCount,
    int ConditionsMetCount
);

public record EscrowAccountSummaryDto(
    Guid Id,
    string AccountNumber,
    EscrowTransactionType TransactionType,
    EscrowStatus Status,
    string BuyerName,
    string SellerName,
    decimal TotalAmount,
    decimal FundedAmount,
    string Currency,
    DateTime CreatedAt,
    DateTime? ExpiresAt
);

public record CreateEscrowAccountDto(
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
    List<CreateReleaseConditionDto>? Conditions
);

#endregion

#region ReleaseCondition DTOs

public record ReleaseConditionDto(
    Guid Id,
    Guid EscrowAccountId,
    ReleaseConditionType Type,
    string Name,
    string? Description,
    ConditionStatus Status,
    bool IsMandatory,
    int Order,
    bool RequiresEvidence,
    DateTime? DueDate,
    DateTime? MetAt,
    string? VerifiedBy,
    string? VerificationNotes
);

public record CreateReleaseConditionDto(
    ReleaseConditionType Type,
    string Name,
    string? Description,
    bool IsMandatory,
    int Order,
    bool RequiresEvidence,
    DateTime? DueDate
);

#endregion

#region FundMovement DTOs

public record FundMovementDto(
    Guid Id,
    Guid EscrowAccountId,
    string TransactionNumber,
    FundMovementType Type,
    PaymentMethod PaymentMethod,
    decimal Amount,
    decimal? FeeAmount,
    string Currency,
    string? SourceAccount,
    string? DestinationAccount,
    string? BankName,
    string? BankReference,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string InitiatedBy,
    string? Notes
);

public record CreateFundMovementDto(
    FundMovementType Type,
    PaymentMethod PaymentMethod,
    decimal Amount,
    string? SourceAccount,
    string? DestinationAccount,
    string? BankName,
    string? BankReference,
    Guid? PartyId,
    string? PartyName,
    string? PartyType,
    string? Notes
);

#endregion

#region Document DTOs

public record EscrowDocumentDto(
    Guid Id,
    Guid EscrowAccountId,
    string Name,
    string? Description,
    string DocumentType,
    string FileName,
    string ContentType,
    long FileSize,
    bool IsVerified,
    DateTime? VerifiedAt,
    string? VerifiedBy,
    DateTime UploadedAt,
    string UploadedBy
);

#endregion

#region Dispute DTOs

public record EscrowDisputeDto(
    Guid Id,
    Guid EscrowAccountId,
    string DisputeNumber,
    EscrowDisputeStatus Status,
    Guid FiledById,
    string FiledByName,
    string FiledByType,
    string Reason,
    string Description,
    decimal? DisputedAmount,
    string? Category,
    string? Resolution,
    decimal? ResolvedBuyerAmount,
    decimal? ResolvedSellerAmount,
    DateTime FiledAt,
    DateTime? ResolvedAt,
    string? ResolvedBy,
    string? AssignedTo
);

public record CreateDisputeDto(
    string Reason,
    string Description,
    decimal? DisputedAmount,
    string? Category
);

public record ResolveDisputeDto(
    string Resolution,
    string? ResolutionNotes,
    decimal ResolvedBuyerAmount,
    decimal ResolvedSellerAmount
);

#endregion

#region Audit DTOs

public record EscrowAuditLogDto(
    Guid Id,
    Guid EscrowAccountId,
    EscrowAuditEventType EventType,
    string Description,
    decimal? AmountInvolved,
    string PerformedBy,
    DateTime PerformedAt
);

#endregion

#region Fee Configuration DTOs

public record EscrowFeeConfigurationDto(
    Guid Id,
    string Name,
    EscrowTransactionType TransactionType,
    decimal MinAmount,
    decimal MaxAmount,
    decimal FeePercentage,
    decimal MinFee,
    decimal MaxFee,
    bool IsActive,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo
);

#endregion

#region Response DTOs

public record CreateEscrowResponse(
    Guid EscrowAccountId,
    string AccountNumber,
    decimal TotalAmount,
    decimal FeeAmount,
    DateTime? ExpiresAt
);

// Alias para compatibilidad con controllers
public record CreateEscrowAccountResponse(
    Guid EscrowAccountId,
    string AccountNumber,
    decimal TotalAmount,
    decimal FeeAmount,
    DateTime? ExpiresAt
);

public record FundEscrowResponse(
    bool Success,
    Guid MovementId,
    string TransactionNumber,
    decimal FundedAmount,
    decimal RemainingAmount,
    string Message
);

public record ReleaseEscrowResponse(
    bool Success,
    Guid? MovementId,
    decimal ReleasedAmount,
    string Message
);

public record RefundEscrowResponse(
    bool Success,
    Guid? MovementId,
    decimal RefundedAmount,
    string Message
);

public record AddConditionResponse(
    Guid ConditionId,
    string Name,
    ReleaseConditionType Type,
    bool IsMandatory
);

public record UploadDocumentResponse(
    Guid DocumentId,
    string Name,
    string FileName,
    long FileSize,
    DateTime UploadedAt
);

public record FileDisputeResponse(
    Guid DisputeId,
    string DisputeNumber,
    EscrowDisputeStatus Status,
    DateTime FiledAt
);

public record ResolveDisputeResponse(
    bool Success,
    Guid DisputeId,
    string Resolution,
    decimal BuyerAmount,
    decimal SellerAmount,
    DateTime ResolvedAt
);

public record CreateFeeConfigurationResponse(
    Guid ConfigurationId,
    string Name,
    decimal FeePercentage,
    DateTime EffectiveFrom
);

#endregion

#region Statistics DTOs

public record EscrowStatisticsDto(
    int TotalAccounts,
    int PendingAccounts,
    int FundedAccounts,
    int ReleasedAccounts,
    int DisputedAccounts,
    decimal TotalAmountInEscrow,
    decimal TotalReleasedAmount,
    decimal TotalFeesCollected,
    int ActiveDisputes
);

#endregion

#region Pagination

public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

#endregion
