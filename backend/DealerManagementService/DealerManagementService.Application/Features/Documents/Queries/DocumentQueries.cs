using DealerManagementService.Application.DTOs;
using DealerManagementService.Domain.Interfaces;
using MediatR;

namespace DealerManagementService.Application.Features.Documents.Queries;

// ============================================
// Get Dealer Documents Query
// ============================================
public record GetDealerDocumentsQuery(Guid DealerId) : IRequest<List<DealerDocumentDto>>;

public class GetDealerDocumentsQueryHandler : IRequestHandler<GetDealerDocumentsQuery, List<DealerDocumentDto>>
{
    private readonly IDealerDocumentRepository _documentRepository;

    public GetDealerDocumentsQueryHandler(IDealerDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<List<DealerDocumentDto>> Handle(GetDealerDocumentsQuery query, CancellationToken cancellationToken)
    {
        var documents = await _documentRepository.GetByDealerIdAsync(query.DealerId, cancellationToken);

        return documents
            .Where(d => !d.IsDeleted)
            .Select(d => new DealerDocumentDto(
                d.Id,
                d.DealerId,
                d.Type.ToString(),
                d.FileName,
                d.FileUrl,
                d.FileSizeBytes,
                d.MimeType,
                d.VerificationStatus.ToString(),
                d.VerifiedAt,
                d.RejectionReason,
                d.ExpiryDate,
                d.IsExpired,
                d.UploadedAt
            ))
            .ToList();
    }
}
