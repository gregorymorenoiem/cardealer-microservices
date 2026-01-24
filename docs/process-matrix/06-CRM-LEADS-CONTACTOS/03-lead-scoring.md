# 🎯 Lead Scoring Service - Matriz de Procesos

> **Servicio:** LeadScoringService  
> **Puerto:** 5055  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 📊 Resumen de Implementación

| Componente    | Total | Implementado | Pendiente | Estado |
| ------------- | ----- | ------------ | --------- | ------ |
| Controllers   | 1     | 0            | 1         | 🔴     |
| SCORE-CALC-\* | 5     | 0            | 5         | 🔴     |
| SCORE-ML-\*   | 4     | 0            | 4         | 🔴     |
| SCORE-RULE-\* | 6     | 0            | 6         | 🔴     |
| SCORE-SEG-\*  | 3     | 0            | 3         | 🔴     |
| Tests         | 0     | 0            | 12        | 🔴     |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## 1. Información General

### 1.1 Descripción

Sistema de calificación de leads basado en Machine Learning que asigna puntuaciones (0-100) a cada prospecto para priorizar el seguimiento. Utiliza señales de comportamiento, demográficas y de engagement para predecir la probabilidad de conversión.

### 1.2 Modelo de Scoring

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    MODELO DE LEAD SCORING                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   SEÑALES DE ENTRADA                    SCORE OUTPUT                     │
│   ──────────────────                    ────────────                     │
│                                                                          │
│   Comportamiento (40%)                                                   │
│   ├── Vehículos vistos                  ┌──────────────────────────────┐│
│   ├── Tiempo en página                  │   0-30   │ COLD   │ 🔵      ││
│   ├── Favoritos guardados               ├──────────┼────────┼─────────┤│
│   ├── Comparaciones                     │  31-60   │ WARM   │ 🟡      ││
│   └── Búsquedas realizadas              ├──────────┼────────┼─────────┤│
│                                         │  61-80   │ HOT    │ 🟠      ││
│   Engagement (30%)                      ├──────────┼────────┼─────────┤│
│   ├── Mensajes enviados                 │  81-100  │ READY  │ 🔴      ││
│   ├── Solicitudes de contacto           └──────────┴────────┴─────────┘│
│   ├── Test drives agendados                                             │
│   └── Respuestas a seguimiento                                          │
│                                                                          │
│   Perfil (20%)                                                           │
│   ├── Verificación completada                                           │
│   ├── Datos de contacto completos                                       │
│   └── Historial en plataforma                                           │
│                                                                          │
│   Intención (10%)                                                        │
│   ├── Precio de vehículos vistos                                        │
│   ├── Financiamiento consultado                                         │
│   └── Urgencia indicada                                                 │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 1.3 Dependencias

| Servicio             | Propósito                  |
| -------------------- | -------------------------- |
| EventTrackingService | Eventos de comportamiento  |
| UserService          | Datos del usuario          |
| LeadService          | Gestión de leads           |
| MLTrainingService    | Entrenamiento de modelos   |
| NotificationService  | Alertas de leads calientes |

---

## 2. Endpoints API

### 2.1 LeadScoringController

| Método | Endpoint                                 | Descripción        | Auth | Roles  |
| ------ | ---------------------------------------- | ------------------ | ---- | ------ |
| `GET`  | `/api/lead-scoring/{leadId}`             | Score de un lead   | ✅   | Dealer |
| `GET`  | `/api/lead-scoring/vehicle/{vehicleId}`  | Leads por vehículo | ✅   | Dealer |
| `POST` | `/api/lead-scoring/{leadId}/recalculate` | Recalcular score   | ✅   | Dealer |
| `GET`  | `/api/lead-scoring/hot-leads`            | Leads calientes    | ✅   | Dealer |
| `GET`  | `/api/lead-scoring/statistics`           | Estadísticas       | ✅   | Dealer |
| `PUT`  | `/api/lead-scoring/thresholds`           | Ajustar umbrales   | ✅   | Admin  |

