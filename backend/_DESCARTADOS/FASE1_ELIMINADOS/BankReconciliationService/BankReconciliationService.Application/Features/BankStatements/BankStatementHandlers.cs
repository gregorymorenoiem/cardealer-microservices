using BankReconciliationService.Application.Common;
using BankReconciliationService.Application.DTOs;
using BankReconciliationService.Domain.Entities;
using BankReconciliationService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BankReconciliationService.Application.Features.BankStatements;

/// <summary>
/// Query to get bank statements
/// </summary>
public record GetBankStatementsQuery(
    Guid BankAccountConfigId,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<Result<List<BankStatementDto>>>;

/// <summary>
/// Handler for GetBankStatementsQuery
/// </summary>
public class GetBankStatementsQueryHandler : IRequestHandler<GetBankStatementsQuery, Result<List<BankStatementDto>>>
{
    private readonly IBankStatementRepository _repository;
    private readonly ILogger<GetBankStatementsQueryHandler> _logger;

    public GetBankStatementsQueryHandler(
        IBankStatementRepository repository,
        ILogger<GetBankStatementsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<BankStatementDto>>> Handle(
        GetBankStatementsQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<BankStatement> statements;

            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                statements = await _repository.GetByDateRangeAsync(
                    request.BankAccountConfigId,
                    request.StartDate.Value,
                    request.EndDate.Value,
                    cancellationToken);
            }
            else
            {
                statements = await _repository.GetByBankAccountAsync(
                    request.BankAccountConfigId,
                    cancellationToken);
            }

            var dtos = statements.Select(s => new BankStatementDto(
                Id: s.Id,
                BankCode: s.BankCode,
                AccountNumber: s.AccountNumber,
                AccountName: s.AccountName,
                StatementDate: s.StatementDate,
                PeriodFrom: s.PeriodFrom,
                PeriodTo: s.PeriodTo,
                OpeningBalance: s.OpeningBalance,
                ClosingBalance: s.ClosingBalance,
                TotalDebits: s.TotalDebits,
                TotalCredits: s.TotalCredits,
                Status: s.Status.ToString(),
                ImportedAt: s.ImportedAt,
                TotalLines: s.Lines?.Count ?? 0,
                ReconciledLines: s.Lines?.Count(l => l.IsReconciled) ?? 0,
                UnreconciledLines: s.Lines?.Count(l => !l.IsReconciled) ?? 0
            )).ToList();

            return Result<List<BankStatementDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching bank statements");
            return Result<List<BankStatementDto>>.Failure($"Error: {ex.Message}");
        }
    }
}

/// <summary>
/// Command to import bank statement from API
/// </summary>
public record ImportBankStatementCommand(
    Guid BankAccountConfigId,
    DateTime StartDate,
    DateTime EndDate,
    Guid ImportedByUserId
) : IRequest<Result<BankStatementDto>>;

/// <summary>
/// Handler for ImportBankStatementCommand
/// </summary>
public class ImportBankStatementCommandHandler : IRequestHandler<ImportBankStatementCommand, Result<BankStatementDto>>
{
    private readonly IBankStatementRepository _repository;
    private readonly IBankAccountConfigRepository _accountRepository;
    private readonly IBankApiServiceFactory _apiFactory;
    private readonly ILogger<ImportBankStatementCommandHandler> _logger;

    public ImportBankStatementCommandHandler(
        IBankStatementRepository repository,
        IBankAccountConfigRepository accountRepository,
        IBankApiServiceFactory apiFactory,
        ILogger<ImportBankStatementCommandHandler> logger)
    {
        _repository = repository;
        _accountRepository = accountRepository;
        _apiFactory = apiFactory;
        _logger = logger;
    }

    public async Task<Result<BankStatementDto>> Handle(
        ImportBankStatementCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(request.BankAccountConfigId, cancellationToken);
            if (account == null)
                return Result<BankStatementDto>.Failure("Bank account configuration not found");

            if (!account.IsActive)
                return Result<BankStatementDto>.Failure("Bank account is not active");

            var apiService = _apiFactory.CreateService(account.BankCode);
            
            _logger.LogInformation("Importing statement from {BankCode} for {StartDate} to {EndDate}",
                account.BankCode, request.StartDate, request.EndDate);

            var statement = await apiService.ImportStatementAsync(
                account, 
                request.StartDate, 
                request.EndDate, 
                cancellationToken);

            statement.ImportedByUserId = request.ImportedByUserId;
            
            await _repository.AddAsync(statement, cancellationToken);

            var dto = new BankStatementDto(
                Id: statement.Id,
                BankCode: statement.BankCode,
                AccountNumber: statement.AccountNumber,
                AccountName: statement.AccountName,
                StatementDate: statement.StatementDate,
                PeriodFrom: statement.PeriodFrom,
                PeriodTo: statement.PeriodTo,
                OpeningBalance: statement.OpeningBalance,
                ClosingBalance: statement.ClosingBalance,
                TotalDebits: statement.TotalDebits,
                TotalCredits: statement.TotalCredits,
                Status: statement.Status.ToString(),
                ImportedAt: statement.ImportedAt,
                TotalLines: statement.Lines?.Count ?? 0,
                ReconciledLines: statement.Lines?.Count(l => l.IsReconciled) ?? 0,
                UnreconciledLines: statement.Lines?.Count(l => !l.IsReconciled) ?? 0
            );

            return Result<BankStatementDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing bank statement");
            return Result<BankStatementDto>.Failure($"Import error: {ex.Message}");
        }
    }
}
