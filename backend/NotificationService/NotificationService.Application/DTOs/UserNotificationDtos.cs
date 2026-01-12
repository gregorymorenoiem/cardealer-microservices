namespace NotificationService.Application.DTOs;

/// <summary>
/// DTO for user notification displayed in UI
/// </summary>
public record UserNotificationDto(
    string Id,
    string UserId,
    string Type,
    string Title,
    string Message,
    string? Icon,
    string? Link,
    bool IsRead,
    string CreatedAt,
    string? ReadAt,
    string? ExpiresAt
);

/// <summary>
/// Paginated response for notifications list
/// </summary>
public record UserNotificationsListResponse(
    List<UserNotificationDto> Notifications,
    int Total,
    int UnreadCount,
    int Page,
    int PageSize,
    int TotalPages
);

/// <summary>
/// Unread count response
/// </summary>
public record UnreadCountResponse(int Count);

/// <summary>
/// Request to create a new user notification
/// </summary>
public record CreateUserNotificationRequest(
    string UserId,
    string Type,
    string Title,
    string Message,
    string? Icon = null,
    string? Link = null,
    string? DealerId = null,
    string? ExpiresAt = null
);

/// <summary>
/// Request to send bulk notifications
/// </summary>
public record BulkNotificationRequest(
    List<string> UserIds,
    string Type,
    string Title,
    string Message,
    string? Icon = null,
    string? Link = null
);
