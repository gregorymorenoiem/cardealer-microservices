using FluentAssertions;
using IdempotencyService.Core.Models;

namespace IdempotencyService.Tests;

/// <summary>
/// Unit tests for IdempotencyCheckResult
/// </summary>
public class IdempotencyCheckResultTests
{
    #region Static Factory Methods

    [Fact]
    public void NotFound_ReturnsCorrectState()
    {
        // Act
        var result = IdempotencyCheckResult.NotFound();

        // Assert
        result.Exists.Should().BeFalse();
        result.IsProcessing.Should().BeFalse();
        result.IsCompleted.Should().BeFalse();
        result.Record.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
        result.RequestHashMatches.Should().BeTrue();
    }

    [Fact]
    public void Processing_ReturnsCorrectState()
    {
        // Arrange
        var record = new IdempotencyRecord
        {
            Key = "test-key",
            Status = IdempotencyStatus.Processing
        };

        // Act
        var result = IdempotencyCheckResult.Processing(record);

        // Assert
        result.Exists.Should().BeTrue();
        result.IsProcessing.Should().BeTrue();
        result.IsCompleted.Should().BeFalse();
        result.Record.Should().Be(record);
        result.RequestHashMatches.Should().BeTrue();
    }

    [Fact]
    public void Completed_ReturnsCorrectState()
    {
        // Arrange
        var record = new IdempotencyRecord
        {
            Key = "test-key",
            Status = IdempotencyStatus.Completed,
            ResponseStatusCode = 200,
            ResponseBody = "{\"success\":true}"
        };

        // Act
        var result = IdempotencyCheckResult.Completed(record);

        // Assert
        result.Exists.Should().BeTrue();
        result.IsProcessing.Should().BeFalse();
        result.IsCompleted.Should().BeTrue();
        result.Record.Should().Be(record);
        result.Record!.ResponseStatusCode.Should().Be(200);
    }

    [Fact]
    public void Conflict_ReturnsCorrectState()
    {
        // Arrange
        var record = new IdempotencyRecord
        {
            Key = "test-key",
            RequestHash = "original-hash"
        };
        var message = "Request body differs from the original request";

        // Act
        var result = IdempotencyCheckResult.Conflict(record, message);

        // Assert
        result.Exists.Should().BeTrue();
        result.IsProcessing.Should().BeFalse();
        result.IsCompleted.Should().BeTrue();
        result.Record.Should().Be(record);
        result.RequestHashMatches.Should().BeFalse();
        result.ErrorMessage.Should().Be(message);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void DefaultInstance_HasCorrectDefaults()
    {
        // Act
        var result = new IdempotencyCheckResult();

        // Assert
        result.Exists.Should().BeFalse();
        result.IsProcessing.Should().BeFalse();
        result.IsCompleted.Should().BeFalse();
        result.Record.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
        result.RequestHashMatches.Should().BeTrue(); // Default is true
    }

    [Fact]
    public void CanSetAllProperties()
    {
        // Arrange
        var record = new IdempotencyRecord { Key = "my-key" };

        // Act
        var result = new IdempotencyCheckResult
        {
            Exists = true,
            IsProcessing = true,
            IsCompleted = false,
            Record = record,
            ErrorMessage = "Some error",
            RequestHashMatches = false
        };

        // Assert
        result.Exists.Should().BeTrue();
        result.IsProcessing.Should().BeTrue();
        result.IsCompleted.Should().BeFalse();
        result.Record.Should().Be(record);
        result.ErrorMessage.Should().Be("Some error");
        result.RequestHashMatches.Should().BeFalse();
    }

    #endregion
}

/// <summary>
/// Unit tests for IdempotencyOptions
/// </summary>
public class IdempotencyOptionsTests
{
    [Fact]
    public void DefaultOptions_HaveCorrectValues()
    {
        // Act
        var options = new IdempotencyOptions();

        // Assert
        options.DefaultTtlSeconds.Should().Be(86400); // 24 hours
        options.MinTtlSeconds.Should().Be(60); // 1 minute
        options.MaxTtlSeconds.Should().Be(604800); // 7 days
        options.HeaderName.Should().Be("X-Idempotency-Key");
        options.RequireIdempotencyKey.Should().BeFalse();
        options.KeyPrefix.Should().Be("idempotency:");
        options.ValidateRequestHash.Should().BeTrue();
        options.ProcessingTimeoutSeconds.Should().Be(30);
    }

    [Fact]
    public void SectionName_IsCorrect()
    {
        // Assert
        IdempotencyOptions.SectionName.Should().Be("Idempotency");
    }

    [Fact]
    public void ExcludedPaths_HasDefaultValues()
    {
        // Act
        var options = new IdempotencyOptions();

        // Assert
        options.ExcludedPaths.Should().Contain("/health");
        options.ExcludedPaths.Should().Contain("/swagger");
        options.ExcludedPaths.Should().Contain("/api/idempotency");
    }

    [Fact]
    public void IdempotentMethods_HasDefaultValues()
    {
        // Act
        var options = new IdempotencyOptions();

        // Assert
        options.IdempotentMethods.Should().Contain("POST");
        options.IdempotentMethods.Should().Contain("PUT");
        options.IdempotentMethods.Should().Contain("PATCH");
        options.IdempotentMethods.Should().NotContain("GET");
        options.IdempotentMethods.Should().NotContain("DELETE");
    }

