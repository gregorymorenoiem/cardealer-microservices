# 🚀 PLAN MAESTRO - Integración Frontend Original con Backend

**Fecha:** 2 Enero 2026  
**Objetivo:** Integrar completamente frontend/web/original con backend microservicios  
**Estado:** 🟡 En Progreso

---

## 📋 ÍNDICE DE SPRINTS

Este plan está dividido en **12 sub-planes** manejables, cada uno enfocado en una funcionalidad específica:

| Sprint        | Documento                                                    | Tokens Est. | Estado       | Prioridad     |
| ------------- | ------------------------------------------------------------ | ----------- | ------------ | ------------- |
| **Sprint 0**  | [SPRINT_0_SETUP_INICIAL.md](SPRINT_0_SETUP_INICIAL.md)       | ~18,000     | ⚪ Pendiente | 🔴 CRÍTICO    |
|               | **+ Migración Assets (16-20h)**                              |             |              | 🔴 BLOQUEANTE |
| **Sprint 1**  | [SPRINT_1_CUENTAS_TERCEROS.md](SPRINT_1_CUENTAS_TERCEROS.md) | ~22,000     | ⚪ Pendiente | 🔴 CRÍTICO    |
| **Sprint 2**  | [SPRINT_2_AUTH_INTEGRATION.md](SPRINT_2_AUTH_INTEGRATION.md) | ~25,000     | ⚪ Pendiente | 🔴 CRÍTICO    |
| **Sprint 3**  | [SPRINT_3_VEHICLE_SERVICE.md](SPRINT_3_VEHICLE_SERVICE.md)   | ~28,000     | ⚪ Pendiente | 🟠 Alta       |
|               | **+ Seed Catálogo Vehículos (12-16h)**                       |             |              | 🔴 CRÍTICO    |
| **Sprint 4**  | [SPRINT_4_MEDIA_UPLOAD.md](SPRINT_4_MEDIA_UPLOAD.md)         | ~24,000     | ⚪ Pendiente | 🟠 Alta       |
| **Sprint 5**  | [SPRINT_5_BILLING_PAYMENTS.md](SPRINT_5_BILLING_PAYMENTS.md) | ~26,000     | ⚪ Pendiente | 🟠 Alta       |
| **Sprint 6**  | [SPRINT_6_NOTIFICATIONS.md](SPRINT_6_NOTIFICATIONS.md)       | ~23,000     | ⚪ Pendiente | 🟡 Media      |
| **Sprint 7**  | [SPRINT_7_MESSAGING_CRM.md](SPRINT_7_MESSAGING_CRM.md)       | ~22,000     | ⚪ Pendiente | 🟡 Media      |
| **Sprint 8**  | [SPRINT_8_SEARCH_FILTERS.md](SPRINT_8_SEARCH_FILTERS.md)     | ~25,000     | ⚪ Pendiente | 🟡 Media      |
| **Sprint 9**  | [SPRINT_9_SAVED_SEARCHES.md](SPRINT_9_SAVED_SEARCHES.md)     | ~20,000     | ⚪ Pendiente | 🟢 Baja       |
| **Sprint 10** | [SPRINT_10_ADMIN_PANEL.md](SPRINT_10_ADMIN_PANEL.md)         | ~27,000     | ⚪ Pendiente | 🟡 Media      |
| **Sprint 11** | [SPRINT_11_TESTING_QA.md](SPRINT_11_TESTING_QA.md)           | ~30,000     | ⚪ Pendiente | 🟠 Alta       |

**Total estimado:** ~290,000 tokens (~15 sesiones de trabajo)

---

## 🎯 OBJETIVOS GENERALES

### Funcionalidades a Implementar

#### 1️⃣ Autenticación y Usuarios

- ✅ Login/Registro con JWT
- ✅ OAuth2 (Google, Microsoft)
- ✅ Gestión de perfiles
- ✅ Recuperación de contraseña
- ⚪ 2FA/TOTP

#### 2️⃣ Vehículos

- ⚪ CRUD completo de vehículos
- ⚪ Upload de imágenes (hasta 20 por vehículo)
- ⚪ Búsqueda avanzada con filtros
- ⚪ Vista de mapa con Google Maps
- ⚪ Vehículos destacados
- ⚪ Comparador de vehículos

