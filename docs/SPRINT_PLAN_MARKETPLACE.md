# 🚀 Plan de Sprints - OKLA Marketplace

**Fecha:** Enero 8, 2026  
**Objetivo:** Marketplace de vehículos 100% funcional  
**Metodología:** Sprints de 2 semanas  
**Equipo estimado:** 3-4 desarrolladores full-stack

---

## 🎁 ESTRATEGIA DE LANZAMIENTO: EARLY BIRD

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       PLAN EARLY BIRD (3 MESES GRATIS)                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  🎁 DURANTE LANZAMIENTO (3 meses):                                         │
│  ├── ✅ TODOS publican GRATIS                                              │
│  ├── ✅ Sin límite de publicaciones                                        │
│  ├── ✅ Todas las features premium                                         │
│  ├── ✅ Badge "Miembro Fundador" permanente                                │
│  └── ✅ 20% descuento DE POR VIDA después del período                      │
│                                                                             │
│  💰 DESPUÉS DE EARLY BIRD:                                                  │
│  ├── Vendedores: $29/listing (Early Birds: $23)                            │
│  ├── Dealer Starter: $49/mes (Early Birds: $39)                            │
│  ├── Dealer Pro: $129/mes (Early Birds: $103)                              │
│  └── Dealer Enterprise: $299/mes (Early Birds: $239)                       │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 🚨 SPRINT 3: CONSOLIDACIÓN DE BASE DE DATOS (PRIORIDAD ALTA)

**Duración:** 14 días  
**Objetivo:** Centralizar todas las bases de datos de microservicios en PostgresDbService

### 🎯 Objetivo Principal

Migrar todas las bases de datos individuales de microservicios (`*_db`) a un servicio centralizado `postgres_db` usando arquitectura JSONB flexible.

### 📝 Tareas del Sprint 3

#### 1️⃣ PostgresDbService - Servicio de Base de Datos Centralizada

**PRIMERA TAREA DEL SPRINT 3:**

- [ ] **PostgresDbService.Domain** ✅ COMPLETADO

  - Entidades base con JSONB support
  - Interfaces de repositorio genérico y específico
  - Multi-tenancy integration

- [ ] **PostgresDbService.Infrastructure** ✅ COMPLETADO

  - CentralizedDbContext con EF Core
  - GenericRepository con CRUD completo
  - Repositorios específicos (User, Vehicle, Contact)
  - Indexing strategy para performance

- [ ] **PostgresDbService.Api** ✅ COMPLETADO

  - Controllers genéricos y específicos
  - Health checks y Swagger docs
  - JWT authentication
  - Docker containerization

- [ ] **Testing Infrastructure** ✅ COMPLETADO
  - Proyecto de tests PostgresDbService.Tests
  - Test helpers y factories
  - Unit tests para GenericRepository (8 tests)
  - Integration tests para controllers

#### 2️⃣ Migración de Datos

- [ ] **Análisis de Esquemas Existentes**

  - Inventario de todas las bases de datos actuales
  - Mapeo de entidades a estructura JSONB
  - Plan de migración de datos sin downtime

- [ ] **Scripts de Migración**

  - Exportar datos de ApplicationDbContext individuales
  - Transformar a formato JSONB
  - Import automático a PostgresDbService

- [ ] **Validación de Datos**
  - Verificar integridad después de migración
  - Tests de performance con datos reales
  - Rollback plan en caso de problemas

#### 3️⃣ Actualización de Microservicios

- [ ] **Remover ApplicationDbContext individuales**

  - AuthService → Use PostgresDbService
  - UserService → Use PostgresDbService
  - VehiclesSaleService → Use PostgresDbService
  - ContactService → Use PostgresDbService
  - Otros servicios según aplique

- [ ] **Actualizar Referencias**

  - Cambiar dependencias de Entity Framework
  - Actualizar connection strings
  - Refactorizar repositories

- [ ] **Testing de Integración**
  - Todos los endpoints funcionando
  - Performance igual o mejor
  - No breaking changes en API

#### 4️⃣ CI/CD y Deployment

- [ ] **Agregar PostgresDbService a smart-cicd.yml**
- [ ] **Kubernetes manifests**

  - Deployment para PostgresDbService
  - Service y ConfigMaps
  - Ingress routing

- [ ] **Database Migration Strategy**
  - Blue-green deployment approach
  - Zero-downtime migration
  - Monitoring y alerting

### 🧪 Testing Requirements (OBLIGATORIO)

- [ ] **PostgresDbService.Tests**

  - ✅ GenericRepositoryTests (8 tests)
  - [ ] UserRepositoryTests (6 tests)
  - [ ] VehicleRepositoryTests (7 tests)
  - [ ] ContactRepositoryTests (5 tests)
  - [ ] ControllersTests (10 tests)
  - [ ] IntegrationTests (5 tests)

- [ ] **Migration Tests**
  - [ ] Data integrity tests (3 tests)
  - [ ] Performance regression tests (2 tests)
  - [ ] Rollback scenario tests (2 tests)

### 📊 Success Criteria

1. ✅ PostgresDbService completamente funcional
2. [ ] Todos los datos migrados sin pérdida
3. [ ] Todos los microservicios funcionando con nueva DB
4. [ ] Performance igual o mejor que antes
5. [ ] Zero downtime durante migración
6. [ ] 100% test coverage en componentes críticos
7. [ ] CI/CD pipeline funcionando
8. [ ] Documentación de migración completa

---

## 📊 ESTADO ACTUAL (Baseline)

### ✅ Ya en Producción (DOKS)

| Servicio            | Estado | Funcionalidad             |
| ------------------- | ------ | ------------------------- |
| frontend-web        | ✅     | React 19 SPA básica       |
| gateway             | ✅     | Ocelot API Gateway        |
| authservice         | ✅     | Login/Register/JWT        |
| userservice         | ✅     | CRUD usuarios básico      |
| roleservice         | ✅     | Roles y permisos          |
| vehiclessaleservice | ✅     | CRUD vehículos + catálogo |
| mediaservice        | ✅     | Upload imágenes S3        |
| notificationservice | ✅     | Email/SMS básico          |
| billingservice      | ✅     | Stripe básico             |
| errorservice        | ✅     | Logging errores           |

### ❌ Falta para MVP Marketplace