### 2.2 ScoringEventsController (Interno)

| Método | Endpoint                    | Descripción      | Auth | Roles  |
| ------ | --------------------------- | ---------------- | ---- | ------ |
| `POST` | `/api/scoring-events/track` | Registrar evento | ✅   | System |
| `POST` | `/api/scoring-events/batch` | Registrar batch  | ✅   | System |

---

## 3. Entidades y Enums

### 3.1 LeadCategory (Enum)

```csharp
public enum LeadCategory
{
    Cold = 0,      // 0-30: Bajo interés
    Warm = 1,      // 31-60: Interés moderado
    Hot = 2,       // 61-80: Alto interés
    Ready = 3      // 81-100: Listo para comprar
}
```

### 3.2 LeadScore (Entidad)

```csharp
public class LeadScore
{
    public Guid Id { get; set; }
    public Guid LeadId { get; set; }
    public Guid UserId { get; set; }
    public Guid DealerId { get; set; }

    // Score actual
    public int Score { get; set; }                // 0-100
    public LeadCategory Category { get; set; }

    // Componentes del score
    public int BehaviorScore { get; set; }        // 0-40
    public int EngagementScore { get; set; }      // 0-30
    public int ProfileScore { get; set; }         // 0-20
    public int IntentScore { get; set; }          // 0-10

    // Detalles de comportamiento
    public int VehiclesViewed { get; set; }
    public int TotalTimeOnSite { get; set; }      // Segundos
    public int FavoritesCount { get; set; }
    public int ComparisonsCount { get; set; }
    public int SearchesCount { get; set; }

    // Detalles de engagement
    public int MessagesSent { get; set; }
    public int ContactRequests { get; set; }
    public int TestDriveRequests { get; set; }
    public int ResponsesReceived { get; set; }

    // Tendencia
    public int PreviousScore { get; set; }
    public ScoreTrend Trend { get; set; }
    public double TrendVelocity { get; set; }     // Puntos/día

    // ML Model
    public double ConversionProbability { get; set; }
    public string ModelVersion { get; set; }

    // Timestamps
    public DateTime CalculatedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
}
```

### 3.3 ScoreTrend (Enum)

```csharp
public enum ScoreTrend
{
    Declining = -1,   // Score bajando
    Stable = 0,       // Sin cambio significativo
    Increasing = 1    // Score subiendo
}
```

### 3.4 ScoringEvent (Entidad)

```csharp
public class ScoringEvent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? LeadId { get; set; }
    public ScoringEventType EventType { get; set; }
    public int PointsAwarded { get; set; }

    // Contexto
    public Guid? VehicleId { get; set; }
    public Guid? DealerId { get; set; }
    public string? Metadata { get; set; }         // JSON

    public DateTime OccurredAt { get; set; }
}
```

### 3.5 ScoringEventType (Enum)

```csharp
public enum ScoringEventType
{
    // Comportamiento
    VehicleViewed = 1,           // +1
    VehicleDetailViewed = 2,     // +2
    TimeOnPageBonus = 3,         // +1 por cada 30s
    FavoriteAdded = 4,           // +5
    ComparisonCreated = 5,       // +5
    SearchPerformed = 6,         // +1
    SimilarVehiclesViewed = 7,   // +2

    // Engagement
    MessageSent = 10,            // +10
    ContactRequested = 11,       // +15
    TestDriveRequested = 12,     // +20
    PhoneNumberViewed = 13,      // +8
    WhatsAppClicked = 14,        // +10

    // Perfil
    ProfileCompleted = 20,       // +5
    EmailVerified = 21,          // +3
    PhoneVerified = 22,          // +5
    DocumentsUploaded = 23,      // +10

    // Intención
    FinancingViewed = 30,        // +5
    PriceAlertCreated = 31,      // +8
    UrgencyIndicated = 32,       // +10
    OfferMade = 33               // +25
}
```

---

