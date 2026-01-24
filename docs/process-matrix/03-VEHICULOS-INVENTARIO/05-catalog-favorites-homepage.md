# 📚 Catálogo, Favoritos y Homepage - Matriz de Procesos

---

## 📊 Resumen de Implementación

| Componente                       | Total | Implementado | Pendiente | Estado  |
| -------------------------------- | ----- | ------------ | --------- | ------- |
| **Controllers**                  | 3     | 3            | 0         | ✅ 100% |
| **CAT-MAKE-\*** (Marcas)         | 4     | 4            | 0         | ✅ 100% |
| **CAT-MODEL-\*** (Modelos)       | 4     | 4            | 0         | ✅ 100% |
| **FAV-CRUD-\*** (Favoritos)      | 5     | 5            | 0         | ✅ 100% |
| **HOME-SECT-\*** (Homepage)      | 4     | 4            | 0         | ✅ 100% |
| **HOME-ADMIN-\*** (Admin Config) | 3     | 3            | 0         | ✅ 100% |
| **Tests**                        | 20    | 20           | 0         | ✅ 100% |
| **TOTAL**                        | 43    | 43           | 0         | ✅ 100% |

---

## 📋 Información General

| Aspecto           | Detalle                                                                                                                             |
| ----------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| **Servicio**      | VehiclesSaleService (Controllers adicionales)                                                                                       |
| **Puerto**        | 5020                                                                                                                                |
| **Base de Datos** | PostgreSQL (vehiclessale_db)                                                                                                        |
| **Tecnología**    | .NET 8, Entity Framework Core                                                                                                       |
| **Descripción**   | Catálogo maestro de vehículos (marcas, modelos, trims), gestión de favoritos de usuarios, y configuración de secciones del homepage |

---

## 🎯 Endpoints del Servicio

### CatalogController (Catálogo de Vehículos)

| Método | Endpoint                                           | Descripción                  | Auth | Roles   |
| ------ | -------------------------------------------------- | ---------------------------- | ---- | ------- |
| `GET`  | `/api/catalog/makes`                               | Listar todas las marcas      | ❌   | Público |
| `GET`  | `/api/catalog/makes/popular`                       | Marcas populares             | ❌   | Público |
| `GET`  | `/api/catalog/makes/search?q=`                     | Buscar marcas (autocomplete) | ❌   | Público |
| `GET`  | `/api/catalog/makes/{makeSlug}/models`             | Modelos de una marca         | ❌   | Público |
| `GET`  | `/api/catalog/makes/{makeId}/models/search?q=`     | Buscar modelos               | ❌   | Público |
| `GET`  | `/api/catalog/models/{modelId}/years`              | Años disponibles             | ❌   | Público |
| `GET`  | `/api/catalog/models/{modelId}/years/{year}/trims` | Trims con specs              | ❌   | Público |
| `GET`  | `/api/catalog/trims/{trimId}`                      | Detalle de un trim           | ❌   | Público |
| `GET`  | `/api/catalog/stats`                               | Estadísticas del catálogo    | ❌   | Público |
| `POST` | `/api/catalog/seed`                                | Importar catálogo masivo     | ✅   | Admin   |

### FavoritesController (Favoritos de Usuario)

| Método   | Endpoint                           | Descripción                     | Auth | Roles |
| -------- | ---------------------------------- | ------------------------------- | ---- | ----- |
| `GET`    | `/api/favorites`                   | Listar mis favoritos            | ✅   | User  |
| `GET`    | `/api/favorites/count`             | Contar favoritos                | ✅   | User  |
| `GET`    | `/api/favorites/check/{vehicleId}` | Verificar si es favorito        | ✅   | User  |
| `POST`   | `/api/favorites/{vehicleId}`       | Agregar a favoritos             | ✅   | User  |
| `PUT`    | `/api/favorites/{vehicleId}`       | Actualizar notas/notificaciones | ✅   | User  |
| `DELETE` | `/api/favorites/{vehicleId}`       | Eliminar de favoritos           | ✅   | User  |

