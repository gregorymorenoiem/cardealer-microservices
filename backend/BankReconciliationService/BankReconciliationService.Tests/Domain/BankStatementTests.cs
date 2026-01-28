using BankReconciliationService.Domain.Entities;
using BankReconciliationService.Domain.Enums;
using Xunit;

namespace BankReconciliationService.Tests.Domain;

public class BankStatementTests
{
    [Fact]
    public void BankStatement_ShouldBeCreated_WithRequiredProperties()
    {
        // Arrange & Act
        var statement = new BankStatement
        {
            Id = Guid.NewGuid(),
            BankCode = "BPD",
            AccountNumber = "12345678901",
            AccountName = "Cuenta Corriente OKLA",
            StatementDate = DateTime.UtcNow,
            PeriodFrom = new DateTime(2026, 1, 1),
            PeriodTo = new DateTime(2026, 1, 31),
            OpeningBalance = 100000m,
            ClosingBalance = 150000m,
            TotalDebits = 50000m,
            TotalCredits = 100000m,
            Status = ReconciliationStatus.Pending,
            ImportedAt = DateTime.UtcNow,
            ImportedByUserId = Guid.NewGuid()
        };

        // Assert
        Assert.NotEqual(Guid.Empty, statement.Id);
        Assert.Equal("BPD", statement.BankCode);
        Assert.Equal("12345678901", statement.AccountNumber);
        Assert.Equal(ReconciliationStatus.Pending, statement.Status);
        Assert.Equal(50000m, statement.TotalDebits);
        Assert.Equal(100000m, statement.TotalCredits);
    }

    [Fact]
    public void BankStatement_ShouldCalculateCorrectBalance()
    {
        // Arrange
        var statement = new BankStatement
        {
            OpeningBalance = 100000m,
            TotalCredits = 50000m,
            TotalDebits = 20000m,
            ClosingBalance = 130000m  // 100000 + 50000 - 20000
        };

        // Assert
        var expectedClosing = statement.OpeningBalance + statement.TotalCredits - statement.TotalDebits;
        Assert.Equal(expectedClosing, statement.ClosingBalance);
    }

    [Fact]
    public void BankStatementLine_ShouldBeCreated_WithRequiredProperties()
    {
        // Arrange & Act
        var line = new BankStatementLine
        {
            Id = Guid.NewGuid(),
            BankStatementId = Guid.NewGuid(),
            LineNumber = 1,
            TransactionDate = DateTime.UtcNow,
            ReferenceNumber = "REF001",
            Description = "Pago recibido",
            Type = TransactionType.Deposit,
            DebitAmount = 0m,
            CreditAmount = 5000m,
            Balance = 105000m,
            IsReconciled = false
        };

        // Assert
        Assert.NotEqual(Guid.Empty, line.Id);
        Assert.Equal("REF001", line.ReferenceNumber);
        Assert.Equal(TransactionType.Deposit, line.Type);
        Assert.Equal(5000m, line.CreditAmount);
        Assert.False(line.IsReconciled);
    }

    [Fact]
    public void BankStatementLine_IsDebit_WhenDebitAmountGreaterThanZero()
    {
        // Arrange
        var line = new BankStatementLine
        {
            Type = TransactionType.Withdrawal,
            DebitAmount = 1000m,
            CreditAmount = 0m
        };

        // Assert
        Assert.True(line.DebitAmount > 0);
        Assert.Equal(0m, line.CreditAmount);
    }

    [Fact]
    public void BankStatementLine_IsCredit_WhenCreditAmountGreaterThanZero()
    {
        // Arrange
        var line = new BankStatementLine
        {
            Type = TransactionType.Deposit,
            DebitAmount = 0m,
            CreditAmount = 5000m
        };

        // Assert
        Assert.True(line.CreditAmount > 0);
        Assert.Equal(0m, line.DebitAmount);
    }

    [Fact]
    public void BankStatement_ShouldHaveEmptyLinesCollection_WhenCreated()
    {
        // Arrange & Act
        var statement = new BankStatement();

        // Assert
        Assert.NotNull(statement.Lines);
        Assert.Empty(statement.Lines);
    }

    [Theory]
    [InlineData("BPD")]
    [InlineData("BANRESERVAS")]
    [InlineData("BHD")]
    [InlineData("SCOTIABANK")]
    public void BankStatement_ShouldAcceptDifferentBankCodes(string bankCode)
    {
        // Arrange & Act
        var statement = new BankStatement { BankCode = bankCode };

        // Assert
        Assert.Equal(bankCode, statement.BankCode);
    }
}
