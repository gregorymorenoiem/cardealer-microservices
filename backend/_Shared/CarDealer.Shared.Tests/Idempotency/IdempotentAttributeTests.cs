using CarDealer.Shared.Idempotency.Attributes;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Idempotency;

public class IdempotentAttributeTests
{
    [Fact]
    public void Defaults_ShouldBeCorrect()
    {
        var attr = new IdempotentAttribute();

        attr.RequireKey.Should().BeTrue();
        attr.HeaderName.Should().BeNull();
        attr.TtlSeconds.Should().Be(0);
        attr.IncludeBodyInHash.Should().BeTrue();
        attr.KeyPrefix.Should().BeNull();
    }

    [Fact]
    public void AttributeUsage_ShouldTargetMethodAndClass()
    {
        var usage = typeof(IdempotentAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .FirstOrDefault() as AttributeUsageAttribute;

        usage.Should().NotBeNull();
        usage!.ValidOn.Should().HaveFlag(AttributeTargets.Method);
        usage!.ValidOn.Should().HaveFlag(AttributeTargets.Class);
        usage.AllowMultiple.Should().BeFalse();
        usage.Inherited.Should().BeTrue();
    }

    [Fact]
    public void AllProperties_ShouldBeSettable()
    {
        var attr = new IdempotentAttribute
        {
            RequireKey = false,
            HeaderName = "X-Custom",
            TtlSeconds = 300,
            IncludeBodyInHash = false,
            KeyPrefix = "payment"
        };

        attr.RequireKey.Should().BeFalse();
        attr.HeaderName.Should().Be("X-Custom");
        attr.TtlSeconds.Should().Be(300);
        attr.IncludeBodyInHash.Should().BeFalse();
        attr.KeyPrefix.Should().Be("payment");
    }
}

public class SkipIdempotencyAttributeTests
{
    [Fact]
    public void ShouldBeInstantiable()
    {
        var attr = new SkipIdempotencyAttribute();
        attr.Should().NotBeNull();
    }

    [Fact]
    public void AttributeUsage_ShouldTargetMethodAndClass()
    {
        var usage = typeof(SkipIdempotencyAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .FirstOrDefault() as AttributeUsageAttribute;

        usage.Should().NotBeNull();
        usage!.ValidOn.Should().HaveFlag(AttributeTargets.Method);
        usage!.ValidOn.Should().HaveFlag(AttributeTargets.Class);
        usage.AllowMultiple.Should().BeFalse();
        usage.Inherited.Should().BeTrue();
    }
}
