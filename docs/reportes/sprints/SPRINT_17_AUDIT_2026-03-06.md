# 🔍 OKLA — Auditoría Completa del Codebase — Sprint 17 Pre-planning

**Fecha:** 2026-03-06  
**Autor:** CPSO (Copilot)  
**Scope:** Backend (19 servicios), Frontend (Next.js 16), Gateway (Ocelot), Shared Libraries

---

## 📊 Resumen Ejecutivo

Se auditaron **19 servicios backend**, el **frontend Next.js**, la **configuración del Gateway Ocelot**, y las **15 shared libraries**. Los hallazgos principales son:

| Severidad                   | Cantidad    | Descripción                                                                                             |
| --------------------------- | ----------- | ------------------------------------------------------------------------------------------------------- |
| 🔴 Seguridad Crítica        | 5 issues    | Credenciales hardcodeadas, servicios sin autenticación, connection strings en logs                      |
| 🟠 Infraestructura Faltante | 4 servicios | KYCService, ChatbotService, ReportsService, CRMService sin extensiones estándar                         |
| 🟡 Cumplimiento Parcial     | 5 servicios | ErrorService, RoleService, AuditService, UserService, VehiclesSaleService con implementaciones manuales |
| 🟢 Cumplimiento Total       | 5 servicios | SearchAgent, SupportAgent, RecoAgent, AdminService, ContactService                                      |

---

## 🔴 HALLAZGOS DE SEGURIDAD CRÍTICOS

### 1. Gateway — Credenciales RabbitMQ "guest" como fallback

- **Archivo:** `Gateway.Api/Program.cs`
- **Línea:** Configuración RabbitMQ
- **Problema:** Si las variables de entorno faltan, el Gateway se conecta con `user=guest, password=guest`
- **Impacto:** Acceso no autorizado a RabbitMQ si los secretos de K8s no están configurados
- **Solución:** Lanzar `InvalidOperationException` si las credenciales no están presentes

### 2. VehiclesSaleService — Connection string expuesta en logs

- **Archivo:** `VehiclesSaleService.Api/Program.cs`
- **Problema:** `Console.WriteLine($"🗄️ Database: {connectionString}")` imprime la connection string completa (incluyendo contraseña) a stdout
- **Impacto:** Credenciales de base de datos expuestas en logs de contenedores, accesibles via `kubectl logs`
- **Solución:** Eliminar el `Console.WriteLine` o reemplazarlo con log de Serilog con el string ofuscado

### 3. AuditService — Sin autenticación

- **Archivo:** `AuditService.Api/Program.cs`
- **Problema:** No tiene JWT Bearer configurado. Cualquier cliente puede leer/escribir logs de auditoría
- **Impacto:** Violación de integridad de los registros de auditoría; incumplimiento de Ley 155-17
- **Solución:** Agregar JWT Bearer con políticas de autorización

### 4. ReportsService — Sin autenticación

- **Archivo:** `ReportsService.Api/Program.cs`
- **Problema:** Llama a `UseAuthorization()` pero nunca configura JWT Bearer
- **Impacto:** Los reportes del sistema son accesibles sin autenticación
- **Solución:** Agregar configuración JWT Bearer estándar

### 5. VehiclesSaleService/BillingService/CRMService — JWT fallbacks hardcodeados

- **Problema:** Issuer/Audience tienen valores fallback hardcodeados (`"CarDealerPlatform"`, `"OKLA-Dev"`)
- **Impacto:** Si los secretos de K8s faltan, el servicio arranca con valores inseguros
- **Solución:** Lanzar excepciones si la configuración JWT no está presente

---

## 🟠 SERVICIOS SIN INFRAESTRUCTURA ESTÁNDAR

### KYCService — Servicio más no-conforme

| Extensión                    | Estado                                                              |
| ---------------------------- | ------------------------------------------------------------------- |
| `UseStandardSerilog()`       | ❌ No usa — logs no centralizados                                   |
| `AddStandardObservability()` | ❌ No usa — invisible en Jaeger/Prometheus                          |
| `AddGlobalErrorHandling()`   | ❌ No usa — errores no publicados a ErrorService                    |
| `AddAuditPublisher()`        | ❌ No usa — sin trail de auditoría                                  |
| `AddAuditMiddleware()`       | ❌ No usa — sin logging de requests                                 |
| Health checks triple         | ❌ Solo `/health` sin filtro de predicado                           |
| HTTPS Redirect               | ⚠️ Activo incluso dentro del cluster (debería manejarse en Ingress) |

**Riesgo especial:** KYCService maneja datos de cumplimiento (Ley 155-17, KYC/AML). Es crítico que tenga auditoría completa y trazabilidad.

### ChatbotService — Segundo más no-conforme

| Extensión                    | Estado                                                          |
| ---------------------------- | --------------------------------------------------------------- |
| `UseStandardSerilog()`       | ❌ Usa Serilog manual con file logging                          |
| `AddStandardObservability()` | ❌ No usa                                                       |
| `AddGlobalErrorHandling()`   | ❌ No usa                                                       |
| `AddAuditMiddleware()`       | ❌ No usa                                                       |
| Health checks                | ⚠️ Incluye checks externos (PostgreSQL, Redis) sin filtrar      |
| `EnsureCreated()`            | ⚠️ Usa `EnsureCreated()` en startup — riesgoso para migraciones |

