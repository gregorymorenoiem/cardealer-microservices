using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using UserService.Application.DTOs.Privacy;

namespace UserService.Application.UseCases.Privacy.GetExportStatus;

/// <summary>
/// Query para obtener el estado de una exportación
/// </summary>
public record GetExportStatusQuery(Guid UserId) : IRequest<DataExportStatusDto?>;

/// <summary>
/// Handler para GetExportStatusQuery
/// </summary>
public class GetExportStatusQueryHandler : IRequestHandler<GetExportStatusQuery, DataExportStatusDto?>
{
    public async Task<DataExportStatusDto?> Handle(GetExportStatusQuery request, CancellationToken cancellationToken)
    {
        // TODO: Buscar última solicitud de exportación del usuario
        await Task.CompletedTask;
        
        // Retornar null si no hay solicitud pendiente
        return new DataExportStatusDto(
            RequestId: Guid.NewGuid(),
            Status: "Ready",
            RequestedAt: DateTime.UtcNow.AddHours(-1),
            ReadyAt: DateTime.UtcNow.AddMinutes(-30),
            ExpiresAt: DateTime.UtcNow.AddDays(7),
            DownloadToken: Guid.NewGuid().ToString("N"),
            FileSize: "2.4 MB",
            Format: "JSON"
        );
    }
}