| Feature                            | Prioridad  | Sprint Target |
| ---------------------------------- | ---------- | ------------- |
| Búsqueda avanzada con filtros      | 🔴 CRÍTICO | Sprint 1      |
| Favoritos y guardados              | 🔴 CRÍTICO | Sprint 1      |
| Plan Early Bird + Onboarding       | 🔴 CRÍTICO | Sprint 1      |
| MaintenanceService                 | 🔴 CRÍTICO | Sprint 1      |
| Contactar vendedor                 | 🔴 CRÍTICO | Sprint 2      |
| Comparador de vehículos            | 🟡 ALTO    | Sprint 2      |
| Alertas de precio                  | 🟡 ALTO    | Sprint 2      |
| Publicar vehículos (wizard)        | 🔴 CRÍTICO | Sprint 3      |
| Sistema de pagos (post Early Bird) | 🔴 CRÍTICO | Sprint 4      |
| Panel de dealer                    | 🟡 ALTO    | Sprint 5-6    |

---

## 🎯 FASES DEL PROYECTO

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          ROADMAP GENERAL                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  FASE 1: MVP MARKETPLACE (Sprints 1-4)                      ████████░░░░░░  │
│  └── Compradores pueden buscar, ver y contactar vendedores                  │
│  └── Vendedores individuales pueden publicar vehículos                      │
│  └── Plan Early Bird: 3 meses GRATIS para todos                             │
│  └── MaintenanceService para operaciones                                    │
│                                                                              │
│  FASE 2: DEALERS BÁSICO (Sprints 5-8)                       ░░░░░░████░░░░  │
│  └── Cuentas de dealer con suscripción mensual                              │
│  └── Panel de dealer con inventario                                         │
│  └── Estadísticas básicas de listings                                       │
│                                                                              │
│  FASE 3: DATA & ANALYTICS (Sprints 9-12)                    ░░░░░░░░░░████  │
│  └── Event tracking completo                                                 │
│  └── Lead scoring para dealers                                              │
│  └── Dashboard de métricas                                                  │
│                                                                              │
│  FASE 4: IA & DIFERENCIACIÓN (Sprints 13-18)                ░░░░░░░░░░░░██  │
│  └── Chatbot con calificación de leads                                      │
│  └── Recomendaciones personalizadas                                         │
│  └── Reviews estilo Amazon                                                  │
│  └── Pricing inteligente                                                    │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

# 📅 FASE 1: MVP MARKETPLACE

## Sprint 1 (Semanas 1-2) - Búsqueda y Descubrimiento

**Objetivo:** Los compradores pueden encontrar vehículos fácilmente

### Backend

| Task                                            | Servicio                      | Story Points |
| ----------------------------------------------- | ----------------------------- | ------------ |
| Implementar búsqueda full-text con PostgreSQL   | VehiclesSaleService           | 5            |
| API de filtros (marca, modelo, año, precio, km) | VehiclesSaleService           | 5            |
| Endpoint de vehículos similares                 | VehiclesSaleService           | 3            |
| API de favoritos (añadir/quitar/listar)         | VehiclesSaleService           | 5            |
| Paginación y ordenamiento optimizado            | VehiclesSaleService           | 3            |
| **MaintenanceService base**                     | **MaintenanceService (5061)** | **5**        |
| **Plan Early Bird en BillingService**           | **BillingService**            | **5**        |
| **Onboarding flags en UserService**             | **UserService**               | **3**        |

### Frontend

| Task                                           | Componente           | Story Points |
| ---------------------------------------------- | -------------------- | ------------ |
| Página de búsqueda con filtros sidebar         | SearchPage           | 8            |
| Componente de filtros (marca/modelo cascading) | FilterSidebar        | 5            |
| Grid de resultados con lazy loading            | VehicleGrid          | 5            |
| Detalle de vehículo mejorado                   | VehicleDetailPage    | 5            |
| Botón y lista de favoritos                     | FavoritesFeature     | 3            |
| Carrusel de fotos con zoom                     | PhotoGallery         | 3            |
| **Página de mantenimiento**                    | **MaintenancePage**  | **3**        |
| **Banner "3 meses gratis" + Countdown**        | **EarlyBirdBanner**  | **3**        |
| **Onboarding wizard (comprador/vendedor)**     | **OnboardingWizard** | **8**        |
| **Badge "Miembro Fundador"**                   | **FounderBadge**     | **2**        |

### Entregables Sprint 1

```
✅ Usuario puede buscar vehículos por texto
✅ Usuario puede filtrar por marca, modelo, año, precio, km
✅ Usuario puede ordenar resultados (precio, fecha, km)
✅ Usuario puede guardar vehículos en favoritos
✅ Usuario puede ver galería de fotos completa
🆕 MaintenanceService funcionando (admin puede activar modo mantenimiento)
🆕 Plan Early Bird activo (todos publican gratis 3 meses)
🆕 Onboarding guiado para nuevos usuarios
🆕 Badge "Miembro Fundador" para Early Birds
```

**Story Points Total:** 71  
**Velocidad esperada:** 60-75 SP (sprint de lanzamiento, más esfuerzo)

---

## Sprint 2 (Semanas 3-4) - Contacto + UX Avanzado

**Objetivo:** Compradores pueden contactar vendedores + features de engagement

### Backend

| Task                                              | Servicio                     | Story Points |
| ------------------------------------------------- | ---------------------------- | ------------ |
| ContactService: crear consulta                    | ContactService               | 5            |
| ContactService: listar consultas (vendedor)       | ContactService               | 3            |
| ContactService: responder consulta                | ContactService               | 3            |
| NotificationService: email de nueva consulta      | NotificationService          | 3            |
| NotificationService: email de respuesta           | NotificationService          | 3            |
| UserService: perfil público de vendedor           | UserService                  | 5            |
| **ComparisonService: comparar hasta 3 vehículos** | **ComparisonService (5066)** | **5**        |
| **AlertService: alertas de precio/búsqueda**      | **AlertService (5067)**      | **5**        |

### Frontend

| Task                                     | Componente            | Story Points |
| ---------------------------------------- | --------------------- | ------------ |
| Modal de contactar vendedor              | ContactModal          | 5            |
| Formulario con validación                | ContactForm           | 3            |
| Página de mis consultas (comprador)      | MyInquiriesPage       | 5            |
| Página de consultas recibidas (vendedor) | ReceivedInquiriesPage | 5            |
| Perfil público del vendedor              | SellerProfilePage     | 5            |
| Chat/mensajería básica                   | MessageThread         | 8            |
| **Comparador de vehículos (hasta 3)**    | **VehicleComparator** | **8**        |
| **Crear/gestionar alertas de precio**    | **PriceAlerts**       | **5**        |

