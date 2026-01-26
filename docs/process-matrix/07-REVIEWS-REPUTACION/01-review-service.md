# ⭐ Review Service - Matriz de Procesos

> **Servicio:** ReviewService  
> **Puerto:** 5030  
> **Última actualización:** Enero 25, 2026  
> **Estado de Implementación:** ✅ Backend 100% | ✅ UI 90%

---

## ✅ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

> **Estado:** ✅ Servicio funcional con UI completa.

| Proceso          | Backend | UI Access | Observación               |
| ---------------- | ------- | --------- | ------------------------- |
| Ver reviews      | ✅ 100% | ✅ 100%   | En perfil dealer/vendedor |
| Escribir review  | ✅ 100% | ✅ 100%   | Post-transacción          |
| Votar review     | ✅ 100% | ✅ 100%   | Botón útil/no útil        |
| Responder review | ✅ 100% | ✅ 100%   | Para dealers              |
| Badges           | ✅ 100% | 🟡 70%    | Parcialmente visible      |
| Moderar reviews  | ✅ 100% | 🟡 60%    | En `/admin/users`         |

### Rutas UI Existentes ✅

- ✅ Sección en `/dealer/:slug` - Reviews del dealer
- ✅ Sección en `/vehicles/:slug` - Reviews del vendedor
- ✅ Modal post-transacción - Escribir review
- ✅ `/admin/reviews` - Moderar reviews (parcial)

**Verificación Backend:** ReviewService existe en `/backend/ReviewService/` ✅

---

## 📊 Resumen de Implementación (ACTUALIZADO)

| Componente                     | Total | Implementado | Pendiente | Estado  |
| ------------------------------ | ----- | ------------ | --------- | ------- |
| **Controllers**                | 2     | 2            | 0         | ✅ 100% |
| **REV-CRUD-\*** (CRUD Reviews) | 5     | 5            | 0         | ✅ 100% |
| **REV-VOTE-\*** (Votos)        | 3     | 3            | 0         | ✅ 100% |
| **REV-MOD-\*** (Moderación)    | 4     | 3            | 1         | 🟡 75%  |
| **REV-BADGE-\*** (Badges)      | 3     | 2            | 1         | 🟡 67%  |
| **REV-STAT-\*** (Estadísticas) | 3     | 3            | 0         | ✅ 100% |
| **Tests**                      | 18    | 15           | 3         | 🟡 83%  |

---

## 📋 Información General

| Aspecto           | Detalle                                                                                                                         |
| ----------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| **Servicio**      | ReviewService                                                                                                                   |
| **Puerto**        | 5030                                                                                                                            |
| **Base de Datos** | PostgreSQL (review_db)                                                                                                          |
| **Tecnología**    | .NET 8, MediatR CQRS, Entity Framework Core                                                                                     |
| **Descripción**   | Sistema de reviews estilo Amazon para vendedores y dealers, con votos de utilidad, badges automáticos y solicitudes post-compra |

### Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        ReviewService Architecture                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   User Actions                       Core Service                            │
│   ┌────────────────┐                ┌────────────────────────────────┐      │
│   │ Buyers         │──┐             │          ReviewService           │      │
│   │ (Write Reviews)│  │             │  ┌──────────────────────────┐   │      │
│   └────────────────┘  │             │  │ Controllers              │   │      │
│   ┌────────────────┐  │             │  │ • ReviewsController      │   │      │
│   │ Visitors       │──┼────────────▶│  │ • BadgesController       │   │      │
│   │ (Read/Vote)    │  │             │  │ • ModerationController   │   │      │
│   └────────────────┘  │             │  └──────────────────────────┘   │      │
│   ┌────────────────┐  │             │  ┌──────────────────────────┐   │      │
│   │ Sellers        │──┤             │  │ Application (CQRS)       │   │      │
│   │ (Respond)      │  │             │  │ • CreateReviewCommand    │   │      │
│   └────────────────┘  │             │  │ • VoteReviewCommand      │   │      │
│   ┌────────────────┐  │             │  │ • CalculateBadgesJob     │   │      │
│   │ Admins         │──┘             │  │ • GetSellerReviewsQuery  │   │      │
│   │ (Moderate)     │               │  └──────────────────────────┘   │      │
│   └────────────────┘               │  ┌──────────────────────────┐   │      │
│                                    │  │ Domain                   │   │      │
│   Triggers                         │  │ • Review, Vote           │   │      │
│   ┌────────────────┐               │  │ • Badge, SellerStats     │   │      │
│   │ BillingService │──────────────▶│  │ • ReviewRequest          │   │      │
│   │ (Sale Complete)│               │  └──────────────────────────┘   │      │
│   └────────────────┘               └────────────────────────────────┘      │
│                                                    │                        │
│                                    ┌───────────────┼───────────────┐        │
│                                    ▼               ▼               ▼        │
│                            ┌────────────┐  ┌────────────┐  ┌────────────┐  │
│                            │ PostgreSQL │  │   Redis    │  │  RabbitMQ  │  │
│                            │ (Reviews,  │  │ (Rating    │  │ (Review    │  │
│                            │  Badges)   │  │  Cache)    │  │  Events)   │  │
│                            └────────────┘  └────────────┘  └────────────┘  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 🎯 Endpoints del Servicio

### Reviews CRUD

| Método   | Endpoint                                 | Descripción                       | Auth | Roles   |
| -------- | ---------------------------------------- | --------------------------------- | ---- | ------- |
| `GET`    | `/api/reviews/seller/{sellerId}`         | Reviews de un vendedor (paginado) | ❌   | Público |
| `GET`    | `/api/reviews/seller/{sellerId}/summary` | Estadísticas de reviews           | ❌   | Público |
| `GET`    | `/api/reviews/{reviewId}`                | Obtener review por ID             | ❌   | Público |
| `POST`   | `/api/reviews`                           | Crear review                      | ✅   | User    |
| `PUT`    | `/api/reviews/{reviewId}`                | Actualizar review (solo autor)    | ✅   | User    |
| `DELETE` | `/api/reviews/{reviewId}`                | Eliminar review (solo autor)      | ✅   | User    |

### Moderación

| Método | Endpoint                           | Descripción                | Auth | Roles  |
| ------ | ---------------------------------- | -------------------------- | ---- | ------ |
| `POST` | `/api/reviews/{reviewId}/moderate` | Aprobar/Rechazar review    | ✅   | Admin  |
| `POST` | `/api/reviews/{reviewId}/respond`  | Vendedor responde a review | ✅   | Seller |

### Votos de Utilidad

| Método | Endpoint                             | Descripción             | Auth | Roles   |
| ------ | ------------------------------------ | ----------------------- | ---- | ------- |
| `POST` | `/api/reviews/{reviewId}/vote`       | Votar si review es útil | ✅   | User    |
| `GET`  | `/api/reviews/{reviewId}/vote-stats` | Estadísticas de votos   | ❌   | Público |

### Badges de Vendedor

| Método | Endpoint                                            | Descripción                 | Auth | Roles   |
| ------ | --------------------------------------------------- | --------------------------- | ---- | ------- |
| `GET`  | `/api/reviews/seller/{sellerId}/badges`             | Badges activos del vendedor | ❌   | Público |
| `POST` | `/api/reviews/seller/{sellerId}/badges/recalculate` | Recalcular badges           | ✅   | Admin   |

### Solicitudes Automáticas

| Método | Endpoint                                | Descripción                | Auth | Roles         |
| ------ | --------------------------------------- | -------------------------- | ---- | ------------- |
| `POST` | `/api/reviews/requests`                 | Enviar solicitud de review | ✅   | Admin, System |
| `GET`  | `/api/reviews/requests/buyer/{buyerId}` | Solicitudes pendientes     | ✅   | User, Admin   |
| `GET`  | `/api/reviews/requests/mine`            | Mis solicitudes pendientes | ✅   | User          |

---

## 📊 Entidades del Dominio

### Review

