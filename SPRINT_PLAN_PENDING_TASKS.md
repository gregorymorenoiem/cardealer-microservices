# 📋 PLAN DE SPRINTS - TAREAS PENDIENTES

> **Fecha**: 4 de Diciembre, 2025  
> **Estado CI/CD**: ✅ LISTO (25 microservicios, 1,483 tests)  
> **Objetivo**: Completar TODOs pendientes para producción

---

## 📊 RESUMEN EJECUTIVO

| Sprint | Enfoque | Esfuerzo | Prioridad | Estado |
|--------|---------|----------|-----------|--------|
| Sprint 13 | Seguridad & Autorización | 4-6h | 🔴 CRÍTICO | ✅ COMPLETADO |
| Sprint 14 | Cobertura de Tests | 3-4h | 🟠 ALTO | ✅ COMPLETADO |
| Sprint 15 | Jobs & Automatización | 4-5h | 🟡 MEDIO | ⏳ PENDIENTE |
| Sprint 16 | Integración & Contratos | 3-4h | 🟡 MEDIO | ⏳ PENDIENTE |
| Sprint 17 | Mejoras Operacionales | 2-3h | 🟢 BAJO | ⏳ PENDIENTE |

---

## 🔴 SPRINT 13: Seguridad & Autorización (CRÍTICO)

**Objetivo**: Implementar obtención real de IP/UserAgent y verificación de permisos

### US-13.1: IP Context Real en AuthService
**Esfuerzo**: 2-3h | **Archivos**: 6

| # | Archivo | Línea | TODO |
|---|---------|-------|------|
| 1 | `AuthService.Application/Features/Auth/Commands/Login/LoginCommandHandler.cs` | 92-93 | Get actual IP and UserAgent from context |
| 2 | `AuthService.Application/Features/Auth/Commands/Register/RegisterCommandHandler.cs` | 67 | Get actual IP from context |
| 3 | `AuthService.Application/Features/Auth/Commands/RefreshToken/RefreshTokenCommandHandler.cs` | 61 | Get actual IP from context |
| 4 | `AuthService.Application/Features/ExternalAuth/Commands/ExternalAuth/ExternalAuthCommandHandler.cs` | 51 | Get actual IP from context |
| 5 | `AuthService.Application/Features/ExternalAuth/Commands/LinkExternalAccount/LinkExternalAccountCommandHandler.cs` | 68 | Get actual IP from context |
| 6 | `AuthService.Application/Features/ExternalAuth/Commands/ExternalAuthCallback/ExternalAuthCallbackCommandHandler.cs` | 84 | Implement OAuth code exchange |

**Implementación**:
```csharp
// Inyectar IHttpContextAccessor en cada handler
private readonly IHttpContextAccessor _httpContextAccessor;

// Obtener IP real
var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() 
    ?? _httpContextAccessor.HttpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault()
    ?? "unknown";

var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
```

**Criterios de Aceptación**:
- [ ] IP real capturada en todos los comandos de autenticación
- [ ] Soporte para X-Forwarded-For (detrás de proxy/load balancer)
- [ ] UserAgent capturado en Login
- [ ] Tests unitarios actualizados

---

### US-13.2: Verificación Real de Permisos en RoleService
**Esfuerzo**: 2-3h | **Archivos**: 1

| # | Archivo | Línea | TODO |
|---|---------|-------|------|
| 1 | `RoleService.Application/UseCases/RolePermissions/CheckPermission/CheckPermissionQueryHandler.cs` | 24 | Implementar verificación real |

**Implementación Requerida**:
```csharp
// 1. Obtener usuario del JWT claims (userId del token)
// 2. Consultar roles del usuario desde base de datos
// 3. Obtener permisos de cada rol
// 4. Verificar si el permiso solicitado está en la lista
```

**Criterios de Aceptación**:
- [ ] Consulta real a base de datos de roles/permisos
- [ ] Cacheo de permisos por usuario (opcional)
- [ ] Tests de integración con datos reales

---

## 🟠 SPRINT 14: Cobertura de Tests (ALTO) ✅ COMPLETADO

**Objetivo**: Aumentar tests en servicios con baja cobertura  
**Estado**: ✅ COMPLETADO (4 de Diciembre 2025)

### US-14.1: Tests para MediaService ✅
**Esfuerzo**: 1.5-2h | **Estado**: ✅ COMPLETADO

| Área | Tests Agregados | Estado |
|------|-----------------|--------|
| InitUploadCommandHandler | 7 tests (image/video/document types) | ✅ |
| GetMediaQueryHandler | 6 tests (retrieval, filtering) | ✅ |
| DeleteMediaCommandHandler | 5 tests (deletion, variants) | ✅ |

**Resultado**: 21 tests unitarios pasando (+ fix bug ImageMedia width/height)

---

### US-14.2: Tests para NotificationService ✅
**Esfuerzo**: 1.5-2h | **Estado**: ✅ COMPLETADO

| Área | Tests Agregados | Estado |
|------|-----------------|--------|
| SendEmailNotificationCommandHandler | 7 tests (send, failures, metadata) | ✅ |
| GetNotificationsQueryHandler | 8 tests (filtering, pagination) | ✅ |
| SendPushNotificationCommandHandler | 7 tests (send, data payload) | ✅ |

**Resultado**: 22 tests unitarios pasando

---

## 🟡 SPRINT 15: Jobs & Automatización (MEDIO)

**Objetivo**: Implementar lógica real en jobs del SchedulerService

