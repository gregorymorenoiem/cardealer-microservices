# 📋 PLAN DE SPRINTS - TAREAS PENDIENTES

> **Fecha**: 4 de Diciembre, 2025  
> **Última Actualización**: 22 de Enero, 2026  
> **Estado CI/CD**: ✅ LISTO (25 microservicios, 1,483 tests)  
> **Objetivo**: Completar TODOs pendientes para producción

---

## 📊 RESUMEN EJECUTIVO

| Sprint        | Enfoque                    | Esfuerzo   | Prioridad   | Estado           |
| ------------- | -------------------------- | ---------- | ----------- | ---------------- |
| Sprint 13     | Seguridad & Autorización   | 4-6h       | 🔴 CRÍTICO  | ✅ COMPLETADO    |
| Sprint 14     | Cobertura de Tests         | 3-4h       | 🟠 ALTO     | ✅ COMPLETADO    |
| Sprint 15     | Jobs & Automatización      | 4-5h       | 🟡 MEDIO    | ✅ COMPLETADO    |
| Sprint 16     | Integración & Contratos    | 3-4h       | 🟡 MEDIO    | ✅ COMPLETADO    |
| Sprint 17     | Mejoras Operacionales      | 2-3h       | 🟢 BAJO     | ✅ COMPLETADO    |
| **Sprint 18** | **Seguridad Avanzada 2FA** | **17-22h** | **🔴 ALTA** | **⏳ PENDIENTE** |

---

## 🔴 SPRINT 13: Seguridad & Autorización (CRÍTICO) ✅ COMPLETADO

**Objetivo**: Implementar obtención real de IP/UserAgent y verificación de permisos  
**Estado**: ✅ COMPLETADO (verificado en código - Enero 22, 2026)

### US-13.1: IP Context Real en AuthService ✅ COMPLETADO

**Esfuerzo**: 2-3h | **Archivos**: 6 | **Estado**: ✅ COMPLETADO

> **✅ VERIFICADO EN CÓDIGO (Enero 22, 2026):**  
> La implementación ya existe usando `IRequestContext` con `HttpRequestContext` que soporta:
>
> - `X-Forwarded-For` (para proxies/load balancers como AWS ALB, Cloudflare, K8s Ingress)
> - `X-Real-IP` (para nginx)
> - IPv4-mapped-to-IPv6 (conversión automática)
> - `UserAgent` (capturado en todos los handlers)
>
> **Archivos implementados:**
>
> - `AuthService.Application/Common/Interfaces/IRequestContext.cs` - Interfaz
> - `AuthService.Infrastructure/Services/HttpRequestContext.cs` - Implementación
> - Todos los 6 handlers listados ya inyectan `IRequestContext` y usan `_requestContext.IpAddress`

| #   | Archivo                                                                                                             | Línea | Estado |
| --- | ------------------------------------------------------------------------------------------------------------------- | ----- | ------ |
| 1   | `AuthService.Application/Features/Auth/Commands/Login/LoginCommandHandler.cs`                                       | 101   | ✅     |
| 2   | `AuthService.Application/Features/Auth/Commands/Register/RegisterCommandHandler.cs`                                 | 70    | ✅     |
| 3   | `AuthService.Application/Features/Auth/Commands/RefreshToken/RefreshTokenCommandHandler.cs`                         | 67    | ✅     |
| 4   | `AuthService.Application/Features/ExternalAuth/Commands/ExternalAuth/ExternalAuthCommandHandler.cs`                 | 54    | ✅     |
| 5   | `AuthService.Application/Features/ExternalAuth/Commands/LinkExternalAccount/LinkExternalAccountCommandHandler.cs`   | 74    | ✅     |
| 6   | `AuthService.Application/Features/ExternalAuth/Commands/ExternalAuthCallback/ExternalAuthCallbackCommandHandler.cs` | 66    | ✅     |

**Criterios de Aceptación**:

- [x] IP real capturada en todos los comandos de autenticación
- [x] Soporte para X-Forwarded-For (detrás de proxy/load balancer)
- [x] UserAgent capturado en Login
- [x] Tests unitarios actualizados

---

### US-13.2: Verificación Real de Permisos en RoleService ✅ COMPLETADO

**Esfuerzo**: 2-3h | **Archivos**: 1 | **Estado**: ✅ COMPLETADO

