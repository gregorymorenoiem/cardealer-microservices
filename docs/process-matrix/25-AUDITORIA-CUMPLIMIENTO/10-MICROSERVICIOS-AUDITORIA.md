# 🏗️ Microservicios de Auditoría - Arquitectura

> **Propósito:** Definir la arquitectura de los nuevos microservicios de cumplimiento  
> **Estado:** Documentación para implementación futura  
> **Última actualización:** Enero 25, 2026

---

## 📋 RESUMEN DE MICROSERVICIOS

### Nuevos Servicios Requeridos

| Servicio                   | Puerto | Base de Datos    | Descripción                        |
| -------------------------- | ------ | ---------------- | ---------------------------------- |
| **AuditService**           | 5070   | auditdb          | Centraliza evidencias y auditorías |
| **ComplianceService**      | 5071   | compliancedb     | Gestión UAF/AML                    |
| **FiscalReportingService** | 5072   | fiscaldb         | Reportes DGII                      |
| **DataProtectionService**  | 5073   | dataprotectiondb | ARCO y datos personales            |

### Diagrama de Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          CAPA DE AUDITORÍA Y CUMPLIMIENTO                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│    ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐           │
│    │  AuditService   │  │ComplianceService│  │FiscalReporting  │           │
│    │    (5070)       │  │    (5071)       │  │   Service(5072) │           │
│    │                 │  │                 │  │                 │           │
│    │ • Evidencias    │  │ • KYC/DDC       │  │ • Formato 606   │           │
│    │ • Controles     │  │ • Alertas AML   │  │ • Formato 607   │           │
│    │ • Dashboard     │  │ • ROS           │  │ • Formato 608   │           │
│    │ • Logs          │  │ • Capacitación  │  │ • ITBIS         │           │
│    └────────┬────────┘  └────────┬────────┘  └────────┬────────┘           │
│             │                    │                    │                     │
│    ┌────────┴────────────────────┴────────────────────┴────────┐           │
│    │                    DataProtectionService (5073)            │           │
│    │                                                            │           │
│    │  • Solicitudes ARCO  • Consentimientos  • Brechas         │           │
│    │  • Registro tratamientos  • Exportación datos             │           │
│    └────────────────────────────────────────────────────────────┘           │
│                                    │                                        │
├────────────────────────────────────┼────────────────────────────────────────┤
│                      SERVICIOS EXISTENTES                                   │
│                                    │                                        │
│  ┌──────────┐  ┌──────────┐  ┌────┴─────┐  ┌──────────┐  ┌──────────┐     │
│  │UserService│  │Billing   │  │Notification│  │MediaService│  │AuthService│     │
│  │  (5003)  │  │Service   │  │ Service   │  │  (5005)  │  │  (5001)  │     │
│  │          │  │ (5020)   │  │  (5006)   │  │          │  │          │     │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘  └──────────┘     │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 1️⃣ AuditService (Puerto 5070)

### Responsabilidades

- Centralizar todas las evidencias de cumplimiento
- Gestionar el dashboard de auditoría
- Mantener logs de auditoría inmutables
- Preparar paquetes de documentación para auditorías
- Monitorear el estado de cumplimiento

### Estructura Clean Architecture

```
AuditService/
├── AuditService.Api/
│   ├── Controllers/
│   │   ├── EvidencesController.cs
│   │   ├── ControlsController.cs
│   │   ├── DashboardController.cs
│   │   └── AuditLogsController.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── Dockerfile
├── AuditService.Application/
│   ├── Features/
│   │   ├── Evidences/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateEvidenceCommand.cs
│   │   │   │   └── UploadEvidenceCommand.cs
│   │   │   └── Queries/
│   │   │       ├── GetEvidencesQuery.cs
│   │   │       └── GetEvidenceByIdQuery.cs
│   │   ├── Controls/
│   │   │   └── ...
│   │   └── Dashboard/
│   │       └── GetComplianceStatusQuery.cs
│   ├── DTOs/
│   ├── Interfaces/
│   └── Validators/
├── AuditService.Domain/
│   ├── Entities/
│   │   ├── Evidence.cs
│   │   ├── Control.cs
│   │   ├── AuditLog.cs
│   │   ├── ComplianceCheck.cs
│   │   └── AuditPreparation.cs
│   ├── Enums/
│   │   ├── EvidenceType.cs
│   │   ├── Regulator.cs
│   │   └── ComplianceStatus.cs
│   └── Interfaces/
│       └── IEvidenceRepository.cs
├── AuditService.Infrastructure/
│   ├── Persistence/
│   │   ├── AuditDbContext.cs
│   │   └── Repositories/
│   ├── Services/
│   │   ├── S3EvidenceStorage.cs
│   │   └── ComplianceChecker.cs
│   └── Clients/
│       ├── FiscalReportingClient.cs
│       └── ComplianceServiceClient.cs
└── AuditService.Tests/
```

