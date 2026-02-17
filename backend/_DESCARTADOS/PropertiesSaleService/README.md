# 📦 ProductService - Servicio de Productos con Campos Personalizados

## 🎯 Descripción

Servicio de gestión de productos genéricos para marketplace con soporte de **campos personalizados dinámicos**. Permite crear productos de cualquier tipo (vehículos, inmuebles, electrónicos, etc.) con campos básicos comunes + campos específicos por categoría.

## 🏗️ Arquitectura

### Clean Architecture
```
ProductService/
├── ProductService.Domain/          # Entidades y contratos
│   ├── Entities/
│   │   └── Product.cs             # Entidad con campos básicos + JSON custom
│   └── Interfaces/
│       ├── IProductRepository.cs
│       └── ICategoryRepository.cs
├── ProductService.Application/     # Lógica de negocio
├── ProductService.Infrastructure/  # EF Core + PostgreSQL
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs
│   │   └── ApplicationDbContextFactory.cs
│   └── Repositories/
│       ├── ProductRepository.cs
│       └── CategoryRepository.cs
├── ProductService.Api/             # REST API
│   ├── Controllers/
│   │   ├── ProductsController.cs
│   │   └── CategoriesController.cs
│   └── Program.cs
└── ProductService.Shared/          # Utilidades comunes
```

## 📊 Modelo de Datos

### Product (Campos Básicos)
- **Id** (Guid): Identificador único
- **Name** (string): Nombre del producto
- **Description** (string): Descripción
- **Price** (decimal): Precio
- **Currency** (string): Moneda (USD por defecto)
- **Status** (enum): Draft | Active | Sold | Reserved | Archived
- **ImageUrl** (string): URL imagen principal
- **SellerId** (Guid): ID del vendedor
- **SellerName** (string): Nombre del vendedor
- **CategoryId** (Guid): ID de categoría
- **CategoryName** (string): Nombre de categoría
- **CreatedAt** (DateTime): Fecha creación
- **UpdatedAt** (DateTime): Fecha actualización
- **IsDeleted** (bool): Borrado lógico

### Product (Campos Personalizados)
- **CustomFieldsJson** (jsonb): Campos dinámicos como JSON en PostgreSQL
- **CustomFields** (ICollection): Alternativa EAV para queries complejas

### Ejemplo: Producto Vehículo
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "Toyota Camry 2020",
  "description": "Sedan en excelente estado",
  "price": 25000,
  "currency": "USD",
  "status": "Active",
  "categoryId": "11111111-1111-1111-1111-111111111111",
  "categoryName": "Vehículos",
  "customFieldsJson": {
    "make": "Toyota",
    "model": "Camry",
    "year": 2020,
    "mileage": 50000,
    "color": "Silver",
    "transmission": "Automatic",
    "fuelType": "Gasoline"
  }
}
```

### Ejemplo: Producto Inmueble
```json
{
  "id": "789e4567-e89b-12d3-a456-426614174111",
  "name": "Casa 3 Habitaciones",
  "description": "Casa familiar con jardín",
  "price": 350000,
  "currency": "USD",
  "status": "Active",
  "categoryId": "22222222-2222-2222-2222-222222222222",
  "categoryName": "Inmuebles",
  "customFieldsJson": {
    "bedrooms": 3,
    "bathrooms": 2,
    "sqft": 1500,
    "parking": true,
    "furnished": false
  }
}
```

## 📁 Categorías Pre-Cargadas

1. **Vehículos** (vehiculos)
   - Campos: make, model, year, mileage, transmission, fuelType, color
   
2. **Inmuebles** (inmuebles)
   - Campos: bedrooms, bathrooms, sqft, parking, furnished
   
3. **Electrónicos** (electronicos)
   - Campos: brand, condition, warranty

## 🚀 Endpoints API

### Products
```
GET    /api/products                    # Buscar con filtros
GET    /api/products/{id}               # Obtener por ID
GET    /api/products/seller/{sellerId}  # Productos de un vendedor
POST   /api/products                    # Crear producto
PUT    /api/products/{id}               # Actualizar producto
DELETE /api/products/{id}               # Eliminar (soft delete)
```

### Categories
```
GET    /api/categories                  # Todas las categorías
GET    /api/categories/root             # Categorías raíz
GET    /api/categories/{id}             # Obtener por ID
GET    /api/categories/slug/{slug}      # Obtener por slug
GET    /api/categories/{id}/children    # Subcategorías
```

## 🔨 Compilación

```powershell
cd backend/ProductService
dotnet build ProductService.sln
```

**Resultado**: ✅ 0 errores, 0 warnings

## 🗄️ Base de Datos

### Crear Base de Datos (Docker)
```powershell
docker run --name postgres-productservice `
  -e POSTGRES_PASSWORD=postgres123 `
  -e POSTGRES_DB=productservice_db `
  -p 5432:5432 `
  -d postgres:15
