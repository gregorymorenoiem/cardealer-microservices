using BankReconciliationService.Domain.Entities;
using BankReconciliationService.Domain.Enums;
using BankReconciliationService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using DomainMatchType = BankReconciliationService.Domain.Enums.MatchType;

namespace BankReconciliationService.Infrastructure.Services;

/// <summary>
/// Motor de conciliación automática con ML y reglas
/// </summary>
public class ReconciliationEngine : IReconciliationEngine
{
    private readonly IBankStatementRepository _bankStatementRepo;
    private readonly IInternalTransactionRepository _internalTxRepo;
    private readonly IReconciliationRepository _reconciliationRepo;
    private readonly ILogger<ReconciliationEngine> _logger;

    public ReconciliationEngine(
        IBankStatementRepository bankStatementRepo,
        IInternalTransactionRepository internalTxRepo,
        IReconciliationRepository reconciliationRepo,
        ILogger<ReconciliationEngine> logger)
    {
        _bankStatementRepo = bankStatementRepo;
        _internalTxRepo = internalTxRepo;
        _reconciliationRepo = reconciliationRepo;
        _logger = logger;
    }

    public async Task<Reconciliation> ExecuteReconciliationAsync(
        Guid bankStatementId, 
        ReconciliationSettings settings, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting reconciliation for bank statement {BankStatementId}", bankStatementId);

        var bankStatement = await _bankStatementRepo.GetByIdAsync(bankStatementId, cancellationToken);
        if (bankStatement == null)
            throw new ArgumentException($"Bank statement {bankStatementId} not found");

        // Obtener transacciones internas no conciliadas del mismo período
        var internalTransactions = (await _internalTxRepo.GetUnreconciledAsync(
            bankStatement.PeriodFrom.AddDays(-settings.DateToleranceDays), 
            bankStatement.PeriodTo.AddDays(settings.DateToleranceDays),
            cancellationToken
        )).ToList();

        var reconciliation = new Reconciliation
        {
            Id = Guid.NewGuid(),
            BankStatementId = bankStatementId,
            ReconciliationDate = DateTime.UtcNow,
            PeriodFrom = bankStatement.PeriodFrom,
            PeriodTo = bankStatement.PeriodTo,
            Status = ReconciliationStatus.InProgress,
            TotalBankLines = bankStatement.Lines?.Count ?? 0,
            TotalInternalTransactions = internalTransactions.Count,
            BankOpeningBalance = bankStatement.OpeningBalance,
            BankClosingBalance = bankStatement.ClosingBalance
        };

        var matches = new List<ReconciliationMatch>();
        var discrepancies = new List<ReconciliationDiscrepancy>();
        var bankLines = bankStatement.Lines ?? new List<BankStatementLine>();

        if (settings.UseAutomaticMatching)
        {
            // Fase 1: Matches exactos (referencia + monto + fecha exacta)
            matches.AddRange(FindExactMatches(bankLines, internalTransactions, settings, reconciliation.Id));

            // Fase 2: Matches por monto y fecha cercana
            var unmatchedBankLines = bankLines.Where(l => !matches.Any(m => m.BankStatementLineId == l.Id)).ToList();
            var unmatchedInternalTxs = internalTransactions.Where(t => !matches.Any(m => m.InternalTransactionId == t.Id)).ToList();
            
            matches.AddRange(FindAmountDateMatches(unmatchedBankLines, unmatchedInternalTxs, settings, reconciliation.Id));

            // Fase 3: ML-based matching
            unmatchedBankLines = bankLines.Where(l => !matches.Any(m => m.BankStatementLineId == l.Id)).ToList();
            unmatchedInternalTxs = internalTransactions.Where(t => !matches.Any(m => m.InternalTransactionId == t.Id)).ToList();
            
            matches.AddRange(FindMLMatches(unmatchedBankLines, unmatchedInternalTxs, settings, reconciliation.Id));
        }

        // Detectar discrepancias
        var unmatchedBank = bankLines.Where(l => !matches.Any(m => m.BankStatementLineId == l.Id)).ToList();
        var unmatchedInternal = internalTransactions.Where(t => !matches.Any(m => m.InternalTransactionId == t.Id)).ToList();

        foreach (var line in unmatchedBank)
        {
            discrepancies.Add(new ReconciliationDiscrepancy
            {
                Id = Guid.NewGuid(),
                ReconciliationId = reconciliation.Id,
                Type = DiscrepancyType.MissingInSystem,
                Description = $"Bank transaction without match: {line.Description}",
                Amount = line.CreditAmount > 0 ? line.CreditAmount : line.DebitAmount,
                BankStatementLineId = line.Id,
                Status = DiscrepancyStatus.Pending
            });
        }

        foreach (var tx in unmatchedInternal)
        {
            discrepancies.Add(new ReconciliationDiscrepancy
            {
                Id = Guid.NewGuid(),
                ReconciliationId = reconciliation.Id,
                Type = DiscrepancyType.MissingInBank,
                Description = $"Internal transaction without match: {tx.Description}",
                Amount = tx.Amount,
                InternalTransactionId = tx.Id,
                Status = DiscrepancyStatus.Pending
            });
        }

        // Actualizar estadísticas
        reconciliation.MatchedCount = matches.Count;
        reconciliation.UnmatchedBankCount = unmatchedBank.Count;
        reconciliation.UnmatchedInternalCount = unmatchedInternal.Count;
        reconciliation.TotalDifference = discrepancies.Sum(d => d.Amount);
        reconciliation.Matches = matches;
        reconciliation.Discrepancies = discrepancies;

        // Calcular balance del sistema
        reconciliation.SystemOpeningBalance = 0;
        reconciliation.SystemClosingBalance = internalTransactions.Sum(t => t.Amount);
        reconciliation.BalanceDifference = reconciliation.BankClosingBalance - reconciliation.SystemClosingBalance;

        if (Math.Abs(reconciliation.BalanceDifference) < 0.01m && discrepancies.Count == 0)
        {
            reconciliation.Status = ReconciliationStatus.Completed;
            reconciliation.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            reconciliation.Status = ReconciliationStatus.RequiresReview;
        }

        await _reconciliationRepo.AddAsync(reconciliation, cancellationToken);

        _logger.LogInformation(
            "Reconciliation completed: {Matched} matches, {UnmatchedBank} unmatched bank, {UnmatchedInternal} unmatched internal",
            matches.Count, unmatchedBank.Count, unmatchedInternal.Count);

        return reconciliation;
    }