### US-15.1: DailyStatsReportJob
**Esfuerzo**: 1-1.5h

| # | Archivo | Línea | TODO |
|---|---------|-------|------|
| 1 | `SchedulerService.Infrastructure/Jobs/DailyStatsReportJob.cs` | 29 | Implement actual report generation |

**Implementación**:
- Recopilar estadísticas de todos los microservicios
- Generar reporte en formato estructurado
- Enviar por email o almacenar en BD

---

### US-15.2: CleanupOldExecutionsJob
**Esfuerzo**: 1-1.5h

| # | Archivo | Línea | TODO |
|---|---------|-------|------|
| 1 | `SchedulerService.Infrastructure/Jobs/CleanupOldExecutionsJob.cs` | 35 | Implement actual cleanup logic |

**Implementación**:
- Definir política de retención (ej: 30 días)
- Limpiar logs/ejecuciones antiguas
- Registrar métricas de limpieza

---

### US-15.3: HealthCheckJob
**Esfuerzo**: 1-1.5h

| # | Archivo | Línea | TODO |
|---|---------|-------|------|
| 1 | `SchedulerService.Infrastructure/Jobs/HealthCheckJob.cs` | 31 | Implement actual health check logic |

**Implementación**:
- Consultar /health de cada microservicio
- Registrar resultados en BD
- Alertar si algún servicio está down

---

### US-15.4: AdminService Use Cases
**Esfuerzo**: 1-1.5h

| # | Archivo | Línea | TODO |
|---|---------|-------|------|
| 1 | `AdminService.Application/UseCases/Reports/ResolveReport/ResolveReportCommandHandler.cs` | 27 | Implementar resolución de reporte |
| 2 | `AdminService.Application/UseCases/Vehicles/ApproveVehicle/ApproveVehicleCommandHandler.cs` | 27 | Implementar aprobación |
| 3 | `AdminService.Application/UseCases/Vehicles/RejectVehicle/RejectVehicleCommandHandler.cs` | 27 | Implementar rechazo |

---

## 🟡 SPRINT 16: Integración & Contratos (MEDIO)

**Objetivo**: Migrar eventos a CarDealer.Contracts

### US-16.1: Migrar Eventos de NotificationService
**Esfuerzo**: 2-3h

| # | Archivo | Línea | TODO |
|---|---------|-------|------|
| 1 | `NotificationService.Infrastructure/Messaging/RabbitMQNotificationConsumer.cs` | 17 | Replace AuthService.Shared events with CarDealer.Contracts |
| 2 | `NotificationService.Domain/Interfaces/IPushNotificationService.cs` | 7 | Uncomment when PushNotificationEvent is migrated |

**Pasos**:
1. Definir eventos en `CarDealer.Contracts`
2. Actualizar consumers en NotificationService
3. Actualizar publishers en AuthService
4. Verificar compatibilidad

---

### US-16.2: Métricas Reales en AuditService
**Esfuerzo**: 1-1.5h

| # | Archivo | Línea | TODO |
|---|---------|-------|------|
| 1 | `AuditService.Infrastructure/Metrics/AuditServiceMetrics.cs` | 132 | Implementar sesiones activas |
| 2 | `AuditService.Infrastructure/Metrics/AuditServiceMetrics.cs` | 138 | Implementar total de logs |
| 3 | `AuditService.Infrastructure/BackgroundServices/DeadLetterQueueProcessor.cs` | 82 | Implementar republicación |

---

## 🟢 SPRINT 17: Mejoras Operacionales (BAJO)

**Objetivo**: Mejoras de infraestructura y seguridad opcionales

### US-17.1: Integración ClamAV para Escaneo de Virus
**Esfuerzo**: 2-3h

| # | Archivo | Línea | TODO |
|---|---------|-------|------|
| 1 | `FileStorageService.Core/Services/VirusScanService.cs` | 95 | Implement actual ClamAV integration |

**Implementación**:
- Agregar cliente ClamAV
- Configurar conexión a servicio ClamAV
- Escanear archivos antes de almacenar
- Rechazar archivos infectados

**Requisito**: Servicio ClamAV en docker-compose

---

## 📈 TRACKING DE PROGRESO

### Sprint 13 - Seguridad & Autorización
- [ ] US-13.1: IP Context Real en AuthService
- [ ] US-13.2: Verificación Real de Permisos

### Sprint 14 - Cobertura de Tests
- [ ] US-14.1: Tests para MediaService
- [ ] US-14.2: Tests para NotificationService

### Sprint 15 - Jobs & Automatización
- [ ] US-15.1: DailyStatsReportJob
- [ ] US-15.2: CleanupOldExecutionsJob
- [ ] US-15.3: HealthCheckJob
- [ ] US-15.4: AdminService Use Cases

### Sprint 16 - Integración & Contratos
- [ ] US-16.1: Migrar Eventos NotificationService
- [ ] US-16.2: Métricas Reales AuditService

### Sprint 17 - Mejoras Operacionales
- [ ] US-17.1: Integración ClamAV

---

## 🎯 RECOMENDACIÓN

**Para ir a producción inmediatamente**: 
- ✅ CI/CD ya está listo
- ⚠️ Sprint 13 (Seguridad) es **altamente recomendado** antes de producción

**Los sprints 14-17 pueden hacerse post-lanzamiento** como mejoras incrementales.

---

*Documento generado automáticamente - Última actualización: 4 Diciembre 2025*
