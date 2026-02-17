using BankReconciliationService.Domain.Entities;
using Xunit;

namespace BankReconciliationService.Tests.Domain;

public class BankAccountConfigTests
{
    [Fact]
    public void BankAccountConfig_ShouldBeCreated_WithRequiredProperties()
    {
        // Arrange & Act
        var config = new BankAccountConfig
        {
            Id = Guid.NewGuid(),
            BankCode = "BPD",
            BankName = "Banco Popular Dominicano",
            AccountNumber = "12345678901",
            AccountName = "Cuenta Principal OKLA",
            AccountType = "CHECKING",
            Currency = "DOP",
            IsActive = true,
            UseApiIntegration = true,
            ApiClientId = "client-id",
            ApiBaseUrl = "https://api.bancopopular.com.do",
            ChartOfAccountsCode = "1.1.02.01",
            EnableAutoReconciliation = true,
            AutoMatchThresholdAmount = 1.0m,
            AutoMatchThresholdDays = 2
        };

        // Assert
        Assert.NotEqual(Guid.Empty, config.Id);
        Assert.Equal("BPD", config.BankCode);
        Assert.Equal("Banco Popular Dominicano", config.BankName);
        Assert.True(config.IsActive);
        Assert.True(config.UseApiIntegration);
    }

    [Fact]
    public void BankAccountConfig_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var config = new BankAccountConfig();

        // Assert
        Assert.Equal(string.Empty, config.BankCode);
        Assert.Equal(string.Empty, config.BankName);
        Assert.Equal("DOP", config.Currency);
        Assert.True(config.IsActive);
        Assert.False(config.UseApiIntegration);
        Assert.False(config.EnableAutoReconciliation);
        Assert.Equal(1.0m, config.AutoMatchThresholdAmount);
        Assert.Equal(2, config.AutoMatchThresholdDays);
    }

    [Fact]
    public void BankAccountConfig_ApiConfiguration_ShouldBeConfigurable()
    {
        // Arrange
        var config = new BankAccountConfig
        {
            UseApiIntegration = true,
            ApiClientId = "client-123",
            ApiClientSecretEncrypted = "encrypted-secret",
            ApiBaseUrl = "https://api.bank.com",
            ApiSyncEnabled = true
        };

        // Assert
        Assert.True(config.UseApiIntegration);
        Assert.Equal("client-123", config.ApiClientId);
        Assert.NotNull(config.ApiClientSecretEncrypted);
        Assert.True(config.ApiSyncEnabled);
    }

    [Fact]
    public void BankAccountConfig_AutoReconciliation_ShouldBeConfigurable()
    {
        // Arrange
        var config = new BankAccountConfig
        {
            EnableAutoReconciliation = true,
            AutoMatchThresholdAmount = 0.50m,
            AutoMatchThresholdDays = 1
        };

        // Assert
        Assert.True(config.EnableAutoReconciliation);
        Assert.Equal(0.50m, config.AutoMatchThresholdAmount);
        Assert.Equal(1, config.AutoMatchThresholdDays);
    }

    [Theory]
    [InlineData("BPD", "Banco Popular Dominicano")]
    [InlineData("BANRESERVAS", "Banco de Reservas")]
    [InlineData("BHD", "Banco BHD León")]
    [InlineData("SCOTIABANK", "Scotiabank RD")]
    public void BankAccountConfig_ShouldSupportDifferentBanks(string bankCode, string bankName)
    {
        // Arrange & Act
        var config = new BankAccountConfig 
        { 
            BankCode = bankCode,
            BankName = bankName
        };

        // Assert
        Assert.Equal(bankCode, config.BankCode);
        Assert.Equal(bankName, config.BankName);
    }

    [Fact]
    public void BankAccountConfig_CreatedAt_ShouldBeSetToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;
        var config = new BankAccountConfig();
        var after = DateTime.UtcNow;

        // Assert
        Assert.True(config.CreatedAt >= before && config.CreatedAt <= after);
    }

    [Fact]
    public void BankAccountConfig_UpdatedAt_ShouldBeNullByDefault()
    {
        // Arrange & Act
        var config = new BankAccountConfig();

        // Assert
        Assert.Null(config.UpdatedAt);
    }
}
