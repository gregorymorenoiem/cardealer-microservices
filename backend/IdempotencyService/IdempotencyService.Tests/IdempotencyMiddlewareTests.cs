using System.Text;
using FluentAssertions;
using IdempotencyService.Api.Middleware;
using IdempotencyService.Core.Interfaces;
using IdempotencyService.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace IdempotencyService.Tests;

public class IdempotencyMiddlewareTests
{
    private readonly Mock<IIdempotencyService> _serviceMock;
    private readonly Mock<ILogger<IdempotencyMiddleware>> _loggerMock;
    private readonly IdempotencyOptions _options;
    private readonly Mock<RequestDelegate> _nextMock;

    public IdempotencyMiddlewareTests()
    {
        _serviceMock = new Mock<IIdempotencyService>();
        _loggerMock = new Mock<ILogger<IdempotencyMiddleware>>();
        _nextMock = new Mock<RequestDelegate>();
        _options = new IdempotencyOptions
        {
            HeaderName = "X-Idempotency-Key",
            DefaultTtlSeconds = 3600,
            IdempotentMethods = new List<string> { "POST", "PUT", "PATCH" },
            ExcludedPaths = new List<string> { "/health", "/metrics" }
        };
    }

    [Fact]
    public async Task InvokeAsync_WithExcludedPath_CallsNextMiddleware()
    {
        // Arrange
        var middleware = new IdempotencyMiddleware(_nextMock.Object, _loggerMock.Object, Options.Create(_options));
        var context = new DefaultHttpContext();
        context.Request.Path = "/health";

        // Act
        await middleware.InvokeAsync(context, _serviceMock.Object);

        // Assert
        _nextMock.Verify(x => x(context), Times.Once);
        _serviceMock.Verify(x => x.CheckAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_WithNonIdempotentMethod_CallsNextMiddleware()
    {
        // Arrange
        var middleware = new IdempotencyMiddleware(_nextMock.Object, _loggerMock.Object, Options.Create(_options));
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/data";

        // Act
        await middleware.InvokeAsync(context, _serviceMock.Object);

        // Assert
        _nextMock.Verify(x => x(context), Times.Once);
        _serviceMock.Verify(x => x.CheckAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_WithoutIdempotencyKeyAndRequired_ReturnsBadRequest()
    {
        // Arrange
        _options.RequireIdempotencyKey = true;
        var middleware = new IdempotencyMiddleware(_nextMock.Object, _loggerMock.Object, Options.Create(_options));
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/orders";
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context, _serviceMock.Object);

        // Assert
        context.Response.StatusCode.Should().Be(400);
        _nextMock.Verify(x => x(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_WithoutIdempotencyKeyAndNotRequired_CallsNextMiddleware()
    {
        // Arrange
        _options.RequireIdempotencyKey = false;
        var middleware = new IdempotencyMiddleware(_nextMock.Object, _loggerMock.Object, Options.Create(_options));
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/orders";

        // Act
        await middleware.InvokeAsync(context, _serviceMock.Object);

        // Assert
        _nextMock.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithCachedResponse_ReturnsCachedResult()
    {
        // Arrange
        var middleware = new IdempotencyMiddleware(_nextMock.Object, _loggerMock.Object, Options.Create(_options));
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/orders";
        context.Request.Headers["X-Idempotency-Key"] = "cached-key";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"item\":\"test\"}"));
        context.Response.Body = new MemoryStream();

        var cachedRecord = new IdempotencyRecord
        {
            Key = "cached-key",
            Status = IdempotencyStatus.Completed,
            ResponseStatusCode = 201,
            ResponseBody = "{\"id\":999}",
            ResponseContentType = "application/json"
        };

        _serviceMock.Setup(x => x.CheckAsync("cached-key", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdempotencyCheckResult.Completed(cachedRecord));

        // Act
        await middleware.InvokeAsync(context, _serviceMock.Object);

        // Assert
        context.Response.StatusCode.Should().Be(201);
        context.Response.Headers["X-Idempotency-Replayed"].ToString().Should().Be("true");
        _nextMock.Verify(x => x(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_WithProcessingRecord_ReturnsConflict()
    {
        // Arrange
        var middleware = new IdempotencyMiddleware(_nextMock.Object, _loggerMock.Object, Options.Create(_options));
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/orders";
        context.Request.Headers["X-Idempotency-Key"] = "processing-key";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"item\":\"test\"}"));
        context.Response.Body = new MemoryStream();

        var processingRecord = new IdempotencyRecord
        {
            Key = "processing-key",
            Status = IdempotencyStatus.Processing
        };

        _serviceMock.Setup(x => x.CheckAsync("processing-key", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdempotencyCheckResult.Processing(processingRecord));

        // Act
        await middleware.InvokeAsync(context, _serviceMock.Object);

        // Assert
        context.Response.StatusCode.Should().Be(409);
        _nextMock.Verify(x => x(It.IsAny<HttpContext>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_WithConflictingRequestHash_ReturnsConflict()
    {
        // Arrange
        var middleware = new IdempotencyMiddleware(_nextMock.Object, _loggerMock.Object, Options.Create(_options));
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/orders";
        context.Request.Headers["X-Idempotency-Key"] = "conflict-key";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"item\":\"test\"}"));
        context.Response.Body = new MemoryStream();

        var conflictRecord = new IdempotencyRecord
        {
            Key = "conflict-key",
            Status = IdempotencyStatus.Completed,
            RequestHash = "different-hash"
        };

        _serviceMock.Setup(x => x.CheckAsync("conflict-key", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdempotencyCheckResult.Conflict(conflictRecord, "Request body differs"));

        // Act
        await middleware.InvokeAsync(context, _serviceMock.Object);

        // Assert
        context.Response.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task InvokeAsync_WithNewKey_ExecutesRequestAndStoresResult()
    {
        // Arrange
        var middleware = new IdempotencyMiddleware(_nextMock.Object, _loggerMock.Object, Options.Create(_options));
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/orders";
        context.Request.Headers["X-Idempotency-Key"] = "new-key";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"item\":\"test\"}"));
        context.Response.Body = new MemoryStream();

        _serviceMock.Setup(x => x.CheckAsync("new-key", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdempotencyCheckResult.NotFound());

        _serviceMock.Setup(x => x.StartProcessingAsync(It.IsAny<IdempotencyRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .Returns((HttpContext ctx) =>
            {
                ctx.Response.StatusCode = 201;
                ctx.Response.Body.Write(Encoding.UTF8.GetBytes("{\"id\":123}"));
                return Task.CompletedTask;
            });

        // Act
        await middleware.InvokeAsync(context, _serviceMock.Object);

        // Assert
        _nextMock.Verify(x => x(context), Times.Once);
        _serviceMock.Verify(x => x.StartProcessingAsync(
            It.Is<IdempotencyRecord>(r => r.Key == "new-key"),
            It.IsAny<CancellationToken>()), Times.Once);
        _serviceMock.Verify(x => x.CompleteAsync(
            "new-key",
            201,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrowsException_FailsIdempotencyRecord()
    {
        // Arrange
        var middleware = new IdempotencyMiddleware(_nextMock.Object, _loggerMock.Object, Options.Create(_options));
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/orders";
        context.Request.Headers["X-Idempotency-Key"] = "error-key";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"item\":\"test\"}"));
        context.Response.Body = new MemoryStream();

        _serviceMock.Setup(x => x.CheckAsync("error-key", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdempotencyCheckResult.NotFound());

        _serviceMock.Setup(x => x.StartProcessingAsync(It.IsAny<IdempotencyRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(new InvalidOperationException("Test error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await middleware.InvokeAsync(context, _serviceMock.Object));

        _serviceMock.Verify(x => x.FailAsync("error-key", "Test error", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithMultipleExcludedPaths_SkipsCorrectPaths()
    {
        // Arrange
        var middleware = new IdempotencyMiddleware(_nextMock.Object, _loggerMock.Object, Options.Create(_options));
        var context1 = new DefaultHttpContext();
        context1.Request.Path = "/metrics";

        var context2 = new DefaultHttpContext();
        context2.Request.Path = "/health/ready";

        // Act
        await middleware.InvokeAsync(context1, _serviceMock.Object);
        await middleware.InvokeAsync(context2, _serviceMock.Object);

        // Assert
        _nextMock.Verify(x => x(It.IsAny<HttpContext>()), Times.Exactly(2));
        _serviceMock.Verify(x => x.CheckAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
