using FluentAssertions;
using FluentValidation;
using Xunit;
using ChatbotService.Application.Validators;
using ChatbotService.Application.Features.Sessions.Commands;

namespace ChatbotService.Tests;

/// <summary>
/// Tests for SecurityValidators (NoSqlInjection, NoXss, NoSecurityThreats)
/// and all session command validators.
/// </summary>
public class SecurityValidatorTests
{
    // ── SQL Injection Tests ──────────────────────────────────────────────────

    [Theory]
    [InlineData("SELECT * FROM users")]
    [InlineData("'; DROP TABLE sessions; --")]
    [InlineData("1 OR 1=1")]
    [InlineData("UNION SELECT password FROM users")]
    [InlineData("'; EXEC xp_cmdshell('dir'); --")]
    [InlineData("WAITFOR DELAY '0:0:10'")]
    [InlineData("test/* comment */value")]
    public void SendMessageValidator_ShouldReject_SqlInjection(string maliciousInput)
    {
        var validator = new SendMessageCommandValidator();
        var command = new SendMessageCommand("valid-token", maliciousInput, "UserText", null);
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Message");
    }

    [Theory]
    [InlineData("¿Tienen carros Toyota en venta?")]
    [InlineData("Quiero un carro de $500,000")]
    [InlineData("Buenos días, me interesa el Corolla 2024")]
    [InlineData("¿Cuál es el precio del Honda CR-V?")]
    [InlineData("Necesito financiamiento para un carro nuevo")]
    public void SendMessageValidator_ShouldAccept_NormalMessages(string normalInput)
    {
        var validator = new SendMessageCommandValidator();
        var command = new SendMessageCommand("valid-token", normalInput, "UserText", null);
        var result = validator.Validate(command);
        result.Errors.Where(e => e.PropertyName == "Message").Should().BeEmpty();
    }

    // ── XSS Tests ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("javascript:void(0)")]
    [InlineData("<iframe src='evil.com'></iframe>")]
    [InlineData("data:text/html,<h1>XSS</h1>")]
    [InlineData("<svg onload=alert(1)>")]
    public void SendMessageValidator_ShouldReject_XssAttacks(string xssInput)
    {
        var validator = new SendMessageCommandValidator();
        var command = new SendMessageCommand("valid-token", xssInput, "UserText", null);
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Message");
    }

    // ── SendMessageCommand Validation ────────────────────────────────────────

    [Fact]
    public void SendMessageValidator_ShouldFail_WhenSessionTokenEmpty()
    {
        var validator = new SendMessageCommandValidator();
        var command = new SendMessageCommand("", "Hello", "UserText", null);
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SessionToken");
    }

    [Fact]
    public void SendMessageValidator_ShouldFail_WhenMessageEmpty()
    {
        var validator = new SendMessageCommandValidator();
        var command = new SendMessageCommand("valid-token", "", "UserText", null);
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Message");
    }

    [Fact]
    public void SendMessageValidator_ShouldFail_WhenMessageTooLong()
    {
        var validator = new SendMessageCommandValidator();
        var longMessage = new string('A', 2001);
        var command = new SendMessageCommand("valid-token", longMessage, "UserText", null);
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Message");
    }

    [Fact]
    public void SendMessageValidator_ShouldPass_ValidCommand()
    {
        var validator = new SendMessageCommandValidator();
        var command = new SendMessageCommand(
            "abc123token",
            "Hola, quiero ver los Toyota disponibles",
            "UserText",
            null);
        var result = validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    // ── StartSessionCommand Validation ───────────────────────────────────────

    [Fact]
    public void StartSessionValidator_ShouldFail_WhenSessionTypeEmpty()
    {
        var validator = new StartSessionCommandValidator();
        var command = new StartSessionCommand(
            null, null, null, null,
            "", "web", null, null, null, null, "es", null);
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SessionType");
    }

    [Fact]
    public void StartSessionValidator_ShouldFail_WhenChannelEmpty()
    {
        var validator = new StartSessionCommandValidator();
        var command = new StartSessionCommand(
            null, null, null, null,
            "WebChat", "", null, null, null, null, "es", null);
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Channel");
    }

    [Fact]
    public void StartSessionValidator_ShouldReject_SqlInjectionInEmail()
    {
        var validator = new StartSessionCommandValidator();
        var command = new StartSessionCommand(
            null, "Test User", "admin'; DROP TABLE users;--@test.com", null,
            "WebChat", "web", null, null, null, null, "es", null);
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void StartSessionValidator_ShouldPass_ValidCommand()
    {
        var validator = new StartSessionCommandValidator();
        var command = new StartSessionCommand(
            Guid.NewGuid(), "Juan Pérez", "juan@test.com", "8095551234",
            "WebChat", "web", null, "Mozilla/5.0", "192.168.1.1", "desktop", "es", null);
        var result = validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    // ── EndSessionCommand Validation ─────────────────────────────────────────

    [Fact]
    public void EndSessionValidator_ShouldFail_WhenTokenEmpty()
    {
        var validator = new EndSessionCommandValidator();
        var command = new EndSessionCommand("", null);
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SessionToken");
    }

    [Fact]
    public void EndSessionValidator_ShouldReject_XssInReason()
    {
        var validator = new EndSessionCommandValidator();
        var command = new EndSessionCommand("valid-token", "<script>alert('xss')</script>");
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void EndSessionValidator_ShouldPass_ValidCommand()
    {
        var validator = new EndSessionCommandValidator();
        var command = new EndSessionCommand("valid-token", "User requested end");
        var result = validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    // ── TransferToAgentCommand Validation ────────────────────────────────────

    [Fact]
    public void TransferToAgentValidator_ShouldFail_WhenTokenEmpty()
    {
        var validator = new TransferToAgentCommandValidator();
        var command = new TransferToAgentCommand("", null, null);
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SessionToken");
    }

    [Fact]
    public void TransferToAgentValidator_ShouldPass_ValidCommand()
    {
        var validator = new TransferToAgentCommandValidator();
        var command = new TransferToAgentCommand("valid-token", "Need human help", null);
        var result = validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
