# 🤖 GitHub Copilot Instructions - OKLA (CarDealer Microservices)

Este documento proporciona contexto para GitHub Copilot sobre el proyecto OKLA (antes CarDealer).

**Última actualización:** Febrero 18, 2026

---

## 📋 RESUMEN DEL PROYECTO

**OKLA** es una plataforma de marketplace para compra y venta de vehículos en República Dominicana. Implementa arquitectura de **microservicios** con Clean Architecture, desplegada en **Digital Ocean Kubernetes (DOKS)**.

### 👥 Tipos de Cuentas

| Tipo                    | AccountType | Paga         | Objetivo                     |
| ----------------------- | ----------- | ------------ | ---------------------------- |
| **Comprador**           | Individual  | No (gratis)  | Encontrar y comprar vehículo |
| **Vendedor Individual** | Individual  | $29/listing  | Vender su vehículo personal  |
| **Dealer** ⭐           | Dealer      | $49-$299/mes | Vender inventario completo   |
| **Admin**               | Admin       | No (staff)   | Moderar plataforma           |

### 🌐 URLs de Producción

| Recurso          | URL                            |
| ---------------- | ------------------------------ |
| **Frontend**     | https://okla.com.do            |
| **API (BFF)**    | https://okla.com.do/api/*      |
| **Health Check** | https://okla.com.do/api/health |

> ⚠️ **BFF Pattern:** El Gateway NO está expuesto al internet. Todo el tráfico API
> fluye: `Browser → okla.com.do/api/* → Next.js (rewrite) → gateway:8080 (interno) → microservicios`.
> El subdominio `api.okla.com.do` ya NO existe.

### 🖥️ URLs de Desarrollo (Local)

| Recurso         | URL                    |
| --------------- | ---------------------- |
| **Frontend**    | http://localhost:3000  |
| **API Gateway** | http://localhost:18443 |

> ⚠️ **IMPORTANTE - Desarrollo Local:** El frontend SIEMPRE corre en **http://localhost:3000**.
>
> - Si el puerto 3000 está ocupado, **detener el proceso** que lo usa antes de iniciar el servidor.
> - NO usar otros puertos (3001, 3002, etc.) para pruebas del frontend.
> - Comando para liberar puerto: `lsof -ti:3000 | xargs kill -9`

### Stack Tecnológico

| Capa                   | Tecnología                           | Versión     |
| ---------------------- | ------------------------------------ | ----------- |
| **Backend**            | .NET 8.0 LTS                         | net8.0      |
| **Frontend Web**       | Next.js 16 + TypeScript + App Router | ^16.1.6     |
| **Frontend Mobile**    | Flutter + Dart                       | SDK >=3.4.0 |
| **Package Manager**    | pnpm (⚠️ NO usar npm ni yarn)        | 9+          |
| **Base de Datos**      | PostgreSQL                           | 16+         |
| **Cache**              | Redis                                | 7+          |
| **Message Broker**     | RabbitMQ                             | 3.12+       |
| **API Gateway**        | Ocelot                               | 22.0.1      |
| **Container Registry** | GitHub Container Registry (ghcr.io)  |             |
| **Kubernetes**         | Digital Ocean DOKS                   | 1.28+       |
| **CI/CD**              | GitHub Actions                       |             |

> ⚠️ **IMPORTANTE - Package Manager:** Este proyecto usa **pnpm** exclusivamente.
>
> - ✅ Usar: `pnpm install`, `pnpm add <package>`, `pnpm dev`
> - ❌ NO usar: `npm install`, `yarn add`

---

## 🚀 ESTADO DE PRODUCCIÓN (Febrero 2026)

### ✅ Servicios Desplegados en DOKS (44 servicios definidos, 14 activos)

El proyecto está **EN STAGING** en Digital Ocean Kubernetes (cluster: `okla-cluster`, namespace: `okla`).

> ⚠️ **Staging:** Cluster con 2× `s-4vcpu-8gb` nodes (~12GB allocatable, autoscale hasta 3).
> Pool: `okla-pool-upgraded`. 14 servicios activos, 30 en `replicas: 0` (sin imagen Docker o bug de startup).
> Para producción, escalar servicios críticos a 2 réplicas: `kubectl scale deployment frontend-web gateway authservice --replicas=2 -n okla`

#### Servicios Activos (replicas: 1)

| Servicio                        | Estado     | Réplicas | Puerto K8s | Imagen Docker                                                 |
| ------------------------------- | ---------- | -------- | ---------- | ------------------------------------------------------------- |
| **frontend-web**                | ✅ Running | 1        | 8080       | `ghcr.io/gregorymorenoiem/frontend-web:latest`                |
| **gateway**                     | ✅ Running | 1        | 8080       | `ghcr.io/gregorymorenoiem/gateway:latest`                     |
| **authservice**                 | ✅ Running | 1        | 8080       | `ghcr.io/gregorymorenoiem/authservice:latest`                 |
| **userservice**                 | ✅ Running | 1        | 8080       | `ghcr.io/gregorymorenoiem/userservice:latest`                 |
| **roleservice**                 | ✅ Running | 1        | 8080       | `ghcr.io/gregorymorenoiem/roleservice:latest`                 |
| **vehiclessaleservice**         | ✅ Running | 1        | 8080       | `ghcr.io/gregorymorenoiem/vehiclessaleservice:latest`         |
| **mediaservice**                | ✅ Running | 1        | 8080       | `ghcr.io/gregorymorenoiem/mediaservice:latest`                |
| **billingservice**              | ✅ Running | 1        | 8080       | `ghcr.io/gregorymorenoiem/billingservice:latest`              |
| **notificationservice**         | ✅ Running | 1        | 8080       | `ghcr.io/gregorymorenoiem/notificationservice:latest`         |
| **errorservice**                | ✅ Running | 1        | 8080       | `ghcr.io/gregorymorenoiem/errorservice:latest`                |
| **kycservice**                  | ✅ Running | 1        | 8080       | `ghcr.io/gregorymorenoiem/kycservice:latest`                  |
| **chatbotservice**              | ✅ Running | 1        | 8080       | `ghcr.io/gregorymorenoiem/chatbotservice:latest`              |
| **auditservice**                | ✅ Running | 1        | 8080       | `ghcr.io/gregorymorenoiem/auditservice:latest`                |
| **postgres**                    | ✅ Running | 1        | 5432       | In-cluster (StatefulSet)                                      |
| **redis**                       | ✅ Running | 1        | 6379       | In-cluster                                                    |
| **rabbitmq**                    | ✅ Running | 1        | 5672/15672 | In-cluster                                                    |

#### Servicios Deshabilitados (replicas: 0)

| Servicio                        | Razón                        | Imagen Docker |
| ------------------------------- | ---------------------------- | ------------- |
| **adminservice**                | 🐛 Crash al iniciar (DI bug) | ✅ Existe     |
| **contactservice**              | 🐛 Crash al iniciar          | ✅ Existe     |
| **idempotencyservice**          | ❌ Sin imagen en GHCR        | ❌ No existe  |
| **reviewservice**               | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **dealermanagementservice**     | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **dealeranalyticsservice**      | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **crmservice**                  | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **maintenanceservice**          | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **comparisonservice**           | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **alertservice**                | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **appointmentservice**          | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **marketingservice**            | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **staffservice**                | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **reportsservice**              | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **inventorymanagementservice**  | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **paymentservice**              | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **aiprocessingservice**         | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **vehicleintelligenceservice**  | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **recommendationservice**       | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **leadscoringservice**          | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **backgroundremovalservice**    | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **vehicle360processingservice** | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **cacheservice**                | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **messagebusservice**           | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **configurationservice**        | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **schedulerservice**            | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **ratelimitingservice**         | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **servicediscovery**            | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **apidocsservice**              | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **integrationservice**          | 📦 Sin imagen en GHCR        | ❌ No existe  |
| **dataprotectionservice**       | 📦 Sin imagen en GHCR        | ❌ No existe  |

**Load Balancer IP:** 146.190.199.0

### 💰 Costos Mensuales (Staging)

| Recurso                      | Detalle                | Costo/mes |
| ---------------------------- | ---------------------- | --------: |
| DOKS Cluster (control plane) | Gratis en DO           |        $0 |
| 2× Worker Nodes              | `s-4vcpu-8gb` × 2      |       $96 |
| DO Managed PostgreSQL        | `db-s-1vcpu-1gb` × 1   |       $15 |
| Load Balancer                | 1× LB (Ingress NGINX)  |       $12 |
| Block Storage                | 2× 10Gi PVCs           |        $2 |
| **TOTAL**                    |                        | **~$125** |

> ℹ️ **Upgrade Feb 2026:** Nodos actualizados de `s-2vcpu-4gb` ($24/nodo) a `s-4vcpu-8gb` ($48/nodo).
> Pool: `okla-pool-upgraded`, autoscale: 2-3 nodos. Capacidad: ~3890m CPU y ~6.4GB memoria por nodo.

### 💳 Pasarelas de Pago (PaymentService)

OKLA implementa **5 pasarelas** via Strategy + Factory + Registry pattern en `PaymentService`. Enum: `PaymentGateway` (0-4).

| #   | Pasarela                 | Tipo      | Estado           | Comisión       | Monedas       | Métodos de Pago                                    | Tokenización           |
| --- | ------------------------ | --------- | ---------------- | -------------- | ------------- | -------------------------------------------------- | ---------------------- |
| 0   | **Azul (Banco Popular)** | Banking   | ✅ **DEFAULT**   | 3.5%           | DOP, USD      | CreditCard, DebitCard, TokenizedCard               | Cybersource (incluido) |
| 1   | **CardNET**              | Banking   | ❌ Deshabilitado | 3.0%           | DOP, USD      | CreditCard, DebitCard, ACH, TokenizedCard          | Bajo solicitud         |
| 2   | **PixelPay**             | Fintech   | ✅ Habilitado    | 2.5% + US$0.15 | DOP, USD, EUR | CreditCard, DebitCard, MobilePayment, EWallet      | Native API             |
| 3   | **Fygaro**               | Agregador | ❌ Deshabilitado | 3.0%           | DOP, USD      | CreditCard, DebitCard, TokenizedCard, Subscription | Módulo suscripción     |
| 4   | **PayPal**               | Fintech   | ❌ Deshabilitado | 2.9% + US$0.30 | USD, EUR, DOP | CreditCard, DebitCard, PayPalWallet, TokenizedCard | Vault API              |

> **Configuración:** Cada provider tiene su `Settings` class y sección en `appsettings.json` (`PaymentGateway:{Provider}`).
> Default gateway configurable en `PaymentGateway:DefaultGateway`.
> Exchange rates DOP↔USD/EUR via **Banco Central (BCRD)** — refresh diario 8:30 AM RD.
> Admin puede habilitar/deshabilitar providers on-the-fly via `GatewayAvailabilityService`.

---

## 📊 MICROSERVICIOS (86 Total)

El proyecto cuenta con **86 microservicios** organizados por dominio:

### 🔐 Autenticación & Seguridad

| Servicio            | Puerto | Descripción                                       |
| ------------------- | ------ | ------------------------------------------------- |
| AuthService         | 15101  | JWT, login, registro, OAuth                       |
| RoleService         | 15102  | Roles y permisos RBAC                             |
| KYCService          | 15180  | Verificación de identidad (Liveness + Documentos) |
| IdempotencyService  | 15136  | Control de operaciones duplicadas                 |
| RateLimitingService | 15134  | Rate limiting por usuario/IP                      |

### 👥 Usuarios & Dealers

| Servicio                | Puerto | Descripción                       |
| ----------------------- | ------ | --------------------------------- |
| UserService             | 15103  | Gestión de usuarios               |
| DealerManagementService | 5039   | Perfiles y sucursales de dealers  |
| DealerAnalyticsService  | 5041   | Métricas y dashboard para dealers |
| ContactService          | 15106  | Gestión de contactos              |
| ReviewService           | 5059   | Reviews y calificaciones          |

