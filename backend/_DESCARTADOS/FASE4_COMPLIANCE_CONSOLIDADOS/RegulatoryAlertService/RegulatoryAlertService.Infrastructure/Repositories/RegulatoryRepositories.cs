using Microsoft.EntityFrameworkCore;
using RegulatoryAlertService.Domain.Entities;
using RegulatoryAlertService.Domain.Enums;
using RegulatoryAlertService.Domain.Interfaces;
using RegulatoryAlertService.Infrastructure.Persistence;

namespace RegulatoryAlertService.Infrastructure.Repositories;

// ===== REGULATORY ALERT REPOSITORY =====

public class RegulatoryAlertRepository : IRegulatoryAlertRepository
{
    private readonly RegulatoryAlertDbContext _context;

    public RegulatoryAlertRepository(RegulatoryAlertDbContext context)
    {
        _context = context;
    }

    public async Task<RegulatoryAlert?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.RegulatoryAlerts
            .Include(a => a.Notifications)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<List<RegulatoryAlert>> GetAllAsync(CancellationToken ct = default)
        => await _context.RegulatoryAlerts
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

    public async Task<List<RegulatoryAlert>> GetActiveAsync(CancellationToken ct = default)
        => await _context.RegulatoryAlerts
            .Where(a => a.Status == AlertStatus.Active || a.Status == AlertStatus.Draft)
            .OrderByDescending(a => a.Priority)
            .ThenBy(a => a.DeadlineDate)
            .ToListAsync(ct);

    public async Task<List<RegulatoryAlert>> GetByRegulatoryBodyAsync(
        RegulatoryBody body, CancellationToken ct = default)
        => await _context.RegulatoryAlerts
            .Where(a => a.RegulatoryBody == body)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

    public async Task<List<RegulatoryAlert>> GetByCategoryAsync(
        RegulatoryCategory category, CancellationToken ct = default)
        => await _context.RegulatoryAlerts
            .Where(a => a.Category == category)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

    public async Task<List<RegulatoryAlert>> GetByPriorityAsync(
        AlertPriority priority, CancellationToken ct = default)
        => await _context.RegulatoryAlerts
            .Where(a => a.Priority == priority && a.Status == AlertStatus.Active)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

    public async Task<List<RegulatoryAlert>> GetUpcomingDeadlinesAsync(
        int days, CancellationToken ct = default)
    {
        var endDate = DateTime.UtcNow.AddDays(days);
        return await _context.RegulatoryAlerts
            .Where(a => a.DeadlineDate.HasValue &&
                        a.DeadlineDate.Value > DateTime.UtcNow &&
                        a.DeadlineDate.Value <= endDate &&
                        a.Status == AlertStatus.Active)
            .OrderBy(a => a.DeadlineDate)
            .ToListAsync(ct);
    }

    public async Task<List<RegulatoryAlert>> GetRequiringActionAsync(CancellationToken ct = default)
        => await _context.RegulatoryAlerts
            .Where(a => a.RequiresAction && a.Status == AlertStatus.Active)
            .OrderByDescending(a => a.Priority)
            .ThenBy(a => a.DeadlineDate)
            .ToListAsync(ct);

    public async Task<int> GetActiveCountAsync(CancellationToken ct = default)
        => await _context.RegulatoryAlerts
            .CountAsync(a => a.Status == AlertStatus.Active, ct);

    public async Task<RegulatoryAlert> AddAsync(RegulatoryAlert alert, CancellationToken ct = default)
    {
        await _context.RegulatoryAlerts.AddAsync(alert, ct);
        await _context.SaveChangesAsync(ct);
        return alert;
    }

    public async Task<RegulatoryAlert> UpdateAsync(RegulatoryAlert alert, CancellationToken ct = default)
    {
        _context.RegulatoryAlerts.Update(alert);
        await _context.SaveChangesAsync(ct);
        return alert;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var alert = await _context.RegulatoryAlerts.FindAsync(new object[] { id }, ct);
        if (alert != null)
        {
            _context.RegulatoryAlerts.Remove(alert);
            await _context.SaveChangesAsync(ct);
        }
    }
}

// ===== ALERT NOTIFICATION REPOSITORY =====

public class AlertNotificationRepository : IAlertNotificationRepository
{
    private readonly RegulatoryAlertDbContext _context;

    public AlertNotificationRepository(RegulatoryAlertDbContext context)
    {
        _context = context;
    }

    public async Task<AlertNotification?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.AlertNotifications.FindAsync(new object[] { id }, ct);

    public async Task<List<AlertNotification>> GetByAlertIdAsync(Guid alertId, CancellationToken ct = default)
        => await _context.AlertNotifications
            .Where(n => n.RegulatoryAlertId == alertId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);

