using MediatR;
using DataProtectionService.Application.DTOs;
using DataProtectionService.Application.Commands;
using DataProtectionService.Domain.Entities;
using DataProtectionService.Domain.Interfaces;

namespace DataProtectionService.Application.Handlers;

public class CreateARCORequestCommandHandler : IRequestHandler<CreateARCORequestCommand, ARCORequestDto>
{
    private readonly IARCORequestRepository _repository;

    public CreateARCORequestCommandHandler(IARCORequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<ARCORequestDto> Handle(CreateARCORequestCommand request, CancellationToken cancellationToken)
    {
        var arcoType = Enum.Parse<ARCOType>(request.Type);
        
        var arcoRequest = new ARCORequest
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            RequestNumber = GenerateRequestNumber(arcoType),
            Type = arcoType,
            Status = ARCOStatus.Received,
            Description = request.Description,
            SpecificDataRequested = request.SpecificDataRequested,
            ProposedChanges = request.ProposedChanges,
            OppositionReason = request.OppositionReason,
            RequestedAt = DateTime.UtcNow,
            Deadline = DateTime.UtcNow.AddDays(30), // Ley 172-13: 30 días
            CreatedAt = DateTime.UtcNow,
            IpAddress = request.IpAddress ?? string.Empty,
            StatusHistory = new List<ARCOStatusHistory>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    OldStatus = ARCOStatus.Received,
                    NewStatus = ARCOStatus.Received,
                    Comment = "Solicitud recibida",
                    ChangedBy = request.UserId,
                    ChangedByName = "Sistema",
                    ChangedAt = DateTime.UtcNow
                }
            }
        };

        await _repository.AddAsync(arcoRequest, cancellationToken);

        return MapToDto(arcoRequest);
    }

    private static string GenerateRequestNumber(ARCOType type)
    {
        var prefix = type switch
        {
            ARCOType.Access => "ACC",
            ARCOType.Rectification => "REC",
            ARCOType.Cancellation => "CAN",
            ARCOType.Opposition => "OPO",
            _ => "ARQ"
        };
        return $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private static ARCORequestDto MapToDto(ARCORequest r) => new(
        r.Id,
        r.UserId,
        r.RequestNumber,
        r.Type.ToString(),
        r.Status.ToString(),
        r.Description,
        r.RequestedAt,
        r.Deadline,
        r.CompletedAt,
        r.ProcessedByName,
        r.Resolution,
        r.RejectionReason,
        r.ExportFileUrl,
        r.IsOverdue,
        r.DaysRemaining,
        r.Attachments?.Select(a => new ARCOAttachmentDto(
            a.Id, a.FileName, a.FileUrl, a.FileType, a.FileSize, a.UploadedAt
        )).ToList() ?? new(),
        r.StatusHistory?.Select(h => new ARCOStatusHistoryDto(
            h.OldStatus.ToString(), h.NewStatus.ToString(), h.Comment, h.ChangedByName, h.ChangedAt
        )).ToList() ?? new()
    );
}

public class ProcessARCORequestCommandHandler : IRequestHandler<ProcessARCORequestCommand, ARCORequestDto>
{
    private readonly IARCORequestRepository _repository;

    public ProcessARCORequestCommandHandler(IARCORequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<ARCORequestDto> Handle(ProcessARCORequestCommand request, CancellationToken cancellationToken)
    {
        var arcoRequest = await _repository.GetByIdAsync(request.RequestId, cancellationToken)
            ?? throw new InvalidOperationException($"ARCO Request {request.RequestId} not found");

        var oldStatus = arcoRequest.Status;
        var newStatus = Enum.Parse<ARCOStatus>(request.Status);

        arcoRequest.Status = newStatus;
        arcoRequest.ProcessedBy = request.ProcessedById;
        arcoRequest.ProcessedByName = request.ProcessedByName;
        arcoRequest.Resolution = request.Resolution;
        arcoRequest.RejectionReason = request.RejectionReason;
        arcoRequest.InternalNotes = request.InternalNotes;

        if (newStatus == ARCOStatus.Completed || newStatus == ARCOStatus.Rejected)
        {
            arcoRequest.CompletedAt = DateTime.UtcNow;
        }

        arcoRequest.StatusHistory ??= new List<ARCOStatusHistory>();
        arcoRequest.StatusHistory.Add(new ARCOStatusHistory
        {
            Id = Guid.NewGuid(),
            ARCORequestId = arcoRequest.Id,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Comment = request.InternalNotes,
            ChangedBy = request.ProcessedById,
            ChangedByName = request.ProcessedByName,
            ChangedAt = DateTime.UtcNow
        });

        await _repository.UpdateAsync(arcoRequest, cancellationToken);

        return new ARCORequestDto(
            arcoRequest.Id,
            arcoRequest.UserId,
            arcoRequest.RequestNumber,
            arcoRequest.Type.ToString(),
            arcoRequest.Status.ToString(),
            arcoRequest.Description,
            arcoRequest.RequestedAt,
            arcoRequest.Deadline,
            arcoRequest.CompletedAt,
            arcoRequest.ProcessedByName,
            arcoRequest.Resolution,
            arcoRequest.RejectionReason,
            arcoRequest.ExportFileUrl,
            arcoRequest.IsOverdue,
            arcoRequest.DaysRemaining,
            arcoRequest.Attachments?.Select(a => new ARCOAttachmentDto(
                a.Id, a.FileName, a.FileUrl, a.FileType, a.FileSize, a.UploadedAt
            )).ToList() ?? new(),
            arcoRequest.StatusHistory?.Select(h => new ARCOStatusHistoryDto(
                h.OldStatus.ToString(), h.NewStatus.ToString(), h.Comment, h.ChangedByName, h.ChangedAt
            )).ToList() ?? new()
        );
    }
}

public class AddARCOAttachmentCommandHandler : IRequestHandler<AddARCOAttachmentCommand, ARCOAttachmentDto>
{
    private readonly IARCORequestRepository _repository;

