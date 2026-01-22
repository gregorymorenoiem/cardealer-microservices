# 📊 PLAN COMPLETO DE SEEDING DE BASE DE DATOS - OKLA

**Fecha:** Enero 20, 2026  
**Objetivo:** Llenar TODAS las tablas con datos representativos que cubran TODAS las opciones disponibles

---

## 📋 RESUMEN EJECUTIVO

| Servicio                | Entidades | Registros Planificados |
| ----------------------- | --------- | ---------------------- |
| AuthService             | 4         | ~150                   |
| UserService             | 12        | ~280                   |
| VehiclesSaleService     | 9         | ~650                   |
| BillingService          | 6         | ~120                   |
| ContactService          | 4         | ~180                   |
| NotificationService     | 6         | ~250                   |
| MediaService            | 5         | ~500                   |
| DealerManagementService | 4         | ~100                   |
| RoleService             | 4         | ~50                    |
| ReviewService           | 7         | ~200                   |
| **TOTAL**               | **61**    | **~2,480**             |

---

## 🔐 1. AuthService - Autenticación

### ApplicationUser (50 usuarios)

| Cantidad | AccountType      | ExternalAuth | 2FA                   | Descripción          |
| -------- | ---------------- | ------------ | --------------------- | -------------------- |
| 20       | Individual       | ❌           | ❌                    | Compradores normales |
| 5        | Individual       | Google       | ❌                    | OAuth con Google     |
| 5        | Individual       | Microsoft    | ❌                    | OAuth con Microsoft  |
| 10       | Dealer           | ❌           | Authenticator         | Dueños de dealers    |
| 5        | DealerEmployee   | ❌           | SMS                   | Empleados de dealers |
| 3        | Admin            | ❌           | Authenticator + Email | Administradores      |
| 2        | PlatformEmployee | ❌           | Authenticator         | Empleados plataforma |

### RefreshToken (100 tokens)

- 2 tokens activos por cada usuario = 100 tokens
- Variantes: activos, revocados, expirados, reemplazados

### TwoFactorAuth (20 registros)

| Cantidad | TwoFactorAuthType   | Status              |
| -------- | ------------------- | ------------------- |
| 8        | Authenticator       | Enabled             |
| 5        | SMS                 | Enabled             |
| 3        | Email               | Enabled             |
| 2        | Authenticator + SMS | Enabled             |
| 2        | -                   | PendingVerification |

### VerificationToken (50 tokens)

| Cantidad | VerificationTokenType | Estado    |
| -------- | --------------------- | --------- |
| 20       | EmailVerification     | Usado     |
| 10       | EmailVerification     | Pendiente |
| 10       | PasswordReset         | Usado     |
| 5        | PasswordReset         | Expirado  |
| 5        | PhoneVerification     | Pendiente |

---

## 👤 2. UserService - Usuarios y Dealers

### User (50 usuarios = mismos que AuthService)

| Cantidad | AccountType      | PlatformRole      | DealerRole    | Descripción            |
| -------- | ---------------- | ----------------- | ------------- | ---------------------- |
| 30       | Individual       | -                 | -             | Compradores/Vendedores |
| 10       | Dealer           | -                 | Owner         | Dueños de dealers      |
| 5        | DealerEmployee   | -                 | Manager/Sales | Empleados              |
| 3        | Admin            | SuperAdmin/Admin  | -             | Administradores        |
| 2        | PlatformEmployee | Moderator/Support | -             | Soporte                |

**Variantes de ubicación (Ciudades RD):**

- Santo Domingo (15)
- Santiago (10)
- La Romana (5)
- Punta Cana (5)
- Puerto Plata (5)
- San Pedro de Macorís (5)
- La Vega (5)

### Dealer (20 dealers)

| Cantidad | DealerType    | VerificationStatus | DealerPlan |
| -------- | ------------- | ------------------ | ---------- |
| 8        | Independent   | Verified           | Pro        |
| 4        | Franchise     | Verified           | Enterprise |
| 4        | MultiLocation | Verified           | Basic      |
| 2        | OnlineOnly    | UnderReview        | Free       |
| 2        | Wholesale     | Pending            | Basic      |

### DealerSubscription (20 suscripciones)

| Cantidad | Plan       | Status    | Cycle   |
| -------- | ---------- | --------- | ------- |
| 5        | Pro        | Active    | Monthly |
| 5        | Enterprise | Active    | Yearly  |
| 4        | Basic      | Active    | Monthly |
| 3        | Free       | Trial     | Monthly |
| 3        | Pro        | Cancelled | Yearly  |