### 🚗 Vehículos & Inventario

| Servicio                    | Puerto | Descripción                         |
| --------------------------- | ------ | ----------------------------------- |
| VehiclesSaleService         | 15104  | CRUD vehículos, catálogo, búsqueda  |
| InventoryManagementService  | 5040   | Import/export masivo, batch editing |
| VehicleIntelligenceService  | 5056   | Pricing IA, predicción de demanda   |
| Vehicle360ProcessingService | -      | Procesamiento de imágenes 360°      |
| SpyneIntegrationService     | -      | Integración con Spyne AI            |
| BackgroundRemovalService    | -      | Remoción de fondos IA               |

### 💰 Pagos & Facturación

| Servicio                  | Puerto | Descripción                                                      |
| ------------------------- | ------ | ---------------------------------------------------------------- |
| BillingService            | 15107  | Lógica de facturación                                            |
| PaymentService            | -      | Procesamiento de pagos (Azul, PixelPay, CardNET, Fygaro, PayPal) |
| InvoicingService          | -      | Generación de facturas                                           |
| BankReconciliationService | -      | Conciliación bancaria                                            |

### 📧 Comunicación

| Servicio            | Puerto | Descripción           |
| ------------------- | ------ | --------------------- |
| NotificationService | 15105  | Email, SMS, Push      |
| ChatbotService      | 5060   | Chatbot IA + WhatsApp |
| MessageBusService   | 15120  | Mensajería interna    |

### 📈 Analytics & ML

| Servicio              | Puerto | Descripción                    |
| --------------------- | ------ | ------------------------------ |
| EventTrackingService  | 5050   | Captura de eventos             |
| DataPipelineService   | 5051   | ETL y transformaciones         |
| UserBehaviorService   | 5052   | Perfiles de comportamiento     |
| FeatureStoreService   | 5053   | Features centralizados para ML |
| RecommendationService | 5054   | Recomendaciones personalizadas |
| LeadScoringService    | 5055   | Calificación de leads          |
| SearchService         | 15128  | Elasticsearch search           |

### ⚖️ Compliance & Legal (RD)

| Servicio                     | Puerto | Descripción                    |
| ---------------------------- | ------ | ------------------------------ |
| ComplianceService            | -      | Cumplimiento regulatorio       |
| ComplianceReportingService   | -      | Reportes de compliance         |
| ComplianceIntegrationService | -      | Integraciones externas         |
| TaxComplianceService         | -      | Cumplimiento fiscal DGII       |
| ConsumerProtectionService    | -      | Pro-Consumidor                 |
| AntiMoneyLaunderingService   | -      | AML/CFT                        |
| DataProtectionService        | -      | Protección de datos personales |
| ECommerceComplianceService   | -      | Ley 126-02 e-commerce          |
| RegulatoryAlertService       | -      | Alertas regulatorias           |
| LegalDocumentService         | -      | Documentos legales             |
| DigitalSignatureService      | -      | Firmas digitales               |
| ContractService              | -      | Gestión de contratos           |
| DisputeService               | -      | Resolución de disputas         |

### 🔧 Infraestructura

| Servicio             | Puerto | Descripción                 |
| -------------------- | ------ | --------------------------- |
| Gateway              | 18443  | Ocelot API Gateway          |
| ErrorService         | 15108  | Errores centralizados + DLQ |
| AuditService         | 15112  | Auditoría centralizada      |
| LoggingService       | 15118  | Logs centralizados          |
| TracingService       | 15130  | Distributed tracing         |
| HealthCheckService   | 15132  | Health checks agregados     |
| CacheService         | 15122  | Redis cache wrapper         |
| ConfigurationService | 15124  | Configuración dinámica      |
| FeatureToggleService | 15126  | Feature flags               |
| SchedulerService     | 15116  | Jobs programados            |
| BackupDRService      | 15138  | Backup y disaster recovery  |
| ServiceDiscovery     | 15140  | Service discovery (Consul)  |

### 📱 UX & Operaciones

| Servicio           | Puerto | Descripción             |
| ------------------ | ------ | ----------------------- |
| MaintenanceService | 5061   | Modo mantenimiento      |
| ComparisonService  | 5066   | Comparador de vehículos |
| AlertService       | 5067   | Alertas de precio       |
| AppointmentService | -      | Test drives             |
| MarketingService   | -      | Campañas de marketing   |
| CRMService         | -      | CRM para dealers        |

---

## 🏗️ ESTRUCTURA DEL PROYECTO

```
cardealer-microservices/
├── .github/
│   ├── copilot-instructions.md     # Este archivo
│   ├── copilot-samples/            # Templates para Copilot
│   └── workflows/                  # GitHub Actions CI/CD
│       ├── deploy-digitalocean.yml # Deploy a DOKS
│       ├── smart-cicd.yml          # Build y push imágenes
│       └── pr-checks.yml           # Validación de PRs
├── backend/                        # Microservicios .NET 8
│   ├── _Shared/                    # Librerías compartidas
│   │   ├── CarDealer.Contracts/    # DTOs y Events compartidos
│   │   └── CarDealer.Shared/       # Utilidades comunes
│   ├── _Tests/                     # Tests unitarios e integración
│   ├── Gateway/                    # Ocelot API Gateway
│   ├── AuthService/                # Autenticación
│   ├── UserService/                # Usuarios
│   ├── KYCService/                 # Verificación de identidad
│   ├── AuditService/               # Auditoría centralizada
│   ├── IdempotencyService/         # Control de idempotencia
│   ├── VehiclesSaleService/        # Vehículos (principal)
│   ├── MediaService/               # Archivos/Imágenes
│   ├── NotificationService/        # Notificaciones
│   ├── BillingService/             # Facturación y suscripciones
│   ├── ErrorService/               # Errores centralizados
│   └── ... (86 servicios total)
├── frontend/
│   ├── web-next/                   # Next.js 16 App Router
│   │   ├── src/
│   │   │   ├── app/               # App Router pages
│   │   │   │   ├── (main)/        # Rutas principales
│   │   │   │   │   ├── cuenta/    # Perfil, verificación
│   │   │   │   │   ├── dealer/    # Portal dealers
│   │   │   │   │   ├── vehiculos/ # Listados
│   │   │   │   │   └── ...
│   │   │   │   ├── (auth)/        # Login, registro
│   │   │   │   └── api/           # API routes
│   │   │   ├── components/        # Componentes React
│   │   │   │   ├── kyc/           # Verificación KYC
│   │   │   │   ├── ui/            # shadcn/ui
│   │   │   │   └── ...
│   │   │   ├── services/          # API clients
│   │   │   ├── hooks/             # Custom hooks
│   │   │   └── lib/               # Utilidades
│   │   ├── Dockerfile
│   │   └── package.json
│   └── mobile/cardealer/           # Flutter app
├── k8s/                            # Kubernetes manifests
│   ├── namespace.yaml
│   ├── deployments.yaml
│   ├── services.yaml
│   ├── ingress.yaml
│   ├── configmaps.yaml
│   └── secrets.yaml
├── docs/                           # 77+ documentos
│   ├── SPRINT_*.md                # Documentación de sprints
│   ├── COMPLIANCE_*.md            # Documentación de compliance
│   └── ...
├── docker-compose.yml              # Docker Compose (desarrollo)
└── cardealer.sln                   # Solución .NET
```

---

## 🔧 ARQUITECTURA DE MICROSERVICIOS

### Clean Architecture por Servicio

Cada microservicio sigue esta estructura:

```
{ServiceName}/
├── {ServiceName}.Api/              # Capa de presentación
│   ├── Controllers/                # REST Controllers
│   ├── Middleware/                 # Custom middleware
│   ├── Program.cs                  # Entry point
│   ├── appsettings.json
│   └── Dockerfile
├── {ServiceName}.Application/      # Capa de aplicación
│   ├── Features/                   # CQRS con MediatR
│   │   ├── Commands/
│   │   └── Queries/
│   ├── DTOs/
│   ├── Validators/                 # FluentValidation
│   └── Clients/                    # Clientes HTTP a otros servicios
├── {ServiceName}.Domain/           # Capa de dominio
│   ├── Entities/
│   ├── Interfaces/
│   ├── Enums/
│   └── Events/
└── {ServiceName}.Infrastructure/   # Capa de infraestructura
    ├── Persistence/                # DbContext, Repositories
    ├── Services/                   # Implementaciones externas
    └── Configurations/             # Entity configurations
```

### Patrones Utilizados

- **CQRS** con MediatR para Commands/Queries
- **Repository Pattern** para acceso a datos
- **Result Pattern** para manejo de errores (evitar excepciones)
- **Domain Events** publicados via RabbitMQ
- **JWT Bearer** para autenticación
- **Centralized Clients** para comunicación inter-servicios:
  - `AuditServiceClient` - Auditoría centralizada
  - `IdempotencyServiceClient` - Control de idempotencia
  - `NotificationServiceClient` - Notificaciones

### Servicios Centralizados (Importantes)

#### AuditService

Todos los microservicios deben registrar acciones críticas:

```csharp
// En Application/Clients/AuditServiceClient.cs
await _auditClient.LogActionAsync(new AuditLogRequest
{
    UserId = userId,
    Action = "CREATE_PROFILE",
    EntityType = "KYCProfile",
    EntityId = profileId,
    Details = JsonSerializer.Serialize(details),
    IpAddress = ipAddress,
    UserAgent = userAgent
});
```

#### IdempotencyService

Para operaciones que no deben duplicarse:

```csharp
// En Middleware/IdempotencyMiddleware.cs
var isProcessed = await _idempotencyClient.CheckAndMarkAsync(idempotencyKey);
if (isProcessed) return cached response;
```

---

### 🔄 Sincronización AuthService ↔ UserService

AuthService y UserService trabajan juntos pero tienen responsabilidades separadas:

| Servicio        | Responsabilidad                                | Datos                              |
| --------------- | ---------------------------------------------- | ---------------------------------- |
| **AuthService** | Autenticación, tokens, 2FA, OAuth, sesiones    | `ApplicationUser`, `RefreshToken`  |
| **UserService** | Perfiles de usuario, datos extendidos, avatars | `User` (FirstName, LastName, etc.) |

#### Flujo de Registro

```
┌─────────────────┐                        ┌─────────────────┐
│    Frontend     │   POST /api/auth/      │   AuthService   │
│  registro/page  │ ────────────────────▶  │  RegisterCmd    │
│                 │   register             │                 │
└─────────────────┘                        └────────┬────────┘
                                                    │
                                                    │ UserRegisteredEvent
                                                    │ {UserId, Email, FirstName,
                                                    │  LastName, PhoneNumber}
                                                    ▼
                                           ┌─────────────────┐
                                           │    RabbitMQ     │
                                           │  Exchange:      │
                                           │  cardealer.events
                                           └────────┬────────┘
                                                    │ routing: auth.user.registered
                                                    ▼
                                           ┌─────────────────┐
                                           │   UserService   │
                                           │  EventConsumer  │
                                           │  → Creates User │
                                           └─────────────────┘
```

#### RegisterCommand (Backend)

```csharp
// Acepta campos del frontend (firstName, lastName, phone)
public record RegisterCommand(
    string? UserName,
    string Email,
    string Password,
    string? FirstName = null,
    string? LastName = null,
    string? Phone = null,
    bool AcceptTerms = true
) : IRequest<RegisterResponse>
{
    // Construye nombre a partir de FirstName/LastName o UserName
    public string GetDisplayName() =>
        !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName)
            ? $"{FirstName.Trim()} {LastName.Trim()}"
            : !string.IsNullOrWhiteSpace(UserName)
                ? UserName
                : Email.Split('@')[0];
}
```

#### UserRegisteredEvent (Compartido)

