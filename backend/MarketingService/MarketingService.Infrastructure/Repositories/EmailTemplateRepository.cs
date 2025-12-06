using Microsoft.EntityFrameworkCore;
using MarketingService.Domain.Entities;
using MarketingService.Domain.Interfaces;
using MarketingService.Infrastructure.Persistence;

namespace MarketingService.Infrastructure.Repositories;

public class EmailTemplateRepository : IEmailTemplateRepository
{
    private readonly MarketingDbContext _context;

    public EmailTemplateRepository(MarketingDbContext context)
    {
        _context = context;
    }

    public async Task<EmailTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<EmailTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates.OrderBy(t => t.Name).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EmailTemplate>> GetByTypeAsync(TemplateType type, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .Where(t => t.Type == type)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EmailTemplate>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .Where(t => t.Category == category)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EmailTemplate>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmailTemplate?> GetDefaultByTypeAsync(TemplateType type, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.Type == type && t.IsDefault, cancellationToken);
    }

    public async Task<EmailTemplate> AddAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        _context.EmailTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);
        return template;
    }

    public async Task UpdateAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        _context.EmailTemplates.Update(template);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await GetByIdAsync(id, cancellationToken);
        if (template != null)
        {
            _context.EmailTemplates.Remove(template);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates.AnyAsync(t => t.Id == id, cancellationToken);
    }
}
