# 🎯 Recommendation Service - Matriz de Procesos

> **Servicio:** RecommendationService  
> **Puerto:** 5055  
> **Última actualización:** Enero 25, 2026  
> **Estado:** 🟢 ACTIVO  
> **Estado de Implementación:** ✅ 100% Backend | ✅ 100% UI

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso                  | Backend                     | UI Access             | Observación                      |
| ------------------------ | --------------------------- | --------------------- | -------------------------------- |
| REC-001 Similar Vehicles | ✅ RecommendationController | ✅ VehicleDetailPage  | Sección "Similares"              |
| REC-002 Para Ti          | ✅ RecommendationController | ✅ HomePage           | Sección personalizada            |
| REC-003 Historial        | ✅ RecommendationController | ✅ RecentlyViewedPage | Historial completo con filtros   |
| ML-001 Modelo            | ✅ MLService                | ✅ Backend            | Inferencia funcional             |
| ML-002 Retraining        | ✅ MLTrainingService        | ✅ MLAdminDashboard   | Dashboard completo de modelos ML |

### Rutas UI Existentes ✅

- `/vehicles/:id` → Sección "Vehículos similares"
- `/` → HomePage con sección "Para ti" (cuando hay historial)
- `/recently-viewed` → Página de vehículos vistos recientemente
- `/admin/ml/models` → Dashboard de modelos ML (admin)

**Verificación Backend:** RecommendationService existe en `/backend/RecommendationService/` ✅

---

## 📊 Resumen de Implementación

| Componente            | Total | Implementado | Pendiente | Estado  |
| --------------------- | ----- | ------------ | --------- | ------- |
| **Controllers**       | 2     | 2            | 0         | ✅ 100% |
| **Procesos (REC-\*)** | 5     | 3            | 2         | 🟡 60%  |
| **Procesos (ML-\*)**  | 4     | 2            | 2         | 🟡 50%  |
| **Tests Unitarios**   | 12    | 8            | 4         | 🟡 67%  |

### Leyenda de Estados

- ✅ **IMPLEMENTADO Y PROBADO**: Código completo con tests
- 🟢 **IMPLEMENTADO**: Código completo, falta testing
- 🟡 **EN PROGRESO**: Implementación parcial
- 🔴 **PENDIENTE**: No implementado

---

## 1. Información General

### 1.1 Descripción

Sistema de recomendaciones personalizadas basado en Machine Learning para OKLA. Analiza el comportamiento del usuario, historial de búsquedas, favoritos e interacciones para sugerir vehículos relevantes.

### 1.2 Dependencias

| Servicio             | Propósito                |
| -------------------- | ------------------------ |
| UserBehaviorService  | Datos de comportamiento  |
| VehiclesSaleService  | Información de vehículos |
| FeatureStoreService  | Features para ML         |
| EventTrackingService | Eventos de usuario       |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                   RecommendationService Architecture                         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   Data Sources                       Core Service                            │
│   ┌────────────────┐                ┌────────────────────────────────┐      │
│   │ UserBehavior   │──┐             │      RecommendationService       │      │
│   │ Service        │  │             │  ┌──────────────────────────┐   │      │
│   └────────────────┘  │             │  │ ML Algorithms            │   │      │
│   ┌────────────────┐  │             │  │ • Collaborative Filter   │   │      │
│   │ VehiclesSale   │──┼────────────▶│  │ • Content-Based          │   │      │
│   │ Service        │  │             │  │ • Hybrid Approach        │   │      │
│   └────────────────┘  │             │  │ • Similar Items          │   │      │
│   ┌────────────────┐  │             │  └──────────────────────────┘   │      │
│   │ FeatureStore   │──┤             │  ┌──────────────────────────┐   │      │
│   │ Service        │  │             │  │ Application (CQRS)       │   │      │
│   └────────────────┘  │             │  │ • GetForYouQuery         │   │      │
│   ┌────────────────┐  │             │  │ • GetSimilarQuery        │   │      │
│   │ EventTracking  │──┘             │  │ • RecordInteractionCmd   │   │      │
│   │ Service        │               │  │ • RegenerateRecsCommand  │   │      │
│   └────────────────┘               │  └──────────────────────────┘   │      │
│                                    └────────────────────────────────┘      │
│                                                    │                        │
│   Consumers                        ┌───────────────┼───────────────┐        │
│   ┌────────────────┐               ▼               ▼               ▼        │
│   │ Web/Mobile     │◀───── ┌────────────┐  ┌────────────┐  ┌────────────┐  │
│   │ (For You)      │       │ PostgreSQL │  │   Redis    │  │  RabbitMQ  │  │
│   └────────────────┘       │ (User Prefs│  │ (Cached    │  │ (Tracking  │  │
│   ┌────────────────┐       │  Recs)     │  │  Recs)     │  │  Events)   │  │
│   │ Vehicle Detail │◀───── └────────────┘  └────────────┘  └────────────┘  │
│   │ (Similar)      │                                                        │
│   └────────────────┘                                                        │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 1.4 Algoritmos