    public async Task<List<MatchSuggestion>> SuggestMatchesAsync(
        BankStatementLine bankLine, 
        CancellationToken cancellationToken = default)
    {
        var amount = bankLine.CreditAmount > 0 ? bankLine.CreditAmount : bankLine.DebitAmount;
        
        var similarTransactions = await _internalTxRepo.FindPotentialMatchesAsync(
            amount,
            bankLine.TransactionDate, 
            7,
            cancellationToken);

        var suggestions = new List<MatchSuggestion>();

        foreach (var tx in similarTransactions.Take(5))
        {
            var confidence = CalculateMatchConfidence(bankLine, tx);
            var amountDiff = Math.Abs(amount - tx.Amount);
            var daysDiff = Math.Abs((bankLine.TransactionDate - tx.TransactionDate).Days);

            suggestions.Add(new MatchSuggestion
            {
                InternalTransaction = tx,
                ConfidenceScore = confidence,
                MatchType = DetermineMatchType(confidence, amountDiff, daysDiff),
                AmountDifference = amountDiff,
                DaysDifference = daysDiff,
                MatchReason = GenerateMatchReason(bankLine, tx, confidence)
            });
        }

        return suggestions.OrderByDescending(s => s.ConfidenceScore).ToList();
    }

    public async Task<ReconciliationMatch> CreateManualMatchAsync(
        Guid bankLineId, 
        Guid internalTxId, 
        Guid userId, 
        string? reason = null, 
        CancellationToken cancellationToken = default)
    {
        var match = new ReconciliationMatch
        {
            Id = Guid.NewGuid(),
            BankStatementLineId = bankLineId,
            InternalTransactionId = internalTxId,
            MatchType = DomainMatchType.Manual,
            MatchConfidence = 1.0m,
            MatchedAt = DateTime.UtcNow,
            MatchedByUserId = userId,
            IsManual = true,
            MatchReason = reason ?? "Manual match by user"
        };

        return match;
    }

