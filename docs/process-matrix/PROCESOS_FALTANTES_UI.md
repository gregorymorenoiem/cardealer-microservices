# 🔴 Procesos Faltantes - Acceso UI

> **Última Actualización:** Enero 25, 2026  
> **Fuente:** Auditoría ESTADO_REAL_IMPLEMENTACION.md  
> **Verificación:** Cruce Backend (71 servicios) vs Frontend (98 rutas)  
> **Estado:** PENDIENTE DE IMPLEMENTACIÓN

---

## 📋 Resumen Ejecutivo

Este documento registra los procesos que están documentados en la matriz pero **NO TIENEN ACCESO COMPLETO DESDE EL FRONTEND (UI)**.

| Prioridad | Procesos Faltantes | Roles Afectados                | Backend | UI  | Impacto    |
| --------- | ------------------ | ------------------------------ | ------- | --- | ---------- |
| 🔴 **P0** | 15                 | ADM-COMP, ADM-MOD, ADM-SUPPORT | ✅/🔴   | 🔴  | Bloqueante |
| 🟠 **P1** | 8                  | DLR-ADMIN, ADM-SUPER           | ✅      | 🔴  | Alto       |
| 🟡 **P2** | 5                  | USR-SELLER, USR-REG            | ✅      | 🟡  | Medio      |

### Resumen de Rutas Faltantes

| Área                  | Rutas Necesarias | Backend Existe  | Estado     |
| --------------------- | ---------------- | --------------- | ---------- |
| `/admin/compliance/*` | 7 páginas        | ✅ 13 servicios | 🔴 0% UI   |
| `/admin/support/*`    | 4 páginas        | ❌ 0% backend   | 🔴 0% TODO |
| `/admin/moderation/*` | 3 páginas        | ✅ Parcial      | 🔴 25% UI  |
| `/admin/audit/*`      | 2 páginas        | ✅ AuditService | 🔴 0% UI   |
| `/dealer/employees`   | 3 páginas        | 🟡 Parcial      | 🔴 0% UI   |

---

## 🔴 PRIORIDAD P0 - CRÍTICA (Bloqueante Legal/Operacional)

### 1. ComplianceService (ADM-COMP: 0% UI)

**Servicios Backend (TODOS EXISTEN ✅):**

- ComplianceService (Puerto 5073)
- AntiMoneyLaunderingService (Puerto 5074)
- ComplianceReportingService (Puerto 5075)
- TaxComplianceService (Puerto 5076)
- ConsumerProtectionService
- DataProtectionService
- DigitalSignatureService
- ECommerceComplianceService
- VehicleRegistryService
- LegalDocumentService
- ContractService
- DisputeService
- RegulatoryAlertService

| Código       | Proceso                | Descripción                        | Ruta UI Propuesta             |
| ------------ | ---------------------- | ---------------------------------- | ----------------------------- |
| **COMP-001** | Reporte 607 DGII       | Generación de NCF y reporte fiscal | `/admin/compliance/dgii-607`  |
| **COMP-002** | Reportes UAF           | Alertas AML/Ley 155-17             | `/admin/compliance/aml`       |
| **COMP-003** | Dashboard Compliance   | Métricas regulatorias              | `/admin/compliance/dashboard` |
| **STR-001**  | Suspicious Transaction | Formulario de reporte sospechoso   | `/admin/compliance/str/new`   |
| **RISK-001** | Risk Assessment        | Dashboard de riesgos               | `/admin/compliance/risks`     |
| **WL-001**   | Watchlist Check        | Consulta de listas de vigilancia   | `/admin/compliance/watchlist` |
| **CAL-001**  | Regulatory Calendar    | Calendario de obligaciones         | `/admin/compliance/calendar`  |

**Backend:** ✅ 13 servicios de compliance EXISTEN  
**Frontend:** ❌ 0 páginas - CREAR EN `/admin/compliance/*`

---

