# 🗄️ OKLA — Auditoría de Performance de Base de Datos

**Fecha:** 2026-03-06
**Autor:** CPSO (Copilot)
**Scope:** Queries y patrones de acceso a datos en todos los servicios backend

---

## 🔴 Problemas Críticos (P0)

### 1. VehiclesSaleService — Paginación en memoria
- **Repositorio:** `VehicleRepository`
- **Métodos:** `GetBySellerIdAsync`, `GetBySellerId`, `SearchAsync`
- **Problema:** Carga TODOS los vehículos de un vendedor y luego aplica `Skip/Take/OrderBy` en memoria
- **Impacto:** Para un dealer con 500+ vehículos, esto carga toda la data en RAM
- **Solución:** Mover `Skip/Take/OrderBy` antes del `ToListAsync()`

### 2. ComparisonService — N+1 Queries en Compare endpoint
- **Controlador:** `ComparisonController`
- **Problema:** `GetVehiclesForComparison` llama a `FindAsync` en un loop (hasta 5 llamadas secuenciales)
- **Solución:** Usar `WHERE id IN (...)` con una sola query

### 3. BillingService — Queries sin paginación
- **Repositorios:** `InvoiceRepository`, `SubscriptionRepository`, `PaymentRepository`, `TransactionRepository`
- **Problema:** ~10 métodos retornan resultados sin límite. A medida que la data crece, serán table scans
- **Solución:** Agregar parámetros `pageSize/pageNumber` a todos los métodos de listado

---

## 🟡 Problemas Medios (P2)

### 4. KYCService — Doble SaveChanges no atómico
- **Método:** `ApproveVerification`
- **Problema:** Llama a `SaveChangesAsync` dos veces en la misma operación — si el segundo falla, queda data inconsistente
- **Solución:** Usar una sola transacción con `SaveChangesAsync()` al final

### 5. Falta de AsNoTracking en queries de lectura
- **Servicios afectados:** UserService, VehiclesSaleService, BillingService, RecommendationService
- **Problema:** ~20+ métodos de lectura trackean entidades innecesariamente (overhead de memoria y CPU)
- **Solución:** Agregar `.AsNoTracking()` a todas las queries que no modifican datos

### 6. Repository-level SaveChanges (anti-patrón Unit of Work)
- **Afecta a:** Mayoría de repositorios
- **Problema:** Cada `Add/Update/Delete` en el repositorio llama a `SaveChangesAsync` individualmente
- **Solución:** Implementar Unit of Work pattern — `SaveChanges` solo al final del handler/use case

### 7. Favoritos e Historial sin paginación
- **Servicios:** VehiclesSaleService (FavoritesController, HistoryController)
- **Problema:** Carga todos los favoritos/historial de un usuario sin límite
- **Impacto:** Usuarios activos con 100+ favoritos experimentarán lentitud

---

## ✅ Aspectos Positivos

| Aspecto | Estado |
|---------|--------|
| SQL Injection | ✅ Sin riesgo — no hay raw SQL con input de usuario |
| DbContext Lifetime | ✅ Todo correctamente Scoped |
| Eager Loading | ✅ Generalmente bien usado, con filtered Includes |
| FindAsync para PK | ✅ Usado correctamente en la mayoría de casos |

---

## 📋 Recomendaciones para Sprint 18/19

1. **Sprint 18 (Developer):** Corregir paginación en VehiclesSaleService y BillingService
2. **Sprint 18 (CPSO):** Agregar `AsNoTracking()` a queries de lectura en servicios bajo responsabilidad CPSO
3. **Sprint 19:** Implementar Unit of Work pattern en servicios principales
4. **Sprint 19:** Corregir N+1 en ComparisonService
5. **Sprint 19:** Transaccionalizar operaciones en KYCService

---

## 📊 Impacto Estimado de las Correcciones

| Corrección | Impacto en Performance | Esfuerzo |
|------------|----------------------|----------|
| Paginación en DB (VehiclesSaleService) | Alto — reduce memoria de 100MB+ a <1MB por request | Bajo |
| AsNoTracking | Medio — ~15-20% mejora en queries de lectura | Bajo |
| N+1 en Compare | Medio — de 5 queries a 1 query | Bajo |
| Unit of Work | Medio — reduce round-trips a DB | Alto |
| Paginación BillingService | Alto — previene table scans en producción | Medio |
