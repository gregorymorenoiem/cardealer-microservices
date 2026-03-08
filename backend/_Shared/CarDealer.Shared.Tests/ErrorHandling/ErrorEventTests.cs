using CarDealer.Shared.ErrorHandling.Models;
using FluentAssertions;

namespace CarDealer.Shared.Tests.ErrorHandling;

public class ErrorEventTests
{
    [Fact]
    public void ErrorEvent_DefaultValues_ShouldBeCorrect()
    {
        var errorEvent = new ErrorEvent();

        errorEvent.Id.Should().NotBeEmpty();
        errorEvent.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        errorEvent.ServiceName.Should().BeEmpty();
        errorEvent.Environment.Should().BeEmpty();
        errorEvent.Severity.Should().Be(ErrorSeverity.Error);
        errorEvent.Category.Should().Be(ErrorCategory.Unhandled);
        errorEvent.ExceptionType.Should().BeEmpty();
        errorEvent.Message.Should().BeEmpty();
    }

    [Fact]
    public void ErrorEvent_NullableProperties_ShouldBeNull()
    {
        var errorEvent = new ErrorEvent();

        errorEvent.StackTrace.Should().BeNull();
        errorEvent.InnerException.Should().BeNull();
        errorEvent.RequestPath.Should().BeNull();
        errorEvent.RequestMethod.Should().BeNull();
        errorEvent.UserId.Should().BeNull();
        errorEvent.CorrelationId.Should().BeNull();
        errorEvent.TraceId.Should().BeNull();
        errorEvent.SpanId.Should().BeNull();
        errorEvent.ClientIp.Should().BeNull();
        errorEvent.UserAgent.Should().BeNull();
        errorEvent.AdditionalData.Should().BeNull();
    }

    [Fact]
    public void ErrorEvent_SetAllProperties_ShouldRetainValues()
    {
        var id = Guid.NewGuid();
        var timestamp = DateTime.UtcNow.AddMinutes(-5);
        var additionalData = new Dictionary<string, object> { { "key", "value" } };

        var errorEvent = new ErrorEvent
        {
            Id = id,
            Timestamp = timestamp,
            ServiceName = "TestService",
            Environment = "Production",
            Severity = ErrorSeverity.Critical,
            Category = ErrorCategory.Database,
            ExceptionType = "SqlException",
            Message = "Connection timeout",
            StackTrace = "at TestClass.Method()",
            InnerException = "Inner error",
            RequestPath = "/api/test",
            RequestMethod = "POST",
            UserId = "user-123",
            CorrelationId = "corr-456",
            TraceId = "trace-789",
            SpanId = "span-012",
            ClientIp = "192.168.1.1",
            UserAgent = "TestAgent/1.0",
            AdditionalData = additionalData
        };

        errorEvent.Id.Should().Be(id);
        errorEvent.Timestamp.Should().Be(timestamp);
        errorEvent.ServiceName.Should().Be("TestService");
        errorEvent.Environment.Should().Be("Production");
        errorEvent.Severity.Should().Be(ErrorSeverity.Critical);
        errorEvent.Category.Should().Be(ErrorCategory.Database);
        errorEvent.ExceptionType.Should().Be("SqlException");
        errorEvent.Message.Should().Be("Connection timeout");
        errorEvent.StackTrace.Should().Be("at TestClass.Method()");
        errorEvent.InnerException.Should().Be("Inner error");
        errorEvent.RequestPath.Should().Be("/api/test");
        errorEvent.RequestMethod.Should().Be("POST");
        errorEvent.UserId.Should().Be("user-123");
        errorEvent.CorrelationId.Should().Be("corr-456");
        errorEvent.TraceId.Should().Be("trace-789");
        errorEvent.SpanId.Should().Be("span-012");
        errorEvent.ClientIp.Should().Be("192.168.1.1");
        errorEvent.UserAgent.Should().Be("TestAgent/1.0");
        errorEvent.AdditionalData.Should().BeEquivalentTo(additionalData);
    }

    [Fact]
    public void ErrorEvent_TwoInstances_ShouldHaveDifferentIds()
    {
        var event1 = new ErrorEvent();
        var event2 = new ErrorEvent();

        event1.Id.Should().NotBe(event2.Id);
    }

    [Fact]
    public void ErrorSeverity_ShouldHaveExpectedValues()
    {
        Enum.GetValues<ErrorSeverity>().Should().HaveCount(6);
        ((int)ErrorSeverity.Debug).Should().Be(0);
        ((int)ErrorSeverity.Info).Should().Be(1);
        ((int)ErrorSeverity.Warning).Should().Be(2);
        ((int)ErrorSeverity.Error).Should().Be(3);
        ((int)ErrorSeverity.Critical).Should().Be(4);
        ((int)ErrorSeverity.Fatal).Should().Be(5);
    }

    [Fact]
    public void ErrorCategory_ShouldHaveExpectedValues()
    {
        Enum.GetValues<ErrorCategory>().Should().HaveCount(12);
        ((int)ErrorCategory.Unhandled).Should().Be(0);
        ((int)ErrorCategory.Validation).Should().Be(1);
        ((int)ErrorCategory.Authentication).Should().Be(2);
        ((int)ErrorCategory.Authorization).Should().Be(3);
        ((int)ErrorCategory.NotFound).Should().Be(4);
        ((int)ErrorCategory.Conflict).Should().Be(5);
        ((int)ErrorCategory.Business).Should().Be(6);
        ((int)ErrorCategory.Database).Should().Be(7);
        ((int)ErrorCategory.ExternalService).Should().Be(8);
        ((int)ErrorCategory.Timeout).Should().Be(9);
        ((int)ErrorCategory.RateLimit).Should().Be(10);
        ((int)ErrorCategory.Infrastructure).Should().Be(11);
    }
}
