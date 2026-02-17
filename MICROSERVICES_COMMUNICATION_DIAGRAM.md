# 📊 Diagrama de Comunicación entre Microservicios

**Fecha:** 6 Enero 2026  
**Proyecto:** CarDealer Microservices  
**Estado:** Arquitectura Event-Driven con RabbitMQ

---

## 🏗️ ARQUITECTURA GENERAL

```mermaid
graph TB
    subgraph "Clients"
        WEB[Web Frontend<br/>React 19]
        MOBILE[Mobile App<br/>Flutter]
    end

    subgraph "API Gateway Layer"
        GATEWAY[Gateway Service<br/>:18443<br/>Ocelot]
    end

    subgraph "Core Services"
        AUTH[AuthService<br/>:15085<br/>Identity, JWT]
        USER[UserService<br/>:15100<br/>User Management]
        ROLE[RoleService<br/>:15101<br/>Roles & Permissions]
    end

    subgraph "Business Services"
        VEHICLES[VehiclesSaleService<br/>:15070<br/>Vehicle Catalog]
        RENT[VehiclesRentService<br/>:15071<br/>Rentals]
        PROPS[PropertiesSaleService<br/>:15072<br/>Real Estate]
        PROPRENT[PropertiesRentService<br/>:15073<br/>Property Rent]
        BILLING[BillingService<br/>:15008<br/>Stripe Integration]
        CRM[CRMService<br/>:15009<br/>Customer Relations]
    end

    subgraph "Infrastructure Services"
        NOTIFY[NotificationService<br/>:15084<br/>Email, SMS, Push]
        MEDIA[MediaService<br/>:15090<br/>File Upload, S3]
        ERROR[ErrorService<br/>:15083<br/>Error Tracking]
        AUDIT[AuditService<br/>Audit Logs]
    end

    subgraph "Message Broker"
        RABBIT[(RabbitMQ<br/>:5672<br/>Event Bus)]
    end

    subgraph "Databases"
        AUTHDB[(PostgreSQL<br/>authservice)]
        USERDB[(PostgreSQL<br/>userservice)]
        VEHICLESDB[(PostgreSQL<br/>vehiclessaleservice)]
        BILLINGDB[(PostgreSQL<br/>billingservice)]
        NOTIFYDB[(PostgreSQL<br/>notificationservice)]
    end

    subgraph "Cache & Storage"
        REDIS[(Redis<br/>:6379<br/>Cache)]
        S3[AWS S3<br/>File Storage]
    end

    WEB --> GATEWAY
    MOBILE --> GATEWAY
    
    GATEWAY --> AUTH
    GATEWAY --> USER
    GATEWAY --> VEHICLES
    GATEWAY --> BILLING
    GATEWAY --> NOTIFY
    
    AUTH -.Publica eventos.-> RABBIT
    VEHICLES -.Publica eventos.-> RABBIT
    BILLING -.Publica eventos.-> RABBIT
    
    RABBIT -.Consume eventos.-> NOTIFY
    RABBIT -.Consume eventos.-> AUDIT
    RABBIT -.Consume eventos.-> ERROR
    
    AUTH --> AUTHDB
    USER --> USERDB
    VEHICLES --> VEHICLESDB
    BILLING --> BILLINGDB
    NOTIFY --> NOTIFYDB
    
    MEDIA --> S3
    VEHICLES --> REDIS
    AUTH --> REDIS
    
    style RABBIT fill:#ff9999,stroke:#333,stroke-width:3px
    style GATEWAY fill:#99ccff,stroke:#333,stroke-width:2px
    style NOTIFY fill:#99ff99,stroke:#333,stroke-width:2px
```

---

## 🔄 SINCRONIZACIÓN 1: Welcome Email

### Flujo Completo

