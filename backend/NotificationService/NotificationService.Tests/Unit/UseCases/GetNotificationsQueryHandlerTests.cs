using FluentAssertions;
using Moq;
using NotificationService.Application.DTOs;
using NotificationService.Application.UseCases.GetNotifications;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Interfaces.Repositories;
using ErrorService.Shared.Exceptions;
using Xunit;

namespace NotificationService.Tests.Unit.UseCases;

public class GetNotificationsQueryHandlerTests
{
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly GetNotificationsQueryHandler _handler;

    public GetNotificationsQueryHandlerTests()
    {
        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _handler = new GetNotificationsQueryHandler(_notificationRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsNotificationsSuccessfully()
    {
        // Arrange
        var notifications = new List<Notification>
        {
            CreateTestNotification("user1@test.com", NotificationType.Email, NotificationStatus.Sent),
            CreateTestNotification("user2@test.com", NotificationType.Email, NotificationStatus.Pending),
        };

        var request = new GetNotificationsRequest(Page: 1, PageSize: 10);
        var query = new GetNotificationsQuery(request);

        _notificationRepositoryMock
            .Setup(x => x.GetNotificationsWithPaginationAsync(request.Page, request.PageSize))
            .ReturnsAsync(notifications);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Notifications.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_FilterByRecipient_ReturnsMatchingNotifications()
    {
        // Arrange
        var notifications = new List<Notification>
        {
            CreateTestNotification("john@test.com", NotificationType.Email, NotificationStatus.Sent),
            CreateTestNotification("jane@test.com", NotificationType.Email, NotificationStatus.Sent),
            CreateTestNotification("john@company.com", NotificationType.Email, NotificationStatus.Pending),
        };

        var request = new GetNotificationsRequest(Recipient: "john");
        var query = new GetNotificationsQuery(request);

        _notificationRepositoryMock
            .Setup(x => x.GetNotificationsWithPaginationAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(notifications);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Notifications.Should().HaveCount(2);
        result.Notifications.Should().AllSatisfy(n => n.Recipient.Should().Contain("john"));
    }

    [Fact]
    public async Task Handle_FilterByType_ReturnsMatchingNotifications()
    {
        // Arrange
        var notifications = new List<Notification>
        {
            CreateTestNotification("user@test.com", NotificationType.Email, NotificationStatus.Sent),
            CreateTestNotification("device-token", NotificationType.Push, NotificationStatus.Sent),
            CreateTestNotification("phone-number", NotificationType.Sms, NotificationStatus.Sent),
        };

        var request = new GetNotificationsRequest(Type: "Email");
        var query = new GetNotificationsQuery(request);

        _notificationRepositoryMock
            .Setup(x => x.GetNotificationsWithPaginationAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(notifications);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Notifications.Should().HaveCount(1);
        result.Notifications.First().Type.Should().Be("Email");
    }

    [Fact]
    public async Task Handle_FilterByStatus_ReturnsMatchingNotifications()
    {
        // Arrange
        var notifications = new List<Notification>
        {
            CreateTestNotification("user1@test.com", NotificationType.Email, NotificationStatus.Sent),
            CreateTestNotification("user2@test.com", NotificationType.Email, NotificationStatus.Failed),
            CreateTestNotification("user3@test.com", NotificationType.Email, NotificationStatus.Pending),
        };

        var request = new GetNotificationsRequest(Status: "Failed");
        var query = new GetNotificationsQuery(request);

        _notificationRepositoryMock
            .Setup(x => x.GetNotificationsWithPaginationAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(notifications);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Notifications.Should().HaveCount(1);
        result.Notifications.First().Status.Should().Be("Failed");
    }

    [Fact]
    public async Task Handle_FilterByDateRange_ReturnsMatchingNotifications()
    {
        // Arrange
        var yesterday = DateTime.UtcNow.AddDays(-1);
        var today = DateTime.UtcNow;
        var tomorrow = DateTime.UtcNow.AddDays(1);

        var notifications = new List<Notification>
        {
            CreateTestNotificationWithDate("user1@test.com", yesterday.AddDays(-1)),
            CreateTestNotificationWithDate("user2@test.com", today),
            CreateTestNotificationWithDate("user3@test.com", tomorrow),
        };

        var request = new GetNotificationsRequest(From: yesterday, To: today.AddHours(1));
        var query = new GetNotificationsQuery(request);

        _notificationRepositoryMock
            .Setup(x => x.GetNotificationsWithPaginationAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(notifications);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Notifications.Should().HaveCount(1);
        result.Notifications.First().Recipient.Should().Be("user2@test.com");
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        // Arrange
        var request = new GetNotificationsRequest();
        var query = new GetNotificationsQuery(request);

        _notificationRepositoryMock
            .Setup(x => x.GetNotificationsWithPaginationAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Notification>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Notifications.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ThrowsServiceUnavailableException()
    {
        // Arrange
        var request = new GetNotificationsRequest();
        var query = new GetNotificationsQuery(request);

        _notificationRepositoryMock
            .Setup(x => x.GetNotificationsWithPaginationAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ServiceUnavailableException>()
            .WithMessage("*error occurred while retrieving notifications*");
    }

    [Fact]
    public async Task Handle_NotificationMapsToDto_CorrectlyMapped()
    {
        // Arrange
        var notification = CreateTestNotification("user@test.com", NotificationType.Email, NotificationStatus.Sent);
        notification.Subject = "Test Subject";
        notification.MarkAsSent();

        var notifications = new List<Notification> { notification };
        var request = new GetNotificationsRequest();
        var query = new GetNotificationsQuery(request);

        _notificationRepositoryMock
            .Setup(x => x.GetNotificationsWithPaginationAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(notifications);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.Notifications.First();
        dto.Id.Should().Be(notification.Id);
        dto.Type.Should().Be("Email");
        dto.Recipient.Should().Be("user@test.com");
        dto.Subject.Should().Be("Test Subject");
        dto.Status.Should().Be("Sent");
        dto.SentAt.Should().NotBeNull();
    }

    private static Notification CreateTestNotification(
        string recipient,
        NotificationType type,
        NotificationStatus status)
    {
        var notification = type switch
        {
            NotificationType.Email => Notification.CreateEmailNotification(recipient, "Subject", "Body"),
            NotificationType.Push => Notification.CreatePushNotification(recipient, "Title", "Body"),
            NotificationType.Sms => Notification.CreateSmsNotification(recipient, "Message"),
            _ => Notification.CreateEmailNotification(recipient, "Subject", "Body")
        };

        if (status == NotificationStatus.Sent)
            notification.MarkAsSent();
        else if (status == NotificationStatus.Failed)
            notification.MarkAsFailed("Test error");

        return notification;
    }

    private static Notification CreateTestNotificationWithDate(string recipient, DateTime createdAt)
    {
        var notification = Notification.CreateEmailNotification(recipient, "Subject", "Body");
        notification.CreatedAt = createdAt;
        return notification;
    }
}
