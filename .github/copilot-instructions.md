# 🤖 GitHub Copilot Instructions - OKLA (CarDealer Microservices)

Este documento proporciona contexto para GitHub Copilot sobre el proyecto OKLA (antes CarDealer).

**Última actualización:** Febrero 7, 2026

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
| **Frontend Web**       | Next.js 14 + TypeScript + App Router | ^14.0.0     |
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

### ✅ Servicios Core Desplegados en DOKS

El proyecto está **EN PRODUCCIÓN** en Digital Ocean Kubernetes (cluster: `okla-cluster`, namespace: `okla`).

| Servicio                | Estado     | Puerto K8s | Descripción               |
| ----------------------- | ---------- | ---------- | ------------------------- |
| **frontend-web**        | ✅ Running | 8080       | Next.js 14 SSR/SSG        |
| **gateway**             | ✅ Running | 8080       | Ocelot API Gateway        |
| **authservice**         | ✅ Running | 8080       | Autenticación JWT         |
| **userservice**         | ✅ Running | 8080       | Gestión de usuarios       |
| **roleservice**         | ✅ Running | 8080       | Roles y permisos          |
| **vehiclessaleservice** | ✅ Running | 8080       | CRUD vehículos + catálogo |
| **mediaservice**        | ✅ Running | 8080       | Gestión de imágenes (S3)  |
| **notificationservice** | ✅ Running | 8080       | Email/SMS/Push            |
| **billingservice**      | ✅ Running | 8080       | Pagos (Stripe + Azul)     |
| **errorservice**        | ✅ Running | 8080       | Centralización de errores |
| **kycservice**          | ✅ Running | 8080       | Verificación de identidad |
| **auditservice**        | ✅ Running | 8080       | Auditoría centralizada    |
| **idempotencyservice**  | ✅ Running | 8080       | Control de idempotencia   |
| **postgres**            | ✅ Running | 5432       | Base de datos principal   |
| **redis**               | ✅ Running | 6379       | Cache distribuido         |
| **rabbitmq**            | ✅ Running | 5672/15672 | Message broker            |

**Load Balancer IP:** 146.190.199.0

### 💳 Pasarelas de Pago

OKLA utiliza **dos pasarelas de pago** para maximizar conversiones:

| Pasarela                 | Uso Principal                              | Comisión | Depósito |
| ------------------------ | ------------------------------------------ | -------- | -------- |
| **Azul (Banco Popular)** | Tarjetas dominicanas (DEFAULT)             | ~2.5%    | 24-48h   |
| **Stripe**               | Tarjetas internacionales, Apple/Google Pay | ~3.5%    | 7 días   |

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

| Servicio                  | Puerto | Descripción            |
| ------------------------- | ------ | ---------------------- |
| BillingService            | 15107  | Lógica de facturación  |
| PaymentService            | -      | Procesamiento de pagos |
| StripePaymentService      | -      | Integración Stripe     |
| InvoicingService          | -      | Generación de facturas |
| BankReconciliationService | -      | Conciliación bancaria  |

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
│   ├── BillingService/             # Pagos Stripe + Azul
│   ├── ErrorService/               # Errores centralizados
│   └── ... (86 servicios total)
├── frontend/
│   ├── web-next/                   # Next.js 14 App Router
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

## 🖥️ FRONTEND (Next.js 14)

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
SERVICES: "frontend-web,gateway,authservice,userservice,roleservice,vehiclessaleservice,mediaservice,notificationservice,billingservice,errorservice,kycservice,auditservice,idempotencyservice"
```

---

## 🐛 TROUBLESHOOTING COMÚN

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
2. ✅ Implementar Health Checks
3. ✅ Agregar rutas al Gateway (ocelot.\*.json)
4. ✅ Crear proyecto de tests
5. ✅ Usar puerto 8080 en Kubernetes
6. ✅ Integrar con AuditService para logging
7. ✅ Implementar IdempotencyMiddleware si aplica
8. ✅ **Copiar e implementar SecurityValidators.cs** (NoSqlInjection, NoXss)
9. ✅ **Aplicar validadores de seguridad en TODOS los commands/queries**

### Al crear UI nueva:

1. ✅ Agregar ruta en App Router
2. ✅ Usar 'use client' solo cuando necesario
3. ✅ Implementar loading.tsx y error.tsx
4. ✅ Verificar responsive design
5. ✅ Probar accesibilidad
6. ✅ **Usar csrfFetch() o CsrfInput para forms/requests mutables**
7. ✅ **Sanitizar TODO input de usuario antes de renderizar**
8. ✅ **Usar sanitizeUrl() para cualquier URL de usuario**

### Al modificar servicios existentes:

1. ✅ Actualizar tests
2. ✅ Verificar compatibilidad con Gateway
3. ✅ Documentar cambios en CHANGELOG
4. ✅ Probar en docker-compose antes de deploy
5. ✅ **Verificar que todos los inputs tienen validadores de seguridad**

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

_Documento mantenido por el equipo de desarrollo - Febrero 2026_
_86 Microservicios | Next.js 14 | .NET 8 | PostgreSQL | Kubernetes_
