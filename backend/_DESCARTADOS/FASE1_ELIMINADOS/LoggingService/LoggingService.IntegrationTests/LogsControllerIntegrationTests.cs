using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LoggingService.Domain;

namespace LoggingService.IntegrationTests;

public class LogsControllerIntegrationTests : IClassFixture<LoggingWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly LoggingWebApplicationFactory _factory;

    public LogsControllerIntegrationTests(LoggingWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetLogs_WithoutFilters_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/logs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLogs_WithValidFilters_ReturnsFilteredLogs()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddHours(-1);
        var endDate = DateTime.UtcNow;

        // Act
        var response = await _client.GetAsync(
            $"/api/logs?startDate={startDate:O}&endDate={endDate:O}&minLevel=2&pageSize=50");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var logs = await response.Content.ReadFromJsonAsync<List<LogEntry>>();
        logs.Should().NotBeNull();
        logs.Should().AllSatisfy(log =>
        {
            log.Timestamp.Should().BeOnOrAfter(startDate);
            log.Timestamp.Should().BeOnOrBefore(endDate);
            ((int)log.Level).Should().BeGreaterThanOrEqualTo((int)Domain.LogLevel.Information);
        });
    }

    [Fact]
    public async Task GetLogs_WithInvalidFilter_ReturnsBadRequest()
    {
        // Arrange - EndDate before StartDate
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddHours(-1);

        // Act
        var response = await _client.GetAsync(
            $"/api/logs?startDate={startDate:O}&endDate={endDate:O}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetLogs_WithPageSizeExceedingMax_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/logs?pageSize=1001");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetLogs_WithServiceNameFilter_ReturnsLogsFromService()
    {
        // Act
        var response = await _client.GetAsync("/api/logs?serviceName=TestService");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var logs = await response.Content.ReadFromJsonAsync<List<LogEntry>>();
        logs.Should().NotBeNull();
        logs.Should().AllSatisfy(log =>
            log.ServiceName.Should().Be("TestService"));
    }

    [Fact]
    public async Task GetLogs_WithSearchText_ReturnsMatchingLogs()
    {
        // Act
        var response = await _client.GetAsync("/api/logs?searchText=error");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var logs = await response.Content.ReadFromJsonAsync<List<LogEntry>>();
        logs.Should().NotBeNull();
    }

    [Fact]
    public async Task GetLogs_WithPagination_ReturnsCorrectPage()
    {
        // Act
        var response1 = await _client.GetAsync("/api/logs?pageNumber=1&pageSize=10");
        var response2 = await _client.GetAsync("/api/logs?pageNumber=2&pageSize=10");

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        var logs1 = await response1.Content.ReadFromJsonAsync<List<LogEntry>>();
        var logs2 = await response2.Content.ReadFromJsonAsync<List<LogEntry>>();

        logs1.Should().NotBeNull();
        logs2.Should().NotBeNull();

        // Different pages should have different logs (if there are enough logs)
        if (logs1!.Count > 0 && logs2!.Count > 0)
        {
            var ids1 = logs1.Select(l => l.Id).ToHashSet();
            var ids2 = logs2.Select(l => l.Id).ToHashSet();
            ids1.Should().NotIntersectWith(ids2);
        }
    }

    [Fact]
    public async Task GetLogById_WithValidId_ReturnsLog()
    {
        // Arrange - First get logs to have a valid ID
        var logsResponse = await _client.GetAsync("/api/logs?pageSize=1");
        var logs = await logsResponse.Content.ReadFromJsonAsync<List<LogEntry>>();

        if (logs?.Any() != true)
        {
            // Skip test if no logs available
            return;
        }

        var logId = logs.First().Id;

        // Act
        var response = await _client.GetAsync($"/api/logs/{logId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var log = await response.Content.ReadFromJsonAsync<LogEntry>();
        log.Should().NotBeNull();
        log!.Id.Should().Be(logId);
    }

    [Fact]
    public async Task GetLogById_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/logs/invalid-id-12345");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetStatistics_WithoutDateRange_ReturnsStatistics()
    {
        // Act
        var response = await _client.GetAsync("/api/logs/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var statistics = await response.Content.ReadFromJsonAsync<LogStatistics>();
        statistics.Should().NotBeNull();
        statistics!.TotalLogs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetStatistics_WithDateRange_ReturnsFilteredStatistics()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddHours(-24);
        var endDate = DateTime.UtcNow;

        // Act
        var response = await _client.GetAsync(
            $"/api/logs/statistics?startDate={startDate:O}&endDate={endDate:O}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var statistics = await response.Content.ReadFromJsonAsync<LogStatistics>();
        statistics.Should().NotBeNull();
        statistics!.TotalLogs.Should().BeGreaterThanOrEqualTo(0);

        if (statistics.OldestLog.HasValue && statistics.NewestLog.HasValue)
        {
            statistics.OldestLog.Value.Should().BeOnOrAfter(startDate);
            statistics.NewestLog.Value.Should().BeOnOrBefore(endDate);
        }
    }

    [Fact]
    public async Task GetStatistics_CalculatesErrorRateCorrectly()
    {
        // Act
        var response = await _client.GetAsync("/api/logs/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var statistics = await response.Content.ReadFromJsonAsync<LogStatistics>();
        statistics.Should().NotBeNull();

        var expectedErrorRate = statistics!.TotalLogs > 0
            ? (double)(statistics.ErrorCount + statistics.CriticalCount) / statistics.TotalLogs * 100
            : 0;

        statistics.GetErrorRate().Should().Be(expectedErrorRate);
    }

    [Fact]
    public async Task Api_SupportsMultipleConcurrentRequests()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Send 10 concurrent requests
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_client.GetAsync("/api/logs?pageSize=10"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response =>
            response.StatusCode.Should().Be(HttpStatusCode.OK));
    }
}
