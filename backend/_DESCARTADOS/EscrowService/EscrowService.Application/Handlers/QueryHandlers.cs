// EscrowService - Query Handlers

namespace EscrowService.Application.Handlers;

using MediatR;
using EscrowService.Application.Queries;
using EscrowService.Application.DTOs;
using EscrowService.Domain.Entities;
using EscrowService.Domain.Interfaces;

#region EscrowAccount Query Handlers

public class GetEscrowAccountByIdHandler : IRequestHandler<GetEscrowAccountByIdQuery, EscrowAccountDto?>
{
    private readonly IEscrowAccountRepository _repository;
    private readonly IReleaseConditionRepository _conditionRepository;

    public GetEscrowAccountByIdHandler(
        IEscrowAccountRepository repository,
        IReleaseConditionRepository conditionRepository)
    {
        _repository = repository;
        _conditionRepository = conditionRepository;
    }

    public async Task<EscrowAccountDto?> Handle(GetEscrowAccountByIdQuery request, CancellationToken ct)
    {
        var account = await _repository.GetByIdAsync(request.Id, ct);
        if (account == null) return null;

        var conditions = await _conditionRepository.GetByEscrowAccountIdAsync(account.Id, ct);
        var conditionsMet = conditions.Count(c => c.Status == ConditionStatus.Met);

        return MapToDto(account, conditions.Count, conditionsMet);
    }

    private EscrowAccountDto MapToDto(EscrowAccount a, int conditionsCount, int conditionsMetCount)
    {
        return new EscrowAccountDto(
            a.Id, a.AccountNumber, a.TransactionType, a.Status,
            a.BuyerId, a.BuyerName, a.BuyerEmail,
            a.SellerId, a.SellerName, a.SellerEmail,
            a.SubjectType, a.SubjectId, a.SubjectDescription, a.ContractId,
            a.TotalAmount, a.FundedAmount, a.ReleasedAmount, a.RefundedAmount,
            a.PendingAmount, a.FeeAmount, a.Currency,
            a.CreatedAt, a.FundedAt, a.ExpiresAt, a.ReleasedAt,
            a.BuyerApproved, a.SellerApproved,
            conditionsCount, conditionsMetCount
        );
    }
}

public class GetEscrowAccountsByBuyerHandler : IRequestHandler<GetEscrowAccountsByBuyerQuery, List<EscrowAccountSummaryDto>>
{
    private readonly IEscrowAccountRepository _repository;

    public GetEscrowAccountsByBuyerHandler(IEscrowAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<EscrowAccountSummaryDto>> Handle(GetEscrowAccountsByBuyerQuery request, CancellationToken ct)
    {
        var accounts = await _repository.GetByBuyerIdAsync(request.BuyerId, ct);
        return accounts.Select(MapToSummary).ToList();
    }

    private EscrowAccountSummaryDto MapToSummary(EscrowAccount a)
    {
        return new EscrowAccountSummaryDto(
            a.Id, a.AccountNumber, a.TransactionType, a.Status,
            a.BuyerName, a.SellerName,
            a.TotalAmount, a.FundedAmount, a.Currency,
            a.CreatedAt, a.ExpiresAt
        );
    }
}

public class GetEscrowAccountsBySellerHandler : IRequestHandler<GetEscrowAccountsBySellerQuery, List<EscrowAccountSummaryDto>>
{
    private readonly IEscrowAccountRepository _repository;

    public GetEscrowAccountsBySellerHandler(IEscrowAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<EscrowAccountSummaryDto>> Handle(GetEscrowAccountsBySellerQuery request, CancellationToken ct)
    {
        var accounts = await _repository.GetBySellerIdAsync(request.SellerId, ct);
        return accounts.Select(a => new EscrowAccountSummaryDto(
            a.Id, a.AccountNumber, a.TransactionType, a.Status,
            a.BuyerName, a.SellerName,
            a.TotalAmount, a.FundedAmount, a.Currency,
            a.CreatedAt, a.ExpiresAt
        )).ToList();
    }
}

public class GetPagedEscrowAccountsHandler : IRequestHandler<GetPagedEscrowAccountsQuery, PagedResult<EscrowAccountSummaryDto>>
{
    private readonly IEscrowAccountRepository _repository;

