using MediatR;
using KYCService.Application.DTOs;
using KYCService.Domain.Entities;

namespace KYCService.Application.Commands;

/// <summary>
/// Comando para verificar identidad contra JCE
/// </summary>
public class VerifyIdentityWithJCECommand : IRequest<JCEVerificationResultDto>
{
    /// <summary>
    /// Número de cédula a verificar (formato: XXX-XXXXXXX-X)
    /// </summary>
    public required string CedulaNumber { get; set; }

    /// <summary>
    /// Nombre completo proporcionado por el usuario
    /// </summary>
    public string? ProvidedFullName { get; set; }

    /// <summary>
    /// Fecha de nacimiento proporcionada
    /// </summary>
    public DateTime? ProvidedDateOfBirth { get; set; }

    /// <summary>
    /// ID del usuario que realiza la verificación
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// ID de la sesión de verificación
    /// </summary>
    public Guid? SessionId { get; set; }
}

/// <summary>
/// Comando para procesar OCR de documento
/// </summary>
public class ProcessDocumentOCRCommand : IRequest<OCRProcessingResultDto>
{
    /// <summary>
    /// ID de la sesión de verificación
    /// </summary>
    public required Guid SessionId { get; set; }

    /// <summary>
    /// Datos binarios de la imagen
    /// </summary>
    public required byte[] ImageData { get; set; }

    /// <summary>
    /// Lado del documento (Front/Back)
    /// </summary>
    public required DocumentSide Side { get; set; }

    /// <summary>
    /// Tipo de documento
    /// </summary>
    public DocumentType DocumentType { get; set; } = DocumentType.Cedula;
}

/// <summary>
/// Comando para comparar rostros
/// </summary>
public class CompareFacesCommand : IRequest<FaceComparisonResultDto>
{
    /// <summary>
    /// ID de la sesión de verificación
    /// </summary>
    public required Guid SessionId { get; set; }

    /// <summary>
    /// Imagen del documento (con rostro)
    /// </summary>
    public required byte[] DocumentImage { get; set; }

    /// <summary>
    /// Imagen de la selfie
    /// </summary>
    public required byte[] SelfieImage { get; set; }

    /// <summary>
    /// Frames para verificación de liveness (opcional)
    /// </summary>
    public List<byte[]>? LivenessFrames { get; set; }

    /// <summary>
    /// Tipo de challenge de liveness
    /// </summary>
    public string? ChallengeType { get; set; }
}
