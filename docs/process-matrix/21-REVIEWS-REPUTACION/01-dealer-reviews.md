# ⭐ Dealer Reviews & Ratings

> **Código:** REVIEW-001, REVIEW-002, REVIEW-003  
> **Versión:** 1.0  
> **Última actualización:** Enero 21, 2026  
> **Criticidad:** 🔴 ALTA (Confianza y transparencia)  
> **Origen:** CarGurus, Cars.com, Google

---

## 📋 Información General

| Campo             | Valor                                                                          |
| ----------------- | ------------------------------------------------------------------------------ |
| **Servicio**      | ReviewService (NUEVO)                                                          |
| **Puerto**        | 5091                                                                           |
| **Base de Datos** | `reviewservice`                                                                |
| **Dependencias**  | UserService, DealerManagementService, VehiclesSaleService, NotificationService |

---

## 🎯 Objetivo del Proceso

1. **Transparencia:** Compradores ven experiencias reales
2. **Confianza:** Reviews verificados de compras reales
3. **Accountability:** Dealers se esfuerzan por buen servicio
4. **SEO:** Contenido generado por usuarios

---

## 📡 Endpoints

| Método   | Endpoint                                 | Descripción                | Auth      |
| -------- | ---------------------------------------- | -------------------------- | --------- |
| `GET`    | `/api/reviews/dealer/{dealerId}`         | Reviews de un dealer       | ❌        |
| `GET`    | `/api/reviews/dealer/{dealerId}/summary` | Resumen de ratings         | ❌        |
| `POST`   | `/api/reviews/dealer/{dealerId}`         | Crear review               | ✅        |
| `PUT`    | `/api/reviews/{id}`                      | Editar mi review           | ✅        |
| `DELETE` | `/api/reviews/{id}`                      | Eliminar mi review         | ✅        |
| `POST`   | `/api/reviews/{id}/helpful`              | Marcar como útil           | ✅        |
| `POST`   | `/api/reviews/{id}/report`               | Reportar review            | ✅        |
| `POST`   | `/api/reviews/{id}/response`             | Dealer responde            | ✅ Dealer |
| `GET`    | `/api/reviews/pending`                   | Reviews pendientes (admin) | ✅ Admin  |
| `POST`   | `/api/reviews/{id}/moderate`             | Moderar review             | ✅ Admin  |

---

## 🗃️ Entidades

### DealerReview

