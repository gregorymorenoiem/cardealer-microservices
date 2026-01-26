# 📋 OKLA - Matriz de Procesos por Microservicio

> **Última actualización:** Enero 25, 2026  
> **Total de Microservicios Backend:** 71 servicios  
> **Total de Rutas Frontend:** 98+ rutas  
> **Total de Procesos Documentados:** 600+  
> **Total de Documentos:** 124 archivos

---

## 🏢 MODELO DE NEGOCIO OKLA

> **⚠️ IMPORTANTE:** OKLA es una plataforma de **anuncios clasificados** (estilo SuperCarros.com).  
> **NO** somos intermediarios financieros. **NO** procesamos transacciones de compraventa.  
> Los pagos entre comprador/vendedor son **directos y externos** a la plataforma.

| Concepto               | Descripción                                                 |
| ---------------------- | ----------------------------------------------------------- |
| **Modelo**             | Marketplace de clasificados para vehículos                  |
| **Ingresos**           | Suscripciones dealers, publicaciones destacadas, publicidad |
| **RNC**                | 1-33-32590-1                                                |
| **Registro Mercantil** | 196339PSD                                                   |
| **UAF**                | ❌ No aplica (no somos sujeto obligado)                     |
| **Facturación**        | e-CF con envío automático a DGII                            |

---

## 🚨 AUDITORÍA DE ESTADO REAL - Enero 25, 2026

> **IMPORTANTE:** Esta auditoría cruza Backend existente vs Frontend existente vs Documentación.  
> Ver documento completo: [ESTADO_REAL_IMPLEMENTACION.md](ESTADO_REAL_IMPLEMENTACION.md)

### Resumen Ejecutivo

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    ESTADO REAL DE IMPLEMENTACIÓN OKLA                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Backend Services: 71/71 ✅     Frontend Rutas: 98/112 🟡    Docs: 124 ✅   │
│                                                                             │
│  Estado Global:                                                             │
│  🟢 COMPLETO (Backend + UI + Tests)   ████████████████░░░░ 40%             │
│  🟡 PARCIAL (Backend OK, UI Parcial)  ████████████░░░░░░░░ 35%             │
│  🔴 CRÍTICO (Sin UI o Sin Backend)    ████████░░░░░░░░░░░░ 25%             │
│                                                                             │
│  📊 Módulo de Auditoría y Cumplimiento (12 docs)                           │
│  📋 Compliance Legal RD expandido (16 docs) ⬆️ +6 nuevos                   │
│  🧾 e-CF + Envío Automático DGII implementado                              │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 📊 Progreso Real por Categoría

| #      | Categoría                     | Docs      | Backend   | Frontend   | Estado Real |
| ------ | ----------------------------- | --------- | --------- | ---------- | ----------- |
| 01     | AUTENTICACIÓN-SEGURIDAD       | 6/6       | ✅ 100%   | ✅ 100%    | 🟢 **100%** |
| 02     | USUARIOS-DEALERS              | 6/6       | ✅ 100%   | ✅ 100%    | 🟢 **100%** |
| 03     | VEHÍCULOS-INVENTARIO          | 6/6       | ✅ 100%   | ✅ 100%    | 🟢 **100%** |
| 04     | BÚSQUEDA-RECOMENDACIONES      | 5/5       | ✅ 100%   | ✅ 100%    | 🟢 **100%** |
| 04B    | BÚSQUEDA-FILTROS              | 1/1       | ✅ 100%   | ✅ 100%    | 🟢 **100%** |
| 04C    | PROPIEDADES-INMUEBLES         | 2/2       | ✅ 100%   | 🔴 0%      | 🟡 **50%**  |
| 05     | PAGOS-FACTURACIÓN             | 6/6       | ✅ 100%   | ✅ 90%     | 🟢 **95%**  |
| 05B    | AGENDAMIENTO                  | 1/1       | ✅ 100%   | ✅ 80%     | 🟢 **90%**  |
| 06     | CRM-LEADS-CONTACTOS           | 5/5       | ✅ 100%   | ✅ 80%     | 🟡 **90%**  |
| 06B    | PAGOS-FACTURACIÓN (NCF)       | 1/1       | ✅ 100%   | 🟡 60%     | 🟡 **80%**  |
| 07     | NOTIFICACIONES                | 5/5       | ✅ 100%   | 🟡 60%     | 🟡 **80%**  |
| 07B    | REVIEWS-REPUTACIÓN            | 1/1       | ✅ 100%   | 🟡 70%     | 🟡 **85%**  |
| 08     | **COMPLIANCE-LEGAL-RD** ⭐    | **16/16** | ✅ 100%   | 🔴 **0%**  | 🟡 **60%**  |
| 09     | REPORTES-ANALYTICS            | 5/5       | ✅ 100%   | 🟡 60%     | 🟡 **80%**  |
| 09B    | NOTIFICACIONES (Consent)      | 1/1       | ✅ 100%   | 🟡 50%     | 🟡 **75%**  |
| 10     | MEDIA-ARCHIVOS                | 4/4       | ✅ 100%   | ✅ 95%     | 🟢 **98%**  |
| 11     | INFRAESTRUCTURA-DEVOPS        | 14/14     | ✅ 100%   | N/A        | 🟢 **100%** |
| 12     | ADMINISTRACIÓN                | 7/7       | 🟡 80%    | 🔴 **35%** | 🔴 **58%**  |
| 13     | INTEGRACIONES-EXTERNAS        | 5/5       | ✅ 100%   | 🟡 80%     | 🟢 **90%**  |
| 14     | FINANCIAMIENTO-TRADEIN        | 4/4       | 🟡 70%    | 🟡 60%     | 🟡 **65%**  |
| 15     | CONFIANZA-SEGURIDAD           | 6/6       | 🟡 80%    | 🟡 50%     | 🟡 **65%**  |
| 16     | PROMOCIÓN-VISIBILIDAD         | 1/1       | 🟡 60%    | 🔴 30%     | 🟡 **45%**  |
| 17     | ENGAGEMENT-RETENCIÓN          | 4/4       | 🟡 60%    | 🔴 40%     | 🟡 **50%**  |
| 18     | SEGUROS                       | 1/1       | 🔴 30%    | 🔴 20%     | 🔴 **25%**  |
| 19     | SOPORTE                       | 2/2       | 🔴 **0%** | 🔴 **0%**  | 🔴 **0%**   |
| 20     | PRICING-INTELLIGENCE          | 2/2       | 🟡 70%    | 🟡 50%     | 🟡 **60%**  |
| 21     | REVIEWS-REPUTACIÓN (Dealer)   | 1/1       | ✅ 100%   | 🟡 70%     | 🟡 **85%**  |
| 22     | COMUNICACIÓN-REALTIME         | 1/1       | 🟡 80%    | 🟡 60%     | 🟡 **70%**  |
| **25** | **AUDITORÍA-CUMPLIMIENTO** 🆕 | **12/12** | 🔴 **0%** | 🔴 **0%**  | 📄 **DOC**  |
|        | **TOTAL**                     | **124**   | **92%**   | **70%**    | **🟡 78%**  |