    public async Task UndoMatchAsync(Guid matchId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Undoing match {MatchId} by user {UserId}", matchId, userId);
        await Task.CompletedTask;
    }

    private List<ReconciliationMatch> FindExactMatches(
        List<BankStatementLine> bankLines,
        List<InternalTransaction> internalTxs,
        ReconciliationSettings settings,
        Guid reconciliationId)
    {
        var matches = new List<ReconciliationMatch>();

        foreach (var line in bankLines)
        {
            var amount = line.CreditAmount > 0 ? line.CreditAmount : line.DebitAmount;

            var exactMatch = internalTxs.FirstOrDefault(tx =>
                Math.Abs(tx.Amount - amount) <= settings.AmountTolerance &&
                tx.TransactionDate.Date == line.TransactionDate.Date &&
                !string.IsNullOrEmpty(tx.GatewayTransactionId) &&
                line.ReferenceNumber.Contains(tx.GatewayTransactionId, StringComparison.OrdinalIgnoreCase)
            );

            if (exactMatch != null)
            {
                matches.Add(new ReconciliationMatch
                {
                    Id = Guid.NewGuid(),
                    ReconciliationId = reconciliationId,
                    BankStatementLineId = line.Id,
                    InternalTransactionId = exactMatch.Id,
                    MatchType = DomainMatchType.Exact,
                    MatchConfidence = 1.0m,
                    AmountDifference = Math.Abs(amount - exactMatch.Amount),
                    DaysDifference = 0,
                    IsManual = false,
                    MatchReason = "Exact match: reference, amount, and date",
                    MatchedAt = DateTime.UtcNow
                });
            }
        }

        return matches;
    }

    private List<ReconciliationMatch> FindAmountDateMatches(
        List<BankStatementLine> bankLines,
        List<InternalTransaction> internalTxs,
        ReconciliationSettings settings,
        Guid reconciliationId)
    {
        var matches = new List<ReconciliationMatch>();

        foreach (var line in bankLines)
        {
            var amount = line.CreditAmount > 0 ? line.CreditAmount : line.DebitAmount;

            var amountDateMatch = internalTxs.FirstOrDefault(tx =>
                Math.Abs(tx.Amount - amount) <= settings.AmountTolerance &&
                Math.Abs((tx.TransactionDate - line.TransactionDate).Days) <= settings.DateToleranceDays
            );

            if (amountDateMatch != null)
            {
                var daysDiff = Math.Abs((amountDateMatch.TransactionDate - line.TransactionDate).Days);
                var confidence = 0.9m - (daysDiff * 0.05m);

                matches.Add(new ReconciliationMatch
                {
                    Id = Guid.NewGuid(),
                    ReconciliationId = reconciliationId,
                    BankStatementLineId = line.Id,
                    InternalTransactionId = amountDateMatch.Id,
                    MatchType = DomainMatchType.AmountAndDate,
                    MatchConfidence = confidence,
                    AmountDifference = Math.Abs(amount - amountDateMatch.Amount),
                    DaysDifference = daysDiff,
                    IsManual = false,
                    MatchReason = $"Amount and date match (±{daysDiff} days)",
                    MatchedAt = DateTime.UtcNow
                });
            }
        }

        return matches;
    }

    private List<ReconciliationMatch> FindMLMatches(
        List<BankStatementLine> bankLines,
        List<InternalTransaction> internalTxs,
        ReconciliationSettings settings,
        Guid reconciliationId)
    {
        var matches = new List<ReconciliationMatch>();

        foreach (var line in bankLines)
        {
            var amount = line.CreditAmount > 0 ? line.CreditAmount : line.DebitAmount;

            foreach (var tx in internalTxs)
            {
                var confidence = CalculateMatchConfidence(line, tx);

                if (confidence >= settings.MinimumConfidenceScore)
                {
                    matches.Add(new ReconciliationMatch
                    {
                        Id = Guid.NewGuid(),
                        ReconciliationId = reconciliationId,
                        BankStatementLineId = line.Id,
                        InternalTransactionId = tx.Id,
                        MatchType = DomainMatchType.ML,
                        MatchConfidence = confidence,
                        AmountDifference = Math.Abs(amount - tx.Amount),
                        DaysDifference = Math.Abs((tx.TransactionDate - line.TransactionDate).Days),
                        IsManual = false,
                        MatchReason = $"ML match (confidence: {confidence:P0})",
                        MatchedAt = DateTime.UtcNow
                    });
                    break;
                }
            }
        }

        return matches;
    }

