# 🏢 Propiedades en Renta - Matriz de Procesos

## 📋 Información General

| Aspecto           | Detalle                                                                                     |
| ----------------- | ------------------------------------------------------------------------------------------- |
| **Servicio**      | PropertiesRentService                                                                       |
| **Puerto**        | 5025                                                                                        |
| **Base de Datos** | PostgreSQL (propertiesrent_db)                                                              |
| **Tecnología**    | .NET 8, Entity Framework Core                                                               |
| **Multi-tenancy** | Por agencia inmobiliaria (DealerId)                                                         |
| **Descripción**   | Gestión de propiedades inmobiliarias en alquiler: apartamentos, casas, oficinas comerciales |

---

## 🎯 Endpoints del Servicio

### PropertiesController

| Método   | Endpoint                            | Descripción                 | Auth | Roles        |
| -------- | ----------------------------------- | --------------------------- | ---- | ------------ |
| `GET`    | `/api/properties`                   | Buscar propiedades en renta | ❌   | Público      |
| `GET`    | `/api/properties/{id}`              | Obtener propiedad por ID    | ❌   | Público      |
| `GET`    | `/api/properties/mls/{mlsNumber}`   | Obtener por número MLS      | ❌   | Público      |
| `GET`    | `/api/properties/featured`          | Propiedades destacadas      | ❌   | Público      |
| `GET`    | `/api/properties/agent/{agentId}`   | Propiedades de un agente    | ❌   | Público      |
| `GET`    | `/api/properties/dealer/{dealerId}` | Propiedades de una agencia  | ❌   | Público      |
| `POST`   | `/api/properties`                   | Crear propiedad             | ✅   | Agent, Admin |
| `PUT`    | `/api/properties/{id}`              | Actualizar propiedad        | ✅   | Agent, Admin |
| `DELETE` | `/api/properties/{id}`              | Eliminar propiedad (soft)   | ✅   | Agent, Admin |

### CategoriesController

| Método | Endpoint                        | Descripción                 | Auth | Roles   |
| ------ | ------------------------------- | --------------------------- | ---- | ------- |
| `GET`  | `/api/categories`               | Listar todas las categorías | ❌   | Público |
| `GET`  | `/api/categories/root`          | Categorías raíz             | ❌   | Público |
| `GET`  | `/api/categories/{id}`          | Categoría por ID            | ❌   | Público |
| `GET`  | `/api/categories/slug/{slug}`   | Categoría por slug          | ❌   | Público |
| `GET`  | `/api/categories/{id}/children` | Subcategorías               | ❌   | Público |

---

## 📊 Entidades del Dominio

### Property (Propiedad en Renta)

La entidad Property para renta es similar a la de venta con campos adicionales específicos para alquiler:

```csharp
public class Property : ITenantEntity
{
    // ========================================
    // CAMPOS BASE (igual que PropertiesSale)
    // ========================================
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }          // Renta mensual
    public string Currency { get; set; }
    public PropertyStatus Status { get; set; }

    // ========================================
    // CAMPOS ESPECÍFICOS DE RENTA
    // ========================================
    public RentPeriod RentPeriod { get; set; }      // Monthly, Weekly, Daily
    public decimal? SecurityDeposit { get; set; }    // Depósito de seguridad
    public int? MinLeaseMonths { get; set; }         // Mínimo de meses
    public int? MaxLeaseMonths { get; set; }         // Máximo de meses
    public DateTime? AvailableFrom { get; set; }     // Disponible desde
    public bool UtilitiesIncluded { get; set; }      // Servicios incluidos
    public string? UtilitiesDetails { get; set; }    // Detalles de servicios
    public bool PetsAllowed { get; set; }            // Mascotas permitidas
    public string? PetPolicy { get; set; }           // Política de mascotas
    public decimal? PetDeposit { get; set; }         // Depósito por mascota
    public bool FurnishedAvailable { get; set; }     // Amueblado disponible
    public FurnishedLevel FurnishedLevel { get; set; } // Nivel de amueblado

    // ========================================
    // REQUISITOS DE ALQUILER
    // ========================================
    public decimal? MinIncomeRequirement { get; set; } // Ingreso mínimo requerido
    public int? MinCreditScore { get; set; }           // Score crediticio mínimo
    public bool BackgroundCheckRequired { get; set; }  // Verificación antecedentes
    public bool IncomeVerificationRequired { get; set; } // Verificación de ingresos

    // ========================================
    // RESTO DE CAMPOS (igual que PropertiesSale)
    // ========================================
    // PropertyType, PropertySubType, Bedrooms, Bathrooms, etc.
    // Ubicación, sistemas, características, etc.
}

public enum RentPeriod
{
    Monthly = 0,     // Mensual
    Weekly = 1,      // Semanal
    Daily = 2,       // Diario
    Yearly = 3       // Anual
}

public enum FurnishedLevel
{
    Unfurnished = 0,    // Sin amueblar
    PartiallyFurnished = 1,  // Parcialmente amueblado
    FullyFurnished = 2  // Completamente amueblado
}
```

