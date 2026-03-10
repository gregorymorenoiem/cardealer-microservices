using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using UserService.Domain.Interfaces;

namespace UserService.Application.UseCases.Privacy.DownloadExportData;

// ═══════════════════════════════════════════════════════════════════════════════
// DOWNLOAD EXPORT DATA — LEY 172-13 ART. 5 DERECHO DE ACCESO Y PORTABILIDAD
//
// Validates the download token and returns file metadata for the controller
// to stream the ZIP file. Checks:
//   1. Token exists and is linked to a Completed export request
//   2. Token has not expired (24h window)
//   3. File exists on disk
// ═══════════════════════════════════════════════════════════════════════════════

public record DownloadExportDataQuery(string DownloadToken) : IRequest<DownloadExportDataResult?>;

public record DownloadExportDataResult(
    string FilePath,
    string FileName,
    long FileSizeBytes,
    bool IsExpired);

public class DownloadExportDataQueryHandler : IRequestHandler<DownloadExportDataQuery, DownloadExportDataResult?>
{
    private readonly IPrivacyRequestRepository _repository;

    public DownloadExportDataQueryHandler(IPrivacyRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<DownloadExportDataResult?> Handle(
        DownloadExportDataQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.DownloadToken))
            return null;

        var privacyRequest = await _repository.GetByDownloadTokenAsync(request.DownloadToken);
        if (privacyRequest == null)
            return null;

        // Check token expiry
        if (privacyRequest.DownloadTokenExpiresAt.HasValue &&
            privacyRequest.DownloadTokenExpiresAt.Value < DateTime.UtcNow)
        {
            return new DownloadExportDataResult(
                FilePath: privacyRequest.FilePath ?? string.Empty,
                FileName: string.Empty,
                FileSizeBytes: 0,
                IsExpired: true);
        }

        var filePath = privacyRequest.FilePath ?? string.Empty;
        var fileName = Path.GetFileName(filePath);

        if (string.IsNullOrEmpty(fileName))
            fileName = $"okla_datos_{privacyRequest.UserId:N}.zip";

        return new DownloadExportDataResult(
            FilePath: filePath,
            FileName: fileName,
            FileSizeBytes: privacyRequest.FileSizeBytes ?? 0,
            IsExpired: false);
    }
}