### ReportsService — Bare-bones

- Sin observabilidad, sin error handling, sin auditoría, sin JWT configurado
- Migraciones solo en Development

### CRMService — Infraestructura faltante

- Serilog manual, sin observabilidad, sin error handling
- Audit middleware llamado ANTES de `UseAuthentication()` — no tendrá contexto de usuario
- Swagger incondicional en producción

---

## 🟡 SERVICIOS CON CUMPLIMIENTO PARCIAL

| Servicio            | Problema Principal                                                                  |
| ------------------- | ----------------------------------------------------------------------------------- |
| ErrorService        | Serilog y OpenTelemetry manuales en lugar de shared libs                            |
| RoleService         | Serilog y OpenTelemetry manuales, usa `IConsulClient` incondicional                 |
| AuditService        | Sin auth, error handler después de `UseEndpoints()`                                 |
| UserService         | Puertos downstream incorrectos (`:80` en lugar de `:8080`), URLs localhost fallback |
| VehiclesSaleService | Sin DLQ, JWT sin `UseStandardJwt()`, connection string en logs                      |
| BillingService      | `ClockSkew = 5min` (otros usan Zero), sin DLQ, Swagger en producción                |

---

## 🔵 HALLAZGOS DE VALIDACIÓN

### ValidationBehavior faltante en 7 servicios

Estos servicios tienen `SecurityValidators.cs` pero **no tienen `ValidationBehavior.cs`**, lo que significa que las validaciones de seguridad NO se ejecutan automáticamente en el pipeline de MediatR:

1. AuditService
2. BillingService
3. ContactService
4. Gateway
5. MediaService
6. ReportsService
7. VehiclesSaleService

### SecurityValidators faltante en 3 servicios

Los Agent services no tienen `SecurityValidators.cs`:

1. SearchAgent
2. SupportAgent
3. RecoAgent

(Nota: Estos servicios probablemente reciben inputs ya validados por el Gateway, pero para defense-in-depth deberían tener validaciones propias.)

---

## ✅ FRONTEND — Estado Excelente

| Aspecto            | Estado                                                        |
| ------------------ | ------------------------------------------------------------- |
| Security Headers   | ✅ CSP, HSTS, X-Frame-Options, X-Content-Type-Options         |
| BFF Pattern        | ✅ Gateway interno, proxy via Next.js rewrites                |
| CSRF               | ✅ Double-submit cookie pattern, auto-injection en API client |
| Error Handling     | ✅ Dual format (ProblemDetails + ApiResponse)                 |
| Testing            | ✅ Vitest + Testing Library + Playwright + MSW                |
| API Client         | ✅ Centralizado con auto CSRF, auto token refresh             |
| Image Optimization | ✅ Sharp, AVIF/WebP, CDN caching                              |

**Observación menor:** Zustand fue eliminado del `package.json` (Sprint 16.6) y las coding guidelines deberían actualizarse para reflejar que se usa React Context + TanStack Query en lugar de Zustand.

---

## 📊 GATEWAY — Estado Sólido

| Aspecto          | Estado                                                                  |
| ---------------- | ----------------------------------------------------------------------- |
| Downstream ports | ✅ Todos los 278 routes usan puerto 8080                                |
| Services routed  | ✅ 24 microservicios registrados                                        |
| JWT + Cookies    | ✅ Cookie-to-Bearer transformation                                      |
| CSRF             | ✅ Validación middleware                                                |
| Rate Limiting    | ⚠️ Solo a nivel de Gateway, sin límites por ruta en endpoints sensibles |
| CORS             | ✅ Whitelist de dominios de producción                                  |

---

## 📋 RECOMENDACIONES PARA SPRINT 17

### Prioridad 1 — Seguridad (Developer)

1. Eliminar fallback "guest" en Gateway RabbitMQ
2. Eliminar `Console.WriteLine` de connection string en VehiclesSaleService
3. Agregar JWT authentication a AuditService y ReportsService
4. Eliminar JWT fallbacks hardcodeados en VehiclesSaleService, BillingService, CRMService

### Prioridad 2 — Estandarización (Developer)

5. Migrar KYCService a extensiones compartidas
6. Migrar ChatbotService a extensiones compartidas

### Prioridad 3 — Validación (CPSO)

7. Agregar ValidationBehavior a los 7 servicios que no lo tienen
8. Estandarizar CRMService y ReportsService con extensiones compartidas

### Prioridad 4 — Documentación (CPSO)

9. Actualizar copilot-instructions para reflejar estado actual
10. Research de features premium para dealers

---

## 📈 MÉTRICAS DE CALIDAD

| Métrica                          | Valor                                 |
| -------------------------------- | ------------------------------------- |
| Servicios totalmente conformes   | 5/19 (26%)                            |
| Servicios con seguridad crítica  | 5 issues                              |
| Servicios sin observabilidad     | 4/19 (21%)                            |
| Servicios sin ValidationBehavior | 7/19 (37%)                            |
| Test projects existentes         | 33 (21 in-service + 12 centralizados) |
| Shared libraries                 | 15                                    |
| Gateway routes                   | 278                                   |
| Frontend packages                | ~60                                   |