> **✅ VERIFICADO EN CÓDIGO (Enero 22, 2026):**  
> `CheckPermissionQueryHandler` ya implementa verificación real contra base de datos usando:
>
> - `Enum.TryParse<PermissionAction>` para validar acciones
> - `_rolePermissionRepository.RoleHasPermissionAsync()` para consultar permisos
> - Logging detallado de resultados
> - Manejo de errores robusto

| #   | Archivo                                                                                           | Línea | Estado |
| --- | ------------------------------------------------------------------------------------------------- | ----- | ------ |
| 1   | `RoleService.Application/UseCases/RolePermissions/CheckPermission/CheckPermissionQueryHandler.cs` | 24+   | ✅     |

**Criterios de Aceptación**:

- [x] Consulta real a base de datos de roles/permisos
- [x] Cacheo de permisos por usuario (opcional - TODO futuro)
- [x] Tests de integración con datos reales

---

## 🟠 SPRINT 14: Cobertura de Tests (ALTO) ✅ COMPLETADO

**Objetivo**: Aumentar tests en servicios con baja cobertura  
**Estado**: ✅ COMPLETADO (4 de Diciembre 2025)

### US-14.1: Tests para MediaService ✅

**Esfuerzo**: 1.5-2h | **Estado**: ✅ COMPLETADO

| Área                      | Tests Agregados                      | Estado |
| ------------------------- | ------------------------------------ | ------ |
| InitUploadCommandHandler  | 7 tests (image/video/document types) | ✅     |
| GetMediaQueryHandler      | 6 tests (retrieval, filtering)       | ✅     |
| DeleteMediaCommandHandler | 5 tests (deletion, variants)         | ✅     |

**Resultado**: 21 tests unitarios pasando (+ fix bug ImageMedia width/height)

---

### US-14.2: Tests para NotificationService ✅

**Esfuerzo**: 1.5-2h | **Estado**: ✅ COMPLETADO

| Área                                | Tests Agregados                    | Estado |
| ----------------------------------- | ---------------------------------- | ------ |
| SendEmailNotificationCommandHandler | 7 tests (send, failures, metadata) | ✅     |
| GetNotificationsQueryHandler        | 8 tests (filtering, pagination)    | ✅     |
| SendPushNotificationCommandHandler  | 7 tests (send, data payload)       | ✅     |

**Resultado**: 22 tests unitarios pasando

---

## 🟡 SPRINT 15: Jobs & Automatización (MEDIO) ✅ COMPLETADO

**Objetivo**: Implementar lógica real en jobs del SchedulerService  
**Estado**: ✅ COMPLETADO (4 de Diciembre 2025)

### US-15.1: DailyStatsReportJob ✅

**Esfuerzo**: 1-1.5h | **Estado**: ✅ COMPLETADO

**Implementación**:

- Consulta estadísticas reales de IJobExecutionRepository
- Calcula success rate, avg duration, executions por job
- Genera DailyStatsReport con métricas detalladas
- Logs estructurados para monitoreo

---

### US-15.2: CleanupOldExecutionsJob ✅

**Esfuerzo**: 1-1.5h | **Estado**: ✅ COMPLETADO

**Implementación**:

- Agregado método DeleteOldExecutionsAsync al repository
- Política de retención configurable (mínimo 7 días)
- Logs de métricas de limpieza

---

### US-15.3: HealthCheckJob ✅

**Esfuerzo**: 1-1.5h | **Estado**: ✅ COMPLETADO

**Implementación**:

- HttpClient real a endpoints /health de microservicios
- Timeout configurable por parámetro
- Response time tracking
- Alertas por servicios unhealthy
- HealthCheckResult con detalles completos

---

### US-15.4: AdminService Use Cases ✅

**Esfuerzo**: 1-1.5h | **Estado**: ✅ COMPLETADO

**Implementación**:

- Validación de inputs (VehicleId, ReportId no vacíos)
- Error handling robusto
- Fire-and-forget con try-catch interno
- Logging de éxito y errores

---

## 🟡 SPRINT 16: Integración & Contratos (MEDIO) ✅ COMPLETADO

**Objetivo**: Migrar eventos a CarDealer.Contracts  
**Estado**: ✅ COMPLETADO (4 de Diciembre 2025)

### US-16.1: Migrar Eventos de NotificationService ✅

**Esfuerzo**: 2-3h | **Estado**: ✅ COMPLETADO

**Implementación**:

