# 🚀 Plan de Sprints - Mejoras Backend CarDealer Microservices

## 📊 Configuración del Modelo AI

| Parámetro | Valor |
|-----------|-------|
| **Modelo** | Claude Opus 4.5 |
| **Context Window (Input)** | 128,000 tokens |
| **Max Output** | 16,000 tokens |
| **Multiplier** | 1x |
| **Tokens Disponibles por Sesión** | ~110,000 tokens útiles |

---

## 📈 Metodología de Estimación de Tokens

### Fórmulas Base

```
Tokens de Lectura por archivo .csproj = ~200-400 tokens
Tokens de Escritura (modificación) = ~50-100 tokens por archivo
Tokens de Contexto base = ~8,000 tokens
Buffer de Seguridad = 15%
```

### Multiplicadores por Complejidad

| Nivel | Multiplicador | Descripción |
|-------|--------------|-------------|
| Simple | 1.0x | Cambio de versión directo |
| Medio | 1.3x | Múltiples archivos, verificación |
| Complejo | 1.6x | Refactoring de código + dependencias |
| Crítico | 2.0x | Cambios de seguridad con testing |

---

## 📋 INVENTARIO DE CAMBIOS REQUERIDOS

### 🔴 CRÍTICOS (Seguridad)

| Archivo | Problema | Acción |
|---------|----------|--------|
| `AuthService.Infrastructure.csproj` | `Microsoft.AspNetCore.Identity 2.3.1` | ELIMINAR |
| `CarDealer.Shared.csproj` | `Microsoft.AspNetCore.Http.Abstractions 2.2.0` | ELIMINAR |
| `RoleService.Shared.csproj` | `Microsoft.AspNetCore.Http.Abstractions 2.3.0` | ELIMINAR |
| `UserService.Shared.csproj` | `Microsoft.AspNetCore.Http.Abstractions 2.3.0` | ELIMINAR |
| `ErrorService.Shared.csproj` | `Microsoft.AspNetCore.Http.Abstractions 2.3.0` | ELIMINAR |
| `MediaService.Infrastructure.csproj` | `Microsoft.AspNetCore.Http 2.2.2` | ELIMINAR |

### 🟡 INCONSISTENCIAS (Swashbuckle ~25 archivos)

| Versión Actual | Cantidad | Archivos |
|----------------|----------|----------|
| 6.4.0 | 12 | TracingService, ServiceDiscovery, SearchService, etc. |
| 6.5.0 | 8 | UserService, RoleService, ProductService, etc. |
| 6.6.2 | 5 | RealEstateService, MarketingService, etc. |
| **Target: 6.8.1** | **25** | TODOS |

### 🟡 INCONSISTENCIAS (Consul ~17 archivos)

| Versión Actual | Cantidad |
|----------------|----------|
| 1.7.14.3 | 14 |
| 1.7.14.9 | 3 |
| **Target: 1.7.14.9** | **17** |

### 🟡 INCONSISTENCIAS (MediatR ~30+ archivos)

| Versión Actual | Cantidad |
|----------------|----------|
| 12.2.0 | ~25 |
| 12.4.1 | ~5 |
| 13.1.0 | 1 (LoggingService) |
| **Target: 12.4.1** | **30+** |

### 🟡 INCONSISTENCIAS (FluentValidation ~20 archivos)

| Versión Actual | Cantidad |
|----------------|----------|
| 11.9.0 | ~18 |
| 11.3.0 (AspNetCore) | 1 |
| 12.0.0 | 1 |
| **Target: 11.11.0** | **20** |

### 🟡 OTROS PAQUETES

| Paquete | Actual | Target | Archivos |
|---------|--------|--------|----------|
| StackExchange.Redis | 2.7.33/2.8.16 | 2.8.22 | 4 |
| Polly | 8.4.2 | 8.5.2 | 10 |
| Hangfire | 1.8.14 | 1.8.17 | 3 |

---

## 🎯 SPRINT 1: Eliminación de Paquetes Obsoletos (Seguridad)
**Duración:** 1 día | **Prioridad:** 🔴 CRÍTICA | **Riesgo:** ALTO