```mermaid
sequenceDiagram
    participant Web as Web Frontend
    participant GW as Gateway
    participant Auth as AuthService
    participant RMQ as RabbitMQ
    participant Notify as NotificationService
    participant SG as SendGrid API
    participant User as Usuario

    Web->>GW: POST /api/auth/register
    Note over Web,GW: { email, password, fullName }
    
    GW->>Auth: POST /api/auth/register
    
    Auth->>Auth: Crear usuario en BD
    Note over Auth: ApplicationUser creado<br/>EmailConfirmed = true
    
    Auth->>RMQ: PublishAsync(UserRegisteredEvent)
    Note over Auth,RMQ: Routing Key: auth.user.registered<br/>Exchange: cardealer.events
    
    RMQ-->>Notify: Enruta a Queue
    Note over RMQ,Notify: Queue: notificationservice.user.registered
    
    Notify->>Notify: UserRegisteredNotificationConsumer<br/>procesa evento
    
    Notify->>Notify: Genera HTML de bienvenida
    Note over Notify: Template con nombre,<br/>link al dashboard
    
    Notify->>SG: SendEmailAsync()
    Note over Notify,SG: To: user@email.com<br/>Subject: ¡Bienvenido a CarDealer!
    
    SG->>User: 📧 Email de bienvenida
    
    Auth->>GW: 201 Created + JWT Token
    GW->>Web: 201 Created + Token
    
    Web->>Web: Redirect a /dashboard
    
    Note over Web,User: Usuario recibe email<br/>mientras navega al dashboard
```

### Datos Sincronizados

| Campo | Origen (AuthService) | Destino (NotificationService) | Uso en Email |
|-------|---------------------|-------------------------------|--------------|
| UserId | `user.Id` | `eventData.UserId` | Tracking |
| Email | `user.Email` | `eventData.Email` | **Recipient** |
| FullName | `user.FullName` | `eventData.FullName` | **Saludo personalizado** |
| AccountType | `user.AccountType` | `eventData.AccountType` | Personalización mensaje |
| CreatedAt | `user.CreatedAt` | `eventData.CreatedAt` | Timestamp |

---

## 🚗 SINCRONIZACIÓN 2: Vehicle Notification

### Flujo Completo

```mermaid
sequenceDiagram
    participant Web as Dealer Frontend
    participant GW as Gateway
    participant Vehicles as VehiclesSaleService
    participant RMQ as RabbitMQ
    participant Notify as NotificationService
    participant SG as SendGrid API
    participant Dealer as Dealer Email

    Web->>GW: POST /api/vehicles
    Note over Web,GW: { make, model, year, vin, price, ... }
    
    GW->>Vehicles: POST /api/vehicles
    
    Vehicles->>Vehicles: Validar datos
    Note over Vehicles: FluentValidation:<br/>VIN único, precio > 0
    
    Vehicles->>Vehicles: Guardar vehículo en BD
    Note over Vehicles: Vehicle entity creado<br/>DealerId asociado
    
    Vehicles->>RMQ: PublishAsync(VehicleCreatedEvent)
    Note over Vehicles,RMQ: Routing Key: vehicle.created<br/>Exchange: cardealer.events<br/>Payload: VehicleId, Make, Model, Year, VIN, Price
    
    RMQ-->>Notify: Enruta a Queue
    Note over RMQ,Notify: Queue: notificationservice.vehicle.created
    
    Notify->>Notify: VehicleCreatedNotificationConsumer<br/>procesa evento
    
    Notify->>Notify: Genera HTML con detalles del vehículo
    Note over Notify: Muestra: Year, Make, Model<br/>VIN, Precio, Link al vehículo
    
    Notify->>SG: SendEmailAsync()
    Note over Notify,SG: To: dealer@email.com<br/>Subject: Nuevo Vehículo Publicado: 2024 Toyota Camry
    
    SG->>Dealer: 📧 Notificación de publicación
    
    Vehicles->>GW: 201 Created + VehicleDto
    GW->>Web: 201 Created + Vehicle data
    
    Web->>Web: Muestra confirmación<br/>"Vehículo publicado exitosamente"
    
    Note over Web,Dealer: Dealer recibe confirmación<br/>visual + email de respaldo
```

### Datos Sincronizados

| Campo | Origen (VehiclesSaleService) | Destino (NotificationService) | Uso en Email |
|-------|----------------------------|-------------------------------|--------------|
| VehicleId | `vehicle.Id` | `eventData.VehicleId` | **Link al vehículo** |
| DealerId | `vehicle.DealerId` | `eventData.DealerId` | Obtener email dealer |
| Make | `vehicle.Make` | `eventData.Make` | **Subject + Body** |
| Model | `vehicle.Model` | `eventData.Model` | **Subject + Body** |
| Year | `vehicle.Year` | `eventData.Year` | **Subject + Body** |
| VIN | `vehicle.VIN` | `eventData.VIN` | **Mostrar en tabla** |
| Price | `vehicle.Price` | `eventData.Price` | **Mostrar en tabla** |
| CreatedAt | `vehicle.CreatedAt` | `eventData.CreatedAt` | Fecha de publicación |

