# 🔍 Comparison Service - Matriz de Procesos

> **Servicio:** ComparisonService  
> **Puerto:** 5032  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO  
> **Estado de Implementación:** ✅ 100% Completo

---

## 📊 Resumen de Implementación

| Componente              | Total | Implementado | Pendiente | Estado  |
| ----------------------- | ----- | ------------ | --------- | ------- |
| **Controllers**         | 2     | 2            | 0         | ✅ 100% |
| **Procesos (COMP-\*)**  | 5     | 5            | 0         | ✅ 100% |
| **Procesos (SHARE-\*)** | 2     | 2            | 0         | ✅ 100% |
| **Tests Unitarios**     | 10    | 10           | 0         | ✅ 100% |

### Leyenda de Estados

- ✅ **IMPLEMENTADO Y PROBADO**: Código completo con tests
- 🟢 **IMPLEMENTADO**: Código completo, falta testing
- 🟡 **EN PROGRESO**: Implementación parcial
- 🔴 **PENDIENTE**: No implementado

---

## 1. Información General

### 1.1 Descripción

Sistema de comparación de vehículos que permite a los usuarios comparar hasta 3 vehículos simultáneamente. Genera tablas comparativas lado a lado con especificaciones, precios, fotos y calificaciones. Soporta compartir comparaciones via link público.

### 1.2 Características

| Feature                       | Descripción               |
| ----------------------------- | ------------------------- |
| Comparar hasta 3 vehículos    | Límite por sesión         |
| Persistencia de comparaciones | Guardadas por usuario     |
| Links compartibles            | URL pública con expiry    |
| Diferencias resaltadas        | Highlight de mejor opción |
| Export PDF                    | Generar reporte           |
| Historial                     | Últimas 10 comparaciones  |

### 1.3 Dependencias

| Servicio            | Propósito                 |
| ------------------- | ------------------------- |
| VehiclesSaleService | Datos de vehículos        |
| MediaService        | Imágenes de vehículos     |
| UserService         | Preferencias de usuario   |
| CatalogService      | Especificaciones técnicas |

---

## 2. Endpoints API

### 2.1 ComparisonsController

| Método   | Endpoint                                     | Descripción          | Auth  | Roles        |
| -------- | -------------------------------------------- | -------------------- | ----- | ------------ |
| `GET`    | `/api/comparisons`                           | Mis comparaciones    | ✅    | User         |
| `GET`    | `/api/comparisons/{id}`                      | Obtener comparación  | ✅/❌ | Owner/Public |
| `GET`    | `/api/comparisons/shared/{shareToken}`       | Ver compartida       | ❌    | Public       |
| `POST`   | `/api/comparisons`                           | Crear comparación    | ✅    | User         |
| `POST`   | `/api/comparisons/{id}/vehicles/{vehicleId}` | Agregar vehículo     | ✅    | Owner        |
| `DELETE` | `/api/comparisons/{id}/vehicles/{vehicleId}` | Quitar vehículo      | ✅    | Owner        |
| `POST`   | `/api/comparisons/{id}/share`                | Generar link         | ✅    | Owner        |
| `DELETE` | `/api/comparisons/{id}`                      | Eliminar comparación | ✅    | Owner        |
| `GET`    | `/api/comparisons/{id}/export`               | Exportar PDF         | ✅    | Owner        |

### 2.2 ComparisonSessionController (Sin auth)

| Método   | Endpoint                                                   | Descripción          | Auth | Roles  |
| -------- | ---------------------------------------------------------- | -------------------- | ---- | ------ |
| `POST`   | `/api/comparison-session`                                  | Crear sesión anónima | ❌   | Public |
| `GET`    | `/api/comparison-session/{sessionId}`                      | Obtener sesión       | ❌   | Public |
| `POST`   | `/api/comparison-session/{sessionId}/vehicles`             | Agregar a sesión     | ❌   | Public |
| `DELETE` | `/api/comparison-session/{sessionId}/vehicles/{vehicleId}` | Quitar de sesión     | ❌   | Public |

---

## 3. Entidades y Enums

### 3.1 Comparison (Entidad)

```csharp
public class Comparison
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Name { get; set; }

    // Vehículos (máximo 3)
    public List<ComparisonVehicle> Vehicles { get; set; }

    // Compartir
    public string? ShareToken { get; set; }
    public DateTime? ShareExpiresAt { get; set; }
    public int ShareViewCount { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastViewedAt { get; set; }
}
```

### 3.2 ComparisonVehicle (Entidad)

```csharp
public class ComparisonVehicle
{
    public Guid Id { get; set; }
    public Guid ComparisonId { get; set; }
    public Guid VehicleId { get; set; }
    public int Position { get; set; }          // 1, 2, 3
    public DateTime AddedAt { get; set; }

    // Snapshot de datos (para histórico)
    public string VehicleSnapshot { get; set; } // JSON
}
```