### Tarea 1.1: Eliminar Microsoft.AspNetCore.Identity 2.3.1

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | 1 |
| **Archivo** | `AuthService.Infrastructure.csproj` |
| **Líneas a leer** | ~50 |
| **Líneas a modificar** | 1-3 |
| **Complejidad** | Crítico (2.0x) - Requiere verificar código dependiente |

**Estimación de Tokens:**
```
Lectura csproj: 50 × 4 = 200 tokens
Lectura código dependiente: ~2,000 tokens (buscar usings)
Escritura: 10 × 5 = 50 tokens
Contexto: 8,000 tokens
Total: (200 + 2,000 + 50 + 8,000) × 1.15 × 2.0 = 23,575 tokens
```

**Acción Requerida:**
```xml
<!-- ELIMINAR esta línea -->
<PackageReference Include="Microsoft.AspNetCore.Identity" Version="2.3.1" />

<!-- AGREGAR si es necesario (solo si se usa Identity) -->
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.11" />
```

| ✅ Cabe en 1 sesión | Tokens: ~23,600 |
|---------------------|-----------------|

---

### Tarea 1.2: Eliminar Microsoft.AspNetCore.Http.Abstractions (4 archivos)

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | 4 |
| **Archivos** | CarDealer.Shared, RoleService.Shared, UserService.Shared, ErrorService.Shared |
| **Líneas a leer** | ~200 (50 × 4) |
| **Líneas a modificar** | 4 |
| **Complejidad** | Medio (1.3x) |

**Estimación de Tokens:**
```
Lectura: 200 × 4 + 4 × 500 = 2,800 tokens
Escritura: 20 × 5 = 100 tokens
Contexto: 8,000 tokens
Total: (2,800 + 100 + 8,000) × 1.15 × 1.3 = 16,289 tokens
```

**Acción en cada archivo:**
```xml
<!-- ELIMINAR - .NET 8 incluye esto en FrameworkReference -->
<PackageReference Include="Microsoft.AspNetCore.Http.Abstractions" Version="2.x.x" />
```

| ✅ Cabe en 1 sesión | Tokens: ~16,300 |
|---------------------|-----------------|

---

### Tarea 1.3: Eliminar Microsoft.AspNetCore.Http 2.2.2

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | 1 |
| **Archivo** | `MediaService.Infrastructure.csproj` |
| **Líneas a leer** | ~50 |
| **Líneas a modificar** | 1 |
| **Complejidad** | Simple (1.0x) |

**Estimación de Tokens:**
```
Lectura: 50 × 4 + 1 × 500 = 700 tokens
Escritura: 5 × 5 = 25 tokens
Contexto: 8,000 tokens
Total: (700 + 25 + 8,000) × 1.15 = 10,034 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~10,000 |
|---------------------|-----------------|

---

### 📋 Resumen Sprint 1

| Tarea | Tokens Est. | Sesiones | Archivos |
|-------|-------------|----------|----------|
| 1.1 Eliminar Identity 2.3.1 | 23,600 | 1 | 1 |
| 1.2 Eliminar Http.Abstractions | 16,300 | 1 | 4 |
| 1.3 Eliminar Http 2.2.2 | 10,000 | 1 | 1 |
| **TOTAL SPRINT 1** | **49,900** | **3** | **6** |

**💡 Optimización:** Las tareas 1.2 y 1.3 pueden combinarse en 1 sesión (~26,300 tokens).

---

## 🎯 SPRINT 2: Unificación de Swashbuckle
**Duración:** 1-2 días | **Prioridad:** 🟡 MEDIA

### Tarea 2.1: Actualizar Swashbuckle Grupo 6.4.0 → 6.8.1

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | 12 |
| **Servicios** | TracingService, ServiceDiscovery, SearchService, SchedulerService, RateLimiting, MessageBus, MediaService, NotificationService, LoggingService, HealthCheck, etc. |
| **Complejidad** | Simple (1.0x) |

**Archivos específicos:**
```
TracingService.Api.csproj
ServiceDiscovery.Api.csproj
SearchService.Api.csproj
SchedulerService.Api.csproj
RateLimitingService.Api.csproj
MessageBusService.Api.csproj
MediaService.Api.csproj
NotificationService.Api.csproj
LoggingService.Api.csproj
HealthCheckService.Api.csproj
+ 2 más
```

**Estimación de Tokens:**
```
Lectura: 12 × 300 = 3,600 tokens
Escritura: 12 × 50 = 600 tokens
Contexto: 8,000 tokens
Total: (3,600 + 600 + 8,000) × 1.15 = 14,030 tokens
```

**Cambio en cada archivo:**
```xml
<!-- DE: -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.4.0" />
<!-- A: -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.8.1" />
```

| ✅ Cabe en 1 sesión | Tokens: ~14,000 |
|---------------------|-----------------|

---

### Tarea 2.2: Actualizar Swashbuckle Grupo 6.5.0 → 6.8.1

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | 8 |
| **Servicios** | UserService, RoleService, ProductService, ReportsService, InvoicingService, IdempotencyService, CRMService, etc. |
| **Complejidad** | Simple (1.0x) |

**Estimación de Tokens:**
```
Lectura: 8 × 300 = 2,400 tokens
Escritura: 8 × 50 = 400 tokens
Contexto: 8,000 tokens
Total: (2,400 + 400 + 8,000) × 1.15 = 12,420 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~12,400 |
|---------------------|-----------------|