---

## 💳 SINCRONIZACIÓN 3: Payment Receipt

### Flujo Completo

```mermaid
sequenceDiagram
    participant Web as User Frontend
    participant GW as Gateway
    participant Billing as BillingService
    participant Stripe as Stripe API
    participant RMQ as RabbitMQ
    participant Notify as NotificationService
    participant SG as SendGrid API
    participant User as Usuario

    Web->>GW: POST /api/billing/create-payment-intent
    Note over Web,GW: { amount, currency, plan }
    
    GW->>Billing: POST /api/billing/create-payment-intent
    
    Billing->>Stripe: Create Payment Intent
    Note over Billing,Stripe: Stripe.PaymentIntentService.CreateAsync()
    
    Stripe-->>Billing: Payment Intent (client_secret)
    Billing-->>GW: 200 OK + client_secret
    GW-->>Web: client_secret
    
    Web->>Web: Mostrar Stripe Elements
    User->>Web: Ingresa datos tarjeta
    
    Web->>Stripe: Confirmar pago (frontend)
    Note over Web,Stripe: stripe.confirmCardPayment(client_secret)
    
    Stripe->>Stripe: Procesar pago
    
    alt Pago Exitoso
        Stripe->>Billing: Webhook: payment_intent.succeeded
        Note over Stripe,Billing: POST /api/billing/webhook/stripe
        
        Billing->>Billing: Verificar firma Stripe
        Note over Billing: StripeConfiguration.SetApiKey()
        
        Billing->>Billing: Actualizar BD (Payment table)
        Note over Billing: Status = Completed<br/>PaidAt = DateTime.UtcNow
        
        Billing->>RMQ: PublishAsync(PaymentCompletedEvent)
        Note over Billing,RMQ: Routing Key: payment.completed<br/>Exchange: cardealer.events<br/>Payload: PaymentId, Amount, Currency, UserEmail
        
        RMQ-->>Notify: Enruta a Queue
        Note over RMQ,Notify: Queue: notificationservice.payment.completed
        
        Notify->>Notify: PaymentReceiptNotificationConsumer<br/>procesa evento
        
        Notify->>Notify: Genera recibo HTML detallado
        Note over Notify: Tabla con:<br/>- ID Pago<br/>- Monto + Currency<br/>- Fecha<br/>- Plan<br/>- Stripe Payment Intent ID
        
        Notify->>SG: SendEmailAsync()
        Note over Notify,SG: To: user@email.com<br/>Subject: Recibo de Pago - $99.00 USD
        
        SG->>User: 📧 Recibo detallado
        
        Billing-->>Stripe: 200 OK (Webhook procesado)
    end
    
    Stripe-->>Web: Payment Success (frontend)
    Web->>Web: Redirect a /billing/success
    
    Note over Web,User: Usuario ve confirmación<br/>+ recibe email de recibo
```

### Datos Sincronizados

| Campo | Origen (BillingService) | Destino (NotificationService) | Uso en Email |
|-------|------------------------|-------------------------------|--------------|
| PaymentId | `payment.Id` | `eventData.PaymentId` | **ID en tabla** |
| UserId | `payment.UserId` | `eventData.UserId` | Tracking |
| UserEmail | `user.Email` | `eventData.UserEmail` | **Recipient** |
| UserName | `user.FullName` | `eventData.UserName` | **Saludo** |
| Amount | `payment.Amount` | `eventData.Amount` | **Monto en tabla + Subject** |
| Currency | `payment.Currency` | `eventData.Currency` | **Currency en tabla** |
| Description | `payment.Description` | `eventData.Description` | Descripción del pago |
| SubscriptionPlan | `payment.PlanName` | `eventData.SubscriptionPlan` | **Mostrar plan si aplica** |
| StripePaymentIntentId | `payment.StripePaymentIntentId` | `eventData.StripePaymentIntentId` | **ID Stripe en tabla** |
| PaidAt | `payment.PaidAt` | `eventData.PaidAt` | **Fecha en tabla** |

---

## 🐰 ARQUITECTURA RABBITMQ

### Exchange y Queues