    public async Task<List<AlertNotification>> GetByUserIdAsync(string userId, CancellationToken ct = default)
        => await _context.AlertNotifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);

    public async Task<List<AlertNotification>> GetUnreadByUserAsync(string userId, CancellationToken ct = default)
        => await _context.AlertNotifications
            .Where(n => n.UserId == userId && n.ReadAt == null)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);

    public async Task<List<AlertNotification>> GetPendingDeliveryAsync(CancellationToken ct = default)
        => await _context.AlertNotifications
            .Where(n => !n.IsDelivered)
            .OrderBy(n => n.CreatedAt)
            .ToListAsync(ct);

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default)
        => await _context.AlertNotifications
            .CountAsync(n => n.UserId == userId && n.ReadAt == null, ct);

    public async Task<AlertNotification> AddAsync(AlertNotification notification, CancellationToken ct = default)
    {
        await _context.AlertNotifications.AddAsync(notification, ct);
        await _context.SaveChangesAsync(ct);
        return notification;
    }

    public async Task<AlertNotification> UpdateAsync(AlertNotification notification, CancellationToken ct = default)
    {
        _context.AlertNotifications.Update(notification);
        await _context.SaveChangesAsync(ct);
        return notification;
    }
}

// ===== ALERT SUBSCRIPTION REPOSITORY =====

public class AlertSubscriptionRepository : IAlertSubscriptionRepository
{
    private readonly RegulatoryAlertDbContext _context;

    public AlertSubscriptionRepository(RegulatoryAlertDbContext context)
    {
        _context = context;
    }

    public async Task<AlertSubscription?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.AlertSubscriptions.FindAsync(new object[] { id }, ct);

    public async Task<List<AlertSubscription>> GetByUserIdAsync(string userId, CancellationToken ct = default)
        => await _context.AlertSubscriptions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

    public async Task<List<AlertSubscription>> GetActiveAsync(CancellationToken ct = default)
        => await _context.AlertSubscriptions
            .Where(s => s.IsActive)
            .ToListAsync(ct);

    public async Task<List<AlertSubscription>> GetMatchingSubscriptionsAsync(
        RegulatoryBody body, RegulatoryCategory category, AlertPriority priority, CancellationToken ct = default)
        => await _context.AlertSubscriptions
            .Where(s => s.IsActive &&
                       (!s.RegulatoryBody.HasValue || s.RegulatoryBody == body) &&
                       (!s.Category.HasValue || s.Category == category) &&
                       priority >= s.MinimumPriority)
            .ToListAsync(ct);

    public async Task<AlertSubscription> AddAsync(AlertSubscription subscription, CancellationToken ct = default)
    {
        await _context.AlertSubscriptions.AddAsync(subscription, ct);
        await _context.SaveChangesAsync(ct);
        return subscription;
    }

    public async Task<AlertSubscription> UpdateAsync(AlertSubscription subscription, CancellationToken ct = default)
    {
        _context.AlertSubscriptions.Update(subscription);
        await _context.SaveChangesAsync(ct);
        return subscription;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var subscription = await _context.AlertSubscriptions.FindAsync(new object[] { id }, ct);
        if (subscription != null)
        {
            _context.AlertSubscriptions.Remove(subscription);
            await _context.SaveChangesAsync(ct);
        }
    }
}

// ===== REGULATORY CALENDAR ENTRY REPOSITORY =====

public class RegulatoryCalendarEntryRepository : IRegulatoryCalendarEntryRepository
{
    private readonly RegulatoryAlertDbContext _context;

    public RegulatoryCalendarEntryRepository(RegulatoryAlertDbContext context)
    {
        _context = context;
    }

    public async Task<RegulatoryCalendarEntry?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.RegulatoryCalendarEntries.FindAsync(new object[] { id }, ct);

    public async Task<List<RegulatoryCalendarEntry>> GetAllAsync(CancellationToken ct = default)
        => await _context.RegulatoryCalendarEntries
            .OrderBy(e => e.DueDate)
            .ToListAsync(ct);

    public async Task<List<RegulatoryCalendarEntry>> GetActiveAsync(CancellationToken ct = default)
        => await _context.RegulatoryCalendarEntries
            .Where(e => e.IsActive)
            .OrderBy(e => e.DueDate)
            .ToListAsync(ct);

    public async Task<List<RegulatoryCalendarEntry>> GetByMonthAsync(int year, int month, CancellationToken ct = default)
        => await _context.RegulatoryCalendarEntries
            .Where(e => e.IsActive && e.DueDate.Year == year && e.DueDate.Month == month)
            .OrderBy(e => e.DueDate)
            .ToListAsync(ct);