---

## 🔄 Procesos Detallados

### PROCESO 1: Búsqueda de Propiedades en Renta

#### Endpoint: `GET /api/properties`

| Paso | Actor      | Acción                               | Sistema                | Resultado        |
| ---- | ---------- | ------------------------------------ | ---------------------- | ---------------- |
| 1    | Usuario    | Aplica filtros de búsqueda           | HTTP GET               | Request recibido |
| 2    | Controller | Construye PropertySearchParameters   | Map request → params   | Parámetros       |
| 3    | Repository | Construye query base                 | IQueryable<Property>   | Query            |
| 4    | Repository | Aplica filtro precio (renta mensual) | WHERE Price BETWEEN    | Filtro rango     |
| 5    | Repository | Aplica filtro habitaciones           | WHERE Bedrooms >=      | Filtro numérico  |
| 6    | Repository | Aplica filtro ubicación              | WHERE City/State =     | Filtro geo       |
| 7    | Repository | Aplica filtros específicos renta     | PetsAllowed, Furnished | Filtros renta    |
| 8    | Repository | Ordena resultados                    | ORDER BY Price, Date   | Ordenado         |
| 9    | Repository | Pagina resultados                    | SKIP/TAKE              | Paginado         |
| 10   | API        | Retorna PropertySearchResult         | HTTP 200               | Respuesta        |

#### Filtros Específicos de Renta

| Parámetro         | Tipo | Descripción                           |
| ----------------- | ---- | ------------------------------------- |
| PetsAllowed       | bool | Solo propiedades que aceptan mascotas |
| FurnishedLevel    | enum | Nivel de amueblado                    |
| MinLeaseMonths    | int  | Mínimo de meses de contrato           |
| MaxLeaseMonths    | int  | Máximo de meses de contrato           |
| AvailableFrom     | date | Disponible a partir de fecha          |
| UtilitiesIncluded | bool | Servicios incluidos                   |

---

### PROCESO 2: Crear Propiedad para Renta

#### Endpoint: `POST /api/properties`

| Paso | Actor      | Acción                   | Sistema                    | Resultado        |
| ---- | ---------- | ------------------------ | -------------------------- | ---------------- |
| 1    | Agente     | Envía datos de propiedad | HTTP POST                  | Request recibido |
| 2    | Controller | Valida categoría existe  | CategoryRepository.GetById | Validada         |
| 3    | Controller | Crea entidad Property    | new Property()             | Creada           |
| 4    | Controller | Mapea campos básicos     | Title, Price (renta)       | Mapeados         |
| 5    | Controller | Mapea campos de renta    | Deposit, LeaseTerms        | Mapeados         |
| 6    | Controller | Mapea políticas          | Pets, Furnished            | Mapeados         |
| 7    | Controller | Procesa imágenes         | Loop images                | Images creadas   |
| 8    | Repository | Persiste propiedad       | INSERT                     | Guardado         |
| 9    | Logger     | Registra creación        | ILogger                    | Log              |
| 10   | API        | Retorna 201 Created      | CreatedAtAction            | Creado           |

