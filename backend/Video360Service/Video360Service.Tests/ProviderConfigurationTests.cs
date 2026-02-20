using FluentAssertions;
using Video360Service.Domain.Entities;
using Video360Service.Domain.Enums;
using Xunit;

namespace Video360Service.Tests;

public class ProviderConfigurationTests
{
    [Fact]
    public void ProviderConfiguration_ShouldBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var config = new ProviderConfiguration();
        
        // Assert
        config.Should().NotBeNull();
        config.Id.Should().NotBeEmpty();
        config.IsEnabled.Should().BeTrue();
        config.Priority.Should().Be(0);
        config.DailyLimit.Should().Be(0);
        config.DailyUsageCount.Should().Be(0);
        config.MaxVideoSizeMb.Should().Be(100);
        config.MaxVideoDurationSeconds.Should().Be(120);
        config.TimeoutSeconds.Should().Be(120);
        config.SupportedFormats.Should().Be("mp4,webm,mov,avi");
    }

    [Fact]
    public void ProviderConfiguration_CanSetProvider()
    {
        // Arrange
        var config = new ProviderConfiguration();
        
        // Act
        config.Provider = Video360Provider.ApyHub;
        config.CostPerVideoUsd = 0.009m;
        
        // Assert
        config.Provider.Should().Be(Video360Provider.ApyHub);
        config.CostPerVideoUsd.Should().Be(0.009m);
    }

    [Theory]
    [InlineData(Video360Provider.FfmpegApi, 0.011)]
    [InlineData(Video360Provider.ApyHub, 0.009)]
    [InlineData(Video360Provider.Cloudinary, 0.012)]
    [InlineData(Video360Provider.Imgix, 0.018)]
    [InlineData(Video360Provider.Shotstack, 0.05)]
    public void ProviderConfiguration_ShouldHaveCorrectCostsPerProvider(Video360Provider provider, decimal cost)
    {
        // Arrange & Act
        var config = new ProviderConfiguration
        {
            Provider = provider,
            CostPerVideoUsd = cost
        };
        
        // Assert
        config.Provider.Should().Be(provider);
        config.CostPerVideoUsd.Should().Be(cost);
    }

    [Fact]
    public void ProviderConfiguration_DailyUsageCanBeTracked()
    {
        // Arrange
        var config = new ProviderConfiguration
        {
            Provider = Video360Provider.Cloudinary,
            DailyLimit = 100,
            DailyUsageCount = 0
        };
        
        // Act
        config.DailyUsageCount = 50;
        
        // Assert
        config.DailyUsageCount.Should().Be(50);
        (config.DailyUsageCount < config.DailyLimit).Should().BeTrue();
    }

    [Fact]
    public void ProviderConfiguration_CanBeDisabled()
    {
        // Arrange
        var config = new ProviderConfiguration
        {
            Provider = Video360Provider.Shotstack,
            IsEnabled = true
        };
        
        // Act
        config.IsEnabled = false;
        config.Notes = "Disabled due to high cost";
        
        // Assert
        config.IsEnabled.Should().BeFalse();
        config.Notes.Should().Be("Disabled due to high cost");
    }

    [Fact]
    public void ProviderConfiguration_PriorityCanBeSet()
    {
        // Arrange
        var config1 = new ProviderConfiguration { Provider = Video360Provider.ApyHub, Priority = 100 };
        var config2 = new ProviderConfiguration { Provider = Video360Provider.FfmpegApi, Priority = 90 };
        var config3 = new ProviderConfiguration { Provider = Video360Provider.Cloudinary, Priority = 80 };
        
        // Act
        var orderedConfigs = new[] { config1, config2, config3 }
            .OrderByDescending(c => c.Priority)
            .ToList();
        
        // Assert
        orderedConfigs[0].Provider.Should().Be(Video360Provider.ApyHub);
        orderedConfigs[1].Provider.Should().Be(Video360Provider.FfmpegApi);
        orderedConfigs[2].Provider.Should().Be(Video360Provider.Cloudinary);
    }

    [Fact]
    public void ProviderConfiguration_DailyResetCanBeTracked()
    {
        // Arrange
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var config = new ProviderConfiguration
        {
            Provider = Video360Provider.Imgix,
            DailyUsageCount = 50,
            LastDailyReset = yesterday
        };
        
        // Act
        var needsReset = config.LastDailyReset.Date < DateTime.UtcNow.Date;
        if (needsReset)
        {
            config.DailyUsageCount = 0;
            config.LastDailyReset = DateTime.UtcNow.Date;
        }
        
        // Assert
        config.DailyUsageCount.Should().Be(0);
        config.LastDailyReset.Date.Should().Be(DateTime.UtcNow.Date);
    }
}
