# 🚗 SPRINT 3 - Vehicle Service y CRUD Completo

**Fecha:** 2 Enero 2026  
**Duración estimada:** 5-6 horas  
**Tokens estimados:** ~28,000  
**Prioridad:** 🟠 Alta

---

## 🎯 OBJETIVOS

1. Crear VehicleService en backend (extiende ProductService)
2. Implementar CRUD completo de vehículos
3. Agregar custom fields específicos para vehículos
4. Integrar frontend con VehicleService
5. Implementar filtros avanzados
6. Crear páginas de listado y detalle
7. Implementar form de crear/editar vehículo

---

## 📋 CHECKLIST DE TAREAS

### Fase 1: Backend - VehicleService (2 horas)

- [ ] 1.1. Crear estructura de VehicleService
- [ ] 1.2. Definir entidades Vehicle con campos personalizados
- [ ] 1.3. Crear VehicleController con endpoints CRUD
- [ ] 1.4. Implementar filtros (marca, modelo, año, precio, etc.)
- [ ] 1.5. Agregar paginación y ordenamiento
- [ ] 1.6. Crear endpoints para featured vehicles
- [ ] 1.7. Dockerizar y agregar a compose.yaml

### Fase 2: Backend - Database Schema (30 min)

- [ ] 2.1. Diseñar schema de vehicles
- [ ] 2.2. Crear migraciones EF Core
- [ ] 2.3. Seed data de prueba
- [ ] 2.4. Validar schema en PostgreSQL

### Fase 2B: Seed de Catálogo de Vehículos (CRÍTICO - 12-16h)

⚠️ **CONTEXTO:** El frontend necesita mostrar catálogos completos de marcas, modelos, años, especificaciones técnicas (como CarGurus). Sin esta data, el frontend solo mostrará páginas vacías.

- [ ] 2B.1. **Diseñar Tablas de Catálogo** (2-3h)
  ```sql
  -- Estructura de datos tipo CarGurus
  vehicle_makes (id, name, logo_url, country, founded_year)
  vehicle_models (id, make_id, name, body_type, years_produced)
  vehicle_trims (id, model_id, name, year, specs_json)
  vehicle_specs (id, trim_id, engine, transmission, fuel_type, horsepower, etc.)
  vehicle_features (id, trim_id, feature_name, category, is_standard)
  ```

- [ ] 2B.2. **Obtener Datos de Catálogo** (3-4h)
  - [ ] **Opción A:** API pública de vehículos (NHTSA vPIC API - Gratis)
    - URL: https://vpic.nhtsa.dot.gov/api/
    - Datos: Marcas, modelos, especificaciones oficiales
  - [ ] **Opción B:** Web scraping de CarGurus/AutoTrader (con permiso)
  - [ ] **Opción C:** Datasets públicos (Kaggle, GitHub)
    - https://github.com/datasets/car-prices
    - https://www.kaggle.com/datasets/CooperUnion/cardataset
  - [ ] Descargar JSON/CSV con ~500-1000 vehículos populares

- [ ] 2B.3. **Crear Script de Seed** (4-5h)
  ```csharp
  // backend/VehicleService/Scripts/SeedVehicleCatalog.cs
  public class SeedVehicleCatalog
  {
      public async Task ExecuteAsync()
      {
          // 1. Leer datos de JSON/CSV
          var catalogData = await File.ReadAllTextAsync("Data/vehicle_catalog.json");
          var vehicles = JsonSerializer.Deserialize<VehicleCatalog>(catalogData);
          
          // 2. Popular marcas (Toyota, Ford, Honda, etc.)
          foreach (var make in vehicles.Makes)
          {
              await _makeRepository.AddAsync(new VehicleMake 
              { 
                  Name = make.Name, 
                  LogoUrl = make.LogoUrl,
                  Country = make.Country 
              });
          }
          
          // 3. Popular modelos por marca
          foreach (var model in vehicles.Models)
          {
              await _modelRepository.AddAsync(new VehicleModel
              {
                  MakeId = model.MakeId,
                  Name = model.Name,
                  BodyType = model.BodyType,
                  YearsProduced = model.YearsProduced
              });
          }
          
          // 4. Popular specs (motor, transmisión, etc.)
          // 5. Popular features (aire acondicionado, ABS, etc.)
      }
  }
  ```

