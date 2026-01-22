# 📋 OKLA - Matriz de Procesos por Microservicio

> **Última actualización:** Enero 21, 2026  
> **Total de Microservicios:** 65+  
> **Total de Controllers:** 195  
> **Total de Procesos Documentados:** 500+

---

## 📊 Progreso de Documentación

| Categoría                   | Documentos | Completados | Estado      |
| --------------------------- | ---------- | ----------- | ----------- |
| 01-AUTENTICACION-SEGURIDAD  | 4          | 4           | 🟢 100%     |
| 02-USUARIOS-DEALERS         | 5          | 5           | 🟢 100%     |
| 03-VEHICULOS-INVENTARIO     | 5          | 5           | 🟢 100%     |
| 04-BUSQUEDA-RECOMENDACIONES | 5          | 5           | 🟢 100%     |
| 04-PROPIEDADES-INMUEBLES    | 2          | 2           | 🟢 100%     |
| 05-PAGOS-FACTURACION        | 6          | 6           | 🟢 100%     |
| 06-CRM-LEADS-CONTACTOS      | 5          | 5           | 🟢 100%     |
| 07-NOTIFICACIONES           | 4          | 4           | 🟢 100%     |
| 07-REVIEWS-REPUTACION       | 1          | 1           | 🟢 100%     |
| 08-COMPLIANCE-LEGAL-RD      | 6          | 6           | 🟢 100%     |
| 09-REPORTES-ANALYTICS       | 5          | 5           | 🟢 100%     |
| 10-MEDIA-ARCHIVOS           | 4          | 4           | 🟢 100%     |
| 11-INFRAESTRUCTURA-DEVOPS   | 14         | 14          | 🟢 100%     |
| 12-ADMINISTRACION           | 7          | 7           | 🟢 100%     |
| 13-INTEGRACIONES-EXTERNAS   | 5          | 5           | 🟢 100%     |
| **TOTAL**                   | **78**     | **78**      | **🟢 100%** |

### ✅ Documentos Completados (78/78 - 100%)

#### 01-AUTENTICACION-SEGURIDAD (4/4)

1. ✅ [01-auth-service.md](01-AUTENTICACION-SEGURIDAD/01-auth-service.md) - Autenticación, JWT, OAuth
2. ✅ [02-role-service.md](01-AUTENTICACION-SEGURIDAD/02-role-service.md) - Roles y permisos RBAC
3. ✅ [03-security-2fa.md](01-AUTENTICACION-SEGURIDAD/03-security-2fa.md) - Two-Factor Authentication
4. ✅ [04-kyc-service.md](01-AUTENTICACION-SEGURIDAD/04-kyc-service.md) - Know Your Customer

#### 02-USUARIOS-DEALERS (5/5)

5. ✅ [01-user-service.md](02-USUARIOS-DEALERS/01-user-service.md) - Gestión de usuarios
6. ✅ [02-dealer-management.md](02-USUARIOS-DEALERS/02-dealer-management.md) - Gestión de dealers
7. ✅ [03-dealer-analytics.md](02-USUARIOS-DEALERS/03-dealer-analytics.md) - Analytics para dealers
8. ✅ [04-dealer-onboarding.md](02-USUARIOS-DEALERS/04-dealer-onboarding.md) - Onboarding de dealers
9. ✅ [05-seller-profiles.md](02-USUARIOS-DEALERS/05-seller-profiles.md) - Perfiles de vendedores

#### 03-VEHICULOS-INVENTARIO (5/5)

10. ✅ [01-vehicles-sale-service.md](03-VEHICULOS-INVENTARIO/01-vehicles-sale-service.md) - Venta de vehículos
11. ✅ [02-vehicles-rent-service.md](03-VEHICULOS-INVENTARIO/02-vehicles-rent-service.md) - Alquiler de vehículos
12. ✅ [03-inventory-management.md](03-VEHICULOS-INVENTARIO/03-inventory-management.md) - Gestión de inventario
13. ✅ [04-vehicle-intelligence.md](03-VEHICULOS-INVENTARIO/04-vehicle-intelligence.md) - IA de precios/demanda
14. ✅ [05-catalog-favorites-homepage.md](03-VEHICULOS-INVENTARIO/05-catalog-favorites-homepage.md) - Catálogo, favoritos, homepage

#### 04-BUSQUEDA-RECOMENDACIONES (5/5)