### Entidades Principales

```csharp
public class Evidence
{
    public Guid Id { get; set; }
    public string Code { get; set; }  // Ej: "DGII-E009"
    public string Name { get; set; }
    public string Description { get; set; }
    public Regulator Regulator { get; set; }
    public EvidenceType Type { get; set; }
    public EvidenceStatus Status { get; set; }
    public string StoragePath { get; set; }
    public string Period { get; set; }  // Ej: "202601"
    public DateTime? GeneratedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Control
{
    public Guid Id { get; set; }
    public string Code { get; set; }  // Ej: "CT-001"
    public string Name { get; set; }
    public string Description { get; set; }
    public ControlType Type { get; set; }  // Technical, Organizational, Process
    public ControlStatus Status { get; set; }
    public string EvidenceRequired { get; set; }
    public DateTime? LastTested { get; set; }
    public bool IsEffective { get; set; }
}

public class AuditLog
{
    public Guid Id { get; set; }
    public string Action { get; set; }
    public string EntityType { get; set; }
    public Guid EntityId { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; }
    public string IpAddress { get; set; }
    public string Details { get; set; }  // JSON
    public DateTime Timestamp { get; set; }
}
```

### Endpoints API

| Método | Endpoint                       | Descripción                |
| ------ | ------------------------------ | -------------------------- |
| `GET`  | `/api/evidences`               | Listar evidencias          |
| `GET`  | `/api/evidences/{id}`          | Detalle de evidencia       |
| `POST` | `/api/evidences`               | Crear evidencia            |
| `POST` | `/api/evidences/{id}/upload`   | Subir archivo              |
| `GET`  | `/api/evidences/{id}/download` | Descargar archivo          |
| `GET`  | `/api/controls`                | Listar controles           |
| `GET`  | `/api/controls/{id}`           | Detalle de control         |
| `POST` | `/api/controls/{id}/test`      | Probar efectividad         |
| `GET`  | `/api/dashboard/summary`       | Resumen de cumplimiento    |
| `GET`  | `/api/dashboard/gaps`          | Brechas identificadas      |
| `GET`  | `/api/dashboard/calendar`      | Calendario de obligaciones |
| `GET`  | `/api/audit-logs`              | Logs de auditoría          |
| `POST` | `/api/prepare/{regulator}`     | Preparar para auditoría    |

---

## 2️⃣ ComplianceService (Puerto 5071)

### Responsabilidades

- Gestionar el programa de cumplimiento AML
- Procesar KYC/DDC de clientes
- Generar y gestionar alertas AML
- Preparar y enviar ROS a la UAF
- Gestionar capacitaciones de personal

### Estructura Clean Architecture

```
ComplianceService/
├── ComplianceService.Api/
│   ├── Controllers/
│   │   ├── KycController.cs
│   │   ├── AlertsController.cs
│   │   ├── RosController.cs
│   │   ├── TrainingController.cs
│   │   └── PepsController.cs
│   └── ...
├── ComplianceService.Application/
│   ├── Features/
│   │   ├── Kyc/
│   │   │   ├── Commands/
│   │   │   │   ├── StartKycCommand.cs
│   │   │   │   ├── VerifyDocumentCommand.cs
│   │   │   │   └── ApproveKycCommand.cs
│   │   │   └── Queries/
│   │   │       ├── GetKycStatusQuery.cs
│   │   │       └── GetHighRiskClientsQuery.cs
│   │   ├── Alerts/
│   │   │   ├── Commands/
│   │   │   │   ├── GenerateAlertCommand.cs
│   │   │   │   └── ResolveAlertCommand.cs
│   │   │   └── Queries/
│   │   │       └── GetPendingAlertsQuery.cs
│   │   ├── Ros/
│   │   │   └── Commands/
│   │   │       ├── CreateRosDraftCommand.cs
│   │   │       └── SubmitRosCommand.cs
│   │   └── Training/
│   │       └── ...
│   └── Services/
│       ├── AlertEngine.cs
│       ├── RiskClassifier.cs
│       └── PepChecker.cs
├── ComplianceService.Domain/
│   ├── Entities/
│   │   ├── KycRecord.cs
│   │   ├── Alert.cs
│   │   ├── Ros.cs
│   │   ├── Training.cs
│   │   └── Pep.cs
│   ├── Enums/
│   │   ├── RiskLevel.cs
│   │   ├── AlertType.cs
│   │   ├── AlertStatus.cs
│   │   └── KycStatus.cs
│   └── Events/
│       ├── AlertGeneratedEvent.cs
│       └── RosSubmittedEvent.cs
└── ComplianceService.Infrastructure/
    ├── Persistence/
    ├── Services/
    │   ├── JceVerificationService.cs  // Verificar cédulas
    │   ├── DgiiVerificationService.cs // Verificar RNC
    │   └── SanctionsListChecker.cs    // Listas OFAC, etc.
    └── Clients/
        └── UafApiClient.cs  // Si existe API UAF
```