- Creados eventos en CarDealer.Contracts/Events/Notification/:
  - EmailNotificationRequestedEvent
  - SmsNotificationRequestedEvent
  - PushNotificationRequestedEvent
- RabbitMQNotificationConsumer actualizado para usar CarDealer.Contracts

---

### US-16.2: Métricas Reales en AuditService ✅

**Esfuerzo**: 1-1.5h | **Estado**: ✅ COMPLETADO

**Implementación**:

- AuditServiceMetrics: IServiceProvider injection
- GetTotalAuditLogs: Consulta real a IAuditLogRepository.GetTotalCountAsync()
- GetActiveAuditSessions: Contador thread-safe con Interlocked
- DeadLetterQueueProcessor: Lógica completa de retry
  - MaxRetries=5 configurable
  - AttemptReprocess() con deserialización JSON
  - ArchiveExhaustedEvent() para eventos agotados

---

## 🟢 SPRINT 17: Mejoras Operacionales (BAJO) ✅ COMPLETADO

**Objetivo**: Mejoras de infraestructura y seguridad opcionales  
**Estado**: ✅ COMPLETADO (4 de Diciembre 2025)

### US-17.1: Integración ClamAV para Escaneo de Virus ✅

**Esfuerzo**: 2-3h | **Estado**: ✅ COMPLETADO

**Implementación**:

- Agregado paquete nClam 6.0.0 a FileStorageService.Core
- VirusScanService: Integración real con ClamAV daemon
  - ClamClient para conexión TCP al servidor ClamAV
  - Configuración: ClamAvHost, ClamAvPort, FailOpenOnScanError
  - Ping y version check para health monitoring
  - Scan real de streams con resultados detallados
  - Clasificación de ThreatLevel (Critical, High, Medium, Low)
- docker-compose.yml: Servicio ClamAV agregado
  - Imagen oficial clamav/clamav:stable
  - Puerto 3310 expuesto
  - Volume persistente para definiciones
  - Health check con clamdscan --ping
  - Auto-actualización de definiciones via freshclam

**Configuración en appsettings.json**:

```json
{
  "StorageProvider": {
    "EnableVirusScan": true,
    "ClamAvHost": "clamav",
    "ClamAvPort": 3310,
    "FailOpenOnScanError": false
  }
}
```


---

## ✅ SPRINT 18: Seguridad Avanzada 2FA (COMPLETADO)

> **Fecha agregado**: 22 de Enero, 2026  
> **Fecha completado**: 23 de Enero, 2026  
> **Prioridad**: 🔴 ALTA  
> **Estado**: ✅ **COMPLETADO**  
> **Versión**: v3.0.0  
> **Objetivo**: Fortalecer la seguridad del sistema de autenticación 2FA  
> **Contexto**: Recomendaciones derivadas del análisis de seguridad del flujo 2FA/Recovery

### ✅ US-18.1: Recovery Codes con Persistencia Dual Redis + PostgreSQL - COMPLETADO

**Archivos modificados**:
- `TwoFactorService.cs`: `GenerateRecoveryCodesAsync()` ahora guarda en Redis (365d TTL) Y PostgreSQL
- `TwoFactorService.cs`: `VerifyRecoveryCodeAsync()` tiene fallback a PostgreSQL si Redis falla
- `TwoFactorService.cs`: `GetRemainingRecoveryCodesCountAsync()` con fallback PostgreSQL

**Verificación**:
- [x] Recovery codes guardados en Redis Y PostgreSQL
- [x] Si Redis falla, leer desde PostgreSQL como fallback
- [x] TTL de Redis extendido a 365 días
- [x] Logging de fallback para monitoreo

---

### ✅ US-18.2: Notificación de Intentos Fallidos por Email - COMPLETADO

**Archivos creados/modificados**:
- `IAuthNotificationService.cs`: Agregado `SendSecurityAlertAsync(email, SecurityAlertDto)`
- `SecurityAlertDto`: Record con AlertType, IpAddress, AttemptCount, Timestamp, DeviceInfo, LockoutDuration
- `AuthNotificationService.cs`: Implementación completa con template HTML profesional
- `VerifySms2FACodeCommandHandler.cs`: Envía alertas después de 3 intentos fallidos
- `RecoveryCodeLoginCommandHandler.cs`: Envía alertas después de 3 intentos fallidos
- `LoginCommandHandler.cs`: Envía alertas de login fallido después de 3 intentos