---

### Tarea 2.3: Actualizar Swashbuckle Grupo 6.6.2 → 6.8.1

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | 5 |
| **Servicios** | RealEstateService, MarketingService, FinanceService, IntegrationService, etc. |
| **Complejidad** | Simple (1.0x) |

**Estimación de Tokens:**
```
Lectura: 5 × 300 = 1,500 tokens
Escritura: 5 × 50 = 250 tokens
Contexto: 8,000 tokens
Total: (1,500 + 250 + 8,000) × 1.15 = 11,213 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~11,200 |
|---------------------|-----------------|

---

### 📋 Resumen Sprint 2

| Tarea | Tokens Est. | Sesiones | Archivos |
|-------|-------------|----------|----------|
| 2.1 Swashbuckle 6.4.0 → 6.8.1 | 14,000 | 1 | 12 |
| 2.2 Swashbuckle 6.5.0 → 6.8.1 | 12,400 | 1 | 8 |
| 2.3 Swashbuckle 6.6.2 → 6.8.1 | 11,200 | 1 | 5 |
| **TOTAL SPRINT 2** | **37,600** | **3** | **25** |

**💡 Optimización:** Las 3 tareas pueden combinarse en 1-2 sesiones (~37,600 tokens < 110k).

---

## 🎯 SPRINT 3: Unificación de Consul
**Duración:** 1 día | **Prioridad:** 🟡 MEDIA

### Tarea 3.1: Actualizar Consul 1.7.14.3 → 1.7.14.9

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | 14 |
| **Complejidad** | Simple (1.0x) |

**Archivos:**
```
UserService.Api.csproj
ServiceDiscovery.Infrastructure.csproj
RoleService.Api.csproj
NotificationService.Api.csproj
MessageBusService.Api.csproj
MediaService.Api.csproj
LoggingService.Api.csproj
ErrorService.Api.csproj
AuthService.Api.csproj
ContactService.Api.csproj
ConfigurationService.Api.csproj
AuditService.Api.csproj
CacheService.Api.csproj
AdminService.Api.csproj
```

**Estimación de Tokens:**
```
Lectura: 14 × 300 = 4,200 tokens
Escritura: 14 × 50 = 700 tokens
Contexto: 8,000 tokens
Total: (4,200 + 700 + 8,000) × 1.15 = 14,835 tokens
```

**Cambio:**
```xml
<!-- DE: -->
<PackageReference Include="Consul" Version="1.7.14.3" />
<!-- A: -->
<PackageReference Include="Consul" Version="1.7.14.9" />
```

| ✅ Cabe en 1 sesión | Tokens: ~14,800 |
|---------------------|-----------------|

---

### 📋 Resumen Sprint 3

| Tarea | Tokens Est. | Sesiones | Archivos |
|-------|-------------|----------|----------|
| 3.1 Consul → 1.7.14.9 | 14,800 | 1 | 14 |
| **TOTAL SPRINT 3** | **14,800** | **1** | **14** |

---

## 🎯 SPRINT 4: Unificación de MediatR
**Duración:** 2 días | **Prioridad:** 🟡 MEDIA

### Tarea 4.1: Actualizar MediatR 12.2.0 → 12.4.1

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | ~25 |
| **Complejidad** | Simple (1.0x) |

**Servicios afectados:**
```
UserService (Api, Application, Infrastructure)
ServiceDiscovery (Api, Application)
RoleService (Api, Application, Infrastructure)
NotificationService (Application)
MediaService (Api, Application)
MessageBusService (Api, Application)
+ más servicios
```

**Estimación de Tokens:**
```
Lectura: 25 × 300 = 7,500 tokens
Escritura: 25 × 50 = 1,250 tokens
Contexto: 8,000 tokens
Total: (7,500 + 1,250 + 8,000) × 1.15 = 19,263 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~19,300 |
|---------------------|-----------------|