### HomepageSectionsController (Secciones del Homepage)

| Método   | Endpoint                                            | Descripción                 | Auth | Roles   |
| -------- | --------------------------------------------------- | --------------------------- | ---- | ------- |
| `GET`    | `/api/homepagesections`                             | Listar secciones            | ❌   | Público |
| `GET`    | `/api/homepagesections/{slug}`                      | Sección con vehículos       | ❌   | Público |
| `GET`    | `/api/homepagesections/homepage`                    | Todas las secciones activas | ❌   | Público |
| `GET`    | `/api/homepagesections/{slug}/vehicles`             | Vehículos de una sección    | ❌   | Público |
| `POST`   | `/api/homepagesections/{slug}/vehicles`             | Asignar vehículo a sección  | ✅   | Admin   |
| `DELETE` | `/api/homepagesections/{slug}/vehicles/{vehicleId}` | Remover de sección          | ✅   | Admin   |
| `POST`   | `/api/homepagesections`                             | Crear sección               | ✅   | Admin   |
| `PUT`    | `/api/homepagesections/{slug}`                      | Actualizar sección          | ✅   | Admin   |
| `DELETE` | `/api/homepagesections/{slug}`                      | Eliminar sección            | ✅   | Admin   |

---

## 📊 Entidades del Dominio

### VehicleMake (Marcas)

```csharp
public class VehicleMake
{
    public Guid Id { get; set; }
    public string Name { get; set; }            // "Toyota", "Honda", "Ford"
    public string Slug { get; set; }            // "toyota", "honda", "ford"
    public string? LogoUrl { get; set; }
    public string? Country { get; set; }        // "Japan", "USA", "Germany"
    public bool IsPopular { get; set; }         // Para mostrar primero
    public bool IsActive { get; set; }

    public ICollection<VehicleModel> Models { get; set; }
}
```

### VehicleModel (Modelos)

```csharp
public class VehicleModel
{
    public Guid Id { get; set; }
    public Guid MakeId { get; set; }
    public string Name { get; set; }            // "Camry", "Civic", "F-150"
    public string Slug { get; set; }            // "camry", "civic", "f-150"
    public VehicleType VehicleType { get; set; }    // Car, Truck, SUV
    public BodyStyle? DefaultBodyStyle { get; set; } // Sedan, Coupe, etc.
    public int? StartYear { get; set; }         // 2018
    public int? EndYear { get; set; }           // null = aún en producción
    public bool IsPopular { get; set; }
    public bool IsActive { get; set; }

    public VehicleMake Make { get; set; }
    public ICollection<VehicleTrim> Trims { get; set; }
}
```

### VehicleTrim (Versiones con Specs)

```csharp
public class VehicleTrim
{
    public Guid Id { get; set; }
    public Guid ModelId { get; set; }
    public string Name { get; set; }            // "LE", "SE", "XLE", "TRD"
    public string Slug { get; set; }
    public int Year { get; set; }               // 2024

    // Specs para auto-fill en formulario
    public string? EngineSize { get; set; }     // "2.5L", "3.5L V6"
    public int? Horsepower { get; set; }        // 203, 301
    public int? Torque { get; set; }            // 184, 267
    public FuelType? FuelType { get; set; }
    public TransmissionType? Transmission { get; set; }
    public DriveType? DriveType { get; set; }

    // Fuel Economy
    public int? MpgCity { get; set; }           // 28
    public int? MpgHighway { get; set; }        // 39
    public int? MpgCombined { get; set; }       // 32

    // Precio base de referencia
    public decimal? BaseMSRP { get; set; }      // 28400.00

    public VehicleModel Model { get; set; }
}
```

### Favorite (Favoritos)