- [ ] 2B.4. **Ejecutar Seed** (1-2h)
  ```bash
  # Comando para seed
  dotnet run --project backend/VehicleService/VehicleService.Api -- seed-catalog
  
  # Validar datos cargados
  docker exec -it vehicleservice-db psql -U postgres -d vehicleservice
  SELECT COUNT(*) FROM vehicle_makes;   -- Debe mostrar ~50-100 marcas
  SELECT COUNT(*) FROM vehicle_models;  -- Debe mostrar ~500-1000 modelos
  SELECT COUNT(*) FROM vehicle_specs;   -- Debe mostrar ~1000-2000 specs
  ```

- [ ] 2B.5. **Crear Endpoints de Catálogo** (2-3h)
  ```csharp
  // VehiclesController.cs
  
  [HttpGet("makes")]
  public async Task<IActionResult> GetMakes()
  {
      // Retorna: [{ id, name, logoUrl, modelsCount }]
  }
  
  [HttpGet("makes/{makeId}/models")]
  public async Task<IActionResult> GetModelsByMake(Guid makeId)
  {
      // Retorna: [{ id, name, bodyType, yearsProduced }]
  }
  
  [HttpGet("models/{modelId}/years")]
  public async Task<IActionResult> GetYearsByModel(Guid modelId)
  {
      // Retorna: [2020, 2021, 2022, 2023, 2024]
  }
  
  [HttpGet("models/{modelId}/years/{year}/trims")]
  public async Task<IActionResult> GetTrimsByModelYear(Guid modelId, int year)
  {
      // Retorna: [{ id, name, specs, features, price }]
  }
  ```

**Resultado Esperado:**
- ✅ Base de datos poblada con ~50-100 marcas de vehículos
- ✅ ~500-1000 modelos con años y especificaciones
- ✅ Frontend puede mostrar dropdowns con marcas/modelos reales
- ✅ Filtros de búsqueda funcionan con datos reales
- ✅ Sin dependencia de APIs externas para catálogo básico

### Fase 3: Frontend - Vehicle Service Layer (1 hora)

- [ ] 3.1. Actualizar vehicleService.ts con endpoints reales
- [ ] 3.2. Crear tipos TypeScript para Vehicle
- [ ] 3.3. Implementar funciones de filtrado
- [ ] 3.4. Agregar cache con TanStack Query

### Fase 4: Frontend - Vehicle Store (45 min)

- [ ] 4.1. Crear Zustand store para vehículos
- [ ] 4.2. Implementar acciones (create, update, delete)
- [ ] 4.3. Gestionar filtros y paginación
- [ ] 4.4. Implementar favoritos

### Fase 5: Frontend - UI Components (2 horas)

- [ ] 5.1. Actualizar VehicleListPage
- [ ] 5.2. Actualizar VehicleDetailPage
- [ ] 5.3. Crear VehicleFormPage (create/edit)
- [ ] 5.4. Crear componentes de filtros
- [ ] 5.5. Agregar loading states y skeletons

---

## 📝 IMPLEMENTACIÓN DETALLADA

### 1️⃣ Backend - Crear VehicleService

**Estructura del servicio:**

```
backend/VehicleService/
├── VehicleService.Api/
│   ├── Controllers/
│   │   └── VehiclesController.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── Dockerfile
├── VehicleService.Application/
│   ├── Features/
│   │   ├── Commands/
│   │   │   ├── CreateVehicleCommand.cs
│   │   │   ├── UpdateVehicleCommand.cs
│   │   │   └── DeleteVehicleCommand.cs
│   │   └── Queries/
│   │       ├── GetVehiclesQuery.cs
│   │       ├── GetVehicleByIdQuery.cs
│   │       └── GetFeaturedVehiclesQuery.cs
│   └── DTOs/
│       └── VehicleDto.cs
├── VehicleService.Domain/
│   ├── Entities/
│   │   └── Vehicle.cs
│   ├── Enums/
│   │   ├── FuelType.cs
│   │   ├── Transmission.cs
│   │   └── VehicleStatus.cs
│   └── ValueObjects/
│       └── VehicleSpecs.cs
└── VehicleService.Infrastructure/
    ├── Persistence/
    │   ├── VehicleDbContext.cs
    │   └── Repositories/
    │       └── VehicleRepository.cs
    └── Extensions/
```

---