    public AddARCOAttachmentCommandHandler(IARCORequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<ARCOAttachmentDto> Handle(AddARCOAttachmentCommand request, CancellationToken cancellationToken)
    {
        var arcoRequest = await _repository.GetByIdAsync(request.RequestId, cancellationToken)
            ?? throw new InvalidOperationException($"ARCO Request {request.RequestId} not found");

        var attachment = new ARCOAttachment
        {
            Id = Guid.NewGuid(),
            ARCORequestId = request.RequestId,
            FileName = request.FileName,
            FileUrl = request.FileUrl,
            FileType = request.FileType,
            FileSize = request.FileSize,
            UploadedBy = request.UploadedById,
            UploadedAt = DateTime.UtcNow
        };

        arcoRequest.Attachments ??= new List<ARCOAttachment>();
        arcoRequest.Attachments.Add(attachment);

        await _repository.UpdateAsync(arcoRequest, cancellationToken);

        return new ARCOAttachmentDto(
            attachment.Id,
            attachment.FileName,
            attachment.FileUrl,
            attachment.FileType,
            attachment.FileSize,
            attachment.UploadedAt
        );
    }
}

public class CompleteARCOExportCommandHandler : IRequestHandler<CompleteARCOExportCommand, ARCORequestDto>
{
    private readonly IARCORequestRepository _repository;

    public CompleteARCOExportCommandHandler(IARCORequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<ARCORequestDto> Handle(CompleteARCOExportCommand request, CancellationToken cancellationToken)
    {
        var arcoRequest = await _repository.GetByIdAsync(request.RequestId, cancellationToken)
            ?? throw new InvalidOperationException($"ARCO Request {request.RequestId} not found");

        if (arcoRequest.Type != ARCOType.Access)
            throw new InvalidOperationException("Export only applies to Access requests");

        var oldStatus = arcoRequest.Status;
        arcoRequest.ExportFileUrl = request.ExportFileUrl;
        arcoRequest.Status = ARCOStatus.Completed;
        arcoRequest.CompletedAt = DateTime.UtcNow;
        arcoRequest.ProcessedBy = request.ProcessedById;
        arcoRequest.ProcessedByName = request.ProcessedByName;

        arcoRequest.StatusHistory ??= new List<ARCOStatusHistory>();
        arcoRequest.StatusHistory.Add(new ARCOStatusHistory
        {
            Id = Guid.NewGuid(),
            ARCORequestId = arcoRequest.Id,
            OldStatus = oldStatus,
            NewStatus = ARCOStatus.Completed,
            Comment = "Data export completed",
            ChangedBy = request.ProcessedById,
            ChangedByName = request.ProcessedByName,
            ChangedAt = DateTime.UtcNow
        });

        await _repository.UpdateAsync(arcoRequest, cancellationToken);

        return new ARCORequestDto(
            arcoRequest.Id,
            arcoRequest.UserId,
            arcoRequest.RequestNumber,
            arcoRequest.Type.ToString(),
            arcoRequest.Status.ToString(),
            arcoRequest.Description,
            arcoRequest.RequestedAt,
            arcoRequest.Deadline,
            arcoRequest.CompletedAt,
            arcoRequest.ProcessedByName,
            arcoRequest.Resolution,
            arcoRequest.RejectionReason,
            arcoRequest.ExportFileUrl,
            arcoRequest.IsOverdue,
            arcoRequest.DaysRemaining,
            new List<ARCOAttachmentDto>(),
            new List<ARCOStatusHistoryDto>()
        );
    }
}
