using FluentAssertions;
using FinanceService.Domain.Entities;
using Xunit;

namespace FinanceService.Tests;

public class TransactionTests
{
    [Fact]
    public void Create_Transaction_Should_Set_Properties_Correctly()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var transactionNumber = "TXN-001";
        var type = TransactionType.Debit;
        var accountId = Guid.NewGuid();
        var amount = 1000m;
        var currency = "MXN";
        var description = "Test transaction";
        var transactionDate = DateTime.UtcNow;
        var createdBy = Guid.NewGuid();

        // Act
        var transaction = new Transaction(
            dealerId, transactionNumber, type, accountId,
            amount, currency, description, transactionDate, createdBy);

        // Assert
        transaction.DealerId.Should().Be(dealerId);
        transaction.TransactionNumber.Should().Be(transactionNumber);
        transaction.Type.Should().Be(type);
        transaction.AccountId.Should().Be(accountId);
        transaction.Amount.Should().Be(amount);
        transaction.Currency.Should().Be(currency);
        transaction.Description.Should().Be(description);
        transaction.Status.Should().Be(TransactionStatus.Pending);
    }

    [Fact]
    public void Post_Transaction_Should_Change_Status_To_Posted()
    {
        // Arrange
        var transaction = CreateTestTransaction();

        // Act
        transaction.Post();

        // Assert
        transaction.Status.Should().Be(TransactionStatus.Posted);
        transaction.PostedDate.Should().NotBeNull();
    }

    [Fact]
    public void Void_Transaction_Should_Change_Status_To_Void()
    {
        // Arrange
        var transaction = CreateTestTransaction();

        // Act
        transaction.Void();

        // Assert
        transaction.Status.Should().Be(TransactionStatus.Void);
    }

    [Fact]
    public void SetTargetAccount_For_Transfer_Should_Set_TargetAccountId()
    {
        // Arrange
        var transaction = new Transaction(
            Guid.NewGuid(), "TXN-001", TransactionType.Transfer,
            Guid.NewGuid(), 1000m, "MXN", "Transfer", DateTime.UtcNow, Guid.NewGuid());
        var targetAccountId = Guid.NewGuid();

        // Act
        transaction.SetTargetAccount(targetAccountId);

        // Assert
        transaction.TargetAccountId.Should().Be(targetAccountId);
    }

    private static Transaction CreateTestTransaction()
    {
        return new Transaction(
            Guid.NewGuid(),
            "TXN-001",
            TransactionType.Debit,
            Guid.NewGuid(),
            1000m,
            "MXN",
            "Test transaction",
            DateTime.UtcNow,
            Guid.NewGuid()
        );
    }
}