```

### Aplicar Migraciones
```powershell
cd ProductService.Api
dotnet ef database update --project ..\ProductService.Infrastructure
```

### Crear Nueva Migración
```powershell
dotnet ef migrations add NombreMigracion `
  --project ..\ProductService.Infrastructure `
  --startup-project .
```

## ▶️ Ejecutar Servicio

```powershell
cd backend/ProductService/ProductService.Api
dotnet run
```

El servicio inicia en: **http://localhost:5006**

### Swagger UI
http://localhost:5006/swagger

### Health Check
http://localhost:5006/health

## 📝 Uso de la API

### Crear Producto Vehículo
```bash
curl -X POST http://localhost:5006/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Toyota Camry 2020",
    "description": "Sedan en excelente estado",
    "price": 25000,
    "currency": "USD",
    "sellerId": "550e8400-e29b-41d4-a716-446655440000",
    "sellerName": "Juan Pérez",
    "categoryId": "11111111-1111-1111-1111-111111111111",
    "customFields": {
      "make": "Toyota",
      "model": "Camry",
      "year": 2020,
      "mileage": 50000,
      "color": "Silver",
      "transmission": "Automatic",
      "fuelType": "Gasoline"
    },
    "images": [
      "https://example.com/images/camry1.jpg",
      "https://example.com/images/camry2.jpg"
    ]
  }'
```

### Buscar Productos
```bash
# Todos los productos activos
curl http://localhost:5006/api/products

# Filtrar por categoría (vehículos)
curl "http://localhost:5006/api/products?categoryId=11111111-1111-1111-1111-111111111111"

# Buscar por texto
curl "http://localhost:5006/api/products?search=Toyota"

# Filtrar por rango de precio
curl "http://localhost:5006/api/products?minPrice=20000&maxPrice=30000"
```

### Obtener Producto por ID
```bash
curl http://localhost:5006/api/products/123e4567-e89b-12d3-a456-426614174000
```

### Actualizar Producto
```bash
curl -X PUT http://localhost:5006/api/products/123e4567-e89b-12d3-a456-426614174000 \
  -H "Content-Type: application/json" \
  -d '{
    "price": 24000,
    "status": "Active",
    "customFields": {
      "mileage": 51000
    }
  }'
```

### Obtener Categorías
```bash
# Todas las categorías
curl http://localhost:5006/api/categories

# Categorías raíz
curl http://localhost:5006/api/categories/root

# Por slug
curl http://localhost:5006/api/categories/slug/vehiculos
```

## 🎨 Características Técnicas

### Ventajas de CustomFieldsJson (JSONB)
✅ **Flexibilidad**: Agregar campos sin modificar schema  
✅ **Performance**: Índices GIN en PostgreSQL  
✅ **Queries JSON**: `WHERE customFieldsJson->>'make' = 'Toyota'`  
✅ **Tipado dinámico**: Strings, numbers, booleans, arrays  
✅ **Sin migraciones**: Para nuevos campos personalizados  

### Alternativa EAV (ProductCustomField)
Si necesitas queries más complejas:
```sql
SELECT * FROM products p
INNER JOIN product_custom_fields cf ON p.id = cf.product_id
WHERE cf.key = 'year' AND CAST(cf.value AS INT) >= 2020
```

## 🔄 Diferencias con VehicleService Original

| Aspecto | VehicleService | ProductService |
|---------|---------------|----------------|
| **Dominio** | Solo vehículos | Cualquier producto |
| **Campos** | Hardcoded en entidad | JSON dinámico |
| **Categorías** | No tiene | Multi-categoría jerárquica |
| **Flexibilidad** | Baja | Alta |
| **Extensibilidad** | Requiere migraciones | Campos por JSON |
| **Compilación** | 66 errores | ✅ 0 errores |

## 📦 Dependencias

- .NET 8.0
- Entity Framework Core 8.0
- PostgreSQL (Npgsql)
- Swashbuckle (Swagger)
- AspNetCore.HealthChecks.NpgSql

## 🧪 Testing (TODO)

```powershell
# Unit tests
dotnet test ProductService.Tests/ProductService.Tests.csproj

# Integration tests
dotnet test ProductService.IntegrationTests/ProductService.IntegrationTests.csproj
```

## 📊 Próximos Pasos

- [ ] Agregar autenticación JWT
- [ ] Implementar paginación
- [ ] Agregar filtros por campos personalizados
- [ ] Implementar búsqueda full-text
- [ ] Agregar caché con Redis
- [ ] Implementar eventos de dominio
- [ ] Agregar tests unitarios
- [ ] Agregar tests de integración
- [ ] Dockerizar servicio
- [ ] Agregar a docker-compose.yml

## 📄 Licencia

MIT

---

**✅ COMPLETADO**: ProductService funcional con arquitectura flexible y 0 errores de compilación.
