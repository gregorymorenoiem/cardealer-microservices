namespace RegulatoryAlertService.Domain.Enums;

/// <summary>
/// Regulatory body in Dominican Republic
/// </summary>
public enum RegulatoryBody
{
    // === TAX ===
    DGII = 1,                   // Dirección General de Impuestos Internos
    
    // === CONSUMER PROTECTION ===
    ProConsumidor = 2,          // Instituto Nacional de Protección de los Derechos del Consumidor
    
    // === FINANCIAL ===
    UAF = 3,                    // Unidad de Análisis Financiero (AML/PLD)
    SuperintendenciaBancos = 4, // Superintendencia de Bancos
    
    // === TELECOMMUNICATIONS ===
    INDOTEL = 5,                // Instituto Dominicano de las Telecomunicaciones
    
    // === COMPETITION ===
    ProCompetencia = 6,         // Comisión Nacional de Defensa de la Competencia
    
    // === DATA PROTECTION ===
    OGTIC = 7,                  // Oficina Gubernamental de Tecnologías de la Información
    
    // === ENVIRONMENT ===
    MedioAmbiente = 8,          // Ministerio de Medio Ambiente
    
    // === LABOR ===
    MinisterioTrabajo = 9,      // Ministerio de Trabajo
    
    // === OTHERS ===
    CamaraDeCuentas = 10,       // Cámara de Cuentas
    CongresoNacional = 11,      // Congreso Nacional (nuevas leyes)
    PoderJudicial = 12,         // Poder Judicial
    
    Other = 99
}

/// <summary>
/// Type of regulatory alert
/// </summary>
public enum AlertType
{
    // === DEADLINES ===
    FilingDeadline = 1,         // Fecha límite de presentación
    PaymentDeadline = 2,        // Fecha límite de pago
    RenewalDeadline = 3,        // Renovación de licencias/permisos
    
    // === REGULATORY CHANGES ===
    NewLaw = 10,                // Nueva ley aprobada
    LawAmendment = 11,          // Modificación a ley existente
    NewRegulation = 12,         // Nuevo reglamento
    NewResolution = 13,         // Nueva resolución
    
    // === COMPLIANCE ===
    ComplianceReminder = 20,    // Recordatorio de cumplimiento
    AuditNotice = 21,           // Aviso de auditoría
    InspectionNotice = 22,      // Aviso de inspección
    
    // === RATES & FEES ===
    TaxRateChange = 30,         // Cambio de tasa impositiva
    FeeIncrease = 31,           // Aumento de tarifas
    
    // === SYSTEM ===
    SystemMaintenance = 40,     // Mantenimiento de sistemas reguladores
    NewPortalFeature = 41,      // Nueva funcionalidad en portal
    
    // === SANCTIONS ===
    SanctionWarning = 50,       // Aviso de posible sanción
    FineNotice = 51,            // Aviso de multa
    
    Other = 99
}

/// <summary>
/// Priority of the alert
/// </summary>
public enum AlertPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4,
    Urgent = 5
}

/// <summary>
/// Status of the alert
/// </summary>
public enum AlertStatus
{
    Draft = 1,
    Active = 2,
    Acknowledged = 3,
    InProgress = 4,
    Resolved = 5,
    Expired = 6,
    Dismissed = 7,
    Escalated = 8
}

/// <summary>
/// Subscription preference for alerts
/// </summary>
public enum SubscriptionFrequency
{
    Immediate = 1,
    Daily = 2,
    Weekly = 3,
    Monthly = 4
}

/// <summary>
/// Notification channel
/// </summary>
public enum NotificationChannel
{
    Email = 1,
    SMS = 2,
    Push = 3,
    InApp = 4,
    Webhook = 5,
    WhatsApp = 6
}

/// <summary>
/// Category of regulations
/// </summary>
public enum RegulatoryCategory
{
    Tax = 1,                    // Impuestos (DGII)
    ConsumerProtection = 2,    // Pro-Consumidor
    AML = 3,                   // Anti-Lavado (UAF, Ley 155-17)
    DataProtection = 4,        // Protección de datos (Ley 172-13)
    ECommerce = 5,             // Comercio electrónico (Ley 126-02)
    Labor = 6,                 // Laboral
    Environment = 7,           // Ambiental
    Competition = 8,           // Competencia
    Financial = 9,             // Sector financiero
    Telecommunications = 10,   // Telecomunicaciones
    General = 99
}