---

## 🆕 NUEVO: Módulo de Auditoría y Cumplimiento (Enero 25, 2026)

### Carpeta: `25-AUDITORIA-CUMPLIMIENTO/` (12 documentos)

Este módulo proporciona una visión completa desde la perspectiva de un **auditor externo** para verificar cumplimiento con las leyes dominicanas.

| #   | Documento                                                                                        | Descripción                                  | Prioridad  |
| --- | ------------------------------------------------------------------------------------------------ | -------------------------------------------- | ---------- |
| 00  | [README.md](25-AUDITORIA-CUMPLIMIENTO/README.md)                                                 | Índice del módulo de auditoría               | 🔴 Crítica |
| 01  | [01-RESUMEN-EJECUTIVO.md](25-AUDITORIA-CUMPLIMIENTO/01-RESUMEN-EJECUTIVO.md)                     | Dashboard ejecutivo para auditores           | 🔴 Crítica |
| 02  | [02-MATRIZ-OBLIGACIONES-LEGALES.md](25-AUDITORIA-CUMPLIMIENTO/02-MATRIZ-OBLIGACIONES-LEGALES.md) | 51 obligaciones legales mapeadas             | 🔴 Crítica |
| 03  | [03-CALENDARIO-FISCAL-REPORTES.md](25-AUDITORIA-CUMPLIMIENTO/03-CALENDARIO-FISCAL-REPORTES.md)   | Calendario con fechas límite DGII            | 🔴 Crítica |
| 04  | [04-AUDITORIA-DGII.md](25-AUDITORIA-CUMPLIMIENTO/04-AUDITORIA-DGII.md)                           | Checklist completo DGII (NCF, 606/607/608)   | 🔴 Crítica |
| 05  | [05-AUDITORIA-UAF.md](25-AUDITORIA-CUMPLIMIENTO/05-AUDITORIA-UAF.md)                             | Checklist UAF/AML (KYC, ROS)                 | 🔴 Crítica |
| 06  | [06-AUDITORIA-PROTECCION-DATOS.md](25-AUDITORIA-CUMPLIMIENTO/06-AUDITORIA-PROTECCION-DATOS.md)   | Checklist Ley 172-13 (ARCO, consentimientos) | 🔴 Crítica |
| 07  | [07-AUDITORIA-PROCONSUMIDOR.md](25-AUDITORIA-CUMPLIMIENTO/07-AUDITORIA-PROCONSUMIDOR.md)         | Checklist Pro Consumidor (quejas, retracto)  | 🟡 Alta    |
| 08  | [08-REPORTES-AUTOMATIZADOS.md](25-AUDITORIA-CUMPLIMIENTO/08-REPORTES-AUTOMATIZADOS.md)           | Especificación técnica de 25 reportes        | 🔴 Crítica |
| 09  | [09-EVIDENCIAS-CONTROLES.md](25-AUDITORIA-CUMPLIMIENTO/09-EVIDENCIAS-CONTROLES.md)               | Catálogo de 72 evidencias, 16 controles      | 🟡 Alta    |
| 10  | [10-MICROSERVICIOS-AUDITORIA.md](25-AUDITORIA-CUMPLIMIENTO/10-MICROSERVICIOS-AUDITORIA.md)       | Arquitectura de 4 microservicios nuevos      | 🔴 Crítica |
| 11  | [11-DASHBOARD-AUDITORIA-UI.md](25-AUDITORIA-CUMPLIMIENTO/11-DASHBOARD-AUDITORIA-UI.md)           | Especificación UI del dashboard              | 🟡 Alta    |

### Brechas Críticas Identificadas

| Regulador          | Cumplimiento Actual | Estado / Notas                                           |
| ------------------ | ------------------- | -------------------------------------------------------- |
| **DGII**           | 85% 🟢              | ✅ e-CF implementado + envío automático reportes         |
| **UAF**            | **N/A**             | ❌ No aplica - OKLA no es sujeto obligado (clasificados) |
| **Ley 172-13**     | 40% 🟡              | ARCO parcial, sin registro de tratamientos               |
| **Pro Consumidor** | 35% 🟡              | Sin sistema de quejas ni libro reclamaciones             |

### Microservicios Planificados (Documentados)

| Servicio               | Puerto | Función                                   | Estado         |
| ---------------------- | ------ | ----------------------------------------- | -------------- |
| AuditService           | 5070   | Centralizar evidencias y auditorías       | 📄 Documentado |
| ComplianceService      | 5071   | Protección datos, ARCO                    | 📄 Documentado |
| FiscalReportingService | 5072   | Formatos DGII automatizados (606/607/608) | 📄 Documentado |
| DataProtectionService  | 5073   | ARCO y protección de datos                | 📄 Documentado |

---

## 🟡 BRECHAS PENDIENTES (Por Prioridad)

> **Nota:** UAF/AML eliminado - OKLA no es sujeto obligado (plataforma de clasificados).

| #   | Área                 | Problema                 | Rol Afectado | Backend | UI     | Prioridad |
| --- | -------------------- | ------------------------ | ------------ | ------- | ------ | --------- |
| 1   | **Soporte**          | SupportService NO EXISTE | ADM-SUPPORT  | 🔴 0%   | 🔴 0%  | 🔴 P0     |
| 2   | **Compliance UI**    | 16 servicios sin páginas | ADM-COMP     | ✅ 100% | 🔴 0%  | 🔴 P0     |
| 3   | **Auditoría UI**     | 12 docs sin implementar  | ADM-AUDIT    | 🔴 0%   | 🔴 0%  | 🔴 P0     |
| 4   | **Moderación Queue** | Sin cola priorizada      | ADM-MOD      | 🟡 60%  | 🔴 25% | 🟠 P1     |
| 5   | **Dealer Employees** | Sin gestión de staff     | DLR-ADMIN    | 🔴 0%   | 🔴 0%  | 🟠 P1     |

### ✅ RESUELTO: Facturación Electrónica DGII

| Item                     | Estado  | Descripción                             |
| ------------------------ | ------- | --------------------------------------- |
| e-CF (Comprobantes)      | ✅ 100% | E31, E32, E34, E47 implementados        |
| Envío Automático DGII    | ✅ 100% | Web Services para 606, 609, IT-1, IR-17 |
| Libros Contables         | ✅ 100% | Automatización con integración e-CF     |
| Formato 607              | ✅ 100% | Auto-generado por DGII desde e-CF       |
| Reducción tiempo mensual | ✅ 95%  | De 10-15 hrs → 30 min supervisión       |

### Rutas Admin Faltantes (15 rutas)

