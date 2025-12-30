using System.Text;
using FluentAssertions;
using IdempotencyService.Api.Filters;
using IdempotencyService.Core.Attributes;
using IdempotencyService.Core.Interfaces;
using IdempotencyService.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace IdempotencyService.Tests;

public class IdempotencyActionFilterTests
{
    private readonly Mock<IIdempotencyService> _serviceMock;
    private readonly Mock<ILogger<IdempotencyActionFilter>> _loggerMock;
    private readonly IdempotencyOptions _options;
    private readonly IdempotencyActionFilter _filter;

    public IdempotencyActionFilterTests()
    {
        _serviceMock = new Mock<IIdempotencyService>();
        _loggerMock = new Mock<ILogger<IdempotencyActionFilter>>();
        _options = new IdempotencyOptions
        {
            HeaderName = "X-Idempotency-Key",
            DefaultTtlSeconds = 3600
        };
        _filter = new IdempotencyActionFilter(
            _serviceMock.Object,
            _loggerMock.Object,
            Options.Create(_options));
    }

    [Fact]
    public async Task OnActionExecutionAsync_WithSkipAttribute_ExecutesActionWithoutCheck()
    {
        // Arrange
        var context = CreateContext(new SkipIdempotencyAttribute());
        var nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult(CreateExecutedContext(context, new OkResult()));
        };

        // Act
        await _filter.OnActionExecutionAsync(context, next);