### 2️⃣ Backend - Vehicle Entity

**Archivo:** `backend/VehicleService/VehicleService.Domain/Entities/Vehicle.cs`

```csharp
using CarDealer.Shared.MultiTenancy;

namespace VehicleService.Domain.Entities;

public class Vehicle : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }  // Multi-tenant

    // Basic Info
    public string Title { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    
    // Specifications
    public int Mileage { get; set; }  // in kilometers
    public FuelType FuelType { get; set; }
    public Transmission Transmission { get; set; }
    public BodyType BodyType { get; set; }
    public string ExteriorColor { get; set; } = string.Empty;
    public string InteriorColor { get; set; } = string.Empty;
    public int Doors { get; set; }
    public int Seats { get; set; }
    
    // Engine
    public string EngineSize { get; set; } = string.Empty;  // e.g., "2.0L"
    public int? Horsepower { get; set; }
    public int? Cylinders { get; set; }
    
    // Additional Info
    public string VIN { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
    
    // Media
    public List<string> ImageUrls { get; set; } = new();
    public string? VideoUrl { get; set; }
    
    // Location
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    // Seller Info (embedded for denormalization)
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string? SellerPhone { get; set; }
    public string? SellerEmail { get; set; }
    
    // Status
    public VehicleStatus Status { get; set; } = VehicleStatus.Pending;
    public bool IsFeatured { get; set; }
    public bool IsPromoted { get; set; }
    public DateTime? FeaturedUntil { get; set; }
    
    // Metrics
    public int ViewCount { get; set; }
    public int ContactCount { get; set; }
    public int FavoriteCount { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

// Enums
public enum FuelType
{
    Gasoline,
    Diesel,
    Electric,
    Hybrid,
    PlugInHybrid,
    CNG,
    LPG
}

public enum Transmission
{
    Manual,
    Automatic,
    SemiAutomatic,
    CVT
}

public enum BodyType
{
    Sedan,
    SUV,
    Truck,
    Coupe,
    Convertible,
    Wagon,
    Van,
    Hatchback,
    Minivan
}

public enum VehicleStatus
{
    Pending,      // Esperando aprobación
    Approved,     // Aprobado y visible
    Rejected,     // Rechazado
    Sold,         // Vendido
    Inactive      // Desactivado por dealer
}
```

---

### 3️⃣ Backend - VehiclesController

**Archivo:** `backend/VehicleService/VehicleService.Api/Controllers/VehiclesController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using VehicleService.Application.Features.Commands;
using VehicleService.Application.Features.Queries;
using VehicleService.Application.DTOs;

namespace VehicleService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VehiclesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<VehiclesController> _logger;

    public VehiclesController(IMediator mediator, ILogger<VehiclesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all vehicles with filters and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<VehicleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVehicles(
        [FromQuery] string? search,
        [FromQuery] string? make,
        [FromQuery] string? model,
        [FromQuery] int? minYear,
        [FromQuery] int? maxYear,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int? minMileage,
        [FromQuery] int? maxMileage,
        [FromQuery] string? fuelType,
        [FromQuery] string? transmission,
        [FromQuery] string? bodyType,
        [FromQuery] string? location,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken ct = default)
    {
        var query = new GetVehiclesQuery
        {
            Search = search,
            Make = make,
            Model = model,
            MinYear = minYear,
            MaxYear = maxYear,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            MinMileage = minMileage,
            MaxMileage = maxMileage,
            FuelType = fuelType,
            Transmission = transmission,
            BodyType = bodyType,
            Location = location,
            SortBy = sortBy,
            SortDescending = sortDescending,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get vehicle by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetVehicleByIdQuery(id);
        var result = await _mediator.Send(query, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        // Increment view count (fire and forget)
        _ = Task.Run(() => _mediator.Send(new IncrementViewCountCommand(id), ct));

        return Ok(result.Value);
    }

    /// <summary>
    /// Get featured vehicles
    /// </summary>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(List<VehicleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeatured(
        [FromQuery] int limit = 6,
        CancellationToken ct = default)
    {
        var query = new GetFeaturedVehiclesQuery(limit);
        var result = await _mediator.Send(query, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Create new vehicle
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateVehicleRequest request,
        CancellationToken ct)
    {
        var command = new CreateVehicleCommand
        {
            Title = request.Title,
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            Price = request.Price,
            Mileage = request.Mileage,
            FuelType = request.FuelType,
            Transmission = request.Transmission,
            BodyType = request.BodyType,
            ExteriorColor = request.ExteriorColor,
            InteriorColor = request.InteriorColor,
            Description = request.Description,
            Features = request.Features,
            Location = request.Location,
            VIN = request.VIN
        };

        var result = await _mediator.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            result.Value
        );
    }

    /// <summary>
    /// Update vehicle
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateVehicleRequest request,
        CancellationToken ct)
    {
        var command = new UpdateVehicleCommand
        {
            Id = id,
            Title = request.Title,
            Price = request.Price,
            Mileage = request.Mileage,
            Description = request.Description,
            Features = request.Features,
            Status = request.Status
        };

        var result = await _mediator.Send(command, ct);

        if (!result.IsSuccess)
            return result.Error!.Contains("not found")
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Delete vehicle
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var command = new DeleteVehicleCommand(id);
        var result = await _mediator.Send(command, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }

    /// <summary>
    /// Get makes (unique list)
    /// </summary>
    [HttpGet("makes")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMakes(CancellationToken ct)
    {
        var query = new GetVehicleMakesQuery();
        var result = await _mediator.Send(query, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get models by make
    /// </summary>
    [HttpGet("models")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModels(
        [FromQuery] string make,
        CancellationToken ct)
    {
        var query = new GetVehicleModelsQuery(make);
        var result = await _mediator.Send(query, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }
}
```

