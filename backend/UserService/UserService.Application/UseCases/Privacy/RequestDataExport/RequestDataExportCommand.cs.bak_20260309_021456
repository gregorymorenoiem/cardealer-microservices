using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using UserService.Application.DTOs.Privacy;
using UserService.Domain.Entities.Privacy;

namespace UserService.Application.UseCases.Privacy.RequestDataExport;

/// <summary>
/// Command para solicitar exportación de datos (Portabilidad ARCO)
/// </summary>
public record RequestDataExportCommand(
    Guid UserId,
    ExportFormat Format,
    bool IncludeProfile,
    bool IncludeActivity,
    bool IncludeMessages,
    bool IncludeFavorites,
    bool IncludeTransactions,
    string? IpAddress,
    string? UserAgent
) : IRequest<DataExportRequestResponseDto>;

/// <summary>
/// Handler para RequestDataExportCommand
/// </summary>
public class RequestDataExportCommandHandler : IRequestHandler<RequestDataExportCommand, DataExportRequestResponseDto>
{
    public async Task<DataExportRequestResponseDto> Handle(RequestDataExportCommand request, CancellationToken cancellationToken)
    {
        // TODO: Guardar solicitud en base de datos y encolar job de procesamiento
        await Task.CompletedTask;
        
        var requestId = Guid.NewGuid();
        var estimatedCompletion = DateTime.UtcNow.AddHours(1);
        
        // Crear PrivacyRequest en DB
        // Encolar job para generar archivo
        
        return new DataExportRequestResponseDto(
            RequestId: requestId,
            Status: "Pending",
            Message: "Tu solicitud ha sido recibida. Te notificaremos por email cuando esté lista.",
            EstimatedCompletionTime: estimatedCompletion
        );
    }
}