```csharp
public class Review : BaseEntity<Guid>
{
    // ========================================
    // INFORMACIÓN PRINCIPAL
    // ========================================
    public Guid BuyerId { get; set; }           // Quien deja la review
    public Guid SellerId { get; set; }          // Quien recibe la review
    public Guid? VehicleId { get; set; }        // Vehículo relacionado
    public Guid? OrderId { get; set; }          // Transacción que valida la compra

    // ========================================
    // CONTENIDO DE LA REVIEW
    // ========================================
    public int Rating { get; set; }             // 1-5 estrellas
    public string Title { get; set; }           // "Excelente servicio"
    public string Content { get; set; }         // Texto detallado

    // ========================================
    // MODERACIÓN
    // ========================================
    public bool IsApproved { get; set; }        // Aprobada por moderación
    public bool IsVerifiedPurchase { get; set; } // Badge "Compra Verificada"
    public string? RejectionReason { get; set; }
    public Guid? ModeratedById { get; set; }
    public DateTime? ModeratedAt { get; set; }

    // ========================================
    // INFORMACIÓN DEL COMPRADOR (Cache)
    // ========================================
    public string BuyerName { get; set; }
    public string? BuyerPhotoUrl { get; set; }

    // ========================================
    // VOTOS DE UTILIDAD
    // ========================================
    public int HelpfulVotes { get; set; }       // Votos positivos
    public int TotalVotes { get; set; }         // Total de votos

    // ========================================
    // DETECCIÓN DE FRAUDE
    // ========================================
    public int TrustScore { get; set; }         // 0-100 (100 = confiable)
    public bool IsFlagged { get; set; }
    public string? FlagReason { get; set; }
    public string? UserIpAddress { get; set; }
    public string? UserAgent { get; set; }

    // ========================================
    // SOLICITUD AUTOMÁTICA
    // ========================================
    public bool WasAutoRequested { get; set; }
    public DateTime? AutoRequestedAt { get; set; }

    // Navigation
    public ReviewResponse? Response { get; set; }
    public List<ReviewHelpfulVote> HelpfulVotesList { get; set; }
}
```

### SellerBadge (Insignias)

```csharp
public class SellerBadge : BaseEntity<Guid>
{
    public Guid SellerId { get; set; }
    public BadgeType BadgeType { get; set; }
    public string Title { get; set; }           // "Top Rated"
    public string Description { get; set; }     // Descripción del badge
    public string Icon { get; set; }            // Emoji o icono
    public string Color { get; set; }           // "#3B82F6"
    public bool IsActive { get; set; }
    public DateTime EarnedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Criteria { get; set; }       // Criterios cumplidos
    public string? QualifyingStats { get; set; } // Stats JSON
    public int DisplayOrder { get; set; }
}

public enum BadgeType
{
    TopRated = 1,           // 4.8+ estrellas, 10+ reviews
    TrustedDealer = 2,      // 6+ meses, 95%+ positivas
    FiveStarSeller = 3,     // Solo 5 estrellas (min 5)
    CustomerChoice = 4,     // 80%+ mencionan "recomendado"
    QuickResponder = 5,     // Responde en <24h
    VerifiedProfessional = 6, // Documentación verificada
    RisingStar = 7,         // Tendencia positiva 3 meses
    VolumeLeader = 8,       // 50+ reviews
    ConsistencyWinner = 9,  // Rating estable 6+ meses
    CommunityFavorite = 10  // Reviews más útiles del mes
}
```

### ReviewRequest (Solicitudes Automáticas)

```csharp
public class ReviewRequest : BaseEntity<Guid>
{
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid OrderId { get; set; }
    public string BuyerEmail { get; set; }
    public string BuyerName { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime RequestSentAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public ReviewRequestStatus Status { get; set; }
    public string Token { get; set; }           // Link único
    public int RemindersSent { get; set; }
    public DateTime? LastReminderAt { get; set; }
    public Guid? ReviewId { get; set; }
    public DateTime? ReviewCreatedAt { get; set; }
}

public enum ReviewRequestStatus
{
    Sent = 1,       // Esperando respuesta
    Viewed = 2,     // Email visto, sin acción
    Completed = 3,  // Review escrita
    Expired = 4,    // Tiempo expirado
    Declined = 5,   // Comprador declinó
    Cancelled = 6   // Cancelada
}
```