```csharp
public class DealerReview
{
    public Guid Id { get; set; }

    // Relaciones
    public Guid DealerId { get; set; }
    public Guid ReviewerId { get; set; }
    public Guid? VehicleId { get; set; }  // Vehículo comprado (si aplica)
    public Guid? TransactionId { get; set; }  // Transacción verificada

    // Ratings (1-5 estrellas)
    public int OverallRating { get; set; }
    public int CustomerServiceRating { get; set; }
    public int TransparencyRating { get; set; }
    public int ValueForMoneyRating { get; set; }
    public int FacilitiesRating { get; set; }

    // Contenido
    public string Title { get; set; }
    public string Content { get; set; }
    public List<string> Pros { get; set; }
    public List<string> Cons { get; set; }

    // Tipo de experiencia
    public ReviewExperienceType ExperienceType { get; set; }
    public bool PurchaseVerified { get; set; }

    // Respuesta del dealer
    public DealerResponse DealerResponse { get; set; }

    // Engagement
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
    public List<Guid> HelpfulByUsers { get; set; }

    // Moderación
    public ReviewStatus Status { get; set; }
    public string ModerationReason { get; set; }
    public Guid? ModeratedBy { get; set; }
    public DateTime? ModeratedAt { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UserAgent { get; set; }
    public string IpAddress { get; set; }
}

public enum ReviewExperienceType
{
    Purchased,           // Compró vehículo
    TestDrove,           // Solo test drive
    Inquired,            // Solo consultó
    ServiceVisit,        // Servicio/mantenimiento
    TradeIn              // Trade-in
}

public enum ReviewStatus
{
    Pending,             // Esperando moderación
    Approved,            // Publicado
    Rejected,            // Rechazado
    Hidden,              // Oculto por el usuario
    Flagged              // Reportado, bajo revisión
}

public class DealerResponse
{
    public Guid Id { get; set; }
    public Guid ReviewId { get; set; }
    public Guid ResponderId { get; set; }  // Quién del dealer respondió
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### DealerRatingSummary

```csharp
public class DealerRatingSummary
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }

    // Promedios
    public decimal OverallAverage { get; set; }
    public decimal CustomerServiceAverage { get; set; }
    public decimal TransparencyAverage { get; set; }
    public decimal ValueForMoneyAverage { get; set; }
    public decimal FacilitiesAverage { get; set; }

    // Conteos
    public int TotalReviews { get; set; }
    public int VerifiedReviews { get; set; }
    public int ReviewsWithResponse { get; set; }

    // Distribución
    public int FiveStarCount { get; set; }
    public int FourStarCount { get; set; }
    public int ThreeStarCount { get; set; }
    public int TwoStarCount { get; set; }
    public int OneStarCount { get; set; }

    // Trends
    public decimal RatingChange30Days { get; set; }
    public int NewReviews30Days { get; set; }

    // Ranking
    public int RankInCity { get; set; }
    public int RankOverall { get; set; }

    public DateTime LastUpdated { get; set; }
}
```

---

## 📊 Proceso REVIEW-001: Comprador Deja Review

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: REVIEW-001 - Comprador Deja Review de Dealer                  │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG (comprador)                                   │
│ Sistemas: ReviewService, UserService, NotificationService              │
│ Triggers: Post-compra, Invitación por email, Voluntario                │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                  | Sistema             | Actor     | Evidencia           | Código     |
| ---- | ------- | --------------------------------------- | ------------------- | --------- | ------------------- | ---------- |
| 1    | 1.1     | **Trigger: Solicitud de review**        | Sistema             | SYS-NOTIF | **Email sent**      | EVD-COMM   |
| 1    | 1.2     | O: Usuario va a perfil del dealer       | Frontend            | USR-REG   | Page view           | EVD-LOG    |
| 1    | 1.3     | Click "Escribir Review"                 | Frontend            | USR-REG   | CTA clicked         | EVD-LOG    |
| 2    | 2.1     | Verificar elegibilidad                  | ReviewService       | Sistema   | Eligibility check   | EVD-LOG    |
| 2    | 2.2     | ¿Tuvo interacción con dealer?           | ReviewService       | Sistema   | Interaction check   | EVD-LOG    |
| 2    | 2.3     | Si compró: Marcar como "Verified"       | ReviewService       | Sistema   | Verified flag       | EVD-LOG    |
| 3    | 3.1     | **Mostrar formulario de review**        | Frontend            | Sistema   | **Form shown**      | EVD-SCREEN |
| 3    | 3.2     | Seleccionar tipo de experiencia         | Frontend            | USR-REG   | Experience type     | EVD-LOG    |
| 4    | 4.1     | **Rating general (1-5 estrellas)**      | Frontend            | USR-REG   | **Rating input**    | EVD-LOG    |
| 4    | 4.2     | **Rating servicio al cliente**          | Frontend            | USR-REG   | **Rating input**    | EVD-LOG    |
| 4    | 4.3     | **Rating transparencia**                | Frontend            | USR-REG   | **Rating input**    | EVD-LOG    |
| 4    | 4.4     | **Rating valor por dinero**             | Frontend            | USR-REG   | **Rating input**    | EVD-LOG    |
| 4    | 4.5     | **Rating instalaciones**                | Frontend            | USR-REG   | **Rating input**    | EVD-LOG    |
| 5    | 5.1     | Ingresar título del review              | Frontend            | USR-REG   | Title input         | EVD-LOG    |
| 5    | 5.2     | **Escribir contenido del review**       | Frontend            | USR-REG   | **Content input**   | EVD-LOG    |
| 5    | 5.3     | Agregar pros (opcional)                 | Frontend            | USR-REG   | Pros input          | EVD-LOG    |
| 5    | 5.4     | Agregar contras (opcional)              | Frontend            | USR-REG   | Cons input          | EVD-LOG    |
| 6    | 6.1     | Click "Publicar Review"                 | Frontend            | USR-REG   | Submit clicked      | EVD-LOG    |
| 6    | 6.2     | **POST /api/reviews/dealer/{dealerId}** | Gateway             | USR-REG   | **Request**         | EVD-AUDIT  |
| 7    | 7.1     | **Validar contenido**                   | ReviewService       | Sistema   | **Validation**      | EVD-LOG    |
| 7    | 7.2     | Filtro de spam                          | ReviewService       | Sistema   | Spam check          | EVD-LOG    |
| 7    | 7.3     | Filtro de palabras prohibidas           | ReviewService       | Sistema   | Profanity check     | EVD-LOG    |
| 7    | 7.4     | Verificar no es duplicado               | ReviewService       | Sistema   | Duplicate check     | EVD-LOG    |
| 8    | 8.1     | **Crear DealerReview**                  | ReviewService       | Sistema   | **Review created**  | EVD-AUDIT  |
| 8    | 8.2     | Status = Pending o Approved (auto)      | ReviewService       | Sistema   | Status set          | EVD-LOG    |
| 9    | 9.1     | **Actualizar DealerRatingSummary**      | ReviewService       | Sistema   | **Summary updated** | EVD-AUDIT  |
| 9    | 9.2     | Recalcular promedios                    | ReviewService       | Sistema   | Averages calc       | EVD-LOG    |
| 10   | 10.1    | **Notificar al dealer**                 | NotificationService | SYS-NOTIF | **Dealer notified** | EVD-COMM   |
| 10   | 10.2    | Email + In-app notification             | NotificationService | Sistema   | Notification sent   | EVD-LOG    |
| 11   | 11.1    | Confirmar al reviewer                   | NotificationService | SYS-NOTIF | Confirmation        | EVD-COMM   |
| 12   | 12.1    | **Audit trail**                         | AuditService        | Sistema   | Complete audit      | EVD-AUDIT  |

### Evidencia de Review

```json
{
  "processCode": "REVIEW-001",
  "review": {
    "id": "rev-12345",
    "dealer": {
      "id": "dealer-001",
      "name": "AutoMax RD",
      "previousRating": 4.3,
      "previousReviewCount": 45
    },
    "reviewer": {
      "id": "user-001",
      "name": "Juan P.",
      "memberSince": "2025-06-15"
    },
    "ratings": {
      "overall": 5,
      "customerService": 5,
      "transparency": 4,
      "valueForMoney": 5,
      "facilities": 4
    },
    "content": {
      "title": "Excelente experiencia de compra",
      "body": "Compré un Toyota Corolla 2023 y la experiencia fue increíble. El vendedor Carlos fue muy profesional y transparente con todo el proceso. Me explicaron todas las opciones de financiamiento y no hubo presión para comprar. El precio final fue el que acordamos, sin cargos ocultos. Recomiendo 100%.",
      "pros": [
        "Transparentes con el precio",
        "Vendedores profesionales",
        "Buen inventario"
      ],
      "cons": ["El estacionamiento es pequeño"]
    },
    "experience": {
      "type": "Purchased",
      "vehicleId": "veh-67890",
      "vehicleTitle": "Toyota Corolla 2023",
      "purchaseVerified": true,
      "purchaseDate": "2026-01-15"
    },
    "status": "Approved",
    "validation": {
      "spamScore": 0.05,
      "profanityFound": false,
      "isDuplicate": false,
      "autoApproved": true
    },
    "createdAt": "2026-01-21T10:00:00Z",
    "impact": {
      "newOverallRating": 4.35,
      "newReviewCount": 46
    }
  }
}
```

---

## 📊 Proceso REVIEW-002: Dealer Responde a Review

| Paso | Subpaso | Acción                           | Sistema             | Actor      | Evidencia           | Código    |
| ---- | ------- | -------------------------------- | ------------------- | ---------- | ------------------- | --------- |
| 1    | 1.1     | Dealer ve notificación de review | Dashboard           | USR-DEALER | Notification viewed | EVD-LOG   |
| 1    | 1.2     | Click para ver review            | Dashboard           | USR-DEALER | Review opened       | EVD-LOG   |
| 2    | 2.1     | Click "Responder"                | Dashboard           | USR-DEALER | CTA clicked         | EVD-LOG   |
| 2    | 2.2     | **Escribir respuesta**           | Dashboard           | USR-DEALER | **Response input**  | EVD-LOG   |
| 3    | 3.1     | POST /api/reviews/{id}/response  | Gateway             | USR-DEALER | Request             | EVD-AUDIT |
| 3    | 3.2     | **Validar respuesta**            | ReviewService       | Sistema    | **Validation**      | EVD-LOG   |
| 3    | 3.3     | **Guardar DealerResponse**       | ReviewService       | Sistema    | **Response saved**  | EVD-AUDIT |
| 4    | 4.1     | **Notificar al reviewer**        | NotificationService | SYS-NOTIF  | **Notification**    | EVD-COMM  |
| 5    | 5.1     | Mostrar respuesta públicamente   | Frontend            | Sistema    | Response shown      | EVD-LOG   |

---

## 📊 Proceso REVIEW-003: Moderación de Reviews

| Paso | Subpaso | Acción                               | Sistema             | Actor     | Evidencia          | Código    |
| ---- | ------- | ------------------------------------ | ------------------- | --------- | ------------------ | --------- |
| 1    | 1.1     | Review reportado o spam detectado    | ReviewService       | Sistema   | Flag raised        | EVD-LOG   |
| 1    | 1.2     | Status = Flagged                     | ReviewService       | Sistema   | Status changed     | EVD-LOG   |
| 2    | 2.1     | Admin ve lista de reviews pendientes | Admin Panel         | USR-ADMIN | List viewed        | EVD-LOG   |
| 2    | 2.2     | GET /api/reviews/pending             | Gateway             | USR-ADMIN | Request            | EVD-LOG   |
| 3    | 3.1     | Admin revisa review                  | Admin Panel         | USR-ADMIN | Review examined    | EVD-LOG   |
| 3    | 3.2     | Verificar políticas de contenido     | Admin Panel         | USR-ADMIN | Policy check       | EVD-LOG   |
| 4    | 4.1     | **Decisión: Aprobar o Rechazar**     | Admin Panel         | USR-ADMIN | **Decision**       | EVD-AUDIT |
| 4    | 4.2     | POST /api/reviews/{id}/moderate      | Gateway             | USR-ADMIN | Request            | EVD-AUDIT |
| 5    | 5.1     | **Actualizar status**                | ReviewService       | Sistema   | **Status updated** | EVD-AUDIT |
| 5    | 5.2     | Si rechazado: Notificar al autor     | NotificationService | SYS-NOTIF | Notification       | EVD-COMM  |
| 6    | 6.1     | Si aprobado: Actualizar summary      | ReviewService       | Sistema   | Summary updated    | EVD-LOG   |
| 7    | 7.1     | **Audit trail completo**             | AuditService        | Sistema   | Complete audit     | EVD-AUDIT |

---

## 📱 UI Mockup - Reviews en Perfil de Dealer

```
┌─────────────────────────────────────────────────────────────────────────┐
│                                                                         │
│  ⭐ RESEÑAS DE CLIENTES                                                │
│  ════════════════════════════════════════════════════════════════════  │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                                                                 │   │
│  │   ⭐⭐⭐⭐⭐  4.3 de 5                                          │   │
│  │                                                                 │   │
│  │   Basado en 46 reseñas                                         │   │
│  │                                                                 │   │
│  │   ★★★★★  ████████████████████████  28 (61%)                    │   │
│  │   ★★★★☆  ████████████              12 (26%)                    │   │
│  │   ★★★☆☆  ██                         3 (7%)                     │   │
│  │   ★★☆☆☆  █                          2 (4%)                     │   │
│  │   ★☆☆☆☆                             1 (2%)                     │   │
│  │                                                                 │   │
│  │   📊 Desglose de ratings:                                      │   │
│  │   Servicio al cliente:  ⭐ 4.5                                  │   │
│  │   Transparencia:        ⭐ 4.2                                  │   │
│  │   Valor por dinero:     ⭐ 4.4                                  │   │
│  │   Instalaciones:        ⭐ 4.0                                  │   │
│  │                                                                 │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  Ordenar por: [Más recientes ▼]    Filtrar: [Todas ▼]                  │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  ⭐⭐⭐⭐⭐   ✓ Compra Verificada                               │   │
│  │                                                                 │   │
│  │  "Excelente experiencia de compra"                             │   │
│  │                                                                 │   │
│  │  Compré un Toyota Corolla 2023 y la experiencia fue            │   │
│  │  increíble. El vendedor Carlos fue muy profesional...          │   │
│  │  [Ver más]                                                      │   │
│  │                                                                 │   │
│  │  ✅ Pros: Transparentes, Profesionales, Buen inventario        │   │
│  │  ⚠️ Contras: Estacionamiento pequeño                           │   │
│  │                                                                 │   │
│  │  Juan P. · hace 2 días                                         │   │
│  │                                                                 │   │
│  │  👍 12 personas encontraron esto útil                          │   │
│  │                                                                 │   │
│  │  ┌───────────────────────────────────────────────────────────┐ │   │
│  │  │ 💬 Respuesta de AutoMax RD:                               │ │   │
│  │  │ ¡Gracias por tu reseña, Juan! Nos alegra que hayas       │ │   │
│  │  │ tenido una excelente experiencia. Estamos trabajando     │ │   │
│  │  │ en mejorar el estacionamiento. ¡Disfruta tu Corolla!     │ │   │
│  │  └───────────────────────────────────────────────────────────┘ │   │
│  │                                                                 │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  [Escribir una reseña]                                                 │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📱 UI Mockup - Formulario de Review

