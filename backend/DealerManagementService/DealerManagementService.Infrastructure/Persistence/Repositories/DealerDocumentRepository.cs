using DealerManagementService.Domain.Entities;
using DealerManagementService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DealerManagementService.Infrastructure.Persistence.Repositories;

public class DealerDocumentRepository : IDealerDocumentRepository
{
    private readonly DealerDbContext _context;

    public DealerDocumentRepository(DealerDbContext context)
    {
        _context = context;
    }

    public async Task<DealerDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DealerDocuments.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<DealerDocument>> GetByDealerIdAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        return await _context.DealerDocuments
            .Where(d => d.DealerId == dealerId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DealerDocument>> GetByDealerIdAndTypeAsync(Guid dealerId, DocumentType type, CancellationToken cancellationToken = default)
    {
        return await _context.DealerDocuments
            .Where(d => d.DealerId == dealerId && d.Type == type)
            .ToListAsync(cancellationToken);
    }

    public async Task<DealerDocument> AddAsync(DealerDocument document, CancellationToken cancellationToken = default)
    {
        await _context.DealerDocuments.AddAsync(document, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return document;
    }

    public async Task UpdateAsync(DealerDocument document, CancellationToken cancellationToken = default)
    {
        _context.DealerDocuments.Update(document);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _context.DealerDocuments.FindAsync(new object[] { id }, cancellationToken);
        if (document != null)
        {
            document.IsDeleted = true;
            document.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<DealerDocument>> GetPendingVerificationAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DealerDocuments
            .Where(d => d.VerificationStatus == DocumentVerificationStatus.Pending ||
                       d.VerificationStatus == DocumentVerificationStatus.UnderReview)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DealerDocument>> GetExpiredDocumentsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.DealerDocuments
            .Where(d => d.ExpiryDate.HasValue && d.ExpiryDate.Value < now)
            .ToListAsync(cancellationToken);
    }
}