    public GetPagedEscrowAccountsHandler(IEscrowAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<EscrowAccountSummaryDto>> Handle(GetPagedEscrowAccountsQuery request, CancellationToken ct)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(request.Page, request.PageSize, request.Status, request.TransactionType, ct);
        
        var dtos = items.Select(a => new EscrowAccountSummaryDto(
            a.Id, a.AccountNumber, a.TransactionType, a.Status,
            a.BuyerName, a.SellerName,
            a.TotalAmount, a.FundedAmount, a.Currency,
            a.CreatedAt, a.ExpiresAt
        )).ToList();

        return new PagedResult<EscrowAccountSummaryDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}

public class GetExpiringEscrowAccountsHandler : IRequestHandler<GetExpiringEscrowAccountsQuery, List<EscrowAccountSummaryDto>>
{
    private readonly IEscrowAccountRepository _repository;

    public GetExpiringEscrowAccountsHandler(IEscrowAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<EscrowAccountSummaryDto>> Handle(GetExpiringEscrowAccountsQuery request, CancellationToken ct)
    {
        var expirationDate = DateTime.UtcNow.AddDays(request.DaysAhead);
        var accounts = await _repository.GetExpiringAsync(expirationDate, ct);
        
        return accounts.Select(a => new EscrowAccountSummaryDto(
            a.Id, a.AccountNumber, a.TransactionType, a.Status,
            a.BuyerName, a.SellerName,
            a.TotalAmount, a.FundedAmount, a.Currency,
            a.CreatedAt, a.ExpiresAt
        )).ToList();
    }
}

public class GetPendingReleaseAccountsHandler : IRequestHandler<GetPendingReleaseAccountsQuery, List<EscrowAccountSummaryDto>>
{
    private readonly IEscrowAccountRepository _repository;

    public GetPendingReleaseAccountsHandler(IEscrowAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<EscrowAccountSummaryDto>> Handle(GetPendingReleaseAccountsQuery request, CancellationToken ct)
    {
        var accounts = await _repository.GetPendingReleaseAsync(ct);
        
        return accounts.Select(a => new EscrowAccountSummaryDto(
            a.Id, a.AccountNumber, a.TransactionType, a.Status,
            a.BuyerName, a.SellerName,
            a.TotalAmount, a.FundedAmount, a.Currency,
            a.CreatedAt, a.ExpiresAt
        )).ToList();
    }
}

#endregion

#region Condition Query Handlers

public class GetConditionsByEscrowAccountHandler : IRequestHandler<GetConditionsByEscrowAccountQuery, List<ReleaseConditionDto>>
{
    private readonly IReleaseConditionRepository _repository;

    public GetConditionsByEscrowAccountHandler(IReleaseConditionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ReleaseConditionDto>> Handle(GetConditionsByEscrowAccountQuery request, CancellationToken ct)
    {
        var conditions = await _repository.GetByEscrowAccountIdAsync(request.EscrowAccountId, ct);
        return conditions.Select(c => new ReleaseConditionDto(
            c.Id, c.EscrowAccountId, c.Type, c.Name, c.Description, c.Status,
            c.IsMandatory, c.Order, c.RequiresEvidence, c.DueDate, c.MetAt,
            c.VerifiedBy, c.VerificationNotes
        )).ToList();
    }
}

public class CheckAllConditionsMetHandler : IRequestHandler<CheckAllConditionsMetQuery, bool>
{
    private readonly IReleaseConditionRepository _repository;

