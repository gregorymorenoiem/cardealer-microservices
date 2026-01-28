using BackgroundRemovalService.Domain.Enums;
using BackgroundRemovalService.Domain.Interfaces;
using BackgroundRemovalService.Infrastructure.Configuration;
using BackgroundRemovalService.Infrastructure.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace BackgroundRemovalService.Tests;

/// <summary>
/// Tests para ClipDropProvider - el proveedor por defecto
/// </summary>
public class ClipDropProviderTests
{
    private readonly Mock<ILogger<ClipDropProvider>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    
    public ClipDropProviderTests()
    {
        _loggerMock = new Mock<ILogger<ClipDropProvider>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
    }
    
    private ClipDropProvider CreateProvider(ClipDropSettings? settings = null, SecretsSettings? secrets = null)
    {
        settings ??= new ClipDropSettings
        {
            BaseUrl = "https://clipdrop-api.co",
            TimeoutSeconds = 60,
            CostPerImageUsd = 0.05m,
            IsDefault = true,
            Priority = 0
        };
        
        secrets ??= new SecretsSettings
        {
            ClipDrop = new ProviderSecrets { ApiKey = "test-api-key-12345678901234567890" }
        };
        
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(settings.BaseUrl)
        };
        
        var optionsMock = new Mock<IOptions<ClipDropSettings>>();
        optionsMock.Setup(o => o.Value).Returns(settings);
        
        var secretsMock = new Mock<IOptions<SecretsSettings>>();
        secretsMock.Setup(o => o.Value).Returns(secrets);
        
        return new ClipDropProvider(httpClient, optionsMock.Object, secretsMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public void ProviderType_ShouldBeClipDrop()
    {
        // Arrange
        var provider = CreateProvider();
        
        // Act & Assert
        Assert.Equal(BackgroundRemovalProvider.ClipDrop, provider.ProviderType);
    }
    
    [Fact]
    public void ProviderName_ShouldContainClipDrop()
    {
        // Arrange
        var provider = CreateProvider();
        
        // Act & Assert
        Assert.Contains("ClipDrop", provider.ProviderName);
    }
    
    [Fact]
    public async Task IsAvailableAsync_WithValidApiKey_ReturnsTrue()
    {
        // Arrange
        var provider = CreateProvider();
        
        // Act
        var result = await provider.IsAvailableAsync();
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task IsAvailableAsync_WithEmptyApiKey_ReturnsFalse()
    {
        // Arrange
        var secrets = new SecretsSettings
        {
            ClipDrop = new ProviderSecrets { ApiKey = "" }
        };
        var provider = CreateProvider(secrets: secrets);
        
        // Act
        var result = await provider.IsAvailableAsync();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task IsAvailableAsync_WithShortApiKey_ReturnsFalse()
    {
        // Arrange
        var secrets = new SecretsSettings
        {
            ClipDrop = new ProviderSecrets { ApiKey = "short" } // Menos de 32 caracteres
        };
        var provider = CreateProvider(secrets: secrets);
        
        // Act
        var result = await provider.IsAvailableAsync();
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task GetAccountInfoAsync_ReturnsBasicInfo()
    {
        // Arrange
        var provider = CreateProvider();
        
        // Act
        var accountInfo = await provider.GetAccountInfoAsync();
        
        // Assert
        Assert.NotNull(accountInfo);
        Assert.True(accountInfo.IsActive);
    }
    
    [Fact]
    public async Task RemoveBackgroundAsync_WithImageTooLarge_ReturnsFailure()
    {
        // Arrange
        var provider = CreateProvider();
        var largeImage = new byte[26 * 1024 * 1024]; // 26MB, excede el límite de 25MB
        var options = new BackgroundRemovalOptions();
        
        // Act
        var result = await provider.RemoveBackgroundAsync(largeImage, options);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("25MB", result.ErrorMessage);
        Assert.Equal("IMAGE_TOO_LARGE", result.ErrorCode);
    }
    
    [Fact]
    public void ClipDropSettings_DefaultValues_AreCorrect()
    {
        // Arrange
        var settings = new ClipDropSettings();
        
        // Assert
        Assert.Equal("https://clipdrop-api.co", settings.BaseUrl);
        Assert.Equal(60, settings.TimeoutSeconds);
        Assert.Equal(0.05m, settings.CostPerImageUsd);
        Assert.True(settings.IsDefault);
        Assert.Equal(0, settings.Priority);
    }
    
    [Fact]
    public void ClipDropSettings_SectionName_IsCorrect()
    {
        // Assert
        Assert.Equal("BackgroundRemoval:Providers:ClipDrop", ClipDropSettings.SectionName);
    }    
    [Fact]
    public void ClipDropProvider_IsDefaultProvider_EnumValueIsZero()
    {
        // El valor 0 del enum significa que es el default cuando se usa default(BackgroundRemovalProvider)
        Assert.Equal(0, (int)BackgroundRemovalProvider.ClipDrop);
    }
}