#### Request Body Específico Renta

```json
{
  "title": "Modern 2BR Apartment in Downtown",
  "description": "Bright and spacious apartment...",
  "price": 2500,
  "currency": "USD",
  "rentPeriod": 0,
  "securityDeposit": 5000,
  "minLeaseMonths": 12,
  "maxLeaseMonths": 24,
  "availableFrom": "2026-02-01T00:00:00Z",
  "utilitiesIncluded": true,
  "utilitiesDetails": "Water, Electricity, Internet included",
  "petsAllowed": true,
  "petPolicy": "Dogs under 25 lbs allowed. Cats welcome.",
  "petDeposit": 500,
  "furnishedAvailable": true,
  "furnishedLevel": 2,
  "minIncomeRequirement": 7500,
  "minCreditScore": 650,
  "backgroundCheckRequired": true,
  "incomeVerificationRequired": true,
  "propertyType": 4,
  "bedrooms": 2,
  "bathrooms": 2,
  "squareFeet": 1200,
  "streetAddress": "500 Main St, Unit 1205",
  "city": "Miami",
  "state": "FL",
  "zipCode": "33130",
  "agentId": "agent-uuid",
  "agentName": "Maria Rodriguez",
  "dealerId": "dealer-uuid",
  "categoryId": "apartment-category-uuid",
  "images": [
    "https://cdn.okla.com.do/rentals/1/living.jpg",
    "https://cdn.okla.com.do/rentals/1/bedroom.jpg"
  ]
}
```

---

### PROCESO 3: Flujo de Estados de Renta

```
┌─────────┐   Publicar    ┌─────────────┐   Aprobar    ┌────────┐
│  Draft  ├──────────────►│PendingReview├─────────────►│ Active │
└─────────┘               └─────────────┘              └────┬───┘
                                                           │
                          ┌────────────────────────────────┤
                          │                                │
                          ▼                                ▼
                    ┌───────────┐                   ┌──────────┐
                    │  Rented   │                   │ Reserved │
                    │(Ocupado)  │                   │(Reservado)│
                    └─────┬─────┘                   └────┬─────┘
                          │                              │
                          │ Fin contrato                 │ Confirmar
                          ▼                              ▼
                    ┌───────────┐                   ┌───────────┐
                    │ Available │                   │  Rented   │
                    │ (Activo)  │                   └───────────┘
                    └───────────┘
```

#### Estados Específicos de Renta

| Estado    | Descripción                              |
| --------- | ---------------------------------------- |
| Active    | Disponible para alquilar                 |
| Reserved  | Reservado (aplicación en proceso)        |
| Rented    | Ocupado (bajo contrato de arrendamiento) |
| Pending   | En proceso de desocupación               |
| Withdrawn | Retirado del mercado                     |

---

### PROCESO 4: Cálculo de Costos para Inquilino

#### Flujo de Cálculo

| Concepto              | Fórmula                | Ejemplo    |
| --------------------- | ---------------------- | ---------- |
| Primera Renta         | Price                  | $2,500     |
| Depósito de Seguridad | SecurityDeposit        | $5,000     |
| Depósito de Mascota   | PetDeposit (si aplica) | $500       |
| Fee de Aplicación     | Fixed Fee              | $50        |
| **Move-in Cost**      | Sum of above           | **$8,050** |

---

## 🔔 Eventos de Dominio (RabbitMQ)

### Eventos Publicados

