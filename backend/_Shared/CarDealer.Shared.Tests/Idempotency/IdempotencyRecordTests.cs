using CarDealer.Shared.Idempotency.Models;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Idempotency;

public class IdempotencyRecordTests
{
    [Fact]
    public void Defaults_ShouldBeCorrect()
    {
        var record = new IdempotencyRecord();

        record.Key.Should().BeEmpty();
        record.HttpMethod.Should().BeEmpty();
        record.Path.Should().BeEmpty();
        record.RequestHash.Should().BeEmpty();
        record.ResponseStatusCode.Should().Be(0);
        record.ResponseBody.Should().BeEmpty();
        record.ResponseContentType.Should().Be("application/json");
        record.ResponseHeaders.Should().BeEmpty();
        record.Status.Should().Be(IdempotencyStatus.Processing);
        record.ClientId.Should().BeNull();
        record.UserId.Should().BeNull();
        record.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void CreatedAt_ShouldDefaultToUtcNow()
    {
        var before = DateTime.UtcNow;
        var record = new IdempotencyRecord();
        var after = DateTime.UtcNow;

        record.CreatedAt.Should().BeOnOrAfter(before);
        record.CreatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Status_ShouldBeSettableToCompleted()
    {
        var record = new IdempotencyRecord { Status = IdempotencyStatus.Completed };
        record.Status.Should().Be(IdempotencyStatus.Completed);
    }

    [Fact]
    public void Status_ShouldBeSettableToFailed()
    {
        var record = new IdempotencyRecord { Status = IdempotencyStatus.Failed };
        record.Status.Should().Be(IdempotencyStatus.Failed);
    }

    [Fact]
    public void ResponseHeaders_ShouldBeSettable()
    {
        var record = new IdempotencyRecord
        {
            ResponseHeaders = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json",
                ["X-Request-Id"] = "abc123"
            }
        };

        record.ResponseHeaders.Should().HaveCount(2);
        record.ResponseHeaders["X-Request-Id"].Should().Be("abc123");
    }

    [Fact]
    public void Metadata_ShouldBeSettable()
    {
        var record = new IdempotencyRecord
        {
            Metadata = new Dictionary<string, string>
            {
                ["source"] = "api",
                ["version"] = "1.0"
            }
        };

        record.Metadata.Should().HaveCount(2);
    }

    [Fact]
    public void IdempotencyStatus_ShouldHaveThreeValues()
    {
        var values = Enum.GetValues<IdempotencyStatus>();
        values.Should().HaveCount(3);
    }
}
