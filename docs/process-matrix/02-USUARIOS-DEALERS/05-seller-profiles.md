# 👤 Perfiles de Vendedores - Matriz de Procesos

> **Servicio:** UserService / SellerProfileController  
> **Puerto:** 5004  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO  
> **Estado de Implementación:** ✅ 100% Completo

---

## 📊 Resumen de Implementación

| Componente               | Total | Implementado | Pendiente | Estado  |
| ------------------------ | ----- | ------------ | --------- | ------- |
| **Controllers**          | 1     | 1            | 0         | ✅ 100% |
| **Procesos (SELLER-\*)** | 5     | 5            | 0         | ✅ 100% |
| **Procesos (PROF-\*)**   | 4     | 4            | 0         | ✅ 100% |
| **Tests Unitarios**      | 12    | 12           | 0         | ✅ 100% |
| **Frontend Pages**       | 2     | 2            | 0         | ✅ 100% |
| **Frontend Services**    | 1     | 1            | 0         | ✅ 100% |

### Leyenda de Estados

- ✅ **IMPLEMENTADO Y PROBADO**: Código completo con tests
- 🟢 **IMPLEMENTADO**: Código completo, falta testing
- 🟡 **EN PROGRESO**: Implementación parcial
- 🔴 **PENDIENTE**: No implementado

---

## 1. Información General

### 1.1 Descripción

Sistema de gestión de perfiles públicos de vendedores (individuales y dealers). Permite a los compradores ver información del vendedor, historial de ventas, reseñas y preferencias de contacto. Los perfiles ayudan a generar confianza y transparencia en las transacciones.

### 1.2 Tipos de Perfiles

| Tipo                    | AccountType  | Descripción                            |
| ----------------------- | ------------ | -------------------------------------- |
| **Vendedor Individual** | Individual   | Persona vendiendo su vehículo personal |
| **Dealer**              | Dealer       | Concesionario con inventario           |
| **Dealer Verificado**   | Dealer + KYC | Dealer con verificación completa       |

### 1.3 Dependencias

| Servicio            | Propósito                 |
| ------------------- | ------------------------- |
| ReviewService       | Reseñas del vendedor      |
| VehiclesSaleService | Listados activos/vendidos |
| MediaService        | Fotos del perfil          |
| LeadService         | Estadísticas de contactos |

### 1.4 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       Seller Profiles Architecture                           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   Public Views                       UserService                             │
│   ┌────────────────┐              ┌─────────────────────────────────────┐   │
│   │ Seller Profile │──┐           │      SellerProfileController        │   │
│   │ Page (Web)     │  │           │  ┌───────────────────────────────┐  │   │
│   └────────────────┘  │           │  │ Endpoints                     │  │   │
│   ┌────────────────┐  │           │  │ • GET /profile (public)       │  │   │
│   │ Vehicle Detail │──┼──────────▶│  │ • GET /listings               │  │   │
│   │ (Seller Card)  │  │           │  │ • GET /reviews                │  │   │
│   └────────────────┘  │           │  │ • PUT /profile (owner)        │  │   │
│   ┌────────────────┐  │           │  └───────────────────────────────┘  │   │
│   │ Search Results │──┘           │  ┌───────────────────────────────┐  │   │
│   │ (Seller Info)  │              │  │ Application (CQRS)            │  │   │
│   └────────────────┘              │  │ • GetSellerProfileQuery       │  │   │
│                                   │  │ • UpdateProfileCommand        │  │   │
│   Data Sources                    │  │ • GetSellerStatsQuery         │  │   │
│   ┌────────────────┐              │  └───────────────────────────────┘  │   │
│   │ ReviewService  │─────────────▶│  ┌───────────────────────────────┐  │   │
│   │ (Ratings)      │              │  │ Domain                        │  │   │
│   └────────────────┘              │  │ • SellerProfile               │  │   │
│   ┌────────────────┐              │  │ • SellerBadge                 │  │   │
│   │ VehiclesSale   │─────────────▶│  │ • ContactPreferences          │  │   │
│   │ (Listings)     │              │  └───────────────────────────────┘  │   │
│   └────────────────┘              └─────────────────────────────────────┘   │
│   ┌────────────────┐                           │                            │
│   │ MediaService   │               ┌───────────┼───────────┐                │
│   │ (Photos)       │               ▼           ▼           ▼                │
│   └────────────────┘       ┌────────────┐ ┌────────────┐ ┌────────────┐    │
│                            │ PostgreSQL │ │   Redis    │ │  RabbitMQ  │    │
│                            │ (Profiles, │ │  (Cache,   │ │ (Profile   │    │
│                            │  Stats)    │ │  Ratings)  │ │  Events)   │    │
│                            └────────────┘ └────────────┘ └────────────┘    │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints API

