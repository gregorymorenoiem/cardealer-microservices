using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs.Privacy;
using UserService.Domain.Entities.Privacy;
using UserService.Domain.Interfaces;

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
/// Handler para RequestDataExportCommand — Ley 172-13 Art. 5 Derecho de Acceso y Portabilidad
/// Persists the export request to DB. The DataExportWorker background service
/// processes pending requests asynchronously (ZIP generation in < 10 min).
/// </summary>
public class RequestDataExportCommandHandler : IRequestHandler<RequestDataExportCommand, DataExportRequestResponseDto>
{
    private readonly IPrivacyRequestRepository _privacyRepo;
    private readonly ILogger<RequestDataExportCommandHandler> _logger;

    public RequestDataExportCommandHandler(
        IPrivacyRequestRepository privacyRepo,
        ILogger<RequestDataExportCommandHandler> logger)
    {
        _privacyRepo = privacyRepo;
        _logger = logger;
    }

    public async Task<DataExportRequestResponseDto> Handle(RequestDataExportCommand request, CancellationToken cancellationToken)
    {
        // Check for existing pending export request (prevent duplicates)
        var hasPending = await _privacyRepo.HasPendingRequestAsync(request.UserId, PrivacyRequestType.Portability);
        if (hasPending)
        {
            var existingRequest = await _privacyRepo.GetLatestExportRequestAsync(request.UserId);
            return new DataExportRequestResponseDto(
                RequestId: existingRequest!.Id,
                Status: "Pending",
                Message: "Ya tienes una solicitud de exportación en proceso. Te notificaremos cuando esté lista.",
                EstimatedCompletionTime: existingRequest.CreatedAt.AddMinutes(10)
            );
        }

        // Build description of included data sections
        var sections = new List<string>();
        if (request.IncludeProfile) sections.Add("perfil");
        if (request.IncludeActivity) sections.Add("actividad");
        if (request.IncludeMessages) sections.Add("mensajes");
        if (request.IncludeFavorites) sections.Add("favoritos");
        if (request.IncludeTransactions) sections.Add("transacciones");

        var privacyRequest = new PrivacyRequest
        {
            UserId = request.UserId,
            Type = PrivacyRequestType.Portability,
            Status = PrivacyRequestStatus.Pending,
            ExportFormat = request.Format,
            Description = $"Exportación de datos: {string.Join(", ", sections)}",
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
        };

        await _privacyRepo.AddAsync(privacyRequest);

        _logger.LogInformation(
            "[DataExport] Export request created: RequestId={RequestId}, UserId={UserId}, Format={Format}, Sections=[{Sections}]",
            privacyRequest.Id, request.UserId, request.Format, string.Join(",", sections));

        return new DataExportRequestResponseDto(
            RequestId: privacyRequest.Id,
            Status: "Pending",
            Message: "Tu solicitud ha sido recibida. Recibirás un email con el enlace de descarga en menos de 10 minutos.",
            EstimatedCompletionTime: DateTime.UtcNow.AddMinutes(10)
        );
    }
}