---

## 🔄 Procesos Detallados

### PROCESO 1: Crear Review de Vendedor

#### Endpoint: `POST /api/reviews`

| Paso | Actor      | Acción                     | Sistema                              | Resultado            |
| ---- | ---------- | -------------------------- | ------------------------------------ | -------------------- |
| 1    | Comprador  | Envía review               | HTTP POST                            | Request recibido     |
| 2    | Controller | Extrae BuyerId de JWT      | ClaimTypes.NameIdentifier            | Usuario identificado |
| 3    | Handler    | Valida rating 1-5          | FluentValidation                     | Rating válido        |
| 4    | Handler    | Verifica compra (OrderId)  | Si existe, IsVerifiedPurchase = true | Badge asignado       |
| 5    | Handler    | Verifica no duplicado      | No existe review buyer+seller        | Sin duplicados       |
| 6    | Handler    | Crea entidad Review        | new Review()                         | Review creada        |
| 7    | Handler    | Aplica detección de fraude | CheckFraud(IP, UserAgent)            | TrustScore calculado |
| 8    | Repository | Persiste review            | INSERT                               | Guardado             |
| 9    | Events     | Publica ReviewCreatedEvent | RabbitMQ                             | Evento publicado     |
| 10   | Events     | Trigger recálculo badges   | BadgeService                         | Badges actualizados  |
| 11   | API        | Retorna 201 Created        | CreatedAtAction                      | Review creada        |

#### Request Body

```json
{
  "sellerId": "seller-uuid",
  "vehicleId": "vehicle-uuid",
  "orderId": "order-uuid",
  "rating": 5,
  "title": "Excelente experiencia de compra",
  "content": "El vendedor fue muy profesional y transparente. El vehículo estaba en perfectas condiciones, mejor de lo que esperaba. Muy recomendado para cualquiera que busque un auto usado de calidad."
}
```

#### Response (201 Created)

```json
{
  "id": "review-uuid",
  "buyerId": "buyer-uuid",
  "sellerId": "seller-uuid",
  "vehicleId": "vehicle-uuid",
  "rating": 5,
  "title": "Excelente experiencia de compra",
  "content": "El vendedor fue muy profesional...",
  "isApproved": true,
  "isVerifiedPurchase": true,
  "buyerName": "Juan Pérez",
  "buyerPhotoUrl": "https://cdn.okla.com.do/users/juan.jpg",
  "helpfulVotes": 0,
  "totalVotes": 0,
  "createdAt": "2026-01-21T15:30:00Z"
}
```

---

### PROCESO 2: Obtener Reviews de Vendedor con Estadísticas

#### Endpoint: `GET /api/reviews/seller/{sellerId}/summary`

| Paso | Actor    | Acción                     | Sistema                         | Resultado         |
| ---- | -------- | -------------------------- | ------------------------------- | ----------------- |
| 1    | Frontend | Solicita resumen           | HTTP GET                        | Request recibido  |
| 2    | Handler  | Consulta reviews aprobadas | WHERE SellerId AND IsApproved   | Reviews filtradas |
| 3    | Handler  | Calcula promedio           | AVG(Rating)                     | AverageRating     |
| 4    | Handler  | Cuenta totales             | COUNT(\*)                       | TotalReviews      |
| 5    | Handler  | Cuenta verificadas         | COUNT(WHERE IsVerifiedPurchase) | VerifiedCount     |
| 6    | Handler  | Agrupa por rating          | GROUP BY Rating                 | Distribución      |
| 7    | Handler  | Obtiene badges activos     | BadgeRepository.GetActive()     | Badges            |
| 8    | API      | Retorna ReviewSummaryDto   | HTTP 200                        | Resumen completo  |

#### Response (200 OK)