### DealerEmployee (30 empleados)

| Cantidad | DealerRole       | EmployeeStatus |
| -------- | ---------------- | -------------- |
| 10       | Manager          | Active         |
| 10       | Salesperson      | Active         |
| 5        | SalesManager     | Active         |
| 3        | InventoryManager | Active         |
| 2        | Viewer           | Pending        |

### DealerEmployeeInvitation (15 invitaciones)

| Cantidad | InvitationStatus |
| -------- | ---------------- |
| 5        | Pending          |
| 5        | Accepted         |
| 3        | Expired          |
| 2        | Revoked          |

### PlatformEmployee (5 empleados)

| Cantidad | PlatformRole | EmployeeStatus |
| -------- | ------------ | -------------- |
| 1        | SuperAdmin   | Active         |
| 2        | Admin        | Active         |
| 1        | Moderator    | Active         |
| 1        | Support      | Active         |

### SellerProfile (30 perfiles)

| Cantidad | VerificationStatus | Tipo              |
| -------- | ------------------ | ----------------- |
| 15       | Verified           | Individual activo |
| 5        | PendingReview      | Nuevo vendedor    |
| 5        | InReview           | En proceso        |
| 3        | Rejected           | Rechazado         |
| 2        | Suspended          | Suspendido        |

### UserOnboarding (50 registros)

| Cantidad | Estado                       |
| -------- | ---------------------------- |
| 30       | Completado (todos los pasos) |
| 10       | Parcial (2-3 pasos)          |
| 5        | Saltado                      |
| 5        | Nuevo (0 pasos)              |

### UserRole (60 asignaciones)

- 50 usuarios × 1-2 roles cada uno

### ModuleAddon (12 módulos)

| Código               | Categoría   | Precio/mes |
| -------------------- | ----------- | ---------- |
| crm-basic            | Sales       | $29        |
| crm-pro              | Sales       | $79        |
| analytics-basic      | Analytics   | $19        |
| analytics-pro        | Analytics   | $49        |
| marketing-email      | Marketing   | $39        |
| marketing-social     | Marketing   | $49        |
| integration-whatsapp | Integration | $29        |
| integration-facebook | Integration | $19        |
| automation-leads     | Automation  | $59        |
| automation-inventory | Automation  | $39        |
| support-chat         | Support     | $29        |
| finance-reports      | Finance     | $49        |

### DealerModuleSubscription (40 suscripciones)

- ~2 módulos por dealer = 40

### SubscriptionHistory (30 registros)

- Historial de cambios de plan

---

## 🚗 3. VehiclesSaleService - Vehículos

### VehicleMake (25 marcas)

| Cantidad | País     | Popular                                   |
| -------- | -------- | ----------------------------------------- |
| 5        | Japón    | Sí (Toyota, Honda, Nissan, Mazda, Subaru) |
| 3        | USA      | Sí (Ford, Chevrolet, Dodge)               |
| 3        | Alemania | Sí (BMW, Mercedes-Benz, Audi)             |
| 4        | Corea    | Mixto (Hyundai, Kia, Genesis)             |
| 3        | Italia   | No (Ferrari, Lamborghini, Maserati)       |
| 3        | UK       | No (Jaguar, Land Rover, Bentley)          |
| 4        | Otros    | Mixto (Tesla, Volvo, Porsche, Lexus)      |

### VehicleModel (80 modelos)

- ~3-4 modelos por marca popular
- Variantes de VehicleType y BodyStyle

| VehicleType | Cantidad |
| ----------- | -------- |
| Car         | 30       |
| SUV         | 20       |
| Truck       | 10       |
| Van         | 5        |
| Motorcycle  | 5        |
| Commercial  | 5        |
| Other       | 5        |

### VehicleTrim (200 trims)

- ~2-3 trims por modelo
- Variantes de FuelType, Transmission, DriveType

| FuelType     | Cantidad |
| ------------ | -------- |
| Gasoline     | 80       |
| Diesel       | 30       |
| Hybrid       | 30       |
| Electric     | 25       |
| PlugInHybrid | 20       |
| FlexFuel     | 10       |
| Other        | 5        |

### Vehicle (100 vehículos - 5 por cada dealer)

