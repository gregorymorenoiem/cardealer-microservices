using Microsoft.EntityFrameworkCore;
using IntegrationService.Domain.Entities;
using IntegrationService.Domain.Interfaces;
using IntegrationService.Infrastructure.Persistence;

namespace IntegrationService.Infrastructure.Repositories;

public class WebhookEventRepository : IWebhookEventRepository
{
    private readonly IntegrationDbContext _context;

    public WebhookEventRepository(IntegrationDbContext context)
    {
        _context = context;
    }

    public async Task<WebhookEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WebhookEvents
            .Include(e => e.Integration)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<WebhookEvent>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.WebhookEvents.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WebhookEvent>> GetByIntegrationIdAsync(Guid integrationId, CancellationToken cancellationToken = default)
    {
        return await _context.WebhookEvents
            .Where(e => e.IntegrationId == integrationId)
            .OrderByDescending(e => e.ReceivedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WebhookEvent>> GetByStatusAsync(WebhookStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.WebhookEvents.Where(e => e.Status == status).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WebhookEvent>> GetPendingRetryAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.WebhookEvents
            .Where(e => e.Status == WebhookStatus.Retrying && e.NextRetryAt <= now)
            .ToListAsync(cancellationToken);
    }

    public async Task<WebhookEvent> AddAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        await _context.WebhookEvents.AddAsync(webhookEvent, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return webhookEvent;
    }

    public async Task UpdateAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        _context.WebhookEvents.Update(webhookEvent);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var webhookEvent = await GetByIdAsync(id, cancellationToken);
        if (webhookEvent != null)
        {
            _context.WebhookEvents.Remove(webhookEvent);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WebhookEvents.AnyAsync(e => e.Id == id, cancellationToken);
    }
}