```json
{
  "sellerId": "seller-uuid",
  "averageRating": 4.7,
  "totalReviews": 156,
  "verifiedPurchases": 142,
  "ratingDistribution": {
    "5": 98,
    "4": 45,
    "3": 8,
    "2": 3,
    "1": 2
  },
  "percentagePositive": 91.7,
  "recentTrend": "up",
  "badges": [
    {
      "type": "TopRated",
      "title": "Top Rated",
      "icon": "⭐",
      "color": "#F59E0B"
    },
    {
      "type": "TrustedDealer",
      "title": "Vendedor Confiable",
      "icon": "✓",
      "color": "#10B981"
    }
  ]
}
```

---

### PROCESO 3: Votar Review como Útil

#### Endpoint: `POST /api/reviews/{reviewId}/vote`

| Paso | Actor      | Acción                  | Sistema                  | Resultado               |
| ---- | ---------- | ----------------------- | ------------------------ | ----------------------- |
| 1    | Usuario    | Vota si es útil         | HTTP POST                | Request recibido        |
| 2    | Controller | Extrae UserId de JWT    | Claims                   | Usuario identificado    |
| 3    | Handler    | Verifica review existe  | GetById()                | Review encontrada       |
| 4    | Handler    | Verifica no es autor    | BuyerId != UserId        | No auto-voto            |
| 5    | Handler    | Busca voto previo       | GetVoteByUser()          | ¿Ya votó?               |
| 6    | Handler    | Si votó, actualiza voto | Update vote              | Voto cambiado           |
| 7    | Handler    | Si no votó, crea voto   | Create vote              | Voto nuevo              |
| 8    | Handler    | Recalcula contadores    | HelpfulVotes, TotalVotes | Contadores actualizados |
| 9    | Repository | Persiste cambios        | SaveChanges()            | Guardado                |
| 10   | API        | Retorna estadísticas    | HTTP 200                 | VoteResultDto           |

#### Request Body

```json
{
  "isHelpful": true
}
```

#### Response (200 OK)

```json
{
  "reviewId": "review-uuid",
  "helpfulVotes": 24,
  "totalVotes": 28,
  "helpfulPercentage": 85.7,
  "userVote": "helpful"
}
```

---

### PROCESO 4: Recálculo Automático de Badges

#### Trigger: Después de cada nueva review

| Paso | Actor        | Acción                                         | Sistema          | Resultado          |
| ---- | ------------ | ---------------------------------------------- | ---------------- | ------------------ |
| 1    | System       | Nueva review creada                            | Event trigger    | Proceso iniciado   |
| 2    | BadgeService | Obtiene stats del vendedor                     | GetSellerStats() | Stats actualizadas |
| 3    | BadgeService | Evalúa cada tipo de badge                      | Loop BadgeType   | -                  |
| 4    | -            | TopRated: avg >= 4.8 AND count >= 10           | Check criteria   | Pass/Fail          |
| 5    | -            | TrustedDealer: months >= 6 AND positive >= 95% | Check criteria   | Pass/Fail          |
| 6    | -            | FiveStarSeller: all 5 stars AND count >= 5     | Check criteria   | Pass/Fail          |
| 7    | -            | ... (otros badges)                             | Check criteria   | Pass/Fail          |
| 8    | BadgeService | Para badges ganados                            | Grant badge      | Badge creado       |
| 9    | BadgeService | Para badges perdidos                           | Revoke badge     | Badge revocado     |
| 10   | Events       | Publica BadgeEarnedEvent                       | RabbitMQ         | Notificación       |

#### Criterios de Badges