| Cantidad | VehicleStatus | VehicleCondition                           |
| -------- | ------------- | ------------------------------------------ |
| 60       | Active        | Used (40), CertifiedPreOwned (15), New (5) |
| 15       | PendingReview | Used (10), New (5)                         |
| 10       | Reserved      | Used                                       |
| 8        | Sold          | Used (5), CertifiedPreOwned (3)            |
| 5        | Draft         | Used                                       |
| 2        | Rejected      | Used                                       |

**Variantes completas:**

| Propiedad    | Valores a cubrir                                                         |
| ------------ | ------------------------------------------------------------------------ |
| BodyStyle    | Sedan, SUV, Pickup, Hatchback, Coupe, Convertible, Van, Wagon, Crossover |
| Transmission | Automatic, Manual, CVT, DualClutch                                       |
| DriveType    | FWD, RWD, AWD, FourWD                                                    |
| FuelType     | Todos los 8 tipos                                                        |
| Year         | 2015-2025                                                                |
| Price        | $5,000 - $500,000                                                        |
| Mileage      | 0 - 200,000 km                                                           |

### VehicleImage (500 imágenes)

- 5 imágenes por vehículo × 100 vehículos = 500
- Usando las 301 carpetas de data/vehicle_images

| ImageType | Cantidad por vehículo |
| --------- | --------------------- |
| Exterior  | 2 (front, rear)       |
| Interior  | 2 (dashboard, seats)  |
| Engine    | 1                     |

### Category (15 categorías)

| Categoría   | Nivel | Sistema     |
| ----------- | ----- | ----------- |
| Sedanes     | 1     | Sí          |
| SUVs        | 1     | Sí          |
| Pickups     | 1     | Sí          |
| Deportivos  | 1     | Sí          |
| Lujo        | 1     | Sí          |
| Eléctricos  | 1     | Sí          |
| Económicos  | 1     | Sí          |
| Familiares  | 1     | Sí          |
| Comerciales | 1     | Sí          |
| Motos       | 1     | Sí          |
| Clásicos    | 1     | No (custom) |
| Importados  | 1     | No (custom) |
| Ofertas     | 1     | No (custom) |
| Nuevos      | 1     | Sí          |
| Usados      | 1     | Sí          |

### Favorite (80 favoritos)

- ~30 usuarios con 1-5 favoritos cada uno
- Variantes con y sin NotifyPriceChange

### HomepageSectionConfig (10 secciones)

| Nombre             | LayoutType | MaxItems |
| ------------------ | ---------- | -------- |
| Carousel Principal | Carousel   | 8        |
| Destacados         | Grid       | 12       |
| Sedanes            | Grid       | 10       |
| SUVs               | Grid       | 10       |
| Camionetas         | Grid       | 10       |
| Deportivos         | Grid       | 8        |
| Lujo               | Grid       | 8        |
| Eléctricos         | Grid       | 8        |
| Más Vistos         | List       | 10       |
| Recién Llegados    | Hero       | 6        |

### VehicleHomepageSection (100 asignaciones)

- Cada vehículo en 1-3 secciones

---

## 💳 4. BillingService - Pagos

### StripeCustomer (25 clientes)

| Cantidad | IsActive | IsTestMode |
| -------- | -------- | ---------- |
| 20       | true     | false      |
| 3        | true     | true       |
| 2        | false    | false      |

### Subscription (20 suscripciones)

| Cantidad | SubscriptionPlan | SubscriptionStatus | BillingCycle |
| -------- | ---------------- | ------------------ | ------------ |
| 8        | Professional     | Active             | Monthly      |
| 5        | Enterprise       | Active             | Yearly       |
| 3        | Basic            | Active             | Monthly      |
| 2        | Free             | Trial              | Monthly      |
| 1        | Professional     | Cancelled          | Yearly       |
| 1        | Basic            | PastDue            | Monthly      |

### Invoice (60 facturas)

| Cantidad | InvoiceStatus |
| -------- | ------------- |
| 35       | Paid          |
| 10       | Issued        |
| 5        | Overdue       |
| 5        | Draft         |
| 3        | PartiallyPaid |
| 2        | Cancelled     |

### Payment (80 pagos)

| Cantidad | PaymentStatus     | PaymentMethod |
| -------- | ----------------- | ------------- |
| 50       | Succeeded         | CreditCard    |
| 10       | Succeeded         | DebitCard     |
| 8        | Failed            | CreditCard    |
| 5        | Refunded          | CreditCard    |
| 4        | Pending           | BankTransfer  |
| 3        | PartiallyRefunded | CreditCard    |