### Entregables Sprint 2

```
✅ Comprador puede enviar consulta sobre vehículo
✅ Vendedor recibe email de nueva consulta
✅ Vendedor puede responder consulta
✅ Comprador recibe email de respuesta
✅ Ambos pueden ver historial de mensajes
✅ Comprador puede ver perfil del vendedor
```

**Story Points Total:** 53  
**Velocidad esperada:** 45-55 SP

---

## Sprint 3 (Semanas 5-6) - Publicar Vehículos

**Objetivo:** Vendedores individuales pueden publicar vehículos

### Backend

| Task                             | Servicio            | Story Points |
| -------------------------------- | ------------------- | ------------ |
| API de publicación multi-step    | VehiclesSaleService | 5            |
| Validación de datos del vehículo | VehiclesSaleService | 3            |
| Upload múltiple de imágenes      | MediaService        | 5            |
| Ordenamiento de imágenes         | MediaService        | 3            |
| Draft/borrador de publicación    | VehiclesSaleService | 3            |
| Previsualización de listing      | VehiclesSaleService | 2            |

### Frontend

| Task                                     | Componente     | Story Points |
| ---------------------------------------- | -------------- | ------------ |
| Wizard de publicación (5 pasos)          | PublishWizard  | 13           |
| Step 1: Datos básicos (marca/modelo/año) | BasicInfoStep  | 5            |
| Step 2: Características y detalles       | FeaturesStep   | 5            |
| Step 3: Upload y ordenar fotos           | PhotosStep     | 8            |
| Step 4: Precio y ubicación               | PricingStep    | 3            |
| Step 5: Revisión y publicar              | ReviewStep     | 5            |
| Mis publicaciones (vendedor)             | MyListingsPage | 5            |

### Entregables Sprint 3

```
✅ Vendedor puede crear publicación paso a paso
✅ Vendedor puede subir hasta 20 fotos
✅ Vendedor puede ordenar fotos (drag & drop)
✅ Vendedor puede guardar borrador
✅ Vendedor puede previsualizar antes de publicar
✅ Vendedor puede ver sus publicaciones activas
✅ Vendedor puede editar/pausar/eliminar publicación
```

**Story Points Total:** 60  
**Velocidad esperada:** 50-60 SP

---

## Sprint 4 (Semanas 7-8) - Pagos y Monetización

**Objetivo:** Sistema de cobro por publicación funcional (Stripe + Azul)

### Backend

| Task                                    | Servicio            | Story Points |
| --------------------------------------- | ------------------- | ------------ |
| BillingService: checkout de listing     | BillingService      | 8            |
| Integración Stripe Checkout             | BillingService      | 5            |
| Webhooks de Stripe (payment_intent)     | BillingService      | 5            |
| **Integración Azul (Banco Popular RD)** | **BillingService**  | **8**        |
| **Webhooks de Azul**                    | **BillingService**  | **5**        |
| **PaymentGatewayFactory (Stripe/Azul)** | **BillingService**  | **5**        |
| Activar listing post-pago               | VehiclesSaleService | 3            |
| Historial de pagos del usuario          | BillingService      | 3            |
| Facturas/recibos automáticos            | BillingService      | 5            |

### Frontend

| Task                                            | Componente                | Story Points |
| ----------------------------------------------- | ------------------------- | ------------ |
| Página de pricing ($29/listing)                 | PricingPage               | 5            |
| **Selector de método de pago (Stripe/Azul)**    | **PaymentMethodSelector** | **5**        |
| Checkout embebido de Stripe                     | StripeCheckout            | 5            |
| **Checkout de Azul (formulario tarjeta local)** | **AzulCheckout**          | **8**        |
| Página de éxito post-pago                       | PaymentSuccessPage        | 3            |
| Historial de pagos                              | PaymentHistoryPage        | 5            |
| Banner de listing pendiente de pago             | PendingPaymentBanner      | 2            |

### Integración Azul (Banco Popular)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    PASARELAS DE PAGO OKLA                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  💳 SELECTOR DE MÉTODO DE PAGO                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  ¿Cómo deseas pagar?                                                │   │
│  │                                                                      │   │
│  │  ○ 🏦 Azul (Banco Popular) - Tarjetas dominicanas                   │   │
│  │     Visa, Mastercard, American Express                              │   │
│  │     ✅ Sin comisión internacional                                    │   │
│  │                                                                      │   │
│  │  ○ 💳 Stripe - Tarjetas internacionales                             │   │
│  │     Visa, Mastercard, American Express                              │   │
│  │     Ideal para tarjetas de USA/Europa                               │   │
│  │                                                                      │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  BENEFICIOS DE AZUL:                                                        │
│  ├── ✅ Comisiones más bajas para tarjetas locales (2.5% vs 3.5%)         │
│  ├── ✅ Confianza del usuario dominicano                                  │
│  ├── ✅ Soporte en español 24/7                                           │
│  ├── ✅ Acepta todas las tarjetas de bancos RD                            │
│  └── ✅ Depósitos en cuenta local en 24-48 horas                          │
│                                                                             │
│  BENEFICIOS DE STRIPE:                                                      │
│  ├── ✅ Acepta tarjetas internacionales                                   │
│  ├── ✅ Mejor para dominicanos en el exterior                             │
│  ├── ✅ Apple Pay, Google Pay                                             │
│  └── ✅ Mejor detección de fraude                                         │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Arquitectura de Pagos Multi-Gateway

```csharp
// PaymentGatewayFactory - Patrón Strategy
public interface IPaymentGateway
{
    Task<PaymentResult> ProcessPayment(PaymentRequest request);
    Task<PaymentResult> ProcessSubscription(SubscriptionRequest request);
    Task<RefundResult> ProcessRefund(RefundRequest request);
    Task HandleWebhook(string payload, string signature);
}

public class StripeGateway : IPaymentGateway { }
public class AzulGateway : IPaymentGateway { }

public class PaymentGatewayFactory
{
    public IPaymentGateway GetGateway(PaymentMethod method)
    {
        return method switch
        {
            PaymentMethod.Stripe => new StripeGateway(_stripeConfig),
            PaymentMethod.Azul => new AzulGateway(_azulConfig),
            _ => throw new NotSupportedException()
        };
    }
}
```

