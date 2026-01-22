using RegulatoryAlertService.Domain.Common;
using RegulatoryAlertService.Domain.Enums;

namespace RegulatoryAlertService.Domain.Entities;

/// <summary>
/// Regulatory alert entity
/// Alertas sobre cambios regulatorios, fechas límite, y requisitos de cumplimiento
/// </summary>
public class RegulatoryAlert : EntityBase, IAggregateRoot
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? DetailedContent { get; private set; }
    
    public AlertType AlertType { get; private set; }
    public AlertPriority Priority { get; private set; }
    public AlertStatus Status { get; private set; } = AlertStatus.Draft;
    
    public RegulatoryBody RegulatoryBody { get; private set; }
    public RegulatoryCategory Category { get; private set; }
    
    public DateTime? EffectiveDate { get; private set; }
    public DateTime? DeadlineDate { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    
    public string? LegalReference { get; private set; }
    public string? OfficialDocumentUrl { get; private set; }
    public string? SourceUrl { get; private set; }
    
    public bool IsPublic { get; private set; } = true;
    public bool RequiresAction { get; private set; }
    public string? ActionRequired { get; private set; }
    
    public string? CreatedBy { get; private set; }
    public string? Tags { get; private set; }
    public string? MetadataJson { get; private set; }

    private readonly List<AlertNotification> _notifications = new();
    public IReadOnlyCollection<AlertNotification> Notifications => _notifications.AsReadOnly();

    private RegulatoryAlert() { }

    public RegulatoryAlert(
        string title,
        string description,
        AlertType alertType,
        AlertPriority priority,
        RegulatoryBody regulatoryBody,
        RegulatoryCategory category,
        string? createdBy = null)
    {
        Title = title;
        Description = description;
        AlertType = alertType;
        Priority = priority;
        RegulatoryBody = regulatoryBody;
        Category = category;
        CreatedBy = createdBy;
    }

    public void SetDeadline(DateTime deadline)
    {
        DeadlineDate = deadline;
        MarkAsUpdated();
    }

    public void SetEffectiveDate(DateTime effectiveDate)
    {
        EffectiveDate = effectiveDate;
        MarkAsUpdated();
    }

    public void SetLegalReference(string reference, string? documentUrl = null)
    {
        LegalReference = reference;
        OfficialDocumentUrl = documentUrl;
        MarkAsUpdated();
    }

    public void SetActionRequired(string action)
    {
        RequiresAction = true;
        ActionRequired = action;
        MarkAsUpdated();
    }

    public void Publish()
    {
        Status = AlertStatus.Active;
        MarkAsUpdated();
    }

    public void Acknowledge(string userId)
    {
        Status = AlertStatus.Acknowledged;
        MarkAsUpdated();
    }

    public void Resolve(string resolution)
    {
        Status = AlertStatus.Resolved;
        MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { Resolution = resolution, ResolvedAt = DateTime.UtcNow });
        MarkAsUpdated();
    }

    public void Escalate(string reason)
    {
        Status = AlertStatus.Escalated;
        Priority = AlertPriority.Critical;
        MarkAsUpdated();
    }

    public void AddNotification(AlertNotification notification)
    {
        _notifications.Add(notification);
        MarkAsUpdated();
    }

    public void SetTags(string tags)
    {
        Tags = tags;
        MarkAsUpdated();
    }
}