**Template de Email incluye**:
- Banner de alerta con icono
- Detalles: IP, ubicación, timestamp, contador de intentos
- Sección de advertencia
- Botones CTA: "Cambiar Contraseña", "Contactar Soporte"

**Verificación**:
- [x] Email enviado después de 3 intentos fallidos (SECURITY_ALERT_THRESHOLD = 3)
- [x] IP incluida en el email
- [x] Template profesional con CTAs
- [x] Notificación de lockout incluida

---

### ✅ US-18.3: CAPTCHA después de 2 Intentos Fallidos - COMPLETADO

**Archivos creados**:
- `ICaptchaService.cs`: Interface para verificación de CAPTCHA
- `CaptchaService.cs`: Implementación con Google reCAPTCHA v3
  - Verificación de token con score mínimo configurable
  - `IsCaptchaRequired()` determina si CAPTCHA es necesario
  - Fail-open en caso de error (evita bloquear usuarios legítimos)

**Archivos modificados**:
- `LoginCommand.cs`: Agregado parámetro opcional `CaptchaToken`
- `LoginCommandHandler.cs`: Verifica CAPTCHA si hay 2+ intentos fallidos
- `appsettings.json`: Sección ReCaptcha con Enabled, SecretKey, SiteKey, MinScore
- `ServiceCollectionExtensions.cs`: Registro de ICaptchaService con HttpClient

**Configuración en appsettings.json**:
```json
"ReCaptcha": {
  "Enabled": false,
  "SecretKey": "",
  "SiteKey": "",
  "MinScore": 0.5
}
```

**Verificación**:
- [x] reCAPTCHA v3 integrado (invisible, basado en score)
- [x] CAPTCHA requerido después de 2 intentos fallidos (CAPTCHA_REQUIRED_AFTER_ATTEMPTS = 2)
- [x] Backend valida token con API de Google
- [x] Fail-open si reCAPTCHA no está configurado

---

### ✅ US-18.4: Device Fingerprinting - COMPLETADO

**Archivos creados**:
- **Entidad**: `TrustedDevice.cs` con 12+ propiedades
  - UserId, FingerprintHash, DeviceName, UserAgent, IpAddress, Location
  - CreatedAt, LastUsedAt, LoginCount, IsTrusted, RevokedAt, RevokeReason
  - Métodos: `RecordLogin()`, `Revoke()`, `Trust()`

- **Repository Interface**: `ITrustedDeviceRepository.cs`
  - `GetByFingerprintAsync()`, `GetByUserIdAsync()`, `GetTrustedByUserIdAsync()`
  - `AddAsync()`, `UpdateAsync()`, `DeleteAsync()`
  - `RevokeAllForUserAsync()`, `CountTrustedDevicesAsync()`

- **Repository Implementation**: `TrustedDeviceRepository.cs`
  - Implementación completa con EF Core

- **Entity Configuration**: `TrustedDeviceConfiguration.cs`
  - Tabla: `trusted_devices`
  - Índices: `user_id`, `(user_id, fingerprint_hash)` UNIQUE, `(user_id, is_trusted)`

- **Service Interface**: `IDeviceFingerprintService.cs`
  - `IsDeviceTrustedAsync()`, `GetOrCreateDeviceAsync()`, `RecordLoginAsync()`
  - `RevokeDeviceAsync()`, `RevokeAllDevicesAsync()`, `GetUserDevicesAsync()`
  - `HashFingerprint()`

- **Service Implementation**: `DeviceFingerprintService.cs`
  - `GetOrCreateDeviceAsync()` retorna `(device, isNew)` para alertas
  - `EnforceMaxDevicesLimitAsync()` limita a 10 dispositivos por usuario
  - Logging completo de operaciones

**Archivos modificados**:
- `ApplicationDbContext.cs`: Agregado `DbSet<TrustedDevice>` y configuración
- `ServiceCollectionExtensions.cs`: Registro de `ITrustedDeviceRepository` y `IDeviceFingerprintService`

**Verificación**:
- [x] Tabla TrustedDevices en AuthService DB
- [x] Fingerprint hash con SHA256
- [x] Máximo 10 dispositivos por usuario (auto-remove oldest)
- [x] Dispositivos pueden ser revocados

---

### ✅ US-18.5: Audit Logging a Servicio Externo (SIEM) - COMPLETADO