### Entregables Sprint 4

```
✅ Vendedor ve precio antes de publicar ($29)
✅ Vendedor puede elegir: Azul (local) o Stripe (internacional)
✅ Vendedor puede pagar con tarjeta dominicana (Azul)
✅ Vendedor puede pagar con tarjeta internacional (Stripe)
✅ Listing se activa automáticamente post-pago
✅ Vendedor recibe factura por email
✅ Vendedor puede ver historial de pagos
✅ Sistema maneja pagos fallidos correctamente
✅ Webhooks de ambas pasarelas funcionando
```

**Story Points Total:** 72  
**Velocidad esperada:** 60-75 SP (sprint más largo por integración Azul)

---

## 🎉 MILESTONE: MVP MARKETPLACE COMPLETO

**Fecha estimada:** Semana 8 (2 meses desde inicio)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          MVP MARKETPLACE ✅                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  👤 COMPRADORES pueden:                                                      │
│  ├── ✅ Buscar vehículos con filtros avanzados                              │
│  ├── ✅ Ver detalle con galería de fotos                                    │
│  ├── ✅ Guardar favoritos                                                   │
│  ├── ✅ Contactar vendedores                                                │
│  └── ✅ Ver perfil público del vendedor                                     │
│                                                                              │
│  🚗 VENDEDORES INDIVIDUALES pueden:                                          │
│  ├── ✅ Publicar vehículos (wizard 5 pasos)                                 │
│  ├── ✅ Subir hasta 20 fotos                                                │
│  ├── ✅ Pagar por publicación ($29)                                         │
│  ├── ✅ Recibir consultas por email                                         │
│  ├── ✅ Responder a compradores                                             │
│  └── ✅ Gestionar sus publicaciones                                         │
│                                                                              │
│  💰 MONETIZACIÓN:                                                            │
│  └── ✅ $29 por publicación (Stripe)                                        │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

# 📅 FASE 2: DEALERS BÁSICO

## Sprint 5 (Semanas 9-10) - Cuentas de Dealer

**Objetivo:** Dealers pueden registrarse y suscribirse

### Backend

| Task                                       | Servicio                | Story Points |
| ------------------------------------------ | ----------------------- | ------------ |
| DealerManagementService: CRUD dealers      | Nuevo servicio          | 8            |
| Modelo de dealer (nombre, RNC, sucursales) | DealerManagementService | 5            |
| Verificación de dealer (manual/docs)       | DealerManagementService | 5            |
| BillingService: suscripciones Stripe       | BillingService          | 8            |
| 3 planes: Starter/Pro/Enterprise           | BillingService          | 5            |
| Webhooks de suscripción                    | BillingService          | 5            |

### Frontend

| Task                             | Componente             | Story Points |
| -------------------------------- | ---------------------- | ------------ |
| Landing page para dealers        | DealerLandingPage      | 8            |
| Página de planes y pricing       | DealerPricingPage      | 5            |
| Registro de dealer (formulario)  | DealerRegistrationForm | 5            |
| Upload de documentos (RNC, etc.) | DocumentUpload         | 3            |
| Checkout de suscripción          | SubscriptionCheckout   | 5            |
| Dashboard de dealer (básico)     | DealerDashboard        | 8            |

### Entregables Sprint 5

```
✅ Dealer puede registrarse con datos de empresa
✅ Dealer puede subir documentos de verificación
✅ Admin puede aprobar/rechazar dealers
✅ Dealer puede ver planes (Starter $49/Pro $129/Enterprise $299)
✅ Dealer puede suscribirse mensualmente
✅ Dealer tiene acceso a dashboard básico
```

**Story Points Total:** 70  
**Velocidad esperada:** 55-65 SP (sprint más pesado)

---

## Sprint 6 (Semanas 11-12) - Inventario de Dealer

**Objetivo:** Dealers pueden gestionar su inventario

### Backend

| Task                                   | Servicio                   | Story Points |
| -------------------------------------- | -------------------------- | ------------ |
| InventoryManagementService base        | Nuevo servicio             | 8            |
| Bulk upload (CSV/Excel)                | InventoryManagementService | 8            |
| Edición en batch                       | InventoryManagementService | 5            |
| Sincronización con VehiclesSaleService | InventoryManagementService | 5            |
| Límites por plan (15/50/ilimitado)     | InventoryManagementService | 3            |

### Frontend

| Task                                        | Componente      | Story Points |
| ------------------------------------------- | --------------- | ------------ |
| Tabla de inventario con filtros             | InventoryTable  | 8            |
| Acciones en batch (activar/pausar/eliminar) | BatchActions    | 5            |
| Import CSV/Excel                            | BulkImportModal | 8            |
| Export de inventario                        | ExportInventory | 3            |
| Vista de límite de listings                 | LimitIndicator  | 2            |
| Quick-edit inline                           | InlineEdit      | 5            |

### Entregables Sprint 6

```
✅ Dealer puede ver tabla de todo su inventario
✅ Dealer puede importar vehículos desde CSV/Excel
✅ Dealer puede editar múltiples vehículos a la vez
✅ Dealer puede activar/pausar/eliminar en batch
✅ Dealer puede exportar inventario
✅ Sistema respeta límites según plan
```

**Story Points Total:** 60  
**Velocidad esperada:** 50-60 SP

---

## Sprint 7 (Semanas 13-14) - Perfil Público de Dealer

**Objetivo:** Dealers tienen presencia profesional en el marketplace

### Backend

| Task                                    | Servicio                | Story Points |
| --------------------------------------- | ----------------------- | ------------ |
| DealerManagementService: perfil público | DealerManagementService | 5            |
| Sucursales con ubicación/horario        | DealerManagementService | 5            |
| Galería de fotos del dealer             | MediaService            | 3            |
| SEO metadata para dealers               | VehiclesSaleService     | 3            |
| Verificación "Trusted Dealer" badge     | DealerManagementService | 3            |

### Frontend

