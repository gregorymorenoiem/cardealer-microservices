using FluentAssertions;
using IdempotencyService.Api.Extensions;
using IdempotencyService.Core.Attributes;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using Xunit;

namespace IdempotencyService.Tests;

/// <summary>
/// Tests for IdempotencyHeaderOperationFilter (Swagger header documentation)
/// </summary>
public class IdempotencyHeaderOperationFilterTests
{
    private readonly IdempotencyHeaderOperationFilter _filter;

    public IdempotencyHeaderOperationFilterTests()
    {
        _filter = new IdempotencyHeaderOperationFilter();
    }

    private static OperationFilterContext CreateOperationFilterContext(MethodInfo methodInfo)
    {
        var schemaGenerator = new Mock<ISchemaGenerator>();
        var schemaRepository = new SchemaRepository();

        var apiDescription = new ApiDescription
        {
            ActionDescriptor = new ControllerActionDescriptor
            {
                MethodInfo = methodInfo,
                ControllerTypeInfo = methodInfo.DeclaringType!.GetTypeInfo()
            }
        };

        return new OperationFilterContext(
            apiDescription,
            schemaGenerator.Object,
            schemaRepository,
            methodInfo);
    }

    // =========== Tests for methods without IdempotentAttribute ===========

    [Fact]
    public void Apply_WithoutIdempotentAttribute_DoesNotAddParameter()
    {
        // Arrange
        var operation = new OpenApiOperation { Parameters = null };
        var methodInfo = typeof(TestControllerWithoutAttribute)
            .GetMethod(nameof(TestControllerWithoutAttribute.RegularMethod))!;
        var context = CreateOperationFilterContext(methodInfo);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Parameters.Should().BeNull();
    }

    [Fact]
    public void Apply_WithoutIdempotentAttribute_DoesNotModifyDescription()
    {
        // Arrange
        var originalDescription = "Original description";
        var operation = new OpenApiOperation { Description = originalDescription };
        var methodInfo = typeof(TestControllerWithoutAttribute)
            .GetMethod(nameof(TestControllerWithoutAttribute.RegularMethod))!;
        var context = CreateOperationFilterContext(methodInfo);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Description.Should().Be(originalDescription);
    }

    // =========== Tests for methods with IdempotentAttribute ===========

    [Fact]
    public void Apply_WithIdempotentAttributeRequireKeyTrue_AddsRequiredParameter()
    {
        // Arrange
        var operation = new OpenApiOperation { Parameters = null };
        var methodInfo = typeof(TestControllerWithIdempotent)
            .GetMethod(nameof(TestControllerWithIdempotent.RequiredKeyMethod))!;
        var context = CreateOperationFilterContext(methodInfo);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Parameters.Should().NotBeNull();
        operation.Parameters.Should().HaveCount(1);

        var param = operation.Parameters[0];
        param.Name.Should().Be("X-Idempotency-Key");
        param.In.Should().Be(ParameterLocation.Header);
        param.Required.Should().BeTrue();
        param.Description.Should().Contain("required");
    }

    [Fact]
    public void Apply_WithIdempotentAttributeRequireKeyFalse_AddsOptionalParameter()
    {
        // Arrange
        var operation = new OpenApiOperation { Parameters = null };
        var methodInfo = typeof(TestControllerWithIdempotent)
            .GetMethod(nameof(TestControllerWithIdempotent.OptionalKeyMethod))!;
        var context = CreateOperationFilterContext(methodInfo);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Parameters.Should().NotBeNull();

        var param = operation.Parameters[0];
        param.Required.Should().BeFalse();
        param.Description.Should().Contain("optional");
    }