    [Fact]
    public void CanSetCustomValues()
    {
        // Act
        var options = new IdempotencyOptions
        {
            DefaultTtlSeconds = 3600,
            MinTtlSeconds = 30,
            MaxTtlSeconds = 86400,
            HeaderName = "X-Request-Id",
            RequireIdempotencyKey = true,
            KeyPrefix = "idem:",
            ValidateRequestHash = false,
            ProcessingTimeoutSeconds = 60
        };

        // Assert
        options.DefaultTtlSeconds.Should().Be(3600);
        options.MinTtlSeconds.Should().Be(30);
        options.MaxTtlSeconds.Should().Be(86400);
        options.HeaderName.Should().Be("X-Request-Id");
        options.RequireIdempotencyKey.Should().BeTrue();
        options.KeyPrefix.Should().Be("idem:");
        options.ValidateRequestHash.Should().BeFalse();
        options.ProcessingTimeoutSeconds.Should().Be(60);
    }

    [Fact]
    public void ExcludedPaths_CanBeModified()
    {
        // Arrange
        var options = new IdempotencyOptions();

        // Act
        options.ExcludedPaths.Add("/custom-path");
        options.ExcludedPaths.Remove("/swagger");

        // Assert
        options.ExcludedPaths.Should().Contain("/custom-path");
        options.ExcludedPaths.Should().NotContain("/swagger");
    }

    [Fact]
    public void IdempotentMethods_CanBeModified()
    {
        // Arrange
        var options = new IdempotencyOptions();

        // Act
        options.IdempotentMethods.Add("DELETE");
        options.IdempotentMethods.Remove("PATCH");

        // Assert
        options.IdempotentMethods.Should().Contain("DELETE");
        options.IdempotentMethods.Should().NotContain("PATCH");
    }
}

/// <summary>
/// Unit tests for IdempotencyRecord
/// </summary>
public class IdempotencyRecordTests
{
    [Fact]
    public void DefaultRecord_HasCorrectDefaults()
    {
        // Act
        var record = new IdempotencyRecord();

        // Assert
        record.Key.Should().BeEmpty();
        record.HttpMethod.Should().BeEmpty();
        record.Path.Should().BeEmpty();
        record.RequestHash.Should().BeEmpty();
        record.ResponseStatusCode.Should().Be(0);
        record.ResponseBody.Should().BeEmpty();
        record.ResponseContentType.Should().Be("application/json");
        record.Status.Should().Be(IdempotencyStatus.Processing);
        record.ClientId.Should().BeNull();
        record.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void CreatedAt_IsSetToUtcNow()
    {
        // Act
        var before = DateTime.UtcNow;
        var record = new IdempotencyRecord();
        var after = DateTime.UtcNow;

        // Assert
        record.CreatedAt.Should().BeOnOrAfter(before);
        record.CreatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void CanSetAllProperties()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var createdAt = DateTime.UtcNow;

        // Act
        var record = new IdempotencyRecord
        {
            Key = "test-key-123",
            HttpMethod = "POST",
            Path = "/api/orders",
            RequestHash = "abc123hash",
            ResponseStatusCode = 201,
            ResponseBody = "{\"id\":999}",
            ResponseContentType = "application/xml",
            CreatedAt = createdAt,
            ExpiresAt = expiresAt,
            Status = IdempotencyStatus.Completed,
            ClientId = "client-001",
            Metadata = new Dictionary<string, string> { { "version", "1.0" } }
        };

        // Assert
        record.Key.Should().Be("test-key-123");
        record.HttpMethod.Should().Be("POST");
        record.Path.Should().Be("/api/orders");
        record.RequestHash.Should().Be("abc123hash");
        record.ResponseStatusCode.Should().Be(201);
        record.ResponseBody.Should().Be("{\"id\":999}");
        record.ResponseContentType.Should().Be("application/xml");
        record.CreatedAt.Should().Be(createdAt);
        record.ExpiresAt.Should().Be(expiresAt);
        record.Status.Should().Be(IdempotencyStatus.Completed);
        record.ClientId.Should().Be("client-001");
        record.Metadata.Should().ContainKey("version");
    }

    [Fact]
    public void Metadata_CanStoreMultipleValues()
    {
        // Arrange
        var record = new IdempotencyRecord();

        // Act
        record.Metadata["error"] = "Something went wrong";
        record.Metadata["retryCount"] = "3";
        record.Metadata["userId"] = "user-123";

        // Assert
        record.Metadata.Should().HaveCount(3);
        record.Metadata["error"].Should().Be("Something went wrong");
        record.Metadata["retryCount"].Should().Be("3");
        record.Metadata["userId"].Should().Be("user-123");
    }
}

/// <summary>
/// Unit tests for IdempotencyStatus enum
/// </summary>
public class IdempotencyStatusTests
{
    [Fact]
    public void Processing_HasCorrectValue()
    {
        // Assert
        ((int)IdempotencyStatus.Processing).Should().Be(0);
    }

    [Fact]
    public void Completed_HasCorrectValue()
    {
        // Assert
        ((int)IdempotencyStatus.Completed).Should().Be(1);
    }

    [Fact]
    public void Failed_HasCorrectValue()
    {
        // Assert
        ((int)IdempotencyStatus.Failed).Should().Be(2);
    }

    [Fact]
    public void AllValuesAreDefined()
    {
        // Assert
        Enum.GetValues<IdempotencyStatus>().Should().HaveCount(3);
    }
}
