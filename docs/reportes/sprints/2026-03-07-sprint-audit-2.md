# 📊 Sprint Report — Auditoría de Código #2

**Fecha:** 2026-03-07  
**Sprint:** Auditoría de Código - Fase 2 (Correcciones Críticas)  
**CPSO:** Gregory Moreno  
**Estado:** ✅ Completado

---

## 🎯 Objetivo del Sprint

Continuar corrigiendo los hallazgos de la auditoría #1, enfocándose en: resiliencia de Service Discovery (Consul), corrección de assembly reference en MediaService, y centralización de SecurityValidators.

---

## 📋 Tareas Completadas

| # | Tarea | Estado | Prioridad |
|---|-------|--------|-----------|
| T1 | Agregar Consul `Enabled` flag + NoOp fallback en ErrorService, MediaService, NotificationService | ✅ | 🔴 CRITICAL |
| T2 | Fix MediaService `AddMediatR` assembly reference (Api → Application) | ✅ | 🟠 HIGH |
| T3 | Crear SecurityValidators centralizado en `_Shared/CarDealer.Shared` | ✅ | 🟠 HIGH |

**Completadas: 3/3 (100%)**

---

## 🔧 Cambios Implementados

### T1: Consul NoOp Fallback — 3 Servicios

**Archivos modificados:**
- `ErrorService/ErrorService.Api/Program.cs`
- `MediaService/MediaService.Api/Program.cs`
- `NotificationService/NotificationService.Api/Program.cs`

**Problema:** Estos 3 servicios registraban Consul Client y Service Discovery de forma incondicional, sin verificar `Consul:Enabled`. Si Consul no estaba disponible (desarrollo local, CI, o fallo de Consul en producción), el servicio crasheaba al intentar conectarse.

**Solución:** Aplicado el mismo patrón que ya usaban AdminService y AuthService:
```csharp
var consulEnabled = builder.Configuration.GetValue<bool>("Consul:Enabled", false);
if (consulEnabled) { /* Consul real */ }
else { /* NoOpServiceRegistry + NoOpServiceDiscovery */ }
```

**Impacto:** Los 3 servicios ahora pueden iniciar sin Consul, usando implementaciones NoOp que devuelven fallbacks estáticos. Esto es crítico para:
- Desarrollo local (no necesitas Consul corriendo)
- CI/CD (tests pueden ejecutar sin dependencias externas)
- Resiliencia en producción (si Consul falla, el servicio no crashea)

### T2: MediaService MediatR Assembly Reference

**Archivo modificado:**
- `MediaService/MediaService.Api/ApplicationServiceRegistration.cs`

**Problema:** `Assembly.GetExecutingAssembly()` estaba retornando el assembly de `MediaService.Api`, no `MediaService.Application` donde residen los handlers y validators de MediatR. Esto significaba que los handlers/validators NO estaban siendo registrados correctamente.

**Solución:** Cambié `Assembly.GetExecutingAssembly()` por `typeof(LoggingBehaviour<,>).Assembly` que apunta al assembly correcto de `MediaService.Application` donde vive `LoggingBehaviour`.

**Impacto:** Todos los handlers de MediatR y validators de FluentValidation en `MediaService.Application` ahora serán correctamente descubiertos y registrados.

### T3: SecurityValidators Centralizado

**Archivo creado:**
- `_Shared/CarDealer.Shared/Validation/SecurityValidators.cs`

**Descripción:** Creado un helper centralizado `SecurityValidationHelper` en `CarDealer.Shared.Validation` que contiene la implementación canónica de detección de SQL Injection y XSS. Este helper:
- No depende de FluentValidation (solo C# puro)
- Incluye 26 patrones SQL y 26 patrones XSS
- Incluye regex pre-compilados para XSS avanzado
- Puede ser usado directamente con `.Must()` o como base para extensiones FluentValidation por servicio

**Próximo paso:** Migrar gradualmente los 25+ `SecurityValidators.cs` por servicio para que deleguen a este helper central, eliminando duplicación de código.

---

## 📊 Métricas de Calidad

| Métrica | Antes | Después |
|---------|-------|---------|
| Servicios con Consul NoOp fallback | 2/7 (AdminService, AuthService) | 5/7 ✅ |
| MediatR handlers correctamente registrados (MediaService) | ❌ | ✅ |
| SecurityValidators centralizado en _Shared | ❌ No existía | ✅ Creado |
| Archivos modificados este sprint | — | 5 |
| Archivos creados este sprint | — | 1 |

---

## 🔍 Deuda Técnica Restante

| # | Prioridad | Hallazgo | Servicio(s) |
|---|-----------|----------|-------------|
| 1 | 🔴 CRITICAL | AdminService no registra DbContext — datos en memoria | AdminService |
| 2 | 🔴 CRITICAL | ContactService usa `AddDbContext<T>` en vez de `AddStandardDatabase<T>` | ContactService |
| 3 | 🟠 HIGH | Gateway y ContactService no tienen Consul NoOp fallback | Gateway, ContactService |
| 4 | 🟠 HIGH | AdminService y ContactService sin RabbitMQ/mensajería | AdminService, ContactService |
| 5 | 🟡 MEDIUM | Migrar 25+ SecurityValidators por servicio al helper centralizado | Todos |
| 6 | 🟡 MEDIUM | AdminService y MediaService baja cobertura de tests | AdminService, MediaService |
| 7 | 🟢 LOW | Renombrar `EfXxxRepository` → `InMemoryXxxRepository` en AdminService | AdminService |

---

## 🔮 Prioridades del Próximo Sprint

1. Investigar features competitivas vs Facebook Marketplace y SuperCarros
2. Registrar DbContext en AdminService
3. Migrar SecurityValidators de servicios al helper centralizado
4. Aumentar cobertura de tests en AdminService y MediaService