```mermaid
graph LR
    subgraph "Publishers (Producers)"
        AUTH[AuthService]
        VEHICLES[VehiclesSaleService]
        BILLING[BillingService]
        ERROR[ErrorService]
    end

    subgraph "RabbitMQ Exchange"
        EXCHANGE[cardealer.events<br/>Type: Topic<br/>Durable: true]
    end

    subgraph "Queues"
        Q1[notificationservice.user.registered<br/>Durable: true]
        Q2[notificationservice.vehicle.created<br/>Durable: true]
        Q3[notificationservice.payment.completed<br/>Durable: true]
        Q4[notification.error.critical<br/>Durable: true]
        Q5[audit.all-events<br/>Durable: true]
        Q6[error-queue<br/>Durable: true]
    end

    subgraph "Consumers (Subscribers)"
        C1[UserRegisteredNotificationConsumer<br/>NotificationService]
        C2[VehicleCreatedNotificationConsumer<br/>NotificationService]
        C3[PaymentReceiptNotificationConsumer<br/>NotificationService]
        C4[ErrorCriticalEventConsumer<br/>NotificationService]
        C5[AuditConsumer<br/>AuditService]
        C6[ErrorConsumer<br/>ErrorService]
    end

    AUTH -->|auth.user.registered| EXCHANGE
    VEHICLES -->|vehicle.created| EXCHANGE
    BILLING -->|payment.completed| EXCHANGE
    ERROR -->|error.critical| EXCHANGE

    EXCHANGE -->|Routing| Q1
    EXCHANGE -->|Routing| Q2
    EXCHANGE -->|Routing| Q3
    EXCHANGE -->|Routing| Q4
    EXCHANGE -->|Routing| Q5
    EXCHANGE -->|Routing| Q6

    Q1 --> C1
    Q2 --> C2
    Q3 --> C3
    Q4 --> C4
    Q5 --> C5
    Q6 --> C6

    style EXCHANGE fill:#ff9999,stroke:#333,stroke-width:3px
    style Q1 fill:#99ff99,stroke:#333,stroke-width:2px
    style Q2 fill:#99ff99,stroke:#333,stroke-width:2px
    style Q3 fill:#99ff99,stroke:#333,stroke-width:2px
```

### Routing Keys y Bindings

| Publisher | Routing Key | Queue | Consumer | Servicio |
|-----------|------------|-------|----------|----------|
| AuthService | `auth.user.registered` | `notificationservice.user.registered` | UserRegisteredNotificationConsumer | NotificationService |
| VehiclesSaleService | `vehicle.created` | `notificationservice.vehicle.created` | VehicleCreatedNotificationConsumer | NotificationService |
| BillingService | `payment.completed` | `notificationservice.payment.completed` | PaymentReceiptNotificationConsumer | NotificationService |
| ErrorService | `error.critical` | `notification.error.critical` | ErrorCriticalEventConsumer | NotificationService |
| *Todos* | `*.*.#` | `audit.all-events` | AuditConsumer | AuditService |
| ErrorService | `error.logged` | `error-queue` | ErrorConsumer | ErrorService |

---

## 📡 MATRIZ DE COMUNICACIÓN ENTRE SERVICIOS

### Comunicación Síncrona (HTTP via Gateway)

| Servicio Origen | Servicio Destino | Endpoint | Método | Propósito |
|----------------|------------------|----------|--------|-----------|
| Web Frontend | AuthService | `/api/auth/login` | POST | Autenticación |
| Web Frontend | AuthService | `/api/auth/register` | POST | Registro |
| Web Frontend | UserService | `/api/users/{id}` | GET | Obtener perfil |
| Web Frontend | VehiclesSaleService | `/api/vehicles` | GET/POST | CRUD vehículos |
| Web Frontend | BillingService | `/api/billing/payment-intent` | POST | Crear pago |
| AuthService | UserService | N/A | - | ❌ **NO DIRECTA** |
| VehiclesSaleService | MediaService | N/A | - | ❌ **NO DIRECTA** |

**🚨 REGLA:** Ningún microservicio llama directamente a otro. **SIEMPRE via Gateway o RabbitMQ.**

### Comunicación Asíncrona (RabbitMQ Events)

| Servicio Origen | Evento Publicado | Servicio Destino | Acción |
|----------------|------------------|------------------|--------|
| AuthService | `UserRegisteredEvent` | NotificationService | Envía email bienvenida |
| VehiclesSaleService | `VehicleCreatedEvent` | NotificationService | Notifica al dealer |
| BillingService | `PaymentCompletedEvent` | NotificationService | Envía recibo |
| *Todos* | `ErrorLoggedEvent` | ErrorService | Centraliza errores |
| *Todos* | Todos los eventos | AuditService | Audita todas las acciones |