    [Fact]
    public void Apply_WithIdempotentAttribute_AddsResponseHeader()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                { "200", new OpenApiResponse { Description = "Success" } }
            }
        };
        var methodInfo = typeof(TestControllerWithIdempotent)
            .GetMethod(nameof(TestControllerWithIdempotent.RequiredKeyMethod))!;
        var context = CreateOperationFilterContext(methodInfo);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses["200"].Headers.Should().ContainKey("X-Idempotency-Replayed");
        var header = operation.Responses["200"].Headers["X-Idempotency-Replayed"];
        header.Description.Should().Contain("replayed");
    }

    [Fact]
    public void Apply_WithIdempotentAttributeAndTtl_AddsDescriptionWithTtl()
    {
        // Arrange
        var operation = new OpenApiOperation { Description = "Original" };
        var methodInfo = typeof(TestControllerWithIdempotent)
            .GetMethod(nameof(TestControllerWithIdempotent.MethodWithTtl))!;
        var context = CreateOperationFilterContext(methodInfo);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Description.Should().Contain("3600 seconds");
    }

    [Fact]
    public void Apply_WithIdempotentAttributeRequireKey_UpdatesDescriptionWithLockIcon()
    {
        // Arrange
        var operation = new OpenApiOperation { Description = "Test endpoint" };
        var methodInfo = typeof(TestControllerWithIdempotent)
            .GetMethod(nameof(TestControllerWithIdempotent.RequiredKeyMethod))!;
        var context = CreateOperationFilterContext(methodInfo);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Description.Should().Contain("🔒");
        operation.Description.Should().Contain("requires an idempotency key");
    }

    [Fact]
    public void Apply_WithIdempotentAttributeOptionalKey_UpdatesDescriptionWithUnlockIcon()
    {
        // Arrange
        var operation = new OpenApiOperation { Description = "Test endpoint" };
        var methodInfo = typeof(TestControllerWithIdempotent)
            .GetMethod(nameof(TestControllerWithIdempotent.OptionalKeyMethod))!;
        var context = CreateOperationFilterContext(methodInfo);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Description.Should().Contain("🔓");
        operation.Description.Should().Contain("supports optional idempotency");
    }

    [Fact]
    public void Apply_WithIdempotentAttribute_SetsSchemaToString()
    {
        // Arrange
        var operation = new OpenApiOperation { Parameters = null };
        var methodInfo = typeof(TestControllerWithIdempotent)
            .GetMethod(nameof(TestControllerWithIdempotent.RequiredKeyMethod))!;
        var context = CreateOperationFilterContext(methodInfo);

        // Act
        _filter.Apply(operation, context);

        // Assert
        var param = operation.Parameters![0];
        param.Schema.Should().NotBeNull();
        param.Schema.Type.Should().Be("string");
        param.Schema.Example.Should().NotBeNull();
    }

    // =========== Tests for SkipIdempotencyAttribute ===========

    [Fact]
    public void Apply_WithSkipIdempotencyAttribute_DoesNotAddParameter()
    {
        // Arrange
        var operation = new OpenApiOperation { Parameters = null };
        var methodInfo = typeof(TestControllerWithIdempotent)
            .GetMethod(nameof(TestControllerWithIdempotent.SkippedMethod))!;
        var context = CreateOperationFilterContext(methodInfo);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Parameters.Should().BeNull();
    }

    [Fact]
    public void Apply_WithSkipIdempotencyAttribute_DoesNotModifyResponses()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                { "200", new OpenApiResponse { Headers = new Dictionary<string, OpenApiHeader>() } }
            }
        };
        var methodInfo = typeof(TestControllerWithIdempotent)
            .GetMethod(nameof(TestControllerWithIdempotent.SkippedMethod))!;
        var context = CreateOperationFilterContext(methodInfo);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses["200"].Headers.Should().BeEmpty();
    }

    // =========== Tests for controller-level IdempotentAttribute ===========

    [Fact]
    public void Apply_WithControllerLevelIdempotent_AddsParameter()
    {
        // Arrange
        var operation = new OpenApiOperation { Parameters = null };
        var methodInfo = typeof(IdempotentController)
            .GetMethod(nameof(IdempotentController.MethodWithoutAttribute))!;
        var context = CreateOperationFilterContext(methodInfo);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Parameters.Should().NotBeNull();
        operation.Parameters.Should().HaveCount(1);
    }

    [Fact]
    public void Apply_WithControllerLevelIdempotentButMethodSkipped_DoesNotAddParameter()
    {
        // Arrange
        var operation = new OpenApiOperation { Parameters = null };
        var methodInfo = typeof(IdempotentController)
            .GetMethod(nameof(IdempotentController.SkippedMethod))!;
        var context = CreateOperationFilterContext(methodInfo);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Parameters.Should().BeNull();
    }

    // =========== Tests for custom header name ===========

    [Fact]
    public void Apply_WithCustomHeaderName_UsesCustomName()
    {
        // Arrange
        var operation = new OpenApiOperation { Parameters = null };
        var methodInfo = typeof(TestControllerWithIdempotent)
            .GetMethod(nameof(TestControllerWithIdempotent.CustomHeaderMethod))!;
        var context = CreateOperationFilterContext(methodInfo);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Parameters.Should().NotBeNull();
        operation.Parameters[0].Name.Should().Be("Custom-Idempotency-Key");
    }

    // =========== Tests for null responses ===========

    [Fact]
    public void Apply_WithNullResponses_CreatesResponsesDict()
    {
        // Arrange
        var operation = new OpenApiOperation { Responses = null };
        var methodInfo = typeof(TestControllerWithIdempotent)
            .GetMethod(nameof(TestControllerWithIdempotent.RequiredKeyMethod))!;
        var context = CreateOperationFilterContext(methodInfo);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses.Should().NotBeNull();
    }

    [Fact]
    public void Apply_WithNullResponseHeaders_CreatesHeadersDict()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                { "200", new OpenApiResponse { Headers = null } }
            }
        };
        var methodInfo = typeof(TestControllerWithIdempotent)
            .GetMethod(nameof(TestControllerWithIdempotent.RequiredKeyMethod))!;
        var context = CreateOperationFilterContext(methodInfo);

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Responses["200"].Headers.Should().NotBeNull();
        operation.Responses["200"].Headers.Should().ContainKey("X-Idempotency-Replayed");
    }

    // =========== Test Helper Classes ===========

    private class TestControllerWithoutAttribute
    {
        public void RegularMethod() { }
    }

    private class TestControllerWithIdempotent
    {
        [Idempotent(RequireKey = true)]
        public void RequiredKeyMethod() { }

        [Idempotent(RequireKey = false)]
        public void OptionalKeyMethod() { }

        [Idempotent(RequireKey = true, TtlSeconds = 3600)]
        public void MethodWithTtl() { }

        [SkipIdempotency]
        public void SkippedMethod() { }

        [Idempotent(RequireKey = true, HeaderName = "Custom-Idempotency-Key")]
        public void CustomHeaderMethod() { }
    }

    [Idempotent(RequireKey = true)]
    private class IdempotentController
    {
        public void MethodWithoutAttribute() { }

        [SkipIdempotency]
        public void SkippedMethod() { }
    }
}