| Badge                    | Criterios                                                | Validación   |
| ------------------------ | -------------------------------------------------------- | ------------ |
| **TopRated**             | Rating promedio >= 4.8 Y total reviews >= 10             | Mensual      |
| **TrustedDealer**        | 6+ meses activo Y 95%+ reviews positivas (4-5 estrellas) | Mensual      |
| **FiveStarSeller**       | 100% reviews de 5 estrellas Y mínimo 5 reviews           | Continuo     |
| **CustomerChoice**       | 80%+ reviews mencionan "recomendado" o similar           | Mensual      |
| **QuickResponder**       | Tiempo promedio de respuesta < 24 horas                  | Semanal      |
| **VerifiedProfessional** | Documentación verificada Y 4+ estrellas promedio         | Al verificar |
| **RisingStar**           | Rating mejoró en últimos 3 meses                         | Trimestral   |
| **VolumeLeader**         | 50+ reviews totales                                      | Continuo     |
| **ConsistencyWinner**    | Rating estable (±0.2) por 6+ meses                       | Semestral    |
| **CommunityFavorite**    | Top 10% reviews más útiles del mes                       | Mensual      |

---

### PROCESO 5: Solicitud Automática de Review

#### Trigger: 7 días después de compra completada

| Paso | Actor               | Acción                             | Sistema             | Resultado          |
| ---- | ------------------- | ---------------------------------- | ------------------- | ------------------ |
| 1    | System              | Compra completada (BillingService) | OrderCompletedEvent | Evento recibido    |
| 2    | Scheduler           | Espera 7 días                      | Timer               | Tiempo cumplido    |
| 3    | Handler             | Verifica no existe review          | Check existing      | Sin review         |
| 4    | Handler             | Crea ReviewRequest                 | new ReviewRequest() | Solicitud creada   |
| 5    | Handler             | Genera token único                 | Guid.NewGuid()      | Token generado     |
| 6    | Handler             | Calcula expiración                 | +30 días            | ExpiresAt          |
| 7    | Repository          | Persiste solicitud                 | INSERT              | Guardado           |
| 8    | NotificationService | Envía email                        | Publish event       | Email enviado      |
| 9    | Email               | Contiene link único                | /review/{token}     | Link en email      |
| 10   | System              | Programa recordatorio              | Timer +7 días       | Reminder scheduled |

#### Email Template

```
Asunto: ¿Cómo fue tu experiencia con [SellerName]?

Hola [BuyerName],

Esperamos que estés disfrutando tu nuevo vehículo [VehicleTitle].

Nos encantaría saber cómo fue tu experiencia con [SellerName].
Tu opinión ayuda a otros compradores a tomar mejores decisiones.

[⭐ Escribir mi review]

Solo toma 2 minutos. ¡Gracias por ser parte de OKLA!

Este link expira en 30 días.
```

---

## 🔔 Eventos de Dominio (RabbitMQ)

### Eventos Publicados

| Evento                     | Exchange         | Routing Key        | Payload                        |
| -------------------------- | ---------------- | ------------------ | ------------------------------ |
| `ReviewCreatedEvent`       | `reviews.events` | `review.created`   | ReviewId, SellerId, Rating     |
| `ReviewUpdatedEvent`       | `reviews.events` | `review.updated`   | ReviewId, OldRating, NewRating |
| `ReviewDeletedEvent`       | `reviews.events` | `review.deleted`   | ReviewId, SellerId             |
| `ReviewModeratedEvent`     | `reviews.events` | `review.moderated` | ReviewId, IsApproved           |
| `ReviewResponseAddedEvent` | `reviews.events` | `review.responded` | ReviewId, SellerId             |
| `BadgeEarnedEvent`         | `reviews.events` | `badge.earned`     | SellerId, BadgeType            |
| `BadgeRevokedEvent`        | `reviews.events` | `badge.revoked`    | SellerId, BadgeType            |
| `ReviewRequestSentEvent`   | `reviews.events` | `request.sent`     | RequestId, BuyerId             |

### Eventos Consumidos

| Evento                | Origen         | Acción                        |
| --------------------- | -------------- | ----------------------------- |
| `OrderCompletedEvent` | BillingService | Programar solicitud de review |
| `UserDeletedEvent`    | UserService    | Anonimizar reviews            |

---

## ⚠️ Reglas de Negocio

