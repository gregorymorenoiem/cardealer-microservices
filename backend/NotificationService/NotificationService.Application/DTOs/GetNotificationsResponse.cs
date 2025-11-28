using System.Collections.Generic;

namespace NotificationService.Application.DTOs;

public record GetNotificationsResponse(
    List<NotificationItemDto> Notifications,
    int TotalCount,
    int Page,
    int PageSize
);

public record NotificationItemDto(
    Guid Id,
    string Type,
    string Recipient,
    string Subject,
    string Status,
    DateTime CreatedAt,
    DateTime? SentAt,
    string? ErrorMessage
);