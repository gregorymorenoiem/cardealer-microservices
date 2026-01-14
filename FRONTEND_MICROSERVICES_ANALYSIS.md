# 📊 Análisis de Microservicios Utilizados por el Frontend

**Fecha de Análisis:** Enero 13, 2026  
**Proyecto:** OKLA (CarDealer Microservices)  
**Frontend:** React 19 + TypeScript (Vite)

---

## 🎯 RESUMEN EJECUTIVO

El frontend de OKLA está diseñado para comunicarse con **28+ microservicios** a través del **Gateway API (Ocelot)** en puerto **18443**.

### 📈 Estadísticas:

- **Servicios en Frontend:** 28 (incluyendo plantillas para no implementados)
- **Servicios Levantados:** 16
- **Servicios Faltantes:** 12 servicios que aún no están implementados
- **Endpoints Mapeados:** 150+ endpoints REST

---

## ✅ SERVICIOS LEVANTADOS (16 corriendo)

### 🔧 Infraestructura Base

| Servicio       | Puerto     | Status     | Uso                     |
| -------------- | ---------- | ---------- | ----------------------- |
| **PostgreSQL** | 5433       | ✅ Running | Base de datos principal |
| **Redis**      | 6379       | ✅ Running | Cache distribuido       |
| **RabbitMQ**   | 5672/15672 | ✅ Running | Message broker          |
| **Consul**     | 8501       | ✅ Running | Service discovery       |

### 🔐 Servicios Core

| Servicio        | Puerto | Status     | Endpoints                                                          |
| --------------- | ------ | ---------- | ------------------------------------------------------------------ |
| **AuthService** | 15085  | ✅ Healthy | `/api/auth/register`, `/api/auth/login`, `/api/auth/refresh-token` |
| **UserService** | 15100  | ✅ Healthy | `/api/users`                                                       |
| **RoleService** | 15101  | ✅ Healthy | `/api/roles`                                                       |

### 🚗 Servicios Principales

| Servicio                | Puerto | Status     | Endpoints                                                |
| ----------------------- | ------ | ---------- | -------------------------------------------------------- |
| **VehiclesSaleService** | 15070  | ✅ Healthy | `/api/vehicles`, `/api/catalog`, `/api/homepagesections` |
| **MediaService**        | 15090  | ✅ Running | `/api/media/upload`, `/api/media/{id}`                   |
| **NotificationService** | 15084  | ✅ Healthy | `/api/notifications`                                     |
| **ErrorService**        | 15083  | ✅ Healthy | `/api/errors`                                            |

### 🛠️ Servicios Adicionales

| Servicio                    | Puerto | Status      | Endpoints                                  |
| --------------------------- | ------ | ----------- | ------------------------------------------ |
| **Gateway (API)**           | 18443  | ✅ Healthy  | Enrutador central para todos los servicios |
| **AdminService**            | 15112  | ✅ Starting | `/api/admin/*`                             |
| **BillingService**          | 15107  | ✅ Running  | `/api/billing/*`                           |
| **DealerManagementService** | 15039  | ✅ Starting | `/api/dealers`, `/api/dealer-*`            |
| **MaintenanceService**      | 5061   | ✅ Starting | `/api/maintenance/*`                       |

---

## ❌ SERVICIOS FALTANTES (12 no implementados)

### 🚨 CRÍTICOS - Frontend está esperando estos:

#### 1. **ChatbotService** (⚠️ MUY IMPORTANTE)

- **Endpoints esperados:** `/api/conversations`
- **Métodos:**
  - POST `/api/conversations` - Crear conversación
  - POST `/api/conversations/{id}/messages` - Enviar mensaje
  - POST `/api/conversations/{id}/handoff` - Transferir a humano
  - GET `/api/conversations/hot-leads` - Leads calientes
  - GET `/api/conversations/statistics/dealer/{dealerId}` - Estadísticas
- **Propósito:** Chatbot IA + Lead scoring + Escalado a agentes
- **Prioridad:** 🔴 ALTA - Usado en múltiples funciones

#### 2. **CRMService** (⚠️ MUY IMPORTANTE)

- **Endpoints esperados:** `/api/crm/*`
- **Métodos:**
  - `/api/crm/leads` - CRUD de leads
  - `/api/crm/deals` - CRUD de oportunidades
  - `/api/crm/pipelines` - Gestión de pipelines
  - `/api/crm/activities` - Actividades y tareas
  - `/api/crm/stats` - Estadísticas
- **Propósito:** Gestión de clientes y oportunidades
- **Prioridad:** 🔴 ALTA - Dashboard de dealers depende

#### 3. **AlertService**

- **Endpoints esperados:** `/api/pricealerts`, `/api/savedsearches`
- **Métodos:**
  - POST `/api/pricealerts` - Crear alerta de precio
  - GET `/api/pricealerts` - Listar alertas
  - POST `/api/savedsearches` - Guardar búsquedas
  - PUT `/api/savedsearches/{id}/activate` - Activar búsquedas
