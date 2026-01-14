````markdown
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

[contenido del Sprint 3...]

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

_Documento actualizado: Enero 9, 2026_
_Versión completa: 52 Sprints (108 semanas)_
````