| Task                                       | Componente         | Story Points |
| ------------------------------------------ | ------------------ | ------------ |
| Página pública del dealer                  | DealerPublicPage   | 8            |
| Header con logo y banner                   | DealerHeader       | 3            |
| Grid de vehículos del dealer               | DealerVehiclesGrid | 5            |
| Mapa con sucursales                        | DealerLocationsMap | 5            |
| Horarios de atención                       | BusinessHours      | 2            |
| Botones de contacto (tel, WhatsApp, email) | ContactButtons     | 3            |
| Editor de perfil (dealer dashboard)        | ProfileEditor      | 5            |

### Entregables Sprint 7

```
✅ Dealer tiene página pública profesional
✅ Página muestra logo, banner, descripción
✅ Compradores ven todos los vehículos del dealer
✅ Mapa muestra ubicación de sucursales
✅ Dealers verificados tienen badge "Trusted"
✅ Dealer puede editar su perfil desde dashboard
```

**Story Points Total:** 50  
**Velocidad esperada:** 45-55 SP

---

## Sprint 8 (Semanas 15-16) - Estadísticas Básicas para Dealers

**Objetivo:** Dealers ven métricas de su performance

### Backend

| Task                            | Servicio                | Story Points |
| ------------------------------- | ----------------------- | ------------ |
| ListingAnalyticsService base    | Nuevo servicio          | 8            |
| Tracking de vistas por vehículo | ListingAnalyticsService | 5            |
| Tracking de contactos/leads     | ListingAnalyticsService | 5            |
| Agregaciones diarias/semanales  | ListingAnalyticsService | 5            |
| API de métricas para dashboard  | ListingAnalyticsService | 5            |

### Frontend

| Task                             | Componente        | Story Points |
| -------------------------------- | ----------------- | ------------ |
| Dashboard con KPIs principales   | MetricsDashboard  | 8            |
| Gráfico de vistas en el tiempo   | ViewsChart        | 5            |
| Top 5 vehículos más vistos       | TopVehiclesWidget | 3            |
| Indicadores de contactos/leads   | LeadsWidget       | 3            |
| Comparación con período anterior | PeriodComparison  | 3            |
| Estadísticas por vehículo        | VehicleStatsRow   | 3            |

### Entregables Sprint 8

```
✅ Dealer ve total de vistas del mes
✅ Dealer ve total de contactos/leads
✅ Dealer ve gráfico de tendencias
✅ Dealer ve cuáles vehículos tienen más interés
✅ Dealer puede comparar con mes anterior
✅ Cada vehículo muestra sus estadísticas individuales
```

**Story Points Total:** 53  
**Velocidad esperada:** 45-55 SP

---

## 🎉 MILESTONE: DEALERS BÁSICO COMPLETO

**Fecha estimada:** Semana 16 (4 meses desde inicio)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        DEALERS BÁSICO ✅                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  🏢 DEALERS pueden:                                                          │
│  ├── ✅ Registrarse y verificarse                                           │
│  ├── ✅ Suscribirse a plan mensual ($49/$129/$299)                          │
│  ├── ✅ Gestionar inventario completo                                       │
│  ├── ✅ Importar vehículos desde CSV/Excel                                  │
│  ├── ✅ Editar en batch                                                     │
│  ├── ✅ Tener página pública profesional                                    │
│  ├── ✅ Ver estadísticas de vistas y contactos                              │
│  └── ✅ Badge "Trusted Dealer"                                              │
│                                                                              │
│  💰 MONETIZACIÓN:                                                            │
│  ├── ✅ $29 por listing (vendedores individuales)                           │
│  └── ✅ $49-$299/mes suscripción dealers                                    │
│                                                                              │
│  📊 NUEVOS SERVICIOS:                                                        │
│  ├── ✅ DealerManagementService (5039)                                      │
│  ├── ✅ InventoryManagementService (5040)                                   │
│  └── ✅ ListingAnalyticsService (5058)                                      │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

# 📅 FASE 3: DATA & ANALYTICS

## Sprint 9 (Semanas 17-18) - Event Tracking

**Objetivo:** Capturar todas las acciones de usuarios

### Backend

| Task                         | Servicio             | Story Points |
| ---------------------------- | -------------------- | ------------ |
| EventTrackingService base    | Nuevo servicio       | 8            |
| Kafka/RabbitMQ consumer      | EventTrackingService | 5            |
| ClickHouse para eventos      | EventTrackingService | 8            |
| API de ingesta de eventos    | EventTrackingService | 5            |
| Retención y cleanup de datos | EventTrackingService | 3            |

### Frontend

| Task                         | Componente        | Story Points |
| ---------------------------- | ----------------- | ------------ |
| SDK de tracking (JS library) | okla-analytics.js | 8            |
| Auto-track de page views     | AutoTrack         | 3            |
| Track de clicks importantes  | ClickTrack        | 3            |
| Track de búsquedas y filtros | SearchTrack       | 3            |
| Track de tiempo en página    | TimeOnPageTrack   | 3            |

### Entregables Sprint 9

```
✅ Sistema captura todas las page views
✅ Sistema captura búsquedas realizadas
✅ Sistema captura filtros aplicados
✅ Sistema captura tiempo en cada vehículo
✅ Sistema captura favoritos y contactos
✅ Eventos almacenados en ClickHouse
```

**Story Points Total:** 49  
**Velocidad esperada:** 45-55 SP

---

## Sprint 10 (Semanas 19-20) - User Behavior & Features

**Objetivo:** Entender comportamiento de usuarios

### Backend

| Task                              | Servicio            | Story Points |
| --------------------------------- | ------------------- | ------------ |
| UserBehaviorService base          | Nuevo servicio      | 8            |
| Perfil de preferencias inferidas  | UserBehaviorService | 5            |
| Historial de acciones por usuario | UserBehaviorService | 5            |
| FeatureStoreService base          | Nuevo servicio      | 8            |
| Features de usuarios              | FeatureStoreService | 5            |
| Features de vehículos             | FeatureStoreService | 5            |

### Tareas de Data

| Task                      | Servicio            | Story Points |
| ------------------------- | ------------------- | ------------ |
| ETL de eventos a features | DataPipelineService | 8            |
| Agregaciones diarias      | DataPipelineService | 5            |
| Segmentación de usuarios  | UserBehaviorService | 5            |

### Entregables Sprint 10

```
✅ Sistema infiere preferencias (SUV, Toyota, <$30k)
✅ Sistema segmenta usuarios (comprador serio, browser, etc.)
✅ Feature store con features de usuarios
✅ Feature store con features de vehículos
✅ Pipeline de ETL funcionando
```