---

### Tarea 4.2: Downgrade MediatR 13.1.0 → 12.4.1 (LoggingService)

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | 1 |
| **Complejidad** | Medio (1.3x) - Verificar breaking changes |

**Estimación de Tokens:**
```
Lectura: 1 × 300 + 500 (código dependiente) = 800 tokens
Escritura: 50 tokens
Contexto: 8,000 tokens
Total: (800 + 50 + 8,000) × 1.15 × 1.3 = 13,225 tokens
```

⚠️ **ADVERTENCIA:** MediatR 13.x tiene breaking changes. Verificar código antes de downgrade.

| ✅ Cabe en 1 sesión | Tokens: ~13,200 |
|---------------------|-----------------|

---

### 📋 Resumen Sprint 4

| Tarea | Tokens Est. | Sesiones | Archivos |
|-------|-------------|----------|----------|
| 4.1 MediatR 12.2.0 → 12.4.1 | 19,300 | 1 | 25 |
| 4.2 MediatR 13.1.0 → 12.4.1 | 13,200 | 1 | 1 |
| **TOTAL SPRINT 4** | **32,500** | **2** | **26** |

---

## 🎯 SPRINT 5: Unificación de FluentValidation
**Duración:** 1-2 días | **Prioridad:** 🟡 MEDIA

### Tarea 5.1: Actualizar FluentValidation 11.9.0 → 11.11.0

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | ~18 |
| **Complejidad** | Simple (1.0x) |

**Estimación de Tokens:**
```
Lectura: 18 × 300 = 5,400 tokens
Escritura: 18 × 50 = 900 tokens
Contexto: 8,000 tokens
Total: (5,400 + 900 + 8,000) × 1.15 = 16,445 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~16,400 |
|---------------------|-----------------|

---

### Tarea 5.2: Downgrade FluentValidation 12.0.0 → 11.11.0 (AuditService)

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | 1 |
| **Complejidad** | Medio (1.3x) |

**Estimación de Tokens:**
```
Lectura: 300 + 500 = 800 tokens
Escritura: 50 tokens
Contexto: 8,000 tokens
Total: (800 + 50 + 8,000) × 1.15 × 1.3 = 13,225 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~13,200 |
|---------------------|-----------------|

---

### Tarea 5.3: Eliminar FluentValidation.AspNetCore 11.3.0

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | 1 |
| **Archivo** | `MediaService.Infrastructure.csproj` |
| **Complejidad** | Medio (1.3x) |

**Nota:** `FluentValidation.AspNetCore` está deprecated. Usar `FluentValidation.DependencyInjectionExtensions`.

**Estimación de Tokens:**
```
Lectura: 300 tokens
Escritura: 50 tokens
Contexto: 8,000 tokens
Total: (300 + 50 + 8,000) × 1.15 × 1.3 = 12,478 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~12,500 |
|---------------------|-----------------|

---

### 📋 Resumen Sprint 5

| Tarea | Tokens Est. | Sesiones | Archivos |
|-------|-------------|----------|----------|
| 5.1 FluentValidation → 11.11.0 | 16,400 | 1 | 18 |
| 5.2 Downgrade 12.0.0 → 11.11.0 | 13,200 | 1 | 1 |
| 5.3 Eliminar AspNetCore deprecated | 12,500 | 1 | 1 |
| **TOTAL SPRINT 5** | **42,100** | **3** | **20** |

**💡 Optimización:** Todas las tareas pueden combinarse en 1-2 sesiones.

---

## 🎯 SPRINT 6: Actualización de Paquetes Menores
**Duración:** 1 día | **Prioridad:** 🟢 BAJA

### Tarea 6.1: Actualizar StackExchange.Redis → 2.8.22

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | 4 |
| **Archivos** | IntegrationTests, RateLimitingService.Core, RateLimitingService.Api, CacheService.Infrastructure |
| **Complejidad** | Simple (1.0x) |

**Estimación de Tokens:**
```
Lectura: 4 × 300 = 1,200 tokens
Escritura: 4 × 50 = 200 tokens
Contexto: 8,000 tokens
Total: (1,200 + 200 + 8,000) × 1.15 = 10,810 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~10,800 |
|---------------------|-----------------|

