using RegulatoryAlertService.Domain.Entities;
using RegulatoryAlertService.Domain.Enums;

namespace RegulatoryAlertService.Domain.Interfaces;

public interface IRegulatoryAlertRepository
{
    Task<RegulatoryAlert?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<RegulatoryAlert>> GetAllAsync(CancellationToken ct = default);
    Task<List<RegulatoryAlert>> GetActiveAsync(CancellationToken ct = default);
    Task<List<RegulatoryAlert>> GetByRegulatoryBodyAsync(RegulatoryBody body, CancellationToken ct = default);
    Task<List<RegulatoryAlert>> GetByCategoryAsync(RegulatoryCategory category, CancellationToken ct = default);
    Task<List<RegulatoryAlert>> GetByPriorityAsync(AlertPriority priority, CancellationToken ct = default);
    Task<List<RegulatoryAlert>> GetUpcomingDeadlinesAsync(int days, CancellationToken ct = default);
    Task<List<RegulatoryAlert>> GetRequiringActionAsync(CancellationToken ct = default);
    Task<RegulatoryAlert> AddAsync(RegulatoryAlert alert, CancellationToken ct = default);
    Task<RegulatoryAlert> UpdateAsync(RegulatoryAlert alert, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<int> GetActiveCountAsync(CancellationToken ct = default);
}

public interface IAlertNotificationRepository
{
    Task<AlertNotification?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<AlertNotification>> GetByAlertIdAsync(Guid alertId, CancellationToken ct = default);
    Task<List<AlertNotification>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<List<AlertNotification>> GetUnreadByUserAsync(string userId, CancellationToken ct = default);
    Task<List<AlertNotification>> GetPendingDeliveryAsync(CancellationToken ct = default);
    Task<AlertNotification> AddAsync(AlertNotification notification, CancellationToken ct = default);
    Task<AlertNotification> UpdateAsync(AlertNotification notification, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default);
}

public interface IAlertSubscriptionRepository
{
    Task<AlertSubscription?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<AlertSubscription>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<List<AlertSubscription>> GetActiveAsync(CancellationToken ct = default);
    Task<List<AlertSubscription>> GetMatchingSubscriptionsAsync(RegulatoryBody body, RegulatoryCategory category, AlertPriority priority, CancellationToken ct = default);
    Task<AlertSubscription> AddAsync(AlertSubscription subscription, CancellationToken ct = default);
    Task<AlertSubscription> UpdateAsync(AlertSubscription subscription, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IRegulatoryCalendarEntryRepository
{
    Task<RegulatoryCalendarEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<RegulatoryCalendarEntry>> GetAllAsync(CancellationToken ct = default);
    Task<List<RegulatoryCalendarEntry>> GetActiveAsync(CancellationToken ct = default);
    Task<List<RegulatoryCalendarEntry>> GetByMonthAsync(int year, int month, CancellationToken ct = default);
    Task<List<RegulatoryCalendarEntry>> GetUpcomingAsync(int days, CancellationToken ct = default);
    Task<List<RegulatoryCalendarEntry>> GetByRegulatoryBodyAsync(RegulatoryBody body, CancellationToken ct = default);
    Task<RegulatoryCalendarEntry> AddAsync(RegulatoryCalendarEntry entry, CancellationToken ct = default);
    Task<RegulatoryCalendarEntry> UpdateAsync(RegulatoryCalendarEntry entry, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IComplianceDeadlineRepository
{
    Task<ComplianceDeadline?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ComplianceDeadline>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<List<ComplianceDeadline>> GetPendingByUserAsync(string userId, CancellationToken ct = default);
    Task<List<ComplianceDeadline>> GetOverdueByUserAsync(string userId, CancellationToken ct = default);
    Task<List<ComplianceDeadline>> GetUpcomingAsync(string userId, int days, CancellationToken ct = default);
    Task<ComplianceDeadline> AddAsync(ComplianceDeadline deadline, CancellationToken ct = default);
    Task<ComplianceDeadline> UpdateAsync(ComplianceDeadline deadline, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<int> GetPendingCountAsync(string userId, CancellationToken ct = default);
    Task<int> GetOverdueCountAsync(string userId, CancellationToken ct = default);
}

public class RegulatoryAlertStatistics
{
    public int TotalAlerts { get; set; }
    public int ActiveAlerts { get; set; }
    public int CriticalAlerts { get; set; }
    public int UpcomingDeadlines { get; set; }
    public int AlertsRequiringAction { get; set; }
    public int TotalSubscriptions { get; set; }
    public int TotalCalendarEntries { get; set; }
    public Dictionary<RegulatoryBody, int> AlertsByBody { get; set; } = new();
    public Dictionary<AlertPriority, int> AlertsByPriority { get; set; } = new();
}