### 2.1 SellerProfileController (Público)

| Método | Endpoint                                      | Descripción            | Auth | Roles  |
| ------ | --------------------------------------------- | ---------------------- | ---- | ------ |
| `GET`  | `/api/sellers/{sellerId}/profile`             | Obtener perfil público | ❌   | Public |
| `GET`  | `/api/sellers/{sellerId}/listings`            | Listados del vendedor  | ❌   | Public |
| `GET`  | `/api/sellers/{sellerId}/reviews`             | Reseñas del vendedor   | ❌   | Public |
| `GET`  | `/api/sellers/{sellerId}/stats`               | Estadísticas públicas  | ❌   | Public |
| `GET`  | `/api/sellers/{sellerId}/contact-preferences` | Preferencias contacto  | ❌   | Public |

### 2.2 SellerProfileController (Autenticado)

| Método | Endpoint                           | Descripción             | Auth | Roles          |
| ------ | ---------------------------------- | ----------------------- | ---- | -------------- |
| `PUT`  | `/api/sellers/profile`             | Actualizar mi perfil    | ✅   | Seller, Dealer |
| `PUT`  | `/api/sellers/profile/photo`       | Subir foto perfil       | ✅   | Seller, Dealer |
| `PUT`  | `/api/sellers/contact-preferences` | Actualizar preferencias | ✅   | Seller, Dealer |
| `GET`  | `/api/sellers/my-stats`            | Mis estadísticas        | ✅   | Seller, Dealer |

---

## 3. Entidades y Enums

### 3.1 SellerProfile (Entidad)

```csharp
public class SellerProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public SellerType Type { get; set; }

    // Información pública
    public string DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? CoverPhotoUrl { get; set; }

    // Ubicación (pública)
    public string City { get; set; }
    public string Province { get; set; }

    // Estadísticas (públicas)
    public int TotalListings { get; set; }
    public int ActiveListings { get; set; }
    public int SoldCount { get; set; }
    public DateTime MemberSince { get; set; }

    // Ratings
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }

    // Badges
    public List<SellerBadge> Badges { get; set; }

    // Verificación
    public bool IsVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsIdentityVerified { get; set; }

    // Para Dealers
    public Guid? DealerId { get; set; }
    public string? BusinessName { get; set; }
    public string? Website { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActiveAt { get; set; }
}
```

### 3.2 SellerType (Enum)

```csharp
public enum SellerType
{
    Individual = 0,      // Vendedor particular
    Dealer = 1,          // Concesionario
    PremiumDealer = 2    // Dealer con plan Pro/Enterprise
}
```

### 3.3 SellerBadge (Enum)

```csharp
public enum SellerBadge
{
    Verified = 0,           // ✓ Identidad verificada
    TopSeller = 1,          // ⭐ Top vendedor del mes
    FastResponder = 2,      // ⚡ Responde en < 1 hora
    TrustedSeller = 3,      // 🛡️ +10 ventas, +4.5 rating
    FounderMember = 4,      // 🏆 Early Bird member
    SuperHost = 5,          // 🌟 5.0 rating, +20 reviews
    PowerSeller = 6,        // 💪 +50 ventas
    NewSeller = 7           // 🆕 Nuevo en OKLA
}
```

### 3.4 ContactPreferences (Entidad)

```csharp
public class ContactPreferences
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }

    // Canales permitidos
    public bool AllowPhoneCalls { get; set; }
    public bool AllowWhatsApp { get; set; }
    public bool AllowEmail { get; set; }
    public bool AllowInAppChat { get; set; }

    // Horarios
    public TimeSpan ContactHoursStart { get; set; }
    public TimeSpan ContactHoursEnd { get; set; }
    public List<DayOfWeek> ContactDays { get; set; }

    // Información de contacto (mostrar si permitido)
    public bool ShowPhoneNumber { get; set; }
    public bool ShowWhatsAppNumber { get; set; }
    public bool ShowEmail { get; set; }

    // Preferencias
    public string? PreferredContactMethod { get; set; }
    public string? AutoReplyMessage { get; set; }
    public string? AwayMessage { get; set; }

    // Filtros
    public bool RequireVerifiedBuyers { get; set; }
    public bool BlockAnonymousContacts { get; set; }
}
```

---

## 4. Procesos Detallados

### 4.1 SELLER-001: Ver Perfil de Vendedor

