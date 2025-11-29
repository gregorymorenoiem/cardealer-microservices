using System.Text.Json;
using CarDealer.Contracts.Events.Auth;
using CarDealer.Contracts.Events.Error;
using CarDealer.Contracts.Events.Vehicle;
using CarDealer.Contracts.Events.Media;
using CarDealer.Contracts.Events.Notification;
using CarDealer.Contracts.Events.Audit;
using FluentAssertions;

namespace CarDealer.Contracts.Tests.Events;

/// <summary>
/// Tests to verify that all events can be serialized and deserialized correctly.
/// This is critical for RabbitMQ message passing.
/// </summary>
public class EventSerializationTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    #region Auth Events

    [Fact]
    public void UserRegisteredEvent_Should_Serialize_And_Deserialize_Correctly()
    {
        // Arrange
        var originalEvent = new UserRegisteredEvent
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            FullName = "John Doe",
            RegisteredAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, string>
            {
                { "Source", "WebApp" },
                { "IPAddress", "192.168.1.1" }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<UserRegisteredEvent>(json, _jsonOptions);

        // Assert
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.EventType.Should().Be("auth.user.registered");
        deserializedEvent.UserId.Should().Be(originalEvent.UserId);
        deserializedEvent.Email.Should().Be(originalEvent.Email);
        deserializedEvent.FullName.Should().Be(originalEvent.FullName);
        deserializedEvent.EventId.Should().NotBeEmpty();
    }

    [Fact]
    public void UserLoggedInEvent_Should_Serialize_And_Deserialize_Correctly()
    {
        // Arrange
        var originalEvent = new UserLoggedInEvent
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            LoggedInAt = DateTime.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<UserLoggedInEvent>(json, _jsonOptions);

        // Assert
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.EventType.Should().Be("auth.user.loggedin");
        deserializedEvent.UserId.Should().Be(originalEvent.UserId);
        deserializedEvent.IpAddress.Should().Be(originalEvent.IpAddress);
    }

    #endregion

    #region Error Events

    [Fact]
    public void ErrorCriticalEvent_Should_Serialize_And_Deserialize_Correctly()
    {
        // Arrange
        var originalEvent = new ErrorCriticalEvent
        {
            ErrorId = Guid.NewGuid(),
            ServiceName = "VehicleService",
            ExceptionType = "NullReferenceException",
            Message = "Critical error occurred",
            StackTrace = "at VehicleService.Controllers.VehicleController.Get()",
            StatusCode = 500,
            Endpoint = "/api/vehicles",
            UserId = Guid.NewGuid().ToString(),
            Metadata = new Dictionary<string, object>
            {
                { "RequestId", Guid.NewGuid().ToString() },
                { "Environment", "Production" }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<ErrorCriticalEvent>(json, _jsonOptions);

        // Assert
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.EventType.Should().Be("error.critical");
        deserializedEvent.ErrorId.Should().Be(originalEvent.ErrorId);
        deserializedEvent.ServiceName.Should().Be(originalEvent.ServiceName);
        deserializedEvent.StatusCode.Should().Be(500);
        deserializedEvent.ExceptionType.Should().Be("NullReferenceException");
    }

    [Fact]
    public void ErrorSpikeDetectedEvent_Should_Serialize_And_Deserialize_Correctly()
    {
        // Arrange
        var originalEvent = new ErrorSpikeDetectedEvent
        {
            ServiceName = "AuthService",
            ErrorCount = 100,
            WindowMinutes = 5,
            Threshold = 50,
            DetectedAt = DateTime.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<ErrorSpikeDetectedEvent>(json, _jsonOptions);

        // Assert
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.EventType.Should().Be("error.spike.detected");
        deserializedEvent.ErrorCount.Should().Be(100);
        deserializedEvent.Threshold.Should().Be(50);
    }

    #endregion

    #region Vehicle Events

    [Fact]
    public void VehicleCreatedEvent_Should_Serialize_And_Deserialize_Correctly()
    {
        // Arrange
        var originalEvent = new VehicleCreatedEvent
        {
            VehicleId = Guid.NewGuid(),
            Make = "Toyota",
            Model = "Camry",
            Year = 2024,
            Price = 35000.00m,
            VIN = "1HGBH41JXMN109186",
            CreatedBy = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<VehicleCreatedEvent>(json, _jsonOptions);

        // Assert
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.EventType.Should().Be("vehicle.created");
        deserializedEvent.Make.Should().Be("Toyota");
        deserializedEvent.Model.Should().Be("Camry");
        deserializedEvent.Price.Should().Be(35000.00m);
    }

    [Fact]
    public void VehicleSoldEvent_Should_Serialize_And_Deserialize_Correctly()
    {
        // Arrange
        var originalEvent = new VehicleSoldEvent
        {
            VehicleId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            SalePrice = 33000.00m,
            SoldAt = DateTime.UtcNow,
            SalesPersonId = "SP-12345"
        };

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<VehicleSoldEvent>(json, _jsonOptions);

        // Assert
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.EventType.Should().Be("vehicle.sold");
        deserializedEvent.SalePrice.Should().Be(33000.00m);
    }

    #endregion

    #region Media Events

    [Fact]
    public void MediaUploadedEvent_Should_Serialize_And_Deserialize_Correctly()
    {
        // Arrange
        var originalEvent = new MediaUploadedEvent
        {
            MediaId = Guid.NewGuid(),
            FileName = "vehicle-photo.jpg",
            ContentType = "image/jpeg",
            FileSizeBytes = 2048576,
            UploadedBy = Guid.NewGuid(),
            UploadedAt = DateTime.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<MediaUploadedEvent>(json, _jsonOptions);

        // Assert
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.EventType.Should().Be("media.uploaded");
        deserializedEvent.FileName.Should().Be("vehicle-photo.jpg");
        deserializedEvent.FileSizeBytes.Should().Be(2048576);
    }

    [Fact]
    public void MediaProcessingFailedEvent_Should_Serialize_And_Deserialize_Correctly()
    {
        // Arrange
        var originalEvent = new MediaProcessingFailedEvent
        {
            MediaId = Guid.NewGuid(),
            ProcessingType = "Thumbnail",
            ErrorMessage = "Invalid image format",
            FailedAt = DateTime.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<MediaProcessingFailedEvent>(json, _jsonOptions);

        // Assert
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.EventType.Should().Be("media.processing.failed");
        deserializedEvent.ErrorMessage.Should().Be("Invalid image format");
    }

    #endregion

    #region Notification Events

    [Fact]
    public void NotificationSentEvent_Should_Serialize_And_Deserialize_Correctly()
    {
        // Arrange
        var originalEvent = new NotificationSentEvent
        {
            NotificationId = Guid.NewGuid(),
            Channel = "Email",
            Recipient = "customer@example.com",
            Subject = "Your order has been confirmed",
            SentAt = DateTime.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<NotificationSentEvent>(json, _jsonOptions);

        // Assert
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.EventType.Should().Be("notification.sent");
        deserializedEvent.Channel.Should().Be("Email");
    }

    [Fact]
    public void TeamsAlertSentEvent_Should_Serialize_And_Deserialize_Correctly()
    {
        // Arrange
        var originalEvent = new TeamsAlertSentEvent
        {
            AlertId = Guid.NewGuid(),
            WebhookUrl = "https://outlook.office.com/webhook/...",
            AlertType = "Critical",
            ServiceName = "ErrorService",
            SentAt = DateTime.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<TeamsAlertSentEvent>(json, _jsonOptions);

        // Assert
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.EventType.Should().Be("notification.teams.alert.sent");
        deserializedEvent.AlertType.Should().Be("Critical");
        deserializedEvent.ServiceName.Should().Be("ErrorService");
    }

    #endregion

    #region Audit Events

    [Fact]
    public void AuditLogCreatedEvent_Should_Serialize_And_Deserialize_Correctly()
    {
        // Arrange
        var originalEvent = new AuditLogCreatedEvent
        {
            AuditId = Guid.NewGuid(),
            ServiceName = "VehicleService",
            Action = "Update",
            EntityType = "Vehicle",
            EntityId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Changes = new Dictionary<string, object>
            {
                { "Price", 35000 },
                { "Status", "Available" }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<AuditLogCreatedEvent>(json, _jsonOptions);

        // Assert
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.EventType.Should().Be("audit.log.created");
        deserializedEvent.Action.Should().Be("Update");
        deserializedEvent.EntityType.Should().Be("Vehicle");
    }

    #endregion

    #region Base Event Properties

    [Fact]
    public void All_Events_Should_Have_Unique_EventId_And_OccurredAt()
    {
        // Arrange & Act
        var event1 = new UserRegisteredEvent { Email = "test1@example.com" };
        var event2 = new UserRegisteredEvent { Email = "test2@example.com" };

        // Assert
        event1.EventId.Should().NotBeEmpty();
        event2.EventId.Should().NotBeEmpty();
        event1.EventId.Should().NotBe(event2.EventId);
        event1.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void EventType_Should_Be_Consistent_For_Same_Event_Class()
    {
        // Arrange
        var event1 = new ErrorCriticalEvent();
        var event2 = new ErrorCriticalEvent();

        // Act & Assert
        event1.EventType.Should().Be("error.critical");
        event2.EventType.Should().Be("error.critical");
        event1.EventType.Should().Be(event2.EventType);
    }

    #endregion
}
