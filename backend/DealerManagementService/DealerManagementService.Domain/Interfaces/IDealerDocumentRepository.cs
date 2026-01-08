using DealerManagementService.Domain.Entities;

namespace DealerManagementService.Domain.Interfaces;

public interface IDealerDocumentRepository
{
    Task<DealerDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DealerDocument>> GetByDealerIdAsync(Guid dealerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DealerDocument>> GetByDealerIdAndTypeAsync(Guid dealerId, DocumentType type, CancellationToken cancellationToken = default);
    Task<DealerDocument> AddAsync(DealerDocument document, CancellationToken cancellationToken = default);
    Task UpdateAsync(DealerDocument document, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DealerDocument>> GetPendingVerificationAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<DealerDocument>> GetExpiredDocumentsAsync(CancellationToken cancellationToken = default);
}
