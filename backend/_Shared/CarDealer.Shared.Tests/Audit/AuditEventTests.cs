using CarDealer.Shared.Audit.Models;
using FluentAssertions;
using System.Text.Json;

namespace CarDealer.Shared.Tests.Audit;

/// <summary>
/// Tests for AuditEvent model: defaults, severity enum, JSON serialization.
/// </summary>
public class AuditEventTests
{
    [Fact]
    public void NewAuditEvent_HasDefaults()
    {
        var evt = new AuditEvent();

        evt.Id.Should().NotBeEmpty();
        evt.EventType.Should().BeEmpty();
        evt.Source.Should().BeEmpty();
        evt.Severity.Should().Be(AuditSeverity.Info);
        evt.Success.Should().BeTrue();
        evt.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        evt.UserId.Should().BeNull();
        evt.Metadata.Should().BeNull();
    }

    [Fact]
    public void AuditEvent_CanSetAllProperties()
    {
        var evt = new AuditEvent
        {
            EventType = "auth.user.login",
            Source = "AuthService",
            UserId = "user-123",
            UserEmail = "user@okla.com",
            ClientIp = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            RequestPath = "/api/auth/login",
            HttpMethod = "POST",
            StatusCode = 200,
            ResourceId = "user-123",
            ResourceType = "User",
            Action = "Login",
            Severity = AuditSeverity.Warning,
            Success = true,
            DurationMs = 42.5,
            CorrelationId = "corr-123",
            TraceId = "trace-456",
            SpanId = "span-789",
            Metadata = new Dictionary<string, object> { ["ip_geo"] = "Santo Domingo" }
        };

        evt.EventType.Should().Be("auth.user.login");
        evt.Severity.Should().Be(AuditSeverity.Warning);
        evt.DurationMs.Should().Be(42.5);
        evt.Metadata.Should().ContainKey("ip_geo");
    }

    [Fact]
    public void AuditSeverity_HasCorrectValues()
    {
        ((int)AuditSeverity.Debug).Should().Be(0);
        ((int)AuditSeverity.Info).Should().Be(1);
        ((int)AuditSeverity.Warning).Should().Be(2);
        ((int)AuditSeverity.Error).Should().Be(3);
        ((int)AuditSeverity.Critical).Should().Be(4);
    }

    [Fact]
    public void AuditEvent_Serializes_SeverityAsString()
    {
        var evt = new AuditEvent { Severity = AuditSeverity.Critical };

        var json = JsonSerializer.Serialize(evt);

        json.Should().Contain("\"Critical\"");
        json.Should().NotContain("\"4\"");
    }

    [Fact]
    public void AuditEvent_Deserializes_SeverityFromString()
    {
        var json = """{"severity":"Warning","eventType":"test","source":"test"}""";

        var evt = JsonSerializer.Deserialize<AuditEvent>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        evt.Should().NotBeNull();
        evt!.Severity.Should().Be(AuditSeverity.Warning);
    }
}
