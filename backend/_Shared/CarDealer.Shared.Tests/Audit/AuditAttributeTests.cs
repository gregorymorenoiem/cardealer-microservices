using CarDealer.Shared.Audit.Attributes;
using CarDealer.Shared.Audit.Models;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Audit;

public class AuditAttributeTests
{
    [Fact]
    public void DefaultConstructor_ShouldSetDefaults()
    {
        var attr = new AuditAttribute();

        attr.EventType.Should().BeEmpty();
        attr.Action.Should().BeEmpty();
        attr.ResourceType.Should().BeNull();
        attr.Severity.Should().Be(AuditSeverity.Info);
        attr.IncludeRequestBody.Should().BeFalse();
        attr.IncludeResponseBody.Should().BeFalse();
    }

    [Fact]
    public void EventTypeConstructor_ShouldSetEventType()
    {
        var attr = new AuditAttribute("user.login");

        attr.EventType.Should().Be("user.login");
        attr.Action.Should().BeEmpty();
    }

    [Fact]
    public void EventTypeActionConstructor_ShouldSetBoth()
    {
        var attr = new AuditAttribute("user.profile", "update");

        attr.EventType.Should().Be("user.profile");
        attr.Action.Should().Be("update");
    }

    [Fact]
    public void AttributeUsage_ShouldTargetMethodAndClass()
    {
        var usage = typeof(AuditAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .FirstOrDefault() as AttributeUsageAttribute;

        usage.Should().NotBeNull();
        usage!.ValidOn.Should().HaveFlag(AttributeTargets.Method);
        usage.ValidOn.Should().HaveFlag(AttributeTargets.Class);
        usage.AllowMultiple.Should().BeFalse();
    }

    [Fact]
    public void Severity_ShouldBeSettable()
    {
        var attr = new AuditAttribute { Severity = AuditSeverity.Warning };
        attr.Severity.Should().Be(AuditSeverity.Warning);
    }

    [Fact]
    public void IncludeRequestBody_ShouldBeSettable()
    {
        var attr = new AuditAttribute { IncludeRequestBody = true };
        attr.IncludeRequestBody.Should().BeTrue();
    }

    [Fact]
    public void IncludeResponseBody_ShouldBeSettable()
    {
        var attr = new AuditAttribute { IncludeResponseBody = true };
        attr.IncludeResponseBody.Should().BeTrue();
    }

    [Fact]
    public void ResourceType_ShouldBeSettable()
    {
        var attr = new AuditAttribute { ResourceType = "Vehicle" };
        attr.ResourceType.Should().Be("Vehicle");
    }
}
