// =====================================================
// C12: ComplianceIntegrationService - Enumeraciones
// Entes reguladores RD y tipos de integración
// =====================================================

namespace ComplianceIntegrationService.Domain.Enums;

/// <summary>
/// Entes reguladores de República Dominicana
/// </summary>
public enum RegulatoryBody
{
    /// <summary>Dirección General de Impuestos Internos</summary>
    DGII = 1,
    
    /// <summary>Unidad de Análisis Financiero - PLD/FT</summary>
    UAF = 2,
    
    /// <summary>Pro Consumidor - Protección al consumidor</summary>
    ProConsumidor = 3,
    
    /// <summary>Superintendencia de Bancos</summary>
    SuperintendenciaBancos = 4,
    
    /// <summary>Instituto Dominicano de las Telecomunicaciones</summary>
    INDOTEL = 5,
    
    /// <summary>Pro Competencia - Defensa de la competencia</summary>
    ProCompetencia = 6,
    
    /// <summary>Oficina Gubernamental de Tecnologías de la Información</summary>
    OGTIC = 7,
    
    /// <summary>Dirección General de Aduanas</summary>
    DGA = 8,
    
    /// <summary>Ministerio de Medio Ambiente</summary>
    MedioAmbiente = 9,
    
    /// <summary>Ministerio de Trabajo</summary>
    MinisterioTrabajo = 10,
    
    /// <summary>Tesorería de la Seguridad Social</summary>
    TSS = 11,
    
    /// <summary>INFOTEP - Capacitación técnico profesional</summary>
    INFOTEP = 12,
    
    /// <summary>Cámara de Comercio y Producción</summary>
    CamaraComercio = 13
}

/// <summary>
/// Tipo de integración con ente regulador
/// </summary>
public enum IntegrationType
{
    /// <summary>Integración vía API REST</summary>
    ApiRest = 1,
    
    /// <summary>Integración vía Web Services SOAP</summary>
    WebServiceSoap = 2,
    
    /// <summary>Transferencia de archivos SFTP</summary>
    Sftp = 3,
    
    /// <summary>Envío por correo electrónico</summary>
    Email = 4,
    
    /// <summary>Portal web manual</summary>
    WebPortal = 5,
    
    /// <summary>Integración por archivo plano</summary>
    FlatFile = 6,
    
    /// <summary>Integración por archivo XML</summary>
    XmlFile = 7,
    
    /// <summary>Integración por archivo JSON</summary>
    JsonFile = 8,
    
    /// <summary>Base de datos directa (legacy)</summary>
    DirectDatabase = 9
}

/// <summary>
/// Estado de la integración
/// </summary>
public enum IntegrationStatus
{
    /// <summary>Integración activa y funcionando</summary>
    Active = 1,
    
    /// <summary>Integración inactiva temporalmente</summary>
    Inactive = 2,
    
    /// <summary>En proceso de configuración</summary>
    Configuring = 3,
    
    /// <summary>En pruebas/testing</summary>
    Testing = 4,
    
    /// <summary>Error en la integración</summary>
    Error = 5,
    
    /// <summary>Suspendida por mantenimiento</summary>
    Maintenance = 6,
    
    /// <summary>Deprecada/obsoleta</summary>
    Deprecated = 7
}

/// <summary>
/// Estado de transmisión de datos
/// </summary>
public enum TransmissionStatus
{
    /// <summary>Pendiente de envío</summary>
    Pending = 1,
    
    /// <summary>En proceso de transmisión</summary>
    InProgress = 2,
    
    /// <summary>Transmitido exitosamente</summary>
    Success = 3,
    
    /// <summary>Error en transmisión</summary>
    Failed = 4,
    
    /// <summary>Parcialmente transmitido</summary>
    Partial = 5,
    
    /// <summary>Cancelado</summary>
    Cancelled = 6,
    
    /// <summary>Reintentando</summary>
    Retrying = 7,
    
    /// <summary>Expirado sin enviar</summary>
    Expired = 8
}

/// <summary>
/// Tipo de credencial para autenticación
/// </summary>
public enum CredentialType
{
    /// <summary>Usuario y contraseña básico</summary>
    BasicAuth = 1,
    
    /// <summary>Token Bearer/JWT</summary>
    BearerToken = 2,
    
    /// <summary>API Key</summary>
    ApiKey = 3,
    
    /// <summary>Certificado digital</summary>
    Certificate = 4,
    
    /// <summary>OAuth 2.0</summary>
    OAuth2 = 5,
    
    /// <summary>Firma digital RD</summary>
    DigitalSignature = 6,
    
    /// <summary>HMAC</summary>
    Hmac = 7
}

/// <summary>
/// Frecuencia de sincronización
/// </summary>
public enum SyncFrequency
{
    /// <summary>Tiempo real</summary>
    RealTime = 1,
    
    /// <summary>Cada hora</summary>
    Hourly = 2,
    
    /// <summary>Diariamente</summary>
    Daily = 3,
    
    /// <summary>Semanalmente</summary>
    Weekly = 4,
    
    /// <summary>Mensualmente</summary>
    Monthly = 5,
    
    /// <summary>Trimestralmente</summary>
    Quarterly = 6,
    
    /// <summary>Anualmente</summary>
    Yearly = 7,
    
    /// <summary>Bajo demanda/manual</summary>
    OnDemand = 8
}

/// <summary>
/// Tipo de reporte regulatorio
/// </summary>
public enum ReportType
{
    /// <summary>Reporte de Operaciones Sospechosas (ROS)</summary>
    ROS = 1,
    
    /// <summary>Declaración Jurada de IR</summary>
    DeclaracionIR = 2,
    
    /// <summary>Reporte ITBIS</summary>
    ReporteITBIS = 3,
    
    /// <summary>Facturación Electrónica e-CF</summary>
    FacturaElectronica = 4,
    
    /// <summary>Reporte 606 - Compras</summary>
    Reporte606 = 5,
    
    /// <summary>Reporte 607 - Ventas</summary>
    Reporte607 = 6,
    
    /// <summary>Reporte 608 - Anulaciones</summary>
    Reporte608 = 7,
    
    /// <summary>Reporte 609 - Pagos al exterior</summary>
    Reporte609 = 8,
    
    /// <summary>Declaración TSS</summary>
    DeclaracionTSS = 9,
    
    /// <summary>Debida Diligencia</summary>
    DebidaDiligencia = 10,
    
    /// <summary>Reporte de Transacciones</summary>
    ReporteTransacciones = 11,
    
    /// <summary>Informe Anual PLD</summary>
    InformeAnualPLD = 12
}

/// <summary>
/// Nivel de severidad para logs de integración
/// </summary>
public enum LogSeverity
{
    /// <summary>Depuración/Debug</summary>
    Debug = 1,
    
    /// <summary>Información general</summary>
    Info = 2,
    
    /// <summary>Advertencia</summary>
    Warning = 3,
    
    /// <summary>Error</summary>
    Error = 4,
    
    /// <summary>Error crítico</summary>
    Critical = 5
}

/// <summary>
/// Dirección de la transmisión de datos
/// </summary>
public enum DataDirection
{
    /// <summary>Datos enviados al ente regulador (outbound)</summary>
    Outbound = 1,
    
    /// <summary>Datos recibidos del ente regulador (inbound)</summary>
    Inbound = 2,
    
    /// <summary>Bidireccional</summary>
    Bidirectional = 3
}