---

### Tarea 6.2: Actualizar Polly 8.4.2 → 8.5.2

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | 10 |
| **Complejidad** | Simple (1.0x) |

**Estimación de Tokens:**
```
Lectura: 10 × 300 = 3,000 tokens
Escritura: 10 × 50 = 500 tokens
Contexto: 8,000 tokens
Total: (3,000 + 500 + 8,000) × 1.15 = 13,225 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~13,200 |
|---------------------|-----------------|

---

### Tarea 6.3: Actualizar Hangfire 1.8.14 → 1.8.17

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | 3 |
| **Archivos** | SchedulerService.Infrastructure (2 paquetes), SchedulerService.Api |
| **Complejidad** | Simple (1.0x) |

**Estimación de Tokens:**
```
Lectura: 3 × 300 = 900 tokens
Escritura: 4 × 50 = 200 tokens
Contexto: 8,000 tokens
Total: (900 + 200 + 8,000) × 1.15 = 10,465 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~10,500 |
|---------------------|-----------------|

---

### 📋 Resumen Sprint 6

| Tarea | Tokens Est. | Sesiones | Archivos |
|-------|-------------|----------|----------|
| 6.1 StackExchange.Redis → 2.8.22 | 10,800 | 1 | 4 |
| 6.2 Polly → 8.5.2 | 13,200 | 1 | 10 |
| 6.3 Hangfire → 1.8.17 | 10,500 | 1 | 3 |
| **TOTAL SPRINT 6** | **34,500** | **3** | **17** |

**💡 Optimización:** Las 3 tareas pueden combinarse en 1 sesión (~34,500 tokens).

---

## 🎯 SPRINT 7: Activar Central Package Management
**Duración:** 2-3 días | **Prioridad:** 🟡 MEDIA | **Riesgo:** MEDIO

### Tarea 7.1: Actualizar Directory.Packages.props

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | 1 |
| **Complejidad** | Complejo (1.6x) |

**Estimación de Tokens:**
```
Lectura actual: 500 tokens
Escritura nuevo: 2,000 tokens (agregar todos los paquetes)
Contexto: 8,000 tokens
Total: (500 + 2,000 + 8,000) × 1.15 × 1.6 = 19,320 tokens
```

**Cambios:**
```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  
  <ItemGroup>
    <!-- Core -->
    <PackageVersion Include="MediatR" Version="12.4.1" />
    <PackageVersion Include="FluentValidation" Version="11.11.0" />
    <PackageVersion Include="Swashbuckle.AspNetCore" Version="6.8.1" />
    <PackageVersion Include="Consul" Version="1.7.14.9" />
    <!-- + todos los demás paquetes -->
  </ItemGroup>
</Project>
```

| ✅ Cabe en 1 sesión | Tokens: ~19,300 |
|---------------------|-----------------|

---

### Tarea 7.2: Migrar .csproj a Central Package Management (Lote 1)

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | ~30 |
| **Complejidad** | Medio (1.3x) |

**Cambio en cada .csproj:**
```xml
<!-- DE: -->
<PackageReference Include="MediatR" Version="12.4.1" />
<!-- A: -->
<PackageReference Include="MediatR" />
```