    public async Task<List<RegulatoryCalendarEntry>> GetUpcomingAsync(int days, CancellationToken ct = default)
    {
        var endDate = DateTime.UtcNow.AddDays(days);
        return await _context.RegulatoryCalendarEntries
            .Where(e => e.IsActive && e.DueDate >= DateTime.UtcNow && e.DueDate <= endDate)
            .OrderBy(e => e.DueDate)
            .ToListAsync(ct);
    }

    public async Task<List<RegulatoryCalendarEntry>> GetByRegulatoryBodyAsync(
        RegulatoryBody body, CancellationToken ct = default)
        => await _context.RegulatoryCalendarEntries
            .Where(e => e.RegulatoryBody == body && e.IsActive)
            .OrderBy(e => e.DueDate)
            .ToListAsync(ct);

    public async Task<RegulatoryCalendarEntry> AddAsync(RegulatoryCalendarEntry entry, CancellationToken ct = default)
    {
        await _context.RegulatoryCalendarEntries.AddAsync(entry, ct);
        await _context.SaveChangesAsync(ct);
        return entry;
    }

    public async Task<RegulatoryCalendarEntry> UpdateAsync(RegulatoryCalendarEntry entry, CancellationToken ct = default)
    {
        _context.RegulatoryCalendarEntries.Update(entry);
        await _context.SaveChangesAsync(ct);
        return entry;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entry = await _context.RegulatoryCalendarEntries.FindAsync(new object[] { id }, ct);
        if (entry != null)
        {
            _context.RegulatoryCalendarEntries.Remove(entry);
            await _context.SaveChangesAsync(ct);
        }
    }
}

// ===== COMPLIANCE DEADLINE REPOSITORY =====

public class ComplianceDeadlineRepository : IComplianceDeadlineRepository
{
    private readonly RegulatoryAlertDbContext _context;

    public ComplianceDeadlineRepository(RegulatoryAlertDbContext context)
    {
        _context = context;
    }

    public async Task<ComplianceDeadline?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.ComplianceDeadlines.FindAsync(new object[] { id }, ct);

    public async Task<List<ComplianceDeadline>> GetByUserIdAsync(string userId, CancellationToken ct = default)
        => await _context.ComplianceDeadlines
            .Where(d => d.UserId == userId)
            .OrderBy(d => d.DueDate)
            .ToListAsync(ct);

    public async Task<List<ComplianceDeadline>> GetPendingByUserAsync(string userId, CancellationToken ct = default)
        => await _context.ComplianceDeadlines
            .Where(d => d.UserId == userId && !d.IsCompleted)
            .OrderBy(d => d.DueDate)
            .ToListAsync(ct);

    public async Task<List<ComplianceDeadline>> GetOverdueByUserAsync(string userId, CancellationToken ct = default)
        => await _context.ComplianceDeadlines
            .Where(d => d.UserId == userId && !d.IsCompleted && d.DueDate < DateTime.UtcNow)
            .OrderBy(d => d.DueDate)
            .ToListAsync(ct);

    public async Task<List<ComplianceDeadline>> GetUpcomingAsync(string userId, int days, CancellationToken ct = default)
    {
        var endDate = DateTime.UtcNow.AddDays(days);
        return await _context.ComplianceDeadlines
            .Where(d => d.UserId == userId && !d.IsCompleted && d.DueDate >= DateTime.UtcNow && d.DueDate <= endDate)
            .OrderBy(d => d.DueDate)
            .ToListAsync(ct);
    }

    public async Task<int> GetPendingCountAsync(string userId, CancellationToken ct = default)
        => await _context.ComplianceDeadlines
            .CountAsync(d => d.UserId == userId && !d.IsCompleted, ct);

    public async Task<int> GetOverdueCountAsync(string userId, CancellationToken ct = default)
        => await _context.ComplianceDeadlines
            .CountAsync(d => d.UserId == userId && !d.IsCompleted && d.DueDate < DateTime.UtcNow, ct);

    public async Task<ComplianceDeadline> AddAsync(ComplianceDeadline deadline, CancellationToken ct = default)
    {
        await _context.ComplianceDeadlines.AddAsync(deadline, ct);
        await _context.SaveChangesAsync(ct);
        return deadline;
    }

    public async Task<ComplianceDeadline> UpdateAsync(ComplianceDeadline deadline, CancellationToken ct = default)
    {
        _context.ComplianceDeadlines.Update(deadline);
        await _context.SaveChangesAsync(ct);
        return deadline;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var deadline = await _context.ComplianceDeadlines.FindAsync(new object[] { id }, ct);
        if (deadline != null)
        {
            _context.ComplianceDeadlines.Remove(deadline);
            await _context.SaveChangesAsync(ct);
        }
    }
}