```
┌─────────────────────────────────────────────────────────────────────────┐
│                                                                         │
│  ✍️ ESCRIBE TU RESEÑA DE AUTOMAX RD                                   │
│  ════════════════════════════════════════════════════════════════════  │
│                                                                         │
│  ¿Qué tipo de experiencia tuviste? *                                   │
│  ┌────────────────────────────────────────────────────────────────┐    │
│  │ ● Compré un vehículo                                          │    │
│  │ ○ Hice test drive                                             │    │
│  │ ○ Solo consulté                                               │    │
│  │ ○ Servicio/mantenimiento                                      │    │
│  │ ○ Trade-in                                                    │    │
│  └────────────────────────────────────────────────────────────────┘    │
│                                                                         │
│  ¿Cómo calificarías tu experiencia? *                                  │
│                                                                         │
│  Experiencia general:      ☆ ☆ ☆ ☆ ☆                                  │
│  Servicio al cliente:      ☆ ☆ ☆ ☆ ☆                                  │
│  Transparencia:            ☆ ☆ ☆ ☆ ☆                                  │
│  Valor por dinero:         ☆ ☆ ☆ ☆ ☆                                  │
│  Instalaciones:            ☆ ☆ ☆ ☆ ☆                                  │
│                                                                         │
│  Título de tu reseña *                                                 │
│  ┌────────────────────────────────────────────────────────────────┐    │
│  │ Excelente experiencia de compra                               │    │
│  └────────────────────────────────────────────────────────────────┘    │
│                                                                         │
│  Cuéntanos tu experiencia *                                            │
│  ┌────────────────────────────────────────────────────────────────┐    │
│  │ Compré un Toyota Corolla 2023 y la experiencia fue            │    │
│  │ increíble...                                                   │    │
│  │                                                                │    │
│  │                                                                │    │
│  └────────────────────────────────────────────────────────────────┘    │
│  Mínimo 50 caracteres (actualmente: 67)                                │
│                                                                         │
│  ¿Qué te gustó? (opcional)                                             │
│  ┌────────────────────────────────────────────────────────────────┐    │
│  │ + Agregar punto positivo                                      │    │
│  └────────────────────────────────────────────────────────────────┘    │
│                                                                         │
│  ¿Qué podría mejorar? (opcional)                                       │
│  ┌────────────────────────────────────────────────────────────────┐    │
│  │ + Agregar punto a mejorar                                     │    │
│  └────────────────────────────────────────────────────────────────┘    │
│                                                                         │
│  ☑️ Confirmo que mi reseña es honesta y basada en mi experiencia      │
│                                                                         │
│          ┌─────────────────────────────────────┐                       │
│          │    📝 PUBLICAR RESEÑA               │                       │
│          └─────────────────────────────────────┘                       │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📊 Métricas Prometheus

```yaml
# Reviews
reviews_submitted_total
reviews_approved_total
reviews_rejected_total{reason}
reviews_response_rate

# Ratings
dealer_rating_average
dealer_rating_distribution{stars}
rating_change_30days

# Engagement
review_helpful_votes_total
review_views_total
review_click_through_rate

# Moderation
reviews_pending_moderation
moderation_time_avg_hours
false_positive_rate
```

---

## 🔗 Referencias

- [00-ANALISIS-COMPETITIVO.md](../00-ANALISIS-COMPETITIVO.md)
- [02-USUARIOS-DEALERS/01-dealer-management.md](../02-USUARIOS-DEALERS/01-dealer-management.md)