| Campo       | Valor                               |
| ----------- | ----------------------------------- |
| **ID**      | SELLER-001                          |
| **Nombre**  | Ver Perfil Público de Vendedor      |
| **Actor**   | Comprador (cualquier usuario)       |
| **Trigger** | GET /api/sellers/{sellerId}/profile |

#### Flujo del Proceso

| Paso | Acción                   | Sistema             | Validación         |
| ---- | ------------------------ | ------------------- | ------------------ |
| 1    | Usuario ve listado       | Frontend            | VehicleDetail      |
| 2    | Click en nombre vendedor | Frontend            | Link a perfil      |
| 3    | Obtener perfil público   | UserService         | Por sellerId       |
| 4    | Obtener reviews          | ReviewService       | Últimas 10         |
| 5    | Obtener listados activos | VehiclesSaleService | Activos del seller |
| 6    | Calcular estadísticas    | UserService         | Ventas, rating     |
| 7    | Obtener badges           | UserService         | Activos            |
| 8    | Renderizar perfil        | Frontend            | Página completa    |

#### Response

```json
{
  "id": "uuid",
  "displayName": "Autos del Caribe",
  "type": "Dealer",
  "bio": "Más de 15 años en el mercado automotriz dominicano",
  "profilePhotoUrl": "https://media.okla.com.do/profiles/xyz.jpg",
  "city": "Santo Domingo",
  "province": "Distrito Nacional",
  "memberSince": "2026-01-15",
  "isVerified": true,
  "badges": ["Verified", "FounderMember", "TrustedSeller"],
  "stats": {
    "totalListings": 45,
    "activeListings": 12,
    "soldCount": 33,
    "averageRating": 4.8,
    "reviewCount": 28,
    "responseTime": "1 hora",
    "responseRate": 98
  },
  "dealer": {
    "businessName": "Autos del Caribe SRL",
    "website": "https://autosdelcaribe.com.do",
    "isKYCVerified": true
  }
}
```

---

### 4.2 SELLER-002: Actualizar Perfil

| Campo       | Valor                    |
| ----------- | ------------------------ |
| **ID**      | SELLER-002               |
| **Nombre**  | Actualizar Mi Perfil     |
| **Actor**   | Vendedor/Dealer          |
| **Trigger** | PUT /api/sellers/profile |

#### Flujo del Proceso

| Paso | Acción                          | Sistema     | Validación      |
| ---- | ------------------------------- | ----------- | --------------- |
| 1    | Vendedor accede a configuración | Dashboard   | Autenticado     |
| 2    | Editar campos del perfil        | Frontend    | Formulario      |
| 3    | Validar campos                  | Frontend    | Bio < 500 chars |
| 4    | Submit actualización            | API         | PUT /profile    |
| 5    | Validar permisos                | UserService | Es el owner     |
| 6    | Actualizar perfil               | Database    | Update          |
| 7    | Invalidar cache                 | Redis       | Del perfil      |
| 8    | Publicar evento                 | RabbitMQ    | profile.updated |

#### Request

```json
{
  "displayName": "Autos del Caribe",
  "bio": "Más de 15 años ofreciendo los mejores vehículos importados en República Dominicana.",
  "city": "Santo Domingo",
  "province": "Distrito Nacional"
}
```

---

### 4.3 SELLER-003: Configurar Preferencias de Contacto

| Campo       | Valor                                |
| ----------- | ------------------------------------ |
| **ID**      | SELLER-003                           |
| **Nombre**  | Configurar Preferencias de Contacto  |
| **Actor**   | Vendedor/Dealer                      |
| **Trigger** | PUT /api/sellers/contact-preferences |

#### Flujo del Proceso

| Paso | Acción                 | Sistema   | Validación          |
| ---- | ---------------------- | --------- | ------------------- |
| 1    | Acceder a preferencias | Dashboard | Settings            |
| 2    | Configurar canales     | Frontend  | Checkboxes          |
| 3    | Definir horarios       | Frontend  | TimeSpan            |
| 4    | Configurar visibilidad | Frontend  | Mostrar/ocultar     |
| 5    | Mensaje automático     | Frontend  | Opcional            |
| 6    | Guardar preferencias   | API       | PUT                 |
| 7    | Actualizar en DB       | Database  | Update              |
| 8    | Publicar evento        | RabbitMQ  | preferences.updated |

#### Request

