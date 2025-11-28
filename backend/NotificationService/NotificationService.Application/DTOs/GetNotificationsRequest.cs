namespace NotificationService.Application.DTOs;


public record GetNotificationsRequest(
    string? Recipient = null,
    string? Type = null,
    string? Status = null,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = 50
);