```csharp
// En _Shared/CarDealer.Contracts/Events/Auth/UserRegisteredEvent.cs
public class UserRegisteredEvent : EventBase
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }      // FirstName + LastName
    public string FirstName { get; set; }     // ← Campo separado
    public string LastName { get; set; }      // ← Campo separado
    public string? PhoneNumber { get; set; }  // ← Opcional
    public DateTime RegisteredAt { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
```

#### GetOrCreateUserCommand (OAuth Sync)

Para usuarios que se autentican vía OAuth (Google, Apple), UserService crea el perfil automáticamente:

```csharp
// En UserService.Application/UseCases/Users/GetOrCreateUser/
var command = new GetOrCreateUserCommand(userId, email, firstName, lastName, avatarUrl);
var result = await _mediator.Send(command);
```

**⚠️ IMPORTANTE:** Siempre que modifiques RegisterCommand o UserRegisteredEvent, asegúrate de:

1. Actualizar el Consumer en UserService
2. Verificar que los campos se propaguen correctamente
3. Mantener backwards compatibility con eventos existentes

---

## 🖥️ FRONTEND (Next.js 16)

### Stack Frontend Completo

| Librería                   | Versión | Uso                                                             |
| -------------------------- | ------- | --------------------------------------------------------------- |
| **Next.js**                | 16.1.6  | Framework (App Router)                                          |
| **React**                  | 19.2.3  | UI Library                                                      |
| **TypeScript**             | 5+      | Tipado                                                          |
| **Tailwind CSS**           | v4      | Estilos (via `@tailwindcss/postcss`)                            |
| **shadcn/ui**              | Latest  | Componentes UI (Radix primitives + CVA + tailwind-merge + clsx) |
| **Zustand**                | 5.0.10  | State management global                                         |
| **TanStack Query**         | 5.90.20 | Data fetching, caching, mutations                               |
| **react-hook-form**        | 7.71.1  | Formularios                                                     |
| **Zod**                    | 4.3.6   | Validación de schemas (con `@hookform/resolvers`)               |
| **Recharts**               | 3.7.0   | Gráficas y dashboards                                           |
| **Vitest**                 | 4.0.18  | Unit/component testing (⚠️ NO Jest)                             |
| **@testing-library/react** | 16.3.2  | Testing de componentes                                          |
| **Playwright**             | 1.58.1  | E2E testing                                                     |
| **MSW**                    | 2.12.7  | API mocking para tests                                          |
| **sonner**                 | Latest  | Toast notifications                                             |
| **@dnd-kit**               | Latest  | Drag & drop                                                     |
| **tesseract.js**           | Latest  | OCR para KYC document reading                                   |
| **pnpm**                   | 9+      | Package manager (⚠️ NO npm/yarn)                                |

> ⚠️ **IMPORTANTE — Dev Server:** Usar `pnpm dev --turbopack` para Turbopack (más rápido).

### Patrones de Frontend Obligatorios

```typescript
// ✅ State management con Zustand
import { create } from "zustand";
export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  setUser: (user) => set({ user }),
}));

// ✅ Data fetching con TanStack Query
import { useQuery, useMutation } from "@tanstack/react-query";
export function useVehicles(filters: VehicleFilters) {
  return useQuery({
    queryKey: ["vehicles", filters],
    queryFn: () => vehicleService.list(filters),
  });
}

// ✅ Formularios con react-hook-form + Zod
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
const schema = z.object({
  email: z.string().email(),
  password: z.string().min(8),
});
const { register, handleSubmit } = useForm({ resolver: zodResolver(schema) });

// ✅ Componentes shadcn/ui
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader } from "@/components/ui/card";

// ✅ Testing con Vitest (NO Jest)
import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
```

### Frontend Testing

```typescript
// ⚠️ USAR Vitest, NO Jest
// Archivo: *.test.tsx o *.spec.tsx

// Unit test
import { describe, it, expect } from 'vitest';
describe('formatPrice', () => {
  it('should format RD currency', () => {
    expect(formatPrice(150000)).toBe('RD$150,000');
  });
});

// Component test
import { render, screen } from '@testing-library/react';
describe('VehicleCard', () => {
  it('should render vehicle title', () => {
    render(<VehicleCard vehicle={mockVehicle} />);
    expect(screen.getByText('Toyota Corolla 2024')).toBeInTheDocument();
  });
});

// API mocking con MSW
import { http, HttpResponse } from 'msw';
import { setupServer } from 'msw/node';
const server = setupServer(
  http.get('/api/vehicles', () => HttpResponse.json({ data: mockVehicles }))
);

// E2E con Playwright
import { test, expect } from '@playwright/test';
test('user can search vehicles', async ({ page }) => {
  await page.goto('/buscar');
  await page.fill('[data-testid="search-input"]', 'Toyota');
  await expect(page.locator('.vehicle-card')).toHaveCount(5);
});
```

### Estructura de App Router

```
src/app/
├── (auth)/                    # Grupo de autenticación
│   ├── login/page.tsx
│   ├── registro/page.tsx
│   ├── recuperar-contrasena/
│   ├── verificar-email/
│   └── layout.tsx
├── (main)/                    # Grupo principal (con navbar)
│   ├── cuenta/
│   │   ├── verificacion/      # KYC verification flow
│   │   ├── perfil/
│   │   └── configuracion/
│   ├── dealer/
│   │   ├── landing/
│   │   ├── pricing/
│   │   ├── register/
│   │   └── dashboard/
│   ├── vehiculos/
│   │   ├── [slug]/page.tsx    # Detalle vehículo
│   │   └── page.tsx           # Listado
│   ├── buscar/page.tsx
│   ├── comparar/page.tsx
│   ├── mis-vehiculos/         # Vehículos del usuario
│   ├── publicar/              # Publicar vehículo
│   ├── checkout/              # Proceso de pago
│   ├── admin/                 # Panel admin
│   └── layout.tsx
├── (messaging)/               # Mensajería
├── api/                       # API Routes
│   └── [...]/route.ts
└── layout.tsx                 # Root layout
```

### Componentes KYC (Verificación de Identidad)

El sistema KYC incluye:

```
src/components/kyc/
├── document-capture.tsx       # Captura de documentos (cédula)
├── liveness-challenge.tsx     # Prueba de vida (blink, smile, turn)
├── verification-gate.tsx      # Gate de verificación para rutas protegidas
└── index.ts                   # Exports
```

**Flujo KYC:**

1. Usuario inicia verificación en `/cuenta/verificacion`
2. Captura documento de identidad (frente y reverso)
3. Completa prueba de vida (parpadear, sonreír, girar cabeza)
4. Backend valida documentos y liveness
5. Admin aprueba/rechaza manualmente (si es necesario)

### Services (API Clients)

```typescript
// src/services/kyc.ts
export const kycService = {
  createProfile: (data: CreateKYCProfileRequest) => api.post('/api/kyc/profiles', data),
  uploadDocument: (profileId: string, file: File, type: string) => ...,
  submitLiveness: (profileId: string, selfie: string, challengeResults: ChallengeResult[]) => ...,
  getStatus: (userId: string) => api.get(`/api/kyc/profiles/user/${userId}`),
};
```

### Variables de Entorno

```env
# .env.local (desarrollo)
NEXT_PUBLIC_API_URL=http://localhost:18443
NEXT_PUBLIC_APP_URL=http://localhost:3000

# .env.production (BFF pattern — Gateway es interno)
NEXT_PUBLIC_API_URL=
INTERNAL_API_URL=http://gateway:8080
NEXT_PUBLIC_APP_URL=https://okla.com.do
```

> ⚠️ **IMPORTANTE - BFF Pattern:**
>
> - `NEXT_PUBLIC_API_URL` está **vacío** en producción — el browser usa URLs relativas (`/api/*`).
> - Next.js rewrites proxean `/api/*` → `gateway:8080/api/*` internamente.
> - `INTERNAL_API_URL` es solo server-side (SSR, API routes, middleware) — NO es `NEXT_PUBLIC_`.
> - Para código server-side, usar `getInternalApiUrl()` de `@/lib/api-url`.
> - Para código client-side, usar `getClientApiUrl()` o `getApiBaseUrl()` de `@/lib/api-url`.

---

## ☸️ KUBERNETES (DOKS)

### Comandos Frecuentes

```bash
# Conectar a cluster
doctl kubernetes cluster kubeconfig save okla-cluster

# Ver pods
kubectl get pods -n okla

# Ver logs de un servicio
kubectl logs -f deployment/gateway -n okla

# Reiniciar un deployment
kubectl rollout restart deployment/vehiclessaleservice -n okla

# Ver ConfigMap del Gateway
kubectl get configmap gateway-config -n okla -o yaml

# Actualizar ConfigMap del Gateway (IMPORTANTE)
kubectl delete configmap gateway-config -n okla
kubectl create configmap gateway-config --from-file=ocelot.json=backend/Gateway/Gateway.Api/ocelot.prod.json -n okla
kubectl rollout restart deployment/gateway -n okla

# Port-forward para debugging
kubectl port-forward svc/kycservice 8080:8080 -n okla
```

### ⚠️ REGLA CRÍTICA: Puertos en Kubernetes

**TODOS los servicios usan puerto 8080 en Kubernetes (NO 80).**

El archivo `ocelot.prod.json` DEBE tener:

```json
{
  "DownstreamHostAndPorts": [{ "Host": "servicename", "Port": 8080 }]
}
```

### Ingress y DNS

| Host            | Service           | TLS              |
| --------------- | ----------------- | ---------------- |
| okla.com.do     | frontend-web:8080 | ✅ Let's Encrypt |
| www.okla.com.do | frontend-web:8080 | ✅ Let's Encrypt |

> **BFF Pattern:** `api.okla.com.do` ya NO tiene regla de Ingress.
> El Gateway solo es accesible desde el pod `frontend-web` (red interna K8s).

---

## 📡 API Endpoints Principales

### Health Check

- `GET /health` - Estado del Gateway

### Auth (`/api/auth`)

- `POST /api/auth/register` - Registro
- `POST /api/auth/login` - Login (retorna JWT)
- `POST /api/auth/refresh` - Refresh token
- `GET /api/auth/me` - Usuario actual

### KYC (`/api/kyc`)

- `POST /api/kyc/profiles` - Crear perfil KYC
- `GET /api/kyc/profiles/user/{userId}` - Obtener perfil por usuario
- `POST /api/kyc/profiles/{id}/documents` - Subir documento
- `POST /api/kyc/profiles/{id}/liveness` - Enviar prueba de vida
- `POST /api/kyc/profiles/{id}/submit` - Enviar para revisión
- `POST /api/kyc/profiles/{id}/approve` - Aprobar (admin)
- `POST /api/kyc/profiles/{id}/reject` - Rechazar (admin)

### Vehicles (`/api/vehicles`)

- `GET /api/vehicles` - Listar (paginado)
- `GET /api/vehicles/{id}` - Detalle
- `GET /api/vehicles/slug/{slug}` - Por slug
- `POST /api/vehicles` - Crear (auth required)
- `PUT /api/vehicles/{id}` - Actualizar
- `DELETE /api/vehicles/{id}` - Eliminar

### Dealers (`/api/dealers`)

- `GET /api/dealers` - Listar dealers
- `GET /api/dealers/{id}` - Detalle dealer
- `POST /api/dealers` - Registrar dealer
- `PUT /api/dealers/{id}` - Actualizar
- `GET /api/dealers/{id}/analytics` - Métricas

### Audit (`/api/audit`)

- `POST /api/audit/logs` - Registrar acción
- `GET /api/audit/logs` - Listar logs (admin)
- `GET /api/audit/logs/entity/{type}/{id}` - Logs por entidad

### Idempotency (`/api/idempotency`)

- `POST /api/idempotency/check` - Verificar key
- `POST /api/idempotency/mark` - Marcar como procesado
- `DELETE /api/idempotency/{key}` - Limpiar key

