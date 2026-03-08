using CarDealer.Shared.ErrorHandling.Models;
using FluentAssertions;
using System.Text.Json;

namespace CarDealer.Shared.Tests.ErrorHandling;

/// <summary>
/// Tests for ProblemDetails factory methods and JSON serialization.
/// </summary>
public class ProblemDetailsTests
{
    [Fact]
    public void ValidationError_SetsCorrectFields()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["Email"] = new[] { "Email is required" },
            ["Password"] = new[] { "Min 8 characters", "Must contain uppercase" }
        };

        var pd = ProblemDetails.ValidationError(errors, "trace-123", "/api/auth/register");

        pd.Status.Should().Be(400);
        pd.Title.Should().Contain("validation errors");
        pd.ErrorCode.Should().Be("VALIDATION_ERROR");
        pd.TraceId.Should().Be("trace-123");
        pd.Instance.Should().Be("/api/auth/register");
        pd.Errors.Should().HaveCount(2);
        pd.Errors!["Password"].Should().HaveCount(2);
    }

    [Fact]
    public void NotFound_SetsStatus404()
    {
        var pd = ProblemDetails.NotFound("Vehicle not found", "trace-456");

        pd.Status.Should().Be(404);
        pd.ErrorCode.Should().Be("NOT_FOUND");
        pd.Detail.Should().Be("Vehicle not found");
        pd.TraceId.Should().Be("trace-456");
    }

    [Fact]
    public void Unauthorized_SetsStatus401_WithDefaultMessage()
    {
        var pd = ProblemDetails.Unauthorized();

        pd.Status.Should().Be(401);
        pd.ErrorCode.Should().Be("UNAUTHORIZED");
        pd.Detail.Should().Contain("Authentication is required");
    }

    [Fact]
    public void Forbidden_SetsStatus403()
    {
        var pd = ProblemDetails.Forbidden("No access to admin panel");

        pd.Status.Should().Be(403);
        pd.ErrorCode.Should().Be("FORBIDDEN");
        pd.Detail.Should().Be("No access to admin panel");
    }

    [Fact]
    public void Conflict_SetsStatus409()
    {
        var pd = ProblemDetails.Conflict("Email already exists");

        pd.Status.Should().Be(409);
        pd.ErrorCode.Should().Be("CONFLICT");
        pd.Detail.Should().Be("Email already exists");
    }

    [Fact]
    public void InternalServerError_HidesDetailByDefault()
    {
        var pd = ProblemDetails.InternalServerError("NullReferenceException at line 42");

        pd.Status.Should().Be(500);
        pd.ErrorCode.Should().Be("INTERNAL_ERROR");
        pd.Detail.Should().NotContain("NullReference");
        pd.Detail.Should().Contain("An error occurred");
    }

    [Fact]
    public void InternalServerError_IncludesDetail_WhenExplicit()
    {
        var pd = ProblemDetails.InternalServerError("NullRef at line 42", includeDetail: true);

        pd.Detail.Should().Be("NullRef at line 42");
    }

    [Fact]
    public void TooManyRequests_SetsStatus429_WithRetryAfter()
    {
        var pd = ProblemDetails.TooManyRequests(retryAfterSeconds: 120);

        pd.Status.Should().Be(429);
        pd.ErrorCode.Should().Be("RATE_LIMIT_EXCEEDED");
        pd.Detail.Should().Contain("120 seconds");
        pd.Extensions.Should().ContainKey("retryAfter");
        pd.Extensions!["retryAfter"].Should().Be(120);
    }

    [Fact]
    public void ToJson_ProducesValidCamelCaseJson()
    {
        var pd = ProblemDetails.NotFound("test");

        var json = pd.ToJson();
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("status").GetInt32().Should().Be(404);
        doc.RootElement.GetProperty("errorCode").GetString().Should().Be("NOT_FOUND");
        doc.RootElement.GetProperty("title").GetString().Should().Contain("not found");
    }

    [Fact]
    public void ToJson_OmitsNullProperties()
    {
        var pd = ProblemDetails.NotFound("test");

        var json = pd.ToJson();

        json.Should().NotContain("\"errors\"");
        json.Should().NotContain("\"extensions\"");
    }
}