```
❌ /admin/audit/dashboard           # Dashboard de auditoría
❌ /admin/audit/obligations         # Calendario obligaciones
❌ /admin/audit/evidences           # Gestión evidencias
❌ /admin/audit/reports             # Centro de reportes
❌ /admin/compliance/dashboard
❌ /admin/compliance/dgii-ecf       # Dashboard e-CF (nuevo)
❌ /admin/compliance/risks
❌ /admin/moderation/queue
❌ /admin/moderation/reports
❌ /admin/support/tickets
❌ /admin/support/faq
❌ /admin/disputes
❌ /admin/contracts
❌ /admin/maintenance
❌ /dealer/employees
```

---

## 📋 Estado por Rol de Usuario

| Rol              | Descripción         | Backend | UI Access | Estado     |
| ---------------- | ------------------- | ------- | --------- | ---------- |
| **USR-ANON**     | Visitante anónimo   | ✅ 100% | ✅ 100%   | 🟢 100%    |
| **USR-REG**      | Usuario registrado  | ✅ 100% | ✅ 90%    | 🟢 95%     |
| **USR-SELLER**   | Vendedor individual | ✅ 100% | ✅ 85%    | 🟢 93%     |
| **DLR-STAFF**    | Staff de dealer     | ✅ 100% | ✅ 85%    | 🟢 93%     |
| **DLR-ADMIN**    | Admin de dealer     | ✅ 95%  | 🟡 75%    | 🟡 85%     |
| **ADM-ADMIN**    | Administrador       | ✅ 90%  | 🟡 70%    | 🟡 80%     |
| **ADM-SUPER**    | Superadmin          | ✅ 90%  | 🟡 55%    | 🟡 73%     |
| **ADM-MOD**      | Moderador           | 🟡 60%  | 🔴 35%    | 🔴 **48%** |
| **ADM-SUPPORT**  | Soporte             | 🔴 0%   | 🔴 0%     | 🔴 **0%**  |
| **ADM-COMP**     | Compliance          | ✅ 100% | 🔴 0%     | 🔴 **50%** |
| **ADM-AUDIT** 🆕 | Auditor             | 📄 Doc  | 🔴 0%     | 📄 **DOC** |

### ✅ Documentos Completados (118/118 - 100%)

#### 01-AUTENTICACION-SEGURIDAD (6/6) ⬆️

1. ✅ [01-auth-service.md](01-AUTENTICACION-SEGURIDAD/01-auth-service.md) - Autenticación, JWT, OAuth
2. ✅ [02-role-service.md](01-AUTENTICACION-SEGURIDAD/02-role-service.md) - Roles y permisos RBAC
3. ✅ [03-security-2fa.md](01-AUTENTICACION-SEGURIDAD/03-security-2fa.md) - Two-Factor Authentication
4. ✅ [04-kyc-service.md](01-AUTENTICACION-SEGURIDAD/04-kyc-service.md) - Know Your Customer
5. ✅ [05-session-security.md](01-AUTENTICACION-SEGURIDAD/05-session-security.md) - Seguridad de sesiones 🆕
6. ✅ [06-unlink-active-provider.md](01-AUTENTICACION-SEGURIDAD/06-unlink-active-provider.md) - Desvinculación proveedores 🆕

#### 02-USUARIOS-DEALERS (6/6) ⬆️

5. ✅ [01-user-service.md](02-USUARIOS-DEALERS/01-user-service.md) - Gestión de usuarios
6. ✅ [02-dealer-management.md](02-USUARIOS-DEALERS/02-dealer-management.md) - Gestión de dealers
7. ✅ [03-dealer-analytics.md](02-USUARIOS-DEALERS/03-dealer-analytics.md) - Analytics para dealers
8. ✅ [04-dealer-onboarding.md](02-USUARIOS-DEALERS/04-dealer-onboarding.md) - Onboarding de dealers
9. ✅ [05-seller-profiles.md](02-USUARIOS-DEALERS/05-seller-profiles.md) - Perfiles de vendedores
10. ✅ [06-derechos-arco.md](02-USUARIOS-DEALERS/06-derechos-arco.md) - Derechos ARCO usuarios 🆕

#### 03-VEHICULOS-INVENTARIO (6/6) ⬆️

11. ✅ [01-vehicles-sale-service.md](03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md) - Venta de vehículos
12. ✅ [02-vehicles-rent-service.md](03-VEHICULOS-INVENTARIO/02-vehicles-rent-service.md) - Alquiler de vehículos
13. ✅ [03-inventory-management.md](03-VEHICULOS-INVENTARIO/03-inventory-management.md) - Gestión de inventario
14. ✅ [04-vehicle-intelligence.md](03-VEHICULOS-INVENTARIO/04-vehicle-intelligence.md) - IA de precios/demanda
15. ✅ [05-catalog-favorites-homepage.md](03-VEHICULOS-INVENTARIO/05-catalog-favorites-homepage.md) - Catálogo, favoritos, homepage
16. ✅ [06-media-360-video.md](03-VEHICULOS-INVENTARIO/06-media-360-video.md) - Media 360° y video 🆕

#### 04-BUSQUEDA-RECOMENDACIONES (5/5)

17. ✅ [01-search-service.md](04-BUSQUEDA-RECOMENDACIONES/01-search-service.md) - Motor de búsqueda Elasticsearch
18. ✅ [02-recommendation-service.md](04-BUSQUEDA-RECOMENDACIONES/02-recommendation-service.md) - Sistema de recomendaciones
19. ✅ [03-comparison-service.md](04-BUSQUEDA-RECOMENDACIONES/03-comparison-service.md) - Comparador de vehículos
20. ✅ [04-alert-service.md](04-BUSQUEDA-RECOMENDACIONES/04-alert-service.md) - Alertas de precio/búsqueda
21. ✅ [05-feature-store.md](04-BUSQUEDA-RECOMENDACIONES/05-feature-store.md) - Feature store para ML

#### 04-BUSQUEDA-FILTROS (1/1)

22. ✅ [03-filtros-avanzados.md](04-BUSQUEDA-FILTROS/03-filtros-avanzados.md) - Filtros avanzados de búsqueda

#### 04-PROPIEDADES-INMUEBLES (2/2)

23. ✅ [01-properties-sale-service.md](04-PROPIEDADES-INMUEBLES/01-properties-sale-service.md) - Propiedades en venta
24. ✅ [02-properties-rent-service.md](04-PROPIEDADES-INMUEBLES/02-properties-rent-service.md) - Propiedades en renta

#### 05-PAGOS-FACTURACION (6/6)

25. ✅ [01-billing-service.md](05-PAGOS-FACTURACION/01-billing-service.md) - Facturación principal
26. ✅ [02-stripe-payment.md](05-PAGOS-FACTURACION/02-stripe-payment.md) - Pagos con Stripe
27. ✅ [03-azul-payment.md](05-PAGOS-FACTURACION/03-azul-payment.md) - Pagos con AZUL (RD)
28. ✅ [04-invoicing-service.md](05-PAGOS-FACTURACION/04-invoicing-service.md) - Generación de facturas
29. ✅ [05-escrow-service.md](05-PAGOS-FACTURACION/05-escrow-service.md) - Escrow/Custodia de pagos
30. ✅ [06-subscriptions.md](05-PAGOS-FACTURACION/06-subscriptions.md) - Suscripciones de dealers