## 4. Procesos Detallados

### 4.1 SCORE-001: Calcular Score Inicial

| Campo       | Valor                        |
| ----------- | ---------------------------- |
| **ID**      | SCORE-001                    |
| **Nombre**  | Calcular Score de Nuevo Lead |
| **Actor**   | Sistema                      |
| **Trigger** | Evento lead.created          |

#### Flujo del Proceso

| Paso | Acción                       | Sistema              | Validación              |
| ---- | ---------------------------- | -------------------- | ----------------------- |
| 1    | Recibir evento lead.created  | RabbitMQ             | Consumer                |
| 2    | Obtener datos del usuario    | UserService          | Por userId              |
| 3    | Obtener historial de eventos | EventTrackingService | Últimos 30 días         |
| 4    | Calcular BehaviorScore       | LeadScoringService   | Algoritmo               |
| 5    | Calcular EngagementScore     | LeadScoringService   | Algoritmo               |
| 6    | Calcular ProfileScore        | LeadScoringService   | Algoritmo               |
| 7    | Calcular IntentScore         | LeadScoringService   | Algoritmo               |
| 8    | Sumar score total            | LeadScoringService   | 0-100                   |
| 9    | Ejecutar modelo ML           | MLService            | Probabilidad conversión |
| 10   | Determinar categoría         | LeadScoringService   | Cold/Warm/Hot/Ready     |
| 11   | Guardar score                | Database             | LeadScore               |
| 12   | Si Hot/Ready                 | Check                | Notificar dealer        |
| 13   | Publicar evento              | RabbitMQ             | lead.scored             |

#### Algoritmo de Cálculo

```csharp
public int CalculateBehaviorScore(UserBehaviorData data)
{
    int score = 0;

    // Vehículos vistos (max 10 puntos)
    score += Math.Min(data.VehiclesViewed * 2, 10);

    // Tiempo en sitio (max 10 puntos, 1 punto por minuto)
    score += Math.Min(data.TotalTimeMinutes, 10);

    // Favoritos (max 10 puntos)
    score += Math.Min(data.FavoritesCount * 2, 10);

    // Comparaciones (max 5 puntos)
    score += Math.Min(data.ComparisonsCount * 2, 5);

    // Búsquedas (max 5 puntos)
    score += Math.Min(data.SearchesCount, 5);

    return Math.Min(score, 40); // Max 40
}
```

---

### 4.2 SCORE-002: Actualizar Score en Tiempo Real

| Campo       | Valor                              |
| ----------- | ---------------------------------- |
| **ID**      | SCORE-002                          |
| **Nombre**  | Actualización Incremental de Score |
| **Actor**   | Sistema                            |
| **Trigger** | Cualquier evento de usuario        |

#### Flujo del Proceso

| Paso | Acción                     | Sistema            | Validación             |
| ---- | -------------------------- | ------------------ | ---------------------- |
| 1    | Recibir evento             | RabbitMQ           | Cualquier tipo         |
| 2    | Identificar tipo de evento | LeadScoringService | ScoringEventType       |
| 3    | Obtener puntos asignados   | Config             | Por tipo               |
| 4    | Obtener score actual       | Database           | LeadScore              |
| 5    | Aplicar decay de actividad | LeadScoringService | Si inactivo            |
| 6    | Sumar nuevos puntos        | LeadScoringService | Al componente correcto |
| 7    | Recalcular total           | LeadScoringService | Con límites            |
| 8    | Determinar nueva categoría | LeadScoringService | Si cambió              |
| 9    | Calcular tendencia         | LeadScoringService | vs anterior            |
| 10   | Actualizar score           | Database           | Update                 |
| 11   | Si subió a Hot/Ready       | Check              | Alerta al dealer       |
| 12   | Publicar evento            | RabbitMQ           | lead.score_changed     |

#### Puntos por Evento