#### 3️⃣ Media y Almacenamiento

- ⚪ Upload de imágenes a S3/Azure Blob
- ⚪ Compresión automática de imágenes
- ⚪ Progressive loading
- ⚪ CDN integration

#### 4️⃣ Facturación y Pagos

- ⚪ Suscripciones por planes
- ⚪ Integración con Stripe
- ⚪ Gestión de métodos de pago
- ⚪ Historial de facturas

#### 5️⃣ Notificaciones

- ⚪ Email (SendGrid/SMTP)
- ⚪ SMS (Twilio)
- ⚪ Push notifications (Firebase)
- ⚪ Notificaciones en app

#### 6️⃣ Mensajería y CRM

- ⚪ Chat entre usuarios
- ⚪ Gestión de leads
- ⚪ Seguimiento de conversaciones

#### 7️⃣ Búsqueda

- ⚪ Elasticsearch integration
- ⚪ Autocompletado
- ⚪ Filtros avanzados
- ⚪ Búsquedas guardadas

#### 8️⃣ Admin

- ⚪ Dashboard de métricas
- ⚪ Gestión de usuarios
- ⚪ Moderación de contenido
- ⚪ Reportes

---

## 🏗️ ARQUITECTURA DE INTEGRACIÓN

```
┌─────────────────────────────────────────────────────────────┐
│                    Frontend (React 19)                      │
│                  localhost:5174                             │
│  ┌─────────┬─────────┬─────────┬─────────┬─────────┐      │
│  │  Auth   │ Vehicle │  Media  │ Billing │  Admin  │      │
│  │ Service │ Service │ Service │ Service │ Service │      │
│  └─────────┴─────────┴─────────┴─────────┴─────────┘      │
└────────────────────────┬────────────────────────────────────┘
                         │ HTTPS
                         ▼
┌─────────────────────────────────────────────────────────────┐
│              API Gateway (Ocelot)                           │
│              localhost:18443                                │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Routing │ Auth │ Rate Limit │ CORS │ Load Balance │  │
│  └──────────────────────────────────────────────────────┘  │
└────────────────────────┬────────────────────────────────────┘
                         │ Service Mesh
          ┌──────────────┼──────────────┬─────────────┐
          ▼              ▼              ▼             ▼
    ┌─────────┐    ┌─────────┐    ┌─────────┐  ┌─────────┐
    │  Auth   │    │ Product │    │  Media  │  │ Billing │
    │ Service │    │ Service │    │ Service │  │ Service │
    │ :15085  │    │ :15006  │    │ :15007  │  │ :15008  │
    └────┬────┘    └────┬────┘    └────┬────┘  └────┬────┘
         │              │              │            │
         └──────────────┴──────────────┴────────────┘
                         │
                         ▼
              ┌─────────────────────┐
              │   RabbitMQ Events   │
              │   PostgreSQL DBs    │
              │   Redis Cache       │
              └─────────────────────┘
```

---

## 🔌 SERVICIOS EXTERNOS REQUERIDOS

### Cuentas a Crear (Sprint 1)

| Servicio                  | Propósito                      | Costo                        | Prioridad   | Plan                      |
| ------------------------- | ------------------------------ | ---------------------------- | ----------- | ------------------------- |
| **Google Cloud Platform** | Google Maps API                | $200/mes credit gratis       | 🔴 Crítico  | Pay-as-you-go             |
| **Firebase**              | Push Notifications             | Gratis hasta 10K users       | 🟠 Alta     | Spark (Free)              |
| **Stripe**                | Pagos y Suscripciones          | 2.9% + $0.30 por transacción | 🔴 Crítico  | Pay-as-you-go             |
| **SendGrid**              | Email transaccional            | 100 emails/día gratis        | 🟠 Alta     | Free → Essentials $20/mes |
| **Twilio**                | SMS                            | $15 credit gratis            | 🟡 Media    | Pay-as-you-go             |
| **AWS S3**                | Almacenamiento de imágenes     | 5GB gratis primer año        | 🔴 Crítico  | Free Tier → S3 Standard   |
| **Azure Blob Storage**    | Almacenamiento alternativo     | 5GB gratis                   | 🟢 Opcional | Pay-as-you-go             |
| **Sentry**                | Error tracking                 | 5K events/mes gratis         | 🟡 Media    | Developer (Free)          |
| **Elasticsearch**         | Search + Indexación de errores | GRATIS (DOKS)                | 🔴 Crítico  | Self-managed (Helm)       |
| **Google Analytics 4**    | Web Vitals + Analytics         | Gratis hasta 10M eventos/mes | 🟡 Media    | Free Tier                 |