#### 05-AGENDAMIENTO (1/1)

31. ✅ [02-testdrive-scheduling.md](05-AGENDAMIENTO/02-testdrive-scheduling.md) - Agendamiento de test drives

#### 06-CRM-LEADS-CONTACTOS (5/5)

32. ✅ [01-crm-service.md](06-CRM-LEADS-CONTACTOS/01-crm-service.md) - CRM (Leads, Deals, Pipelines)
33. ✅ [02-contact-service.md](06-CRM-LEADS-CONTACTOS/02-contact-service.md) - Sistema de contactos/mensajería
34. ✅ [03-lead-scoring.md](06-CRM-LEADS-CONTACTOS/03-lead-scoring.md) - Lead scoring IA
35. ✅ [04-chatbot-service.md](06-CRM-LEADS-CONTACTOS/04-chatbot-service.md) - Chatbot IA + WhatsApp
36. ✅ [05-appointment-service.md](06-CRM-LEADS-CONTACTOS/05-appointment-service.md) - Test drives y citas

#### 06-PAGOS-FACTURACION NCF (1/1) 🆕

37. ✅ [06-ncf-comprobantes-fiscales.md](06-PAGOS-FACTURACION/06-ncf-comprobantes-fiscales.md) - NCF y comprobantes fiscales 🆕

#### 07-NOTIFICACIONES (5/5) ⬆️

38. ✅ [01-notification-service.md](07-NOTIFICACIONES/01-notification-service.md) - Sistema de notificaciones
39. ✅ [02-notificacion-vehiculo-vendido.md](07-NOTIFICACIONES/02-notificacion-vehiculo-vendido.md) - Notificación vehículo vendido 🆕
40. ✅ [02-templates-scheduling.md](07-NOTIFICACIONES/02-templates-scheduling.md) - Templates y programación
41. ✅ [03-marketing-service.md](07-NOTIFICACIONES/03-marketing-service.md) - Marketing automation
42. ✅ [04-teams-integration.md](07-NOTIFICACIONES/04-teams-integration.md) - Integración MS Teams

#### 07-REVIEWS-REPUTACION (1/1)

43. ✅ [01-review-service.md](07-REVIEWS-REPUTACION/01-review-service.md) - Reviews y reputación

#### 08-COMPLIANCE-LEGAL-RD (16/16) ⬆️ ⭐ EXPANDIDO

> **Nota:** Este módulo incluye toda la documentación fiscal y de cumplimiento para OKLA.  
> OKLA es plataforma de clasificados, NO intermediario financiero. UAF no aplica.

##### Leyes y Regulaciones (01-09)

44. ✅ [01-compliance-service.md](08-COMPLIANCE-LEGAL-RD/01-compliance-service.md) - Compliance RD general
45. ✅ [01-ley-155-17.md](08-COMPLIANCE-LEGAL-RD/01-ley-155-17.md) - Ley 155-17 Anti-Lavado (referencia, no aplica)
46. ✅ [02-ley-172-13.md](08-COMPLIANCE-LEGAL-RD/02-ley-172-13.md) - Ley 172-13 Protección Datos
47. ✅ [03-dgii-integration.md](08-COMPLIANCE-LEGAL-RD/03-dgii-integration.md) - Integración DGII
48. ✅ [04-proconsumidor.md](08-COMPLIANCE-LEGAL-RD/04-proconsumidor.md) - Pro Consumidor
49. ✅ [05-compliance-reports.md](08-COMPLIANCE-LEGAL-RD/05-compliance-reports.md) - Reportes regulatorios
50. ✅ [06-ley-126-02-comercio-electronico.md](08-COMPLIANCE-LEGAL-RD/06-ley-126-02-comercio-electronico.md) - Ley 126-02 Comercio Electrónico
51. ✅ [07-ley-63-17-intrant.md](08-COMPLIANCE-LEGAL-RD/07-ley-63-17-intrant.md) - Ley 63-17 INTRANT
52. ✅ [08-obligaciones-fiscales-dgii.md](08-COMPLIANCE-LEGAL-RD/08-obligaciones-fiscales-dgii.md) - Obligaciones fiscales DGII
53. ✅ [09-ros-reporte-operaciones-sospechosas.md](08-COMPLIANCE-LEGAL-RD/09-ros-reporte-operaciones-sospechosas.md) - ROS (referencia, no aplica)

##### Procedimientos Fiscales OKLA (10-15) 🆕

54. ✅ [10-PROCEDIMIENTO-FISCAL-OKLA.md](08-COMPLIANCE-LEGAL-RD/10-PROCEDIMIENTO-FISCAL-OKLA.md) - Procedimiento fiscal completo 🆕
55. ✅ [11-REGISTRO-GASTOS-OPERATIVOS.md](08-COMPLIANCE-LEGAL-RD/11-REGISTRO-GASTOS-OPERATIVOS.md) - Registro gastos (606) 🆕
56. ✅ [12-AUTOMATIZACION-REPORTES-DGII.md](08-COMPLIANCE-LEGAL-RD/12-AUTOMATIZACION-REPORTES-DGII.md) - Automatización + e-CF 🆕
57. ✅ [13-PREPARACION-AUDITORIA-DGII.md](08-COMPLIANCE-LEGAL-RD/13-PREPARACION-AUDITORIA-DGII.md) - Preparación auditoría 🆕
58. ✅ [14-E-CF-COMPROBANTES-ELECTRONICOS.md](08-COMPLIANCE-LEGAL-RD/14-E-CF-COMPROBANTES-ELECTRONICOS.md) - e-CF + Envío automático DGII 🆕
59. ✅ [15-LIBROS-CONTABLES-AUTOMATIZACION.md](08-COMPLIANCE-LEGAL-RD/15-LIBROS-CONTABLES-AUTOMATIZACION.md) - Libros contables + e-CF 🆕

#### 09-REPORTES-ANALYTICS (5/5)

60. ✅ [01-reports-service.md](09-REPORTES-ANALYTICS/01-reports-service.md) - Reportes generales
61. ✅ [02-analytics-service.md](09-REPORTES-ANALYTICS/02-analytics-service.md) - Analytics y métricas
62. ✅ [03-event-tracking.md](09-REPORTES-ANALYTICS/03-event-tracking.md) - Event tracking
63. ✅ [04-dashboards.md](09-REPORTES-ANALYTICS/04-dashboards.md) - Dashboards ejecutivos
64. ✅ [05-regulatory-alerts.md](09-REPORTES-ANALYTICS/05-regulatory-alerts.md) - Alertas regulatorias

#### 09-NOTIFICACIONES (1/1) 🆕

65. ✅ [05-consentimiento-comunicaciones.md](09-NOTIFICACIONES/05-consentimiento-comunicaciones.md) - Consentimiento comunicaciones 🆕

#### 10-MEDIA-ARCHIVOS (4/4)

