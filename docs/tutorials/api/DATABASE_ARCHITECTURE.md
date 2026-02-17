# 🗄️ Arquitectura de Base de Datos - CarDealer

Este documento describe la estructura de bases de datos de cada microservicio en la plataforma CarDealer.

---

## 📋 Tabla de Contenidos

1. [Visión General](#visión-general)
2. [AuthService Database](#authservice-database)
3. [UserService Database](#userservice-database)
4. [VehiclesSaleService Database](#vehiclessaleservice-database)
5. [BillingService Database](#billingservice-database)
6. [ContactService Database](#contactservice-database)
7. [NotificationService Database](#notificationservice-database)
8. [MediaService Database](#mediaservice-database)
9. [DealerManagementService Database](#dealermanagementservice-database)
10. [SearchService (Elasticsearch)](#searchservice-elasticsearch)
11. [Relaciones Entre Servicios](#relaciones-entre-servicios)

---

## Visión General

CarDealer utiliza **PostgreSQL 16** como base de datos principal para la mayoría de los servicios, con bases de datos separadas por microservicio siguiendo el patrón **Database per Service**.

### Infraestructura de Datos

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         INFRAESTRUCTURA DE DATOS                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌───────────────┐  ┌───────────────┐  ┌───────────────┐                   │
│  │  PostgreSQL   │  │    Redis      │  │ Elasticsearch │                   │
│  │   (Primary)   │  │   (Cache)     │  │   (Search)    │                   │
│  └───────────────┘  └───────────────┘  └───────────────┘                   │
│         │                   │                   │                           │
│         ▼                   ▼                   ▼                           │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                        MICROSERVICIOS                               │   │
│  ├─────────────────────────────────────────────────────────────────────┤   │
│  │  AuthService      → auth_db                                         │   │
│  │  UserService      → user_db                                         │   │
│  │  VehiclesSale     → vehicles_db                                     │   │
│  │  BillingService   → billing_db                                      │   │
│  │  ContactService   → contact_db                                      │   │
│  │  NotificationSvc  → notification_db                                 │   │
│  │  MediaService     → media_db                                        │   │
│  │  DealerMgmt       → dealer_db                                       │   │
│  │  SearchService    → elasticsearch index                             │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Connection Strings (Desarrollo)

```json
{
  "ConnectionStrings": {
    "AuthDb": "Host=postgres;Database=auth_db;Username=cardealer;Password=xxx",
    "UserDb": "Host=postgres;Database=user_db;Username=cardealer;Password=xxx",
    "VehiclesDb": "Host=postgres;Database=vehicles_db;Username=cardealer;Password=xxx",
    "BillingDb": "Host=postgres;Database=billing_db;Username=cardealer;Password=xxx",
    "Redis": "redis:6379"
  }
}
```

---

## AuthService Database

Base de datos para autenticación y gestión de identidades.

### Diagrama ER

```
┌─────────────────────────┐       ┌─────────────────────────┐
│     AspNetUsers         │       │   AspNetUserRoles       │
├─────────────────────────┤       ├─────────────────────────┤
│ Id (PK)                 │──────<│ UserId (FK)             │
│ Email                   │       │ RoleId (FK)             │
│ PasswordHash            │       └─────────────────────────┘
│ FullName                │                    │
│ PhoneNumber             │       ┌────────────┘
│ EmailConfirmed          │       │
│ TwoFactorEnabled        │       ▼
│ AccountType             │  ┌─────────────────────────┐
│ LockoutEnd              │  │     AspNetRoles         │
│ AccessFailedCount       │  ├─────────────────────────┤
│ CreatedAt               │  │ Id (PK)                 │
│ LastLoginAt             │  │ Name                    │
└─────────────────────────┘  │ NormalizedName          │
         │                   └─────────────────────────┘
         │
         ▼
┌─────────────────────────┐       ┌─────────────────────────┐
│    RefreshTokens        │       │  VerificationTokens     │
├─────────────────────────┤       ├─────────────────────────┤
│ Id (PK)                 │       │ Id (PK)                 │
│ UserId (FK)             │       │ UserId (FK)             │
│ Token                   │       │ Token                   │
│ ExpiresAt               │       │ Type                    │
│ CreatedAt               │       │ ExpiresAt               │
│ RevokedAt               │       │ UsedAt                  │
│ ReplacedByToken         │       │ CreatedAt               │
│ DeviceInfo              │       └─────────────────────────┘
└─────────────────────────┘
         │
         ▼
┌─────────────────────────┐
│    TwoFactorAuth        │
├─────────────────────────┤
│ Id (PK)                 │
│ UserId (FK)             │
│ SecretKey               │
│ IsEnabled               │
│ RecoveryCodes           │
│ CreatedAt               │
└─────────────────────────┘
```

### Tablas Principales

#### `AspNetUsers`

| Columna             | Tipo         | Descripción                    |
| ------------------- | ------------ | ------------------------------ |
| `Id`                | UUID         | Identificador único            |
| `Email`             | VARCHAR(256) | Email del usuario              |
| `NormalizedEmail`   | VARCHAR(256) | Email normalizado (mayúsculas) |
| `PasswordHash`      | TEXT         | Hash de contraseña (BCrypt)    |
| `FullName`          | VARCHAR(200) | Nombre completo                |
| `PhoneNumber`       | VARCHAR(20)  | Teléfono                       |
| `EmailConfirmed`    | BOOLEAN      | Email verificado               |
| `TwoFactorEnabled`  | BOOLEAN      | 2FA habilitado                 |
| `AccountType`       | VARCHAR(50)  | Individual, Dealer, Admin      |
| `LockoutEnd`        | TIMESTAMP    | Fin de bloqueo                 |
| `AccessFailedCount` | INT          | Intentos fallidos              |
| `CreatedAt`         | TIMESTAMP    | Fecha de creación              |
| `LastLoginAt`       | TIMESTAMP    | Último login                   |

#### `RefreshTokens`

| Columna           | Tipo         | Descripción          |
| ----------------- | ------------ | -------------------- |
| `Id`              | UUID         | Identificador único  |
| `UserId`          | UUID (FK)    | Usuario propietario  |
| `Token`           | VARCHAR(500) | Token de refresh     |
| `ExpiresAt`       | TIMESTAMP    | Expiración           |
| `CreatedAt`       | TIMESTAMP    | Creación             |
| `RevokedAt`       | TIMESTAMP    | Si fue revocado      |
| `ReplacedByToken` | VARCHAR(500) | Token de reemplazo   |
| `DeviceInfo`      | JSONB        | Info del dispositivo |

---

## UserService Database

Gestión de perfiles de usuario, vendedores y dealers.

### Diagrama ER

```
┌─────────────────────────┐       ┌─────────────────────────┐
│        Users            │       │      SellerProfiles     │
├─────────────────────────┤       ├─────────────────────────┤
│ Id (PK)                 │──────<│ Id (PK)                 │
│ AuthUserId (FK)         │       │ UserId (FK)             │
│ Email                   │       │ DisplayName             │
│ FullName                │       │ Bio                     │
│ PhoneNumber             │       │ Rating                  │
│ AvatarUrl               │       │ TotalSales              │
│ AccountType             │       │ ResponseTime            │
│ IsVerified              │       │ IsVerified              │
│ Preferences             │       │ CreatedAt               │
│ CreatedAt               │       └─────────────────────────┘
│ UpdatedAt               │
└─────────────────────────┘
         │
         │ (Si es Dealer)
         ▼
┌─────────────────────────┐       ┌─────────────────────────┐
│       Dealers           │       │   DealerSubscriptions   │
├─────────────────────────┤       ├─────────────────────────┤
│ Id (PK)                 │──────<│ Id (PK)                 │
│ UserId (FK)             │       │ DealerId (FK)           │
│ BusinessName            │       │ PlanId                  │
│ LegalName               │       │ Status                  │
│ RNC                     │       │ CurrentPeriodStart      │
│ DealerType              │       │ CurrentPeriodEnd        │
│ Status                  │       │ CancelAtPeriodEnd       │
│ VerificationStatus      │       │ StripeSubscriptionId    │
│ CurrentPlan             │       │ CreatedAt               │
│ MaxActiveListings       │       └─────────────────────────┘
│ CreatedAt               │
└─────────────────────────┘
         │
         ▼
┌─────────────────────────┐       ┌─────────────────────────┐
│    DealerEmployees      │       │     ModuleAddons        │
├─────────────────────────┤       ├─────────────────────────┤
│ Id (PK)                 │       │ Id (PK)                 │
│ DealerId (FK)           │       │ DealerId (FK)           │
│ UserId (FK)             │       │ ModuleName              │
│ Role                    │       │ IsActive                │
│ Permissions             │       │ ActivatedAt             │
│ InvitedAt               │       │ ExpiresAt               │
│ AcceptedAt              │       │ Price                   │
│ Status                  │       └─────────────────────────┘
└─────────────────────────┘
```

### Tablas Principales

#### `Users`

| Columna       | Tipo         | Descripción              |
| ------------- | ------------ | ------------------------ |
| `Id`          | UUID         | Identificador único      |
| `AuthUserId`  | UUID         | ID en AuthService        |
| `Email`       | VARCHAR(256) | Email                    |
| `FullName`    | VARCHAR(200) | Nombre completo          |
| `PhoneNumber` | VARCHAR(20)  | Teléfono                 |
| `AvatarUrl`   | VARCHAR(500) | URL de avatar            |
| `AccountType` | VARCHAR(50)  | Tipo de cuenta           |
| `IsVerified`  | BOOLEAN      | Usuario verificado       |
| `Preferences` | JSONB        | Preferencias del usuario |
| `CreatedAt`   | TIMESTAMP    | Creación                 |
| `UpdatedAt`   | TIMESTAMP    | Última actualización     |

#### `Dealers`

| Columna              | Tipo         | Descripción                |
| -------------------- | ------------ | -------------------------- |
| `Id`                 | UUID         | Identificador único        |
| `UserId`             | UUID (FK)    | Usuario propietario        |
| `BusinessName`       | VARCHAR(200) | Nombre comercial           |
| `LegalName`          | VARCHAR(200) | Razón social               |
| `RNC`                | VARCHAR(15)  | RNC                        |
| `DealerType`         | VARCHAR(50)  | Independent, Chain, etc    |
| `Status`             | VARCHAR(50)  | Pending, Active, Suspended |
| `VerificationStatus` | VARCHAR(50)  | NotVerified, Verified, etc |
| `CurrentPlan`        | VARCHAR(50)  | Starter, Pro, Enterprise   |
| `MaxActiveListings`  | INT          | Límite de publicaciones    |
| `Address`            | VARCHAR(500) | Dirección                  |
| `City`               | VARCHAR(100) | Ciudad                     |
| `Province`           | VARCHAR(100) | Provincia                  |
| `CreatedAt`          | TIMESTAMP    | Creación                   |

---

## VehiclesSaleService Database

Gestión de vehículos, catálogo y secciones del homepage.

### Diagrama ER

```
┌─────────────────────────┐       ┌─────────────────────────┐
│      VehicleMakes       │       │     VehicleModels       │
├─────────────────────────┤       ├─────────────────────────┤
│ Id (PK)                 │──────<│ Id (PK)                 │
│ Name                    │       │ MakeId (FK)             │
│ LogoUrl                 │       │ Name                    │
│ IsActive                │       │ IsActive                │
└─────────────────────────┘       └─────────────────────────┘
                                           │
                                           ▼
┌─────────────────────────┐       ┌─────────────────────────┐
│       Categories        │       │       Vehicles          │
├─────────────────────────┤       ├─────────────────────────┤
│ Id (PK)                 │       │ Id (PK)                 │
│ Name                    │──────<│ CategoryId (FK)         │
│ Slug                    │       │ MakeId (FK)             │
│ IconUrl                 │       │ ModelId (FK)            │
└─────────────────────────┘       │ SellerId (FK)           │
                                  │ Title                   │
                                  │ Slug                    │
                                  │ Description             │
                                  │ Price                   │
                                  │ Currency                │
                                  │ Year                    │
                                  │ Mileage                 │
                                  │ FuelType                │
                                  │ Transmission            │
                                  │ Color                   │
                                  │ Condition               │
                                  │ Status                  │
                                  │ Location (JSONB)        │
                                  │ Features (JSONB)        │
                                  │ ViewCount               │
                                  │ FavoriteCount           │
                                  │ CreatedAt               │
                                  │ UpdatedAt               │
                                  │ ExpiresAt               │
                                  └─────────────────────────┘
                                           │
              ┌────────────────────────────┼────────────────────────────┐
              │                            │                            │
              ▼                            ▼                            ▼
┌─────────────────────────┐  ┌─────────────────────────┐  ┌─────────────────────────┐
│     VehicleImages       │  │       Favorites         │  │  VehicleHomepageSections│
├─────────────────────────┤  ├─────────────────────────┤  ├─────────────────────────┤
│ Id (PK)                 │  │ Id (PK)                 │  │ Id (PK)                 │
│ VehicleId (FK)          │  │ UserId                  │  │ VehicleId (FK)          │
│ Url                     │  │ VehicleId (FK)          │  │ SectionConfigId (FK)    │
│ ThumbnailUrl            │  │ Notes                   │  │ SortOrder               │
│ IsPrimary               │  │ NotifyOnPriceChange     │  │ IsPinned                │
│ Order                   │  │ CreatedAt               │  │ StartDate               │
│ CreatedAt               │  └─────────────────────────┘  │ EndDate                 │
└─────────────────────────┘                               └─────────────────────────┘
                                                                      │
                                                                      ▼
                                                         ┌─────────────────────────┐
                                                         │ HomepageSectionConfigs  │
                                                         ├─────────────────────────┤
                                                         │ Id (PK)                 │
                                                         │ Name                    │
                                                         │ Slug                    │
                                                         │ DisplayOrder            │
                                                         │ MaxItems                │
                                                         │ IsActive                │
                                                         │ AccentColor             │
                                                         │ ViewAllHref             │
                                                         └─────────────────────────┘
```

### Tablas Principales

#### `Vehicles`

| Columna         | Tipo          | Descripción                   |
| --------------- | ------------- | ----------------------------- |
| `Id`            | UUID          | Identificador único           |
| `SellerId`      | UUID          | ID del vendedor (UserService) |
| `Title`         | VARCHAR(200)  | Título de la publicación      |
| `Slug`          | VARCHAR(250)  | URL-friendly slug             |
| `Description`   | TEXT          | Descripción detallada         |
| `MakeId`        | INT (FK)      | Marca                         |
| `ModelId`       | INT (FK)      | Modelo                        |
| `CategoryId`    | INT (FK)      | Categoría                     |
| `Year`          | INT           | Año del vehículo              |
| `Price`         | DECIMAL(18,2) | Precio                        |
| `Currency`      | VARCHAR(3)    | DOP, USD                      |
| `Mileage`       | INT           | Kilometraje                   |
| `FuelType`      | VARCHAR(50)   | Tipo de combustible           |
| `Transmission`  | VARCHAR(50)   | Tipo de transmisión           |
| `Color`         | VARCHAR(50)   | Color exterior                |
| `InteriorColor` | VARCHAR(50)   | Color interior                |
| `EngineSize`    | VARCHAR(20)   | Tamaño de motor               |
| `Cylinders`     | INT           | Cilindros                     |
| `Doors`         | INT           | Número de puertas             |
| `Seats`         | INT           | Número de asientos            |
| `Drivetrain`    | VARCHAR(20)   | FWD, RWD, AWD, 4WD            |
| `VIN`           | VARCHAR(17)   | Número de identificación      |
| `Condition`     | VARCHAR(20)   | New, Used, Certified          |
| `Status`        | VARCHAR(50)   | Draft, Active, Sold, etc      |
| `Location`      | JSONB         | {city, province, address}     |
| `Features`      | JSONB         | Array de características      |
| `ViewCount`     | INT           | Contador de vistas            |
| `FavoriteCount` | INT           | Contador de favoritos         |
| `CreatedAt`     | TIMESTAMP     | Fecha de creación             |
| `UpdatedAt`     | TIMESTAMP     | Última actualización          |
| `ExpiresAt`     | TIMESTAMP     | Fecha de expiración           |

#### `HomepageSectionConfigs`

| Columna        | Tipo         | Descripción                |
| -------------- | ------------ | -------------------------- |
| `Id`           | UUID         | Identificador único        |
| `Name`         | VARCHAR(100) | Nombre de la sección       |
| `Slug`         | VARCHAR(100) | Identificador URL-friendly |
| `DisplayOrder` | INT          | Orden de visualización     |
| `MaxItems`     | INT          | Máximo de items a mostrar  |
| `IsActive`     | BOOLEAN      | Sección activa             |
| `Subtitle`     | VARCHAR(200) | Subtítulo                  |
| `AccentColor`  | VARCHAR(50)  | Color de acento            |
| `ViewAllHref`  | VARCHAR(200) | Link "Ver todos"           |

---

## BillingService Database

Gestión de pagos, suscripciones y facturación.

### Diagrama ER

```
┌─────────────────────────┐       ┌─────────────────────────┐
│    StripeCustomers      │       │     Subscriptions       │
├─────────────────────────┤       ├─────────────────────────┤
│ Id (PK)                 │──────<│ Id (PK)                 │
│ UserId                  │       │ CustomerId (FK)         │
│ StripeCustomerId        │       │ StripeSubscriptionId    │
│ Email                   │       │ PlanId                  │
│ DefaultPaymentMethodId  │       │ Status                  │
│ CreatedAt               │       │ CurrentPeriodStart      │
└─────────────────────────┘       │ CurrentPeriodEnd        │
         │                        │ CancelAtPeriodEnd       │
         │                        │ CanceledAt              │
         ▼                        │ CreatedAt               │
┌─────────────────────────┐       └─────────────────────────┘
│       Payments          │                 │
├─────────────────────────┤                 │
│ Id (PK)                 │                 ▼
│ CustomerId (FK)         │  ┌─────────────────────────┐
│ SubscriptionId (FK)     │  │       Invoices          │
│ StripePaymentIntentId   │  ├─────────────────────────┤
│ Amount                  │  │ Id (PK)                 │
│ Currency                │  │ SubscriptionId (FK)     │
│ Status                  │  │ StripeInvoiceId         │
│ PaymentMethod           │  │ Amount                  │
│ Description             │  │ Currency                │
│ Metadata                │  │ Status                  │
│ CreatedAt               │  │ PdfUrl                  │
│ CompletedAt             │  │ DueDate                 │
└─────────────────────────┘  │ PaidAt                  │
                             │ CreatedAt               │
                             └─────────────────────────┘

┌─────────────────────────┐       ┌─────────────────────────┐
│   EarlyBirdMembers      │       │   AzulTransactions      │
├─────────────────────────┤       ├─────────────────────────┤
│ Id (PK)                 │       │ Id (PK)                 │
│ UserId                  │       │ CustomerId (FK)         │
│ Email                   │       │ AzulOrderId             │
│ EnrolledAt              │       │ Amount                  │
│ FreeMonthsRemaining     │       │ Currency                │
│ DiscountPercentage      │       │ Status                  │
│ BadgeType               │       │ AuthorizationCode       │
│ PlanAtEnrollment        │       │ ResponseMessage         │
│ IsActive                │       │ CardType                │
│ CreatedAt               │       │ Last4                   │
└─────────────────────────┘       │ CreatedAt               │
                                  └─────────────────────────┘
```

### Tablas Principales

#### `Subscriptions`

| Columna                | Tipo         | Descripción                |
| ---------------------- | ------------ | -------------------------- |
| `Id`                   | UUID         | Identificador único        |
| `CustomerId`           | UUID (FK)    | Cliente                    |
| `StripeSubscriptionId` | VARCHAR(100) | ID de Stripe               |
| `PlanId`               | VARCHAR(50)  | starter, pro, enterprise   |
| `Status`               | VARCHAR(50)  | active, canceled, past_due |
| `CurrentPeriodStart`   | TIMESTAMP    | Inicio del período         |
| `CurrentPeriodEnd`     | TIMESTAMP    | Fin del período            |
| `CancelAtPeriodEnd`    | BOOLEAN      | Cancelar al final          |
| `CanceledAt`           | TIMESTAMP    | Fecha de cancelación       |
| `TrialEnd`             | TIMESTAMP    | Fin de trial               |
| `CreatedAt`            | TIMESTAMP    | Creación                   |

#### `Payments`

| Columna                 | Tipo          | Descripción                |
| ----------------------- | ------------- | -------------------------- |
| `Id`                    | UUID          | Identificador único        |
| `CustomerId`            | UUID (FK)     | Cliente                    |
| `StripePaymentIntentId` | VARCHAR(100)  | ID de Stripe               |
| `Amount`                | DECIMAL(18,2) | Monto                      |
| `Currency`              | VARCHAR(3)    | DOP, USD                   |
| `Status`                | VARCHAR(50)   | pending, succeeded, failed |
| `PaymentMethod`         | VARCHAR(50)   | card, azul                 |
| `Description`           | VARCHAR(500)  | Descripción                |
| `Metadata`              | JSONB         | Datos adicionales          |
| `CreatedAt`             | TIMESTAMP     | Creación                   |
| `CompletedAt`           | TIMESTAMP     | Completado                 |

---

## ContactService Database

Gestión de solicitudes de contacto y mensajes.

### Diagrama ER

```
┌─────────────────────────┐
│    ContactRequests      │
├─────────────────────────┤
│ Id (PK)                 │
│ VehicleId               │
│ BuyerId                 │
│ SellerId                │
│ InitialMessage          │
│ ContactPreference       │
│ PhoneNumber             │
│ Status                  │
│ IsReadByBuyer           │
│ IsReadBySeller          │
│ CreatedAt               │
│ UpdatedAt               │
│ ClosedAt                │
└─────────────────────────┘
         │
         │
         ▼
┌─────────────────────────┐
│    ContactMessages      │
├─────────────────────────┤
│ Id (PK)                 │
│ ContactRequestId (FK)   │
│ SenderId                │
│ Message                 │
│ IsRead                  │
│ CreatedAt               │
└─────────────────────────┘
```

### Tablas Principales

#### `ContactRequests`

| Columna             | Tipo        | Descripción             |
| ------------------- | ----------- | ----------------------- |
| `Id`                | UUID        | Identificador único     |
| `VehicleId`         | UUID        | ID del vehículo         |
| `BuyerId`           | UUID        | ID del comprador        |
| `SellerId`          | UUID        | ID del vendedor         |
| `InitialMessage`    | TEXT        | Mensaje inicial         |
| `ContactPreference` | VARCHAR(50) | Email, Phone, WhatsApp  |
| `PhoneNumber`       | VARCHAR(20) | Teléfono del comprador  |
| `Status`            | VARCHAR(50) | Open, Responded, Closed |
| `IsReadByBuyer`     | BOOLEAN     | Leído por comprador     |
| `IsReadBySeller`    | BOOLEAN     | Leído por vendedor      |
| `CreatedAt`         | TIMESTAMP   | Creación                |
| `ClosedAt`          | TIMESTAMP   | Cierre                  |

---

## NotificationService Database

Gestión de notificaciones y templates.

### Diagrama ER

```
┌─────────────────────────┐       ┌─────────────────────────┐
│   NotificationTemplates │       │     Notifications       │
├─────────────────────────┤       ├─────────────────────────┤
│ Id (PK)                 │──────<│ Id (PK)                 │
│ Name                    │       │ TemplateId (FK)         │
│ Type                    │       │ UserId                  │
│ Subject                 │       │ Type                    │
│ Body                    │       │ Channel                 │
│ BodyHtml                │       │ Title                   │
│ Variables               │       │ Body                    │
│ IsActive                │       │ Data                    │
│ CreatedAt               │       │ Status                  │
└─────────────────────────┘       │ IsRead                  │
                                  │ ReadAt                  │
                                  │ SentAt                  │
                                  │ CreatedAt               │
                                  └─────────────────────────┘
                                           │
                                           ▼
                                  ┌─────────────────────────┐
                                  │  ScheduledNotifications │
                                  ├─────────────────────────┤
                                  │ Id (PK)                 │
                                  │ NotificationId (FK)     │
                                  │ ScheduledFor            │
                                  │ Status                  │
                                  │ Attempts                │
                                  │ LastAttemptAt           │
                                  │ CreatedAt               │
                                  └─────────────────────────┘
```

---

## MediaService Database

Gestión de archivos multimedia.

### Diagrama ER

```
┌─────────────────────────┐
│      MediaAssets        │
├─────────────────────────┤
│ Id (PK)                 │
│ OwnerId                 │
│ FileName                │
│ OriginalFileName        │
│ ContentType             │
│ FileSize                │
│ Category                │
│ S3Key                   │
│ S3Bucket                │
│ Url                     │
│ Status                  │
│ Metadata                │
│ CreatedAt               │
│ UpdatedAt               │
└─────────────────────────┘
         │
         │
         ▼
┌─────────────────────────┐
│     MediaVariants       │
├─────────────────────────┤
│ Id (PK)                 │
│ MediaAssetId (FK)       │
│ VariantType             │
│ Width                   │
│ Height                  │
│ FileSize                │
│ S3Key                   │
│ Url                     │
│ CreatedAt               │
└─────────────────────────┘
```

---

## DealerManagementService Database

Gestión de dealers, documentos y ubicaciones.

### Diagrama ER

```
┌─────────────────────────┐
│        Dealers          │
├─────────────────────────┤
│ Id (PK)                 │
│ UserId                  │
│ BusinessName            │
│ LegalName               │
│ RNC                     │
│ DealerType              │
│ Status                  │
│ VerificationStatus      │
│ CurrentPlan             │
│ MaxActiveListings       │
│ Email                   │
│ Phone                   │
│ Website                 │
│ Description             │
│ EstablishedDate         │
│ EmployeeCount           │
│ CreatedAt               │
│ UpdatedAt               │
└─────────────────────────┘
         │
    ┌────┴────┐
    │         │
    ▼         ▼
┌─────────────────────────┐  ┌─────────────────────────┐
│    DealerDocuments      │  │    DealerLocations      │
├─────────────────────────┤  ├─────────────────────────┤
│ Id (PK)                 │  │ Id (PK)                 │
│ DealerId (FK)           │  │ DealerId (FK)           │
│ DocumentType            │  │ Name                    │
│ FileName                │  │ LocationType            │
│ S3Key                   │  │ Address                 │
│ Status                  │  │ City                    │
│ SubmittedAt             │  │ Province                │
│ ReviewedAt              │  │ Phone                   │
│ ReviewedBy              │  │ Email                   │
│ RejectionReason         │  │ IsPrimary               │
│ ExpiresAt               │  │ Latitude                │
└─────────────────────────┘  │ Longitude               │
                             │ BusinessHours           │
                             │ IsActive                │
                             └─────────────────────────┘
```

---

## SearchService (Elasticsearch)

El SearchService utiliza Elasticsearch para búsquedas rápidas de vehículos.

### Índice: `vehicles`

```json
{
  "mappings": {
    "properties": {
      "id": { "type": "keyword" },
      "title": {
        "type": "text",
        "analyzer": "spanish"
      },
      "description": {
        "type": "text",
        "analyzer": "spanish"
      },
      "make": {
        "properties": {
          "id": { "type": "integer" },
          "name": { "type": "keyword" }
        }
      },
      "model": {
        "properties": {
          "id": { "type": "integer" },
          "name": { "type": "keyword" }
        }
      },
      "year": { "type": "integer" },
      "price": { "type": "float" },
      "mileage": { "type": "integer" },
      "fuelType": { "type": "keyword" },
      "transmission": { "type": "keyword" },
      "condition": { "type": "keyword" },
      "status": { "type": "keyword" },
      "location": {
        "properties": {
          "city": { "type": "keyword" },
          "province": { "type": "keyword" },
          "geo": { "type": "geo_point" }
        }
      },
      "features": { "type": "keyword" },
      "sellerId": { "type": "keyword" },
      "sellerType": { "type": "keyword" },
      "primaryImage": { "type": "keyword" },
      "createdAt": { "type": "date" },
      "updatedAt": { "type": "date" }
    }
  }
}
```

---

## Relaciones Entre Servicios

### IDs Compartidos

Los microservicios comparten IDs a través de llamadas HTTP y eventos:

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    FLUJO DE IDs ENTRE SERVICIOS                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  AuthService.UserId ────► UserService.AuthUserId                           │
│         │                                                                   │
│         └────────────────► VehiclesSaleService.SellerId                     │
│         │                                                                   │
│         └────────────────► BillingService.UserId                            │
│         │                                                                   │
│         └────────────────► ContactService.BuyerId/SellerId                  │
│         │                                                                   │
│         └────────────────► NotificationService.UserId                       │
│                                                                             │
│  UserService.DealerId ──► DealerManagementService.DealerId                  │
│                                                                             │
│  VehiclesSaleService.VehicleId ──► SearchService (Elasticsearch)           │
│                              │                                              │
│                              └──► ContactService.VehicleId                  │
│                              │                                              │
│                              └──► MediaService (imágenes)                   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Eventos de Dominio (RabbitMQ)

```yaml
# Eventos publicados y consumidores

VehicleCreatedEvent:
  Publisher: VehiclesSaleService
  Consumers:
    - SearchService (indexar en Elasticsearch)
    - NotificationService (notificar seguidores)

VehicleUpdatedEvent:
  Publisher: VehiclesSaleService
  Consumers:
    - SearchService (actualizar índice)

PaymentSucceededEvent:
  Publisher: BillingService
  Consumers:
    - VehiclesSaleService (activar publicación)
    - NotificationService (enviar confirmación)

UserRegisteredEvent:
  Publisher: AuthService
  Consumers:
    - UserService (crear perfil)
    - NotificationService (email bienvenida)

DealerVerifiedEvent:
  Publisher: DealerManagementService
  Consumers:
    - NotificationService (email confirmación)
    - UserService (actualizar verificación)
```

---

## Migraciones y Seeds

### Ejecutar Migraciones

```bash
# Desde cada servicio
cd backend/VehiclesSaleService
dotnet ef database update --project VehiclesSaleService.Infrastructure

# O desde Docker
docker exec -it vehiclessaleservice dotnet ef database update
```

### Scripts de Seed

Ubicación: `backend/postgresql/`

```bash
# Insertar datos de prueba
psql -h localhost -U cardealer -d vehicles_db -f insert_mock_vehicles.sql

# Agregar imágenes
psql -h localhost -U cardealer -d vehicles_db -f add_vehicle_images.sql
```

---

**Última actualización:** Enero 2026  
**Autor:** CarDealer Team