**Costo mensual estimado:** $50-$200 (según tráfico) + Elasticsearch en DOKS (incluido)

---

## 📊 MICROSERVICIOS BACKEND

### Servicios Existentes (Listos)

| Servicio            | Puerto | Estado | Endpoints              |
| ------------------- | ------ | ------ | ---------------------- |
| Gateway             | 18443  | ✅     | Routing + Auth         |
| AuthService         | 15085  | ✅     | 11 endpoints           |
| UserService         | 15100  | ✅     | Gestión usuarios       |
| RoleService         | 15101  | ✅     | Roles y permisos       |
| ProductService      | 15006  | ✅     | CRUD productos         |
| ErrorService        | 15083  | ✅     | Error logging          |
| NotificationService | 15084  | ✅     | Email, SMS, Push       |
| MediaService        | 15007  | ⚠️     | **Necesita endpoints** |
| BillingService      | 15008  | ⚠️     | **Necesita endpoints** |
| CRMService          | 15009  | ⚠️     | **Necesita endpoints** |
| SearchService       | 15010  | ⚠️     | **Necesita endpoints** |
| AdminService        | 15011  | ⚠️     | **Necesita endpoints** |

### Servicios a Crear (Sprint 3)

| Servicio               | Propósito                                                 | Prioridad  |
| ---------------------- | --------------------------------------------------------- | ---------- |
| **VehicleService**     | Gestión específica de vehículos (extiende ProductService) | 🔴 CRÍTICO |
| **SavedSearchService** | Búsquedas guardadas y alertas                             | 🟡 Media   |
| **ComparisonService**  | Comparador de vehículos                                   | 🟢 Baja    |
| **DealerService**      | Gestión de dealers (multi-tenant)                         | 🟠 Alta    |

---

## 🛣️ RUTA DE IMPLEMENTACIÓN

### Orden Recomendado

```
FASE 0: Setup Inicial (Sprint 0-1)
├── Sprint 0: Configuración de entorno
└── Sprint 1: Crear cuentas de terceros

FASE 1: Core Features (Sprint 2-4) - 2-3 días
├── Sprint 2: Autenticación completa
├── Sprint 3: VehicleService + CRUD
└── Sprint 4: Upload de imágenes

FASE 2: Pagos y Notificaciones (Sprint 5-6) - 2 días
├── Sprint 5: Billing + Stripe
└── Sprint 6: Notificaciones multi-canal

FASE 3: Features Avanzados (Sprint 7-9) - 2-3 días
├── Sprint 7: Mensajería + CRM
├── Sprint 8: Búsqueda + Filtros
└── Sprint 9: Búsquedas guardadas

FASE 4: Admin y QA (Sprint 10-11) - 2 días
├── Sprint 10: Panel de administración
└── Sprint 11: Testing completo
```

**Tiempo total estimado:** 8-12 días de trabajo

---

## 📝 CONVENCIONES

### Naming de Endpoints

```
GET    /api/vehicles              # Listar con paginación
GET    /api/vehicles/{id}         # Obtener por ID
POST   /api/vehicles              # Crear
PUT    /api/vehicles/{id}         # Actualizar
DELETE /api/vehicles/{id}         # Eliminar
GET    /api/vehicles/featured     # Destacados
POST   /api/vehicles/{id}/images  # Upload imágenes
```

### Response Format

```json
{
  "success": true,
  "data": { ... },
  "message": "Operation successful",
  "timestamp": "2026-01-02T10:30:00Z"
}
```

### Error Format

```json
{
  "success": false,
  "error": {
    "code": "VEHICLE_NOT_FOUND",
    "message": "Vehicle with ID abc123 not found",
    "details": {}
  },
  "timestamp": "2026-01-02T10:30:00Z"
}
```

---

## 🔐 VARIABLES DE ENTORNO

