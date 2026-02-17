# 📚 OKLA - Índice Maestro de APIs

**Fecha de actualización:** Enero 15, 2026  
**Plataforma:** OKLA Marketplace de Vehículos  
**Estado:** En Producción

---

## 🎯 Navegación Rápida

| Categoría                       | APIs    | Estado        | Documentación                               |
| ------------------------------- | ------- | ------------- | ------------------------------------------- |
| **APIs Externas**               | 5 APIs  | ✅ Activas    | [Ver sección](#apis-externas)               |
| **Microservicios Core**         | 6 APIs  | ✅ Producción | [Ver sección](#microservicios-core)         |
| **Microservicios de Negocio**   | 8 APIs  | ✅ Producción | [Ver sección](#microservicios-de-negocio)   |
| **Microservicios Planificados** | 15 APIs | 🚧 Desarrollo | [Ver sección](#microservicios-planificados) |
| **Infraestructura**             | 6 APIs  | ✅ Producción | [Ver sección](#infraestructura)             |

---

## 📊 Estadísticas Generales

| Métrica               | Valor |
| --------------------- | ----- |
| **Total de APIs**     | 40    |
| **En Producción**     | 20    |
| **En Desarrollo**     | 15    |
| **Planificadas**      | 5     |
| **APIs Externas**     | 5     |
| **Endpoints Totales** | ~300+ |

---

## 🌐 APIs Externas

APIs de terceros integradas en la plataforma.

### 1. AZUL Payment Gateway ✅

- **Proveedor:** Banco Popular (República Dominicana)
- **Propósito:** Pagos con tarjetas locales
- **Estado:** ✅ En Producción
- **Documentación:** [AZUL_API_DOCUMENTATION.md](payments/AZUL_API_DOCUMENTATION.md)
- **Endpoints:** 8
- **Autenticación:** API Key + SHA-256 Hash
- **Roadmap:** [AZUL_ROADMAP.md](payments/AZUL_ROADMAP.md)

### 2. Stripe Payment Platform ✅

- **Proveedor:** Stripe Inc.
- **Propósito:** Pagos internacionales + Subscripciones
- **Estado:** ✅ En Producción
- **Documentación:** [STRIPE_API_DOCUMENTATION.md](payments/STRIPE_API_DOCUMENTATION.md)
- **Endpoints:** 12
- **Autenticación:** Bearer Token
- **Roadmap:** [STRIPE_ROADMAP.md](payments/STRIPE_ROADMAP.md)

### 3. Amazon S3 (DigitalOcean Spaces) ✅

- **Proveedor:** DigitalOcean
- **Propósito:** Almacenamiento de archivos/imágenes
- **Estado:** ✅ En Producción
- **Documentación:** [S3_API_DOCUMENTATION.md](storage/S3_API_DOCUMENTATION.md)
- **Endpoints:** 6
- **Autenticación:** AWS Signature V4
- **Roadmap:** [S3_ROADMAP.md](storage/S3_ROADMAP.md)

### 4. Zoho Mail API 🚧

- **Proveedor:** Zoho Corporation
- **Propósito:** Envío de emails transaccionales
- **Estado:** 🚧 En Configuración
- **Documentación:** [ZOHO_API_DOCUMENTATION.md](email/ZOHO_API_DOCUMENTATION.md)
- **Endpoints:** 5
- **Autenticación:** OAuth 2.0
- **Roadmap:** [ZOHO_ROADMAP.md](email/ZOHO_ROADMAP.md)

### 5. Firebase Cloud Messaging (FCM) 📱

- **Proveedor:** Google Firebase
- **Propósito:** Push notifications mobile
- **Estado:** 🚧 Planificado
- **Documentación:** [FCM_API_DOCUMENTATION.md](notifications/FCM_API_DOCUMENTATION.md)
- **Endpoints:** 4
- **Autenticación:** Service Account JSON
- **Roadmap:** [FCM_ROADMAP.md](notifications/FCM_ROADMAP.md)

---

## 🎯 Microservicios Core

Servicios fundamentales de la plataforma (en producción DOKS).

### 1. Gateway (Ocelot) ✅

- **Puerto:** 8080
- **Propósito:** API Gateway unificado
- **Estado:** ✅ Producción
- **URL:** https://api.okla.com.do
- **Documentación:** [GATEWAY_API_DOCUMENTATION.md](core/GATEWAY_API_DOCUMENTATION.md)
- **Endpoints:** Enrutamiento a todos los servicios
- **Autenticación:** JWT Bearer Token passthrough
- **Roadmap:** [GATEWAY_ROADMAP.md](core/GATEWAY_ROADMAP.md)

### 2. AuthService ✅

- **Puerto:** 8080
- **Propósito:** Autenticación y autorización
- **Estado:** ✅ Producción
- **Documentación:** [AUTH_API_DOCUMENTATION.md](core/AUTH_API_DOCUMENTATION.md)
- **Endpoints:** 8
- **Autenticación:** JWT + Refresh Tokens
- **Roadmap:** [AUTH_ROADMAP.md](core/AUTH_ROADMAP.md)

### 3. UserService ✅

- **Puerto:** 8080
- **Propósito:** Gestión de usuarios y perfiles
- **Estado:** ✅ Producción
- **Documentación:** [USER_API_DOCUMENTATION.md](core/USER_API_DOCUMENTATION.md)
- **Endpoints:** 12
- **Autenticación:** JWT Required
- **Roadmap:** [USER_ROADMAP.md](core/USER_ROADMAP.md)

### 4. RoleService ✅

- **Puerto:** 8080
- **Propósito:** Roles y permisos (RBAC)
- **Estado:** ✅ Producción
- **Documentación:** [ROLE_API_DOCUMENTATION.md](core/ROLE_API_DOCUMENTATION.md)
- **Endpoints:** 10
- **Autenticación:** JWT + Admin role
- **Roadmap:** [ROLE_ROADMAP.md](core/ROLE_ROADMAP.md)

### 5. ErrorService ✅

- **Puerto:** 8080
- **Propósito:** Centralización de errores
- **Estado:** ✅ Producción
- **Documentación:** [ERROR_API_DOCUMENTATION.md](core/ERROR_API_DOCUMENTATION.md)
- **Endpoints:** 7
- **Autenticación:** API Key (interno)
- **Roadmap:** [ERROR_ROADMAP.md](core/ERROR_ROADMAP.md)

### 6. NotificationService ✅

- **Puerto:** 8080
- **Propósito:** Email, SMS, Push notifications
- **Estado:** ✅ Producción
- **Documentación:** [NOTIFICATION_API_DOCUMENTATION.md](core/NOTIFICATION_API_DOCUMENTATION.md)
- **Endpoints:** 15
- **Autenticación:** JWT + API Key
- **Roadmap:** [NOTIFICATION_ROADMAP.md](core/NOTIFICATION_ROADMAP.md)

---

## 💼 Microservicios de Negocio

Servicios que implementan lógica de negocio específica de OKLA.

### 1. VehiclesSaleService ✅

- **Puerto:** 8080
- **Propósito:** CRUD de vehículos + catálogo
- **Estado:** ✅ Producción
- **Documentación:** [VEHICLES_API_DOCUMENTATION.md](business/VEHICLES_API_DOCUMENTATION.md)
- **Endpoints:** 25+
- **Autenticación:** JWT (algunas rutas públicas)
- **Roadmap:** [VEHICLES_ROADMAP.md](business/VEHICLES_ROADMAP.md)

### 2. MediaService ✅

- **Puerto:** 8080
- **Propósito:** Gestión de archivos (S3)
- **Estado:** ✅ Producción
- **Documentación:** [MEDIA_API_DOCUMENTATION.md](business/MEDIA_API_DOCUMENTATION.md)
- **Endpoints:** 10
- **Autenticación:** JWT Required
- **Roadmap:** [MEDIA_ROADMAP.md](business/MEDIA_ROADMAP.md)

### 3. BillingService ✅

- **Puerto:** 8080
- **Propósito:** Pagos (Stripe + AZUL)
- **Estado:** ✅ Producción
- **Documentación:** [BILLING_API_DOCUMENTATION.md](business/BILLING_API_DOCUMENTATION.md)
- **Endpoints:** 20
- **Autenticación:** JWT + Webhook signatures
- **Roadmap:** [BILLING_ROADMAP.md](business/BILLING_ROADMAP.md)

### 4. DealerManagementService ✅

- **Puerto:** 5039
- **Propósito:** Gestión de dealers
- **Estado:** ✅ Producción
- **Documentación:** [DEALER_API_DOCUMENTATION.md](business/DEALER_API_DOCUMENTATION.md)
- **Endpoints:** 8
- **Autenticación:** JWT Required
- **Roadmap:** [DEALER_ROADMAP.md](business/DEALER_ROADMAP.md)

### 5. MaintenanceService ✅

- **Puerto:** 5061
- **Propósito:** Modo mantenimiento programable
- **Estado:** ✅ Producción
- **Documentación:** [MAINTENANCE_API_DOCUMENTATION.md](business/MAINTENANCE_API_DOCUMENTATION.md)
- **Endpoints:** 7
- **Autenticación:** JWT Admin
- **Roadmap:** [MAINTENANCE_ROADMAP.md](business/MAINTENANCE_ROADMAP.md)

### 6. ComparisonService ✅

- **Puerto:** 5066
- **Propósito:** Comparador de vehículos
- **Estado:** ✅ Producción
- **Documentación:** [COMPARISON_API_DOCUMENTATION.md](business/COMPARISON_API_DOCUMENTATION.md)
- **Endpoints:** 8
- **Autenticación:** JWT Required
- **Roadmap:** [COMPARISON_ROADMAP.md](business/COMPARISON_ROADMAP.md)

### 7. AlertService ✅

- **Puerto:** 5067
- **Propósito:** Alertas de precio/búsquedas
- **Estado:** ✅ Producción
- **Documentación:** [ALERT_API_DOCUMENTATION.md](business/ALERT_API_DOCUMENTATION.md)
- **Endpoints:** 10
- **Autenticación:** JWT Required
- **Roadmap:** [ALERT_ROADMAP.md](business/ALERT_ROADMAP.md)

### 8. ContactService ✅

- **Puerto:** 8080
- **Propósito:** Mensajería entre usuarios
- **Estado:** ✅ Producción
- **Documentación:** [CONTACT_API_DOCUMENTATION.md](business/CONTACT_API_DOCUMENTATION.md)
- **Endpoints:** 12
- **Autenticación:** JWT Required
- **Roadmap:** [CONTACT_ROADMAP.md](business/CONTACT_ROADMAP.md)

---

## 🚧 Microservicios Planificados

Servicios en desarrollo o planificados para futuros sprints.

### Data & ML Services

#### 1. EventTrackingService 📊

- **Puerto:** 5050
- **Propósito:** Captura de eventos de usuario
- **Estado:** 🚧 Sprint 18 (Q1 2026)
- **Documentación:** [EVENT_TRACKING_API_DOCUMENTATION.md](ml/EVENT_TRACKING_API_DOCUMENTATION.md)
- **Endpoints:** 6
- **Roadmap:** [EVENT_TRACKING_ROADMAP.md](ml/EVENT_TRACKING_ROADMAP.md)

#### 2. RecommendationService 🤖

- **Puerto:** 5054
- **Propósito:** Recomendaciones con ML
- **Estado:** 🚧 Sprint 19 (Q2 2026)
- **Documentación:** [RECOMMENDATION_API_DOCUMENTATION.md](ml/RECOMMENDATION_API_DOCUMENTATION.md)
- **Endpoints:** 8
- **Roadmap:** [RECOMMENDATION_ROADMAP.md](ml/RECOMMENDATION_ROADMAP.md)

#### 3. LeadScoringService 🎯

- **Puerto:** 5055
- **Propósito:** Scoring de leads con IA
- **Estado:** 🚧 Sprint 20 (Q2 2026)
- **Documentación:** [LEAD_SCORING_API_DOCUMENTATION.md](ml/LEAD_SCORING_API_DOCUMENTATION.md)
- **Endpoints:** 7
- **Roadmap:** [LEAD_SCORING_ROADMAP.md](ml/LEAD_SCORING_ROADMAP.md)

#### 4. VehicleIntelligenceService 💡

- **Puerto:** 5056
- **Propósito:** Pricing inteligente con IA
- **Estado:** 📝 Planificado Q2 2026
- **Documentación:** [VEHICLE_INTELLIGENCE_API_DOCUMENTATION.md](ml/VEHICLE_INTELLIGENCE_API_DOCUMENTATION.md)
- **Endpoints:** 10
- **Roadmap:** [VEHICLE_INTELLIGENCE_ROADMAP.md](ml/VEHICLE_INTELLIGENCE_ROADMAP.md)

### Dealer Services

#### 5. InventoryManagementService 📦

- **Puerto:** 5040
- **Propósito:** Import/export masivo
- **Estado:** ✅ Producción
- **Documentación:** [INVENTORY_API_DOCUMENTATION.md](business/INVENTORY_API_DOCUMENTATION.md)
- **Endpoints:** 15
- **Roadmap:** [INVENTORY_ROADMAP.md](business/INVENTORY_ROADMAP.md)

#### 6. DealerAnalyticsService 📈

- **Puerto:** 5041
- **Propósito:** Dashboard y métricas dealers
- **Estado:** ✅ Producción
- **Documentación:** [DEALER_ANALYTICS_API_DOCUMENTATION.md](business/DEALER_ANALYTICS_API_DOCUMENTATION.md)
- **Endpoints:** 12
- **Roadmap:** [DEALER_ANALYTICS_ROADMAP.md](business/DEALER_ANALYTICS_ROADMAP.md)

#### 7. ReviewService ⭐

- **Puerto:** 5059
- **Propósito:** Reviews estilo Amazon
- **Estado:** ✅ Producción
- **Documentación:** [REVIEW_API_DOCUMENTATION.md](business/REVIEW_API_DOCUMENTATION.md)
- **Endpoints:** 10
- **Roadmap:** [REVIEW_ROADMAP.md](business/REVIEW_ROADMAP.md)

### UX Services

#### 8. ChatbotService 🤖

- **Puerto:** 5060
- **Propósito:** Chatbot IA + WhatsApp
- **Estado:** 🚧 Sprint 21 (Q2 2026)
- **Documentación:** [CHATBOT_API_DOCUMENTATION.md](ux/CHATBOT_API_DOCUMENTATION.md)
- **Endpoints:** 8
- **Roadmap:** [CHATBOT_ROADMAP.md](ux/CHATBOT_ROADMAP.md)

#### 9. TestDriveService 🚗

- **Puerto:** 5064
- **Propósito:** Agendamiento test drives
- **Estado:** 📝 Planificado Q3 2026
- **Documentación:** [TEST_DRIVE_API_DOCUMENTATION.md](ux/TEST_DRIVE_API_DOCUMENTATION.md)
- **Endpoints:** 12
- **Roadmap:** [TEST_DRIVE_ROADMAP.md](ux/TEST_DRIVE_ROADMAP.md)

#### 10. FinancingService 💰

- **Puerto:** 5065
- **Propósito:** Financiamiento con bancos RD
- **Estado:** 📝 Planificado Q3 2026
- **Documentación:** [FINANCING_API_DOCUMENTATION.md](ux/FINANCING_API_DOCUMENTATION.md)
- **Endpoints:** 15
- **Roadmap:** [FINANCING_ROADMAP.md](ux/FINANCING_ROADMAP.md)

### Operations Services

#### 11. SupportService 💬

- **Puerto:** 5063
- **Propósito:** Help center + ticketing
- **Estado:** 📝 Planificado Q2 2026
- **Documentación:** [SUPPORT_API_DOCUMENTATION.md](operations/SUPPORT_API_DOCUMENTATION.md)
- **Endpoints:** 20
- **Roadmap:** [SUPPORT_ROADMAP.md](operations/SUPPORT_ROADMAP.md)

#### 12. FraudDetectionService 🛡️

- **Puerto:** 5062
- **Propósito:** Detección de fraude
- **Estado:** 📝 Planificado Q4 2026
- **Documentación:** [FRAUD_DETECTION_API_DOCUMENTATION.md](operations/FRAUD_DETECTION_API_DOCUMENTATION.md)
- **Endpoints:** 8
- **Roadmap:** [FRAUD_DETECTION_ROADMAP.md](operations/FRAUD_DETECTION_ROADMAP.md)

#### 13. PlatformAnalyticsService 📊

- **Puerto:** 5068
- **Propósito:** Dashboard ejecutivo
- **Estado:** 📝 Planificado Q3 2026
- **Documentación:** [PLATFORM_ANALYTICS_API_DOCUMENTATION.md](operations/PLATFORM_ANALYTICS_API_DOCUMENTATION.md)
- **Endpoints:** 15
- **Roadmap:** [PLATFORM_ANALYTICS_ROADMAP.md](operations/PLATFORM_ANALYTICS_ROADMAP.md)

---

## 🛠️ Infraestructura

Servicios de soporte y utilidades.

### 1. PostgreSQL Database

- **Puerto:** 5432
- **Propósito:** Base de datos principal
- **Estado:** ✅ Producción
- **Documentación:** [POSTGRES_DOCUMENTATION.md](infrastructure/POSTGRES_DOCUMENTATION.md)
- **Bases de Datos:** 20+

### 2. Redis Cache

- **Puerto:** 6379
- **Propósito:** Cache distribuido
- **Estado:** ✅ Producción
- **Documentación:** [REDIS_DOCUMENTATION.md](infrastructure/REDIS_DOCUMENTATION.md)
- **Uso:** Session, Cache, Rate limiting

### 3. RabbitMQ Message Broker

- **Puerto:** 5672, 15672 (management)
- **Propósito:** Message queue asíncrona
- **Estado:** ✅ Producción
- **Documentación:** [RABBITMQ_DOCUMENTATION.md](infrastructure/RABBITMQ_DOCUMENTATION.md)
- **Exchanges:** 10+

### 4. Let's Encrypt SSL

- **Proveedor:** Let's Encrypt + cert-manager
- **Propósito:** Certificados SSL/TLS
- **Estado:** ✅ Producción
- **Documentación:** [SSL_DOCUMENTATION.md](infrastructure/SSL_DOCUMENTATION.md)
- **Dominios:** okla.com.do, api.okla.com.do

### 5. GitHub Container Registry

- **Proveedor:** GitHub (ghcr.io)
- **Propósito:** Docker image registry
- **Estado:** ✅ Producción
- **Documentación:** [GHCR_DOCUMENTATION.md](infrastructure/GHCR_DOCUMENTATION.md)
- **Imágenes:** 20+ servicios

### 6. DigitalOcean Kubernetes (DOKS)

- **Proveedor:** DigitalOcean
- **Propósito:** Orquestación de containers
- **Estado:** ✅ Producción
- **Documentación:** [DOKS_DOCUMENTATION.md](infrastructure/DOKS_DOCUMENTATION.md)
- **Cluster:** okla-cluster, namespace: okla

---

## 📅 Roadmap General de APIs

### Q1 2026 (Enero - Marzo) ✅ EN PROGRESO

**Objetivo:** Consolidar microservicios core y dealers

- [x] Gateway (Ocelot) - Producción
- [x] AuthService - Producción
- [x] UserService - Producción
- [x] VehiclesSaleService - Producción
- [x] BillingService (Stripe + AZUL) - Producción
- [x] DealerManagementService - Producción
- [x] InventoryManagementService - Producción
- [x] DealerAnalyticsService - Producción
- [x] ReviewService - Producción
- [ ] EventTrackingService - Sprint 18
- [ ] MaintenanceBanner frontend - Sprint 18

**Hitos:**

- ✅ Early Bird Program activo (deadline: 31 enero 2026)
- ✅ Sistema de Reviews completo
- ✅ Dashboard avanzado dealers
- 🚧 Event tracking para ML

### Q2 2026 (Abril - Junio)

**Objetivo:** Machine Learning y recomendaciones

- [ ] RecommendationService
- [ ] LeadScoringService
- [ ] VehicleIntelligenceService
- [ ] ChatbotService (MVP)
- [ ] SupportService
- [ ] DataPipelineService
- [ ] FeatureStoreService

**Hitos:**

- Recomendaciones personalizadas activas
- Lead scoring automático
- Pricing inteligente con IA
- Chatbot básico funcionando

### Q3 2026 (Julio - Septiembre)

**Objetivo:** Experiencia de usuario premium

- [ ] TestDriveService
- [ ] FinancingService (integración bancos RD)
- [ ] PlatformAnalyticsService
- [ ] SearchService (Elasticsearch)
- [ ] AppointmentService

**Hitos:**

- Test drives agendables online
- Financiamiento pre-aprobado
- Dashboard ejecutivo completo
- Búsqueda avanzada con filtros ML

### Q4 2026 (Octubre - Diciembre)

**Objetivo:** Seguridad y escalabilidad

- [ ] FraudDetectionService
- [ ] AuditService completo
- [ ] BackupDRService
- [ ] RateLimitingService avanzado
- [ ] TracingService (OpenTelemetry)

**Hitos:**

- Sistema anti-fraude activo
- Auditoría completa de transacciones
- Disaster recovery automatizado
- Observabilidad completa (traces + metrics + logs)

---

## 🔗 Enlaces Útiles

### Documentación por Categoría

| Categoría           | Enlace                                       |
| ------------------- | -------------------------------------------- |
| **Pagos**           | [/docs/api/payments/](payments/)             |
| **Core**            | [/docs/api/core/](core/)                     |
| **Negocio**         | [/docs/api/business/](business/)             |
| **ML/Data**         | [/docs/api/ml/](ml/)                         |
| **UX**              | [/docs/api/ux/](ux/)                         |
| **Operaciones**     | [/docs/api/operations/](operations/)         |
| **Infraestructura** | [/docs/api/infrastructure/](infrastructure/) |

### Guías y Tutoriales

- [Guía de inicio rápido](../guides/QUICK_START.md)
- [Arquitectura general](../architecture/SYSTEM_ARCHITECTURE.md)
- [Convenciones de API](../guides/API_CONVENTIONS.md)
- [Autenticación y seguridad](../guides/AUTHENTICATION_GUIDE.md)
- [Testing de APIs](../guides/API_TESTING_GUIDE.md)

### Recursos Externos

- [Documentación oficial Stripe](https://stripe.com/docs/api)
- [Documentación oficial AZUL](https://desarrolladores.azul.com.do)
- [AWS S3 API Reference](https://docs.aws.amazon.com/s3/)
- [Ocelot Documentation](https://ocelot.readthedocs.io/)

---

## 📞 Contacto y Soporte

**Equipo de Desarrollo:**

- Email: dev@okla.com.do
- Slack: #api-support
- Jira: OKLA Project

**Reporte de Issues:**

- GitHub Issues: https://github.com/gregorymorenoiem/cardealer-microservices/issues
- SLA: 24h para bugs críticos

---

**Última actualización:** Enero 15, 2026  
**Versión del documento:** 1.0  
**Mantenido por:** Equipo de Arquitectura OKLA