- **Propósito:** Notificaciones de precios y búsquedas guardadas
- **Prioridad:** 🟡 MEDIA - Sprint 1 feature completado pero sin backend

#### 4. **ReviewService**

- **Endpoints esperados:** `/api/reviews`
- **Métodos:**
  - GET `/api/reviews/seller` - Reviews de vendedor
  - GET `/api/reviews/{id}` - Obtener review
  - POST `/api/reviews` - Crear review (no implementado)
  - PUT `/api/reviews/{id}` - Moderar review
  - POST `/api/reviews/{id}/vote` - Votar review
- **Propósito:** Sistema de reseñas estilo Amazon
- **Prioridad:** 🟡 MEDIA - UI completada, falta backend

#### 5. **RecommendationService**

- **Endpoints esperados:** `/api/recommendations`
- **Métodos:**
  - GET `/api/recommendations/vehicles-for-you` - Vehículos personalizados
  - GET `/api/recommendations/similar` - Vehículos similares
- **Propósito:** Motor de recomendaciones IA
- **Prioridad:** 🟡 MEDIA - Componente UI existe

#### 6. **VehicleIntelligenceService**

- **Endpoints esperados:** `/api/vehicle-intelligence`
- **Métodos:**
  - GET `/api/vehicle-intelligence/pricing` - Análisis de precios
  - GET `/api/vehicle-intelligence/demand` - Predicción de demanda
- **Propósito:** IA para pricing y análisis de mercado
- **Prioridad:** 🟡 MEDIA - Soporte para dealers

#### 7. **UserBehaviorService**

- **Endpoints esperados:** `/api/userbehavior`
- **Métodos:**
  - POST `/api/userbehavior/track` - Rastrear eventos
  - GET `/api/userbehavior/profile/{userId}` - Perfil de comportamiento
- **Propósito:** Análisis de comportamiento de usuarios
- **Prioridad:** 🟡 MEDIA - Para segmentación

---

### 🟢 SERVICIOS PARCIALMENTE UTILIZADOS:

#### 8. **DealerBillingService**

- **Endpoints esperados:** `/api/dealer-billing/*`
- **Estado:** Frontend llama pero no totalmente implementado
- **Métodos:**
  - GET `/api/dealer-billing/dashboard/{dealerId}`
  - GET `/api/dealer-billing/subscription`
  - GET `/api/dealer-billing/invoices`
  - POST `/api/dealer-billing/payment-methods`
- **Prioridad:** 🔴 ALTA - Billing es crítico

#### 9. **AzulPaymentService**

- **Endpoints esperados:** `/api/payment/azul/*, /api/azul/*`
- **Estado:** Integración con pasarela AZUL (Banco Popular RD)
- **Métodos:**
  - POST `/api/azul/payment-page/init`
  - GET `/api/payment/azul/transaction/{orderNumber}`
- **Prioridad:** 🔴 ALTA - Pagos críticos

#### 10. **StripeService**

- **Endpoints esperados:** `/api/payment/stripe/*`
- **Estado:** NO ENCONTRADO en frontend (pero hay intención de implementar)
- **Propósito:** Pasarela de pagos internacionales
- **Prioridad:** 🔴 ALTA - Pagos críticos

---

### 📦 SERVICIOS OPCIONALES:

#### 11. **ListingAnalyticsService**

- **Endpoints esperados:** `/api/listing-analytics`
- **Propósito:** Estadísticas de publicaciones
- **Prioridad:** 🟡 BAJA - Puede esperar

#### 12. **MaintenanceService**

- **Status:** ✅ Levantado pero se necesita GET `/api/maintenance/current`
- **Propósito:** Modo de mantenimiento programable
- **Prioridad:** 🟢 BAJA - Infraestructura

---

## 📋 MAPA DE ENDPOINTS POR SERVICIO

### VehiclesSaleService (15070)

```
GET  /api/vehicles              - Listar vehículos
GET  /api/vehicles/{id}         - Obtener vehículo
POST /api/vehicles              - Crear vehículo
PUT  /api/vehicles/{id}         - Actualizar vehículo
GET  /api/homepagesections/homepage - Secciones del home
GET  /api/catalog/makes         - Marcas
GET  /api/catalog/models/{id}   - Modelos
```

### AuthService (15085)

```
POST /api/auth/register         - Registro
POST /api/auth/login            - Login
POST /api/auth/refresh-token    - Refresh JWT
POST /api/auth/change-password  - Cambiar contraseña
POST /api/auth/2fa/enable       - 2FA
GET  /api/auth/security         - Configuración de seguridad
```

### MediaService (15090)

```
POST /api/media/upload          - Subir archivo (multipart)
GET  /api/media/{id}            - Obtener archivo
DELETE /api/media/{id}          - Eliminar archivo
```

### NotificationService (15084)

```
GET  /api/notifications         - Listar notificaciones
PATCH /api/notifications/{id}/read - Marcar como leído
GET  /api/notifications/preferences - Preferencias
PUT  /api/notifications/preferences - Actualizar preferencias
POST /api/notifications/push/subscribe - Push notifications
```

### Dealers (DealerManagementService)