66. ✅ [01-media-service.md](10-MEDIA-ARCHIVOS/01-media-service.md) - Gestión de media
67. ✅ [02-image-processing.md](10-MEDIA-ARCHIVOS/02-image-processing.md) - Procesamiento de imágenes
68. ✅ [03-document-storage.md](10-MEDIA-ARCHIVOS/03-document-storage.md) - Almacenamiento de documentos
69. ✅ [04-multimedia-processing.md](10-MEDIA-ARCHIVOS/04-multimedia-processing.md) - Video y multimedia

#### 11-INFRAESTRUCTURA-DEVOPS (14/14)

70. ✅ [01-gateway-service.md](11-INFRAESTRUCTURA-DEVOPS/01-gateway-service.md) - API Gateway (Ocelot)
71. ✅ [02-error-service.md](11-INFRAESTRUCTURA-DEVOPS/02-error-service.md) - Gestión de errores
72. ✅ [02-service-discovery.md](11-INFRAESTRUCTURA-DEVOPS/02-service-discovery.md) - Consul service discovery
73. ✅ [04-health-checks.md](11-INFRAESTRUCTURA-DEVOPS/04-health-checks.md) - Health checks
74. ✅ [05-logging-service.md](11-INFRAESTRUCTURA-DEVOPS/05-logging-service.md) - Logging centralizado
75. ✅ [06-rate-limiting.md](11-INFRAESTRUCTURA-DEVOPS/06-rate-limiting.md) - Rate limiting
76. ✅ [07-caching-service.md](11-INFRAESTRUCTURA-DEVOPS/07-caching-service.md) - Redis caching
77. ✅ [08-queue-management.md](11-INFRAESTRUCTURA-DEVOPS/08-queue-management.md) - RabbitMQ
78. ✅ [09-deployment.md](11-INFRAESTRUCTURA-DEVOPS/09-deployment.md) - CI/CD y deployment
79. ✅ [10-monitoring.md](11-INFRAESTRUCTURA-DEVOPS/10-monitoring.md) - Monitoreo Prometheus/Grafana
80. ✅ [10-scheduler-service.md](11-INFRAESTRUCTURA-DEVOPS/10-scheduler-service.md) - Scheduler (Quartz.NET)
81. ✅ [11-configuration-service.md](11-INFRAESTRUCTURA-DEVOPS/11-configuration-service.md) - Configuración centralizada
82. ✅ [12-feature-toggle.md](11-INFRAESTRUCTURA-DEVOPS/12-feature-toggle.md) - Feature flags
83. ✅ [13-idempotency.md](11-INFRAESTRUCTURA-DEVOPS/13-idempotency.md) - Sistema de idempotencia

#### 12-ADMINISTRACION (7/7)

84. ✅ [01-admin-service.md](12-ADMINISTRACION/01-admin-service.md) - Panel de administración
85. ✅ [02-admin-users.md](12-ADMINISTRACION/02-admin-users.md) - Gestión de usuarios admin
86. ✅ [03-maintenance-mode.md](12-ADMINISTRACION/03-maintenance-mode.md) - Modo mantenimiento
87. ✅ [03-system-config.md](12-ADMINISTRACION/03-system-config.md) - Configuración del sistema
88. ✅ [04-audit-service.md](12-ADMINISTRACION/04-audit-service.md) - Auditoría del sistema
89. ✅ [04-feature-flags.md](12-ADMINISTRACION/04-feature-flags.md) - Feature flags admin
90. ✅ [05-error-service.md](12-ADMINISTRACION/05-error-service.md) - Dashboard de errores

#### 13-INTEGRACIONES-EXTERNAS (5/5)

91. ✅ [01-whatsapp-integration.md](13-INTEGRACIONES-EXTERNAS/01-whatsapp-integration.md) - WhatsApp Business API
92. ✅ [02-sms-integration.md](13-INTEGRACIONES-EXTERNAS/02-sms-integration.md) - SMS (Twilio)
93. ✅ [03-email-providers.md](13-INTEGRACIONES-EXTERNAS/03-email-providers.md) - Email (SendGrid/SES)
94. ✅ [04-maps-integration.md](13-INTEGRACIONES-EXTERNAS/04-maps-integration.md) - Google Maps
95. ✅ [05-social-auth.md](13-INTEGRACIONES-EXTERNAS/05-social-auth.md) - OAuth social (Google/Apple)

#### 14-FINANCIAMIENTO-TRADEIN (4/4)

96. ✅ [01-calculadora-financiamiento.md](14-FINANCIAMIENTO-TRADEIN/01-calculadora-financiamiento.md) - Calculadora financiamiento
97. ✅ [02-trade-in-estimador.md](14-FINANCIAMIENTO-TRADEIN/02-trade-in-estimador.md) - Estimador trade-in
98. ✅ [03-historial-vehiculo.md](14-FINANCIAMIENTO-TRADEIN/03-historial-vehiculo.md) - Historial vehículos
99. ✅ [04-calculadora-costos-totales.md](14-FINANCIAMIENTO-TRADEIN/04-calculadora-costos-totales.md) - Calculadora costos

#### 15-CONFIANZA-SEGURIDAD (6/6)

100. ✅ [01-verificacion-identidad.md](15-CONFIANZA-SEGURIDAD/01-verificacion-identidad.md) - Verificación identidad
101. ✅ [02-garantia-inspeccion.md](15-CONFIANZA-SEGURIDAD/02-garantia-inspeccion.md) - Garantía e inspección
102. ✅ [03-devolucion-cancelacion.md](15-CONFIANZA-SEGURIDAD/03-devolucion-cancelacion.md) - Devolución y cancelación
103. ✅ [04-disputas-mediacion.md](15-CONFIANZA-SEGURIDAD/04-disputas-mediacion.md) - Disputas y mediación
104. ✅ [05-okla-certified.md](15-CONFIANZA-SEGURIDAD/05-okla-certified.md) - OKLA Certified
105. ✅ [06-derecho-retracto.md](15-CONFIANZA-SEGURIDAD/06-derecho-retracto.md) - Derecho de retracto 🆕

#### 16-PROMOCION-VISIBILIDAD (1/1)

106. ✅ [01-boost-destacado.md](16-PROMOCION-VISIBILIDAD/01-boost-destacado.md) - Boost y destacado

#### 17-ENGAGEMENT-RETENCION (4/4)

107. ✅ [01-alertas-busquedas-guardadas.md](17-ENGAGEMENT-RETENCION/01-alertas-busquedas-guardadas.md) - Alertas y búsquedas
108. ✅ [02-programa-referidos.md](17-ENGAGEMENT-RETENCION/02-programa-referidos.md) - Programa referidos
109. ✅ [03-onboarding-comprador.md](17-ENGAGEMENT-RETENCION/03-onboarding-comprador.md) - Onboarding comprador
110. ✅ [04-wishlist-compartida.md](17-ENGAGEMENT-RETENCION/04-wishlist-compartida.md) - Wishlist compartida

