// =====================================================
// C12: ComplianceIntegrationService - Entidades de Dominio
// Integración con entes reguladores RD
// =====================================================

using ComplianceIntegrationService.Domain.Common;
using ComplianceIntegrationService.Domain.Enums;

namespace ComplianceIntegrationService.Domain.Entities;

/// <summary>
/// Configuración de integración con un ente regulador.
/// Define cómo conectarse y transmitir datos a cada entidad gubernamental.
/// </summary>
public class IntegrationConfig : EntityBase
{
    /// <summary>
    /// Nombre descriptivo de la integración
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada de la integración
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Ente regulador destino
    /// </summary>
    public RegulatoryBody RegulatoryBody { get; set; }

    /// <summary>
    /// Tipo de integración técnica
    /// </summary>
    public IntegrationType IntegrationType { get; set; }

    /// <summary>
    /// Estado actual de la integración
    /// </summary>
    public IntegrationStatus Status { get; set; } = IntegrationStatus.Configuring;

    /// <summary>
    /// URL base del endpoint (API, Web Service, SFTP, etc.)
    /// </summary>
    public string? EndpointUrl { get; set; }

    /// <summary>
    /// URL de ambiente de pruebas/sandbox
    /// </summary>
    public string? SandboxUrl { get; set; }

    /// <summary>
    /// Puerto para conexión (si aplica)
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// Indica si está en modo sandbox/pruebas
    /// </summary>
    public bool IsSandboxMode { get; set; } = true;

    /// <summary>
    /// Frecuencia de sincronización configurada
    /// </summary>
    public SyncFrequency SyncFrequency { get; set; } = SyncFrequency.Daily;

    /// <summary>
    /// Hora programada para sincronización (formato HH:mm)
    /// </summary>
    public string? ScheduledTime { get; set; }

    /// <summary>
    /// Días de la semana para sincronización (1=Lun, 7=Dom)
    /// </summary>
    public string? ScheduledDays { get; set; }

    /// <summary>
    /// Timeout en segundos para conexiones
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Número máximo de reintentos en caso de fallo
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Intervalo entre reintentos en segundos
    /// </summary>
    public int RetryIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Indica si requiere certificado SSL/TLS
    /// </summary>
    public bool RequiresSsl { get; set; } = true;

    /// <summary>
    /// Versión del protocolo/API
    /// </summary>
    public string? ProtocolVersion { get; set; }

    /// <summary>
    /// Configuración adicional en formato JSON
    /// </summary>
    public string? AdditionalConfig { get; set; }

    /// <summary>
    /// Notas o comentarios sobre la integración
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Última fecha de sincronización exitosa
    /// </summary>
    public DateTime? LastSuccessfulSync { get; set; }

    /// <summary>
    /// Última fecha de error en sincronización
    /// </summary>
    public DateTime? LastFailedSync { get; set; }

    /// <summary>
    /// Contador de errores consecutivos
    /// </summary>
    public int ConsecutiveErrors { get; set; }

    // Navegación
    public virtual ICollection<IntegrationCredential> Credentials { get; set; } = new List<IntegrationCredential>();
    public virtual ICollection<DataTransmission> Transmissions { get; set; } = new List<DataTransmission>();
    public virtual ICollection<IntegrationLog> Logs { get; set; } = new List<IntegrationLog>();
}

/// <summary>
/// Credenciales de autenticación para una integración.
/// Almacena de forma segura las credenciales para cada ente regulador.
/// </summary>
public class IntegrationCredential : EntityBase
{
    /// <summary>
    /// ID de la configuración de integración
    /// </summary>
    public Guid IntegrationConfigId { get; set; }

    /// <summary>
    /// Nombre descriptivo de la credencial
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de credencial
    /// </summary>
    public CredentialType CredentialType { get; set; }

    /// <summary>
    /// Usuario o Client ID (encriptado)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Contraseña o Client Secret (encriptado)
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// API Key o Token (encriptado)
    /// </summary>
    public string? ApiKeyHash { get; set; }