### AzulTransaction (20 transacciones)

| Cantidad | Status   |
| -------- | -------- |
| 15       | Approved |
| 3        | Declined |
| 2        | Pending  |

### EarlyBirdMember (15 miembros)

| Cantidad | HasUsedBenefit |
| -------- | -------------- |
| 10       | true           |
| 5        | false          |

---

## 📞 5. ContactService - Contactos

### ContactRequest (50 solicitudes)

| Cantidad | Status     |
| -------- | ---------- |
| 15       | Open       |
| 15       | InProgress |
| 15       | Responded  |
| 5        | Closed     |

### ContactMessage (120 mensajes)

- ~2-3 mensajes por solicitud

### Inquiry (30 consultas)

| Cantidad | Status    |
| -------- | --------- |
| 10       | Open      |
| 10       | Responded |
| 10       | Closed    |

### InquiryMessage (60 mensajes)

- ~2 mensajes por consulta

---

## 🔔 6. NotificationService - Notificaciones

### NotificationTemplate (30 plantillas)

| Cantidad | NotificationType | Categoría                                 |
| -------- | ---------------- | ----------------------------------------- |
| 10       | Email            | Auth (welcome, password-reset, etc.)      |
| 8        | Email            | Vehicle (new-listing, price-change, etc.) |
| 5        | SMS              | Auth (verification, 2fa)                  |
| 4        | Push             | Alerts (price-drop, new-message)          |
| 3        | Webhook          | Integrations                              |

### Notification (100 notificaciones)

| Cantidad | NotificationType | NotificationStatus | PriorityLevel |
| -------- | ---------------- | ------------------ | ------------- |
| 40       | Email            | Delivered          | Medium        |
| 25       | Email            | Sent               | Low           |
| 15       | SMS              | Delivered          | High          |
| 10       | Push             | Delivered          | Medium        |
| 5        | Email            | Failed             | High          |
| 5        | Webhook          | Delivered          | Low           |

### NotificationLog (200 logs)

- ~2 logs por notificación

### NotificationQueue (50 en cola)

| Cantidad | QueueStatus |
| -------- | ----------- |
| 20       | Completed   |
| 15       | Pending     |
| 8        | Processing  |
| 5        | Failed      |
| 2        | Retry       |

### ScheduledNotification (20 programadas)

| Cantidad | ScheduledNotificationStatus | IsRecurring   |
| -------- | --------------------------- | ------------- |
| 8        | Pending                     | false         |
| 5        | Executed                    | false         |
| 4        | Pending                     | true (Daily)  |
| 2        | Pending                     | true (Weekly) |
| 1        | Cancelled                   | false         |

### UserNotification (100 notificaciones)

| Cantidad | IsRead |
| -------- | ------ |
| 60       | true   |
| 40       | false  |

---

## 🖼️ 7. MediaService - Archivos

### ImageMedia (500 imágenes)

- Usando las 301 carpetas de data/vehicle_images
- 5 imágenes × 100 vehículos = 500
- Variantes de tamaño y contexto

### MediaVariant (1500 variantes)

- 3 variantes por imagen (thumb, medium, large)

### DocumentMedia (30 documentos)

| Cantidad | Contexto                             |
| -------- | ------------------------------------ |
| 15       | DealerDocuments (RNC, License, etc.) |
| 10       | VehicleDocuments (Carfax, etc.)      |
| 5        | UserDocuments (ID, etc.)             |

### VideoMedia (10 videos)

- Opcional, para vehículos destacados

---

## 🏢 8. DealerManagementService

### Dealer (20 dealers - mismo que UserService)

| Cantidad | DealerType    | DealerStatus | VerificationStatus | DealerPlan |
| -------- | ------------- | ------------ | ------------------ | ---------- |
| 8        | Independent   | Active       | Verified           | Pro        |
| 4        | Franchise     | Active       | Verified           | Enterprise |
| 3        | MultipleStore | Active       | Verified           | Basic      |
| 2        | Chain         | UnderReview  | DocumentsUploaded  | Basic      |
| 2        | Independent   | Pending      | NotVerified        | Free       |
| 1        | Independent   | Suspended    | Rejected           | Free       |

### DealerLocation (40 ubicaciones)

- 2 ubicaciones por dealer en promedio

