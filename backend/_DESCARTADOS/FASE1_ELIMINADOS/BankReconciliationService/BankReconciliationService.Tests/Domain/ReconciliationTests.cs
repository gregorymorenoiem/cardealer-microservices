using BankReconciliationService.Domain.Entities;
using BankReconciliationService.Domain.Enums;
using Xunit;
using DomainMatchType = BankReconciliationService.Domain.Enums.MatchType;

namespace BankReconciliationService.Tests.Domain;

public class ReconciliationTests
{
    [Fact]
    public void Reconciliation_ShouldBeCreated_WithRequiredProperties()
    {
        // Arrange & Act
        var reconciliation = new Reconciliation
        {
            Id = Guid.NewGuid(),
            BankStatementId = Guid.NewGuid(),
            ReconciliationDate = DateTime.UtcNow,
            PeriodFrom = new DateTime(2026, 1, 1),
            PeriodTo = new DateTime(2026, 1, 31),
            Status = ReconciliationStatus.InProgress,
            PerformedByUserId = Guid.NewGuid(),
            TotalBankLines = 100,
            TotalInternalTransactions = 95,
            MatchedCount = 90,
            UnmatchedBankCount = 10,
            UnmatchedInternalCount = 5
        };

        // Assert
        Assert.NotEqual(Guid.Empty, reconciliation.Id);
        Assert.Equal(ReconciliationStatus.InProgress, reconciliation.Status);
        Assert.Equal(100, reconciliation.TotalBankLines);
        Assert.Equal(90, reconciliation.MatchedCount);
    }

    [Fact]
    public void Reconciliation_ShouldCalculateBalanceDifference()
    {
        // Arrange
        var reconciliation = new Reconciliation
        {
            BankOpeningBalance = 100000m,
            BankClosingBalance = 150000m,
            SystemOpeningBalance = 100000m,
            SystemClosingBalance = 149500m,
            BalanceDifference = 500m  // 150000 - 149500
        };

        // Assert
        var expectedDifference = reconciliation.BankClosingBalance - reconciliation.SystemClosingBalance;
        Assert.Equal(expectedDifference, reconciliation.BalanceDifference);
    }

    [Fact]
    public void Reconciliation_ShouldHaveEmptyCollections_WhenCreated()
    {
        // Arrange & Act
        var reconciliation = new Reconciliation();

        // Assert
        Assert.NotNull(reconciliation.Matches);
        Assert.Empty(reconciliation.Matches);
        Assert.NotNull(reconciliation.Discrepancies);
        Assert.Empty(reconciliation.Discrepancies);
    }

    [Fact]
    public void ReconciliationMatch_ShouldBeCreated_WithRequiredProperties()
    {
        // Arrange & Act
        var match = new ReconciliationMatch
        {
            Id = Guid.NewGuid(),
            ReconciliationId = Guid.NewGuid(),
            BankStatementLineId = Guid.NewGuid(),
            InternalTransactionId = Guid.NewGuid(),
            MatchType = DomainMatchType.Exact,
            MatchConfidence = 0.95m,
            MatchedAt = DateTime.UtcNow,
            MatchedByUserId = Guid.NewGuid(),
            IsManual = false,
            AmountDifference = 0m,
            DaysDifference = 0
        };

        // Assert
        Assert.NotEqual(Guid.Empty, match.Id);
        Assert.Equal(DomainMatchType.Exact, match.MatchType);
        Assert.Equal(0.95m, match.MatchConfidence);
        Assert.False(match.IsManual);
    }

    [Theory]
    [InlineData(DomainMatchType.Exact)]
    [InlineData(DomainMatchType.Partial)]
    [InlineData(DomainMatchType.Manual)]
    public void ReconciliationMatch_ShouldSupportDifferentMatchTypes(DomainMatchType matchType)
    {
        // Arrange & Act
        var match = new ReconciliationMatch { MatchType = matchType };

        // Assert
        Assert.Equal(matchType, match.MatchType);
    }

    [Fact]
    public void ReconciliationDiscrepancy_ShouldBeCreated_WithRequiredProperties()
    {
        // Arrange & Act
        var discrepancy = new ReconciliationDiscrepancy
        {
            Id = Guid.NewGuid(),
            ReconciliationId = Guid.NewGuid(),
            Type = DiscrepancyType.MissingInSystem,
            Description = "Transacción bancaria sin match interno",
            Amount = 5000m,
            Status = DiscrepancyStatus.Pending
        };

        // Assert
        Assert.NotEqual(Guid.Empty, discrepancy.Id);
        Assert.Equal(DiscrepancyType.MissingInSystem, discrepancy.Type);
        Assert.Equal(DiscrepancyStatus.Pending, discrepancy.Status);
        Assert.Equal(5000m, discrepancy.Amount);
    }

    [Theory]
    [InlineData(DiscrepancyStatus.Pending)]
    [InlineData(DiscrepancyStatus.UnderReview)]
    [InlineData(DiscrepancyStatus.Resolved)]
    [InlineData(DiscrepancyStatus.Ignored)]
    public void ReconciliationDiscrepancy_ShouldSupportDifferentStatuses(DiscrepancyStatus status)
    {
        // Arrange & Act
        var discrepancy = new ReconciliationDiscrepancy { Status = status };

        // Assert
        Assert.Equal(status, discrepancy.Status);
    }

    [Fact]
    public void Reconciliation_ApprovalFlow_ShouldWork()
    {
        // Arrange
        var reconciliation = new Reconciliation
        {
            Status = ReconciliationStatus.Completed,
            IsApproved = false
        };

        // Act - Simulate approval
        reconciliation.IsApproved = true;
        reconciliation.ApprovedByUserId = Guid.NewGuid();
        reconciliation.ApprovedAt = DateTime.UtcNow;

        // Assert
        Assert.True(reconciliation.IsApproved);
        Assert.NotNull(reconciliation.ApprovedByUserId);
        Assert.NotNull(reconciliation.ApprovedAt);
    }

    [Fact]
    public void ReconciliationMatch_ManualMatch_ShouldHaveReason()
    {
        // Arrange & Act
        var match = new ReconciliationMatch
        {
            MatchType = DomainMatchType.Manual,
            IsManual = true,
            MatchReason = "Aprobado por supervisor debido a diferencia de fecha"
        };

        // Assert
        Assert.True(match.IsManual);
        Assert.NotNull(match.MatchReason);
        Assert.Equal(DomainMatchType.Manual, match.MatchType);
    }
}