#### 18-SEGUROS (1/1)

111. ✅ [01-cotizacion-seguro.md](18-SEGUROS/01-cotizacion-seguro.md) - Cotización de seguros

#### 19-SOPORTE (2/2) ⬆️

112. ✅ [01-centro-ayuda.md](19-SOPORTE/01-centro-ayuda.md) - Centro de ayuda
113. ✅ [02-quejas-reclamos.md](19-SOPORTE/02-quejas-reclamos.md) - Quejas y reclamos 🆕

#### 20-PRICING-INTELLIGENCE (2/2)

114. ✅ [01-deal-rating.md](20-PRICING-INTELLIGENCE/01-deal-rating.md) - Deal rating
115. ✅ [02-valuacion-instantanea.md](20-PRICING-INTELLIGENCE/02-valuacion-instantanea.md) - Valuación instantánea

#### 21-REVIEWS-REPUTACION (1/1)

116. ✅ [01-dealer-reviews.md](21-REVIEWS-REPUTACION/01-dealer-reviews.md) - Reviews de dealers

#### 22-COMUNICACION-REALTIME (1/1)

117. ✅ [01-chat-realtime.md](22-COMUNICACION-REALTIME/01-chat-realtime.md) - Chat en tiempo real

#### 25-AUDITORIA-CUMPLIMIENTO (12/12) 🆕 ⭐

> **Nota:** El módulo de auditoría UAF/AML es solo de referencia.  
> OKLA no es sujeto obligado por ser plataforma de clasificados.

118. ✅ [README.md](25-AUDITORIA-CUMPLIMIENTO/README.md) - Índice del módulo 🆕
119. ✅ [01-RESUMEN-EJECUTIVO.md](25-AUDITORIA-CUMPLIMIENTO/01-RESUMEN-EJECUTIVO.md) - Dashboard ejecutivo 🆕
120. ✅ [02-MATRIZ-OBLIGACIONES-LEGALES.md](25-AUDITORIA-CUMPLIMIENTO/02-MATRIZ-OBLIGACIONES-LEGALES.md) - 51 obligaciones legales 🆕
121. ✅ [03-CALENDARIO-FISCAL-REPORTES.md](25-AUDITORIA-CUMPLIMIENTO/03-CALENDARIO-FISCAL-REPORTES.md) - Calendario fiscal DGII 🆕
122. ✅ [04-AUDITORIA-DGII.md](25-AUDITORIA-CUMPLIMIENTO/04-AUDITORIA-DGII.md) - Checklist DGII 🆕
123. ✅ [05-AUDITORIA-UAF.md](25-AUDITORIA-CUMPLIMIENTO/05-AUDITORIA-UAF.md) - Checklist UAF/AML (referencia) 🆕
124. ✅ [06-AUDITORIA-PROTECCION-DATOS.md](25-AUDITORIA-CUMPLIMIENTO/06-AUDITORIA-PROTECCION-DATOS.md) - Checklist Ley 172-13 🆕
125. ✅ [07-AUDITORIA-PROCONSUMIDOR.md](25-AUDITORIA-CUMPLIMIENTO/07-AUDITORIA-PROCONSUMIDOR.md) - Checklist Pro Consumidor 🆕
126. ✅ [08-REPORTES-AUTOMATIZADOS.md](25-AUDITORIA-CUMPLIMIENTO/08-REPORTES-AUTOMATIZADOS.md) - 25 reportes automatizados 🆕
127. ✅ [09-EVIDENCIAS-CONTROLES.md](25-AUDITORIA-CUMPLIMIENTO/09-EVIDENCIAS-CONTROLES.md) - 72 evidencias, 16 controles 🆕
128. ✅ [10-MICROSERVICIOS-AUDITORIA.md](25-AUDITORIA-CUMPLIMIENTO/10-MICROSERVICIOS-AUDITORIA.md) - 4 microservicios nuevos 🆕
129. ✅ [11-DASHBOARD-AUDITORIA-UI.md](25-AUDITORIA-CUMPLIMIENTO/11-DASHBOARD-AUDITORIA-UI.md) - Dashboard UI 🆕

---

## 🆕 DOCUMENTOS FISCALES AGREGADOS (Enero 25, 2026)

### Carpeta: `08-COMPLIANCE-LEGAL-RD/` - Documentos 10-15

| #   | Documento                          | Descripción                         | Prioridad  |
| --- | ---------------------------------- | ----------------------------------- | ---------- |
| 10  | PROCEDIMIENTO-FISCAL-OKLA.md       | Procedimiento fiscal completo       | 🔴 Crítica |
| 11  | REGISTRO-GASTOS-OPERATIVOS.md      | Gastos operativos para Formato 606  | 🔴 Crítica |
| 12  | AUTOMATIZACION-REPORTES-DGII.md    | Automatización con e-CF             | 🔴 Crítica |
| 13  | PREPARACION-AUDITORIA-DGII.md      | Preparación para auditoría DGII     | 🟡 Alta    |
| 14  | E-CF-COMPROBANTES-ELECTRONICOS.md  | e-CF + Envío automático a DGII      | 🔴 Crítica |
| 15  | LIBROS-CONTABLES-AUTOMATIZACION.md | Libros contables + integración e-CF | 🔴 Crítica |

### Características Clave

| Funcionalidad                  | Estado  | Descripción                             |
| ------------------------------ | ------- | --------------------------------------- |
| **e-CF (Factura Electrónica)** | ✅ 100% | E31, E32, E34, E47 implementados        |
| **Envío Automático DGII**      | ✅ 100% | Web Services para 606, 609, IT-1, IR-17 |
| **Libros Contables**           | ✅ 100% | Automatización con integración e-CF     |
| **Reducción Tiempo**           | ✅ 95%  | De 10-15 hrs/mes → 30 min supervisión   |

---

## 📖 Índice de Documentos

Esta matriz de procesos está organizada en **26 carpetas temáticas**, cada una con su documentación detallada de procesos, endpoints, flujos y validaciones.