**Story Points Total:** 54  
**Velocidad esperada:** 45-55 SP

---

## Sprint 11 (Semanas 21-22) - Lead Scoring

**Objetivo:** Identificar leads HOT para dealers

### Backend

| Task                                 | Servicio            | Story Points |
| ------------------------------------ | ------------------- | ------------ |
| LeadScoringService base              | Nuevo servicio      | 8            |
| Modelo de scoring (reglas iniciales) | LeadScoringService  | 8            |
| Integración con eventos              | LeadScoringService  | 5            |
| API de leads por dealer              | LeadScoringService  | 5            |
| Notificaciones de leads HOT          | NotificationService | 3            |

### Frontend

| Task                                | Componente       | Story Points |
| ----------------------------------- | ---------------- | ------------ |
| Widget de leads en dashboard dealer | LeadsWidget      | 5            |
| Lista de leads con score            | LeadsList        | 5            |
| Detalle de lead (historial)         | LeadDetail       | 5            |
| Indicador visual HOT/WARM/COLD      | LeadScoreBadge   | 2            |
| Notificación push de lead HOT       | PushNotification | 3            |

### Entregables Sprint 11

```
✅ Sistema calcula score de cada lead (0-100)
✅ Leads clasificados como HOT/WARM/COLD
✅ Dealers ven lista de leads ordenada por score
✅ Dealers reciben notificación de leads HOT
✅ Dealers ven historial de acciones del lead
```

**Story Points Total:** 49  
**Velocidad esperada:** 45-55 SP

---

## Sprint 12 (Semanas 23-24) - Dashboard Avanzado

**Objetivo:** Analytics completos para dealers

### Backend

| Task                               | Servicio               | Story Points |
| ---------------------------------- | ---------------------- | ------------ |
| DealerAnalyticsService base        | Nuevo servicio         | 8            |
| Métricas de conversión             | DealerAnalyticsService | 5            |
| Comparación con competencia (anon) | DealerAnalyticsService | 5            |
| Reportes exportables               | DealerAnalyticsService | 5            |
| Insights automáticos               | DealerAnalyticsService | 5            |

### Frontend

| Task                        | Componente        | Story Points |
| --------------------------- | ----------------- | ------------ |
| Dashboard rediseñado        | AdvancedDashboard | 8            |
| Funnel de conversión visual | ConversionFunnel  | 5            |
| Benchmark vs mercado        | MarketBenchmark   | 5            |
| Insights/recomendaciones    | InsightsCard      | 5            |
| Export PDF/Excel            | ReportExport      | 3            |

### Entregables Sprint 12

```
✅ Dashboard con métricas avanzadas
✅ Funnel: Vistas → Contactos → Test Drives → Ventas
✅ Comparación anónima con otros dealers
✅ Insights: "Tu Toyota está 10% arriba del mercado"
✅ Export de reportes en PDF/Excel
```

**Story Points Total:** 54  
**Velocidad esperada:** 45-55 SP

---

## 🎉 MILESTONE: DATA & ANALYTICS COMPLETO

**Fecha estimada:** Semana 24 (6 meses desde inicio)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     DATA & ANALYTICS ✅                                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  📊 TRACKING:                                                                │
│  ├── ✅ Todas las acciones de usuarios capturadas                           │
│  ├── ✅ Perfiles de comportamiento inferidos                                │
│  └── ✅ Feature store para ML                                               │
│                                                                              │
│  🔥 LEAD SCORING:                                                            │
│  ├── ✅ Score 0-100 para cada lead                                          │
│  ├── ✅ Clasificación HOT/WARM/COLD                                         │
│  └── ✅ Notificaciones de leads HOT                                         │
│                                                                              │
│  📈 ANALYTICS DEALERS:                                                       │
│  ├── ✅ Dashboard con todas las métricas                                    │
│  ├── ✅ Funnel de conversión                                                │
│  ├── ✅ Benchmark vs mercado                                                │
│  └── ✅ Insights automáticos                                                │
│                                                                              │
│  🆕 NUEVOS SERVICIOS:                                                        │
│  ├── ✅ EventTrackingService (5050)                                         │
│  ├── ✅ DataPipelineService (5051)                                          │
│  ├── ✅ UserBehaviorService (5052)                                          │
│  ├── ✅ FeatureStoreService (5053)                                          │
│  ├── ✅ LeadScoringService (5055)                                           │
│  └── ✅ DealerAnalyticsService (5041)                                       │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

# 📅 FASE 4: IA & DIFERENCIACIÓN

## Sprint 13 (Semanas 25-26) - Recomendaciones

**Objetivo:** "Vehículos para ti" personalizados

### Backend

| Task                                    | Servicio              | Story Points |
| --------------------------------------- | --------------------- | ------------ |
| RecommendationService base              | Nuevo servicio        | 8            |
| Modelo de recomendación (collaborative) | RecommendationService | 8            |
| "Vehículos similares"                   | RecommendationService | 5            |
| "Usuarios también vieron"               | RecommendationService | 5            |
| Cache de recomendaciones (Redis)        | RecommendationService | 3            |

### Frontend

| Task                             | Componente          | Story Points |
| -------------------------------- | ------------------- | ------------ |
| Sección "Para ti" en homepage    | ForYouSection       | 5            |
| Carrusel de similares en detalle | SimilarVehicles     | 5            |
| "También vieron" en detalle      | AlsoViewed          | 3            |
| Email de recomendaciones         | RecommendationEmail | 5            |

### Entregables Sprint 13

```
✅ Homepage muestra vehículos personalizados
✅ Detalle muestra vehículos similares
✅ Detalle muestra "usuarios también vieron"
✅ Email semanal con recomendaciones
```

**Story Points Total:** 47  
**Velocidad esperada:** 45-55 SP

---

## Sprint 14 (Semanas 27-28) - Reviews Básico

**Objetivo:** Sistema de reviews estilo Amazon

### Backend

| Task                           | Servicio       | Story Points |
| ------------------------------ | -------------- | ------------ |
| ReviewService base             | Nuevo servicio | 8            |
| CRUD de reviews                | ReviewService  | 5            |
| Rating summary por vendedor    | ReviewService  | 5            |
| Validación "compra verificada" | ReviewService  | 5            |
| Moderación básica              | ReviewService  | 3            |

