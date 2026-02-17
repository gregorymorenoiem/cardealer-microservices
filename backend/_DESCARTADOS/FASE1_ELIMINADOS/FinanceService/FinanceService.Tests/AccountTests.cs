using FluentAssertions;
using FinanceService.Domain.Entities;
using Xunit;

namespace FinanceService.Tests;

public class AccountTests
{
    [Fact]
    public void Create_Account_Should_Set_Properties_Correctly()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var code = "1001";
        var name = "Bank Account";
        var type = AccountType.Bank;
        var currency = "MXN";
        var initialBalance = 10000m;

        // Act
        var account = new Account(dealerId, code, name, type, currency, initialBalance);

        // Assert
        account.DealerId.Should().Be(dealerId);
        account.Code.Should().Be(code);
        account.Name.Should().Be(name);
        account.Type.Should().Be(type);
        account.Currency.Should().Be(currency);
        account.InitialBalance.Should().Be(initialBalance);
        account.Balance.Should().Be(initialBalance);
        account.IsActive.Should().BeTrue();
        account.IsSystem.Should().BeFalse();
    }

    [Fact]
    public void Update_Account_Should_Change_Name_And_Description()
    {
        // Arrange
        var account = CreateTestAccount();
        var newName = "Updated Name";
        var newDescription = "Updated Description";

        // Act
        account.Update(newName, newDescription);

        // Assert
        account.Name.Should().Be(newName);
        account.Description.Should().Be(newDescription);
        account.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_Account_Should_Set_IsActive_To_False()
    {
        // Arrange
        var account = CreateTestAccount();

        // Act
        account.Deactivate();

        // Assert
        account.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_Account_Should_Set_IsActive_To_True()
    {
        // Arrange
        var account = CreateTestAccount();
        account.Deactivate();

        // Act
        account.Activate();

        // Assert
        account.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateBalance_Should_Add_Amount_To_Balance()
    {
        // Arrange
        var account = CreateTestAccount();
        var amount = 500m;

        // Act
        account.UpdateBalance(amount);

        // Assert
        account.Balance.Should().Be(1500m);
    }

    private static Account CreateTestAccount()
    {
        return new Account(
            Guid.NewGuid(),
            "TEST001",
            "Test Account",
            AccountType.Bank,
            "MXN",
            1000m
        );
    }
}