### Entidades Principales

```csharp
public class KycRecord
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public KycLevel Level { get; set; }  // Simplified, Standard, Enhanced
    public KycStatus Status { get; set; }
    public RiskLevel RiskLevel { get; set; }

    // Datos de identificación
    public string DocumentType { get; set; }
    public string DocumentNumber { get; set; }
    public string DocumentImagePath { get; set; }
    public bool DocumentVerified { get; set; }

    // Datos adicionales
    public string SourceOfFunds { get; set; }
    public string Occupation { get; set; }
    public bool IsPep { get; set; }

    // Tracking
    public DateTime CreatedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedBy { get; set; }
    public DateTime? NextReviewAt { get; set; }
}

public class Alert
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public AlertType Type { get; set; }
    public AlertStatus Status { get; set; }
    public string Description { get; set; }
    public decimal? TransactionAmount { get; set; }
    public Guid? TransactionId { get; set; }
    public string Indicators { get; set; }  // JSON
    public DateTime DetectedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedBy { get; set; }
    public string Resolution { get; set; }
    public bool EscalatedToRos { get; set; }
}

public class Ros
{
    public Guid Id { get; set; }
    public string InternalNumber { get; set; }
    public Guid SubjectUserId { get; set; }
    public string SubjectName { get; set; }
    public string SubjectDocument { get; set; }
    public RosStatus Status { get; set; }
    public string NarrativeDescription { get; set; }
    public List<Guid> RelatedAlerts { get; set; }
    public decimal? TotalAmount { get; set; }
    public DateTime DetectionDate { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string UafReference { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
}
```

### Motor de Alertas

```csharp
public class AlertEngine
{
    private readonly List<IAlertRule> _rules;

    public AlertEngine()
    {
        _rules = new List<IAlertRule>
        {
            new HighValueTransactionRule(5_000_000),  // > 5M DOP
            new FrequentTransactionsRule(3, TimeSpan.FromHours(24)),
            new PriceAnomalyRule(0.5m),  // < 50% market value
            new NewUserHighValueRule(1_000_000),
            new MultipleAccountsRule(),
            new SanctionsListRule(),
            new PepRelatedRule()
        };
    }

    public async Task<List<Alert>> EvaluateTransaction(Transaction tx)
    {
        var alerts = new List<Alert>();

        foreach (var rule in _rules)
        {
            if (await rule.Evaluate(tx))
            {
                alerts.Add(new Alert
                {
                    Type = rule.AlertType,
                    Description = rule.GetDescription(tx),
                    Indicators = rule.GetIndicators(tx)
                });
            }
        }

        return alerts;
    }
}
```

### Endpoints API

| Método | Endpoint                   | Descripción                |
| ------ | -------------------------- | -------------------------- |
| `POST` | `/api/kyc/start`           | Iniciar proceso KYC        |
| `POST` | `/api/kyc/{id}/verify`     | Verificar documentos       |
| `GET`  | `/api/kyc/{userId}`        | Estado KYC de usuario      |
| `GET`  | `/api/kyc/pending`         | KYC pendientes de revisión |
| `GET`  | `/api/kyc/high-risk`       | Usuarios alto riesgo       |
| `GET`  | `/api/alerts`              | Listar alertas             |
| `GET`  | `/api/alerts/pending`      | Alertas pendientes         |
| `PUT`  | `/api/alerts/{id}/resolve` | Resolver alerta            |
| `POST` | `/api/ros`                 | Crear borrador ROS         |
| `GET`  | `/api/ros`                 | Listar ROS                 |
| `PUT`  | `/api/ros/{id}/submit`     | Enviar ROS                 |
| `GET`  | `/api/training/sessions`   | Sesiones de capacitación   |
| `POST` | `/api/training/sessions`   | Crear sesión               |
| `POST` | `/api/training/attendance` | Registrar asistencia       |

