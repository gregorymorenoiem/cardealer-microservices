using MarketingService.Domain.Entities;

namespace MarketingService.Domain.Interfaces;

public interface IEmailTemplateRepository
{
    Task<EmailTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<EmailTemplate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<EmailTemplate>> GetByTypeAsync(TemplateType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<EmailTemplate>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<IEnumerable<EmailTemplate>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<EmailTemplate?> GetDefaultByTypeAsync(TemplateType type, CancellationToken cancellationToken = default);
    Task<EmailTemplate> AddAsync(EmailTemplate template, CancellationToken cancellationToken = default);
    Task UpdateAsync(EmailTemplate template, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
