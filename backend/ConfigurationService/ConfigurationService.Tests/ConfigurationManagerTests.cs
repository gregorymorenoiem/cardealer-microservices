using ConfigurationService.Application.Interfaces;
using ConfigurationService.Domain.Entities;
using ConfigurationService.Infrastructure.Data;
using ConfigurationService.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationService.Tests;

public class ConfigurationManagerTests : IDisposable
{
    private readonly ConfigurationDbContext _context;
    private readonly IConfigurationManager _manager;

    public ConfigurationManagerTests()
    {
        var options = new DbContextOptionsBuilder<ConfigurationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ConfigurationDbContext(options);
        _manager = new ConfigurationManager(_context);
    }

    [Fact]
    public async Task CreateConfiguration_ShouldCreateSuccessfully()
    {
        // Arrange
        var config = new ConfigurationItem
        {
            Key = "TestKey",
            Value = "TestValue",
            Environment = "Dev",
            CreatedBy = "TestUser"
        };

        // Act
        var result = await _manager.CreateConfigurationAsync(config);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Version.Should().Be(1);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetConfiguration_ShouldReturnConfiguration_WhenExists()
    {
        // Arrange
        var config = new ConfigurationItem
        {
            Key = "GetTestKey",
            Value = "GetTestValue",
            Environment = "Dev",
            CreatedBy = "TestUser"
        };
        await _manager.CreateConfigurationAsync(config);

        // Act
        var result = await _manager.GetConfigurationAsync("GetTestKey", "Dev");

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be("GetTestValue");
    }

    [Fact]
    public async Task UpdateConfiguration_ShouldUpdateAndCreateHistory()
    {
        // Arrange
        var config = new ConfigurationItem
        {
            Key = "UpdateKey",
            Value = "OldValue",
            Environment = "Dev",
            CreatedBy = "TestUser"
        };
        var created = await _manager.CreateConfigurationAsync(config);

        // Act
        var updated = await _manager.UpdateConfigurationAsync(created.Id, "NewValue", "UpdateUser", "Test update");

        // Assert
        updated.Value.Should().Be("NewValue");
        updated.Version.Should().Be(2);
        updated.UpdatedBy.Should().Be("UpdateUser");

        var history = await _manager.GetConfigurationHistoryAsync(created.Id);
        history.Should().HaveCount(1);
        history.First().OldValue.Should().Be("OldValue");
        history.First().NewValue.Should().Be("NewValue");
    }

    [Fact]
    public async Task DeleteConfiguration_ShouldMarkAsInactive()
    {
        // Arrange
        var config = new ConfigurationItem
        {
            Key = "DeleteKey",
            Value = "DeleteValue",
            Environment = "Dev",
            CreatedBy = "TestUser"
        };
        var created = await _manager.CreateConfigurationAsync(config);

        // Act
        await _manager.DeleteConfigurationAsync(created.Id);

        // Assert
        var result = await _manager.GetConfigurationAsync("DeleteKey", "Dev");
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