**Archivos creados**:
- **Interface**: `ISecurityAuditService.cs` con 11 métodos de logging:
  - `LogLoginSuccessAsync()`, `LogLoginFailureAsync()`
  - `LogTwoFactorSuccessAsync()`, `LogTwoFactorFailureAsync()`
  - `LogPasswordChangeAsync()`, `LogAccountLockoutAsync()`
  - `LogNewDeviceLoginAsync()`, `LogRecoveryCodesGeneratedAsync()`
  - `LogRecoveryCodeUsedAsync()`, `LogTwoFactorStatusChangeAsync()`
  - `LogSessionsRevokedAsync()`, `LogSuspiciousActivityAsync()`

- **Implementation**: `SecurityAuditService.cs`
  - Formato: `[SECURITY_AUDIT] {EventType} | Field=Value | ...`
  - Event types constantes para correlación SIEM
  - `MaskEmail()` para privacidad (jo***@domain.com)
  - Compatible con Splunk, Elasticsearch, Datadog, Azure Sentinel

**Event Types**:
- AUTH_LOGIN_SUCCESS, AUTH_LOGIN_FAILURE
- AUTH_2FA_SUCCESS, AUTH_2FA_FAILURE
- AUTH_PASSWORD_CHANGE, AUTH_ACCOUNT_LOCKOUT
- AUTH_NEW_DEVICE, AUTH_RECOVERY_CODES_GEN
- AUTH_RECOVERY_CODE_USED, AUTH_2FA_STATUS_CHANGE
- AUTH_SESSIONS_REVOKED, AUTH_SUSPICIOUS_ACTIVITY

**Archivos modificados**:
- `ServiceCollectionExtensions.cs`: Registro de `ISecurityAuditService`

**Verificación**:
- [x] Logging estructurado compatible con SIEM
- [x] Todos los eventos de seguridad cubiertos
- [x] Email masking para cumplimiento de privacidad
- [x] Severity levels apropiados (Info, Warning, Error)
## 📈 TRACKING DE PROGRESO

### Sprint 13 - Seguridad & Autorización ✅ COMPLETADO

- [x] US-13.1: IP Context Real en AuthService ✅ (IRequestContext + HttpRequestContext con X-Forwarded-For)
- [x] US-13.2: Verificación Real de Permisos ✅ (CheckPermissionQueryHandler con DB query)

### Sprint 14 - Cobertura de Tests ✅ COMPLETADO

- [x] US-14.1: Tests para MediaService
- [x] US-14.2: Tests para NotificationService

### Sprint 15 - Jobs & Automatización ✅ COMPLETADO

- [x] US-15.1: DailyStatsReportJob
- [x] US-15.2: CleanupOldExecutionsJob
- [x] US-15.3: HealthCheckJob
- [x] US-15.4: AdminService Use Cases

### Sprint 16 - Integración & Contratos ✅ COMPLETADO

- [x] US-16.1: Migrar Eventos NotificationService
- [x] US-16.2: Métricas Reales AuditService

### Sprint 17 - Mejoras Operacionales ✅ COMPLETADO

- [x] US-17.1: Integración ClamAV

### Sprint 18 - Seguridad Avanzada 2FA ✅ COMPLETADO

- [x] US-18.1: Recovery Codes Persistencia Dual (Redis + PostgreSQL)
- [x] US-18.2: Notificación de Intentos Fallidos por Email
- [x] US-18.3: CAPTCHA después de 2 Intentos Fallidos
- [x] US-18.4: Device Fingerprinting
- [x] US-18.5: Audit Logging a SIEM

---

## 🎯 RECOMENDACIÓN

**Para ir a producción inmediatamente**:

- ✅ CI/CD ya está listo
- ⚠️ Sprint 13 (Seguridad) es **altamente recomendado** antes de producción

**Sprint 18 (Seguridad Avanzada 2FA)** - 🔴 **ALTA PRIORIDAD POST-LANZAMIENTO**:

- US-18.1 y US-18.2 son los más críticos (notificaciones de seguridad)
- US-18.3 (CAPTCHA) y US-18.4 (Device Fingerprinting) previenen ataques automatizados
- US-18.5 (SIEM) es esencial para monitoreo en producción

**Los sprints 14-17 pueden hacerse post-lanzamiento** como mejoras incrementales.

---

_Documento generado automáticamente - Última actualización: 22 Enero 2026_