```csharp
public class Favorite
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid DealerId { get; set; }          // Para multi-tenancy
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }          // Notas personales del usuario
    public bool NotifyPriceChange { get; set; } // Notificar cuando cambie el precio

    public Vehicle Vehicle { get; set; }
}
```

### HomepageSectionConfig (Secciones del Homepage)

```csharp
public class HomepageSectionConfig
{
    public Guid Id { get; set; }
    public string Name { get; set; }            // "Sedanes", "SUVs", "Destacados"
    public string Slug { get; set; }            // "sedanes", "suvs", "destacados"
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }       // 1, 2, 3...
    public int MaxItems { get; set; }           // Máximo de vehículos a mostrar
    public bool IsActive { get; set; }
    public string? Icon { get; set; }
    public string AccentColor { get; set; }     // "blue", "amber", "red"
    public string? ViewAllHref { get; set; }    // "/vehicles?category=suv"
    public SectionLayoutType LayoutType { get; set; }  // Carousel, Grid, Featured
    public string? Subtitle { get; set; }

    public ICollection<VehicleHomepageSection> VehicleSections { get; set; }
}

public enum SectionLayoutType
{
    Carousel,       // Carrusel horizontal
    Grid,           // Grid de tarjetas
    Featured,       // Destacado grande
    Hero            // Banner hero
}
```

### VehicleHomepageSection (Relación Vehículo-Sección)

```csharp
public class VehicleHomepageSection
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid HomepageSectionConfigId { get; set; }
    public int SortOrder { get; set; }          // Orden dentro de la sección
    public bool IsPinned { get; set; }          // Fijado al inicio
    public DateTime? StartDate { get; set; }    // Fecha inicio (opcional)
    public DateTime? EndDate { get; set; }      // Fecha fin (opcional)
    public DateTime CreatedAt { get; set; }

    public Vehicle Vehicle { get; set; }
    public HomepageSectionConfig HomepageSectionConfig { get; set; }
}
```

---

## 🔄 Procesos Detallados

### PROCESO 1: Flujo de Auto-completar en Publicación

#### Flujo Completo de Selección

| Paso | Actor    | Acción                         | Endpoint                                      | Resultado             |
| ---- | -------- | ------------------------------ | --------------------------------------------- | --------------------- |
| 1    | Usuario  | Abre formulario de publicación | -                                             | Form vacío            |
| 2    | Frontend | Obtiene marcas populares       | GET /api/catalog/makes/popular                | Lista de marcas       |
| 3    | Usuario  | Selecciona marca "Toyota"      | -                                             | Make ID capturado     |
| 4    | Frontend | Obtiene modelos de Toyota      | GET /api/catalog/makes/toyota/models          | Lista de modelos      |
| 5    | Usuario  | Selecciona modelo "Camry"      | -                                             | Model ID capturado    |
| 6    | Frontend | Obtiene años disponibles       | GET /api/catalog/models/{id}/years            | [2024, 2023, 2022...] |
| 7    | Usuario  | Selecciona año "2024"          | -                                             | Year capturado        |
| 8    | Frontend | Obtiene trims con specs        | GET /api/catalog/models/{id}/years/2024/trims | Lista de trims        |
| 9    | Usuario  | Selecciona trim "XLE"          | -                                             | Trim seleccionado     |
| 10   | Frontend | Auto-llena formulario          | Mapea trim specs → form fields                | Form auto-completado  |

#### Auto-fill Mapping (Trim → Formulario)

| Campo Trim   | Campo Formulario | Ejemplo     |
| ------------ | ---------------- | ----------- |
| EngineSize   | engineSize       | "2.5L"      |
| Horsepower   | horsepower       | 203         |
| Torque       | torque           | 184         |
| FuelType     | fuelType         | "Gasoline"  |
| Transmission | transmission     | "Automatic" |
| DriveType    | driveType        | "FWD"       |
| MpgCity      | mpgCity          | 28          |
| MpgHighway   | mpgHighway       | 39          |
| BaseMSRP     | suggestedPrice   | 31170       |