```
docs/process-matrix/
├── README.md                                    # Este archivo (índice principal)
│
├── 01-AUTENTICACION-SEGURIDAD/
│   ├── 01-auth-service.md                       # Autenticación, JWT, OAuth
│   ├── 02-role-service.md                       # Roles y permisos RBAC
│   ├── 03-security-2fa.md                       # Two-Factor Authentication
│   └── 04-kyc-service.md                        # Know Your Customer
│
├── 02-USUARIOS-DEALERS/
│   ├── 01-user-service.md                       # Gestión de usuarios
│   ├── 02-dealer-management.md                  # Gestión de dealers
│   ├── 03-dealer-analytics.md                   # Analytics para dealers
│   ├── 04-dealer-onboarding.md                  # Onboarding de dealers
│   └── 05-seller-profiles.md                    # Perfiles de vendedores
│
├── 03-VEHICULOS-INVENTARIO/
│   ├── 01-vehicles-sale-service.md              # Venta de vehículos
│   ├── 02-vehicles-rent-service.md              # Alquiler de vehículos
│   ├── 03-inventory-management.md               # Gestión de inventario
│   ├── 04-vehicle-intelligence.md               # IA de precios/demanda
│   ├── 05-catalog-categories.md                 # Catálogo y categorías
│   └── 06-favorites-homepage.md                 # Favoritos y homepage
│
├── 04-BUSQUEDA-RECOMENDACIONES/
│   ├── 01-search-service.md                     # Motor de búsqueda
│   ├── 02-recommendation-service.md             # Sistema de recomendaciones
│   ├── 03-comparison-service.md                 # Comparador de vehículos
│   ├── 04-alert-service.md                      # Alertas de precio/búsqueda
│   └── 05-feature-store.md                      # Feature store para ML
│
├── 05-PAGOS-FACTURACION/
│   ├── 01-billing-service.md                    # Facturación principal
│   ├── 02-stripe-payment.md                     # Pagos con Stripe
│   ├── 03-azul-payment.md                       # Pagos con AZUL (RD)
│   ├── 04-invoicing-service.md                  # Generación de facturas
│   ├── 05-escrow-service.md                     # Escrow/Fideicomiso
│   └── 06-subscriptions.md                      # Suscripciones dealers
│
├── 06-CRM-LEADS-CONTACTOS/
│   ├── 01-crm-service.md                        # CRM principal
│   ├── 02-contact-service.md                    # Gestión de contactos
│   ├── 03-lead-scoring.md                       # Scoring de leads con IA
│   ├── 04-chatbot-service.md                    # Chatbot y WhatsApp
│   └── 05-appointment-service.md                # Citas y test drives
│
├── 07-NOTIFICACIONES-COMUNICACION/
│   ├── 01-notification-service.md               # Sistema de notificaciones
│   ├── 02-templates-scheduling.md               # Templates y programación
│   ├── 03-marketing-service.md                  # Campañas de marketing
│   └── 04-teams-integration.md                  # Integración con Teams
│
├── 08-COMPLIANCE-LEGAL-RD/                     # ⭐ 16 documentos (expandido)
│   ├── 01-compliance-service.md                 # Compliance general
│   ├── 01-ley-155-17.md                         # Ley 155-17 (referencia)
│   ├── 02-ley-172-13.md                         # Protección de datos
│   ├── 03-dgii-integration.md                   # Integración DGII
│   ├── 04-proconsumidor.md                      # Pro Consumidor
│   ├── 05-compliance-reports.md                 # Reportes regulatorios
│   ├── 06-ley-126-02-comercio-electronico.md    # Comercio Electrónico
│   ├── 07-ley-63-17-intrant.md                  # INTRANT
│   ├── 08-obligaciones-fiscales-dgii.md         # Obligaciones fiscales
│   ├── 09-ros-reporte-operaciones-sospechosas.md # ROS (referencia)
│   ├── 10-PROCEDIMIENTO-FISCAL-OKLA.md          # 🆕 Procedimiento fiscal
│   ├── 11-REGISTRO-GASTOS-OPERATIVOS.md         # 🆕 Gastos (606)
│   ├── 12-AUTOMATIZACION-REPORTES-DGII.md       # 🆕 Automatización + e-CF
│   ├── 13-PREPARACION-AUDITORIA-DGII.md         # 🆕 Preparación auditoría
│   ├── 14-E-CF-COMPROBANTES-ELECTRONICOS.md     # 🆕 e-CF + Envío automático
│   └── 15-LIBROS-CONTABLES-AUTOMATIZACION.md    # 🆕 Libros contables
│
├── 09-REPORTES-ANALYTICS/
│   ├── 01-reports-service.md                    # Reportes generales
│   ├── 02-compliance-reporting.md               # Reportes regulatorios
│   ├── 03-reporting-dgii-uaf.md                 # Reportes DGII/UAF
│   ├── 04-dashboards.md                         # Dashboards ejecutivos
│   └── 05-regulatory-alerts.md                  # Alertas regulatorias
│
├── 10-MEDIA-ARCHIVOS/
│   ├── 01-media-service.md                      # Gestión de media
│   ├── 02-file-storage.md                       # Almacenamiento S3
│   ├── 03-spyne-integration.md                  # Integración con Spyne AI
│   └── 04-multimedia-processing.md              # Procesamiento multimedia
│
├── 11-INFRAESTRUCTURA-DEVOPS/
│   ├── 01-gateway-routing.md                    # API Gateway (Ocelot)
│   ├── 02-service-discovery.md                  # Descubrimiento de servicios
│   ├── 03-health-checks.md                      # Health checks
│   ├── 04-cache-service.md                      # Cache (Redis)
│   ├── 05-message-bus.md                        # Message Bus (RabbitMQ)
│   ├── 06-backup-dr.md                          # Backup y DR
│   ├── 07-logging-service.md                    # Logging centralizado
│   ├── 08-tracing-service.md                    # Distributed tracing
│   ├── 09-rate-limiting.md                      # Rate limiting
│   ├── 10-scheduler-service.md                  # Scheduler de jobs
│   ├── 11-configuration-service.md              # Configuración centralizada
│   ├── 12-feature-toggle.md                     # Feature flags
│   └── 13-idempotency.md                        # Idempotencia
│
├── 12-ADMINISTRACION/
│   ├── 01-admin-service.md                      # Panel de administración
│   ├── 02-moderation.md                         # Moderación de contenido
│   ├── 03-maintenance-mode.md                   # Modo mantenimiento
│   ├── 04-audit-service.md                      # Auditoría
│   └── 05-error-service.md                      # Gestión de errores
│
└── 13-INTEGRACIONES-EXTERNAS/
    ├── 01-integration-service.md                # Integraciones generales
    ├── 02-compliance-integration.md             # Integraciones compliance
    ├── 03-data-pipeline.md                      # Pipelines de datos
    ├── 04-event-tracking.md                     # Tracking de eventos
    └── 05-user-behavior.md                      # Comportamiento de usuario
```

---

## 🎯 Estructura de Cada Documento de Proceso

Cada documento sigue la estructura estándar:

```markdown
# [Nombre del Servicio] - Matriz de Procesos

## 1. Información General

- Descripción del servicio
- Puerto asignado
- Dependencias
- Base de datos

## 2. Endpoints API

- Tabla de todos los endpoints
- Métodos HTTP
- Autenticación requerida

## 3. Procesos Detallados

Para cada proceso:

- ID del proceso
- Nombre
- Actor(es)
- Precondiciones
- Flujo paso a paso
- Postcondiciones
- Validaciones
- Errores posibles
- Endpoints involucrados

## 4. Flujos de Integración

- Diagramas de secuencia
- Comunicación entre servicios

## 5. Reglas de Negocio

- Validaciones específicas
- Límites y restricciones
- Fórmulas de cálculo

## 6. Manejo de Errores

- Códigos de error
- Mensajes
- Acciones de recuperación
```

---

## 📊 Resumen por Categoría