- **Collaborative Filtering**: Usuarios similares
- **Content-Based**: Características de vehículos
- **Hybrid**: Combinación de ambos
- **Similar Items**: Vehículos similares al visto

---

## 2. Endpoints API

| Método | Endpoint                                   | Descripción                    | Auth | Roles |
| ------ | ------------------------------------------ | ------------------------------ | ---- | ----- |
| `GET`  | `/api/recommendations/for-you`             | Recomendaciones personalizadas | ✅   | User  |
| `GET`  | `/api/recommendations/similar/{vehicleId}` | Vehículos similares            | ❌   | -     |
| `POST` | `/api/recommendations/generate`            | Forzar regeneración            | ✅   | User  |
| `POST` | `/api/recommendations/{id}/viewed`         | Marcar como vista              | ✅   | User  |
| `POST` | `/api/recommendations/{id}/clicked`        | Marcar como clickeada          | ✅   | User  |
| `GET`  | `/api/recommendations/preferences`         | Preferencias del usuario       | ✅   | User  |
| `GET`  | `/api/interactions`                        | Historial de interacciones     | ✅   | User  |
| `POST` | `/api/interactions`                        | Registrar interacción          | ✅   | User  |

---

## 3. Entidades y Enums

### 3.1 RecommendationType (Enum)

```csharp
public enum RecommendationType
{
    ForYou = 0,           // Personalizadas
    SimilarItems = 1,     // Similar al visto
    TrendingNow = 2,      // Tendencias actuales
    RecentlyViewed = 3,   // Basado en visto recientemente
    BasedOnFavorites = 4, // Basado en favoritos
    PriceDrops = 5,       // Bajas de precio
    NewArrivals = 6,      // Recién llegados
    PopularInArea = 7     // Popular en tu zona
}
```

### 3.2 InteractionType (Enum)

```csharp
public enum InteractionType
{
    View = 0,             // Vio el vehículo
    Click = 1,            // Click en la tarjeta
    DetailView = 2,       // Vio detalles completos
    Favorite = 3,         // Agregó a favoritos
    Contact = 4,          // Contactó al vendedor
    Share = 5,            // Compartió
    Compare = 6,          // Agregó a comparación
    Search = 7,           // Búsqueda realizada
    Filter = 8            // Filtros aplicados
}
```

### 3.3 Recommendation (Entidad)

```csharp
public class Recommendation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid VehicleId { get; set; }
    public RecommendationType Type { get; set; }
    public decimal Score { get; set; }              // 0-1 confianza
    public string Reason { get; set; }              // Explicación
    public int Rank { get; set; }                   // Posición en lista
    public bool IsViewed { get; set; }
    public bool IsClicked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ViewedAt { get; set; }
    public DateTime? ClickedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

### 3.4 UserPreference (Entidad)

```csharp
public class UserPreference
{
    public Guid UserId { get; set; }