---

## 🔗 DEPENDENCIAS ENTRE SERVICIOS

### Diagrama de Dependencias

```mermaid
graph TD
    subgraph "Capa de Presentación"
        WEB[Web Frontend]
        MOBILE[Mobile App]
    end

    subgraph "Capa de Gateway"
        GATEWAY[Gateway<br/>Ocelot]
    end

    subgraph "Capa de Negocio"
        AUTH[AuthService]
        USER[UserService]
        VEHICLES[VehiclesSaleService]
        BILLING[BillingService]
    end

    subgraph "Capa de Infraestructura"
        NOTIFY[NotificationService]
        ERROR[ErrorService]
        MEDIA[MediaService]
        AUDIT[AuditService]
    end

    subgraph "Capa de Datos"
        RABBIT[RabbitMQ<br/>Message Broker]
        REDIS[Redis<br/>Cache]
        DBS[(PostgreSQL<br/>Databases)]
    end

    WEB --> GATEWAY
    MOBILE --> GATEWAY
    
    GATEWAY --> AUTH
    GATEWAY --> USER
    GATEWAY --> VEHICLES
    GATEWAY --> BILLING
    
    AUTH -.events.-> RABBIT
    USER -.events.-> RABBIT
    VEHICLES -.events.-> RABBIT
    BILLING -.events.-> RABBIT
    
    RABBIT -.events.-> NOTIFY
    RABBIT -.events.-> ERROR
    RABBIT -.events.-> AUDIT
    
    AUTH --> DBS
    USER --> DBS
    VEHICLES --> DBS
    BILLING --> DBS
    NOTIFY --> DBS
    
    AUTH --> REDIS
    VEHICLES --> REDIS
    
    MEDIA -.storage.-> S3[AWS S3]

    style RABBIT fill:#ff9999,stroke:#333,stroke-width:3px
    style GATEWAY fill:#99ccff,stroke:#333,stroke-width:2px
    style REDIS fill:#ffcc99,stroke:#333,stroke-width:2px
```

### Tabla de Dependencias

| Servicio | Depende de | Tipo de Dependencia | Crítico |
|----------|-----------|---------------------|---------|
| **Gateway** | Consul | Service Discovery | ✅ Sí |
| **AuthService** | PostgreSQL | Base de datos | ✅ Sí |
| **AuthService** | Redis | Cache (opcional) | ❌ No |
| **AuthService** | RabbitMQ | Message Broker | ❌ No |
| **UserService** | PostgreSQL | Base de datos | ✅ Sí |
| **VehiclesSaleService** | PostgreSQL | Base de datos | ✅ Sí |
| **VehiclesSaleService** | Redis | Cache | ❌ No |
| **VehiclesSaleService** | RabbitMQ | Event Publishing | ❌ No |
| **BillingService** | PostgreSQL | Base de datos | ✅ Sí |
| **BillingService** | Stripe API | Payment Gateway | ✅ Sí |
| **BillingService** | RabbitMQ | Event Publishing | ❌ No |
| **NotificationService** | PostgreSQL | Base de datos | ✅ Sí |
| **NotificationService** | RabbitMQ | Event Consuming | ✅ Sí |
| **NotificationService** | SendGrid API | Email Provider | ✅ Sí |
| **NotificationService** | Twilio API | SMS Provider | ❌ No |
| **NotificationService** | Firebase | Push Notifications | ❌ No |
| **MediaService** | PostgreSQL | Metadata storage | ✅ Sí |
| **MediaService** | AWS S3 | File storage | ✅ Sí |
| **ErrorService** | PostgreSQL | Error logs | ✅ Sí |
| **ErrorService** | RabbitMQ | Event Consuming | ❌ No |
| **AuditService** | PostgreSQL | Audit logs | ✅ Sí |
| **AuditService** | RabbitMQ | Event Consuming | ✅ Sí |

**Leyenda:**
- ✅ **Crítico:** El servicio NO funciona sin esta dependencia
- ❌ **No Crítico:** El servicio puede funcionar sin esta dependencia (degraded mode)

---

## 🌊 FLUJO DE DATOS COMPLETO

### Ejemplo: Usuario compra plan Premium