| #         | Categoría                     | Docs    | Controllers | Procesos Est. |
| --------- | ----------------------------- | ------- | ----------- | ------------- |
| 01        | Autenticación y Seguridad     | 6       | 12          | 50+           |
| 02        | Usuarios y Dealers            | 6       | 18          | 60+           |
| 03        | Vehículos e Inventario        | 6       | 15          | 65+           |
| 04        | Búsqueda y Recomendaciones    | 8       | 12          | 45+           |
| 05        | Pagos y Facturación           | 7       | 18          | 75+           |
| 06        | CRM, Leads y Contactos        | 6       | 12          | 50+           |
| 07        | Notificaciones                | 6       | 10          | 35+           |
| 08        | **Compliance Legal RD** ⭐    | **10**  | 15          | **90+**       |
| 09        | Reportes y Analytics          | 6       | 12          | 45+           |
| 10        | Media y Archivos              | 4       | 12          | 35+           |
| 11        | Infraestructura               | 14      | 25          | 55+           |
| 12        | Administración                | 7       | 10          | 35+           |
| 13        | Integraciones                 | 5       | 8           | 30+           |
| 14        | Financiamiento/Trade-In       | 4       | 6           | 25+           |
| 15        | Confianza y Seguridad         | 6       | 8           | 40+           |
| 16        | Promoción/Visibilidad         | 1       | 3           | 15+           |
| 17        | Engagement/Retención          | 4       | 6           | 25+           |
| 18        | Seguros                       | 1       | 2           | 10+           |
| 19        | Soporte                       | 2       | 4           | 20+           |
| 20        | Pricing Intelligence          | 2       | 4           | 15+           |
| 21        | Reviews Dealer                | 1       | 3           | 12+           |
| 22        | Comunicación Realtime         | 1       | 3           | 10+           |
| **25**    | **Auditoría Cumplimiento** 🆕 | **12**  | **20**      | **100+**      |
| **TOTAL** | **26 carpetas**               | **118** | **238**     | **1,000+**    |

---

## 🔗 Referencias Cruzadas

### Flujos Principales del Sistema

| Flujo                 | Documento Principal              | Servicios Involucrados             |
| --------------------- | -------------------------------- | ---------------------------------- |
| Registro de Usuario   | 01-auth-service.md               | Auth, User, KYC, Notification      |
| Publicar Vehículo     | 03-vehicles-sale.md              | Vehicle, Media, Billing, Search    |
| Compra de Vehículo    | 05-billing-service.md            | Billing, Escrow, Contract, Vehicle |
| Onboarding Dealer     | 02-dealer-management.md          | Dealer, KYC, Billing, Compliance   |
| Proceso de Lead       | 06-crm-service.md                | CRM, Lead, Contact, Notification   |
| **Reporte DGII** 🆕   | 08-obligaciones-fiscales-dgii.md | Fiscal, Billing, Compliance        |
| **Auditoría UAF** 🆕  | 05-AUDITORIA-UAF.md              | Compliance, KYC, Alert, ROS        |
| **Solicitud ARCO** 🆕 | 06-derechos-arco.md              | DataProtection, User, Notification |

### Leyes RD Mapeadas

| Ley    | Nombre               | Documento                                           | Cumplimiento |
| ------ | -------------------- | --------------------------------------------------- | ------------ |
| 155-17 | Anti-Lavado (LA/FT)  | 01-ley-155-17.md + 05-AUDITORIA-UAF.md              | 🔴 10%       |
| 172-13 | Protección de Datos  | 02-ley-172-13.md + 06-AUDITORIA-PROTECCION-DATOS.md | 🟡 40%       |
| 11-92  | Código Tributario    | 03-dgii-integration.md + 04-AUDITORIA-DGII.md       | 🔴 30%       |
| 358-05 | Pro Consumidor       | 04-proconsumidor.md + 07-AUDITORIA-PROCONSUMIDOR.md | 🟡 35%       |
| 126-02 | Comercio Electrónico | 06-ley-126-02-comercio-electronico.md               | 🟡 60%       |
| 63-17  | INTRANT              | 07-ley-63-17-intrant.md                             | 🟡 50%       |

---

## 📝 Convenciones de Documentación

### IDs de Procesos

```
[SERVICIO]-[MÓDULO]-[NÚMERO]

Ejemplos:
- AUTH-LOGIN-001: Proceso de login
- VEH-PUB-001: Publicar vehículo
- PAY-STRIPE-001: Pago con Stripe
- COMP-AML-001: Verificación AML
```

### Estados de Proceso

| Estado         | Descripción           |
| -------------- | --------------------- |
| 🟢 ACTIVO      | Proceso en producción |
| 🟡 DESARROLLO  | En desarrollo         |
| 🔴 DEPRECADO   | Será eliminado        |
| 🔵 PLANIFICADO | Futuro                |

### Niveles de Criticidad

| Nivel      | Descripción              | SLA   |
| ---------- | ------------------------ | ----- |
| 🔴 CRÍTICO | Afecta pagos/compliance  | < 1h  |
| 🟠 ALTO    | Afecta operación         | < 4h  |
| 🟡 MEDIO   | Funcionalidad importante | < 24h |
| 🟢 BAJO    | Mejora de UX             | < 72h |

---

## 🚀 Cómo Usar Esta Documentación

### Para Desarrolladores

1. Identificar el servicio relevante en el índice
2. Leer el documento de proceso correspondiente
3. Seguir el flujo paso a paso
4. Implementar validaciones documentadas
5. Manejar errores según la tabla

### Para QA

1. Usar los flujos como casos de prueba
2. Verificar precondiciones y postcondiciones
3. Probar todos los errores documentados
4. Validar integraciones entre servicios

### Para Product

1. Revisar reglas de negocio
2. Validar flujos de usuario
3. Confirmar requisitos legales (RD)
4. Aprobar cambios en procesos

---

## 📅 Historial de Cambios

| Fecha      | Versión | Cambios                                                 |
| ---------- | ------- | ------------------------------------------------------- |
| 2026-01-25 | 3.0.0   | ✅ **Nuevo módulo 25-AUDITORIA-CUMPLIMIENTO (12 docs)** |
| 2026-01-25 | 3.0.0   | ✅ Compliance-Legal-RD expandido (6→10 docs)            |
| 2026-01-25 | 3.0.0   | ✅ 9 documentos regulatorios nuevos (ARCO, NCF, etc.)   |
| 2026-01-25 | 3.0.0   | 📊 Total: 78→118 documentos (+40)                       |
| 2026-01-24 | 2.5.0   | Sincronización de secciones de auditoría (104 archivos) |
| 2026-01-21 | 2.0.0   | Reorganización en 22 categorías                         |
| 2026-01-21 | 1.0.0   | Creación inicial con 13 categorías                      |

---

**Mantenido por:** Equipo de Desarrollo OKLA  
**Contacto:** compliance@okla.com.do  
**Repositorio:** gregorymorenoiem/cardealer-microservices  
**Branch:** development