---

## 🔄 CI/CD (GitHub Actions)

### Workflows Principales

| Workflow    | Archivo                   | Trigger             | Función               |
| ----------- | ------------------------- | ------------------- | --------------------- |
| Smart CI/CD | `smart-cicd.yml`          | Push a main/develop | Build + Push imágenes |
| Deploy DO   | `deploy-digitalocean.yml` | Manual o post-CI    | Deploy a DOKS         |
| PR Checks   | `pr-checks.yml`           | PR abierto          | Validación            |

### Servicios en CI/CD

```yaml
SERVICES: "frontend-web,gateway,authservice,userservice,roleservice,vehiclessaleservice,mediaservice,notificationservice,billingservice,errorservice,kycservice,auditservice,idempotencyservice,adminservice,chatbotservice,contactservice,reviewservice,dealermanagementservice,dealeranalyticsservice,crmservice,maintenanceservice,comparisonservice,alertservice,appointmentservice,marketingservice,staffservice,reportsservice,inventorymanagementservice,paymentservice,aiprocessingservice,vehicleintelligenceservice,recommendationservice,leadscoringservice,backgroundremovalservice,vehicle360processingservice,cacheservice,messagebusservice,configurationservice,schedulerservice,ratelimitingservice,servicediscovery,apidocsservice,integrationservice,dataprotectionservice"
```

### ⚠️ REGLAS CRÍTICAS DE CI/CD

#### 1. Nombres de Imagen Docker — DEBEN coincidir entre CI/CD y K8s

El nombre de la imagen en `k8s/deployments.yaml` **DEBE** ser idéntico al que se pushea en el workflow de CI/CD.

| Servicio     | Imagen correcta (GHCR)                         | Archivo CI/CD                  |
| ------------ | ---------------------------------------------- | ------------------------------ |
| frontend-web | `ghcr.io/gregorymorenoiem/frontend-web:latest` | `_reusable-frontend.yml`       |
| gateway      | `ghcr.io/gregorymorenoiem/gateway:latest`      | `_reusable-dotnet-service.yml` |
| authservice  | `ghcr.io/gregorymorenoiem/authservice:latest`  | `_reusable-dotnet-service.yml` |
| (otros)      | `ghcr.io/gregorymorenoiem/{service}:latest`    | `_reusable-dotnet-service.yml` |

> 🔴 **INCIDENTE Feb 2026:** `deployments.yaml` referenciaba `cardealer-web:latest` (imagen vieja Vite/nginx)
> pero el CI/CD pushea `frontend-web:latest` (imagen Next.js nueva). Resultado: el frontend mostraba la página vieja.
> **SIEMPRE** verificar que `deployments.yaml` y el workflow usan el MISMO nombre de imagen.

#### 2. Docker Build Cache — Puede causar imágenes stale

El workflow `_reusable-dotnet-service.yml` usa `cache-from: type=local` con `restore-keys`. Esto puede causar
que buildx reutilice ALL cached layers (incluyendo `COPY . .` y `dotnet publish`), produciendo imágenes con
código viejo a pesar de que el CI reporta "build exitoso".

**Síntomas:**

- CI/CD muestra todos los pasos como `CACHED`
- El digest de la imagen nueva es idéntico al anterior
- Los pods siguen ejecutando código viejo

**Solución cuando ocurre:**

```bash
# Listar y eliminar caches de buildx
gh cache list --key "Linux-buildx-{service}" | awk '{print $1}' | xargs -I{} gh cache delete {}
# Trigger nuevo build (commit vacío o touch)
```

**Prevención:** Si un servicio no recoge cambios después de push, sospechar del cache primero.

#### 3. Registry Credentials — Tokens efímeros vs duraderos

El K8s secret `registry-credentials` permite a los pods bajar imágenes de GHCR (privado).

| Tipo de Token    | Prefijo        | Duración                        | Fuente                         |
| ---------------- | -------------- | ------------------------------- | ------------------------------ |
| GitHub Actions   | `ghs_*`        | ~1 hora (solo dura el workflow) | `secrets.GITHUB_TOKEN` en CI   |
| OAuth (CLI)      | `gho_*`        | ~8 horas                        | `gh auth token`                |
| PAT clásico      | `ghp_*`        | Configurable (hasta never)      | GitHub Settings → Tokens       |
| Fine-grained PAT | `github_pat_*` | Configurable                    | GitHub Settings → Fine-grained |

> 🔴 **INCIDENTE Feb 2026:** El secret usaba un token `ghs_*` efímero del CI/CD que expiró.
> Resultado: `ImagePullBackOff` en todos los pods nuevos.

**Para refrescar el secret:**

```bash
# 1. Obtener token (usar PAT para larga duración)
TOKEN=$(gh auth token)  # o usar un PAT clásico

# 2. Recrear secret
kubectl delete secret registry-credentials -n okla
kubectl create secret docker-registry registry-credentials \
  --docker-server=ghcr.io \
  --docker-username=gregorymorenoiem \
  --docker-password=$TOKEN \
  -n okla
```

> ⚠️ **MEJOR PRÁCTICA:** Usar un **Fine-grained PAT** con scope `read:packages` y expiración larga (90 días+)
> para el secret de K8s. NO usar tokens de workflow (`ghs_*`).

---

## 🐳 DOCKER — REGLAS Y PATRONES

### Dockerfiles Backend (.NET 8)

Cada servicio tiene su Dockerfile en `backend/{Service}/Dockerfile`. Todos siguen multi-stage build:

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "{Service}.Api/{Service}.Api.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "{Service}.Api.dll"]
```

> ⚠️ **IMPORTANTE — Contexto del build:** El CI/CD usa `context: ./backend` (NO `./backend/{Service}`).
> Esto es necesario porque los servicios referencian `_Shared/` (CarDealer.Shared, CarDealer.Contracts).
> Los Dockerfiles deben usar paths relativos desde `./backend/`.

### Dockerfile Frontend (Next.js)

El frontend usa multi-stage con pnpm y standalone output:

```dockerfile
# Key points:
# - Usa pnpm (NO npm/yarn)
# - Build args: NEXT_PUBLIC_API_URL= (vacío para BFF pattern)
# - Standalone output con node server.js
# - Puerto 8080 (para K8s)
# - Runner: node:20-alpine (NO nginx)
```

> ⚠️ **REGLA:** El frontend en K8s corre como **Node.js server** (port 8080), NO como nginx.
> La imagen vieja `cardealer-web` usaba nginx — ya NO se usa.

### OpenTelemetry — Versión Compatible

> ⚠️ **REGLA:** OpenTelemetry DEBE usar versión **1.9.0** (máximo). La versión 1.10.0 requiere .NET 9.
> Si se actualiza a 1.10.0, el build falla con errores de API incompatible.

---

## 🔌 DEPENDENCY INJECTION (DI) — REGLAS CRÍTICAS

### El Mismatch IDeadLetterQueue vs ISharedDeadLetterQueue

La librería compartida `CarDealer.Shared` registra `ISharedDeadLetterQueue` (PostgreSQL-backed) via
`AddPostgreSqlDeadLetterQueue()`. Pero cada servicio tiene su propia interfaz local `IDeadLetterQueue`
(en `Domain.Interfaces` o `Infrastructure.Messaging`) que es la que `DeadLetterQueueProcessor` inyecta.

> 🔴 **INCIDENTE Feb 2026:** 6 servicios crasheaban al iniciar con
> `Unable to resolve service for type 'IDeadLetterQueue'`.

**Regla:** Si un servicio usa `DeadLetterQueueProcessor` (HostedService), DEBE registrar `IDeadLetterQueue`:

```csharp
// En Program.cs — DESPUÉS de AddPostgreSqlDeadLetterQueue y ANTES de AddHostedService<DeadLetterQueueProcessor>
builder.Services.AddSingleton<IDeadLetterQueue, InMemoryDeadLetterQueue>();
builder.Services.AddHostedService<DeadLetterQueueProcessor>();
```

**Servicios que requieren esta registración:**

- AuthService, ErrorService, RoleService, AuditService, NotificationService, MediaService

### Regla General de DI

**SIEMPRE verificar que todas las dependencias inyectadas en HostedServices, Controllers y Handlers
están registradas en `Program.cs`.** Un `AddHostedService<T>()` sin su correspondiente `AddSingleton<IDependency>()`
causa crash silencioso al iniciar el pod.

**Test de validación (recomendado):**

```csharp
[Fact]
public async Task Application_DI_Container_Resolves_All_Services()
{
    await using var app = new WebApplicationFactory<Program>();
    using var client = app.CreateClient();
    var response = await client.GetAsync("/health");
    response.EnsureSuccessStatusCode(); // Falla si DI no resuelve algún servicio
}
```

---

## 🐇 RABBITMQ — REGLAS CRÍTICAS

### Queue Arguments Son Inmutables

RabbitMQ **NO permite cambiar los argumentos** de una queue existente. Si el código declara una queue
con argumentos diferentes a los que ya tiene, RabbitMQ responde con `PRECONDITION_FAILED` y el servicio crashea.

> 🔴 **INCIDENTE Feb 2026:** Queues existentes sin `x-dead-letter-exchange`. Código nuevo las declara
> CON `x-dead-letter-exchange`. Resultado: `PRECONDITION_FAILED` y crash en loop.

**Regla:** Si cambias argumentos de una queue (DLX, TTL, max-length, etc.):

1. **PRIMERO** eliminar la queue vieja manualmente
2. **DESPUÉS** desplegar el código nuevo que la recrea con los argumentos nuevos

```bash
# Eliminar queue manualmente
kubectl exec deployment/rabbitmq -n okla -- rabbitmqctl delete_queue {queue-name}
```

**Argumentos comunes que causan PRECONDITION_FAILED si se cambian:**

- `x-dead-letter-exchange`
- `x-dead-letter-routing-key`
- `x-message-ttl`
- `x-max-length`
- `x-queue-type` (classic vs quorum)

### Queues Actuales en Producción

| Exchange                | Queue                      | DLX                         | Servicio            |
| ----------------------- | -------------------------- | --------------------------- | ------------------- |
| `notification-exchange` | `notification-queue`       | `notification-exchange.dlx` | NotificationService |
| `notification-exchange` | `notification-email-queue` | `notification-exchange.dlx` | NotificationService |
| `notification-exchange` | `notification-sms-queue`   | `notification-exchange.dlx` | NotificationService |
| `errors-exchange`       | `errors.queue`             | —                           | ErrorService        |
| `cardealer.events`      | (varios por servicio)      | —                           | Todos               |

### Configuración de RabbitMQ en K8s

Las credenciales de RabbitMQ se pasan via K8s secrets. Los servicios leen:

| Variable de Entorno  | Valor        | Notas                                   |
| -------------------- | ------------ | --------------------------------------- |
| `RabbitMQ__HostName` | `rabbitmq`   | Nombre del service K8s                  |
| `RabbitMQ__UserName` | `okla_admin` | ⚠️ También existe como `RabbitMQ__User` |
| `RabbitMQ__Password` | (en secret)  |                                         |
| `RabbitMQ__Port`     | `5672`       |                                         |

> ⚠️ **IMPORTANTE:** El secret de K8s tiene AMBAS keys `RabbitMQ__UserName` y `RabbitMQ__User`
> porque algunos servicios leen una u otra. Si creas un servicio nuevo, usar `RabbitMQ__UserName`.

---

## 🏥 HEALTH CHECKS — REGLAS CRÍTICAS

### Configuración Correcta de Health Checks

Cada servicio .NET debe configurar **3 endpoints** de health check:

```csharp
// En Program.cs
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = check => !check.Tags.Contains("external") // ⚠️ Excluir checks externos
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false // Solo verifica que el proceso responde
});
```

> 🔴 **INCIDENTE Feb 2026:** El endpoint `/health` (sin filtro) ejecutaba `ExternalServiceHealthCheck`
> que intentaba conectar a Consul en `localhost:8500` (no desplegado). Timeout de 200 segundos bloqueaba
> el thread pool, causando que TAMBIÉN `/health/ready` fallara. Los pods entraban en restart loop.

**Reglas:**

1. ✅ **SIEMPRE** excluir checks con tag `"external"` del endpoint `/health`
2. ✅ Los checks externos (Consul, servicios terceros) deben tener tag `["external"]`
3. ✅ El endpoint `/health/live` NUNCA debe ejecutar checks reales (solo verifica proceso vivo)
4. ✅ Health checks que conectan a servicios externos deben tener timeout ≤ 5 segundos
5. ❌ **NUNCA** dejar el endpoint `/health` sin `Predicate` — cualquier check lento mata el pod

### Consul — NO Desplegado

Consul (`localhost:8500`) **NO está desplegado** en el cluster K8s. Los servicios que lo referencian
(`ExternalServiceHealthCheck`, `ServiceRegistrationMiddleware`) emiten warnings no-fatales.
Esto es esperado y no afecta la operación de los servicios.

---

## 🗄️ BASES DE DATOS

### Dos PostgreSQL en el Cluster

| Recurso                   | Host                                                 | Puerto | Uso                                               |
| ------------------------- | ---------------------------------------------------- | ------ | ------------------------------------------------- |
| **DO Managed PostgreSQL** | `okla-db-do-user-31493168-0.g.db.ondigitalocean.com` | 25060  | Producción (backups automáticos, $15/mes)         |
| **In-cluster PostgreSQL** | `postgres` (K8s service)                             | 5432   | Staging/desarrollo (pod StatefulSet, sin backups) |

> ⚠️ Los servicios actualmente apuntan al **DO Managed PostgreSQL** via K8s secrets.
> Las connection strings usan `sslmode=require` para la DB managed.

### Bases de Datos Creadas

Cada microservicio tiene su propia base de datos (database-per-service pattern):

```
authservice_db, userservice_db, roleservice_db, vehiclessaleservice_db,
mediaservice_db, notificationservice_db, billingservice_db, errorservice_db,
kycservice_db, auditservice_db, idempotencyservice_db, contactservice_db,
chatbotservice_db, adminservice_db, dealermanagementservice_db, reviewservice_db
```

### Serilog — Crash Conocido

> ⚠️ **REGLA:** NO usar `CreateBootstrapLogger()` si el servicio usa `UseStandardSerilog()`
> (de CarDealer.Shared). La combinación causa "logger already frozen" crash al iniciar.
>
> ```csharp
> // ❌ INCORRECTO — causa crash
> Log.Logger = new LoggerConfiguration().CreateBootstrapLogger();
> builder.UseStandardSerilog(); // BOOM: "logger already frozen"
>
> // ✅ CORRECTO
> builder.UseStandardSerilog(); // Solo esto, sin CreateBootstrapLogger
> ```

---

## � SHARED LIBRARY — EXTENSIONES OBLIGATORIAS

La carpeta `_Shared/` contiene librerías compartidas con **extension methods estandarizados** que TODOS los servicios DEBEN usar. **NO reimplementar funcionalidad que ya existe en shared.**

### CarDealer.Shared — Extensions Obligatorias en Program.cs

```csharp
// ============= PROGRAM.CS — PATRÓN ESTÁNDAR =============
var builder = WebApplication.CreateBuilder(args);

