using RegulatoryAlertService.Domain.Enums;

namespace RegulatoryAlertService.Application.DTOs;

// ===== ALERT DTOs =====

public record RegulatoryAlertDto(
    Guid Id,
    string Title,
    string Description,
    string? DetailedContent,
    AlertType AlertType,
    AlertPriority Priority,
    AlertStatus Status,
    RegulatoryBody RegulatoryBody,
    RegulatoryCategory Category,
    DateTime? EffectiveDate,
    DateTime? DeadlineDate,
    DateTime? ExpirationDate,
    string? LegalReference,
    string? OfficialDocumentUrl,
    bool RequiresAction,
    string? ActionRequired,
    string? Tags,
    DateTime CreatedAt);

public record RegulatoryAlertSummaryDto(
    Guid Id,
    string Title,
    AlertType AlertType,
    AlertPriority Priority,
    AlertStatus Status,
    RegulatoryBody RegulatoryBody,
    DateTime? DeadlineDate,
    bool RequiresAction);

public record CreateAlertDto(
    string Title,
    string Description,
    string? DetailedContent,
    AlertType AlertType,
    AlertPriority Priority,
    RegulatoryBody RegulatoryBody,
    RegulatoryCategory Category,
    DateTime? EffectiveDate,
    DateTime? DeadlineDate,
    string? LegalReference,
    string? OfficialDocumentUrl,
    string? ActionRequired,
    string? Tags,
    string? CreatedBy);

public record UpdateAlertDto(
    Guid Id,
    string? Title,
    string? Description,
    string? DetailedContent,
    AlertPriority? Priority,
    DateTime? EffectiveDate,
    DateTime? DeadlineDate,
    string? ActionRequired);

// ===== NOTIFICATION DTOs =====

public record AlertNotificationDto(
    Guid Id,
    Guid RegulatoryAlertId,
    string UserId,
    NotificationChannel Channel,
    DateTime? SentAt,
    DateTime? ReadAt,
    bool IsDelivered);

// ===== SUBSCRIPTION DTOs =====

public record AlertSubscriptionDto(
    Guid Id,
    string UserId,
    RegulatoryBody? RegulatoryBody,
    RegulatoryCategory? Category,
    AlertPriority MinimumPriority,
    SubscriptionFrequency Frequency,
    NotificationChannel PreferredChannel,
    bool IsActive,
    string? Email,
    string? PhoneNumber);

public record CreateSubscriptionDto(
    string UserId,
    RegulatoryBody? RegulatoryBody,
    RegulatoryCategory? Category,
    AlertPriority MinimumPriority,
    SubscriptionFrequency Frequency,
    NotificationChannel PreferredChannel,
    string? Email,
    string? PhoneNumber);

public record UpdateSubscriptionDto(
    Guid Id,
    RegulatoryBody? RegulatoryBody,
    RegulatoryCategory? Category,
    AlertPriority? MinimumPriority,
    SubscriptionFrequency? Frequency,
    NotificationChannel? PreferredChannel,
    string? Email,
    string? PhoneNumber,
    bool? IsActive);

// ===== CALENDAR DTOs =====

public record RegulatoryCalendarEntryDto(
    Guid Id,
    string Title,
    string Description,
    RegulatoryBody RegulatoryBody,
    RegulatoryCategory Category,
    DateTime DueDate,
    bool IsRecurring,
    string? RecurrencePattern,
    string? LegalBasis,
    int ReminderDaysBefore);

public record CreateCalendarEntryDto(
    string Title,
    string Description,
    RegulatoryBody RegulatoryBody,
    RegulatoryCategory Category,
    DateTime DueDate,
    bool IsRecurring,
    string? RecurrencePattern,
    string? LegalBasis,
    int ReminderDaysBefore);

// ===== DEADLINE DTOs =====

public record ComplianceDeadlineDto(
    Guid Id,
    string UserId,
    string Title,
    string? Description,
    DateTime DueDate,
    bool IsCompleted,
    DateTime? CompletedAt,
    AlertPriority Priority,
    bool IsOverdue);

public record CreateDeadlineDto(
    string UserId,
    string Title,
    string? Description,
    DateTime DueDate,
    AlertPriority Priority,
    Guid? CalendarEntryId,
    Guid? AlertId);

public record CompleteDeadlineDto(
    Guid Id,
    string CompletedBy,
    string? Notes);

// ===== STATISTICS DTOs =====

public record RegulatoryStatisticsDto(
    int TotalAlerts,
    int ActiveAlerts,
    int CriticalAlerts,
    int UpcomingDeadlines,
    int AlertsRequiringAction,
    int TotalSubscriptions,
    int TotalCalendarEntries,
    Dictionary<string, int> AlertsByBody,
    Dictionary<string, int> AlertsByPriority);

// ===== FILTER DTOs =====

public record AlertFilterDto(
    RegulatoryBody? RegulatoryBody,
    RegulatoryCategory? Category,
    AlertType? AlertType,
    AlertPriority? MinPriority,
    AlertStatus? Status,
    DateTime? FromDate,
    DateTime? ToDate,
    bool? RequiresAction,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20);