    /// <summary>
    /// Certificado en formato Base64 (encriptado)
    /// </summary>
    public string? CertificateData { get; set; }

    /// <summary>
    /// Thumbprint del certificado
    /// </summary>
    public string? CertificateThumbprint { get; set; }

    /// <summary>
    /// Fecha de expiración del certificado/token
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Indica si es la credencial principal/activa
    /// </summary>
    public bool IsPrimary { get; set; } = true;

    /// <summary>
    /// Ambiente: Production, Sandbox, Development
    /// </summary>
    public string Environment { get; set; } = "Sandbox";

    /// <summary>
    /// Notas sobre la credencial
    /// </summary>
    public string? Notes { get; set; }

    // Navegación
    public virtual IntegrationConfig? IntegrationConfig { get; set; }
}

/// <summary>
/// Registro de transmisión de datos hacia/desde ente regulador.
/// Almacena cada envío o recepción de información.
/// </summary>
public class DataTransmission : EntityBase
{
    /// <summary>
    /// ID de la configuración de integración
    /// </summary>
    public Guid IntegrationConfigId { get; set; }

    /// <summary>
    /// Identificador único de la transmisión (referencia externa)
    /// </summary>
    public string TransmissionCode { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de reporte transmitido
    /// </summary>
    public ReportType ReportType { get; set; }

    /// <summary>
    /// Dirección de la transmisión
    /// </summary>
    public DataDirection Direction { get; set; } = DataDirection.Outbound;

    /// <summary>
    /// Estado de la transmisión
    /// </summary>
    public TransmissionStatus Status { get; set; } = TransmissionStatus.Pending;

    /// <summary>
    /// Período del reporte (inicio)
    /// </summary>
    public DateTime? PeriodStart { get; set; }

    /// <summary>
    /// Período del reporte (fin)
    /// </summary>
    public DateTime? PeriodEnd { get; set; }

    /// <summary>
    /// Nombre del archivo transmitido
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Tamaño del archivo en bytes
    /// </summary>
    public long? FileSizeBytes { get; set; }

    /// <summary>
    /// Hash del archivo para verificación de integridad
    /// </summary>
    public string? FileHash { get; set; }

    /// <summary>
    /// Número de registros transmitidos
    /// </summary>
    public int RecordCount { get; set; }

    /// <summary>
    /// Fecha/hora de inicio de transmisión
    /// </summary>
    public DateTime? TransmissionStartedAt { get; set; }

    /// <summary>
    /// Fecha/hora de fin de transmisión
    /// </summary>
    public DateTime? TransmissionCompletedAt { get; set; }

    /// <summary>
    /// Número de confirmación del ente regulador
    /// </summary>
    public string? ConfirmationNumber { get; set; }

    /// <summary>
    /// Respuesta del ente regulador
    /// </summary>
    public string? ResponseData { get; set; }

    /// <summary>
    /// Código de respuesta HTTP o equivalente
    /// </summary>
    public int? ResponseCode { get; set; }

    /// <summary>
    /// Mensaje de error si falló
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Detalle técnico del error
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// Número de intentos realizados
    /// </summary>
    public int AttemptCount { get; set; } = 1;

    /// <summary>
    /// Próximo intento programado
    /// </summary>
    public DateTime? NextRetryAt { get; set; }

    /// <summary>
    /// Usuario que inició la transmisión
    /// </summary>
    public Guid? InitiatedByUserId { get; set; }

    /// <summary>
    /// Notas adicionales
    /// </summary>
    public string? Notes { get; set; }

    // Navegación
    public virtual IntegrationConfig? IntegrationConfig { get; set; }
}

/// <summary>
/// Mapeo de campos entre sistema interno y formato del ente regulador.
/// Define cómo transformar los datos para cada integración.
/// </summary>
public class FieldMapping : EntityBase
{
    /// <summary>
    /// ID de la configuración de integración
    /// </summary>
    public Guid IntegrationConfigId { get; set; }

    /// <summary>
    /// Tipo de reporte al que aplica el mapeo
    /// </summary>
    public ReportType ReportType { get; set; }