// 1. Configuración externalizada (K8s secrets)
builder.Configuration.AddMicroserviceSecrets();

// 2. Serilog centralizado (Console + Seq) — ⚠️ NO usar CreateBootstrapLogger()
builder.UseStandardSerilog();

// 3. Base de datos (PostgreSQL + EF Core + retry + auto-migrate)
builder.Services.AddStandardDatabase<ServiceDbContext>(builder.Configuration);

// 4. RabbitMQ (singleton connection per pod)
builder.Services.AddStandardRabbitMq(builder.Configuration);

// 5. Dead Letter Queue (PostgreSQL-backed)
builder.Services.AddPostgreSqlDeadLetterQueue(builder.Configuration);

// 6. OpenTelemetry (tracing + metrics + Prometheus)
builder.Services.AddStandardObservability(builder.Configuration, "ServiceName");

// 7. Global error handling (ProblemDetails + IErrorPublisher)
builder.Services.AddGlobalErrorHandling(builder.Configuration);

// 8. Security headers (OWASP)
// (se configura en middleware, no en services)

// 9. Audit middleware
builder.Services.AddAuditMiddleware();

// 10. MediatR + ValidationBehavior (auto-ejecuta FluentValidation)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Command).Assembly));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// 11. FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CommandValidator).Assembly);
```

| Extension Method                 | Paquete          | Qué hace                                    |
| -------------------------------- | ---------------- | ------------------------------------------- |
| `AddMicroserviceSecrets()`       | CarDealer.Shared | Lee config de K8s secrets                   |
| `UseStandardSerilog()`           | CarDealer.Shared | Serilog Console + Seq                       |
| `AddStandardDatabase<T>()`       | CarDealer.Shared | EF Core PostgreSQL + retry + auto-migrate   |
| `AddStandardRabbitMq()`          | CarDealer.Shared | RabbitMQ singleton connection               |
| `AddPostgreSqlDeadLetterQueue()` | CarDealer.Shared | DLQ PostgreSQL-backed                       |
| `AddStandardObservability()`     | CarDealer.Shared | OpenTelemetry tracing + metrics             |
| `AddGlobalErrorHandling()`       | CarDealer.Shared | GlobalExceptionMiddleware + IErrorPublisher |
| `UseApiSecurityHeaders()`        | CarDealer.Shared | OWASP security headers (CSP, HSTS, etc.)    |
| `AddAuditMiddleware()`           | CarDealer.Shared | Audit event publishing via RabbitMQ         |
| `UseRequestLogging()`            | CarDealer.Shared | Request logging con CorrelationId           |

### ValidationBehavior — Auto-ejecución de FluentValidation

MediatR tiene un pipeline behavior que ejecuta automáticamente TODOS los validators FluentValidation antes del handler:

```csharp
// NO hacer validación manual en handlers — ValidationBehavior lo hace automáticamente
// ❌ INCORRECTO
public async Task<Result<T>> Handle(Command cmd, CancellationToken ct)
{
    var validation = await _validator.ValidateAsync(cmd); // Innecesario
    if (!validation.IsValid) return Result.Fail(...);
}

// ✅ CORRECTO — ValidationBehavior ya ejecutó los validators
public async Task<Result<T>> Handle(Command cmd, CancellationToken ct)
{
    // Si llegamos aquí, la validación ya pasó
    var entity = new Entity(cmd.Name, cmd.Email);
    await _repository.AddAsync(entity, ct);
    return Result.Ok(entity.ToDto());
}
```

Si la validación falla, `ValidationBehavior` lanza `ValidationException` que `GlobalExceptionMiddleware` convierte en RFC 7807 `ProblemDetails` (400).

---

## 🔄 MIDDLEWARE PIPELINE — ORDEN CANÓNICO

El orden del middleware en `Program.cs` es **CRÍTICO**. Seguir este orden exacto:

```csharp
var app = builder.Build();

// 1. Global error handling — SIEMPRE PRIMERO
app.UseGlobalErrorHandling();

// 2. Request logging (shared library — agrega CorrelationId, RequestId)
app.UseRequestLogging();

// 3. Security headers (OWASP — CSP, HSTS, X-Frame-Options)
app.UseApiSecurityHeaders(isProduction: !app.Environment.IsDevelopment());

// 4. Rate limiting
app.UseMiddleware<RateLimitBypassMiddleware>();
app.UseCustomRateLimiting(rateLimitingConfig);

// 5. HTTPS redirection — solo fuera de K8s (K8s termina TLS en Ingress)
if (!app.Environment.IsProduction()) app.UseHttpsRedirection();

// 6. Swagger — solo desarrollo
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

// 7. CORS — ANTES de auth
app.UseCors();

// 8. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 9. Audit middleware — DESPUÉS de auth (necesita userId)
app.UseAuditMiddleware();

// 10. Endpoints
app.MapControllers();

// 11. Health checks (3 endpoints obligatorios)
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = check => !check.Tags.Contains("external")
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

// 12. Prometheus metrics endpoint
app.UsePrometheusScrapingEndpoint(); // Expone /metrics

app.Run();

// ⚠️ OBLIGATORIO al final del archivo — necesario para WebApplicationFactory en integration tests
public partial class Program { }
```

> ⚠️ **REGLA:** Todo `Program.cs` DEBE terminar con `public partial class Program { }` para que los integration tests con `WebApplicationFactory<Program>` funcionen.

---

## 📡 API RESPONSE FORMATS — DOBLE FORMATO

El sistema usa **dos formatos de respuesta** que coexisten. El frontend DEBE manejar ambos.

### Formato 1: ApiResponse<T> (respuestas normales)

```csharp
// En {Service}.Shared/ApiResponse.cs — Usado en Controllers
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public ApiMetadata? Metadata { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };
    public static ApiResponse<T> Fail(string error) => new() { Success = false, Error = error };
}

// Uso en controllers:
return Ok(ApiResponse<UserDto>.Ok(user));
return BadRequest(ApiResponse<UserDto>.Fail("Email ya registrado"));
```

```json
// Response JSON (success)
{ "success": true, "data": { "id": "...", "email": "..." }, "timestamp": "2026-02-18T..." }

// Response JSON (error)
{ "success": false, "error": "Email ya registrado", "timestamp": "2026-02-18T..." }
```

### Formato 2: RFC 7807 ProblemDetails (errores no manejados / validación)

```json
// Retornado automáticamente por GlobalExceptionMiddleware
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "Validation failed",
  "traceId": "abc123",
  "errorCode": "VALIDATION_ERROR",
  "errors": {
    "Email": ["Email is required"],
    "Password": ["Minimum 8 characters"]
  }
}
```

**Mapeo de excepciones a status codes:**

| Excepción                     | Status | Cuándo                                                     |
| ----------------------------- | ------ | ---------------------------------------------------------- |
| `ValidationException`         | 400    | FluentValidation falla (automático via ValidationBehavior) |
| `UnauthorizedAccessException` | 401    | Token inválido o expirado                                  |
| `KeyNotFoundException`        | 404    | Entidad no encontrada                                      |
| `TimeoutException`            | 504    | Timeout a servicio externo                                 |
| Cualquier otra                | 500    | Error interno (detail oculto en prod)                      |

### Frontend — Manejo de Ambos Formatos

```typescript
// En services/api.ts — wrapper que maneja ambos formatos
async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const body = await response.json();
    // Formato ProblemDetails (RFC 7807)
    if (body.type && body.status) {
      throw new ApiError(body.title, body.status, body.errors);
    }
    // Formato ApiResponse
    if (body.success === false) {
      throw new ApiError(body.error, response.status);
    }
    throw new ApiError("Unknown error", response.status);
  }
  const data = await response.json();
  return data.data ?? data; // ApiResponse wraps in .data
}
```

---

## 📐 OBSERVABILITY — PATRONES OBLIGATORIOS

### Structured Logging — Niveles

| Nivel         | Cuándo usar                       | Ejemplo                                                      |
| ------------- | --------------------------------- | ------------------------------------------------------------ |
| `Debug`       | Detalles internos para desarrollo | `Log.Debug("Processing item {ItemId}", id)`                  |
| `Information` | Eventos de negocio significativos | `Log.Information("User {UserId} registered", userId)`        |
| `Warning`     | Algo inesperado pero no fatal     | `Log.Warning("Retry {Attempt} for {Service}", n, svc)`       |
| `Error`       | Error que afecta la operación     | `Log.Error(ex, "Failed to process payment {PaymentId}", id)` |
| `Fatal`       | Error irrecuperable (app crash)   | `Log.Fatal(ex, "Database connection lost")`                  |

> ⚠️ **REGLA:** Usar **structured logging** con templates, NO concatenación de strings.
> ✅ `Log.Information("User {UserId} logged in", userId)`
> ❌ `Log.Information($"User {userId} logged in")`

### Custom Metrics — Patrón ServiceMetrics

Cada servicio crea una clase `ServiceMetrics` con contadores y histogramas:

```csharp
// En {Service}.Infrastructure/Metrics/ServiceMetrics.cs
public class ServiceMetrics
{
    private readonly Counter<long> _operationCounter;
    private readonly Histogram<double> _operationDuration;