---

### 4️⃣ Frontend - Vehicle Types

**Archivo:** `frontend/web/original/src/types/vehicle.ts`

```typescript
export interface Vehicle {
  id: string;
  dealerId: string;
  
  // Basic Info
  title: string;
  make: string;
  model: string;
  year: number;
  price: number;
  currency: string;
  
  // Specifications
  mileage: number;
  fuelType: FuelType;
  transmission: Transmission;
  bodyType: BodyType;
  exteriorColor: string;
  interiorColor: string;
  doors: number;
  seats: number;
  
  // Engine
  engineSize?: string;
  horsepower?: number;
  cylinders?: number;
  
  // Additional
  vin: string;
  description: string;
  features: string[];
  
  // Media
  imageUrls: string[];
  videoUrl?: string;
  
  // Location
  location: string;
  latitude?: number;
  longitude?: number;
  
  // Seller
  sellerId: string;
  sellerName: string;
  sellerPhone?: string;
  sellerEmail?: string;
  
  // Status
  status: VehicleStatus;
  isFeatured: boolean;
  isPromoted: boolean;
  featuredUntil?: string;
  
  // Metrics
  viewCount: number;
  contactCount: number;
  favoriteCount: number;
  
  // Audit
  createdAt: string;
  updatedAt?: string;
}

export type FuelType = 
  | 'Gasoline'
  | 'Diesel'
  | 'Electric'
  | 'Hybrid'
  | 'PlugInHybrid'
  | 'CNG'
  | 'LPG';

export type Transmission = 
  | 'Manual'
  | 'Automatic'
  | 'SemiAutomatic'
  | 'CVT';

export type BodyType = 
  | 'Sedan'
  | 'SUV'
  | 'Truck'
  | 'Coupe'
  | 'Convertible'
  | 'Wagon'
  | 'Van'
  | 'Hatchback'
  | 'Minivan';

export type VehicleStatus = 
  | 'Pending'
  | 'Approved'
  | 'Rejected'
  | 'Sold'
  | 'Inactive';

export interface VehicleFilters {
  search?: string;
  make?: string;
  model?: string;
  minYear?: number;
  maxYear?: number;
  minPrice?: number;
  maxPrice?: number;
  minMileage?: number;
  maxMileage?: number;
  fuelType?: FuelType;
  transmission?: Transmission;
  bodyType?: BodyType;
  location?: string;
}

export interface PaginatedVehicles {
  items: Vehicle[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateVehicleRequest {
  title: string;
  make: string;
  model: string;
  year: number;
  price: number;
  mileage: number;
  fuelType: FuelType;
  transmission: Transmission;
  bodyType: BodyType;
  exteriorColor: string;
  interiorColor: string;
  description: string;
  features: string[];
  location: string;
  vin: string;
}
```

---

### 5️⃣ Frontend - Vehicle Service