| Cantidad | LocationType  | IsPrimary |
| -------- | ------------- | --------- |
| 20       | Headquarters  | true      |
| 10       | Branch        | false     |
| 5        | Showroom      | false     |
| 3        | ServiceCenter | false     |
| 2        | Warehouse     | false     |

### DealerDocument (60 documentos)

| Cantidad | DocumentType       | DocumentVerificationStatus |
| -------- | ------------------ | -------------------------- |
| 20       | RNC                | Approved                   |
| 15       | BusinessLicense    | Approved                   |
| 10       | IdentificationCard | Approved                   |
| 5        | ProofOfAddress     | Approved                   |
| 5        | TaxCertificate     | Pending                    |
| 3        | InsurancePolicy    | UnderReview                |
| 2        | Other              | Rejected                   |

### BusinessHours (280 registros)

- 7 días × 40 ubicaciones = 280

---

## 🔐 9. RoleService - Roles y Permisos

### Role (10 roles)

| Nombre        | IsSystemRole | Priority |
| ------------- | ------------ | -------- |
| SuperAdmin    | true         | 100      |
| Admin         | true         | 90       |
| Moderator     | true         | 70       |
| Support       | true         | 60       |
| Analyst       | true         | 50       |
| DealerOwner   | true         | 80       |
| DealerManager | true         | 70       |
| Salesperson   | true         | 40       |
| Buyer         | true         | 20       |
| Seller        | true         | 30       |

### Permission (50 permisos)

| Módulo        | Cantidad | Acciones                          |
| ------------- | -------- | --------------------------------- |
| Users         | 5        | Create, Read, Update, Delete, All |
| Vehicles      | 5        | Create, Read, Update, Delete, All |
| Dealers       | 5        | Create, Read, Update, Delete, All |
| Billing       | 5        | Create, Read, Update, Delete, All |
| Reports       | 3        | Read, Execute, All                |
| Settings      | 5        | Create, Read, Update, Delete, All |
| Notifications | 5        | Create, Read, Update, Delete, All |
| Reviews       | 5        | Create, Read, Update, Delete, All |
| Media         | 5        | Create, Read, Update, Delete, All |
| Analytics     | 5        | Read, Execute, All                |

### RolePermission (100 asignaciones)

- ~10 permisos por rol

### RoleLog (20 logs)

- Historial de cambios de roles

---

## ⭐ 10. ReviewService - Reseñas

### Review (80 reseñas)

| Cantidad | Rating | IsApproved      | IsVerifiedPurchase |
| -------- | ------ | --------------- | ------------------ |
| 25       | 5 ⭐   | true            | true               |
| 20       | 4 ⭐   | true            | true               |
| 15       | 4 ⭐   | true            | false              |
| 10       | 3 ⭐   | true            | true               |
| 5        | 2 ⭐   | true            | false              |
| 3        | 1 ⭐   | true            | true               |
| 2        | 5 ⭐   | false (pending) | true               |

### ReviewResponse (40 respuestas)

- ~50% de las reseñas tienen respuesta del vendedor

### ReviewHelpfulVote (150 votos)

| Cantidad | IsHelpful |
| -------- | --------- |
| 120      | true      |
| 30       | false     |

### ReviewRequest (60 solicitudes)

| Cantidad | ReviewRequestStatus |
| -------- | ------------------- |
| 25       | Completed           |
| 15       | Sent                |
| 10       | Expired             |
| 5        | Viewed              |
| 3        | Declined            |
| 2        | Cancelled           |

### ReviewSummary (20 resúmenes)

- 1 por cada dealer/vendedor

### SellerBadge (30 badges)

| Cantidad | BadgeType            | IsActive        |
| -------- | -------------------- | --------------- |
| 10       | TopRated             | true            |
| 8        | TrustedDealer        | true            |
| 5        | FiveStarSeller       | true            |
| 3        | QuickResponder       | true            |
| 2        | VerifiedProfessional | true            |
| 1        | VolumeLeader         | true            |
| 1        | ConsistencyWinner    | false (revoked) |

### FraudDetectionLog (50 logs)

| Cantidad | FraudCheckType       | FraudCheckResult |
| -------- | -------------------- | ---------------- |
| 30       | ContentAnalysis      | Pass             |
| 10       | PurchaseVerification | Pass             |
| 5        | DuplicateIp          | Warning          |
| 3        | SpeedCheck           | Suspicious       |
| 2        | TextSimilarity       | Fail             |