    public ServiceMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("ServiceName", "1.0.0");
        _operationCounter = meter.CreateCounter<long>(
            "servicename.operations.total",  // Naming: {service}.{operation}.{unit}
            description: "Total operations processed");
        _operationDuration = meter.CreateHistogram<double>(
            "servicename.operations.duration_ms",
            unit: "ms",
            description: "Operation duration in milliseconds");
    }

    public void RecordOperation(string type) => _operationCounter.Add(1, new("type", type));
    public void RecordDuration(double ms) => _operationDuration.Record(ms);
}
```

### Domain Events — Contrato EventBase

```csharp
// Todos los eventos DEBEN heredar de EventBase (en CarDealer.Contracts)
public abstract class EventBase : IEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public abstract string EventType { get; }    // ⚠️ Formato: "{domain}.{entity}.{action}"
    public int SchemaVersion { get; set; } = 1;  // Versionado de schema
    public string? CorrelationId { get; set; }   // Trazabilidad cross-service
}

// Ejemplo:
public class VehicleCreatedEvent : EventBase
{
    public override string EventType => "vehicles.vehicle.created";  // ← Naming convention
    public Guid VehicleId { get; set; }
    public string Title { get; set; }
    public decimal Price { get; set; }
}
```

**Convención de EventType:** `{domain}.{entity}.{action}` (e.g., `auth.user.registered`, `vehicles.vehicle.created`, `billing.payment.completed`).

---

## 🧪 TESTING — ESTÁNDARES OBLIGATORIOS

### Stack de Testing Backend

| Librería                  | Versión | Uso                 |
| ------------------------- | ------- | ------------------- |
| **xUnit**                 | 2.6.2   | Framework de tests  |
| **FluentAssertions**      | 6.12.0  | Assertions legibles |
| **Moq**                   | 4.20.70 | Mocking             |
| **WebApplicationFactory** | 8.0.0   | Integration tests   |
| **coverlet**              | 6.0.0   | Code coverage       |

### Naming Convention

```
{Método}_{Escenario}_{Resultado}

Ejemplos:
  Login_WithValidCredentials_ReturnsToken
  CreateVehicle_WithoutTitle_ReturnsValidationError
  GetUser_WithNonExistentId_ReturnsNotFound