/// <summary>
/// Alert notification record
/// </summary>
public class AlertNotification : EntityBase
{
    public Guid RegulatoryAlertId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public NotificationChannel Channel { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public bool IsDelivered { get; private set; }
    public string? DeliveryError { get; private set; }

    public RegulatoryAlert? RegulatoryAlert { get; private set; }

    private AlertNotification() { }

    public AlertNotification(Guid alertId, string userId, NotificationChannel channel)
    {
        RegulatoryAlertId = alertId;
        UserId = userId;
        Channel = channel;
    }

    public void MarkAsSent()
    {
        SentAt = DateTime.UtcNow;
        IsDelivered = true;
        MarkAsUpdated();
    }

    public void MarkAsRead()
    {
        ReadAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void MarkAsFailed(string error)
    {
        IsDelivered = false;
        DeliveryError = error;
        MarkAsUpdated();
    }
}

/// <summary>
/// User subscription to regulatory alerts
/// </summary>
public class AlertSubscription : EntityBase, IAggregateRoot
{
    public string UserId { get; private set; } = string.Empty;
    public string? DealerId { get; private set; }
    
    public RegulatoryBody? RegulatoryBody { get; private set; }
    public RegulatoryCategory? Category { get; private set; }
    public AlertType? AlertType { get; private set; }
    public AlertPriority MinimumPriority { get; private set; } = AlertPriority.Low;
    
    public SubscriptionFrequency Frequency { get; private set; } = SubscriptionFrequency.Immediate;
    public NotificationChannel PreferredChannel { get; private set; } = NotificationChannel.Email;
    
    public bool IsActive { get; private set; } = true;
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? WebhookUrl { get; private set; }

    private AlertSubscription() { }

    public AlertSubscription(
        string userId,
        SubscriptionFrequency frequency,
        NotificationChannel preferredChannel,
        string? email = null)
    {
        UserId = userId;
        Frequency = frequency;
        PreferredChannel = preferredChannel;
        Email = email;
    }

    public void FilterByRegulatoryBody(RegulatoryBody body)
    {
        RegulatoryBody = body;
        MarkAsUpdated();
    }

    public void FilterByCategory(RegulatoryCategory category)
    {
        Category = category;
        MarkAsUpdated();
    }

    public void SetMinimumPriority(AlertPriority priority)
    {
        MinimumPriority = priority;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    public void UpdateContactInfo(string? email, string? phone)
    {
        Email = email;
        PhoneNumber = phone;
        MarkAsUpdated();
    }
}

/// <summary>
/// Regulatory calendar entry
/// </summary>
public class RegulatoryCalendarEntry : EntityBase, IAggregateRoot
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public RegulatoryBody RegulatoryBody { get; private set; }
    public RegulatoryCategory Category { get; private set; }
    
    public DateTime DueDate { get; private set; }
    public bool IsRecurring { get; private set; }
    public string? RecurrencePattern { get; private set; }
    
    public string? LegalBasis { get; private set; }
    public bool IsActive { get; private set; } = true;
    
    public int ReminderDaysBefore { get; private set; } = 7;

    private RegulatoryCalendarEntry() { }

    public RegulatoryCalendarEntry(
        string title,
        string description,
        RegulatoryBody regulatoryBody,
        RegulatoryCategory category,
        DateTime dueDate,
        bool isRecurring = false)
    {
        Title = title;
        Description = description;
        RegulatoryBody = regulatoryBody;
        Category = category;
        DueDate = dueDate;
        IsRecurring = isRecurring;
    }

    public void SetRecurrence(string pattern)
    {
        IsRecurring = true;
        RecurrencePattern = pattern;
        MarkAsUpdated();
    }

    public void SetReminder(int daysBefore)
    {
        ReminderDaysBefore = daysBefore;
        MarkAsUpdated();
    }

    public void UpdateDueDate(DateTime newDate)
    {
        DueDate = newDate;
        MarkAsUpdated();
    }
}

/// <summary>
/// Compliance deadline tracker
/// </summary>
public class ComplianceDeadline : EntityBase, IAggregateRoot
{
    public string UserId { get; private set; } = string.Empty;
    public string? DealerId { get; private set; }
    
    public Guid? CalendarEntryId { get; private set; }
    public Guid? AlertId { get; private set; }
    
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    
    public DateTime DueDate { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? CompletedBy { get; private set; }
    public string? CompletionNotes { get; private set; }
    
    public AlertPriority Priority { get; private set; } = AlertPriority.Medium;

    private ComplianceDeadline() { }

    public ComplianceDeadline(
        string userId,
        string title,
        DateTime dueDate,
        AlertPriority priority = AlertPriority.Medium)
    {
        UserId = userId;
        Title = title;
        DueDate = dueDate;
        Priority = priority;
    }

    public void Complete(string completedBy, string? notes = null)
    {
        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
        CompletedBy = completedBy;
        CompletionNotes = notes;
        MarkAsUpdated();
    }

    public void LinkToCalendar(Guid calendarEntryId)
    {
        CalendarEntryId = calendarEntryId;
        MarkAsUpdated();
    }

    public void LinkToAlert(Guid alertId)
    {
        AlertId = alertId;
        MarkAsUpdated();
    }
}