15. ✅ [01-search-service.md](04-BUSQUEDA-RECOMENDACIONES/01-search-service.md) - Motor de búsqueda Elasticsearch
16. ✅ [02-recommendation-service.md](04-BUSQUEDA-RECOMENDACIONES/02-recommendation-service.md) - Sistema de recomendaciones
17. ✅ [03-comparison-service.md](04-BUSQUEDA-RECOMENDACIONES/03-comparison-service.md) - Comparador de vehículos
18. ✅ [04-alert-service.md](04-BUSQUEDA-RECOMENDACIONES/04-alert-service.md) - Alertas de precio/búsqueda
19. ✅ [05-feature-store.md](04-BUSQUEDA-RECOMENDACIONES/05-feature-store.md) - Feature store para ML

#### 04-PROPIEDADES-INMUEBLES (2/2)

20. ✅ [01-properties-sale-service.md](04-PROPIEDADES-INMUEBLES/01-properties-sale-service.md) - Propiedades en venta
21. ✅ [02-properties-rent-service.md](04-PROPIEDADES-INMUEBLES/02-properties-rent-service.md) - Propiedades en renta

#### 05-PAGOS-FACTURACION (6/6)

22. ✅ [01-billing-service.md](05-PAGOS-FACTURACION/01-billing-service.md) - Facturación principal
23. ✅ [02-stripe-payment.md](05-PAGOS-FACTURACION/02-stripe-payment.md) - Pagos con Stripe
24. ✅ [03-azul-payment.md](05-PAGOS-FACTURACION/03-azul-payment.md) - Pagos con AZUL (RD)
25. ✅ [04-invoicing-service.md](05-PAGOS-FACTURACION/04-invoicing-service.md) - Generación de facturas
26. ✅ [05-escrow-service.md](05-PAGOS-FACTURACION/05-escrow-service.md) - Escrow/Custodia de pagos
27. ✅ [06-subscriptions.md](05-PAGOS-FACTURACION/06-subscriptions.md) - Suscripciones de dealers

#### 06-CRM-LEADS-CONTACTOS (5/5)

28. ✅ [01-crm-service.md](06-CRM-LEADS-CONTACTOS/01-crm-service.md) - CRM (Leads, Deals, Pipelines)
29. ✅ [02-contact-service.md](06-CRM-LEADS-CONTACTOS/02-contact-service.md) - Sistema de contactos/mensajería
30. ✅ [03-lead-scoring.md](06-CRM-LEADS-CONTACTOS/03-lead-scoring.md) - Lead scoring IA
31. ✅ [04-chatbot-service.md](06-CRM-LEADS-CONTACTOS/04-chatbot-service.md) - Chatbot IA + WhatsApp
32. ✅ [05-appointment-service.md](06-CRM-LEADS-CONTACTOS/05-appointment-service.md) - Test drives y citas

#### 07-NOTIFICACIONES (4/4)

33. ✅ [01-notification-service.md](07-NOTIFICACIONES/01-notification-service.md) - Sistema de notificaciones
34. ✅ [02-templates-scheduling.md](07-NOTIFICACIONES/02-templates-scheduling.md) - Templates y programación
35. ✅ [03-marketing-service.md](07-NOTIFICACIONES/03-marketing-service.md) - Marketing automation
36. ✅ [04-teams-integration.md](07-NOTIFICACIONES/04-teams-integration.md) - Integración MS Teams

#### 07-REVIEWS-REPUTACION (1/1)

37. ✅ [01-review-service.md](07-REVIEWS-REPUTACION/01-review-service.md) - Reviews y reputación

#### 08-COMPLIANCE-LEGAL-RD (6/6)

38. ✅ [01-compliance-service.md](08-COMPLIANCE-LEGAL-RD/01-compliance-service.md) - Compliance RD general
39. ✅ [01-ley-155-17.md](08-COMPLIANCE-LEGAL-RD/01-ley-155-17.md) - Ley 155-17 Anti-Lavado
40. ✅ [02-ley-172-13.md](08-COMPLIANCE-LEGAL-RD/02-ley-172-13.md) - Ley 172-13 Protección Datos
41. ✅ [03-dgii-integration.md](08-COMPLIANCE-LEGAL-RD/03-dgii-integration.md) - Integración DGII
42. ✅ [04-proconsumidor.md](08-COMPLIANCE-LEGAL-RD/04-proconsumidor.md) - Pro Consumidor
43. ✅ [05-compliance-reports.md](08-COMPLIANCE-LEGAL-RD/05-compliance-reports.md) - Reportes regulatorios

