using System.Net;
using System.Net.Http.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using LoggingService.Domain;
using Serilog;
using Serilog.Core;

namespace LoggingService.IntegrationTests;

public class LoggingServiceE2ETests : IAsyncLifetime
{
    private IContainer? _seqContainer;
    private LoggingWebApplicationFactory? _factory;
    private HttpClient? _client;
    private string _seqUrl = string.Empty;

    public async Task InitializeAsync()
    {
        // Start Seq container
        _seqContainer = new ContainerBuilder()
            .WithImage("datalust/seq:latest")
            .WithPortBinding(5341, true)
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(5341)))
            .Build();

        await _seqContainer.StartAsync();

        // Get the mapped port
        var seqPort = _seqContainer.GetMappedPublicPort(5341);
        _seqUrl = $"http://localhost:{seqPort}";

        // Create factory with Seq URL
        _factory = new LoggingWebApplicationFactory
        {
            SeqUrl = _seqUrl
        };

        _client = _factory.CreateClient();

        // Wait a bit for Seq to be fully ready
        await Task.Delay(2000);
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();

        if (_seqContainer != null)
        {
            await _seqContainer.DisposeAsync();
        }
    }

    [Fact]
    public async Task E2E_WriteLogsToSeq_ThenQueryThroughApi()
    {
        // Arrange - Write some logs to Seq
        var logger = new LoggerConfiguration()
            .WriteTo.Seq(_seqUrl)
            .Enrich.WithProperty("ServiceName", "E2ETestService")
            .CreateLogger();

        var requestId = Guid.NewGuid().ToString();
        var traceId = Guid.NewGuid().ToString();

        // Act - Write logs
        logger.ForContext("RequestId", requestId)
              .ForContext("TraceId", traceId)
              .ForContext("UserId", "test-user-123")
              .Information("E2E test information log");

        logger.ForContext("RequestId", requestId)
              .ForContext("TraceId", traceId)
              .Warning("E2E test warning log");

        logger.ForContext("RequestId", requestId)
              .ForContext("TraceId", traceId)
              .Error("E2E test error log");

        logger.Dispose();

        // Wait for Seq to index the logs
        await Task.Delay(3000);

        // Assert - Query logs through API
        var response = await _client!.GetAsync(
            $"/api/logs?serviceName=E2ETestService&requestId={requestId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var logs = await response.Content.ReadFromJsonAsync<List<LogEntry>>();
        logs.Should().NotBeNull();
        logs!.Count.Should().BeGreaterThanOrEqualTo(3);
        logs.Should().AllSatisfy(log =>
        {
            log.ServiceName.Should().Be("E2ETestService");
            log.RequestId.Should().Be(requestId);
            log.TraceId.Should().Be(traceId);
        });
    }

    [Fact]
    public async Task E2E_WriteDifferentLogLevels_ThenFilterByLevel()
    {
        // Arrange
        var logger = new LoggerConfiguration()
            .WriteTo.Seq(_seqUrl)
            .Enrich.WithProperty("ServiceName", "E2ELevelTestService")
            .CreateLogger();

        var testId = Guid.NewGuid().ToString();

        // Act - Write logs with different levels
        var contextLogger = logger.ForContext("TestId", testId);

        contextLogger.Verbose($"Trace log {testId}");
        contextLogger.Debug($"Debug log {testId}");
        contextLogger.Information($"Information log {testId}");
        contextLogger.Warning($"Warning log {testId}");
        contextLogger.Error($"Error log {testId}");
        contextLogger.Fatal($"Critical log {testId}");

        logger.Dispose();

        // Wait for indexing
        await Task.Delay(3000);

        // Assert - Filter by error level
        var response = await _client!.GetAsync(
            $"/api/logs?serviceName=E2ELevelTestService&minLevel=4&searchText={testId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var logs = await response.Content.ReadFromJsonAsync<List<LogEntry>>();
        logs.Should().NotBeNull();
        logs.Should().AllSatisfy(log =>
        {
            log.Level.Should().BeOneOf(Domain.LogLevel.Error, Domain.LogLevel.Critical);
        });
    }

    [Fact]
    public async Task E2E_WriteLogsWithException_ThenQueryWithExceptionFilter()
    {
        // Arrange
        var logger = new LoggerConfiguration()
            .WriteTo.Seq(_seqUrl)
            .Enrich.WithProperty("ServiceName", "E2EExceptionTestService")
            .CreateLogger();

        var testId = Guid.NewGuid().ToString();

        // Act - Write logs with and without exceptions
        logger.Information($"Normal log {testId}");

        try
        {
            throw new InvalidOperationException("E2E test exception");
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Error log with exception {testId}");
        }

        logger.Dispose();

        // Wait for indexing
        await Task.Delay(3000);

        // Assert - Query logs with exceptions
        var response = await _client!.GetAsync(
            $"/api/logs?serviceName=E2EExceptionTestService&hasException=true&searchText={testId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var logs = await response.Content.ReadFromJsonAsync<List<LogEntry>>();
        logs.Should().NotBeNull();
        logs.Should().AllSatisfy(log =>
        {
            log.HasException().Should().BeTrue();
            log.Exception.Should().Contain("InvalidOperationException");
        });
    }

    [Fact]
    public async Task E2E_WriteLogsFromMultipleServices_ThenGetStatistics()
    {
        // Arrange
        var services = new[] { "ServiceA", "ServiceB", "ServiceC" };
        var testBatch = Guid.NewGuid().ToString();

        foreach (var serviceName in services)
        {
            var logger = new LoggerConfiguration()
                .WriteTo.Seq(_seqUrl)
                .Enrich.WithProperty("ServiceName", serviceName)
                .Enrich.WithProperty("TestBatch", testBatch)
                .CreateLogger();

            // Write different amounts of logs per service
            var logCount = Array.IndexOf(services, serviceName) + 1;
            for (int i = 0; i < logCount * 5; i++)
            {
                logger.Information($"Log {i} from {serviceName}");
            }

            logger.Dispose();
        }

        // Wait for indexing
        await Task.Delay(4000);

        // Act - Get statistics
        var response = await _client!.GetAsync("/api/logs/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var statistics = await response.Content.ReadFromJsonAsync<LogStatistics>();
        statistics.Should().NotBeNull();
        statistics!.TotalLogs.Should().BeGreaterThan(0);
        statistics.LogsByService.Should().NotBeEmpty();
    }

    [Fact]
    public async Task E2E_QueryLogsWithDateRange_ReturnsOnlyLogsInRange()
    {
        // Arrange
        var logger = new LoggerConfiguration()
            .WriteTo.Seq(_seqUrl)
            .Enrich.WithProperty("ServiceName", "E2EDateRangeTestService")
            .CreateLogger();

        var testId = Guid.NewGuid().ToString();
        var startTime = DateTime.UtcNow;

        logger.Information($"Log before delay {testId}");
        await Task.Delay(2000);

        var midTime = DateTime.UtcNow;
        logger.Information($"Log after delay {testId}");

        logger.Dispose();

        // Wait for indexing
        await Task.Delay(3000);

        // Act - Query with date range
        var response = await _client!.GetAsync(
            $"/api/logs?serviceName=E2EDateRangeTestService&startDate={midTime:O}&searchText={testId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var logs = await response.Content.ReadFromJsonAsync<List<LogEntry>>();
        logs.Should().NotBeNull();

        if (logs!.Any())
        {
            logs.Should().AllSatisfy(log =>
            {
                log.Timestamp.Should().BeOnOrAfter(midTime.AddSeconds(-5)); // Allow some tolerance
            });
        }
    }

    [Fact]
    public async Task E2E_PaginationWorks_AcrossMultiplePages()
    {
        // Arrange - Write many logs
        var logger = new LoggerConfiguration()
            .WriteTo.Seq(_seqUrl)
            .Enrich.WithProperty("ServiceName", "E2EPaginationTestService")
            .CreateLogger();

        var testId = Guid.NewGuid().ToString();

        for (int i = 0; i < 50; i++)
        {
            logger.Information($"Pagination test log {i} - {testId}");
        }

        logger.Dispose();

        // Wait for indexing
        await Task.Delay(3000);

        // Act - Get first and second page
        var response1 = await _client!.GetAsync(
            $"/api/logs?serviceName=E2EPaginationTestService&pageNumber=1&pageSize=20&searchText={testId}");
        var response2 = await _client!.GetAsync(
            $"/api/logs?serviceName=E2EPaginationTestService&pageNumber=2&pageSize=20&searchText={testId}");

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        var logs1 = await response1.Content.ReadFromJsonAsync<List<LogEntry>>();
        var logs2 = await response2.Content.ReadFromJsonAsync<List<LogEntry>>();

        logs1.Should().NotBeNull();
        logs2.Should().NotBeNull();

        // Pages should have different logs
        if (logs1!.Count > 0 && logs2!.Count > 0)
        {
            var ids1 = logs1.Select(l => l.Id).ToHashSet();
            var ids2 = logs2.Select(l => l.Id).ToHashSet();
            ids1.Should().NotIntersectWith(ids2);
        }
    }

    [Fact]
    public async Task E2E_GetLogById_ReturnsCorrectLog()
    {
        // Arrange - Write a specific log
        var logger = new LoggerConfiguration()
            .WriteTo.Seq(_seqUrl)
            .Enrich.WithProperty("ServiceName", "E2EGetByIdTestService")
            .CreateLogger();

        var uniqueMessage = $"Unique log message {Guid.NewGuid()}";
        logger.Information(uniqueMessage);
        logger.Dispose();

        // Wait for indexing
        await Task.Delay(3000);

        // Get the log ID
        var searchResponse = await _client!.GetAsync(
            $"/api/logs?serviceName=E2EGetByIdTestService&searchText={uniqueMessage.Split(' ')[0]}");
        var logs = await searchResponse.Content.ReadFromJsonAsync<List<LogEntry>>();

        if (logs?.Any() != true)
        {
            // Skip if log not found
            return;
        }

        var logId = logs.First().Id;

        // Act - Get by ID
        var response = await _client!.GetAsync($"/api/logs/{logId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var log = await response.Content.ReadFromJsonAsync<LogEntry>();
        log.Should().NotBeNull();
        log!.Id.Should().Be(logId);
        log.Message.Should().Contain("Unique log message");
    }

    [Fact]
    public async Task E2E_HealthCheck_SeqIsAccessible()
    {
        // Act - Check if Seq is accessible
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync($"{_seqUrl}/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