| Evento                     | Exchange         | Routing Key             | Payload                          |
| -------------------------- | ---------------- | ----------------------- | -------------------------------- |
| `RentalCreatedEvent`       | `rentals.events` | `rental.created`        | PropertyId, MonthlyRent          |
| `RentalStatusChangedEvent` | `rentals.events` | `rental.status_changed` | PropertyId, OldStatus, NewStatus |
| `RentalReservedEvent`      | `rentals.events` | `rental.reserved`       | PropertyId, ApplicantId          |
| `RentalRentedEvent`        | `rentals.events` | `rental.rented`         | PropertyId, TenantId, LeaseStart |
| `RentalVacatedEvent`       | `rentals.events` | `rental.vacated`        | PropertyId, LeaseEnd             |
| `RentalPriceChangedEvent`  | `rentals.events` | `rental.price_changed`  | PropertyId, OldRent, NewRent     |

---

## ⚠️ Reglas de Negocio Específicas de Renta

| #   | Regla           | Descripción                                             |
| --- | --------------- | ------------------------------------------------------- |
| 1   | Depósito máximo | SecurityDeposit <= 2 x Price (renta mensual)            |
| 2   | Período mínimo  | MinLeaseMonths >= 1                                     |
| 3   | Disponibilidad  | AvailableFrom >= Today                                  |
| 4   | Ingreso mínimo  | MinIncomeRequirement >= 3 x Price típicamente           |
| 5   | Mascotas        | Si PetsAllowed = true, PetPolicy requerido              |
| 6   | Amueblado       | Si FurnishedLevel > 0, precio puede ajustarse           |
| 7   | Servicios       | Si UtilitiesIncluded = true, UtilitiesDetails requerido |

---

## ❌ Códigos de Error

| Código     | HTTP Status | Mensaje                | Causa                  |
| ---------- | ----------- | ---------------------- | ---------------------- |
| `RENT_001` | 404         | Property not found     | Propiedad no existe    |
| `RENT_002` | 400         | Invalid deposit        | Depósito excede límite |
| `RENT_003` | 400         | Invalid lease terms    | Términos inválidos     |
| `RENT_004` | 400         | Property not available | No disponible          |
| `RENT_005` | 400         | Already reserved       | Ya reservado           |

---

## ⚙️ Configuración

### appsettings.json

```json
{
  "PropertiesRentSettings": {
    "DefaultCurrency": "USD",
    "DefaultCountry": "USA",
    "MaxDepositMultiplier": 2,
    "DefaultMinLeaseMonths": 12,
    "FeaturedLimit": 10,
    "ApplicationFee": 50
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=propertiesrent_db;..."
  }
}
```

---

## 📈 Métricas Prometheus

| Métrica                      | Tipo    | Labels                | Descripción               |
| ---------------------------- | ------- | --------------------- | ------------------------- |
| `rentals_total`              | Gauge   | status, property_type | Total por estado          |
| `rentals_avg_price`          | Gauge   | city, bedrooms        | Renta promedio            |
| `rentals_occupancy_rate`     | Gauge   | -                     | Tasa de ocupación         |
| `rentals_avg_days_to_rent`   | Gauge   | -                     | Días promedio para rentar |
| `rentals_applications_total` | Counter | -                     | Total de aplicaciones     |

---

## 📚 Comparación: Venta vs Renta

| Aspecto          | PropertiesSaleService       | PropertiesRentService       |
| ---------------- | --------------------------- | --------------------------- |
| **Precio**       | Precio de venta (total)     | Renta mensual               |
| **Estados**      | Draft → Active → Sold       | Draft → Active → Rented     |
| **Término**      | Venta única                 | Contrato renovable          |
| **Campos extra** | OriginalPrice, DaysOnMarket | SecurityDeposit, LeaseTerms |
| **Políticas**    | N/A                         | PetsAllowed, Furnished      |
| **Requisitos**   | N/A                         | MinIncome, CreditScore      |

---

## 📚 Referencias

- [PropertiesController](../../backend/PropertiesRentService/PropertiesRentService.Api/Controllers/PropertiesController.cs)
- [CategoriesController](../../backend/PropertiesRentService/PropertiesRentService.Api/Controllers/CategoriesController.cs)

---

**Última actualización:** Enero 21, 2026  
**Versión:** 1.0.0