    /// <summary>
    /// Nombre del campo en el sistema interno
    /// </summary>
    public string SourceField { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del campo en el formato del ente regulador
    /// </summary>
    public string TargetField { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de dato del campo origen
    /// </summary>
    public string? SourceDataType { get; set; }

    /// <summary>
    /// Tipo de dato del campo destino
    /// </summary>
    public string? TargetDataType { get; set; }

    /// <summary>
    /// Transformación a aplicar (ej: "ToUpper", "FormatDate:yyyy-MM-dd")
    /// </summary>
    public string? Transformation { get; set; }

    /// <summary>
    /// Valor por defecto si el origen es nulo
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Indica si el campo es requerido
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Longitud máxima del campo
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Formato de validación (regex)
    /// </summary>
    public string? ValidationPattern { get; set; }

    /// <summary>
    /// Orden del campo en el archivo/mensaje
    /// </summary>
    public int FieldOrder { get; set; }

    /// <summary>
    /// Descripción del mapeo
    /// </summary>
    public string? Description { get; set; }

    // Navegación
    public virtual IntegrationConfig? IntegrationConfig { get; set; }
}

/// <summary>
/// Log de eventos de integración para auditoría y debugging.
/// </summary>
public class IntegrationLog : EntityBase
{
    /// <summary>
    /// ID de la configuración de integración
    /// </summary>
    public Guid IntegrationConfigId { get; set; }

    /// <summary>
    /// ID de transmisión relacionada (si aplica)
    /// </summary>
    public Guid? DataTransmissionId { get; set; }

    /// <summary>
    /// Severidad del log
    /// </summary>
    public LogSeverity Severity { get; set; } = LogSeverity.Info;

    /// <summary>
    /// Categoría o componente que generó el log
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Mensaje del log
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Detalles adicionales o stack trace
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Request enviado (para debugging)
    /// </summary>
    public string? RequestData { get; set; }

    /// <summary>
    /// Response recibido (para debugging)
    /// </summary>
    public string? ResponseData { get; set; }

    /// <summary>
    /// Duración de la operación en milisegundos
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// IP del servidor/cliente
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Correlation ID para trazabilidad
    /// </summary>
    public string? CorrelationId { get; set; }

    // Navegación
    public virtual IntegrationConfig? IntegrationConfig { get; set; }
}

/// <summary>
/// Configuración de webhooks para recibir notificaciones de entes reguladores.
/// </summary>
public class WebhookConfig : EntityBase
{
    /// <summary>
    /// ID de la configuración de integración
    /// </summary>
    public Guid IntegrationConfigId { get; set; }

    /// <summary>
    /// Nombre del webhook
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Endpoint URL del webhook (nuestra URL)
    /// </summary>
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>
    /// Secret para validar la autenticidad
    /// </summary>
    public string? SecretHash { get; set; }

    /// <summary>
    /// Eventos a los que está suscrito
    /// </summary>
    public string? SubscribedEvents { get; set; }

    /// <summary>
    /// Indica si el webhook está activo
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Última vez que se recibió un evento
    /// </summary>
    public DateTime? LastEventReceivedAt { get; set; }

    /// <summary>
    /// Contador de eventos recibidos
    /// </summary>
    public int EventCount { get; set; }

    // Navegación
    public virtual IntegrationConfig? IntegrationConfig { get; set; }
}

/// <summary>
/// Historial de cambios de estado de integración.
/// </summary>
public class IntegrationStatusHistory : EntityBase
{
    /// <summary>
    /// ID de la configuración de integración
    /// </summary>
    public Guid IntegrationConfigId { get; set; }

    /// <summary>
    /// Estado anterior
    /// </summary>
    public IntegrationStatus PreviousStatus { get; set; }

    /// <summary>
    /// Estado nuevo
    /// </summary>
    public IntegrationStatus NewStatus { get; set; }

    /// <summary>
    /// Razón del cambio de estado
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Usuario que realizó el cambio
    /// </summary>
    public Guid? ChangedByUserId { get; set; }

    // Navegación
    public virtual IntegrationConfig? IntegrationConfig { get; set; }
}
