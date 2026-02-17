using MediatR;
using DataProtectionService.Application.DTOs;
using DataProtectionService.Application.Commands;
using DataProtectionService.Application.Queries;
using DataProtectionService.Domain.Entities;
using DataProtectionService.Domain.Interfaces;

namespace DataProtectionService.Application.Handlers;

// Query Handlers
public class GetDataExportQueryHandler : IRequestHandler<GetDataExportQuery, DataExportDto?>
{
    private readonly IDataExportRepository _repository;

    public GetDataExportQueryHandler(IDataExportRepository repository)
    {
        _repository = repository;
    }

    public async Task<DataExportDto?> Handle(GetDataExportQuery request, CancellationToken cancellationToken)
    {
        var export = await _repository.GetByIdAsync(request.ExportId, cancellationToken);
        if (export == null) return null;

        return new DataExportDto(
            export.Id,
            export.UserId,
            export.ARCORequestId,
            export.Status.ToString(),
            export.Format,
            export.IncludeTransactions,
            export.IncludeMessages,
            export.IncludeVehicleHistory,
            export.IncludeUserActivity,
            export.RequestedAt,
            export.CompletedAt,
            export.DownloadUrl,
            export.DownloadExpiresAt,
            export.FileSizeBytes,
            export.ErrorMessage
        );
    }
}

public class GetDataExportsByUserQueryHandler : IRequestHandler<GetDataExportsByUserQuery, List<DataExportDto>>
{
    private readonly IDataExportRepository _repository;

    public GetDataExportsByUserQueryHandler(IDataExportRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<DataExportDto>> Handle(GetDataExportsByUserQuery request, CancellationToken cancellationToken)
    {
        var exports = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
        
        return exports.Select(e => new DataExportDto(
            e.Id,
            e.UserId,
            e.ARCORequestId,
            e.Status.ToString(),
            e.Format,
            e.IncludeTransactions,
            e.IncludeMessages,
            e.IncludeVehicleHistory,
            e.IncludeUserActivity,
            e.RequestedAt,
            e.CompletedAt,
            e.DownloadUrl,
            e.DownloadExpiresAt,
            e.FileSizeBytes,
            e.ErrorMessage
        )).ToList();
    }
}

// Command Handlers
public class RequestDataExportCommandHandler : IRequestHandler<RequestDataExportCommand, DataExportDto>
{
    private readonly IDataExportRepository _repository;

    public RequestDataExportCommandHandler(IDataExportRepository repository)
    {
        _repository = repository;
    }

    public async Task<DataExportDto> Handle(RequestDataExportCommand request, CancellationToken cancellationToken)
    {
        var export = new DataExport
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ARCORequestId = request.ARCORequestId,
            Status = ExportStatus.Pending,
            Format = request.Format ?? "JSON",
            IncludeTransactions = request.IncludeTransactions ?? true,
            IncludeMessages = request.IncludeMessages ?? true,
            IncludeVehicleHistory = request.IncludeVehicleHistory ?? true,
            IncludeUserActivity = request.IncludeUserActivity ?? true,
            RequestedAt = DateTime.UtcNow,
            IpAddress = request.IpAddress ?? string.Empty,
            UserAgent = request.UserAgent ?? string.Empty
        };

        await _repository.CreateAsync(export, cancellationToken);

        return new DataExportDto(
            export.Id,
            export.UserId,
            export.ARCORequestId,
            export.Status.ToString(),
            export.Format,
            export.IncludeTransactions,
            export.IncludeMessages,
            export.IncludeVehicleHistory,
            export.IncludeUserActivity,
            export.RequestedAt,
            export.CompletedAt,
            export.DownloadUrl,
            export.DownloadExpiresAt,
            export.FileSizeBytes,
            export.ErrorMessage
        );
    }
}

public class CompleteDataExportCommandHandler : IRequestHandler<CompleteDataExportCommand, bool>
{
    private readonly IDataExportRepository _repository;

    public CompleteDataExportCommandHandler(IDataExportRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(CompleteDataExportCommand request, CancellationToken cancellationToken)
    {
        var export = await _repository.GetByIdAsync(request.ExportId, cancellationToken);
        if (export == null) return false;

        export.Status = request.Success ? ExportStatus.Completed : ExportStatus.Failed;
        export.CompletedAt = DateTime.UtcNow;
        export.DownloadUrl = request.DownloadUrl;
        export.DownloadExpiresAt = request.ExpiresAt;
        export.FileSizeBytes = request.FileSizeBytes;
        export.ErrorMessage = request.ErrorMessage;

        await _repository.UpdateAsync(export, cancellationToken);
        return true;
    }
}

public class AnonymizeUserDataCommandHandler : IRequestHandler<AnonymizeUserDataCommand, AnonymizationResultDto>
{
    private readonly IAnonymizationRecordRepository _repository;

    public AnonymizeUserDataCommandHandler(IAnonymizationRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<AnonymizationResultDto> Handle(AnonymizeUserDataCommand request, CancellationToken cancellationToken)
    {
        // Check if already anonymized
        var isAnonymized = await _repository.IsUserAnonymizedAsync(request.UserId, cancellationToken);
        if (isAnonymized)
        {
            return new AnonymizationResultDto(
                false,
                "User data is already anonymized",
                0,
                null
            );
        }

        var record = new AnonymizationRecord
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ARCORequestId = request.ARCORequestId,
            RequestedBy = request.ProcessedBy,
            AnonymizedAt = DateTime.UtcNow,
            Reason = request.Reason ?? string.Empty,
            OriginalEmail = request.OriginalEmail ?? string.Empty,
            OriginalPhone = request.OriginalPhone ?? string.Empty,
            AnonymizedEmail = $"anonymized_{Guid.NewGuid():N}@deleted.okla.com.do",
            AnonymizedPhone = "+1-000-000-0000",
            AffectedTables = new List<string> { "Users", "Vehicles", "Transactions", "Messages" },
            AffectedRecordsCount = 0,
            IsComplete = true
        };

        await _repository.CreateAsync(record, cancellationToken);

        return new AnonymizationResultDto(
            true,
            "User data anonymized successfully",
            record.AffectedRecordsCount,
            record.AnonymizedAt
        );
    }
}

public class GetAnonymizationRecordsQueryHandler : IRequestHandler<GetAnonymizationRecordsQuery, List<AnonymizationRecordDto>>
{
    private readonly IAnonymizationRecordRepository _repository;

    public GetAnonymizationRecordsQueryHandler(IAnonymizationRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AnonymizationRecordDto>> Handle(GetAnonymizationRecordsQuery request, CancellationToken cancellationToken)
    {
        var records = await _repository.GetAllAsync(request.FromDate, request.ToDate, cancellationToken);

        return records.Select(r => new AnonymizationRecordDto(
            r.Id,
            r.UserId,
            r.ARCORequestId,
            r.RequestedBy,
            r.AnonymizedAt,
            r.Reason,
            r.AffectedTables,
            r.AffectedRecordsCount,
            r.IsComplete
        )).ToList();
    }
}

public class CheckAnonymizationStatusQueryHandler : IRequestHandler<CheckAnonymizationStatusQuery, bool>
{
    private readonly IAnonymizationRecordRepository _repository;

    public CheckAnonymizationStatusQueryHandler(IAnonymizationRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(CheckAnonymizationStatusQuery request, CancellationToken cancellationToken)
    {
        return await _repository.IsUserAnonymizedAsync(request.UserId, cancellationToken);
    }
}