```

### Test Obligatorio — DI Startup

```csharp
// CADA servicio DEBE tener este test
[Fact]
public async Task Application_Starts_And_DI_Resolves_All_Services()
{
    await using var app = new WebApplicationFactory<Program>();
    using var client = app.CreateClient();
    var response = await client.GetAsync("/health");
    response.EnsureSuccessStatusCode();
}
```

### Test Pyramid — Targets

| Tipo              |  Target Mínimo  | Framework                                |
| ----------------- | :-------------: | ---------------------------------------- |
| Unit tests        |  70% cobertura  | xUnit + FluentAssertions + Moq           |
| Integration tests | Startup + CRUD  | WebApplicationFactory                    |
| E2E tests         | Flujos críticos | Playwright (frontend), scripts (backend) |

### Integration Test Infrastructure

```csharp
// En {Service}.Tests/CustomWebApplicationFactory.cs
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Reemplazar DB real con InMemory
            services.RemoveAll(typeof(DbContextOptions<ServiceDbContext>));
            services.AddDbContext<ServiceDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
        });
    }
}
```

---

## 🗄️ EF CORE — PATRONES DE PERSISTENCIA

### DbContext Naming

```csharp
// Naming: {Service}DbContext — en {Service}.Infrastructure/Persistence/
public class AuthServiceDbContext : DbContext
{
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthServiceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

### Entity Configuration

```csharp
// En Infrastructure/Persistence/Configurations/{Entity}Configuration.cs
public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Title).HasMaxLength(200).IsRequired();
        builder.Property(v => v.Price).HasColumnType("decimal(18,2)");
        builder.HasIndex(v => v.Slug).IsUnique();

        // Soft delete — query filter global
        builder.HasQueryFilter(v => !v.IsDeleted);
    }
}
```

### Soft Delete Pattern

```csharp
// En Domain/Entities — interfaz ISoftDeletable
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}

// En DbContext — override SaveChangesAsync
public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    foreach (var entry in ChangeTracker.Entries<ISoftDeletable>())
    {
        if (entry.State == EntityState.Deleted)
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = DateTime.UtcNow;
        }
    }
    return await base.SaveChangesAsync(ct);
}
```

### Migration Commands

```bash
# Crear migration
dotnet ef migrations add {NombreDescriptivo} \
  --project {Service}.Infrastructure \
  --startup-project {Service}.Api

# Aplicar migration
dotnet ef database update \
  --project {Service}.Infrastructure \
  --startup-project {Service}.Api

# Revertir última migration
dotnet ef migrations remove \
  --project {Service}.Infrastructure \
  --startup-project {Service}.Api
```

> ⚠️ **Auto-migration en K8s:** Si `EnableAutoMigration: true` en appsettings, las migrations se aplican al iniciar el pod. Para producción, preferir migrations explícitas.

---

## ⚡ PERFORMANCE — ESTÁNDARES

### Targets de Respuesta

| Endpoint                    | Target  | Máximo |
| --------------------------- | ------- | ------ |
| Health check (`/health`)    | < 100ms | 500ms  |
| Lectura simple (GET by ID)  | < 200ms | 1s     |
| Lectura paginada (GET list) | < 500ms | 2s     |
| Escritura (POST/PUT)        | < 500ms | 3s     |
| Búsqueda con filtros        | < 1s    | 5s     |

### Reglas de Performance

1. ✅ **SIEMPRE paginar** listados — nunca retornar colecciones completas
2. ✅ **Usar `AsNoTracking()`** para queries de solo lectura
3. ✅ **Incluir solo lo necesario** — `Select()` específico en vez de `Include()` masivo
4. ✅ **Indexes** en columnas usadas en WHERE, ORDER BY, y foreign keys
5. ✅ **Redis cache** para datos que cambian poco (roles, configuración, catálogos)
6. ✅ **Connection pooling** — configurado via `AddStandardDatabase()` (MaxPoolSize en config)
7. ❌ **NUNCA** hacer N+1 queries — usar `Include()` o `Join()` cuando necesario
8. ❌ **NUNCA** retornar entidades de dominio en controllers — siempre DTOs

### Paginación Estándar

```csharp
// Request
public record GetVehiclesQuery(int Page = 1, int PageSize = 20, string? SortBy = null) : IRequest<PagedResult<VehicleDto>>;

// Response
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
```

### CORS Configuration Estándar

```csharp
// CORS con headers específicos requeridos
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000", "https://okla.com.do" };

        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
              .WithHeaders("Content-Type", "Authorization", "X-CSRF-Token",
                           "X-Requested-With", "X-Idempotency-Key")
              .AllowCredentials();
    });
});
```

---

## 🤖 CHATBOT SERVICE — CONTEXTO LLM

### Arquitectura

El ChatbotService implementa un chatbot llamado **"Ana"** para asistencia automotriz en español dominicano.

| Componente           | Tecnología                          | Descripción                           |
| -------------------- | ----------------------------------- | ------------------------------------- |
| **Backend**          | .NET 8 (Clean Architecture)         | API, gestión de sesiones, integración |
| **Inference Server** | Python (FastAPI + llama-cpp-python) | Sirve el modelo LLM                   |
| **Modelo**           | Llama 3 (fine-tuned QLoRA)          | GGUF Q4_K_M quantization              |
| **Dataset**          | 37 intents, 1,376 templates         | Español dominicano automotriz         |

### Configuración Clave

| Parámetro      | Valor       | Razón                         |
| -------------- | ----------- | ----------------------------- |
| `N_CTX`        | 4096        | Context window (tokens)       |
| `MAX_TOKENS`   | 600         | Max response length           |
| `TEMPERATURE`  | 0.7         | Balance creatividad/precisión |
| `MODEL_FORMAT` | GGUF Q4_K_M | Optimizado para CPU           |

### Seguridad del Chatbot

```csharp
// PiiDetector.cs — Detecta y enmascara datos personales
// PromptInjectionDetector.cs — Detecta intentos de inyección de prompt
// Ambos son OBLIGATORIOS para cualquier endpoint que acepte texto libre del usuario
```

### Español Dominicano

El chatbot usa español dominicano auténtico con 60+ mappings de slang regional. Los templates de respuesta están diseñados para ser naturales al mercado local.

---

## �🐛 TROUBLESHOOTING COMÚN

### 404 en Gateway

1. Verificar que la ruta existe en `ocelot.prod.json` o `ocelot.Development.json`
2. Verificar que el ConfigMap está actualizado
3. Reiniciar Gateway después de actualizar ConfigMap

### 503 Service Unavailable

1. **Verificar puerto** - Debe ser 8080, no 80
2. Verificar que el servicio destino está Running
3. Verificar conectividad interna

### CORS Error

1. Verificar configuración CORS en Gateway y servicios
2. Verificar que el dominio está en la lista permitida

### KYC Camera Issues

1. Verificar permisos de cámara en navegador
2. Usar HTTPS (cámara requiere contexto seguro)
3. Verificar que `react-webcam` está instalado

### ImagePullBackOff en K8s

1. **Verificar secret de registry:** `kubectl get secret registry-credentials -n okla -o jsonpath='{.data.\.dockerconfigjson}' | base64 -d`
2. **Verificar que el token no expiró** (tokens `ghs_*` duran ~1 hora)
3. **Refrescar secret:**
   ```bash
   TOKEN=$(gh auth token)
   kubectl delete secret registry-credentials -n okla
   kubectl create secret docker-registry registry-credentials \
     --docker-server=ghcr.io --docker-username=gregorymorenoiem \
     --docker-password=$TOKEN -n okla
   ```
4. **Verificar nombre de imagen** — debe coincidir EXACTAMENTE con lo que CI/CD pushea a GHCR

### Pod CrashLoopBackOff — DI Resolution Failure

**Síntoma:** Pod inicia, crashea inmediatamente, logs muestran `Unable to resolve service for type 'IXxx'`

1. Verificar que TODAS las interfaces inyectadas en HostedServices están registradas en `Program.cs`
2. Caso común: `IDeadLetterQueue` no registrada (ver sección DI más arriba)
3. **Test de validación:**
   ```csharp
   [Fact]
   public async Task DI_Container_Resolves_All_Services()
   {
       await using var app = new WebApplicationFactory<Program>();
       using var client = app.CreateClient();
       var response = await client.GetAsync("/health");
       response.EnsureSuccessStatusCode();
   }
   ```

### Pod CrashLoopBackOff — RabbitMQ PRECONDITION_FAILED

**Síntoma:** Logs muestran `PRECONDITION_FAILED - inequivalent arg 'x-dead-letter-exchange'`

1. Los argumentos de una queue RabbitMQ son INMUTABLES
2. **Solución:** Eliminar la queue vieja antes de desplegar el código nuevo:
   ```bash
   kubectl exec deployment/rabbitmq -n okla -- rabbitmqctl delete_queue {queue-name}
   ```

### Pod CrashLoopBackOff — Serilog "Logger Already Frozen"

**Síntoma:** Logs muestran `System.InvalidOperationException: Logger already frozen`

1. **Causa:** `CreateBootstrapLogger()` + `UseStandardSerilog()` en el mismo servicio
2. **Solución:** Eliminar `Log.Logger = new LoggerConfiguration().CreateBootstrapLogger();`
3. Solo usar `builder.UseStandardSerilog();`

### Health Check Timeout — Pods en Restart Loop

**Síntoma:** `/health/ready` tarda >200 segundos, pods reinician por readiness probe timeout

1. **Causa:** `ExternalServiceHealthCheck` intenta conectar a Consul (`localhost:8500`) que no está desplegado
2. **Solución:** Excluir checks con tag `"external"` del endpoint `/health`:
   ```csharp
   app.MapHealthChecks("/health", new HealthCheckOptions
   {
       Predicate = check => !check.Tags.Contains("external")
   });
   ```

### Frontend Muestra Página Vieja

1. **Verificar nombre de imagen** en `k8s/deployments.yaml` — debe ser `frontend-web:latest`, NO `cardealer-web:latest`
2. **Verificar digest:** `kubectl get pod -n okla -l app=frontend-web -o jsonpath='{.items[0].status.containerStatuses[0].imageID}'`
3. Si el digest es idéntico al anterior, limpiar Docker build cache:
   ```bash
   gh cache list --key "Linux-buildx-frontend" | awk '{print $1}' | xargs -I{} gh cache delete {}
   ```

### CI/CD Build Exitoso Pero Imagen No Cambia

1. **Causa:** Docker buildx cache reutiliza ALL layers incluyendo `COPY . .` y `dotnet publish`
2. **Síntoma:** Todos los pasos del build muestran `CACHED`, digest idéntico al anterior
3. **Solución:** Limpiar cache de buildx para el servicio:
   ```bash
   gh cache list --key "Linux-buildx-{service}" | awk '{print $1}' | xargs -I{} gh cache delete {}
   ```

---

## 📝 CONVENCIONES DE CÓDIGO

### C# / .NET

```csharp
// Namespaces file-scoped
namespace AuthService.Domain.Entities;

// Records para DTOs inmutables
public record UserDto(Guid Id, string Email, string FullName);

// Primary constructors para DI
public class UserService(IUserRepository repo, ILogger<UserService> logger)
{
    public async Task<User?> GetAsync(Guid id) => await repo.GetByIdAsync(id);
}

// Result Pattern para errores
public async Task<Result<T>> HandleAsync(Command cmd, CancellationToken ct);

// Siempre usar CancellationToken
public async Task ProcessAsync(CancellationToken ct = default);
```

### TypeScript / React (Next.js)

```typescript
// Server Components por defecto
export default async function Page() {
  const data = await fetchData();
  return <div>{data}</div>;
}

// 'use client' solo cuando necesario
'use client';
export function InteractiveComponent() {
  const [state, setState] = useState();
  // ...
}

// Custom hooks con prefijo use
export const useAuth = () => { /* ... */ };

// API calls con error handling
try {
  const response = await kycService.createProfile(data);
} catch (error: unknown) {
  const err = error as { message?: string; status?: number };
  // Handle error
}
```

### Commits

```
<type>(<scope>): <description>

Tipos: feat, fix, docs, style, refactor, test, chore
Ejemplos:
  feat(kyc): add liveness challenge component
  fix(gateway): use correct port 8080 for production
  docs(readme): update deployment instructions
```

---

## 🔐 SEGURIDAD

- JWT tokens con expiración de 24h
- Refresh tokens para renovación automática
- HTTPS obligatorio en producción (Let's Encrypt)
- Secrets en Kubernetes Secrets (no en código)
- CORS configurado para dominios específicos
- Rate limiting en Gateway
- KYC verification para operaciones sensibles
- Audit logging de todas las acciones críticas
- Idempotency keys para prevenir operaciones duplicadas

---

## 📚 DOCUMENTACIÓN

La carpeta `docs/` contiene **77+ documentos** organizados por categoría:

### Sprints Completados

- `SPRINT_1_COMPLETE_REPORT.md` hasta `SPRINT_17_COMPLETED.md`
- Documentación detallada de cada sprint

### Compliance (RD)

- `COMPLIANCE_MICROSERVICES_ARCHITECTURE.md`
- `NORMATIVAS_RD_OKLA.md`
- `PLAN_COMPLIANCE_AUDITABILIDAD_RD.md`

### Arquitectura

- `MICROSERVICES_ANALYSIS_AND_IMPROVEMENTS.md`
- `DATA_ML_MICROSERVICES_STRATEGY.md`
- `GATEWAY_ENDPOINTS_AUDIT.md`

### Integraciones

- `STRIPE_API_DOCUMENTATION.md`
- `AZUL_SANDBOX_SETUP_GUIDE.md`
- `SPYNE_INTEGRATION_COMPLETE.md`
- `ZOHO_MAIL_SETUP_GUIDE.md`

### KYC & Verificación

- `KYC_CAMERA_ENHANCEMENT_COMPLETED.md`
- `KYC_CAMERA_TESTING_RESULTS.md`

---

## 🛡️ SEGURIDAD - VULNERABILIDADES RESUELTAS

Este proyecto implementa múltiples capas de seguridad para proteger contra vulnerabilidades comunes. **Es obligatorio aplicar estas protecciones en todo nuevo código.**

### 1. SQL Injection Protection

**Ubicación:** `{Service}.Application/Validators/SecurityValidators.cs`

**Implementación:**

```csharp
// Validador FluentValidation
public static IRuleBuilderOptions<T, string> NoSqlInjection<T>(this IRuleBuilder<T, string> ruleBuilder)
{
    return ruleBuilder.Must(input =>
    {
        if (string.IsNullOrWhiteSpace(input)) return true;
        var upperInput = input.ToUpperInvariant();
        return !SqlKeywords.Any(keyword => upperInput.Contains(keyword));
    })
    .WithMessage("Input contains potential SQL injection patterns.");
}
```

**Patrones bloqueados (25+):**

| Categoría      | Patrones                                         |
| -------------- | ------------------------------------------------ |
| DML            | `SELECT`, `INSERT`, `UPDATE`, `DELETE`           |
| DDL            | `DROP`, `CREATE`, `ALTER`                        |
| Procedimientos | `EXEC`, `EXECUTE`, `xp_`, `sp_`                  |
| Combinaciones  | `UNION`, `DECLARE`, `CAST`, `CONVERT`            |
| Comentarios    | `--`, `/*`, `*/`                                 |
| Metadata       | `INFORMATION_SCHEMA`, `SYSOBJECTS`, `SYSCOLUMNS` |
| Time-based     | `WAITFOR DELAY`, `BENCHMARK`, `SLEEP(`           |
| Bypass         | `OR 1=1`, `OR '1'='1'`                           |

**Uso obligatorio:**

```csharp
// En TODOS los validadores de commands/queries
RuleFor(x => x.Email)
    .NotEmpty()
    .EmailAddress()
    .NoSqlInjection(); // ✅ OBLIGATORIO
```

---

### 2. XSS (Cross-Site Scripting) Protection

**Backend - Validación con FluentValidation:**

```csharp
public static IRuleBuilderOptions<T, string> NoXss<T>(this IRuleBuilder<T, string> ruleBuilder)
{
    return ruleBuilder.Must(input =>
    {
        if (string.IsNullOrWhiteSpace(input)) return true;
        var lowerInput = input.ToLowerInvariant();
        return !XssPatterns.Any(pattern => lowerInput.Contains(pattern));
    })
    .WithMessage("Input contains potential XSS attack patterns.");
}
```

**Patrones XSS bloqueados (25+):**

| Categoría   | Patrones                                                                 |
| ----------- | ------------------------------------------------------------------------ |
| Scripts     | `<script`, `</script>`, `javascript:`, `vbscript:`                       |
| Eventos     | `onerror=`, `onload=`, `onclick=`, `onmouseover=`, `onfocus=`, `onblur=` |
| Iframes     | `<iframe`, `</iframe>`                                                   |
| Objects     | `<object`, `<embed`, `<svg`                                              |
| Ejecución   | `eval(`, `expression(`, `alert(`, `confirm(`, `prompt(`                  |
| Data URLs   | `data:text/html`                                                         |
| Animaciones | `onanimationstart=`, `onanimationend=`, `ontransitionend=`               |

**Frontend - Sanitización de inputs (`/lib/security/sanitize.ts`):**

```typescript
// Escape HTML entities para prevenir XSS
export function escapeHtml(str: string): string {
  const HTML_ENTITIES = {
    "&": "&amp;",
    "<": "&lt;",
    ">": "&gt;",
    '"': "&quot;",
    "'": "&#x27;",
    "/": "&#x2F;",
    "`": "&#x60;",
    "=": "&#x3D;",
  };
  return str.replace(/[&<>"'`=/]/g, (char) => HTML_ENTITIES[char]);
}

// Strip all HTML tags
export function stripHtml(str: string): string {
  return str.replace(/<[^>]*>/g, "");
}

// Sanitize URLs - bloquea javascript:, data:, vbscript:
export function sanitizeUrl(url: string): string {
  const lower = url.trim().toLowerCase();
  if (
    lower.startsWith("javascript:") ||
    lower.startsWith("data:") ||
    lower.startsWith("vbscript:")
  ) {
    return "";
  }
  return url;
}
```

---

### 3. CSRF (Cross-Site Request Forgery) Protection

**Ubicación Frontend:** `/lib/security/csrf.tsx`

**Implementación - Double Submit Cookie Pattern:**

```typescript
// Hook React para obtener token CSRF
export function useCsrfToken() {
  const [token, setToken] = useState<string>('');

  useEffect(() => {
    setToken(getCsrfToken());
  }, []);

  return { token, headers: { 'X-CSRF-Token': token }, refresh };
}

// Fetch wrapper con CSRF automático
export async function csrfFetch(url: string, options: RequestInit = {}): Promise<Response> {
  const headers = new Headers(options.headers);
  headers.set('X-CSRF-Token', getCsrfToken());

  return fetch(url, {
    ...options,
    headers,
    credentials: 'same-origin', // Include cookies
  });
}

// Componente para formularios
export function CsrfInput() {
  const { token } = useCsrfToken();
  return <input type="hidden" name="csrf" value={token} />;
}

// Validación timing-safe para prevenir timing attacks
export function validateDoubleSubmit(headerToken: string, cookieToken: string): boolean {
  if (headerToken.length !== cookieToken.length) return false;
  let result = 0;
  for (let i = 0; i < headerToken.length; i++) {
    result |= headerToken.charCodeAt(i) ^ cookieToken.charCodeAt(i);
  }
  return result === 0;
}
```

**Uso obligatorio en formularios:**

```tsx
// Opción 1: Componente
<form action="/api/action">
  <CsrfInput />
  {/* ... otros campos */}
</form>;

// Opción 2: Hook
const { headers } = useCsrfToken();
await fetch("/api/action", {
  method: "POST",
  headers,
  body: JSON.stringify(data),
});

// Opción 3: Wrapper
await csrfFetch("/api/action", { method: "POST", body: JSON.stringify(data) });
```

---

### 4. JWT Authentication Security

**Configuración Backend (`Program.cs`):**

```csharp
// Configuración JWT con secrets centralizados
var (jwtKey, jwtIssuer, jwtAudience) = MicroserviceSecretsConfiguration.GetJwtConfig(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // No tolerancia de tiempo
        };
    });
