using MediatR;
using Vehicle360ProcessingService.Application.DTOs;
using Vehicle360ProcessingService.Domain.Entities;

namespace Vehicle360ProcessingService.Application.Features.Commands;

/// <summary>
/// Comando para iniciar el procesamiento 360 completo de un vehículo
/// </summary>
public class StartVehicle360ProcessingCommand : IRequest<StartProcessingResponse>
{
    public Guid UserId { get; set; }
    public Guid? TenantId { get; set; }
    public Guid VehicleId { get; set; }
    
    // Video puede venir como stream (upload directo) o URL (ya en S3)
    public Stream? VideoStream { get; set; }
    public string? VideoFileName { get; set; }
    public string? VideoContentType { get; set; }
    public long? VideoSize { get; set; }
    public string? VideoUrl { get; set; } // Si ya está en S3
    
    public int FrameCount { get; set; } = 6;
    public ProcessingOptions Options { get; set; } = new();
    
    public string? ClientIpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }
}

/// <summary>
/// Comando para procesar un job (ejecutado por el background worker)
/// </summary>
public class ProcessVehicle360JobCommand : IRequest<Vehicle360JobResponse>
{
    public Guid JobId { get; set; }
}

/// <summary>
/// Comando para reintentar un job fallido
/// </summary>
public class RetryVehicle360JobCommand : IRequest<Vehicle360JobResponse>
{
    public Guid JobId { get; set; }
    public Guid UserId { get; set; }
}

/// <summary>
/// Comando para cancelar un job en progreso
/// </summary>
public class CancelVehicle360JobCommand : IRequest<bool>
{
    public Guid JobId { get; set; }
    public Guid UserId { get; set; }
    public string? Reason { get; set; }
}