### Frontend

| Task                                  | Componente         | Story Points |
| ------------------------------------- | ------------------ | ------------ |
| Sección de reviews en perfil vendedor | ReviewsSection     | 8            |
| Formulario de review                  | ReviewForm         | 5            |
| Rating con estrellas                  | StarRating         | 3            |
| Distribución de ratings               | RatingDistribution | 3            |
| Badge "Compra verificada"             | VerifiedBadge      | 2            |

### Entregables Sprint 14

```
✅ Compradores pueden dejar reviews
✅ Rating 1-5 estrellas + texto
✅ Badge de compra verificada
✅ Vendedor ve rating promedio
✅ Distribución visual de ratings
```

**Story Points Total:** 47  
**Velocidad esperada:** 45-55 SP

---

## Sprint 15 (Semanas 29-30) - Reviews Avanzado

**Objetivo:** Reviews completo con respuestas y votos

### Backend

| Task                             | Servicio      | Story Points |
| -------------------------------- | ------------- | ------------ |
| Respuestas de vendedor a reviews | ReviewService | 5            |
| Votos de utilidad                | ReviewService | 3            |
| Sistema de badges                | ReviewService | 5            |
| Solicitud automática de review   | ReviewService | 5            |
| Anti-spam y fraude               | ReviewService | 5            |

### Frontend

| Task                          | Componente         | Story Points |
| ----------------------------- | ------------------ | ------------ |
| Respuesta del vendedor UI     | SellerResponse     | 3            |
| Botón "¿Te resultó útil?"     | HelpfulVote        | 3            |
| Badges en perfil              | BadgeDisplay       | 3            |
| Modal de solicitud de review  | ReviewRequestModal | 5            |
| Filtrar reviews por estrellas | ReviewFilters      | 3            |

### Entregables Sprint 15

```
✅ Vendedor puede responder reviews
✅ Usuarios pueden votar reviews útiles
✅ Badges: "Top Rated", "Trusted Dealer"
✅ Solicitud automática 7 días después de compra
✅ Sistema anti-fraude de reviews
```

**Story Points Total:** 40  
**Velocidad esperada:** 35-45 SP

---

## Sprint 16 (Semanas 31-32) - Chatbot MVP

**Objetivo:** Chatbot básico con OpenAI

### Backend

| Task                           | Servicio       | Story Points |
| ------------------------------ | -------------- | ------------ |
| ChatbotService base            | Nuevo servicio | 8            |
| Integración OpenAI GPT-4o-mini | ChatbotService | 8            |
| SignalR para real-time         | ChatbotService | 5            |
| Contexto del vehículo en chat  | ChatbotService | 5            |
| Historial de conversaciones    | ChatbotService | 3            |

### Frontend

| Task                      | Componente      | Story Points |
| ------------------------- | --------------- | ------------ |
| Widget de chat flotante   | ChatWidget      | 8            |
| Interfaz de conversación  | ChatInterface   | 5            |
| Indicador de typing       | TypingIndicator | 2            |
| Botón de cerrar/minimizar | ChatControls    | 2            |

### Entregables Sprint 16

```
✅ Widget de chat en páginas de vehículos
✅ Chatbot responde preguntas del vehículo
✅ Conversación en tiempo real (SignalR)
✅ Contexto del vehículo actual
✅ Historial de conversación
```

**Story Points Total:** 46  
**Velocidad esperada:** 40-50 SP

---

## Sprint 17 (Semanas 33-34) - Chatbot con Lead Scoring

**Objetivo:** Chatbot califica leads y transfiere a WhatsApp

### Backend

| Task                               | Servicio       | Story Points |
| ---------------------------------- | -------------- | ------------ |
| RAG con Pinecone                   | ChatbotService | 8            |
| Análisis de intención de compra    | ChatbotService | 8            |
| Integración con LeadScoringService | ChatbotService | 5            |
| Integración WhatsApp (Twilio)      | ChatbotService | 8            |
| Handoff a vendedor                 | ChatbotService | 5            |

### Frontend

| Task                              | Componente         | Story Points |
| --------------------------------- | ------------------ | ------------ |
| Botón "Hablar con vendedor"       | TransferButton     | 3            |
| Transición a WhatsApp             | WhatsAppHandoff    | 5            |
| Indicador de lead score (interno) | LeadScoreIndicator | 2            |

### Entregables Sprint 17

```
✅ Chatbot responde con info específica del vehículo (RAG)
✅ Sistema detecta intención de compra
✅ Lead clasificado como HOT/WARM/COLD
✅ Lead HOT transferido a WhatsApp automáticamente
✅ Vendedor recibe contexto de la conversación
```

**Story Points Total:** 44  
**Velocidad esperada:** 40-50 SP

---

## Sprint 18 (Semanas 35-36) - Pricing Inteligente

**Objetivo:** IA sugiere precio óptimo

### Backend

| Task                            | Servicio                   | Story Points |
| ------------------------------- | -------------------------- | ------------ |
| VehicleIntelligenceService base | Nuevo servicio             | 8            |
| Modelo de pricing (XGBoost)     | VehicleIntelligenceService | 8            |
| Predicción de demanda           | VehicleIntelligenceService | 8            |
| Tiempo estimado de venta        | VehicleIntelligenceService | 5            |
| API de sugerencias              | VehicleIntelligenceService | 3            |

### Frontend

| Task                                | Componente       | Story Points |
| ----------------------------------- | ---------------- | ------------ |
| Widget de precio sugerido (publish) | PriceSuggestion  | 5            |
| Indicador vs mercado                | MarketComparison | 3            |
| Tips para vender más rápido         | SellingTips      | 3            |
| Predicción de tiempo de venta       | TimeToSell       | 3            |

### Entregables Sprint 18

```
✅ Vendedor ve precio sugerido al publicar
✅ Indicador: "Tu precio está 10% arriba del mercado"
✅ Predicción: "Este vehículo se venderá en ~18 días"
✅ Tips para mejorar el listing
✅ Dealers ven demanda por categoría
```

**Story Points Total:** 46  
**Velocidad esperada:** 40-50 SP

---

## 🎉 MILESTONE: MARKETPLACE 100% COMPLETO