### 3.3 ComparisonSession (Entidad - Redis)

```csharp
public class ComparisonSession
{
    public string SessionId { get; set; }        // UUID
    public List<Guid> VehicleIds { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }      // +24h
}
```

### 3.4 VehicleComparisonData (DTO)

```csharp
public class VehicleComparisonData
{
    public Guid VehicleId { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public decimal Price { get; set; }
    public string PrimaryImageUrl { get; set; }

    // Seller
    public string SellerName { get; set; }
    public string SellerType { get; set; }

    // Specs
    public int Year { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public string Trim { get; set; }
    public int Mileage { get; set; }

    // Motor
    public string Engine { get; set; }
    public int Horsepower { get; set; }
    public string Transmission { get; set; }
    public string FuelType { get; set; }
    public decimal? FuelEconomy { get; set; }

    // Exterior
    public string ExteriorColor { get; set; }
    public string BodyType { get; set; }
    public int Doors { get; set; }
    public int Seats { get; set; }

    // Features (para comparar)
    public List<string> Features { get; set; }
    public List<string> SafetyFeatures { get; set; }

    // Rating
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
}
```

---

## 4. Procesos Detallados

### 4.1 COMP-001: Agregar Vehículo a Comparación

| Campo       | Valor                          |
| ----------- | ------------------------------ |
| **ID**      | COMP-001                       |
| **Nombre**  | Agregar Vehículo a Comparación |
| **Actor**   | Usuario                        |
| **Trigger** | Click "Comparar" en listing    |

#### Flujo del Proceso

| Paso | Acción                               | Sistema           | Validación                 |
| ---- | ------------------------------------ | ----------------- | -------------------------- |
| 1    | Usuario ve listing                   | Frontend          | VehicleDetail              |
| 2    | Click botón "Comparar"               | Frontend          | Icon ⚖️                    |
| 3    | Verificar si autenticado             | Frontend          | Redirect login o continuar |
| 4    | Si no auth → Sesión anónima          | Redis             | 24h TTL                    |
| 5    | Si auth → Obtener comparación activa | ComparisonService | O crear nueva              |
| 6    | Validar no duplicado                 | ComparisonService | VehicleId único            |
| 7    | Validar < 3 vehículos                | ComparisonService | Max 3                      |
| 8    | Agregar vehículo                     | Database          | Position = next            |
| 9    | Snapshot de datos                    | ComparisonService | JSON con specs             |
| 10   | Mostrar confirmación                 | Frontend          | Toast + badge              |
| 11   | Publicar evento                      | RabbitMQ          | comparison.vehicle_added   |

#### Response

```json
{
  "comparisonId": "uuid",
  "vehicleCount": 2,
  "vehicles": [
    { "id": "uuid", "title": "Toyota Camry 2024", "position": 1 },
    { "id": "uuid", "title": "Honda Accord 2024", "position": 2 }
  ],
  "canAddMore": true,
  "compareUrl": "/compare/abc123"
}
```

---

### 4.2 COMP-002: Ver Comparación Completa

| Campo       | Valor                     |
| ----------- | ------------------------- |
| **ID**      | COMP-002                  |
| **Nombre**  | Ver Tabla de Comparación  |
| **Actor**   | Usuario                   |
| **Trigger** | GET /api/comparisons/{id} |

#### Flujo del Proceso

| Paso | Acción                    | Sistema             | Validación       |
| ---- | ------------------------- | ------------------- | ---------------- |
| 1    | Usuario navega a /compare | Frontend            | Con comparisonId |
| 2    | Obtener comparación       | ComparisonService   | Por ID           |
| 3    | Validar acceso            | ComparisonService   | Owner o pública  |
| 4    | Por cada vehículo         | Loop                | Enriquecer datos |
| 5    | Obtener datos actuales    | VehiclesSaleService | Precio actual    |
| 6    | Obtener especificaciones  | CatalogService      | Specs técnicos   |
| 7    | Calcular diferencias      | ComparisonService   | Highlight mejor  |
| 8    | Renderizar tabla          | Frontend            | Lado a lado      |
| 9    | Registrar view            | ComparisonService   | LastViewedAt     |

#### Response