---

## 3️⃣ FiscalReportingService (Puerto 5072)

### Responsabilidades

- Generar formatos 606, 607, 608 para DGII
- Calcular ITBIS y retenciones
- Gestionar calendario fiscal
- Alertar sobre fechas límite
- Almacenar reportes históricos

### Estructura

```
FiscalReportingService/
├── FiscalReportingService.Api/
│   ├── Controllers/
│   │   ├── Formato606Controller.cs
│   │   ├── Formato607Controller.cs
│   │   ├── Formato608Controller.cs
│   │   ├── ItbisController.cs
│   │   └── CalendarController.cs
│   └── ...
├── FiscalReportingService.Application/
│   ├── Features/
│   │   ├── Formato606/
│   │   │   └── Commands/
│   │   │       └── GenerateFormato606Command.cs
│   │   ├── Formato607/
│   │   ├── Formato608/
│   │   └── Itbis/
│   └── Services/
│       ├── FormatoGenerator.cs
│       ├── ItbisCalculator.cs
│       ├── NcfValidator.cs
│       └── DeadlineTracker.cs
└── FiscalReportingService.Domain/
    ├── Entities/
    │   ├── FiscalReport.cs
    │   ├── Obligation.cs
    │   └── NcfSequence.cs
    └── ValueObjects/
        ├── Formato606Record.cs
        ├── Formato607Record.cs
        └── Formato608Record.cs
```

(Detalle completo en [08-REPORTES-AUTOMATIZADOS.md])

---

## 4️⃣ DataProtectionService (Puerto 5073)

### Responsabilidades

- Procesar solicitudes ARCO
- Gestionar consentimientos
- Mantener registro de tratamientos
- Gestionar brechas de seguridad
- Exportar datos de usuarios

### Estructura

```
DataProtectionService/
├── DataProtectionService.Api/
│   ├── Controllers/
│   │   ├── ArcoController.cs
│   │   ├── ConsentsController.cs
│   │   ├── BreachesController.cs
│   │   └── DataExportController.cs
│   └── ...
├── DataProtectionService.Application/
│   ├── Features/
│   │   ├── Arco/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateArcoRequestCommand.cs
│   │   │   │   └── ProcessArcoRequestCommand.cs
│   │   │   └── Queries/
│   │   │       └── GetArcoRequestsQuery.cs
│   │   ├── Consents/
│   │   │   └── ...
│   │   └── Breaches/
│   │       └── ...
│   └── Services/
│       ├── DataCollector.cs
│       ├── DataAnonymizer.cs
│       └── DataExporter.cs
└── DataProtectionService.Domain/
    ├── Entities/
    │   ├── ArcoRequest.cs
    │   ├── Consent.cs
    │   ├── Breach.cs
    │   └── TreatmentRegistry.cs
    └── Enums/
        ├── ArcoType.cs
        └── ConsentPurpose.cs
```

### Entidades Principales

```csharp
public class ArcoRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ArcoType Type { get; set; }  // Access, Rectification, Cancellation, Opposition
    public ArcoStatus Status { get; set; }
    public string Details { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime DueDate { get; set; }  // +10 días hábiles
    public DateTime? CompletedAt { get; set; }
    public Guid? ProcessedBy { get; set; }
    public string Response { get; set; }
    public string ExportPath { get; set; }  // Para Access/Portability
}

public class Consent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ConsentPurpose Purpose { get; set; }
    public bool Granted { get; set; }
    public string Version { get; set; }  // Versión del texto
    public string IpAddress { get; set; }
    public DateTime GrantedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}

public class Breach
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DetectedAt { get; set; }
    public BreachSeverity Severity { get; set; }
    public string DataTypesAffected { get; set; }
    public int UsersAffected { get; set; }
    public bool AuthoritiesNotified { get; set; }
    public bool UsersNotified { get; set; }
    public string RootCause { get; set; }
    public string Remediation { get; set; }
    public BreachStatus Status { get; set; }
}
```

### Endpoints API

