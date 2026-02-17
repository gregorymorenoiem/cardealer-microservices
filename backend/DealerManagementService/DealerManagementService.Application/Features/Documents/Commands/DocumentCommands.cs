using DealerManagementService.Application.DTOs;
using DealerManagementService.Domain.Entities;
using DealerManagementService.Domain.Interfaces;
using MediatR;

namespace DealerManagementService.Application.Features.Documents.Commands;

// ============================================
// Upload Document Command
// ============================================
public record UploadDocumentCommand(
    Guid DealerId,
    string Type,
    string FileName,
    string FileUrl,
    string? FileKey,
    long FileSizeBytes,
    string MimeType,
    DateTime? ExpiryDate
) : IRequest<DealerDocumentDto>;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, DealerDocumentDto>
{
    private readonly IDealerDocumentRepository _documentRepository;
    private readonly IDealerRepository _dealerRepository;

    public UploadDocumentCommandHandler(
        IDealerDocumentRepository documentRepository,
        IDealerRepository dealerRepository)
    {
        _documentRepository = documentRepository;
        _dealerRepository = dealerRepository;
    }

    public async Task<DealerDocumentDto> Handle(UploadDocumentCommand command, CancellationToken cancellationToken)
    {
        // Verify dealer exists
        var dealer = await _dealerRepository.GetByIdAsync(command.DealerId, cancellationToken);
        if (dealer == null)
        {
            throw new KeyNotFoundException($"Dealer {command.DealerId} not found");
        }

        // Parse document type
        if (!Enum.TryParse<DocumentType>(command.Type, true, out var documentType))
        {
            throw new ArgumentException($"Invalid document type: {command.Type}");
        }

        // Check if a document of the same type already exists (replace it)
        var existingDocs = await _documentRepository.GetByDealerIdAndTypeAsync(
            command.DealerId, documentType, cancellationToken);
        
        foreach (var existing in existingDocs.Where(d => !d.IsDeleted))
        {
            existing.IsDeleted = true;
            existing.DeletedAt = DateTime.UtcNow;
            await _documentRepository.UpdateAsync(existing, cancellationToken);
        }

        var document = new DealerDocument
        {
            Id = Guid.NewGuid(),
            DealerId = command.DealerId,
            Type = documentType,
            FileName = command.FileName,
            FileUrl = command.FileUrl,
            FileKey = command.FileKey,
            FileSizeBytes = command.FileSizeBytes,
            MimeType = command.MimeType,
            VerificationStatus = DocumentVerificationStatus.Pending,
            ExpiryDate = command.ExpiryDate,
            UploadedAt = DateTime.UtcNow
        };

        var result = await _documentRepository.AddAsync(document, cancellationToken);

        return new DealerDocumentDto(
            result.Id,
            result.DealerId,
            result.Type.ToString(),
            result.FileName,
            result.FileUrl,
            result.FileSizeBytes,
            result.MimeType,
            result.VerificationStatus.ToString(),
            result.VerifiedAt,
            result.RejectionReason,
            result.ExpiryDate,
            result.IsExpired,
            result.UploadedAt
        );
    }
}

// ============================================
// Delete Document Command
// ============================================
public record DeleteDocumentCommand(Guid DealerId, Guid DocumentId) : IRequest<Unit>;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, Unit>
{
    private readonly IDealerDocumentRepository _documentRepository;

    public DeleteDocumentCommandHandler(IDealerDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<Unit> Handle(DeleteDocumentCommand command, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(command.DocumentId, cancellationToken);
        if (document == null || document.DealerId != command.DealerId)
        {
            throw new KeyNotFoundException($"Document {command.DocumentId} not found for dealer {command.DealerId}");
        }

        // Soft delete
        document.IsDeleted = true;
        document.DeletedAt = DateTime.UtcNow;
        await _documentRepository.UpdateAsync(document, cancellationToken);

        return Unit.Value;
    }
}