---

### PROCESO 2: Agregar Vehículo a Favoritos

#### Endpoint: `POST /api/favorites/{vehicleId}`

| Paso | Actor      | Acción                       | Sistema                                  | Resultado              |
| ---- | ---------- | ---------------------------- | ---------------------------------------- | ---------------------- |
| 1    | Usuario    | Click en botón ❤️            | HTTP POST                                | Request recibido       |
| 2    | API        | Valida autenticación         | JWT check                                | UserId extraído        |
| 3    | Handler    | Verifica que vehículo existe | VehicleRepository.GetById()              | Vehículo existe        |
| 4    | Handler    | Verifica si ya es favorito   | FavoriteRepository.GetByUserAndVehicle() | No duplicado           |
| 5    | Handler    | Si ya existe                 | HTTP 400                                 | "Ya está en favoritos" |
| 6    | Handler    | Crea entidad Favorite        | new Favorite()                           | Favorito creado        |
| 7    | Repository | Persiste en BD               | INSERT favorites                         | Guardado               |
| 8    | Logger     | Registra acción              | ILogger                                  | Log creado             |
| 9    | API        | Retorna 201 Created          | HTTP Response                            | FavoriteResponse       |

#### Request Body (opcional)

```json
{
  "notes": "Me interesa por el bajo kilometraje",
  "notifyPriceChange": true
}
```

#### Response (201 Created)

```json
{
  "id": "favorite-uuid",
  "userId": "user-uuid",
  "vehicleId": "vehicle-uuid",
  "createdAt": "2026-01-21T10:30:00Z",
  "notes": "Me interesa por el bajo kilometraje",
  "notifyPriceChange": true
}
```

---

### PROCESO 3: Obtener Homepage Completo

#### Endpoint: `GET /api/homepagesections/homepage`

| Paso | Actor    | Acción                              | Sistema               | Resultado                 |
| ---- | -------- | ----------------------------------- | --------------------- | ------------------------- |
| 1    | Frontend | Solicita homepage                   | HTTP GET              | Request recibido          |
| 2    | Handler  | Consulta secciones activas          | WHERE IsActive = true | Secciones filtradas       |
| 3    | Handler  | Ordena por DisplayOrder             | ORDER BY DisplayOrder | Secciones ordenadas       |
| 4    | Handler  | Para cada sección:                  | -                     | -                         |
| 5    | -        | Filtra por fechas (Start/End)       | WHERE dates valid     | Vehículos válidos         |
| 6    | -        | Ordena por IsPinned DESC, SortOrder | ORDER BY              | Vehículos ordenados       |
| 7    | -        | Limita por MaxItems                 | TAKE MaxItems         | Limitado                  |
| 8    | -        | Incluye imágenes del vehículo       | Include(Images)       | Imágenes cargadas         |
| 9    | API      | Retorna todas las secciones         | HTTP 200              | List<SectionWithVehicles> |

#### Response (200 OK)

```json
[
  {
    "id": "section-uuid-1",
    "name": "Carousel Principal",
    "slug": "carousel-principal",
    "description": "Los mejores vehículos destacados",
    "displayOrder": 1,
    "maxItems": 5,
    "isActive": true,
    "icon": "star",
    "accentColor": "amber",
    "viewAllHref": "/vehicles?featured=true",
    "layoutType": "Hero",
    "subtitle": "Selección especial",
    "vehicles": [
      {
        "id": "vehicle-uuid-1",
        "name": "2024 Toyota Camry XLE",
        "make": "Toyota",
        "model": "Camry",
        "year": 2024,
        "price": 1450000,
        "mileage": 12000,
        "fuelType": "Gasoline",
        "transmission": "Automatic",
        "exteriorColor": "Pearl White",
        "bodyStyle": "Sedan",
        "imageUrl": "https://cdn.okla.com.do/vehicles/1/main.jpg",
        "imageUrls": [
          "https://cdn.okla.com.do/vehicles/1/main.jpg",
          "https://cdn.okla.com.do/vehicles/1/side.jpg"
        ],
        "sortOrder": 0,
        "isPinned": true
      }
    ]
  },
  {
    "id": "section-uuid-2",
    "name": "SUVs",
    "slug": "suvs",
    "description": "Los SUVs más populares",
    "displayOrder": 2,
    "maxItems": 10,
    "isActive": true,
    "icon": "truck",
    "accentColor": "blue",
    "viewAllHref": "/vehicles?category=suv",
    "layoutType": "Carousel",
    "subtitle": null,
    "vehicles": [...]
  }
]
```

