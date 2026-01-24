// EscrowService - Repository Interfaces

namespace EscrowService.Domain.Interfaces;

using EscrowService.Domain.Entities;

public interface IEscrowAccountRepository
{
    Task<EscrowAccount?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<EscrowAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct = default);
    Task<EscrowAccount?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<List<EscrowAccount>> GetByBuyerIdAsync(Guid buyerId, CancellationToken ct = default);
    Task<List<EscrowAccount>> GetBySellerIdAsync(Guid sellerId, CancellationToken ct = default);
    Task<List<EscrowAccount>> GetByStatusAsync(EscrowStatus status, CancellationToken ct = default);
    Task<List<EscrowAccount>> GetExpiringAsync(DateTime before, CancellationToken ct = default);
    Task<List<EscrowAccount>> GetPendingReleaseAsync(CancellationToken ct = default);
    Task<(List<EscrowAccount> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, EscrowStatus? status = null, EscrowTransactionType? type = null, CancellationToken ct = default);
    Task<EscrowAccount> AddAsync(EscrowAccount account, CancellationToken ct = default);
    Task UpdateAsync(EscrowAccount account, CancellationToken ct = default);
    Task<decimal> GetTotalFundedAmountAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<int> GetCountByStatusAsync(EscrowStatus status, CancellationToken ct = default);
}

public interface IReleaseConditionRepository
{
    Task<ReleaseCondition?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ReleaseCondition>> GetByEscrowAccountIdAsync(Guid escrowAccountId, CancellationToken ct = default);
    Task<List<ReleaseCondition>> GetPendingByEscrowAccountIdAsync(Guid escrowAccountId, CancellationToken ct = default);
    Task<bool> AllMandatoryConditionsMetAsync(Guid escrowAccountId, CancellationToken ct = default);
    Task<ReleaseCondition> AddAsync(ReleaseCondition condition, CancellationToken ct = default);
    Task UpdateAsync(ReleaseCondition condition, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IFundMovementRepository
{
    Task<FundMovement?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<FundMovement?> GetByTransactionNumberAsync(string transactionNumber, CancellationToken ct = default);
    Task<List<FundMovement>> GetByEscrowAccountIdAsync(Guid escrowAccountId, CancellationToken ct = default);
    Task<List<FundMovement>> GetByTypeAsync(FundMovementType type, DateTime from, DateTime to, CancellationToken ct = default);
    Task<decimal> GetTotalByTypeAsync(Guid escrowAccountId, FundMovementType type, CancellationToken ct = default);
    Task<FundMovement> AddAsync(FundMovement movement, CancellationToken ct = default);
    Task UpdateAsync(FundMovement movement, CancellationToken ct = default);
}

public interface IEscrowDocumentRepository
{
    Task<EscrowDocument?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<EscrowDocument>> GetByEscrowAccountIdAsync(Guid escrowAccountId, CancellationToken ct = default);
    Task<List<EscrowDocument>> GetByTypeAsync(Guid escrowAccountId, string documentType, CancellationToken ct = default);
    Task<EscrowDocument> AddAsync(EscrowDocument document, CancellationToken ct = default);
    Task UpdateAsync(EscrowDocument document, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IEscrowDisputeRepository
{
    Task<EscrowDispute?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<EscrowDispute?> GetByDisputeNumberAsync(string disputeNumber, CancellationToken ct = default);
    Task<List<EscrowDispute>> GetByEscrowAccountIdAsync(Guid escrowAccountId, CancellationToken ct = default);
    Task<List<EscrowDispute>> GetByStatusAsync(EscrowDisputeStatus status, CancellationToken ct = default);
    Task<List<EscrowDispute>> GetAssignedToAsync(string assignedTo, CancellationToken ct = default);
    Task<(List<EscrowDispute> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, EscrowDisputeStatus? status = null, CancellationToken ct = default);
    Task<EscrowDispute> AddAsync(EscrowDispute dispute, CancellationToken ct = default);
    Task UpdateAsync(EscrowDispute dispute, CancellationToken ct = default);
}

public interface IEscrowAuditLogRepository
{
    Task<List<EscrowAuditLog>> GetByEscrowAccountIdAsync(Guid escrowAccountId, CancellationToken ct = default);
    Task<List<EscrowAuditLog>> GetByEventTypeAsync(EscrowAuditEventType eventType, DateTime from, DateTime to, CancellationToken ct = default);
    Task<EscrowAuditLog> AddAsync(EscrowAuditLog log, CancellationToken ct = default);
}

public interface IEscrowFeeConfigurationRepository
{
    Task<EscrowFeeConfiguration?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<EscrowFeeConfiguration?> GetActiveForAmountAsync(EscrowTransactionType type, decimal amount, CancellationToken ct = default);
    Task<List<EscrowFeeConfiguration>> GetAllActiveAsync(CancellationToken ct = default);
    Task<EscrowFeeConfiguration> AddAsync(EscrowFeeConfiguration config, CancellationToken ct = default);
    Task UpdateAsync(EscrowFeeConfiguration config, CancellationToken ct = default);
}