    // Preferencias extraídas
    public List<string> PreferredMakes { get; set; }
    public List<string> PreferredBodyTypes { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public int MinYear { get; set; }
    public int MaxYear { get; set; }
    public int MaxMileage { get; set; }
    public List<string> PreferredColors { get; set; }
    public List<string> PreferredFeatures { get; set; }

    // Metadata
    public int TotalInteractions { get; set; }
    public DateTime LastUpdated { get; set; }
    public decimal ConfidenceScore { get; set; }
}
```

---

## 4. Procesos Detallados

### 4.1 REC-001: Obtener Recomendaciones Personalizadas

| Campo       | Valor                                    |
| ----------- | ---------------------------------------- |
| **ID**      | REC-001                                  |
| **Nombre**  | For You - Recomendaciones Personalizadas |
| **Actor**   | Usuario autenticado                      |
| **Trigger** | GET /api/recommendations/for-you         |

#### Flujo del Proceso

| Paso | Acción                                | Sistema               | Validación            |
| ---- | ------------------------------------- | --------------------- | --------------------- |
| 1    | Usuario solicita recomendaciones      | Frontend              | Token JWT válido      |
| 2    | Extraer UserId del token              | RecommendationService | Claim presente        |
| 3    | Verificar cache de recomendaciones    | Redis                 | TTL no expirado       |
| 4    | Si no hay cache, obtener preferencias | Database              | UserPreference        |
| 5    | Obtener interacciones recientes       | UserBehaviorService   | Últimos 30 días       |
| 6    | Consultar FeatureStore                | FeatureStoreService   | Embeddings de usuario |
| 7    | Ejecutar modelo ML                    | ML Model              | Score predictions     |
| 8    | Filtrar vehículos no disponibles      | VehiclesSaleService   | Status = Active       |
| 9    | Ordenar por score                     | RecommendationService | DESC                  |
| 10   | Guardar en cache                      | Redis                 | TTL 1 hora            |
| 11   | Retornar top N                        | Response              | limit parámetro       |

#### Request

```
GET /api/recommendations/for-you?limit=10
Authorization: Bearer {token}
```

#### Response

```json
{
  "recommendations": [
    {
      "id": "uuid",
      "vehicleId": "uuid",
      "type": "ForYou",
      "score": 0.95,
      "reason": "Similar a tus búsquedas recientes de Toyota RAV4",
      "rank": 1,
      "vehicle": {
        "id": "uuid",
        "title": "Toyota RAV4 2023",
        "price": 1850000,
        "mainImage": "https://...",
        "make": "Toyota",
        "model": "RAV4",
        "year": 2023,
        "mileage": 15000
      }
    }
  ],
  "totalCount": 50,
  "generatedAt": "2026-01-21T10:00:00Z",
  "expiresAt": "2026-01-21T11:00:00Z"
}
```

---

### 4.2 REC-002: Vehículos Similares

| Campo       | Valor                                        |
| ----------- | -------------------------------------------- |
| **ID**      | REC-002                                      |
| **Nombre**  | Similar Vehicles                             |
| **Actor**   | Cualquier usuario                            |
| **Trigger** | GET /api/recommendations/similar/{vehicleId} |

#### Flujo del Proceso

| Paso | Acción                            | Sistema               | Validación                |
| ---- | --------------------------------- | --------------------- | ------------------------- |
| 1    | Usuario ve un vehículo            | Frontend              | VehicleId válido          |
| 2    | Obtener features del vehículo     | VehiclesSaleService   | Make, Model, Year, etc.   |
| 3    | Consultar embeddings del vehículo | FeatureStoreService   | Vector de características |
| 4    | Buscar vecinos más cercanos       | ML Model              | KNN algorithm             |
| 5    | Excluir vehículo actual           | Filter                | vehicleId != source       |
| 6    | Filtrar solo activos              | VehiclesSaleService   | Status = Active           |
| 7    | Calcular similarity score         | Algorithm             | Cosine similarity         |
| 8    | Ordenar por similaridad           | RecommendationService | DESC                      |
| 9    | Retornar top N                    | Response              | limit parámetro           |

#### Criterios de Similaridad

| Factor          | Peso | Descripción           |
| --------------- | ---- | --------------------- |
| Marca           | 25%  | Misma marca = +25     |
| Modelo          | 20%  | Mismo modelo = +20    |
| Año             | 15%  | Diferencia máx 2 años |
| Precio          | 20%  | ±20% del precio       |
| Tipo carrocería | 10%  | SUV, Sedan, etc.      |
| Características | 10%  | Features similares    |

---

### 4.3 REC-003: Registrar Interacción

| Campo       | Valor                  |
| ----------- | ---------------------- |
| **ID**      | REC-003                |
| **Nombre**  | Track User Interaction |
| **Actor**   | Usuario autenticado    |
| **Trigger** | POST /api/interactions |

#### Flujo del Proceso

| Paso | Acción                          | Sistema               | Validación              |
| ---- | ------------------------------- | --------------------- | ----------------------- |
| 1    | Usuario interactúa con vehículo | Frontend              | Evento capturado        |
| 2    | Enviar interacción              | API                   | Async (fire & forget)   |
| 3    | Validar datos                   | RecommendationService | VehicleId existe        |
| 4    | Enriquecer con contexto         | RecommendationService | Device, location, etc.  |
| 5    | Guardar interacción             | Database              | Interaction entity      |
| 6    | Publicar evento                 | RabbitMQ              | interaction.created     |
| 7    | Actualizar UserPreferences      | Async Job             | Recalcular preferencias |
| 8    | Invalidar cache                 | Redis                 | Forzar regeneración     |

#### Request

```json
{
  "vehicleId": "uuid",
  "interactionType": "DetailView",
  "duration": 45,
  "context": {
    "source": "search",
    "searchQuery": "toyota rav4",
    "position": 3
  }
}
```

---

### 4.4 REC-004: Generar Recomendaciones (Batch)

| Campo       | Valor                          |
| ----------- | ------------------------------ |
| **ID**      | REC-004                        |
| **Nombre**  | Generate Recommendations Batch |
| **Actor**   | Sistema (Scheduled Job)        |
| **Trigger** | Cron: cada 4 horas             |

#### Flujo del Proceso

| Paso | Acción                           | Sistema             | Validación                |
| ---- | -------------------------------- | ------------------- | ------------------------- |
| 1    | Job scheduled inicia             | SchedulerService    | Cron expression           |
| 2    | Obtener usuarios activos         | Database            | LastLogin < 7 días        |
| 3    | Por cada usuario (paralelo)      | Loop                | Max 100 concurrentes      |
| 4    | Obtener interacciones            | UserBehaviorService | Últimos 30 días           |
| 5    | Calcular UserPreference          | ML Pipeline         | Feature extraction        |
| 6    | Ejecutar modelo de recomendación | ML Model            | TensorFlow/ONNX           |
| 7    | Generar top 50                   | Algorithm           | Score + diversidad        |
| 8    | Guardar recomendaciones          | Database            | Batch insert              |
| 9    | Actualizar cache                 | Redis               | Pre-warm                  |
| 10   | Publicar métricas                | Prometheus          | recommendations_generated |

---

### 4.5 REC-005: Obtener Preferencias de Usuario

| Campo       | Valor                                |
| ----------- | ------------------------------------ |
| **ID**      | REC-005                              |
| **Nombre**  | Get User Preferences                 |
| **Actor**   | Usuario autenticado                  |
| **Trigger** | GET /api/recommendations/preferences |

#### Response

```json
{
  "userId": "uuid",
  "preferences": {
    "preferredMakes": ["Toyota", "Honda", "Hyundai"],
    "preferredBodyTypes": ["SUV", "Sedan"],
    "priceRange": {
      "min": 800000,
      "max": 2000000
    },
    "yearRange": {
      "min": 2020,
      "max": 2026
    },
    "maxMileage": 50000,
    "preferredColors": ["Blanco", "Negro", "Gris"],
    "preferredFeatures": ["Sunroof", "Leather Seats", "Apple CarPlay"]
  },
  "metadata": {
    "totalInteractions": 156,
    "confidenceScore": 0.87,
    "lastUpdated": "2026-01-21T08:00:00Z"
  },
  "topSearches": ["toyota rav4 2023", "honda crv", "hyundai tucson"]
}
```

---

## 5. Algoritmos de Machine Learning

### 5.1 Collaborative Filtering

```
User-Item Matrix:
              Veh1  Veh2  Veh3  Veh4
User A         5     3     -     1
User B         4     -     4     2
User C         -     4     5     -
User D         3     4     -     4

Predicción para User A en Veh3:
1. Encontrar usuarios similares (B, C)
2. Ponderar sus ratings en Veh3
3. Predecir: (B=4 * sim_B + C=5 * sim_C) / (sim_B + sim_C)
```

### 5.2 Content-Based Filtering

```
Vehicle Features Vector:
[make_encoded, model_encoded, year_normalized, price_normalized,
 mileage_normalized, bodytype_onehot, features_embedding]

User Profile = Average(Liked_Vehicles_Vectors)

Recommendation Score = Cosine_Similarity(User_Profile, Vehicle_Vector)
```

### 5.3 Modelo Híbrido

```
Final_Score = α * Collaborative_Score + β * ContentBased_Score + γ * Popularity_Score

Donde:
- α = 0.4 (peso collaborative)
- β = 0.4 (peso content-based)
- γ = 0.2 (peso popularidad)

Con ajuste por:
- Freshness: Bonus para vehículos nuevos
- Diversity: Penalización para repetir marcas
- Business Rules: Boost para listings patrocinados
```

---

## 6. Reglas de Negocio

### 6.1 Políticas de Recomendación

| Regla                        | Valor                        |
| ---------------------------- | ---------------------------- |
| Máximo por marca             | 30% del total                |
| Mínimo interacciones para ML | 5                            |
| Cold start (nuevos usuarios) | Trending + Popular           |
| Refresh rate                 | Cada 4 horas batch, 1h cache |
| Expiración recomendaciones   | 24 horas                     |

### 6.2 Diversidad de Resultados

```csharp
// Asegurar diversidad en recomendaciones
public List<Recommendation> ApplyDiversity(List<Recommendation> recs)
{
    var result = new List<Recommendation>();
    var makeCount = new Dictionary<string, int>();

    foreach (var rec in recs.OrderByDescending(r => r.Score))
    {
        var make = rec.Vehicle.Make;
        if (!makeCount.ContainsKey(make)) makeCount[make] = 0;

        if (makeCount[make] < maxPerMake)
        {
            result.Add(rec);
            makeCount[make]++;
        }
    }
    return result;
}
```

### 6.3 Cold Start Strategy

| Interacciones | Estrategia                      |
| ------------- | ------------------------------- |
| 0             | Popular en tu ciudad            |
| 1-5           | Trending + Content-based básico |
| 6-20          | Híbrido con peso content-based  |
| 20+           | Híbrido completo                |

---

## 7. Manejo de Errores

| Código | Error                | Mensaje                                          | Acción              |
| ------ | -------------------- | ------------------------------------------------ | ------------------- |
| 400    | InvalidVehicleId     | "Vehicle not found"                              | Verificar ID        |
| 401    | Unauthorized         | "Authentication required"                        | Login               |
| 404    | NoRecommendations    | "No recommendations available"                   | Cold start fallback |
| 503    | MLServiceUnavailable | "Recommendation service temporarily unavailable" | Retry               |

---

## 8. Eventos RabbitMQ

| Evento                     | Exchange                | Descripción               | Payload                       |
| -------------------------- | ----------------------- | ------------------------- | ----------------------------- |
| `recommendation.generated` | `recommendation.events` | Nuevas recomendaciones    | `{ userId, count }`           |
| `recommendation.viewed`    | `recommendation.events` | Recomendación vista       | `{ recId, vehicleId }`        |
| `recommendation.clicked`   | `recommendation.events` | Recomendación clickeada   | `{ recId, vehicleId }`        |
| `interaction.created`      | `recommendation.events` | Nueva interacción         | `{ userId, vehicleId, type }` |
| `preference.updated`       | `recommendation.events` | Preferencias actualizadas | `{ userId, changes }`         |

---

## 9. Métricas y Monitoreo

### 9.1 KPIs de Negocio

| Métrica         | Fórmula                       | Target |
| --------------- | ----------------------------- | ------ |
| CTR             | Clicks / Views                | > 5%   |
| Conversion Rate | Contacts / Clicks             | > 10%  |
| Diversity Index | Unique Makes / Total Recs     | > 0.4  |
| Coverage        | Users with Recs / Total Users | > 95%  |

### 9.2 Prometheus Metrics

```
# Recomendaciones generadas
recommendation_generated_total{type="foryou|similar"}

# Latencia de generación
recommendation_latency_seconds{quantile="0.5|0.95|0.99"}

# CTR por tipo
recommendation_ctr_ratio{type="foryou|similar"}

# Cache hits
recommendation_cache_hits_total
recommendation_cache_misses_total
```

---

## 10. Configuración

### 10.1 appsettings.json

```json
{
  "Recommendations": {
    "DefaultLimit": 10,
    "MaxLimit": 50,
    "CacheTTLMinutes": 60,
    "MinInteractionsForML": 5,
    "BatchSize": 1000,
    "MaxConcurrency": 100,
    "RefreshCronExpression": "0 */4 * * *"
  },
  "MLModel": {
    "Endpoint": "http://ml-service:8501/v1/models/recommendations:predict",
    "TimeoutSeconds": 5,
    "FallbackEnabled": true
  },
  "Diversity": {
    "MaxPerMake": 0.3,
    "MaxPerDealer": 0.2,
    "FreshnessBoost": 0.1,
    "PopularityWeight": 0.2
  }
}
```

---

## 📚 Referencias

- [02-user-behavior.md](../13-INTEGRACIONES-EXTERNAS/05-user-behavior.md) - Tracking de comportamiento
- [05-feature-store.md](05-feature-store.md) - Feature Store para ML
- [01-search-service.md](01-search-service.md) - Motor de búsqueda