---

### PROCESO 4: Asignar Vehículo a Sección

#### Endpoint: `POST /api/homepagesections/{slug}/vehicles`

| Paso | Actor      | Acción                      | Sistema                        | Resultado           |
| ---- | ---------- | --------------------------- | ------------------------------ | ------------------- |
| 1    | Admin      | Asigna vehículo a sección   | HTTP POST                      | Request recibido    |
| 2    | API        | Valida autenticación Admin  | JWT + Role check               | Autorizado          |
| 3    | Handler    | Verifica sección existe     | GetBySlug()                    | Sección encontrada  |
| 4    | Handler    | Verifica vehículo existe    | IgnoreQueryFilters().GetById() | Vehículo encontrado |
| 5    | Handler    | Verifica no duplicado       | Check assignment exists        | No duplicado        |
| 6    | Handler    | Si duplicado                | HTTP 400                       | "Ya asignado"       |
| 7    | Handler    | Crea VehicleHomepageSection | new VehicleHomepageSection()   | Asignación creada   |
| 8    | Repository | Persiste en BD              | INSERT                         | Guardado            |
| 9    | Logger     | Registra asignación         | ILogger                        | Log creado          |
| 10   | API        | Retorna 201 Created         | HTTP Response                  | Mensaje éxito       |

#### Request Body

```json
{
  "vehicleId": "vehicle-uuid",
  "sortOrder": 5,
  "isPinned": false,
  "startDate": "2026-01-01T00:00:00Z",
  "endDate": "2026-02-01T00:00:00Z"
}
```

---

### PROCESO 5: Seed de Catálogo (Importación Masiva)

#### Endpoint: `POST /api/catalog/seed`

| Paso | Actor     | Acción                                | Sistema            | Resultado        |
| ---- | --------- | ------------------------------------- | ------------------ | ---------------- |
| 1    | Admin     | Envía datos de catálogo               | HTTP POST          | Request recibido |
| 2    | API       | Valida autenticación Admin            | JWT + Role check   | Autorizado       |
| 3    | Validador | Valida estructura                     | Check makes array  | Datos válidos    |
| 4    | Handler   | Para cada Make:                       | Loop               | -                |
| 5    | -         | Upsert VehicleMake                    | UpsertMakeAsync()  | Make guardado    |
| 6    | -         | Para cada Model:                      | Loop               | -                |
| 7    | -         | Parse VehicleType/BodyStyle           | Enum.TryParse()    | Enums parseados  |
| 8    | -         | Upsert VehicleModel                   | UpsertModelAsync() | Model guardado   |
| 9    | -         | Para cada Trim:                       | Loop               | -                |
| 10   | -         | Parse FuelType/Transmission/DriveType | Enum.TryParse()    | Enums parseados  |
| 11   | -         | Upsert VehicleTrim                    | UpsertTrimAsync()  | Trim guardado    |
| 12   | API       | Retorna conteo                        | HTTP 200           | SeedResult       |

#### Request Body