#### 09-REPORTES-ANALYTICS (5/5)

44. ✅ [01-reports-service.md](09-REPORTES-ANALYTICS/01-reports-service.md) - Reportes generales
45. ✅ [02-analytics-service.md](09-REPORTES-ANALYTICS/02-analytics-service.md) - Analytics y métricas
46. ✅ [03-event-tracking.md](09-REPORTES-ANALYTICS/03-event-tracking.md) - Event tracking
47. ✅ [04-dashboards.md](09-REPORTES-ANALYTICS/04-dashboards.md) - Dashboards ejecutivos
48. ✅ [05-regulatory-alerts.md](09-REPORTES-ANALYTICS/05-regulatory-alerts.md) - Alertas regulatorias

#### 10-MEDIA-ARCHIVOS (4/4)

49. ✅ [01-media-service.md](10-MEDIA-ARCHIVOS/01-media-service.md) - Gestión de media
50. ✅ [02-image-processing.md](10-MEDIA-ARCHIVOS/02-image-processing.md) - Procesamiento de imágenes
51. ✅ [03-document-storage.md](10-MEDIA-ARCHIVOS/03-document-storage.md) - Almacenamiento de documentos
52. ✅ [04-multimedia-processing.md](10-MEDIA-ARCHIVOS/04-multimedia-processing.md) - Video y multimedia

#### 11-INFRAESTRUCTURA-DEVOPS (14/14)

53. ✅ [01-gateway-service.md](11-INFRAESTRUCTURA-DEVOPS/01-gateway-service.md) - API Gateway (Ocelot)
54. ✅ [02-error-service.md](11-INFRAESTRUCTURA-DEVOPS/02-error-service.md) - Gestión de errores
55. ✅ [02-service-discovery.md](11-INFRAESTRUCTURA-DEVOPS/02-service-discovery.md) - Consul service discovery
56. ✅ [04-health-checks.md](11-INFRAESTRUCTURA-DEVOPS/04-health-checks.md) - Health checks
57. ✅ [05-logging-service.md](11-INFRAESTRUCTURA-DEVOPS/05-logging-service.md) - Logging centralizado
58. ✅ [06-rate-limiting.md](11-INFRAESTRUCTURA-DEVOPS/06-rate-limiting.md) - Rate limiting
59. ✅ [07-caching-service.md](11-INFRAESTRUCTURA-DEVOPS/07-caching-service.md) - Redis caching
60. ✅ [08-queue-management.md](11-INFRAESTRUCTURA-DEVOPS/08-queue-management.md) - RabbitMQ
61. ✅ [09-deployment.md](11-INFRAESTRUCTURA-DEVOPS/09-deployment.md) - CI/CD y deployment
62. ✅ [10-monitoring.md](11-INFRAESTRUCTURA-DEVOPS/10-monitoring.md) - Monitoreo Prometheus/Grafana
63. ✅ [10-scheduler-service.md](11-INFRAESTRUCTURA-DEVOPS/10-scheduler-service.md) - Scheduler (Quartz.NET)
64. ✅ [11-configuration-service.md](11-INFRAESTRUCTURA-DEVOPS/11-configuration-service.md) - Configuración centralizada
65. ✅ [12-feature-toggle.md](11-INFRAESTRUCTURA-DEVOPS/12-feature-toggle.md) - Feature flags
66. ✅ [13-idempotency.md](11-INFRAESTRUCTURA-DEVOPS/13-idempotency.md) - Sistema de idempotencia

#### 12-ADMINISTRACION (7/7)

67. ✅ [01-admin-service.md](12-ADMINISTRACION/01-admin-service.md) - Panel de administración
68. ✅ [02-admin-users.md](12-ADMINISTRACION/02-admin-users.md) - Gestión de usuarios admin
69. ✅ [03-maintenance-mode.md](12-ADMINISTRACION/03-maintenance-mode.md) - Modo mantenimiento
70. ✅ [03-system-config.md](12-ADMINISTRACION/03-system-config.md) - Configuración del sistema
71. ✅ [04-audit-service.md](12-ADMINISTRACION/04-audit-service.md) - Auditoría del sistema
72. ✅ [04-feature-flags.md](12-ADMINISTRACION/04-feature-flags.md) - Feature flags admin
73. ✅ [05-error-service.md](12-ADMINISTRACION/05-error-service.md) - Dashboard de errores