**Estimación de Tokens:**
```
Lectura: 30 × 300 = 9,000 tokens
Escritura: 30 × 200 = 6,000 tokens
Contexto: 8,000 tokens
Total: (9,000 + 6,000 + 8,000) × 1.15 × 1.3 = 34,385 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~34,400 |
|---------------------|-----------------|

---

### Tarea 7.3: Migrar .csproj a Central Package Management (Lote 2)

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | ~30 |
| **Complejidad** | Medio (1.3x) |

**Estimación de Tokens:**
```
Lectura: 30 × 300 = 9,000 tokens
Escritura: 30 × 200 = 6,000 tokens
Contexto: 8,000 tokens
Total: (9,000 + 6,000 + 8,000) × 1.15 × 1.3 = 34,385 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~34,400 |
|---------------------|-----------------|

---

### Tarea 7.4: Migrar .csproj restantes (Lote 3)

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | ~30 |
| **Complejidad** | Medio (1.3x) |

**Estimación de Tokens:** ~34,400 tokens

---

### Tarea 7.5: Migrar .csproj restantes (Lote 4)

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | ~30 |
| **Complejidad** | Medio (1.3x) |

**Estimación de Tokens:** ~34,400 tokens

---

### Tarea 7.6: Migrar .csproj restantes (Lote 5 - Final)

| Métrica | Valor |
|---------|-------|
| **Archivos a modificar** | ~65 (restantes) |
| **Complejidad** | Medio (1.3x) |

**Estimación de Tokens:**
```
Lectura: 65 × 300 = 19,500 tokens
Escritura: 65 × 200 = 13,000 tokens
Contexto: 8,000 tokens
Total: (19,500 + 13,000 + 8,000) × 1.15 × 1.3 = 60,548 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~60,500 |
|---------------------|-----------------|

---

### 📋 Resumen Sprint 7

| Tarea | Tokens Est. | Sesiones | Archivos |
|-------|-------------|----------|----------|
| 7.1 Actualizar Directory.Packages.props | 19,300 | 1 | 1 |
| 7.2 Migrar Lote 1 | 34,400 | 1 | 30 |
| 7.3 Migrar Lote 2 | 34,400 | 1 | 30 |
| 7.4 Migrar Lote 3 | 34,400 | 1 | 30 |
| 7.5 Migrar Lote 4 | 34,400 | 1 | 30 |
| 7.6 Migrar Lote 5 Final | 60,500 | 1 | 65 |
| **TOTAL SPRINT 7** | **217,400** | **6** | **186** |

---

## 🎯 SPRINT 8: Verificación y Testing
**Duración:** 1-2 días | **Prioridad:** 🔴 ALTA

### Tarea 8.1: Ejecutar Build Completo

| Métrica | Valor |
|---------|-------|
| **Acción** | `dotnet build CarDealer.sln` |
| **Complejidad** | Simple (1.0x) |

**Estimación de Tokens:**
```
Comando: 100 tokens
Análisis output: 2,000 tokens
Contexto: 8,000 tokens
Total: (100 + 2,000 + 8,000) × 1.15 = 11,615 tokens
```

| ✅ Cabe en 1 sesión | Tokens: ~11,600 |
|---------------------|-----------------|

---

### Tarea 8.2: Ejecutar Tests

| Métrica | Valor |
|---------|-------|
| **Acción** | `dotnet test` |
| **Complejidad** | Simple (1.0x) |

**Estimación de Tokens:** ~11,600 tokens

---

### Tarea 8.3: Fix de Errores de Build (si aplica)

| Métrica | Valor |
|---------|-------|
| **Archivos potenciales** | Variable |
| **Complejidad** | Complejo (1.6x) |

**Estimación de Tokens:**
```
Por cada error: ~5,000 tokens
Reservar: 30,000 tokens para ~6 errores
```

| ⚠️ Variable | Tokens: ~30,000 (reserva) |
|-------------|---------------------------|

---

### 📋 Resumen Sprint 8

| Tarea | Tokens Est. | Sesiones | Tipo |
|-------|-------------|----------|------|
| 8.1 Build completo | 11,600 | 1 | Verificación |
| 8.2 Ejecutar tests | 11,600 | 1 | Verificación |
| 8.3 Fix errores | 30,000 | 2-3 | Corrección |
| **TOTAL SPRINT 8** | **53,200** | **4-5** | - |

---

## 📊 RESUMEN GLOBAL DEL PLAN

### Vista General por Sprint

| Sprint | Nombre | Tokens Total | Sesiones | Días | Prioridad |
|--------|--------|--------------|----------|------|-----------|
| 1 | Eliminar Paquetes Obsoletos | 49,900 | 3 | 1 | 🔴 Crítica |
| 2 | Unificar Swashbuckle | 37,600 | 3 | 1-2 | 🟡 Media |
| 3 | Unificar Consul | 14,800 | 1 | 1 | 🟡 Media |
| 4 | Unificar MediatR | 32,500 | 2 | 2 | 🟡 Media |
| 5 | Unificar FluentValidation | 42,100 | 3 | 1-2 | 🟡 Media |
| 6 | Actualizar Paquetes Menores | 34,500 | 3 | 1 | 🟢 Baja |
| 7 | Central Package Management | 217,400 | 6 | 2-3 | 🟡 Media |
| 8 | Verificación y Testing | 53,200 | 4-5 | 1-2 | 🔴 Alta |
| **TOTAL** | - | **482,000** | **25-26** | **10-14** | - |

---

### 📈 Distribución de Tokens por Prioridad

```
🔴 CRÍTICA/ALTA:  103,100 tokens  (21.4%)  → Sprints 1, 8
🟡 MEDIA:         344,400 tokens  (71.5%)  → Sprints 2-5, 7
🟢 BAJA:           34,500 tokens  (7.1%)   → Sprint 6
```

---

### ⚡ Optimizaciones Posibles

| Optimización | Ahorro Estimado | Descripción |
|--------------|-----------------|-------------|
| Combinar Sprint 1 (tareas 1.2+1.3) | ~10,000 tokens | Misma sesión |
| Combinar Sprint 2 completo | ~15,000 tokens | 1-2 sesiones en lugar de 3 |
| Combinar Sprint 3+6 | ~10,000 tokens | Cambios simples juntos |
| **TOTAL AHORRO** | **~35,000 tokens** | ~7% reducción |

---

### 📅 Cronograma Recomendado (2 semanas)

```
Semana 1:
├── Día 1: Sprint 1 - Eliminar paquetes obsoletos (CRÍTICO)
├── Día 2: Sprint 2 - Unificar Swashbuckle
├── Día 3: Sprint 3 + Sprint 6 - Consul + Paquetes menores
├── Día 4: Sprint 4 - Unificar MediatR
└── Día 5: Sprint 5 - Unificar FluentValidation

