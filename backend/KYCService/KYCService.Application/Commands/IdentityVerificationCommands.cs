using MediatR;
using KYCService.Application.DTOs;
using KYCService.Domain.Entities;

namespace KYCService.Application.Commands;

/// <summary>
/// Comando para iniciar una sesión de verificación de identidad
/// </summary>
public class StartIdentityVerificationCommand : IRequest<StartVerificationResponse>
{
    public Guid UserId { get; set; }
    public DocumentType DocumentType { get; set; } = DocumentType.Cedula;
    public DeviceInfoDto? DeviceInfo { get; set; }
    public LocationDto? Location { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

/// <summary>
/// Comando para procesar documento (frente o reverso)
/// </summary>
public class ProcessDocumentCommand : IRequest<DocumentProcessedResponse>
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public DocumentSide Side { get; set; }
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "image/jpeg";
}

/// <summary>
/// Comando para procesar selfie con liveness data
/// </summary>
public class ProcessSelfieCommand : IRequest<VerificationCompletedResponse>
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public byte[] SelfieImageData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "image/jpeg";
    public LivenessDataDto? LivenessData { get; set; }
}

/// <summary>
/// Comando para completar la verificación
/// </summary>
public class CompleteVerificationCommand : IRequest<VerificationCompletedResponse>
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
}

/// <summary>
/// Comando para reintentar verificación fallida
/// </summary>
public class RetryVerificationCommand : IRequest<StartVerificationResponse>
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
}

/// <summary>
/// Comando para cancelar sesión de verificación
/// </summary>
public class CancelVerificationCommand : IRequest<bool>
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public string? Reason { get; set; }
}