| #   | Regla                  | Descripción                                                   |
| --- | ---------------------- | ------------------------------------------------------------- |
| 1   | Rating 1-5             | Solo valores enteros 1-5 permitidos                           |
| 2   | Una review por compra  | Un usuario no puede dejar múltiples reviews al mismo vendedor |
| 3   | Solo compradores       | Solo usuarios que compraron pueden dejar reviews              |
| 4   | 24h para editar        | Reviews solo editables en primeras 24 horas                   |
| 5   | No auto-voto           | No se puede votar útil una review propia                      |
| 6   | Moderación             | Reviews con palabras ofensivas requieren aprobación           |
| 7   | Badge expiration       | Badges se recalculan mensualmente                             |
| 8   | Mínimo contenido       | Review debe tener mínimo 20 caracteres                        |
| 9   | Máximo 3 recordatorios | Solo 3 emails de recordatorio por solicitud                   |
| 10  | Expiración 30 días     | Solicitudes expiran después de 30 días                        |

---

## ❌ Códigos de Error

| Código      | HTTP Status | Mensaje           | Causa                    |
| ----------- | ----------- | ----------------- | ------------------------ |
| `REV_001`   | 404         | Review not found  | Review no existe         |
| `REV_002`   | 400         | Already reviewed  | Ya existe review         |
| `REV_003`   | 400         | Invalid rating    | Rating fuera de 1-5      |
| `REV_004`   | 403         | Not authorized    | No es autor de la review |
| `REV_005`   | 400         | Edit time expired | Más de 24h               |
| `REV_006`   | 400         | Cannot vote own   | Auto-voto no permitido   |
| `REV_007`   | 400         | Content too short | Menos de 20 caracteres   |
| `BADGE_001` | 404         | Seller not found  | Vendedor no existe       |
| `REQ_001`   | 400         | Already requested | Solicitud ya enviada     |
| `REQ_002`   | 400         | Request expired   | Token expirado           |

---

## ⚙️ Configuración

### appsettings.json

```json
{
  "ReviewSettings": {
    "MinContentLength": 20,
    "MaxContentLength": 2000,
    "EditWindowHours": 24,
    "RequestExpirationDays": 30,
    "ReminderIntervalDays": 7,
    "MaxReminders": 3,
    "DaysAfterPurchaseToRequest": 7
  },
  "BadgeSettings": {
    "TopRatedMinRating": 4.8,
    "TopRatedMinReviews": 10,
    "TrustedDealerMinMonths": 6,
    "TrustedDealerMinPositive": 0.95,
    "FiveStarMinReviews": 5,
    "VolumeLeaderMinReviews": 50,
    "RecalculationIntervalHours": 24
  },
  "FraudDetection": {
    "MinTrustScore": 50,
    "SameIPDaysThreshold": 30,
    "MaxReviewsPerIP": 5
  }
}
```

---

## 📈 Métricas Prometheus

| Métrica                           | Tipo    | Labels           | Descripción                      |
| --------------------------------- | ------- | ---------------- | -------------------------------- |
| `reviews_total`                   | Counter | rating, verified | Total de reviews                 |
| `reviews_avg_rating`              | Gauge   | seller_id        | Rating promedio por vendedor     |
| `reviews_moderation_pending`      | Gauge   | -                | Reviews pendientes de moderación |
| `badges_total`                    | Gauge   | badge_type       | Total de badges por tipo         |
| `review_requests_sent`            | Counter | status           | Solicitudes enviadas             |
| `review_requests_completion_rate` | Gauge   | -                | Tasa de conversión               |
| `helpful_votes_total`             | Counter | -                | Total de votos útiles            |

---

## 📚 Referencias

- [ReviewsController](../../backend/ReviewService/ReviewService.Api/Controllers/ReviewsController.cs)
- [Review Entity](../../backend/ReviewService/ReviewService.Domain/Entities/Review.cs)
- [SellerBadge Entity](../../backend/ReviewService/ReviewService.Domain/Entities/SellerBadge.cs)
- [ReviewRequest Entity](../../backend/ReviewService/ReviewService.Domain/Entities/ReviewRequest.cs)

---

**Última actualización:** Enero 21, 2026  
**Versión:** 1.0.0