**Fecha estimada:** Semana 36 (9 meses desde inicio)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                   🚀 OKLA MARKETPLACE 100% ✅                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  👤 COMPRADORES:                                                             │
│  ├── ✅ Búsqueda avanzada con filtros                                       │
│  ├── ✅ Favoritos y guardados                                               │
│  ├── ✅ Contactar vendedores                                                │
│  ├── ✅ Recomendaciones personalizadas                                      │
│  ├── ✅ Reviews de vendedores                                               │
│  └── ✅ Chatbot 24/7 para preguntas                                         │
│                                                                              │
│  🚗 VENDEDORES INDIVIDUALES:                                                 │
│  ├── ✅ Publicar con wizard de 5 pasos                                      │
│  ├── ✅ Pago por listing ($29)                                              │
│  ├── ✅ Precio sugerido por IA                                              │
│  ├── ✅ Estadísticas de vistas                                              │
│  ├── ✅ Reviews y reputación                                                │
│  └── ✅ Leads pre-calificados                                               │
│                                                                              │
│  🏢 DEALERS:                                                                 │
│  ├── ✅ Suscripción mensual ($49/$129/$299)                                 │
│  ├── ✅ Gestión de inventario completo                                      │
│  ├── ✅ Import/export masivo                                                │
│  ├── ✅ Página pública profesional                                          │
│  ├── ✅ Dashboard con todas las métricas                                    │
│  ├── ✅ Lead scoring (HOT/WARM/COLD)                                        │
│  ├── ✅ Chatbot con transferencia a WhatsApp                                │
│  ├── ✅ Pricing inteligente                                                 │
│  └── ✅ Badges y reputación                                                 │
│                                                                              │
│  🤖 INTELIGENCIA ARTIFICIAL:                                                 │
│  ├── ✅ Recomendaciones personalizadas                                      │
│  ├── ✅ Lead scoring automático                                             │
│  ├── ✅ Chatbot con GPT-4                                                   │
│  ├── ✅ Pricing óptimo sugerido                                             │
│  └── ✅ Predicción de demanda                                               │
│                                                                              │
│  📊 SERVICIOS TOTALES: 17 nuevos microservicios                              │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

# 📊 RESUMEN EJECUTIVO

## Timeline Visual

```
MES 1    MES 2    MES 3    MES 4    MES 5    MES 6    MES 7    MES 8    MES 9
─────────────────────────────────────────────────────────────────────────────
S1  S2   S3  S4   S5  S6   S7  S8   S9  S10  S11 S12  S13 S14  S15 S16  S17 S18
███████████████   ███████████████   ████████████████   ████████████████████████
    FASE 1            FASE 2              FASE 3              FASE 4
   MVP MARKETPLACE   DEALERS BÁSICO    DATA & ANALYTICS    IA & DIFERENCIACIÓN

   🎯 Semana 8       🎯 Semana 16      🎯 Semana 24        🎯 Semana 36
   MVP Live!         Dealers Live!     Analytics Live!     100% Complete!
```

## Métricas por Fase

| Fase                    | Sprints | Semanas | Story Points | Servicios Nuevos                                              |
| ----------------------- | ------- | ------- | ------------ | ------------------------------------------------------------- |
| 1 - MVP Marketplace     | 1-4     | 1-8     | ~209         | 0 (mejoras a existentes)                                      |
| 2 - Dealers Básico      | 5-8     | 9-16    | ~233         | 3 (Dealer, Inventory, ListingAnalytics)                       |
| 3 - Data & Analytics    | 9-12    | 17-24   | ~206         | 6 (Event, Pipeline, Behavior, Feature, Lead, DealerAnalytics) |
| 4 - IA & Diferenciación | 13-18   | 25-36   | ~270         | 4 (Recommendation, Review, Chatbot, VehicleIntelligence)      |
| **TOTAL**               | **18**  | **36**  | **~918**     | **13**                                                        |

## Equipo Sugerido

| Rol                        | Cantidad | Notas                      |
| -------------------------- | -------- | -------------------------- |
| Backend Developer (.NET)   | 2        | Full-time, senior          |
| Frontend Developer (React) | 1-2      | Full-time                  |
| ML/Data Engineer           | 1        | Desde Sprint 9             |
| DevOps/SRE                 | 0.5      | Part-time o contratista    |
| QA                         | 1        | Part-time o desde Sprint 3 |
| Product Owner              | 1        | Part-time                  |

## Dependencias Críticas

```
Sprint 1: ─────────────────────────────────────────►
Sprint 2: ─────────────────────────────────────────►
Sprint 3: ───────────────────────► (depende de Auth funcional)
Sprint 4: ───────────────────────────────► (depende de Stripe config)
Sprint 5: ───────────────────────────────────────► (nuevo servicio)
Sprint 9: ─────────────────────────────────────────► (Kafka/ClickHouse)
Sprint 11: ──────────────────────────────► (depende de Sprint 9-10)
Sprint 16: ─────────────────────────────────────────► (OpenAI API)
Sprint 17: ──────────────────────────────► (WhatsApp Business API)
```

## Riesgos y Mitigaciones

| Riesgo                     | Probabilidad | Impacto | Mitigación                      |
| -------------------------- | ------------ | ------- | ------------------------------- |
| Integración Stripe demora  | Media        | Alto    | Empezar config en Sprint 3      |
| OpenAI API costs higher    | Media        | Medio   | Usar GPT-4o-mini, monitor costs |
| WhatsApp Business approval | Alta         | Alto    | Iniciar proceso en Sprint 12    |
| ML models underperform     | Media        | Medio   | Empezar con reglas, iterar      |
| Team velocity lower        | Media        | Alto    | Buffer de 20% en estimaciones   |

---

## 🚀 PRÓXIMOS PASOS INMEDIATOS

### Esta Semana (Prep Sprint 1)

1. ✅ Validar plan con stakeholders
2. ✅ Configurar board de Jira/Linear
3. ✅ Crear tickets de Sprint 1
4. ✅ Setup de ambiente de desarrollo
5. ✅ Revisión técnica de VehiclesSaleService

### Sprint 1 Kick-off

1. 📋 Sprint planning (4 horas)
2. 📋 Asignar tareas a developers
3. 📋 Definir criterios de aceptación
4. 📋 Daily standups
5. 📋 Sprint review + retro (Semana 2)

---

_Documento creado: Enero 8, 2026_  
_Próxima revisión: Sprint 1 Planning_  
_Autor: Equipo OKLA_
