using MediatR;
using DataProtectionService.Application.DTOs;

namespace DataProtectionService.Application.Commands;

public record CreateARCORequestCommand(
    Guid UserId,
    string Type,
    string Description,
    string? SpecificDataRequested,
    string? ProposedChanges,
    string? OppositionReason,
    string? IpAddress
) : IRequest<ARCORequestDto>;

public record ProcessARCORequestCommand(
    Guid RequestId,
    Guid ProcessedById,
    string ProcessedByName,
    string Status,
    string? Resolution,
    string? RejectionReason,
    string? InternalNotes
) : IRequest<ARCORequestDto>;

public record AddARCOAttachmentCommand(
    Guid RequestId,
    string FileName,
    string FileUrl,
    string FileType,
    long FileSize,
    Guid UploadedById
) : IRequest<ARCOAttachmentDto>;

public record CompleteARCOExportCommand(
    Guid RequestId,
    string ExportFileUrl,
    Guid ProcessedById,
    string ProcessedByName
) : IRequest<ARCORequestDto>;

public record ExtendARCODeadlineCommand(
    Guid RequestId,
    int AdditionalDays,
    string Reason,
    Guid ExtendedById
) : IRequest<ARCORequestDto>;