#### 13-INTEGRACIONES-EXTERNAS (5/5)

74. ✅ [01-whatsapp-integration.md](13-INTEGRACIONES-EXTERNAS/01-whatsapp-integration.md) - WhatsApp Business API
75. ✅ [02-sms-integration.md](13-INTEGRACIONES-EXTERNAS/02-sms-integration.md) - SMS (Twilio)
76. ✅ [03-email-providers.md](13-INTEGRACIONES-EXTERNAS/03-email-providers.md) - Email (SendGrid/SES)
77. ✅ [04-maps-integration.md](13-INTEGRACIONES-EXTERNAS/04-maps-integration.md) - Google Maps
78. ✅ [05-social-auth.md](13-INTEGRACIONES-EXTERNAS/05-social-auth.md) - OAuth social (Google/Apple)

---

## �📖 Índice de Documentos

Esta matriz de procesos está organizada en **12 categorías principales**, cada una con su documentación detallada de procesos, endpoints, flujos y validaciones.

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
├── 08-COMPLIANCE-LEGAL-RD/
│   ├── 01-compliance-service.md                 # Compliance general
│   ├── 02-aml-service.md                        # Anti-Lavado (Ley 155-17)
│   ├── 03-tax-compliance.md                     # Cumplimiento DGII (Ley 11-92)
│   ├── 04-consumer-protection.md                # Pro Consumidor (Ley 358-05)
│   ├── 05-digital-signature.md                  # Firma Digital (Ley 339-22)
│   ├── 06-vehicle-registry.md                   # Registro INTRANT (Ley 63-17)
│   ├── 07-data-protection.md                    # Protección de datos
│   ├── 08-contract-service.md                   # Contratos legales
│   ├── 09-dispute-service.md                    # Resolución de disputas
│   └── 10-legal-documents.md                    # Documentos legales
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

| #         | Categoría                  | Servicios | Controllers | Procesos Est. |
| --------- | -------------------------- | --------- | ----------- | ------------- |
| 01        | Autenticación y Seguridad  | 4         | 12          | 45+           |
| 02        | Usuarios y Dealers         | 5         | 18          | 55+           |
| 03        | Vehículos e Inventario     | 6         | 15          | 60+           |
| 04        | Búsqueda y Recomendaciones | 5         | 10          | 35+           |
| 05        | Pagos y Facturación        | 6         | 18          | 70+           |
| 06        | CRM, Leads y Contactos     | 5         | 12          | 45+           |
| 07        | Notificaciones             | 4         | 10          | 30+           |
| 08        | Compliance Legal RD        | 10        | 15          | 80+           |
| 09        | Reportes y Analytics       | 5         | 12          | 40+           |
| 10        | Media y Archivos           | 4         | 12          | 35+           |
| 11        | Infraestructura            | 13        | 25          | 50+           |
| 12        | Administración             | 5         | 10          | 30+           |
| 13        | Integraciones              | 5         | 8           | 25+           |
| **TOTAL** | **77**                     | **177**   | **600+**    |

---

## 🔗 Referencias Cruzadas

### Flujos Principales del Sistema

| Flujo               | Documento Principal        | Servicios Involucrados             |
| ------------------- | -------------------------- | ---------------------------------- |
| Registro de Usuario | 01-auth-service.md         | Auth, User, KYC, Notification      |
| Publicar Vehículo   | 03-vehicles-sale.md        | Vehicle, Media, Billing, Search    |
| Compra de Vehículo  | 05-billing-service.md      | Billing, Escrow, Contract, Vehicle |
| Onboarding Dealer   | 02-dealer-management.md    | Dealer, KYC, Billing, Compliance   |
| Proceso de Lead     | 06-crm-service.md          | CRM, Lead, Contact, Notification   |
| Reporte DGII        | 09-compliance-reporting.md | Reporting, Tax, Compliance         |

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

| Fecha      | Versión | Cambios                             |
| ---------- | ------- | ----------------------------------- |
| 2026-01-21 | 1.0.0   | Creación inicial con 13 categorías  |
| -          | -       | Documentación de 65+ microservicios |
| -          | -       | 195 controllers mapeados            |
| -          | -       | 600+ procesos estimados             |

---

**Mantenido por:** Equipo de Desarrollo OKLA  
**Contacto:** dev@okla.com.do