Semana 2:
├── Día 1-2: Sprint 7 (Lotes 1-3) - Central Package Management
├── Día 3-4: Sprint 7 (Lotes 4-6) - Central Package Management
└── Día 5: Sprint 8 - Verificación y Testing
```

---

### 💰 Resumen de Consumo de Tokens

| Métrica | Valor |
|---------|-------|
| **Total Tokens Estimados** | ~482,000 |
| **Sesiones Totales** | 25-26 |
| **Tokens Promedio/Sesión** | ~18,500 |
| **Máximo Tokens/Sesión** | ~60,500 (Sprint 7.6) |
| **Días de Trabajo** | 10-14 |

---

### ✅ Checklist de Validación Post-Sprint

- [ ] `dotnet restore` exitoso
- [ ] `dotnet build CarDealer.sln` sin errores
- [ ] `dotnet test` todos los tests pasan
- [ ] Verificar que no hay warnings de versiones
- [ ] Commit con mensaje descriptivo
- [ ] Actualizar CHANGELOG.md

---

### 📝 Scripts de Validación Rápida

```powershell
# Verificar versiones de paquetes
dotnet list package --outdated

# Build completo
dotnet build CarDealer.sln --configuration Release

# Ejecutar todos los tests
dotnet test CarDealer.sln --no-build --verbosity normal

# Verificar vulnerabilidades
dotnet list package --vulnerable
```

---

### ⚠️ Riesgos y Mitigaciones

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|--------------|---------|------------|
| Breaking changes en MediatR 13→12 | Media | Alto | Verificar código antes de downgrade |
| Incompatibilidad CPM | Baja | Alto | Hacer por lotes, verificar cada lote |
| Tests fallando | Media | Medio | Ejecutar tests después de cada sprint |
| Build errors | Baja | Medio | Reservar sesiones para fixes |

---

*Documento generado el 29 de Diciembre 2025*
*Basado en auditoría de backend CarDealer Microservices*
*Total de archivos .csproj: 185*
