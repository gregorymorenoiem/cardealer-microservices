using FluentAssertions;
using IdempotencyService.Api.Extensions;

namespace IdempotencyService.Tests;

/// <summary>
/// Unit tests for IdempotencyMiddlewareOptions
/// </summary>
public class IdempotencyMiddlewareOptionsTests
{
    [Fact]
    public void DefaultOptions_HasCorrectDefaults()
    {
        // Act
        var options = new IdempotencyMiddlewareOptions();

        // Assert
        options.UseMiddleware.Should().BeFalse();
        options.ExcludePaths.Should().NotBeEmpty();
    }

    [Fact]
    public void ExcludePaths_HasDefaultValues()
    {
        // Act
        var options = new IdempotencyMiddlewareOptions();

        // Assert
        options.ExcludePaths.Should().Contain("/health");
        options.ExcludePaths.Should().Contain("/swagger");
        options.ExcludePaths.Should().Contain("/metrics");
    }

    [Fact]
    public void CanSetUseMiddleware()
    {
        // Act
        var options = new IdempotencyMiddlewareOptions
        {
            UseMiddleware = true
        };

        // Assert
        options.UseMiddleware.Should().BeTrue();
    }

    [Fact]
    public void CanSetExcludePaths()
    {
        // Act
        var options = new IdempotencyMiddlewareOptions
        {
            ExcludePaths = new List<string> { "/custom", "/path" }
        };

        // Assert
        options.ExcludePaths.Should().HaveCount(2);
        options.ExcludePaths.Should().Contain("/custom");
        options.ExcludePaths.Should().Contain("/path");
    }

    [Fact]
    public void ExcludePaths_CanBeModified()
    {
        // Arrange
        var options = new IdempotencyMiddlewareOptions();

        // Act
        options.ExcludePaths.Add("/api/v2");
        options.ExcludePaths.Remove("/swagger");

        // Assert
        options.ExcludePaths.Should().Contain("/api/v2");
        options.ExcludePaths.Should().NotContain("/swagger");
    }
}