```json
{
  "allowPhoneCalls": true,
  "allowWhatsApp": true,
  "allowEmail": true,
  "allowInAppChat": true,
  "contactHoursStart": "08:00:00",
  "contactHoursEnd": "18:00:00",
  "contactDays": [
    "Monday",
    "Tuesday",
    "Wednesday",
    "Thursday",
    "Friday",
    "Saturday"
  ],
  "showPhoneNumber": true,
  "showWhatsAppNumber": true,
  "showEmail": false,
  "preferredContactMethod": "WhatsApp",
  "autoReplyMessage": "¡Gracias por contactarnos! Responderemos en menos de 1 hora.",
  "awayMessage": "Actualmente fuera de horario. Responderemos el próximo día hábil.",
  "requireVerifiedBuyers": false,
  "blockAnonymousContacts": true
}
```

---

### 4.4 SELLER-004: Asignar Badges

| Campo       | Valor                 |
| ----------- | --------------------- |
| **ID**      | SELLER-004            |
| **Nombre**  | Asignar/Quitar Badges |
| **Actor**   | Sistema (automático)  |
| **Trigger** | Eventos de negocio    |

#### Criterios de Badges

| Badge           | Criterio                       | Auto/Manual |
| --------------- | ------------------------------ | ----------- |
| `Verified`      | KYC aprobado                   | Auto        |
| `FounderMember` | Early Bird activo              | Auto        |
| `NewSeller`     | < 30 días en plataforma        | Auto        |
| `FastResponder` | Response time < 1h por 30 días | Auto        |
| `TrustedSeller` | +10 ventas Y rating >= 4.5     | Auto        |
| `TopSeller`     | Top 10 ventas del mes          | Auto        |
| `SuperHost`     | Rating 5.0 Y +20 reviews       | Auto        |
| `PowerSeller`   | +50 ventas totales             | Auto        |

#### Flujo de Asignación Automática

| Paso | Acción               | Sistema             | Trigger                             |
| ---- | -------------------- | ------------------- | ----------------------------------- |
| 1    | Evento recibido      | RabbitMQ            | sale.completed, review.created, etc |
| 2    | Evaluar criterios    | UserService         | Por cada badge                      |
| 3    | Si cumple y no tiene | Check               | Agregar badge                       |
| 4    | Si no cumple y tiene | Check               | Quitar badge (algunos)              |
| 5    | Actualizar perfil    | Database            | Badges list                         |
| 6    | Notificar si nuevo   | NotificationService | "¡Ganaste badge!"                   |
| 7    | Publicar evento      | RabbitMQ            | badge.earned/lost                   |

---

### 4.5 SELLER-005: Calcular Estadísticas

| Campo       | Valor                             |
| ----------- | --------------------------------- |
| **ID**      | SELLER-005                        |
| **Nombre**  | Calcular Estadísticas de Vendedor |
| **Actor**   | Sistema (Job nocturno)            |
| **Trigger** | Cron 03:00 AM                     |

#### Estadísticas Calculadas

| Métrica        | Fuente              | Cálculo                    |
| -------------- | ------------------- | -------------------------- |
| TotalListings  | VehiclesSaleService | COUNT(listings)            |
| ActiveListings | VehiclesSaleService | COUNT(status=Active)       |
| SoldCount      | VehiclesSaleService | COUNT(status=Sold)         |
| AverageRating  | ReviewService       | AVG(rating)                |
| ReviewCount    | ReviewService       | COUNT(reviews)             |
| ResponseTime   | LeadService         | AVG(first_response_time)   |
| ResponseRate   | LeadService         | (responded / total) \* 100 |
| ViewsThisMonth | AnalyticsService    | SUM(listing_views)         |
| LeadsThisMonth | LeadService         | COUNT(leads, thisMonth)    |

#### Flujo del Proceso

| Paso | Acción                        | Sistema          | Validación     |
| ---- | ----------------------------- | ---------------- | -------------- |
| 1    | Job inicia                    | SchedulerService | 03:00 AM       |
| 2    | Obtener todos los vendedores  | UserService      | Activos        |
| 3    | Por cada vendedor             | Loop             | Batch de 100   |
| 4    | Consultar VehiclesSaleService | HTTP             | Listings stats |
| 5    | Consultar ReviewService       | HTTP             | Reviews stats  |
| 6    | Consultar LeadService         | HTTP             | Response stats |
| 7    | Calcular métricas             | UserService      | Agregaciones   |
| 8    | Actualizar perfil             | Database         | Stats          |
| 9    | Evaluar badges                | UserService      | Asignar/quitar |
| 10   | Cachear perfil                | Redis            | 24h TTL        |

---

## 5. Página de Perfil Público

### 5.1 Estructura de la Página