```json
{
  "VehicleViewed": 1,
  "VehicleDetailViewed": 2,
  "TimeOnPageBonus": 1,
  "FavoriteAdded": 5,
  "ComparisonCreated": 5,
  "SearchPerformed": 1,
  "MessageSent": 10,
  "ContactRequested": 15,
  "TestDriveRequested": 20,
  "PhoneNumberViewed": 8,
  "WhatsAppClicked": 10,
  "FinancingViewed": 5,
  "PriceAlertCreated": 8,
  "OfferMade": 25
}
```

---

### 4.3 SCORE-003: Decay por Inactividad

| Campo       | Valor                         |
| ----------- | ----------------------------- |
| **ID**      | SCORE-003                     |
| **Nombre**  | Aplicar Decay por Inactividad |
| **Actor**   | Sistema (Job diario)          |
| **Trigger** | Cron 04:00 AM                 |

#### Flujo del Proceso

| Paso | Acción                  | Sistema            | Validación              |
| ---- | ----------------------- | ------------------ | ----------------------- |
| 1    | Job diario ejecuta      | SchedulerService   | 04:00 AM                |
| 2    | Obtener leads inactivos | Database           | LastActivityAt > 3 días |
| 3    | Por cada lead           | Loop               | Batch de 100            |
| 4    | Calcular días inactivos | LeadScoringService | Desde LastActivity      |
| 5    | Aplicar factor de decay | LeadScoringService | -2% por día             |
| 6    | Límite mínimo           | LeadScoringService | No bajar de 10          |
| 7    | Actualizar score        | Database           | Nuevo valor             |
| 8    | Recategorizar           | LeadScoringService | Si bajó categoría       |
| 9    | Log de decays           | Database           | Para auditoría          |

#### Fórmula de Decay

```csharp
public int ApplyDecay(int currentScore, int daysInactive)
{
    if (daysInactive <= 3) return currentScore;

    // -2% por día después de 3 días, máximo -50%
    double decayFactor = Math.Min(0.02 * (daysInactive - 3), 0.5);
    int newScore = (int)(currentScore * (1 - decayFactor));

    return Math.Max(newScore, 10); // Mínimo 10
}
```

---

### 4.4 SCORE-004: Obtener Hot Leads

| Campo       | Valor                           |
| ----------- | ------------------------------- |
| **ID**      | SCORE-004                       |
| **Nombre**  | Obtener Leads Calientes         |
| **Actor**   | Dealer                          |
| **Trigger** | GET /api/lead-scoring/hot-leads |

#### Flujo del Proceso

| Paso | Acción                        | Sistema             | Validación       |
| ---- | ----------------------------- | ------------------- | ---------------- |
| 1    | Dealer accede a dashboard     | Frontend            | CRM tab          |
| 2    | Request hot leads             | LeadScoringService  | Por dealerId     |
| 3    | Filtrar por categoría         | Database            | Hot + Ready      |
| 4    | Ordenar por score             | Database            | DESC             |
| 5    | Incluir tendencia             | LeadScoringService  | Trend + velocity |
| 6    | Enriquecer con datos          | UserService         | Nombre, contacto |
| 7    | Incluir vehículos interesados | VehiclesSaleService | IDs vistos       |
| 8    | Retornar lista                | Response            | Paginada         |

#### Response

```json
{
  "totalHotLeads": 15,
  "leads": [
    {
      "leadId": "uuid",
      "userId": "uuid",
      "userName": "María García",
      "email": "maria@email.com",
      "phone": "829-555-0100",
      "score": 92,
      "category": "Ready",
      "trend": "Increasing",
      "trendVelocity": 5.2,
      "conversionProbability": 0.78,
      "lastActivityAt": "2026-01-21T10:30:00Z",
      "interestedVehicles": [
        { "id": "uuid", "title": "Toyota RAV4 2024", "viewCount": 5 }
      ],
      "recentActions": [
        { "action": "TestDriveRequested", "at": "2026-01-21T10:00:00Z" },
        { "action": "FinancingViewed", "at": "2026-01-21T09:45:00Z" }
      ],
      "suggestedAction": "Llamar inmediatamente - alta probabilidad de cierre"
    }
  ]
}
```

