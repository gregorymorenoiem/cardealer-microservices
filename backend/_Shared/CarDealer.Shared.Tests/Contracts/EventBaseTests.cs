using CarDealer.Contracts.Abstractions;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Contracts;

/// <summary>
/// Concrete test implementation of EventBase.
/// </summary>
public class TestEvent : EventBase
{
    public override string EventType => "test.entity.action";
    public string? TestPayload { get; set; }
}

public class AnotherTestEvent : EventBase
{
    public override string EventType => "test.entity.other";
}

public class EventBaseTests
{
    [Fact]
    public void EventId_ShouldBeUniquePerInstance()
    {
        var event1 = new TestEvent();
        var event2 = new TestEvent();

        event1.EventId.Should().NotBe(Guid.Empty);
        event2.EventId.Should().NotBe(Guid.Empty);
        event1.EventId.Should().NotBe(event2.EventId);
    }

    [Fact]
    public void OccurredAt_ShouldBeCloseToUtcNow()
    {
        var before = DateTime.UtcNow;
        var evt = new TestEvent();
        var after = DateTime.UtcNow;

        evt.OccurredAt.Should().BeOnOrAfter(before);
        evt.OccurredAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void SchemaVersion_ShouldDefaultToOne()
    {
        var evt = new TestEvent();
        evt.SchemaVersion.Should().Be(1);
    }

    [Fact]
    public void SchemaVersion_ShouldBeSettable()
    {
        var evt = new TestEvent { SchemaVersion = 3 };
        evt.SchemaVersion.Should().Be(3);
    }

    [Fact]
    public void CorrelationId_ShouldDefaultToNull()
    {
        var evt = new TestEvent();
        evt.CorrelationId.Should().BeNull();
    }

    [Fact]
    public void CorrelationId_ShouldBeSettable()
    {
        var correlationId = Guid.NewGuid().ToString();
        var evt = new TestEvent { CorrelationId = correlationId };
        evt.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public void EventType_ShouldReturnConcreteImplementation()
    {
        var evt = new TestEvent();
        evt.EventType.Should().Be("test.entity.action");
    }

    [Fact]
    public void EventType_ShouldDifferBySubclass()
    {
        var evt1 = new TestEvent();
        var evt2 = new AnotherTestEvent();

        evt1.EventType.Should().NotBe(evt2.EventType);
    }

    [Fact]
    public void IEvent_Interface_ShouldBeImplemented()
    {
        var evt = new TestEvent();
        evt.Should().BeAssignableTo<IEvent>();
    }

    [Fact]
    public void EventId_ShouldBeSettable()
    {
        var id = Guid.NewGuid();
        var evt = new TestEvent { EventId = id };
        evt.EventId.Should().Be(id);
    }

    [Fact]
    public void OccurredAt_ShouldBeSettable()
    {
        var date = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var evt = new TestEvent { OccurredAt = date };
        evt.OccurredAt.Should().Be(date);
    }

    [Fact]
    public void TestPayload_ShouldBeSettableOnConcrete()
    {
        var evt = new TestEvent { TestPayload = "hello" };
        evt.TestPayload.Should().Be("hello");
    }
}