```json
{
  "makes": [
    {
      "name": "Toyota",
      "slug": "toyota",
      "country": "Japan",
      "isPopular": true,
      "models": [
        {
          "name": "Camry",
          "slug": "camry",
          "vehicleType": "Car",
          "bodyStyle": "Sedan",
          "startYear": 2018,
          "isPopular": true,
          "trims": [
            {
              "name": "LE",
              "year": 2024,
              "engineSize": "2.5L",
              "horsepower": 203,
              "torque": 184,
              "fuelType": "Gasoline",
              "transmission": "Automatic",
              "driveType": "FWD",
              "mpgCity": 28,
              "mpgHighway": 39,
              "mpgCombined": 32,
              "baseMSRP": 28400
            },
            {
              "name": "SE",
              "year": 2024,
              "engineSize": "2.5L",
              "horsepower": 203,
              "baseMSRP": 29495
            },
            {
              "name": "XLE",
              "year": 2024,
              "engineSize": "2.5L",
              "horsepower": 203,
              "baseMSRP": 31170
            }
          ]
        }
      ]
    }
  ]
}
```

#### Response (200 OK)

```json
{
  "makesCreated": 1,
  "modelsCreated": 1,
  "trimsCreated": 3,
  "errors": []
}
```

---

## 🔔 Eventos de Dominio (RabbitMQ)

### Eventos Publicados

| Evento                           | Exchange              | Routing Key                 | Payload                             |
| -------------------------------- | --------------------- | --------------------------- | ----------------------------------- |
| `FavoriteAddedEvent`             | `vehiclessale.events` | `favorite.added`            | UserId, VehicleId                   |
| `FavoriteRemovedEvent`           | `vehiclessale.events` | `favorite.removed`          | UserId, VehicleId                   |
| `VehicleAssignedToSectionEvent`  | `vehiclessale.events` | `homepage.vehicle_assigned` | VehicleId, SectionSlug              |
| `VehicleRemovedFromSectionEvent` | `vehiclessale.events` | `homepage.vehicle_removed`  | VehicleId, SectionSlug              |
| `CatalogImportedEvent`           | `vehiclessale.events` | `catalog.imported`          | MakesCount, ModelsCount, TrimsCount |

### Eventos Consumidos

| Evento                     | Origen              | Acción                                        |
| -------------------------- | ------------------- | --------------------------------------------- |
| `VehiclePriceChangedEvent` | VehiclesSaleService | Notificar usuarios con NotifyPriceChange=true |
| `VehicleDeletedEvent`      | VehiclesSaleService | Remover de favoritos y secciones              |

---

## ⚠️ Reglas de Negocio

### Catálogo

| #   | Regla                   | Descripción                                   |
| --- | ----------------------- | --------------------------------------------- |
| 1   | Slug único              | Cada make, model, trim debe tener slug único  |
| 2   | Trim por año            | Mismo trim puede existir para diferentes años |
| 3   | Upsert                  | Seed no duplica, actualiza si ya existe       |
| 4   | Makes populares primero | IsPopular = true aparecen primero             |

### Favoritos

| #   | Regla                 | Descripción                                           |
| --- | --------------------- | ----------------------------------------------------- |
| 1   | Vehículo debe existir | No se puede agregar vehículo inexistente              |
| 2   | No duplicados         | Un usuario no puede tener el mismo vehículo dos veces |
| 3   | Máximo 100 favoritos  | Límite por usuario                                    |
| 4   | Notificación opcional | NotifyPriceChange default = false                     |

### Homepage Sections

| #   | Regla              | Descripción                                   |
| --- | ------------------ | --------------------------------------------- |
| 1   | MaxItems respetado | No mostrar más de MaxItems vehículos          |
| 2   | Fechas opcionales  | StartDate/EndDate para promociones temporales |
| 3   | IsPinned primero   | Vehículos fijados aparecen antes              |
| 4   | DisplayOrder único | No repetir orden de secciones                 |

---

## ❌ Códigos de Error

