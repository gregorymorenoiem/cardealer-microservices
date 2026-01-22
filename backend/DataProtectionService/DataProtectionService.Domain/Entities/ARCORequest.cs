namespace DataProtectionService.Domain.Entities;

/// <summary>
/// Tipo de solicitud ARCO según Ley 172-13
/// </summary>
public enum ARCOType
{
    /// <summary>
    /// Acceso: Derecho a conocer qué datos personales se tienen
    /// </summary>
    Access = 1,
    
    /// <summary>
    /// Rectificación: Derecho a corregir datos inexactos
    /// </summary>
    Rectification = 2,
    
    /// <summary>
    /// Cancelación: Derecho a eliminar datos personales
    /// </summary>
    Cancellation = 3,
    
    /// <summary>
    /// Oposición: Derecho a oponerse al tratamiento de datos
    /// </summary>
    Opposition = 4
}

/// <summary>
/// Estado de la solicitud ARCO
/// </summary>
public enum ARCOStatus
{
    Received = 1,
    IdentityVerification = 2,
    InProgress = 3,
    PendingInformation = 4,
    Completed = 5,
    Rejected = 6,
    Expired = 7
}

/// <summary>
/// Solicitud de ejercicio de derechos ARCO
/// Según Ley 172-13, debe responderse en máximo 30 días
/// </summary>
public class ARCORequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Número único de solicitud (ARCO-2026-00001)
    /// </summary>
    public string RequestNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Tipo de derecho ARCO ejercido
    /// </summary>
    public ARCOType Type { get; set; }
    
    /// <summary>
    /// Estado actual de la solicitud
    /// </summary>
    public ARCOStatus Status { get; set; } = ARCOStatus.Received;
    
    /// <summary>
    /// Descripción detallada de la solicitud
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Datos específicos solicitados (para Access/Rectification)
    /// </summary>
    public string? SpecificDataRequested { get; set; }
    
    /// <summary>
    /// Nuevos valores propuestos (para Rectification)
    /// </summary>
    public string? ProposedChanges { get; set; }
    
    /// <summary>
    /// Razón de oposición (para Opposition)
    /// </summary>
    public string? OppositionReason { get; set; }
    
    /// <summary>
    /// Fecha de recepción
    /// </summary>
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Fecha límite de respuesta (30 días según Ley 172-13)
    /// </summary>
    public DateTime Deadline { get; set; }
    
    /// <summary>
    /// Fecha de completado
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// ID del administrador que procesó la solicitud
    /// </summary>
    public Guid? ProcessedBy { get; set; }
    
    /// <summary>
    /// Nombre del procesador
    /// </summary>
    public string? ProcessedByName { get; set; }
    
    /// <summary>
    /// Resolución de la solicitud
    /// </summary>
    public string? Resolution { get; set; }
    
    /// <summary>
    /// Razón de rechazo (si aplica)
    /// </summary>
    public string? RejectionReason { get; set; }
    
    /// <summary>
    /// URL del archivo de datos exportados (para Access)
    /// </summary>
    public string? ExportFileUrl { get; set; }
    
    /// <summary>
    /// Hash del archivo exportado para verificar integridad
    /// </summary>
    public string? ExportFileHash { get; set; }
    
    /// <summary>
    /// IP del solicitante
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// User Agent del solicitante
    /// </summary>
    public string UserAgent { get; set; } = string.Empty;
    
    /// <summary>
    /// Notas internas del proceso
    /// </summary>
    public string? InternalNotes { get; set; }
    
    /// <summary>
    /// Adjuntos de la solicitud
    /// </summary>
    public List<ARCOAttachment> Attachments { get; set; } = new();
    
    /// <summary>
    /// Historial de cambios de estado
    /// </summary>
    public List<ARCOStatusHistory> StatusHistory { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Calcula si la solicitud está vencida
    /// </summary>
    public bool IsOverdue => Status != ARCOStatus.Completed && 
                             Status != ARCOStatus.Rejected && 
                             DateTime.UtcNow > Deadline;
    
    /// <summary>
    /// Días restantes para responder
    /// </summary>
    public int DaysRemaining => Math.Max(0, (Deadline - DateTime.UtcNow).Days);
}

/// <summary>
/// Adjunto de solicitud ARCO
/// </summary>
public class ARCOAttachment
{
    public Guid Id { get; set; }
    public Guid ARCORequestId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Description { get; set; }
    public Guid UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    public ARCORequest? ARCORequest { get; set; }
}

/// <summary>
/// Historial de cambios de estado de solicitud ARCO
/// </summary>
public class ARCOStatusHistory
{
    public Guid Id { get; set; }
    public Guid ARCORequestId { get; set; }
    public ARCOStatus OldStatus { get; set; }
    public ARCOStatus NewStatus { get; set; }
    public string? Comment { get; set; }
    public Guid ChangedBy { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    
    public ARCORequest? ARCORequest { get; set; }
}