### 2. SupportService (ADM-SUPPORT: 0% Backend + 0% UI)

**⚠️ ÚNICO SERVICIO SIN BACKEND - CREAR COMPLETO**

| Código              | Proceso          | Descripción                 | Ruta UI Propuesta             |
| ------------------- | ---------------- | --------------------------- | ----------------------------- |
| **HELP-FAQ-001**    | FAQ Management   | CRUD de artículos FAQ       | `/admin/support/faq`          |
| **HELP-TICKET-001** | Ticket Dashboard | Gestión de tickets          | `/admin/support/tickets`      |
| **HELP-TICKET-002** | Ticket Detail    | Ver/responder ticket        | `/admin/support/tickets/{id}` |
| **HELP-CHAT-001**   | Live Chat Admin  | Chat en vivo administración | `/admin/support/chat`         |
| **HELP-PUBLIC-001** | Help Center      | Centro de ayuda público     | `/help`                       |
| **HELP-PUBLIC-002** | My Tickets       | Mis tickets (usuario)       | `/help/tickets`               |

**Backend:** ❌ SupportService NO EXISTE  
**Frontend:** ❌ NO EXISTE

**Estructura Requerida:**

```
backend/SupportService/
├── SupportService.Api/
│   └── Controllers/
│       ├── ArticlesController.cs
│       ├── TicketsController.cs
│       └── LiveChatController.cs
├── SupportService.Application/
├── SupportService.Domain/
│   └── Entities/
│       ├── Ticket.cs
│       ├── Article.cs
│       └── Category.cs
└── SupportService.Infrastructure/

frontend/web/src/pages/
├── help/
│   ├── HelpCenterPage.tsx
│   ├── ArticlePage.tsx
│   └── MyTicketsPage.tsx
└── admin/support/
    ├── SupportDashboardPage.tsx
    ├── TicketListPage.tsx
    └── FAQManagementPage.tsx
```

---

### 3. ModerationService (ADM-MOD: 35% UI)

**Backend:** AdminService ✅ tiene endpoints de moderación  
**Páginas Existentes:**

- ✅ `/admin/pending` - Listados pendientes
- ✅ `/admin/listings` - Todos los listados

| Código      | Proceso          | Descripción                         | Ruta UI Propuesta              | Estado   |
| ----------- | ---------------- | ----------------------------------- | ------------------------------ | -------- |
| **MOD-001** | Moderation Queue | Cola priorizada de moderación       | `/admin/moderation/queue`      | 🔴 FALTA |
| **MOD-002** | Content Reports  | Reportes de contenido de usuarios   | `/admin/moderation/reports`    | 🔴 FALTA |
| **MOD-003** | User Moderation  | Historial de moderación por usuario | `/admin/moderation/users/{id}` | 🔴 FALTA |

---

## 🟠 PRIORIDAD P1 - ALTA (Afecta Operación)

### 4. Dealer Employees (DLR-ADMIN)

| Código      | Proceso              | Descripción                   | Ruta UI Propuesta                    |
| ----------- | -------------------- | ----------------------------- | ------------------------------------ |
| **EMP-001** | Employee List        | Lista de empleados del dealer | `/dealer/employees`                  |
| **EMP-002** | Employee Create      | Invitar nuevo empleado        | `/dealer/employees/invite`           |
| **EMP-003** | Employee Permissions | Permisos por empleado         | `/dealer/employees/{id}/permissions` |

**Backend:** DealerManagementService ✅ EXISTE  
**Frontend:** ❌ NO EXISTE

---

### 5. Audit & System Admin (ADM-SUPER)

| Código        | Proceso          | Descripción                | Ruta UI Propuesta     |
| ------------- | ---------------- | -------------------------- | --------------------- |
| **AUDIT-001** | Audit Logs       | Ver logs de auditoría      | `/admin/audit`        |
| **AUDIT-002** | Audit Search     | Buscar en logs             | `/admin/audit/search` |
| **MAINT-001** | Maintenance Mode | Activar modo mantenimiento | `/admin/maintenance`  |
| **DISP-001**  | Disputes         | Gestión de disputas        | `/admin/disputes`     |
| **CONT-001**  | Contracts        | Gestión de contratos       | `/admin/contracts`    |