---

## 5. Modelo de Machine Learning

### 5.1 Features del Modelo

| Feature                    | Tipo  | Descripción              |
| -------------------------- | ----- | ------------------------ |
| `vehicles_viewed_count`    | Int   | Vehículos únicos vistos  |
| `avg_time_per_vehicle`     | Float | Segundos promedio        |
| `favorites_count`          | Int   | Favoritos guardados      |
| `comparisons_count`        | Int   | Comparaciones realizadas |
| `messages_sent`            | Int   | Mensajes a vendedores    |
| `test_drive_requests`      | Int   | Test drives solicitados  |
| `days_since_registration`  | Int   | Antigüedad en plataforma |
| `profile_completeness`     | Float | 0-1                      |
| `avg_vehicle_price_viewed` | Float | Precio promedio visto    |
| `price_range_consistency`  | Float | Consistencia de rango    |
| `financing_interest`       | Bool  | Vio financiamiento       |
| `response_rate`            | Float | Tasa de respuesta        |
| `session_frequency`        | Float | Sesiones por semana      |

### 5.2 Salida del Modelo

```python
# Modelo: XGBoost Classifier
# Output: Probabilidad de conversión en próximos 30 días

model_output = {
    "conversion_probability": 0.78,
    "confidence": 0.92,
    "top_features": [
        ("test_drive_requests", 0.25),
        ("messages_sent", 0.18),
        ("avg_time_per_vehicle", 0.15)
    ]
}
```

---

## 6. Reglas de Negocio

### 6.1 Umbrales por Categoría

| Categoría  | Rango  | Acción Recomendada       |
| ---------- | ------ | ------------------------ |
| Cold (🔵)  | 0-30   | Nurturing automático     |
| Warm (🟡)  | 31-60  | Follow-up semanal        |
| Hot (🟠)   | 61-80  | Contactar en 24h         |
| Ready (🔴) | 81-100 | Contactar inmediatamente |

### 6.2 Alertas Automáticas

| Evento            | Condición                | Notificación          |
| ----------------- | ------------------------ | --------------------- |
| Lead Ready        | Score >= 81              | Push + Email a dealer |
| Score subió a Hot | Score pasó de Warm a Hot | Push a dealer         |
| Alta velocidad    | +10 puntos en 1h         | Push inmediato        |
| Inactividad       | 7 días sin actividad     | Reminder a dealer     |

---

## 7. Eventos RabbitMQ

| Evento                  | Exchange      | Payload                                |
| ----------------------- | ------------- | -------------------------------------- |
| `lead.scored`           | `lead.events` | `{ leadId, score, category }`          |
| `lead.score_changed`    | `lead.events` | `{ leadId, oldScore, newScore }`       |
| `lead.category_changed` | `lead.events` | `{ leadId, oldCategory, newCategory }` |
| `lead.became_hot`       | `lead.events` | `{ leadId, dealerId, score }`          |
| `lead.became_ready`     | `lead.events` | `{ leadId, dealerId, score }`          |

---

## 8. Métricas

```
# Scores
lead_scores_total{category="cold|warm|hot|ready"}
lead_score_distribution_bucket{le="10|20|...|100"}
lead_score_changes_total{direction="up|down"}

# ML Model
lead_scoring_predictions_total
lead_scoring_accuracy{model_version="v1.0"}
lead_conversion_predictions_total{outcome="correct|incorrect"}

# Processing
lead_scoring_calculation_duration_seconds
lead_scoring_events_processed_total{type="..."}
```

---

## 📚 Referencias

- [02-lead-service.md](02-lead-service.md) - Gestión de leads
- [04-chatbot-service.md](04-chatbot-service.md) - Chatbot
- [01-event-tracking.md](../13-INTEGRACIONES-EXTERNAS/01-event-tracking.md) - Tracking