**Archivo:** `frontend/web/original/src/services/vehicleService.ts`

```typescript
import { api } from './api';
import type { Vehicle, VehicleFilters, PaginatedVehicles, CreateVehicleRequest } from '@/types/vehicle';

const VEHICLE_API_URL = '/vehicles';  // Via Gateway

export const vehicleService = {
  /**
   * Get all vehicles with filters and pagination
   */
  async getAll(
    filters?: VehicleFilters,
    page: number = 1,
    pageSize: number = 12,
    sortBy: string = 'CreatedAt',
    sortDescending: boolean = true
  ): Promise<PaginatedVehicles> {
    const params = new URLSearchParams();
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());
    params.append('sortBy', sortBy);
    params.append('sortDescending', sortDescending.toString());

    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          params.append(key, value.toString());
        }
      });
    }

    const response = await api.get<PaginatedVehicles>(`${VEHICLE_API_URL}?${params.toString()}`);
    return response.data;
  },

  /**
   * Get vehicle by ID
   */
  async getById(id: string): Promise<Vehicle> {
    const response = await api.get<Vehicle>(`${VEHICLE_API_URL}/${id}`);
    return response.data;
  },

  /**
   * Get featured vehicles
   */
  async getFeatured(limit: number = 6): Promise<Vehicle[]> {
    const response = await api.get<Vehicle[]>(`${VEHICLE_API_URL}/featured?limit=${limit}`);
    return response.data;
  },

  /**
   * Create new vehicle
   */
  async create(data: CreateVehicleRequest): Promise<Vehicle> {
    const response = await api.post<Vehicle>(VEHICLE_API_URL, data);
    return response.data;
  },

  /**
   * Update vehicle
   */
  async update(id: string, data: Partial<CreateVehicleRequest>): Promise<Vehicle> {
    const response = await api.put<Vehicle>(`${VEHICLE_API_URL}/${id}`, data);
    return response.data;
  },

  /**
   * Delete vehicle
   */
  async delete(id: string): Promise<void> {
    await api.delete(`${VEHICLE_API_URL}/${id}`);
  },

  /**
   * Get available makes
   */
  async getMakes(): Promise<string[]> {
    const response = await api.get<string[]>(`${VEHICLE_API_URL}/makes`);
    return response.data;
  },

  /**
   * Get available models for a make
   */
  async getModels(make: string): Promise<string[]> {
    const response = await api.get<string[]>(`${VEHICLE_API_URL}/models?make=${make}`);
    return response.data;
  },
};
```

---

### 6️⃣ Frontend - Vehicle Store with TanStack Query

**Archivo:** `frontend/web/original/src/hooks/useVehicles.ts`

```typescript
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { vehicleService } from '@/services/vehicleService';
import type { VehicleFilters, CreateVehicleRequest } from '@/types/vehicle';
import toast from 'react-hot-toast';

const VEHICLES_KEY = 'vehicles';

export const useVehicles = (
  filters?: VehicleFilters,
  page: number = 1,
  pageSize: number = 12
) => {
  return useQuery({
    queryKey: [VEHICLES_KEY, filters, page, pageSize],
    queryFn: () => vehicleService.getAll(filters, page, pageSize),
    staleTime: 5 * 60 * 1000, // 5 minutos
  });
};

export const useVehicle = (id: string) => {
  return useQuery({
    queryKey: [VEHICLES_KEY, id],
    queryFn: () => vehicleService.getById(id),
    enabled: !!id,
  });
};

export const useFeaturedVehicles = (limit: number = 6) => {
  return useQuery({
    queryKey: [VEHICLES_KEY, 'featured', limit],
    queryFn: () => vehicleService.getFeatured(limit),
    staleTime: 10 * 60 * 1000, // 10 minutos
  });
};

export const useCreateVehicle = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateVehicleRequest) => vehicleService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [VEHICLES_KEY] });
      toast.success('Vehículo creado exitosamente');
    },
    onError: (error: any) => {
      toast.error(error.message || 'Error al crear vehículo');
    },
  });
};

export const useUpdateVehicle = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<CreateVehicleRequest> }) =>
      vehicleService.update(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: [VEHICLES_KEY] });
      queryClient.invalidateQueries({ queryKey: [VEHICLES_KEY, variables.id] });
      toast.success('Vehículo actualizado exitosamente');
    },
    onError: (error: any) => {
      toast.error(error.message || 'Error al actualizar vehículo');
    },
  });
};

export const useDeleteVehicle = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => vehicleService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [VEHICLES_KEY] });
      toast.success('Vehículo eliminado exitosamente');
    },
    onError: (error: any) => {
      toast.error(error.message || 'Error al eliminar vehículo');
    },
  });
};

export const useVehicleMakes = () => {
  return useQuery({
    queryKey: [VEHICLES_KEY, 'makes'],
    queryFn: () => vehicleService.getMakes(),
    staleTime: 60 * 60 * 1000, // 1 hora
  });
};

export const useVehicleModels = (make: string) => {
  return useQuery({
    queryKey: [VEHICLES_KEY, 'models', make],
    queryFn: () => vehicleService.getModels(make),
    enabled: !!make,
    staleTime: 60 * 60 * 1000, // 1 hora
  });
};
```