    private decimal CalculateMatchConfidence(BankStatementLine line, InternalTransaction tx)
    {
        decimal score = 0;

        var lineAmount = line.CreditAmount > 0 ? line.CreditAmount : line.DebitAmount;

        // 1. Similitud de monto (40%)
        var amountDiff = Math.Abs(lineAmount - tx.Amount);
        var amountScore = amountDiff == 0 ? 0.4m : Math.Max(0, 0.4m - (amountDiff / lineAmount * 0.4m));
        score += amountScore;

        // 2. Similitud de fecha (30%)
        var daysDiff = Math.Abs((line.TransactionDate - tx.TransactionDate).Days);
        var dateScore = daysDiff == 0 ? 0.3m : Math.Max(0, 0.3m - (daysDiff * 0.05m));
        score += dateScore;

        // 3. Similitud de descripción (20%)
        var descriptionScore = CalculateStringSimilarity(line.Description, tx.Description) * 0.2m;
        score += descriptionScore;

        // 4. Similitud de referencia (10%)
        if (!string.IsNullOrEmpty(tx.GatewayTransactionId) && 
            line.ReferenceNumber.Contains(tx.GatewayTransactionId, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.1m;
        }

        return Math.Min(score, 1.0m);
    }

    private decimal CalculateStringSimilarity(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            return 0;

        s1 = s1.ToLower();
        s2 = s2.ToLower();

        var longer = s1.Length > s2.Length ? s1 : s2;
        var shorter = s1.Length > s2.Length ? s2 : s1;

        if (longer.Length == 0)
            return 1.0m;

        var matchCount = 0;
        foreach (var word in shorter.Split(' '))
        {
            if (longer.Contains(word))
                matchCount++;
        }

        var words = shorter.Split(' ');
        return words.Length > 0 ? (decimal)matchCount / words.Length : 0;
    }

    private DomainMatchType DetermineMatchType(decimal confidence, decimal amountDiff, int daysDiff)
    {
        if (confidence >= 0.95m && amountDiff == 0 && daysDiff == 0)
            return DomainMatchType.Exact;
        if (confidence >= 0.85m)
            return DomainMatchType.AmountAndDate;
        if (confidence >= 0.7m)
            return DomainMatchType.ML;
        return DomainMatchType.Partial;
    }

    private string GenerateMatchReason(BankStatementLine line, InternalTransaction tx, decimal confidence)
    {
        var reasons = new List<string>();

        var lineAmount = line.CreditAmount > 0 ? line.CreditAmount : line.DebitAmount;
        var amountDiff = Math.Abs(lineAmount - tx.Amount);
        var daysDiff = Math.Abs((line.TransactionDate - tx.TransactionDate).Days);

        if (amountDiff == 0)
            reasons.Add("exact amount");
        else if (amountDiff < 1)
            reasons.Add($"amount diff: {amountDiff:C}");

        if (daysDiff == 0)
            reasons.Add("same date");
        else if (daysDiff <= 2)
            reasons.Add($"±{daysDiff} days");

        if (!string.IsNullOrEmpty(tx.GatewayTransactionId) && 
            line.ReferenceNumber.Contains(tx.GatewayTransactionId, StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add("reference match");
        }

        var descSimilarity = CalculateStringSimilarity(line.Description, tx.Description);
        if (descSimilarity > 0.5m)
        {
            reasons.Add($"description {descSimilarity:P0} similar");
        }

        return $"{string.Join(", ", reasons)} (confidence: {confidence:P0})";
    }
}