```mermaid
sequenceDiagram
    autonumber
    participant U as Usuario
    participant W as Web Frontend
    participant GW as Gateway
    participant A as AuthService
    participant B as BillingService
    participant RMQ as RabbitMQ
    participant N as NotificationService
    participant S as SendGrid
    participant ST as Stripe

    U->>W: Selecciona "Plan Premium"
    W->>GW: POST /api/auth/register
    GW->>A: POST /api/auth/register
    A->>A: Crear cuenta
    A->>RMQ: UserRegisteredEvent
    RMQ-->>N: Queue: user.registered
    N->>S: Welcome Email
    S->>U: 📧 Bienvenida
    A-->>GW: 201 + JWT Token
    GW-->>W: Token
    W->>W: Store token + redirect
    
    W->>GW: POST /api/billing/create-payment-intent
    Note over W,GW: { amount: 99.00, plan: "Premium" }
    GW->>B: POST /api/billing/create-payment-intent
    B->>ST: Create Payment Intent
    ST-->>B: client_secret
    B-->>GW: client_secret
    GW-->>W: client_secret
    
    W->>W: Mostrar Stripe Elements
    U->>W: Ingresa tarjeta
    W->>ST: Confirmar pago
    ST->>ST: Procesar
    
    ST->>B: Webhook: payment_intent.succeeded
    B->>B: Actualizar BD
    B->>RMQ: PaymentCompletedEvent
    RMQ-->>N: Queue: payment.completed
    N->>S: Payment Receipt
    S->>U: 📧 Recibo
    
    ST-->>W: Success
    W->>W: Redirect /billing/success
    
    Note over U,ST: Usuario tiene:<br/>✅ Cuenta creada<br/>✅ Email bienvenida<br/>✅ Plan Premium activo<br/>✅ Recibo de pago
```

---

## 📊 ESTADÍSTICAS DE COMUNICACIÓN

### Comunicación HTTP (Síncrona)

| Ruta | Servicio | Avg Response Time | QPS Esperado | Cacheado |
|------|----------|-------------------|--------------|----------|
| `/api/auth/login` | AuthService | 50-100ms | Alto (500-1000) | ✅ Redis |
| `/api/vehicles` | VehiclesSaleService | 30-80ms | Muy Alto (2000+) | ✅ Redis |
| `/api/billing/*` | BillingService | 100-200ms | Medio (100-300) | ❌ No |
| `/api/users/*` | UserService | 40-90ms | Alto (800-1200) | ✅ Redis |
| `/api/notifications/*` | NotificationService | 50-120ms | Bajo (50-100) | ❌ No |

### Comunicación RabbitMQ (Asíncrona)

| Evento | Publisher | Consumers | Frecuencia | Prioridad |
|--------|-----------|-----------|------------|-----------|
| `auth.user.registered` | AuthService | 1 | Baja (~10/día) | Alta |
| `vehicle.created` | VehiclesSaleService | 1 | Media (~50/día) | Media |
| `payment.completed` | BillingService | 1 | Media (~30/día) | Alta |
| `error.critical` | ErrorService | 1 | Baja (~5/día) | Crítica |
| `*.*.#` (todos) | Múltiples | 1 (Audit) | Alta (~1000/día) | Baja |

---

## 🔐 FLUJO DE AUTENTICACIÓN Y AUTORIZACIÓN

```mermaid
sequenceDiagram
    participant U as Usuario
    participant W as Web Frontend
    participant GW as Gateway
    participant A as AuthService
    participant V as VehiclesSaleService
    participant DB as PostgreSQL

    U->>W: Login
    W->>GW: POST /api/auth/login
    GW->>A: POST /api/auth/login
    A->>DB: Validar credenciales
    DB-->>A: Usuario encontrado
    A->>A: Generar JWT Token
    Note over A: Claims:<br/>- UserId<br/>- Email<br/>- DealerId<br/>- Roles
    A-->>GW: 200 OK + JWT Token
    GW-->>W: Token
    W->>W: Store en localStorage
    
    rect rgb(200, 220, 250)
        Note over U,DB: Usuario autenticado - Realizando operaciones
        
        U->>W: Crear vehículo
        W->>GW: POST /api/vehicles
        Note over W,GW: Authorization: Bearer {token}
        
        GW->>GW: Validar JWT signature
        Note over GW: Verifica con Jwt:SecretKey
        
        alt JWT válido
            GW->>V: POST /api/vehicles<br/>+ UserId, DealerId (claims)
            V->>V: Verificar permisos
            Note over V: ¿User es Dealer?<br/>¿Tiene plan activo?
            
            V->>DB: INSERT vehicle
            DB-->>V: Vehicle creado
            V-->>GW: 201 Created
            GW-->>W: 201 Created
            W->>U: ✅ Éxito
        else JWT inválido o expirado
            GW-->>W: 401 Unauthorized
            W->>W: Redirect a /login
        end
    end
```