---

### 7️⃣ Agregar VehicleService a compose.yaml

**Archivo:** `compose.yaml`

Agregar entrada para VehicleService:

```yaml
# Vehicle Service Database
vehicleservice-db:
  image: postgres:16
  container_name: vehicleservice-db
  environment:
    POSTGRES_DB: vehicleservice
    POSTGRES_USER: postgres
    POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-password}
  ports:
    - "25447:5432"
  volumes:
    - vehicleservice_data:/var/lib/postgresql/data
  networks:
    - cargurus-net
  healthcheck:
    test: ["CMD-SHELL", "pg_isready -U postgres"]
    interval: 10s
    timeout: 5s
    retries: 5
  deploy:
    resources:
      limits:
        cpus: '0.25'
        memory: 256M
      reservations:
        memory: 128M

# Vehicle Service API
vehicleservice:
  build:
    context: ./backend
    dockerfile: VehicleService/VehicleService.Api/Dockerfile.dev
  container_name: vehicleservice
  environment:
    ASPNETCORE_ENVIRONMENT: Development
    ASPNETCORE_URLS: http://+:80
    Database__Provider: "PostgreSQL"
    Database__Host: vehicleservice-db
    Database__Port: 5432
    Database__Database: vehicleservice
    Database__Username: postgres
    Database__Password: ${POSTGRES_PASSWORD:-password}
    Database__AutoMigrate: "true"
    Jwt__Key: ${JWT__KEY}
    RabbitMQ__Host: rabbitmq
  ports:
    - "15012:80"
  depends_on:
    vehicleservice-db:
      condition: service_healthy
    rabbitmq:
      condition: service_healthy
  networks:
    - cargurus-net
  deploy:
    resources:
      limits:
        cpus: '0.5'
        memory: 384M
      reservations:
        memory: 256M

volumes:
  vehicleservice_data:
```

---

## ✅ CRITERIOS DE ACEPTACIÓN

### Test Backend

```bash
# Health check
curl http://localhost:15012/health

# Get vehicles
curl http://localhost:15012/api/vehicles?page=1&pageSize=10

# Get featured
curl http://localhost:15012/api/vehicles/featured
```

### Test Frontend

1. Ir a http://localhost:5174/vehicles
2. Ver listado de vehículos con paginación
3. Aplicar filtros (marca, precio, etc.)
4. Click en un vehículo → Ver detalle
5. Login como dealer → Crear nuevo vehículo
6. Editar vehículo creado
7. Eliminar vehículo

---

## 📊 ESTIMACIÓN DE TOKENS

| Tarea | Tokens |
|-------|--------|
| VehicleService backend completo | 8,000 |
| Database schema y migraciones | 2,000 |
| VehicleController | 4,000 |
| Frontend types | 2,000 |
| Frontend service | 3,000 |
| TanStack Query hooks | 3,500 |
| Docker configuration | 1,500 |
| Testing | 2,000 |
| **TOTAL** | **~26,000** |

**Con buffer 15%:** ~30,000 tokens

---

## ➡️ PRÓXIMO SPRINT

**Sprint 4:** [SPRINT_4_MEDIA_UPLOAD.md](SPRINT_4_MEDIA_UPLOAD.md)

Upload de imágenes a S3/Azure, compresión automática, progressive loading.

---

**Estado:** ⚪ Pendiente  
**Última actualización:** 2 Enero 2026