**Backend:** ✅ AuditService, MaintenanceService, DisputeService, ContractService EXISTEN  
**Frontend:** ❌ NO EXISTEN páginas

## 🟢 PRIORIDAD MEDIA - Mejoras

### 6. NotificationService - Preferencias (USR-REG)

| Código        | Proceso                  | Descripción                 | Ruta UI Propuesta         |
| ------------- | ------------------------ | --------------------------- | ------------------------- |
| **NOTIF-001** | Notification Preferences | Configurar preferencias     | `/settings/notifications` |
| **NOTIF-002** | Notification History     | Historial de notificaciones | `/notifications/history`  |
| **NOTIF-003** | Unsubscribe              | Darse de baja               | `/unsubscribe`            |

**Backend:** NotificationService ✅ EXISTE  
**Frontend:** 🟡 PARCIAL - Solo `/notifications`, falta configuración detallada

---

### 7. AlertService - Gestión Completa (USR-REG)

| Código        | Proceso          | Descripción             | Ruta UI Propuesta  |
| ------------- | ---------------- | ----------------------- | ------------------ |
| **ALERT-001** | Saved Searches   | Búsquedas guardadas     | `/alerts/searches` |
| **ALERT-002** | Price Alerts     | Alertas de precio       | `/alerts/prices`   |
| **ALERT-003** | Alert Statistics | Estadísticas de alertas | `/alerts/stats`    |

**Backend:** AlertService ✅ EXISTE  
**Frontend:** ✅ EXISTE en `/alerts` - Verificar funcionalidad completa

---

## 📊 Métricas de Brecha por Rol

| Rol             | Procesos Documentados | Con UI | Sin UI | % Completitud |
| --------------- | --------------------- | ------ | ------ | ------------- |
| **ADM-COMP**    | 12                    | 0      | 12     | 🔴 0%         |
| **ADM-MOD**     | 8                     | 2      | 6      | 🔴 25%        |
| **ADM-SUPPORT** | 16                    | 0      | 16     | 🔴 0%         |
| **DLR-ADMIN**   | 10                    | 7      | 3      | 🟡 70%        |
| **USR-SELLER**  | 8                     | 5      | 3      | 🟡 63%        |
| **USR-REG**     | 12                    | 8      | 4      | 🟡 67%        |

---

## 🎯 Plan de Acción Propuesto

### Sprint Prioridad 1: SupportService (Crítico)

```
Semana 1-2:
1. Crear SupportService backend
2. Crear páginas /help/* (público)
3. Crear páginas /admin/support/* (admin)
```

### Sprint Prioridad 2: ComplianceService UI (Crítico)

```
Semana 3-4:
1. Crear páginas /admin/compliance/*
2. Dashboard de compliance
3. Formulario 607 DGII
```

### Sprint Prioridad 3: Moderation Queue (Crítico)

```
Semana 5:
1. Crear /admin/moderation/queue
2. Mejorar flujo de moderación
```

### Sprint Prioridad 4: Dealer Employees (Alta)

```
Semana 6:
1. Crear /dealer/employees
2. CRUD de empleados
3. Gestión de permisos
```

---

## 📝 Referencias

- [PROCESO_VS_MENU_ANALISIS.md](../analysis/PROCESO_VS_MENU_ANALISIS.md) - Análisis original
- [App.tsx](../../frontend/web/src/App.tsx) - Rutas actuales
- [Navbar.tsx](../../frontend/web/src/components/organisms/Navbar.tsx) - Menú actual

---

**Última actualización:** Enero 25, 2026  
**Próxima revisión:** Febrero 15, 2026