---

## 🎯 PUNTOS CLAVE DE LA ARQUITECTURA

### ✅ Ventajas del Diseño Actual

1. **Desacoplamiento:**
   - Servicios no se llaman directamente entre sí
   - Cambios en un servicio no afectan a otros
   - Facilita escalabilidad independiente

2. **Resiliencia:**
   - Si NotificationService cae, AuthService sigue funcionando
   - RabbitMQ persiste mensajes si consumer está down
   - Retries automáticos configurables

3. **Observabilidad:**
   - Todos los eventos pasan por RabbitMQ (punto central)
   - AuditService recibe TODOS los eventos para logging
   - ErrorService centraliza errores de todos los servicios

4. **Escalabilidad:**
   - Cada consumer puede escalar independientemente
   - Múltiples instancias de NotificationService pueden consumir la misma queue
   - Load balancing automático por RabbitMQ

### ⚠️ Puntos a Considerar

1. **Latencia:**
   - Eventos asíncronos NO son inmediatos (1-5 segundos típicamente)
   - No usar para operaciones que requieren respuesta instantánea

2. **Orden de Eventos:**
   - RabbitMQ NO garantiza orden entre queues diferentes
   - Si el orden importa, usar single queue o añadir sequence numbers

3. **Idempotencia:**
   - Consumers deben ser idempotentes (procesar mismo evento múltiples veces sin problemas)
   - Usar IDs únicos de evento para tracking

4. **Monitoreo:**
   - Crítico monitorear:
     - Mensajes en cola (no debe crecer indefinidamente)
     - Consumer lag (tiempo entre publish y consume)
     - Dead Letter Queue (mensajes fallidos)

---

## 🧪 CÓMO PROBAR LAS SINCRONIZACIONES

### 1. Test Welcome Email

```bash
# 1. Registrar usuario
curl -X POST http://localhost:18443/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Password123!",
    "fullName": "Test User"
  }'

# 2. Verificar logs
docker logs authservice | grep "UserRegisteredEvent"
docker logs notificationservice | grep "Welcome email sent"

# 3. Verificar queue en RabbitMQ
docker exec rabbitmq rabbitmqctl list_queues | grep user.registered
```

### 2. Test Vehicle Notification

```bash
# 1. Crear vehículo (necesitas token JWT primero)
curl -X POST http://localhost:18443/api/vehicles \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "make": "Toyota",
    "model": "Camry",
    "year": 2024,
    "vin": "1HGCM82633A123456",
    "price": 32000
  }'

# 2. Verificar logs
docker logs vehiclessaleservice | grep "VehicleCreatedEvent"
docker logs notificationservice | grep "Vehicle creation notification"

# 3. Verificar queue
docker exec rabbitmq rabbitmqctl list_queues | grep vehicle.created
```

### 3. Test Payment Receipt

```bash
# 1. Simular webhook de Stripe (en desarrollo)
curl -X POST http://localhost:18443/api/billing/webhook/stripe \
  -H "Content-Type: application/json" \
  -d '{
    "type": "payment_intent.succeeded",
    "data": {
      "object": {
        "id": "pi_test123",
        "amount": 9900,
        "currency": "usd"
      }
    }
  }'

# 2. Verificar logs
docker logs billingservice | grep "PaymentCompletedEvent"
docker logs notificationservice | grep "Payment receipt email sent"
```

---

## 📚 CONCLUSIÓN

Esta arquitectura **event-driven** con RabbitMQ permite:

✅ **Desacoplamiento total** entre servicios  
✅ **Sincronizaciones automáticas** sin necesidad de polling  
✅ **Escalabilidad horizontal** independiente por servicio  
✅ **Resiliencia** ante fallos temporales  
✅ **Auditoría completa** de todas las operaciones  
✅ **Extensibilidad** fácil para agregar nuevos consumers  

**Próximo paso:** Testing exhaustivo de las 3 sincronizaciones implementadas 🧪

---

**Documentado por:** GitHub Copilot AI Assistant  
**Fecha:** 6 Enero 2026  
**Actualizado:** Tras implementación Sprint 0.8