        // Assert
        nextCalled.Should().BeTrue();
        _serviceMock.Verify(x => x.CheckAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WithoutIdempotentAttribute_ExecutesActionWithoutCheck()
    {
        // Arrange
        var context = CreateContext();
        var nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult(CreateExecutedContext(context, new OkResult()));
        };

        // Act
        await _filter.OnActionExecutionAsync(context, next);

        // Assert
        nextCalled.Should().BeTrue();
        _serviceMock.Verify(x => x.CheckAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WithIdempotentAttributeAndNoKey_ReturnsBadRequest()
    {
        // Arrange
        var context = CreateContext(new IdempotentAttribute { RequireKey = true });

        // Act
        await _filter.OnActionExecutionAsync(context, () => throw new Exception("Should not be called"));

        // Assert
        context.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = (BadRequestObjectResult)context.Result!;
        badRequest.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task OnActionExecutionAsync_WithIdempotentAttributeAndKeyNotRequired_ExecutesAction()
    {
        // Arrange
        var context = CreateContext(new IdempotentAttribute { RequireKey = false });
        var nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult(CreateExecutedContext(context, new OkResult()));
        };

        // Act
        await _filter.OnActionExecutionAsync(context, next);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task OnActionExecutionAsync_WithExistingCompletedRecord_ReturnsCachedResponse()
    {
        // Arrange
        var context = CreateContext(new IdempotentAttribute());
        context.HttpContext.Request.Headers["X-Idempotency-Key"] = "test-key";

        var cachedRecord = new IdempotencyRecord
        {
            Key = "test-key",
            Status = IdempotencyStatus.Completed,
            ResponseStatusCode = 201,
            ResponseBody = "{\"id\":123}",
            ResponseContentType = "application/json"
        };

        _serviceMock.Setup(x => x.CheckAsync("test-key", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdempotencyCheckResult.Completed(cachedRecord));

        // Act
        await _filter.OnActionExecutionAsync(context, () => throw new Exception("Should not execute action"));

        // Assert
        context.Result.Should().BeOfType<ObjectResult>();
        var result = (ObjectResult)context.Result!;
        result.StatusCode.Should().Be(201);
        context.HttpContext.Response.Headers["X-Idempotency-Replayed"].ToString().Should().Be("true");
    }

    [Fact]
    public async Task OnActionExecutionAsync_WithProcessingRecord_ReturnsConflict()
    {
        // Arrange
        var context = CreateContext(new IdempotentAttribute());
        context.HttpContext.Request.Headers["X-Idempotency-Key"] = "test-key";

        var processingRecord = new IdempotencyRecord
        {
            Key = "test-key",
            Status = IdempotencyStatus.Processing
        };

        _serviceMock.Setup(x => x.CheckAsync("test-key", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdempotencyCheckResult.Processing(processingRecord));

        // Act
        await _filter.OnActionExecutionAsync(context, () => throw new Exception("Should not execute"));

        // Assert
        context.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task OnActionExecutionAsync_WithConflictingRequestHash_ReturnsConflict()
    {
        // Arrange
        var context = CreateContext(new IdempotentAttribute());
        context.HttpContext.Request.Headers["X-Idempotency-Key"] = "test-key";

        var conflictRecord = new IdempotencyRecord
        {
            Key = "test-key",
            Status = IdempotencyStatus.Completed,
            RequestHash = "different-hash"
        };

        _serviceMock.Setup(x => x.CheckAsync("test-key", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdempotencyCheckResult.Conflict(conflictRecord, "Request body differs"));

        // Act
        await _filter.OnActionExecutionAsync(context, () => throw new Exception("Should not execute"));

        // Assert
        context.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task OnActionExecutionAsync_WithNewKey_ExecutesActionAndStoresResult()
    {
        // Arrange
        var context = CreateContext(new IdempotentAttribute());
        context.HttpContext.Request.Headers["X-Idempotency-Key"] = "new-key";

        _serviceMock.Setup(x => x.CheckAsync("new-key", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdempotencyCheckResult.NotFound());

        _serviceMock.Setup(x => x.StartProcessingAsync(It.IsAny<IdempotencyRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _serviceMock.Setup(x => x.CompleteAsync("new-key", 200, It.IsAny<string>(), "application/json", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var executedContext = CreateExecutedContext(context, new OkObjectResult(new { id = 456 }));

        // Act
        await _filter.OnActionExecutionAsync(context, () => Task.FromResult(executedContext));

        // Assert
        _serviceMock.Verify(x => x.StartProcessingAsync(
            It.Is<IdempotencyRecord>(r => r.Key == "new-key"),
            It.IsAny<CancellationToken>()), Times.Once);

        _serviceMock.Verify(x => x.CompleteAsync(
            "new-key",
            200,
            It.IsAny<string>(),
            "application/json",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenActionThrowsException_FailsIdempotencyRecord()
    {
        // Arrange
        var context = CreateContext(new IdempotentAttribute());
        context.HttpContext.Request.Headers["X-Idempotency-Key"] = "error-key";

        _serviceMock.Setup(x => x.CheckAsync("error-key", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdempotencyCheckResult.NotFound());

        _serviceMock.Setup(x => x.StartProcessingAsync(It.IsAny<IdempotencyRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var exception = new InvalidOperationException("Test error");
        ActionExecutionDelegate next = () =>
        {
            var executedContext = CreateExecutedContext(context, null);
            executedContext.Exception = exception;
            return Task.FromResult(executedContext);
        };

        // Act
        await _filter.OnActionExecutionAsync(context, next);

        // Assert
        _serviceMock.Verify(x => x.FailAsync("error-key", "Test error", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WithKeyPrefix_AppliesPrefix()
    {
        // Arrange
        var context = CreateContext(new IdempotentAttribute { KeyPrefix = "orders" });
        context.HttpContext.Request.Headers["X-Idempotency-Key"] = "123";

        _serviceMock.Setup(x => x.CheckAsync("orders:123", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdempotencyCheckResult.NotFound());

        // Act
        await _filter.OnActionExecutionAsync(context, () => Task.FromResult(CreateExecutedContext(context, new OkResult())));

        // Assert
        _serviceMock.Verify(x => x.CheckAsync("orders:123", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WithCustomHeaderName_UsesCustomHeader()
    {
        // Arrange
        var context = CreateContext(new IdempotentAttribute { HeaderName = "X-Request-Id" });
        context.HttpContext.Request.Headers["X-Request-Id"] = "custom-key";

        _serviceMock.Setup(x => x.CheckAsync("custom-key", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdempotencyCheckResult.NotFound());

        // Act
        await _filter.OnActionExecutionAsync(context, () => Task.FromResult(CreateExecutedContext(context, new OkResult())));

        // Assert
        _serviceMock.Verify(x => x.CheckAsync("custom-key", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WithRequestBody_ComputesRequestHash()
    {
        // Arrange
        var context = CreateContext(new IdempotentAttribute { IncludeBodyInHash = true });
        context.HttpContext.Request.Headers["X-Idempotency-Key"] = "hash-key";
        context.HttpContext.Request.ContentType = "application/json";

        var body = "{\"name\":\"test\"}";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));
        context.HttpContext.Request.Body = stream;
        context.HttpContext.Request.ContentLength = body.Length;

        _serviceMock.Setup(x => x.CheckAsync("hash-key", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdempotencyCheckResult.NotFound());

        // Act
        await _filter.OnActionExecutionAsync(context, () => Task.FromResult(CreateExecutedContext(context, new OkResult())));

        // Assert
        _serviceMock.Verify(x => x.CheckAsync("hash-key", It.IsNotNull<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private ActionExecutingContext CreateContext(params object[] attributes)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "POST";
        httpContext.Request.Path = "/api/test";

        var actionDescriptor = new ActionDescriptor();
        if (attributes != null && attributes.Length > 0)
        {
            actionDescriptor.EndpointMetadata = new List<object>(attributes);
        }

        var routeData = new RouteData();
        var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller: null!);
    }

    private ActionExecutedContext CreateExecutedContext(ActionExecutingContext executing, IActionResult? result)
    {
        return new ActionExecutedContext(
            executing,
            new List<IFilterMetadata>(),
            controller: null!)
        {
            Result = result
        };
    }
}