```json
{
  "id": "uuid",
  "name": "Mi comparación SUVs",
  "vehicles": [
    {
      "vehicleId": "uuid",
      "position": 1,
      "data": {
        "title": "Toyota RAV4 2024",
        "price": 1850000,
        "year": 2024,
        "make": "Toyota",
        "model": "RAV4",
        "mileage": 0,
        "engine": "2.5L 4-Cyl",
        "horsepower": 203,
        "transmission": "Automatic 8-Speed",
        "fuelType": "Gasoline",
        "fuelEconomy": 30.5,
        "features": ["Apple CarPlay", "Lane Assist", "Sunroof"],
        "safetyFeatures": ["Airbags", "ABS", "Backup Camera"],
        "primaryImageUrl": "https://..."
      }
    },
    {
      "vehicleId": "uuid",
      "position": 2,
      "data": {
        "title": "Honda CR-V 2024",
        "price": 1750000,
        "year": 2024,
        "make": "Honda",
        "model": "CR-V",
        "mileage": 5000,
        "engine": "1.5L Turbo 4-Cyl",
        "horsepower": 190,
        "transmission": "CVT",
        "fuelType": "Gasoline",
        "fuelEconomy": 32.0,
        "features": ["Apple CarPlay", "Android Auto", "Honda Sensing"],
        "safetyFeatures": ["Airbags", "ABS", "Collision Mitigation"],
        "primaryImageUrl": "https://..."
      }
    }
  ],
  "highlights": {
    "lowestPrice": 2,
    "lowestMileage": 1,
    "bestFuelEconomy": 2,
    "mostHorsepower": 1,
    "newestYear": [1, 2]
  }
}
```

---

### 4.3 COMP-003: Compartir Comparación

| Campo       | Valor                            |
| ----------- | -------------------------------- |
| **ID**      | COMP-003                         |
| **Nombre**  | Generar Link para Compartir      |
| **Actor**   | Usuario                          |
| **Trigger** | POST /api/comparisons/{id}/share |

#### Flujo del Proceso

| Paso | Acción                    | Sistema           | Validación               |
| ---- | ------------------------- | ----------------- | ------------------------ |
| 1    | Usuario click "Compartir" | Frontend          | En página compare        |
| 2    | Generar token único       | ComparisonService | UUID o hash              |
| 3    | Establecer expiración     | ComparisonService | 7 días default           |
| 4    | Guardar token             | Database          | ShareToken               |
| 5    | Generar URL pública       | ComparisonService | /compare/shared/{token}  |
| 6    | Mostrar modal compartir   | Frontend          | Copy link, WhatsApp, etc |
| 7    | Publicar evento           | RabbitMQ          | comparison.shared        |

#### Request

```json
{
  "expiresInDays": 7
}
```

#### Response

```json
{
  "shareUrl": "https://okla.com.do/compare/shared/abc123xyz",
  "shareToken": "abc123xyz",
  "expiresAt": "2026-01-28T12:00:00Z"
}
```

---

### 4.4 COMP-004: Exportar PDF

| Campo       | Valor                            |
| ----------- | -------------------------------- |
| **ID**      | COMP-004                         |
| **Nombre**  | Exportar Comparación a PDF       |
| **Actor**   | Usuario                          |
| **Trigger** | GET /api/comparisons/{id}/export |

#### Flujo del Proceso

| Paso | Acción                       | Sistema           | Validación            |
| ---- | ---------------------------- | ----------------- | --------------------- |
| 1    | Usuario click "Exportar PDF" | Frontend          | Botón descarga        |
| 2    | Obtener datos comparación    | ComparisonService | Completos             |
| 3    | Obtener imágenes             | MediaService      | URLs                  |
| 4    | Renderizar HTML template     | ComparisonService | Con datos             |
| 5    | Convertir a PDF              | PdfService        | Puppeteer/wkhtmltopdf |
| 6    | Retornar archivo             | Response          | application/pdf       |
| 7    | Descargar en cliente         | Browser           | File download         |

#### Response Headers

```
Content-Type: application/pdf
Content-Disposition: attachment; filename="comparison-2024-01-21.pdf"
```

---

## 5. UI: Página de Comparación

