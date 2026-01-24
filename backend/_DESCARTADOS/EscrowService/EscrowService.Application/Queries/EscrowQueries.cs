// EscrowService - Queries

namespace EscrowService.Application.Queries;

using MediatR;
using EscrowService.Application.DTOs;
using EscrowService.Domain.Entities;

#region EscrowAccount Queries

public record GetEscrowAccountByIdQuery(Guid Id) : IRequest<EscrowAccountDto?>;

public record GetEscrowAccountByNumberQuery(string AccountNumber) : IRequest<EscrowAccountDto?>;

public record GetEscrowAccountWithDetailsQuery(Guid Id) : IRequest<EscrowAccountDto?>;

public record GetEscrowAccountsByBuyerQuery(Guid BuyerId) : IRequest<List<EscrowAccountSummaryDto>>;

public record GetEscrowAccountsBySellerQuery(Guid SellerId) : IRequest<List<EscrowAccountSummaryDto>>;

public record GetEscrowAccountsByStatusQuery(EscrowStatus Status) : IRequest<List<EscrowAccountSummaryDto>>;

public record GetExpiringEscrowAccountsQuery(int DaysAhead) : IRequest<List<EscrowAccountSummaryDto>>;

public record GetPendingReleaseAccountsQuery() : IRequest<List<EscrowAccountSummaryDto>>;

public record GetPagedEscrowAccountsQuery(
    int Page,
    int PageSize,
    EscrowStatus? Status,
    EscrowTransactionType? TransactionType
) : IRequest<PagedResult<EscrowAccountSummaryDto>>;

#endregion

#region Condition Queries

public record GetConditionsByEscrowAccountQuery(Guid EscrowAccountId) : IRequest<List<ReleaseConditionDto>>;

public record GetPendingConditionsQuery(Guid EscrowAccountId) : IRequest<List<ReleaseConditionDto>>;

public record CheckAllConditionsMetQuery(Guid EscrowAccountId) : IRequest<bool>;

#endregion

#region Movement Queries

public record GetMovementsByEscrowAccountQuery(Guid EscrowAccountId) : IRequest<List<FundMovementDto>>;

public record GetMovementByTransactionNumberQuery(string TransactionNumber) : IRequest<FundMovementDto?>;

#endregion

#region Document Queries

public record GetDocumentsByEscrowAccountQuery(Guid EscrowAccountId) : IRequest<List<EscrowDocumentDto>>;

public record GetDocumentByIdQuery(Guid DocumentId) : IRequest<EscrowDocumentDto?>;

#endregion

#region Dispute Queries

public record GetDisputeByIdQuery(Guid DisputeId) : IRequest<EscrowDisputeDto?>;

public record GetDisputesByEscrowAccountQuery(Guid EscrowAccountId) : IRequest<List<EscrowDisputeDto>>;

public record GetDisputesByStatusQuery(EscrowDisputeStatus Status) : IRequest<List<EscrowDisputeDto>>;

public record GetDisputesAssignedToQuery(string AssignedTo) : IRequest<List<EscrowDisputeDto>>;

public record GetPagedDisputesQuery(
    int Page,
    int PageSize,
    EscrowDisputeStatus? Status
) : IRequest<PagedResult<EscrowDisputeDto>>;

#endregion

#region Audit Queries

public record GetAuditLogsByEscrowAccountQuery(Guid EscrowAccountId) : IRequest<List<EscrowAuditLogDto>>;

#endregion

#region Statistics Queries

public record GetEscrowStatisticsQuery() : IRequest<EscrowStatisticsDto>;

public record GetFeeConfigurationsQuery() : IRequest<List<EscrowFeeConfigurationDto>>;

public record CalculateFeeQuery(
    EscrowTransactionType TransactionType,
    decimal Amount
) : IRequest<decimal>;

#endregion
