using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using UserService.Application.DTOs.Privacy;
using UserService.Domain.Entities.Privacy;
using UserService.Domain.Interfaces;

namespace UserService.Application.UseCases.Privacy.GetExportStatus;

/// <summary>
/// Query para obtener el estado de una exportación
/// </summary>
public record GetExportStatusQuery(Guid UserId) : IRequest<DataExportStatusDto?>;

/// <summary>
/// Handler para GetExportStatusQuery — Ley 172-13 Derecho de Acceso
/// Returns the latest export request status for a user.
/// </summary>
public class GetExportStatusQueryHandler : IRequestHandler<GetExportStatusQuery, DataExportStatusDto?>
{
    private readonly IPrivacyRequestRepository _privacyRepo;

    public GetExportStatusQueryHandler(IPrivacyRequestRepository privacyRepo)
    {
        _privacyRepo = privacyRepo;
    }

    public async Task<DataExportStatusDto?> Handle(GetExportStatusQuery request, CancellationToken cancellationToken)
    {
        var exportRequest = await _privacyRepo.GetLatestExportRequestAsync(request.UserId);
        if (exportRequest == null)
            return null;

        var fileSizeStr = exportRequest.FileSizeBytes.HasValue
            ? FormatFileSize(exportRequest.FileSizeBytes.Value)
            : null;

        return new DataExportStatusDto(
            RequestId: exportRequest.Id,
            Status: exportRequest.Status.ToString(),
            RequestedAt: exportRequest.CreatedAt,
            ReadyAt: exportRequest.CompletedAt,
            ExpiresAt: exportRequest.DownloadTokenExpiresAt,
            DownloadToken: exportRequest.Status == PrivacyRequestStatus.Completed
                ? exportRequest.DownloadToken
                : null,
            FileSize: fileSizeStr,
            Format: exportRequest.ExportFormat?.ToString() ?? "JSON"
        );
    }

    private static string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int i = 0;
        double size = bytes;
        while (size >= 1024 && i < suffixes.Length - 1)
        {
            size /= 1024;
            i++;
        }
        return $"{size:0.#} {suffixes[i]}";
    }
}