### 5.1 Estructura de la Página

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      COMPARAR VEHÍCULOS                                  │
│                                                                          │
│  ┌─────────────────────────┬─────────────────────────┬────────────────┐ │
│  │       Toyota RAV4       │       Honda CR-V        │   [+ Agregar]  │ │
│  │       ┌────────┐        │       ┌────────┐        │                │ │
│  │       │  IMG   │        │       │  IMG   │        │   Buscar       │ │
│  │       └────────┘        │       └────────┘        │   vehículo...  │ │
│  │                         │                         │                │ │
│  │    RD$ 1,850,000        │    RD$ 1,750,000 ✓      │                │ │
│  │                         │   (mejor precio)        │                │ │
│  │    [× Quitar]           │    [× Quitar]           │                │ │
│  └─────────────────────────┴─────────────────────────┴────────────────┘ │
│                                                                          │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ │
│                                                                          │
│  ESPECIFICACIONES BÁSICAS                                                │
│  ┌─────────────────┬─────────────────────┬─────────────────────┐        │
│  │                 │   Toyota RAV4       │   Honda CR-V        │        │
│  ├─────────────────┼─────────────────────┼─────────────────────┤        │
│  │ Año             │   2024              │   2024              │        │
│  │ Kilometraje     │   0 km ✓            │   5,000 km          │        │
│  │ Transmisión     │   Automática 8-Vel  │   CVT               │        │
│  │ Combustible     │   Gasolina          │   Gasolina          │        │
│  │ Rendimiento     │   30.5 km/L         │   32.0 km/L ✓       │        │
│  └─────────────────┴─────────────────────┴─────────────────────┘        │
│                                                                          │
│  MOTOR Y RENDIMIENTO                                                     │
│  ┌─────────────────┬─────────────────────┬─────────────────────┐        │
│  │ Motor           │   2.5L 4-Cyl        │   1.5L Turbo        │        │
│  │ Potencia        │   203 HP ✓          │   190 HP            │        │
│  │ Torque          │   184 lb-ft         │   179 lb-ft         │        │
│  └─────────────────┴─────────────────────┴─────────────────────┘        │
│                                                                          │
│  CARACTERÍSTICAS                                                          │
│  ┌─────────────────┬─────────────────────┬─────────────────────┐        │
│  │ Apple CarPlay   │   ✓                 │   ✓                 │        │
│  │ Android Auto    │   ✓                 │   ✓                 │        │
│  │ Sunroof         │   ✓                 │   ✗                 │        │
│  │ Lane Assist     │   ✓                 │   ✓                 │        │
│  │ Honda Sensing   │   ✗                 │   ✓                 │        │
│  └─────────────────┴─────────────────────┴─────────────────────┘        │
│                                                                          │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ │
│                                                                          │
│  [📤 Compartir]    [📄 Exportar PDF]    [🗑️ Limpiar Todo]              │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 6. Reglas de Negocio

### 6.1 Límites

| Regla                            | Valor    |
| -------------------------------- | -------- |
| Máximo vehículos por comparación | 3        |
| Máximo comparaciones guardadas   | 10       |
| Expiración link compartido       | 7 días   |
| Expiración sesión anónima        | 24 horas |
| Historial de comparaciones       | 30 días  |

### 6.2 Highlight Logic

| Categoría               | Mejor es...       | Badge           |
| ----------------------- | ----------------- | --------------- |
| Precio                  | Menor             | ✓ Mejor precio  |
| Kilometraje             | Menor             | ✓ Menos km      |
| Año                     | Mayor (más nuevo) | ✓ Más nuevo     |
| Potencia                | Mayor             | ✓ Más potente   |
| Rendimiento combustible | Mayor             | ✓ Más eficiente |
| Cantidad de features    | Mayor             | ✓ Más equipado  |

---

## 7. Eventos RabbitMQ

| Evento                       | Exchange            | Payload                        |
| ---------------------------- | ------------------- | ------------------------------ |
| `comparison.created`         | `comparison.events` | `{ comparisonId, userId }`     |
| `comparison.vehicle_added`   | `comparison.events` | `{ comparisonId, vehicleId }`  |
| `comparison.vehicle_removed` | `comparison.events` | `{ comparisonId, vehicleId }`  |
| `comparison.shared`          | `comparison.events` | `{ comparisonId, shareToken }` |
| `comparison.viewed`          | `comparison.events` | `{ comparisonId, isShared }`   |
| `comparison.exported`        | `comparison.events` | `{ comparisonId, format }`     |

---

## 8. Métricas

### 8.1 Prometheus

```
# Comparaciones
comparison_created_total
comparison_vehicles_added_total
comparison_shared_total
comparison_views_total{type="owner|shared"}
comparison_exports_total{format="pdf"}

# Sesiones
comparison_sessions_active
comparison_session_duration_seconds

# Vehículos más comparados
comparison_vehicle_count{vehicleId="..."}
```

---

## 9. Configuración

```json
{
  "Comparison": {
    "MaxVehiclesPerComparison": 3,
    "MaxComparisonsPerUser": 10,
    "ShareLinkExpiryDays": 7,
    "SessionExpiryHours": 24,
    "HistoryRetentionDays": 30
  },
  "Redis": {
    "SessionPrefix": "comparison:session:",
    "CacheTTL": 3600
  },
  "Pdf": {
    "Provider": "Puppeteer",
    "Timeout": 30000
  }
}
```

---

## 📚 Referencias

- [01-search-service.md](01-search-service.md) - Búsqueda de vehículos
- [02-recommendation-service.md](02-recommendation-service.md) - Recomendaciones
- [04-alert-service.md](04-alert-service.md) - Alertas de precio
