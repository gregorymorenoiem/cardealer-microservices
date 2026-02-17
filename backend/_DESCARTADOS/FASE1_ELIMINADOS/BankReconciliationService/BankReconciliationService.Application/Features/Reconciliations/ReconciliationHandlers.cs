using BankReconciliationService.Application.Common;
using BankReconciliationService.Application.DTOs;
using BankReconciliationService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BankReconciliationService.Application.Features.Reconciliations;

/// <summary>
/// Command to start a reconciliation
/// </summary>
public record StartReconciliationCommand(
    Guid BankStatementId,
    bool UseAutomaticMatching = true,
    decimal AmountTolerance = 1.0m,
    int DateToleranceDays = 2,
    Guid StartedByUserId = default
) : IRequest<Result<ReconciliationDto>>;

/// <summary>
/// Handler for StartReconciliationCommand
/// </summary>
public class StartReconciliationCommandHandler : IRequestHandler<StartReconciliationCommand, Result<ReconciliationDto>>
{
    private readonly IReconciliationRepository _repository;
    private readonly IBankStatementRepository _statementRepository;
    private readonly IReconciliationEngine _engine;
    private readonly ILogger<StartReconciliationCommandHandler> _logger;

    public StartReconciliationCommandHandler(
        IReconciliationRepository repository,
        IBankStatementRepository statementRepository,
        IReconciliationEngine engine,
        ILogger<StartReconciliationCommandHandler> logger)
    {
        _repository = repository;
        _statementRepository = statementRepository;
        _engine = engine;
        _logger = logger;
    }

    public async Task<Result<ReconciliationDto>> Handle(
        StartReconciliationCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var statement = await _statementRepository.GetByIdAsync(request.BankStatementId, cancellationToken);
            if (statement == null)
                return Result<ReconciliationDto>.Failure("Bank statement not found");

            var settings = new ReconciliationSettings
            {
                UseAutomaticMatching = request.UseAutomaticMatching,
                AmountTolerance = request.AmountTolerance,
                DateToleranceDays = request.DateToleranceDays
            };

            _logger.LogInformation("Starting reconciliation for statement {StatementId}", request.BankStatementId);

            var reconciliation = await _engine.ExecuteReconciliationAsync(
                request.BankStatementId, 
                settings, 
                cancellationToken);

            // Create DTO using record constructor
            var dto = new ReconciliationDto(
                Id: reconciliation.Id,
                BankStatementId: reconciliation.BankStatementId,
                ReconciliationDate: reconciliation.ReconciliationDate,
                PeriodFrom: reconciliation.PeriodFrom,
                PeriodTo: reconciliation.PeriodTo,
                Status: reconciliation.Status.ToString(),
                TotalBankLines: reconciliation.TotalBankLines,
                TotalInternalTransactions: reconciliation.TotalInternalTransactions,
                MatchedCount: reconciliation.MatchedCount,
                UnmatchedBankCount: reconciliation.UnmatchedBankCount,
                UnmatchedInternalCount: reconciliation.UnmatchedInternalCount,
                TotalDifference: reconciliation.TotalDifference,
                BankClosingBalance: reconciliation.BankClosingBalance,
                SystemClosingBalance: reconciliation.SystemClosingBalance,
                BalanceDifference: reconciliation.BalanceDifference,
                IsApproved: reconciliation.IsApproved,
                CompletedAt: reconciliation.CompletedAt
            );

            return Result<ReconciliationDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing reconciliation");
            return Result<ReconciliationDto>.Failure($"Reconciliation error: {ex.Message}");
        }
    }
}