    public CheckAllConditionsMetHandler(IReleaseConditionRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(CheckAllConditionsMetQuery request, CancellationToken ct)
    {
        return await _repository.AllMandatoryConditionsMetAsync(request.EscrowAccountId, ct);
    }
}

#endregion

#region Movement Query Handlers

public class GetMovementsByEscrowAccountHandler : IRequestHandler<GetMovementsByEscrowAccountQuery, List<FundMovementDto>>
{
    private readonly IFundMovementRepository _repository;

    public GetMovementsByEscrowAccountHandler(IFundMovementRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<FundMovementDto>> Handle(GetMovementsByEscrowAccountQuery request, CancellationToken ct)
    {
        var movements = await _repository.GetByEscrowAccountIdAsync(request.EscrowAccountId, ct);
        return movements.Select(m => new FundMovementDto(
            m.Id, m.EscrowAccountId, m.TransactionNumber, m.Type, m.PaymentMethod,
            m.Amount, m.FeeAmount, m.Currency, m.SourceAccount, m.DestinationAccount,
            m.BankName, m.BankReference, m.Status,
            m.CreatedAt, m.CompletedAt, m.InitiatedBy, m.Notes
        )).ToList();
    }
}

#endregion

#region Document Query Handlers

public class GetDocumentsByEscrowAccountHandler : IRequestHandler<GetDocumentsByEscrowAccountQuery, List<EscrowDocumentDto>>
{
    private readonly IEscrowDocumentRepository _repository;

    public GetDocumentsByEscrowAccountHandler(IEscrowDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<EscrowDocumentDto>> Handle(GetDocumentsByEscrowAccountQuery request, CancellationToken ct)
    {
        var documents = await _repository.GetByEscrowAccountIdAsync(request.EscrowAccountId, ct);
        return documents.Select(d => new EscrowDocumentDto(
            d.Id, d.EscrowAccountId, d.Name, d.Description, d.DocumentType,
            d.FileName, d.ContentType, d.FileSize,
            d.IsVerified, d.VerifiedAt, d.VerifiedBy,
            d.UploadedAt, d.UploadedBy
        )).ToList();
    }
}

#endregion

#region Dispute Query Handlers

public class GetDisputesByEscrowAccountHandler : IRequestHandler<GetDisputesByEscrowAccountQuery, List<EscrowDisputeDto>>
{
    private readonly IEscrowDisputeRepository _repository;

    public GetDisputesByEscrowAccountHandler(IEscrowDisputeRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<EscrowDisputeDto>> Handle(GetDisputesByEscrowAccountQuery request, CancellationToken ct)
    {
        var disputes = await _repository.GetByEscrowAccountIdAsync(request.EscrowAccountId, ct);
        return disputes.Select(d => new EscrowDisputeDto(
            d.Id, d.EscrowAccountId, d.DisputeNumber, d.Status,
            d.FiledById, d.FiledByName, d.FiledByType,
            d.Reason, d.Description, d.DisputedAmount, d.Category,
            d.Resolution, d.ResolvedBuyerAmount, d.ResolvedSellerAmount,
            d.FiledAt, d.ResolvedAt, d.ResolvedBy, d.AssignedTo
        )).ToList();
    }
}

public class GetPagedDisputesHandler : IRequestHandler<GetPagedDisputesQuery, PagedResult<EscrowDisputeDto>>
{
    private readonly IEscrowDisputeRepository _repository;

    public GetPagedDisputesHandler(IEscrowDisputeRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<EscrowDisputeDto>> Handle(GetPagedDisputesQuery request, CancellationToken ct)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(request.Page, request.PageSize, request.Status, ct);
        
        var dtos = items.Select(d => new EscrowDisputeDto(
            d.Id, d.EscrowAccountId, d.DisputeNumber, d.Status,
            d.FiledById, d.FiledByName, d.FiledByType,
            d.Reason, d.Description, d.DisputedAmount, d.Category,
            d.Resolution, d.ResolvedBuyerAmount, d.ResolvedSellerAmount,
            d.FiledAt, d.ResolvedAt, d.ResolvedBy, d.AssignedTo
        )).ToList();

        return new PagedResult<EscrowDisputeDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}

#endregion

#region Audit Query Handlers

public class GetAuditLogsByEscrowAccountHandler : IRequestHandler<GetAuditLogsByEscrowAccountQuery, List<EscrowAuditLogDto>>
{
    private readonly IEscrowAuditLogRepository _repository;

