using Microsoft.EntityFrameworkCore;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Infrastructure.Persistence.Repositories;

public class WebhookEventRepository : IWebhookEventRepository
{
    private readonly SpyneDbContext _context;

    public WebhookEventRepository(SpyneDbContext context)
    {
        _context = context;
    }

    public async Task<SpyneWebhookEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WebhookEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<SpyneWebhookEvent?> GetBySpyneJobIdAsync(string spyneJobId, CancellationToken cancellationToken = default)
    {
        return await _context.WebhookEvents
            .AsNoTracking()
            .Where(x => x.SpyneJobId == spyneJobId)
            .OrderByDescending(x => x.ReceivedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<SpyneWebhookEvent>> GetUnprocessedAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _context.WebhookEvents
            .AsNoTracking()
            .Where(x => !x.IsProcessed)
            .OrderBy(x => x.ReceivedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<SpyneWebhookEvent> AddAsync(SpyneWebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        await _context.WebhookEvents.AddAsync(webhookEvent, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return webhookEvent;
    }

    public async Task<SpyneWebhookEvent> UpdateAsync(SpyneWebhookEvent webhookEvent, CancellationToken cancellationToken = default)
    {
        _context.WebhookEvents.Update(webhookEvent);
        await _context.SaveChangesAsync(cancellationToken);
        return webhookEvent;
    }
}