```
GET  /api/dealers               - Listar dealers
POST /api/dealers               - Crear dealer
GET  /api/dealers/{id}          - Obtener dealer
PUT  /api/dealers/{id}          - Actualizar dealer
POST /api/dealers/{id}/verify   - Verificar dealer
GET  /api/dealers/{id}/employees - Empleados
POST /api/dealers/{id}/modules/{moduleCode}/subscribe - Suscribirse a módulo
```

---

## 🔄 FLUJO DE DATOS FRONTEND → BACKEND

```
┌─────────────────────────────────────┐
│         FRONTEND (React)             │
│  - VehiclesOnlyHomePage             │
│  - DealerDashboard                  │
│  - AdminPanel                       │
│  - UserProfile                      │
└──────────┬──────────────────────────┘
           │
           ├─→ API Client (axios)
           │   └─→ JWT Token Injection
           │       └─→ Token Refresh
           │
           ▼
┌──────────────────────────────────────┐
│   API GATEWAY (Ocelot) - :18443      │
│   - Enrutamiento                     │
│   - Rate limiting                    │
│   - Load balancing                   │
└──┬───────┬───────┬────────┬─────────┘
   │       │       │        │
   ▼       ▼       ▼        ▼
┌─────┐ ┌─────┐ ┌──────┐ ┌─────────┐
│Auth │ │User │ │Roles │ │Vehicles │ ...
│Svc  │ │Svc  │ │Svc   │ │SaleSvc  │
└─────┘ └─────┘ └──────┘ └─────────┘
   │       │       │        │
   └───────┴───────┴────────┴──────────┐
                                       │
                                       ▼
                          ┌──────────────────────┐
                          │  PostgreSQL (5433)   │
                          │  Redis (6379)        │
                          │  RabbitMQ (5672)     │
                          └──────────────────────┘
```

---

## 🚨 SERVICIOS QUE NECESITA IMPLEMENTAR INMEDIATAMENTE

### 🔴 PRIORIDAD 1 - BLOQUEA FEATURES PRINCIPALES:

1. **ChatbotService** - Múltiples pantallas dependen (leads, support, IA)
2. **CRMService** - Dashboard de dealers completamente bloqueado
3. **DealerBillingService** - Billing y pagos bloqueados
4. **AlertService** - Features de alerts (Sprint 1) bloqueadas

### 🟡 PRIORIDAD 2 - FEATURES DE VALOR:

5. **ReviewService** - Reviews/ratings
6. **RecommendationService** - Recomendaciones personalizadas
7. **AzulPaymentService** - Pagos con tarjetas dominicanas
8. **StripeService** - Pagos internacionales

### 🟢 PRIORIDAD 3 - COMPLEMENTARIOS:

9. **VehicleIntelligenceService** - IA para pricing
10. **UserBehaviorService** - Analytics de usuarios
11. **ListingAnalyticsService** - Estadísticas de vehículos

---

## 📊 RESUMEN POR ESTADO

### ✅ IMPLEMENTADOS Y CORRIENDO (8):

- ✅ Gateway
- ✅ AuthService
- ✅ UserService
- ✅ RoleService
- ✅ VehiclesSaleService
- ✅ MediaService
- ✅ NotificationService
- ✅ ErrorService

### 🔄 PARCIALMENTE IMPLEMENTADOS (4):

- 🔄 AdminService (levantado, pero no usado en frontend)
- 🔄 BillingService (levantado, pero CRUDly basic)
- 🔄 DealerManagementService (levantado, pero falta billing integration)
- 🔄 MaintenanceService (levantado, pero falta endpoint principal)

### ❌ FALTANTES COMPLETAMENTE (12):

- ❌ ChatbotService
- ❌ CRMService
- ❌ AlertService (backend)
- ❌ ReviewService
- ❌ RecommendationService
- ❌ VehicleIntelligenceService
- ❌ UserBehaviorService
- ❌ ListingAnalyticsService
- ❌ AzulPaymentService
- ❌ StripePaymentService
- ❌ ComparisonService
- ❌ SearchService (si está separado)

---

## 🏗️ RECOMENDACIÓN: ORDEN DE IMPLEMENTACIÓN

### Sprint Siguiente (Próximas 2-3 semanas):

**SEMANA 1-2:**

1. Implementar **ChatbotService** (backend + routing en Gateway)
2. Implementar **CRMService** (backend + routing)
3. Conectar **BillingService** completamente

**SEMANA 2-3:**

1. Implementar **AlertService** (completar Sprint 1)
2. Integrar **AzulPaymentService** realmente
3. Integrar **StripePaymentService**

**Después:**

1. ReviewService
2. RecommendationService
3. Analytics services

---

## 📚 REFERENCIAS

- **Gateway Config:** `/backend/Gateway/Gateway.Api/ocelot.prod.json`
- **Frontend Services:** `/frontend/web/src/services/`
- **API Types:** `/frontend/web/src/types/`

---

_Documento generado automáticamente el 13 de Enero 2026_
_Por: GitHub Copilot Analysis_