| Código    | HTTP Status | Mensaje               | Causa                  |
| --------- | ----------- | --------------------- | ---------------------- |
| `CAT_001` | 404         | Make not found        | Marca no existe        |
| `CAT_002` | 404         | Model not found       | Modelo no existe       |
| `CAT_003` | 404         | Trim not found        | Trim no existe         |
| `CAT_004` | 400         | No makes provided     | Seed sin datos         |
| `FAV_001` | 404         | Vehicle not found     | Vehículo no existe     |
| `FAV_002` | 400         | Already in favorites  | Duplicado              |
| `FAV_003` | 404         | Favorite not found    | No está en favoritos   |
| `FAV_004` | 400         | Max favorites reached | Límite de 100          |
| `HP_001`  | 404         | Section not found     | Sección no existe      |
| `HP_002`  | 400         | Already assigned      | Vehículo ya en sección |

---

## ⚙️ Configuración del Servicio

### appsettings.json

```json
{
  "CatalogSettings": {
    "PopularMakesLimit": 20,
    "AutocompleteLimit": 10,
    "CacheEnabled": true,
    "CacheTTLMinutes": 60
  },
  "FavoritesSettings": {
    "MaxFavoritesPerUser": 100,
    "NotifyPriceChangeDefault": false
  },
  "HomepageSettings": {
    "DefaultMaxItems": 10,
    "DefaultLayoutType": "Carousel",
    "CacheEnabled": true,
    "CacheTTLMinutes": 5
  }
}
```

---

## 🔒 Seguridad

### Autenticación y Autorización

| Endpoint                                 | Auth | Acceso              |
| ---------------------------------------- | ---- | ------------------- |
| GET /api/catalog/\*                      | ❌   | Público             |
| POST /api/catalog/seed                   | ✅   | Admin only          |
| GET /api/favorites/\*                    | ✅   | Usuario autenticado |
| POST/PUT/DELETE /api/favorites/\*        | ✅   | Usuario owner       |
| GET /api/homepagesections/\*             | ❌   | Público             |
| POST/PUT/DELETE /api/homepagesections/\* | ✅   | Admin only          |

---

## 📈 Métricas y Observabilidad

### Métricas Prometheus

| Métrica                      | Tipo  | Labels       | Descripción           |
| ---------------------------- | ----- | ------------ | --------------------- |
| `catalog_makes_total`        | Gauge | -            | Total de marcas       |
| `catalog_models_total`       | Gauge | -            | Total de modelos      |
| `catalog_trims_total`        | Gauge | -            | Total de trims        |
| `favorites_total`            | Gauge | -            | Total de favoritos    |
| `favorites_per_user_avg`     | Gauge | -            | Promedio por usuario  |
| `homepage_sections_total`    | Gauge | -            | Total de secciones    |
| `homepage_vehicles_assigned` | Gauge | section_slug | Vehículos por sección |

---

## 📚 Referencias

- [CatalogController](../../backend/VehiclesSaleService/VehiclesSaleService.Api/Controllers/CatalogController.cs)
- [FavoritesController](../../backend/VehiclesSaleService/VehiclesSaleService.Api/Controllers/FavoritesController.cs)
- [HomepageSectionsController](../../backend/VehiclesSaleService/VehiclesSaleService.Api/Controllers/HomepageSectionsController.cs)
- [VehicleMake Entity](../../backend/VehiclesSaleService/VehiclesSaleService.Domain/Entities/VehicleMake.cs)
- [Favorite Entity](../../backend/VehiclesSaleService/VehiclesSaleService.Domain/Entities/Favorite.cs)
- [HomepageSectionConfig Entity](../../backend/VehiclesSaleService/VehiclesSaleService.Domain/Entities/HomepageSectionConfig.cs)

---

**Última actualización:** Enero 21, 2026  
**Autor:** Sistema de Documentación Automatizado  
**Versión:** 1.0.0