    public GetAuditLogsByEscrowAccountHandler(IEscrowAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<EscrowAuditLogDto>> Handle(GetAuditLogsByEscrowAccountQuery request, CancellationToken ct)
    {
        var logs = await _repository.GetByEscrowAccountIdAsync(request.EscrowAccountId, ct);
        return logs.Select(l => new EscrowAuditLogDto(
            l.Id, l.EscrowAccountId, l.EventType, l.Description, l.AmountInvolved, l.PerformedBy, l.PerformedAt
        )).ToList();
    }
}

#endregion

#region Statistics Query Handlers

public class GetEscrowStatisticsHandler : IRequestHandler<GetEscrowStatisticsQuery, EscrowStatisticsDto>
{
    private readonly IEscrowAccountRepository _accountRepository;
    private readonly IEscrowDisputeRepository _disputeRepository;

    public GetEscrowStatisticsHandler(
        IEscrowAccountRepository accountRepository,
        IEscrowDisputeRepository disputeRepository)
    {
        _accountRepository = accountRepository;
        _disputeRepository = disputeRepository;
    }

    public async Task<EscrowStatisticsDto> Handle(GetEscrowStatisticsQuery request, CancellationToken ct)
    {
        var pending = await _accountRepository.GetCountByStatusAsync(EscrowStatus.Pending, ct);
        var funded = await _accountRepository.GetCountByStatusAsync(EscrowStatus.Funded, ct);
        var released = await _accountRepository.GetCountByStatusAsync(EscrowStatus.Released, ct);
        var disputed = await _accountRepository.GetCountByStatusAsync(EscrowStatus.Disputed, ct);
        
        var activeDisputes = await _disputeRepository.GetByStatusAsync(EscrowDisputeStatus.UnderReview, ct);
        
        // These would need actual sum queries
        return new EscrowStatisticsDto(
            pending + funded + released + disputed,
            pending,
            funded,
            released,
            disputed,
            0, // Total in escrow - would need sum query
            0, // Total released - would need sum query
            0, // Total fees - would need sum query
            activeDisputes.Count
        );
    }
}

public class CalculateFeeHandler : IRequestHandler<CalculateFeeQuery, decimal>
{
    private readonly IEscrowFeeConfigurationRepository _repository;

    public CalculateFeeHandler(IEscrowFeeConfigurationRepository repository)
    {
        _repository = repository;
    }

    public async Task<decimal> Handle(CalculateFeeQuery request, CancellationToken ct)
    {
        var config = await _repository.GetActiveForAmountAsync(request.TransactionType, request.Amount, ct);
        if (config == null) return 0;

        var calculatedFee = request.Amount * config.FeePercentage / 100;
        return Math.Max(config.MinFee, Math.Min(config.MaxFee, calculatedFee));
    }
}

public class GetFeeConfigurationsHandler : IRequestHandler<GetFeeConfigurationsQuery, List<EscrowFeeConfigurationDto>>
{
    private readonly IEscrowFeeConfigurationRepository _repository;

    public GetFeeConfigurationsHandler(IEscrowFeeConfigurationRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<EscrowFeeConfigurationDto>> Handle(GetFeeConfigurationsQuery request, CancellationToken ct)
    {
        var configs = await _repository.GetAllActiveAsync(ct);
        return configs.Select(c => new EscrowFeeConfigurationDto(
            c.Id, c.Name, c.TransactionType,
            c.MinAmount, c.MaxAmount, c.FeePercentage, c.MinFee, c.MaxFee,
            c.IsActive, c.EffectiveFrom, c.EffectiveTo
        )).ToList();
    }
}

#endregion