---

## 🗂️ ARCHIVOS DE DATOS A CREAR

### Estructura de carpetas

```
data/seeding/
├── COMPLETE_SEEDING_PLAN.md          # Este archivo
├── 01_auth/
│   ├── users.json                     # 50 usuarios
│   ├── refresh_tokens.json            # 100 tokens
│   ├── two_factor_auth.json           # 20 registros
│   └── verification_tokens.json       # 50 tokens
├── 02_users/
│   ├── users_extended.json            # 50 usuarios (datos extendidos)
│   ├── dealers.json                   # 20 dealers
│   ├── dealer_subscriptions.json      # 20 suscripciones
│   ├── dealer_employees.json          # 30 empleados
│   ├── seller_profiles.json           # 30 perfiles
│   ├── user_onboarding.json           # 50 registros
│   ├── module_addons.json             # 12 módulos
│   └── user_roles.json                # 60 asignaciones
├── 03_vehicles/
│   ├── vehicle_makes.json             # 25 marcas
│   ├── vehicle_models.json            # 80 modelos
│   ├── vehicle_trims.json             # 200 trims
│   ├── vehicles.json                  # 100 vehículos
│   ├── vehicle_images.json            # 500 imágenes (referencia)
│   ├── categories.json                # 15 categorías
│   ├── favorites.json                 # 80 favoritos
│   └── homepage_sections.json         # 10 secciones + 100 asignaciones
├── 04_billing/
│   ├── stripe_customers.json          # 25 clientes
│   ├── subscriptions.json             # 20 suscripciones
│   ├── invoices.json                  # 60 facturas
│   ├── payments.json                  # 80 pagos
│   └── early_bird_members.json        # 15 miembros
├── 05_contact/
│   ├── contact_requests.json          # 50 solicitudes
│   ├── contact_messages.json          # 120 mensajes
│   ├── inquiries.json                 # 30 consultas
│   └── inquiry_messages.json          # 60 mensajes
├── 06_notifications/
│   ├── notification_templates.json    # 30 plantillas
│   ├── notifications.json             # 100 notificaciones
│   ├── notification_logs.json         # 200 logs
│   ├── notification_queue.json        # 50 en cola
│   ├── scheduled_notifications.json   # 20 programadas
│   └── user_notifications.json        # 100 notificaciones
├── 07_media/
│   ├── image_media.json               # 500 imágenes
│   ├── media_variants.json            # 1500 variantes
│   └── document_media.json            # 30 documentos
├── 08_dealer_management/
│   ├── dealers_full.json              # 20 dealers (completo)
│   ├── dealer_locations.json          # 40 ubicaciones
│   ├── dealer_documents.json          # 60 documentos
│   └── business_hours.json            # 280 registros
├── 09_roles/
│   ├── roles.json                     # 10 roles
│   ├── permissions.json               # 50 permisos
│   └── role_permissions.json          # 100 asignaciones
├── 10_reviews/
│   ├── reviews.json                   # 80 reseñas
│   ├── review_responses.json          # 40 respuestas
│   ├── review_helpful_votes.json      # 150 votos
│   ├── review_requests.json           # 60 solicitudes
│   ├── review_summaries.json          # 20 resúmenes
│   ├── seller_badges.json             # 30 badges
│   └── fraud_detection_logs.json      # 50 logs
└── scripts/
    ├── seed_via_api.py                # Script principal Python
    ├── seed_via_api.js                # Alternativa Node.js
    ├── requirements.txt               # Dependencias Python
    └── package.json                   # Dependencias Node.js
```

---

## 📝 DATOS DE EJEMPLO POR TIPO

### 📍 Ciudades de República Dominicana

```json
[
  { "city": "Santo Domingo", "province": "Distrito Nacional" },
  { "city": "Santiago", "province": "Santiago" },
  { "city": "La Romana", "province": "La Romana" },
  { "city": "San Pedro de Macorís", "province": "San Pedro de Macorís" },
  { "city": "La Vega", "province": "La Vega" },
  { "city": "San Francisco de Macorís", "province": "Duarte" },
  { "city": "Puerto Plata", "province": "Puerto Plata" },
  { "city": "Higüey", "province": "La Altagracia" },
  { "city": "San Cristóbal", "province": "San Cristóbal" },
  { "city": "Moca", "province": "Espaillat" }
]
```

### 💼 Nombres de Negocios (Dealers)