### Frontend (.env)

```env
# API Endpoints
VITE_API_URL=http://localhost:18443/api
VITE_AUTH_SERVICE_URL=http://localhost:15085/api
VITE_VEHICLE_SERVICE_URL=http://localhost:15006/api
VITE_UPLOAD_SERVICE_URL=http://localhost:15007/api

# Third Party APIs
VITE_GOOGLE_MAPS_API_KEY=your-google-maps-key
VITE_STRIPE_PUBLIC_KEY=pk_test_xxxxx
VITE_FIREBASE_CONFIG={"apiKey":"xxx",...}

# Feature Flags
VITE_USE_MOCK_AUTH=false
VITE_ENABLE_2FA=true
VITE_ENABLE_PUSH_NOTIFICATIONS=true
```

### Backend (compose.yaml secrets)

```yaml
# Se configurarán en Sprint 0
JWT__KEY: ${JWT__KEY}
GOOGLE_MAPS_API_KEY: ${GOOGLE_MAPS_API_KEY}
STRIPE_SECRET_KEY: ${STRIPE_SECRET_KEY}
SENDGRID_API_KEY: ${SENDGRID_API_KEY}
TWILIO_AUTH_TOKEN: ${TWILIO_AUTH_TOKEN}
AWS_ACCESS_KEY_ID: ${AWS_ACCESS_KEY_ID}
AWS_SECRET_ACCESS_KEY: ${AWS_SECRET_ACCESS_KEY}
```

---

## 📈 MÉTRICAS DE ÉXITO

### Criterios de Aceptación

- [ ] ✅ Frontend se conecta a Gateway sin errores
- [ ] ✅ Autenticación funciona con JWT + OAuth2
- [ ] ✅ CRUD de vehículos funciona end-to-end
- [ ] ✅ Upload de imágenes guarda en S3/Azure
- [ ] ✅ Stripe procesa pagos en sandbox
- [ ] ✅ Notificaciones se envían correctamente
- [ ] ✅ Búsqueda retorna resultados en <2s
- [ ] ✅ Tests de integración pasan al 100%
- [ ] ✅ Coverage de código >80%
- [ ] ✅ Sin errores críticos en Sentry

### KPIs Técnicos

| Métrica             | Objetivo | Actual |
| ------------------- | -------- | ------ |
| Response Time (p95) | <500ms   | -      |
| Error Rate          | <1%      | -      |
| API Availability    | 99.9%    | -      |
| Test Coverage       | >80%     | -      |
| Build Time          | <5min    | -      |

---

## 🚨 RIESGOS Y MITIGACIONES

| Riesgo                                      | Probabilidad | Impacto | Mitigación                                   |
| ------------------------------------------- | ------------ | ------- | -------------------------------------------- |
| Costos de APIs externos superan presupuesto | Media        | Alto    | Implementar rate limiting y caching          |
| Stripe sandbox no disponible                | Baja         | Alto    | Usar mocks como fallback                     |
| Problemas de CORS en Gateway                | Alta         | Medio   | Configurar CORS correctamente desde Sprint 0 |
| JWT tokens expiran durante testing          | Alta         | Bajo    | Aumentar expiración en dev                   |
| Elasticsearch consume mucha RAM             | Media        | Medio   | Configurar limits en Docker                  |

---

## 📞 PRÓXIMOS PASOS

1. **AHORA:** Revisar este plan maestro
2. **Siguiente:** Ejecutar Sprint 0 (Setup Inicial)
3. **Luego:** Ejecutar Sprint 1 (Cuentas de Terceros)
4. **Después:** Continuar con Sprint 2 (Auth Integration)

---

## 📚 DOCUMENTOS RELACIONADOS

- [Backend Copilot Instructions](../../../.github/copilot-instructions.md)
- [FASE_1_PROGRESS_REPORT.md](../FASE_1_PROGRESS_REPORT.md)
- [SPRINT_0.7.2_SECRETS_VALIDATION_COMPLETION.md](../SPRINT_0.7.2_SECRETS_VALIDATION_COMPLETION.md)
- [Frontend Package.json](../../../frontend/web/original/package.json)

---

**Última actualización:** 2 Enero 2026  
**Responsable:** Gregory Moreno  
**Revisión:** Pendiente
