using System.Text.Json;

namespace IntegrationTests.Contract;

/// <summary>
/// Contract tests for event serialization/deserialization
/// Ensures backward compatibility of event schemas
/// </summary>
public class EventContractTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    [Fact]
    public void ErrorCriticalEvent_ShouldSerializeCorrectly()
    {
        // Arrange
        var @event = new ErrorCriticalEvent
        {
            ErrorId = Guid.NewGuid(),
            ServiceName = "VehicleService",
            Message = "Database connection failed",
            StatusCode = 500,
            ExceptionType = "SqlException",
            StackTrace = "at VehicleService.Repositories...",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(@event, _options);
        var deserialized = JsonSerializer.Deserialize<ErrorCriticalEvent>(json, _options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.ErrorId.Should().Be(@event.ErrorId);
        deserialized.ServiceName.Should().Be(@event.ServiceName);
        deserialized.StatusCode.Should().Be(@event.StatusCode);
        deserialized.EventType.Should().Be("error.critical");
    }

    [Fact]
    public void UserRegisteredEvent_ShouldSerializeCorrectly()
    {
        // Arrange
        var @event = new UserRegisteredEvent
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            FullName = "Test User",
            RegisteredAt = DateTime.UtcNow,
            Roles = new[] { "User", "Customer" }
        };

        // Act
        var json = JsonSerializer.Serialize(@event, _options);
        var deserialized = JsonSerializer.Deserialize<UserRegisteredEvent>(json, _options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.UserId.Should().Be(@event.UserId);
        deserialized.Email.Should().Be(@event.Email);
        deserialized.Roles.Should().BeEquivalentTo(@event.Roles);
        deserialized.EventType.Should().Be("auth.user.registered");
    }

    [Fact]
    public void NotificationSentEvent_ShouldSerializeCorrectly()
    {
        // Arrange
        var @event = new NotificationSentEvent
        {
            NotificationId = Guid.NewGuid(),
            Channel = "Email",
            Recipient = "user@example.com",
            Subject = "Welcome!",
            SentAt = DateTime.UtcNow,
            Success = true
        };

        // Act
        var json = JsonSerializer.Serialize(@event, _options);
        var deserialized = JsonSerializer.Deserialize<NotificationSentEvent>(json, _options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.NotificationId.Should().Be(@event.NotificationId);
        deserialized.Channel.Should().Be(@event.Channel);
        deserialized.Success.Should().BeTrue();
    }

    [Fact]
    public void BackupCompletedEvent_ShouldSerializeCorrectly()
    {
        // Arrange
        var @event = new BackupCompletedEvent
        {
            BackupId = Guid.NewGuid(),
            DatabaseName = "cardealer_db",
            BackupType = "Full",
            FilePath = "/backups/2025/12/03/backup.sql.gz",
            FileSizeBytes = 1024 * 1024 * 100, // 100MB
            Duration = TimeSpan.FromMinutes(5),
            Success = true,
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(@event, _options);
        var deserialized = JsonSerializer.Deserialize<BackupCompletedEvent>(json, _options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.BackupId.Should().Be(@event.BackupId);
        deserialized.FileSizeBytes.Should().Be(@event.FileSizeBytes);
        deserialized.Success.Should().BeTrue();
    }

    [Theory]
    [InlineData(typeof(ErrorCriticalEvent))]
    [InlineData(typeof(UserRegisteredEvent))]
    [InlineData(typeof(NotificationSentEvent))]
    [InlineData(typeof(BackupCompletedEvent))]
    public void AllEvents_ShouldHaveEventTypeProperty(Type eventType)
    {
        // Arrange
        var instance = Activator.CreateInstance(eventType);

        // Act
        var eventTypeProperty = eventType.GetProperty("EventType");

        // Assert
        eventTypeProperty.Should().NotBeNull();
        eventTypeProperty!.PropertyType.Should().Be<string>();
    }

    [Fact]
    public void Events_ShouldBeBackwardCompatible_WithMissingOptionalFields()
    {
        // Arrange - JSON without optional fields
        var minimalJson = """
        {
            "errorId": "550e8400-e29b-41d4-a716-446655440000",
            "serviceName": "TestService",
            "statusCode": 500
        }
        """;

        // Act
        var deserialized = JsonSerializer.Deserialize<ErrorCriticalEvent>(minimalJson, _options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.ServiceName.Should().Be("TestService");
        deserialized.Message.Should().BeNull();
        deserialized.StackTrace.Should().BeNull();
    }

    [Fact]
    public void Events_ShouldIgnoreUnknownFields()
    {
        // Arrange - JSON with extra unknown fields
        var jsonWithExtras = """
        {
            "errorId": "550e8400-e29b-41d4-a716-446655440000",
            "serviceName": "TestService",
            "statusCode": 500,
            "unknownField": "should be ignored",
            "anotherUnknown": 12345
        }
        """;

        // Act
        var deserialized = JsonSerializer.Deserialize<ErrorCriticalEvent>(jsonWithExtras, _options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.ServiceName.Should().Be("TestService");
    }
}

#region Event DTOs for Contract Testing

public class ErrorCriticalEvent
{
    public Guid ErrorId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? Message { get; set; }
    public int StatusCode { get; set; }
    public string? ExceptionType { get; set; }
    public string? StackTrace { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string EventType => "error.critical";
}

public class UserRegisteredEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public string[] Roles { get; set; } = Array.Empty<string>();
    public string EventType => "auth.user.registered";
}

public class NotificationSentEvent
{
    public Guid NotificationId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string EventType => "notification.sent";
}

public class BackupCompletedEvent
{
    public Guid BackupId { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public string BackupType { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public long FileSizeBytes { get; set; }
    public TimeSpan Duration { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    public string EventType => "backup.completed";
}

#endregion