```json
[
  "Auto Premium RD",
  "CarMax Dominicana",
  "Elite Motors",
  "Bavarian Auto",
  "Importadora del Caribe",
  "Autos del Cibao",
  "Súper Carros RD",
  "Mega Auto Santo Domingo",
  "Zona Auto Santiago",
  "Capital Motors",
  "Frontier Autos",
  "Pacific Motors RD",
  "Automotriz Hispaniola",
  "King Motors",
  "Autos Express",
  "Premium Wheels RD",
  "Auto Gallery",
  "Motor City RD",
  "Santiago Auto Sales",
  "Luxury Cars Dominicana"
]
```

### 📞 Formatos de Teléfono RD

```
+1 809-XXX-XXXX (Santo Domingo/Sur)
+1 829-XXX-XXXX (Móvil)
+1 849-XXX-XXXX (Móvil nuevo)
```

### 🏢 RNC (Registro Nacional de Contribuyentes)

Formato: XXX-XXXXX-X (9 dígitos) o XXX-XXXXXXX-X (11 dígitos)

```
101-12345-1 (Persona física)
401-12345-1 (Persona jurídica)
```

### 💵 Rangos de Precios (USD)

| Categoría  | Mínimo   | Máximo   |
| ---------- | -------- | -------- |
| Económico  | $5,000   | $15,000  |
| Medio      | $15,000  | $35,000  |
| Premium    | $35,000  | $80,000  |
| Lujo       | $80,000  | $250,000 |
| Super Lujo | $250,000 | $500,000 |

### 🚗 Features Comunes (JSON)

```json
{
  "safety": [
    "ABS",
    "Airbags",
    "Blind Spot Monitor",
    "Lane Assist",
    "Backup Camera"
  ],
  "comfort": [
    "A/C",
    "Heated Seats",
    "Sunroof",
    "Leather Interior",
    "Power Windows"
  ],
  "technology": [
    "Navigation",
    "Bluetooth",
    "Apple CarPlay",
    "Android Auto",
    "WiFi"
  ],
  "performance": ["Turbo", "Sport Mode", "All-Wheel Drive", "Paddle Shifters"]
}
```

### 📧 Formatos de Email

```
{nombre}.{apellido}@gmail.com
{nombre}{numero}@hotmail.com
info@{empresa}.com.do
ventas@{empresa}.com.do
contacto@{empresa}.com
```

---

## 🔄 ORDEN DE EJECUCIÓN

El seeding debe ejecutarse en este orden para respetar las dependencias:

1. **RoleService** - Roles y Permisos (sin dependencias)
2. **AuthService** - Usuarios base
3. **UserService** - Extensión de usuarios, Dealers, Empleados
4. **DealerManagementService** - Detalles de dealers
5. **MediaService** - Imágenes y documentos
6. **VehiclesSaleService** - Catálogo y vehículos
7. **BillingService** - Pagos y suscripciones
8. **ContactService** - Solicitudes de contacto
9. **ReviewService** - Reseñas y badges
10. **NotificationService** - Notificaciones

---

## ⏱️ TIEMPO ESTIMADO

| Fase      | Descripción               | Tiempo         |
| --------- | ------------------------- | -------------- |
| 1         | Crear archivos JSON       | 4-6 horas      |
| 2         | Crear script de seeding   | 2-3 horas      |
| 3         | Probar localmente         | 1-2 horas      |
| 4         | Ejecutar seeding completo | 30-60 min      |
| **TOTAL** |                           | **8-12 horas** |

---

## ✅ PRÓXIMOS PASOS

1. [ ] Crear carpeta `data/seeding/` con estructura
2. [ ] Generar usuarios base (01_auth)
3. [ ] Generar dealers y empleados (02_users)
4. [ ] Generar catálogo de vehículos (03_vehicles)
5. [ ] Generar 100 vehículos con imágenes
6. [ ] Generar datos de billing (04_billing)
7. [ ] Generar contactos y mensajes (05_contact)
8. [ ] Generar notificaciones (06_notifications)
9. [ ] Generar documentos de dealers (08_dealer_management)
10. [ ] Generar roles y permisos (09_roles)
11. [ ] Generar reseñas y badges (10_reviews)
12. [ ] Crear script de seeding Python/Node
13. [ ] Ejecutar seeding via API

---

**Documento creado:** Enero 20, 2026
**Última actualización:** Enero 20, 2026
