using System.Text.Json;
using System.Text.Json.Serialization;

namespace IntegrationTests.Contracts;

/// <summary>
/// Contract tests for API responses
/// Verifies that response structures remain consistent
/// </summary>
public class ApiResponseContractTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public void HealthResponse_ShouldMatchContract()
    {
        // Arrange
        var response = new HealthResponse
        {
            Status = "Healthy",
            Duration = "00:00:00.1234567"
        };

        // Act
        var json = JsonSerializer.Serialize(response, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<HealthResponse>(json, _jsonOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Status.Should().Be("Healthy");
        deserialized.Duration.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ErrorResponse_ShouldMatchContract()
    {
        // Arrange
        var response = new ErrorResponse
        {
            Type = "ValidationError",
            Title = "Validation Failed",
            Status = 400,
            Detail = "One or more fields are invalid",
            Instance = "/api/users"
        };

        // Act
        var json = JsonSerializer.Serialize(response, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<ErrorResponse>(json, _jsonOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Type.Should().Be("ValidationError");
        deserialized.Status.Should().Be(400);
    }

    [Fact]
    public void PaginatedResponse_ShouldMatchContract()
    {
        // Arrange
        var response = new PaginatedResponse<UserDto>
        {
            Data = new List<UserDto>
            {
                new() { Id = 1, Email = "test@example.com", Username = "testuser" }
            },
            Page = 1,
            PageSize = 10,
            TotalCount = 100,
            TotalPages = 10
        };

        // Act
        var json = JsonSerializer.Serialize(response, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<PaginatedResponse<UserDto>>(json, _jsonOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Data.Should().HaveCount(1);
        deserialized.TotalPages.Should().Be(10);
    }

    [Fact]
    public void NotificationEvent_ShouldMatchContract()
    {
        // Arrange
        var notification = new NotificationEventDto
        {
            EventId = Guid.NewGuid(),
            EventType = "UserRegistered",
            Timestamp = DateTimeOffset.UtcNow,
            Payload = new Dictionary<string, object>
            {
                ["userId"] = 123,
                ["email"] = "user@example.com"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(notification, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<NotificationEventDto>(json, _jsonOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.EventType.Should().Be("UserRegistered");
        deserialized.Payload.Should().ContainKey("userId");
    }

    [Fact]
    public void AuthTokenResponse_ShouldMatchContract()
    {
        // Arrange
        var response = new AuthTokenResponse
        {
            AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
            RefreshToken = "refresh-token-123",
            TokenType = "Bearer",
            ExpiresIn = 3600
        };

        // Act
        var json = JsonSerializer.Serialize(response, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<AuthTokenResponse>(json, _jsonOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.TokenType.Should().Be("Bearer");
        deserialized.ExpiresIn.Should().Be(3600);
    }

    [Fact]
    public void ServiceHealthResponse_ShouldMatchContract()
    {
        // Arrange
        var response = new ServiceHealthResponse
        {
            ServiceName = "UserService",
            Status = ServiceStatus.Healthy,
            Version = "1.0.0",
            Dependencies = new List<DependencyHealth>
            {
                new() { Name = "Database", Status = ServiceStatus.Healthy, ResponseTime = 5 },
                new() { Name = "Redis", Status = ServiceStatus.Healthy, ResponseTime = 2 }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(response, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<ServiceHealthResponse>(json, _jsonOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Status.Should().Be(ServiceStatus.Healthy);
        deserialized.Dependencies.Should().HaveCount(2);
    }

    [Fact]
    public void AuditLogEntry_ShouldMatchContract()
    {
        // Arrange
        var entry = new AuditLogEntryDto
        {
            Id = Guid.NewGuid(),
            Action = "UserLogin",
            UserId = "user-123",
            Timestamp = DateTimeOffset.UtcNow,
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0...",
            Details = new Dictionary<string, string>
            {
                ["browser"] = "Chrome",
                ["os"] = "Windows"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(entry, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<AuditLogEntryDto>(json, _jsonOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Action.Should().Be("UserLogin");
        deserialized.Details.Should().ContainKey("browser");
    }

    [Fact]
    public void BackupStatus_ShouldMatchContract()
    {
        // Arrange
        var status = new BackupStatusDto
        {
            Id = Guid.NewGuid(),
            Status = BackupState.Completed,
            StartedAt = DateTimeOffset.UtcNow.AddMinutes(-10),
            CompletedAt = DateTimeOffset.UtcNow,
            SizeInBytes = 1024 * 1024 * 100,
            FileName = "backup-2024-01-15.bak"
        };

        // Act
        var json = JsonSerializer.Serialize(status, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<BackupStatusDto>(json, _jsonOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Status.Should().Be(BackupState.Completed);
        deserialized.SizeInBytes.Should().Be(1024 * 1024 * 100);
    }

    [Fact]
    public void CacheEntry_ShouldMatchContract()
    {
        // Arrange
        var entry = new CacheEntryDto
        {
            Key = "user:123:profile",
            Value = JsonSerializer.Serialize(new { name = "John", age = 30 }),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(entry, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<CacheEntryDto>(json, _jsonOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Key.Should().Be("user:123:profile");
        deserialized.Value.Should().Contain("John");
    }
}

#region Contract DTOs

public record HealthResponse
{
    public string Status { get; init; } = string.Empty;
    public string Duration { get; init; } = string.Empty;
}

public record ErrorResponse
{
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public int Status { get; init; }
    public string Detail { get; init; } = string.Empty;
    public string Instance { get; init; } = string.Empty;
}

public record PaginatedResponse<T>
{
    public List<T> Data { get; init; } = new();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}

public record UserDto
{
    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
}

public record NotificationEventDto
{
    public Guid EventId { get; init; }
    public string EventType { get; init; } = string.Empty;
    public DateTimeOffset Timestamp { get; init; }
    public Dictionary<string, object> Payload { get; init; } = new();
}

public record AuthTokenResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public string TokenType { get; init; } = string.Empty;
    public int ExpiresIn { get; init; }
}

public record ServiceHealthResponse
{
    public string ServiceName { get; init; } = string.Empty;
    public ServiceStatus Status { get; init; }
    public string Version { get; init; } = string.Empty;
    public List<DependencyHealth> Dependencies { get; init; } = new();
}

public record DependencyHealth
{
    public string Name { get; init; } = string.Empty;
    public ServiceStatus Status { get; init; }
    public long ResponseTime { get; init; }
}

public enum ServiceStatus
{
    Healthy,
    Degraded,
    Unhealthy
}

public record AuditLogEntryDto
{
    public Guid Id { get; init; }
    public string Action { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public DateTimeOffset Timestamp { get; init; }
    public string IpAddress { get; init; } = string.Empty;
    public string UserAgent { get; init; } = string.Empty;
    public Dictionary<string, string> Details { get; init; } = new();
}

public record BackupStatusDto
{
    public Guid Id { get; init; }
    public BackupState Status { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public long SizeInBytes { get; init; }
    public string FileName { get; init; } = string.Empty;
}

public enum BackupState
{
    Pending,
    InProgress,
    Completed,
    Failed
}

public record CacheEntryDto
{
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

#endregion