| Método   | Endpoint                          | Descripción                 |
| -------- | --------------------------------- | --------------------------- |
| `POST`   | `/api/arco/requests`              | Crear solicitud ARCO        |
| `GET`    | `/api/arco/requests`              | Listar solicitudes          |
| `GET`    | `/api/arco/requests/{id}`         | Detalle solicitud           |
| `PUT`    | `/api/arco/requests/{id}/process` | Procesar solicitud          |
| `GET`    | `/api/data/{userId}`              | Obtener todos los datos     |
| `POST`   | `/api/data/{userId}/export`       | Exportar datos              |
| `DELETE` | `/api/data/{userId}`              | Anonimizar datos            |
| `GET`    | `/api/consents/{userId}`          | Consentimientos del usuario |
| `PUT`    | `/api/consents/{userId}`          | Actualizar consentimientos  |
| `POST`   | `/api/breaches`                   | Registrar brecha            |
| `GET`    | `/api/breaches`                   | Listar brechas              |
| `PUT`    | `/api/breaches/{id}/notify`       | Notificar afectados         |

---

## 🔌 COMUNICACIÓN ENTRE SERVICIOS

### Eventos (RabbitMQ)

```csharp
// Eventos publicados
public record UserRegisteredEvent(Guid UserId, string DocumentNumber);
public record TransactionCompletedEvent(Guid TxId, Guid UserId, decimal Amount);
public record AlertGeneratedEvent(Guid AlertId, Guid UserId, AlertType Type);
public record RosSubmittedEvent(Guid RosId, DateTime SubmittedAt);
public record ArcoRequestCreatedEvent(Guid RequestId, Guid UserId, ArcoType Type);
public record BreachDetectedEvent(Guid BreachId, BreachSeverity Severity);

// Suscripciones
// ComplianceService escucha:
//   - UserRegisteredEvent → Iniciar KYC básico
//   - TransactionCompletedEvent → Evaluar en motor de alertas

// AuditService escucha:
//   - AlertGeneratedEvent → Registrar en log
//   - RosSubmittedEvent → Crear evidencia

// DataProtectionService escucha:
//   - UserRegisteredEvent → Registrar consentimientos iniciales
```

### Clientes HTTP

```csharp
// En AuditService
public class FiscalReportingClient
{
    public Task<Formato606Response> GetFormato606(string period);
    public Task<Formato607Response> GetFormato607(string period);
    public Task<Formato608Response> GetFormato608(string period);
}

// En ComplianceService
public class UserServiceClient
{
    public Task<UserDto> GetUserById(Guid userId);
    public Task<List<UserDto>> GetUsersWithPendingKyc();
}

public class BillingServiceClient
{
    public Task<List<TransactionDto>> GetUserTransactions(Guid userId, DateTime from, DateTime to);
}
```

---

## 📊 BASES DE DATOS

### Esquemas

```sql
-- auditdb
CREATE TABLE evidences (...);
CREATE TABLE controls (...);
CREATE TABLE audit_logs (...);
CREATE TABLE compliance_checks (...);

-- compliancedb
CREATE TABLE kyc_records (...);
CREATE TABLE alerts (...);
CREATE TABLE ros (...);
CREATE TABLE trainings (...);
CREATE TABLE training_attendees (...);
CREATE TABLE pep_list (...);

-- fiscaldb
CREATE TABLE fiscal_reports (...);
CREATE TABLE obligations (...);
CREATE TABLE ncf_sequences (...);

-- dataprotectiondb
CREATE TABLE arco_requests (...);
CREATE TABLE consents (...);
CREATE TABLE breaches (...);
CREATE TABLE treatment_registry (...);
```

---

## 🚀 PLAN DE IMPLEMENTACIÓN

### Fase 1 (Semana 1-2): Infraestructura Base

- [ ] Crear proyectos con Clean Architecture
- [ ] Configurar bases de datos
- [ ] Configurar RabbitMQ exchanges
- [ ] Agregar a docker-compose
- [ ] Agregar rutas en Gateway

### Fase 2 (Semana 3-4): FiscalReportingService

- [ ] Implementar generación 606/607/608
- [ ] Implementar cálculo ITBIS
- [ ] Crear UI de reportes
- [ ] Testing con datos reales

### Fase 3 (Semana 5-6): ComplianceService

- [ ] Implementar KYC básico
- [ ] Implementar motor de alertas
- [ ] Crear UI de alertas
- [ ] Integrar con UserService

### Fase 4 (Semana 7-8): DataProtectionService

- [ ] Implementar solicitudes ARCO
- [ ] Implementar gestión de consentimientos
- [ ] Crear UI de privacidad
- [ ] Integrar recolección de datos

### Fase 5 (Semana 9-10): AuditService

- [ ] Implementar gestión de evidencias
- [ ] Crear dashboard de cumplimiento
- [ ] Implementar logs de auditoría
- [ ] Integrar con todos los servicios

---

**Última revisión:** Enero 25, 2026  
**Próxima revisión:** Al iniciar implementación  
**Responsable:** Equipo de Desarrollo