```

**Reglas de seguridad JWT:**

| Regla          | Implementación                            |
| -------------- | ----------------------------------------- |
| Key mínimo     | 32 caracteres (256 bits) para HMAC-SHA256 |
| Expiración     | 24 horas máximo                           |
| Refresh tokens | Almacenados en HttpOnly cookies           |
| ClockSkew      | 0 (sin tolerancia)                        |
| Validate all   | Issuer, Audience, Lifetime, SigningKey    |

---

### 5. Input Sanitization (Frontend)

**Funciones disponibles en `/lib/security/sanitize.ts`:**

| Función                 | Uso                         | Ejemplo                                 |
| ----------------------- | --------------------------- | --------------------------------------- |
| `escapeHtml()`          | Renderizar texto de usuario | `{escapeHtml(userInput)}`               |
| `stripHtml()`           | Limpiar tags HTML           | `stripHtml("<p>texto</p>")` → `"texto"` |
| `sanitizeUrl()`         | URLs seguras                | Bloquea `javascript:`, `data:`          |
| `sanitizeSearchQuery()` | Queries de búsqueda         | Limita a 200 chars, elimina `<>"'`      |
| `sanitizeFilename()`    | Nombres de archivo          | Solo `a-zA-Z0-9._-`                     |
| `sanitizeNumber()`      | Números con límites         | `{ min, max, allowFloat }`              |
| `sanitizePhone()`       | Teléfonos RD                | Formato 10 dígitos                      |
| `sanitizeEmail()`       | Emails                      | Lowercase, max 254 chars                |
| `sanitizeRNC()`         | RNC dominicano              | 9 o 11 dígitos                          |
| `sanitizePlate()`       | Placas RD                   | Max 7 chars, uppercase                  |
| `sanitizeVIN()`         | Número VIN                  | 17 chars, excluye I,O,Q                 |
| `sanitizePrice()`       | Precios                     | 0 - 100,000,000                         |
| `sanitizeYear()`        | Años                        | 1900 - (año actual + 2)                 |
| `sanitizeMileage()`     | Kilometraje                 | 0 - 2,000,000                           |
| `sanitizeText()`        | Descripciones               | Strip HTML, max length                  |

---

### 6. Rate Limiting

**Ubicación:** `/lib/security/rate-limit.ts`

**Implementación:**

```typescript
// Configuración por endpoint
const rateLimitConfig = {
  "/api/auth/login": { max: 5, window: "15m" }, // 5 intentos cada 15 min
  "/api/auth/register": { max: 3, window: "1h" }, // 3 registros por hora
  "/api/contact": { max: 10, window: "1h" }, // 10 mensajes por hora
  "/api/vehicles": { max: 100, window: "1m" }, // 100 requests por minuto
};
```

---

### 7. Password Security

**Requisitos de contraseña (Login y Register):**

```csharp
// En RegisterCommandValidator.cs y LoginCommandValidator.cs
RuleFor(x => x.Password)
    .NotEmpty()
    .MinimumLength(8)
    .MaximumLength(128)
    .Matches("[A-Z]").WithMessage("Debe contener al menos una mayúscula")
    .Matches("[a-z]").WithMessage("Debe contener al menos una minúscula")
    .Matches("[0-9]").WithMessage("Debe contener al menos un número")
    .Matches("[^a-zA-Z0-9]").WithMessage("Debe contener al menos un carácter especial")
    .NoXss()           // ✅ OBLIGATORIO en Password
    .NoSqlInjection(); // ✅ OBLIGATORIO en Password
```

**Frontend - Formulario de Registro (`registro/page.tsx`):**

```typescript
// Sanitizar inputs ANTES de enviar al backend
const sanitizedFirstName = sanitizeText(formData.firstName.trim(), {
  maxLength: 50,
});
const sanitizedLastName = sanitizeText(formData.lastName.trim(), {
  maxLength: 50,
});
const sanitizedEmail = sanitizeEmail(formData.email);
const sanitizedPhone = formData.phone
  ? sanitizePhone(formData.phone)
  : undefined;

// Enviar datos sanitizados
await authService.register({
  firstName: sanitizedFirstName,
  lastName: sanitizedLastName,
  email: sanitizedEmail,
  phone: sanitizedPhone,
  password: formData.password, // Password NO se sanitiza
  acceptTerms: formData.acceptTerms,
});
```

**⚠️ IMPORTANTE:** El password NO se sanitiza en frontend porque podría contener caracteres válidos que las funciones de sanitización eliminarían (como `<`, `>`, `&` que son válidos en contraseñas).

---

### 8. Servicios con Security Validators Implementados

| Servicio            | SecurityValidators.cs | Aplicado en                          |
| ------------------- | --------------------- | ------------------------------------ |
| AuthService         | ✅                    | Login, Register, ChangePassword, 2FA |
| MediaService        | ✅                    | Upload, Metadata                     |
| NotificationService | ✅                    | Send, Templates                      |
| AuditService        | ✅                    | LogAction                            |

---

### ⚠️ REGLAS DE SEGURIDAD OBLIGATORIAS

**Al crear/modificar código:**

1. ✅ **SIEMPRE** usar `.NoSqlInjection()` y `.NoXss()` en validators de strings
2. ✅ **SIEMPRE** usar `csrfFetch()` o `CsrfInput` en formularios/requests POST/PUT/DELETE
3. ✅ **SIEMPRE** sanitizar inputs de usuario antes de renderizar (`escapeHtml`, `sanitizeText`)
4. ✅ **SIEMPRE** sanitizar URLs con `sanitizeUrl()` antes de usar en `href` o `src`
5. ✅ **NUNCA** concatenar strings en queries SQL (usar parámetros)
6. ✅ **NUNCA** renderizar HTML de usuario sin sanitizar
7. ✅ **NUNCA** exponer stack traces en producción
8. ✅ **NUNCA** almacenar secrets en código (usar Kubernetes Secrets o env vars)

**Copiar SecurityValidators a nuevos servicios:**

```bash
# Copiar desde AuthService como template
cp backend/AuthService/AuthService.Application/Validators/SecurityValidators.cs \
   backend/NewService/NewService.Application/Validators/
```

---

## ✅ REGLAS OBLIGATORIAS

### Al crear un nuevo microservicio:

1. ✅ Usar Clean Architecture (Domain, Application, Infrastructure, Api)
2. ✅ Implementar Health Checks (excluir checks `"external"` del endpoint `/health`)
3. ✅ Agregar rutas al Gateway (ocelot.\*.json)
4. ✅ Crear proyecto de tests (xUnit + FluentAssertions + Moq)
5. ✅ Usar puerto 8080 en Kubernetes
6. ✅ Integrar con AuditService para logging
7. ✅ Implementar IdempotencyMiddleware si aplica
8. ✅ **Copiar e implementar SecurityValidators.cs** (NoSqlInjection, NoXss)
9. ✅ **Aplicar validadores de seguridad en TODOS los commands/queries**
10. ✅ **Registrar TODAS las dependencias de DI** — verificar con test de startup
11. ✅ **Verificar nombre de imagen Docker** coincide entre Dockerfile, CI/CD workflow y `k8s/deployments.yaml`
12. ✅ **NO usar `CreateBootstrapLogger()`** si el servicio usa `UseStandardSerilog()`
13. ✅ **Verificar Health Checks** excluyen checks con tag `"external"` del endpoint `/health`
14. ✅ **Usar OpenTelemetry 1.9.0** (NO 1.10.0 que requiere .NET 9)
15. ✅ **Usar TODAS las shared extensions** (AddStandardDatabase, AddStandardObservability, etc.)
16. ✅ **Seguir middleware pipeline canónico** (12 pasos en orden exacto)
17. ✅ **Usar ApiResponse<T>** para respuestas y **ProblemDetails** para errores
18. ✅ **Configurar Swagger/OpenAPI** con descripción de endpoints
19. ✅ **Implementar paginación** (PagedResult<T>) para todos los listados
20. ✅ **Agregar `public partial class Program { }`** al final de Program.cs
21. ✅ **Crear test de DI startup** con WebApplicationFactory
22. ✅ **Crear ServiceMetrics** class para métricas custom

### Al crear UI nueva:

1. ✅ Agregar ruta en App Router
2. ✅ Usar 'use client' solo cuando necesario
3. ✅ Implementar loading.tsx y error.tsx
4. ✅ Verificar responsive design
5. ✅ Probar accesibilidad (WCAG 2.1 AA)
6. ✅ **Usar csrfFetch() o CsrfInput para forms/requests mutables**
7. ✅ **Sanitizar TODO input de usuario antes de renderizar**
8. ✅ **Usar sanitizeUrl() para cualquier URL de usuario**
9. ✅ **Usar Zustand para state, TanStack Query para data fetching**
10. ✅ **Usar react-hook-form + Zod para formularios**
11. ✅ **Usar componentes shadcn/ui (NO crear componentes UI custom)**
12. ✅ **Manejar ambos formatos de respuesta** (ApiResponse + ProblemDetails)
13. ✅ **Escribir tests con Vitest (NO Jest)** + Testing Library
14. ✅ **Usar next/image para imágenes** (optimización automática)

### Al modificar servicios existentes:

1. ✅ Actualizar tests
2. ✅ Verificar compatibilidad con Gateway
3. ✅ Documentar cambios en CHANGELOG
4. ✅ Probar en docker-compose antes de deploy
5. ✅ **Verificar que todos los inputs tienen validadores de seguridad**
6. ✅ **Verificar Docker build cache** — si la imagen no recoge cambios, limpiar cache de buildx
7. ✅ **Verificar argumentos de queues RabbitMQ** — si cambiaron, eliminar queue vieja primero
8. ✅ **Verificar DI registrations** — todo `AddHostedService<T>()` debe tener sus dependencias registradas
9. ✅ **Verificar nombre de imagen** en `k8s/deployments.yaml` coincide con CI/CD

### Al desplegar a Kubernetes:

1. ✅ Verificar que la imagen Docker existe en GHCR con el nombre correcto
2. ✅ Verificar que `registry-credentials` secret no ha expirado
3. ✅ Verificar que el `k8s/deployments.yaml` usa el nombre de imagen correcto
4. ✅ Verificar que los ConfigMaps están actualizados (especialmente `gateway-config`)
5. ✅ Verificar que las environment variables en K8s secrets incluyen las keys correctas
6. ✅ Verificar health check endpoints (`/health`, `/health/ready`, `/health/live`)
7. ✅ Verificar que NO hay `CreateBootstrapLogger()` en el servicio
8. ✅ Si se cambiaron argumentos de queues RabbitMQ, eliminar queues viejas primero

---

## 🔴 DESPUÉS DE CADA IMPLEMENTACIÓN

**OBLIGATORIO:** Después de cualquier cambio de código, verificar la ventana de **PROBLEMS** en VS Code:

```
Ver → Problems (Ctrl+Shift+M / Cmd+Shift+M)
```

### Pasos a seguir:

1. ✅ **Revisar todos los ERRORS** (🔴) - Deben corregirse ANTES de continuar
2. ✅ **Revisar todos los WARNINGS** (🟡) - Deben corregirse si es posible
3. ✅ **Usar `get_errors` tool** para obtener la lista de errores programáticamente:
   ```
   get_errors({ filePaths: ["/ruta/al/archivo/modificado.cs"] })
   ```

### Errores comunes a corregir:

| Error    | Causa                                | Solución                             |
| -------- | ------------------------------------ | ------------------------------------ |
| `CS8618` | Property no nullable sin inicializar | Agregar `= string.Empty` o `= null!` |
| `CS0246` | Tipo o namespace no encontrado       | Agregar `using` statement            |
| `TS2304` | Cannot find name                     | Agregar import o declarar tipo       |
| `TS2322` | Type mismatch                        | Verificar tipos y agregar casting    |
| `ESLint` | Reglas de linting                    | Corregir según la regla indicada     |

### ⚠️ NO TERMINAR una tarea si hay errores en PROBLEMS

Antes de marcar una tarea como completada:

1. Ejecutar `get_errors` en todos los archivos modificados
2. Corregir todos los errores reportados
3. Verificar que el código compila sin errores

---

_Documento mantenido por el equipo de desarrollo - Febrero 18, 2026_
_86 Microservicios | Next.js 16 | .NET 8 | PostgreSQL | Kubernetes (DOKS)_