```
┌─────────────────────────────────────────────────────────────────────┐
│  ┌──────────┐                                                       │
│  │  FOTO    │  Autos del Caribe           ✓ Verificado             │
│  │  PERFIL  │  ⭐ 4.8 (28 reseñas)        🏆 Miembro Fundador       │
│  └──────────┘  📍 Santo Domingo, DN       ⚡ Responde en 1 hora     │
│                                                                      │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ │
│                                                                      │
│  📊 Estadísticas                                                     │
│  ┌─────────────┬─────────────┬─────────────┬─────────────┐          │
│  │ 12 Activos  │ 33 Vendidos │ 98% Resp.   │ 15 Años     │          │
│  └─────────────┴─────────────┴─────────────┴─────────────┘          │
│                                                                      │
│  📝 Sobre nosotros                                                   │
│  "Más de 15 años ofreciendo los mejores vehículos importados..."   │
│                                                                      │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ │
│                                                                      │
│  🚗 Vehículos Activos (12)                        [Ver todos →]     │
│  ┌───────────┬───────────┬───────────┬───────────┐                  │
│  │ Toyota    │ Honda     │ Hyundai   │ BMW       │                  │
│  │ Camry     │ Accord    │ Tucson    │ X3        │                  │
│  │ $1.2M     │ $1.0M     │ $1.5M     │ $2.8M     │                  │
│  └───────────┴───────────┴───────────┴───────────┘                  │
│                                                                      │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ │
│                                                                      │
│  ⭐ Reseñas Recientes                             [Ver todas →]     │
│  ┌───────────────────────────────────────────────────────────┐     │
│  │ ⭐⭐⭐⭐⭐ "Excelente trato, muy profesionales"              │     │
│  │ Juan P. - hace 3 días                                      │     │
│  └───────────────────────────────────────────────────────────┘     │
│                                                                      │
│  📞 Contactar                                                        │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐        │
│  │ 📱 WhatsApp    │  │ 📞 Llamar      │  │ ✉️ Mensaje     │        │
│  └────────────────┘  └────────────────┘  └────────────────┘        │
│                                                                      │
│  ⏰ Horario: Lun-Sáb 8AM-6PM                                        │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 6. Reglas de Negocio

### 6.1 Visibilidad del Perfil

| Dato               | Público      | Solo si permite    |
| ------------------ | ------------ | ------------------ |
| Nombre/DisplayName | ✅           | -                  |
| Foto perfil        | ✅           | -                  |
| Ciudad/Provincia   | ✅           | -                  |
| Bio                | ✅           | -                  |
| Estadísticas       | ✅           | -                  |
| Badges             | ✅           | -                  |
| Teléfono           | -            | ShowPhoneNumber    |
| WhatsApp           | -            | ShowWhatsAppNumber |
| Email              | -            | ShowEmail          |
| Website            | ✅ (dealers) | -                  |

### 6.2 Restricciones

| Regla                | Valor          |
| -------------------- | -------------- |
| Bio máximo           | 500 caracteres |
| DisplayName mínimo   | 3 caracteres   |
| Foto perfil máximo   | 5 MB           |
| Foto perfil formatos | JPG, PNG, WebP |

---

## 7. Eventos RabbitMQ

| Evento                       | Exchange      | Payload                |
| ---------------------------- | ------------- | ---------------------- |
| `seller.profile.created`     | `user.events` | `{ sellerId, type }`   |
| `seller.profile.updated`     | `user.events` | `{ sellerId, fields }` |
| `seller.preferences.updated` | `user.events` | `{ sellerId }`         |
| `seller.badge.earned`        | `user.events` | `{ sellerId, badge }`  |
| `seller.badge.lost`          | `user.events` | `{ sellerId, badge }`  |
| `seller.verified`            | `user.events` | `{ sellerId }`         |

---

## 8. Métricas

### 8.1 Prometheus

```
# Perfiles
seller_profiles_total{type="individual|dealer"}
seller_profiles_views_total
seller_profiles_complete_rate

# Badges
seller_badges_total{badge="verified|trusted|..."}
seller_badges_earned_total
seller_badges_lost_total

# Contacto
seller_contact_requests_total{method="phone|whatsapp|email|chat"}
seller_response_time_seconds
seller_response_rate
```

---

## 📚 Referencias

- [01-user-service.md](01-user-service.md) - Servicio de usuarios
- [02-dealer-management.md](02-dealer-management.md) - Gestión de dealers
- [01-review-service.md](../07-REVIEWS-REPUTACION/01-review-service.md) - Reseñas
