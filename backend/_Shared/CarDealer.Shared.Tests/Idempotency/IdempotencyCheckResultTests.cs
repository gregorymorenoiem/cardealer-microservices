using CarDealer.Shared.Idempotency.Models;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Idempotency;

/// <summary>
/// Tests for IdempotencyCheckResult factory methods.
/// </summary>
public class IdempotencyCheckResultTests
{
    [Fact]
    public void NotFound_SetsAllFieldsCorrectly()
    {
        var result = IdempotencyCheckResult.NotFound();

        result.Exists.Should().BeFalse();
        result.IsProcessing.Should().BeFalse();
        result.IsCompleted.Should().BeFalse();
        result.IsConflict.Should().BeFalse();
        result.Record.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Processing_SetsExistsAndProcessing()
    {
        var record = new IdempotencyRecord { Key = "test-key" };

        var result = IdempotencyCheckResult.Processing(record);

        result.Exists.Should().BeTrue();
        result.IsProcessing.Should().BeTrue();
        result.IsCompleted.Should().BeFalse();
        result.IsConflict.Should().BeFalse();
        result.Record.Should().BeSameAs(record);
    }

    [Fact]
    public void Completed_SetsExistsAndCompleted()
    {
        var record = new IdempotencyRecord { Key = "test-key" };

        var result = IdempotencyCheckResult.Completed(record);

        result.Exists.Should().BeTrue();
        result.IsProcessing.Should().BeFalse();
        result.IsCompleted.Should().BeTrue();
        result.IsConflict.Should().BeFalse();
        result.Record.Should().BeSameAs(record);
    }

    [Fact]
    public void Conflict_SetsExistsAndConflictWithMessage()
    {
        var record = new IdempotencyRecord { Key = "test-key" };

        var result = IdempotencyCheckResult.Conflict(record, "Request hash mismatch");

        result.Exists.Should().BeTrue();
        result.IsProcessing.Should().BeFalse();
        result.IsCompleted.Should().BeFalse();
        result.IsConflict.Should().BeTrue();
        result.Record.Should().BeSameAs(record);
        result.ErrorMessage.Should().Be("Request hash mismatch");
    }
}
