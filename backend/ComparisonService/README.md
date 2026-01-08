# 🚗 ComparisonService - Comparador de Vehículos

## 📋 Descripción

Servicio de microservicios para comparar hasta 3 vehículos lado a lado. Permite a los usuarios crear comparaciones guardadas y compartirlas mediante links públicos.

**Puerto:** 5066  
**Base de datos:** PostgreSQL (`comparisonservice`)

## 🎯 Funcionalidades

### ✅ Crear Comparaciones

- Comparar hasta 3 vehículos simultáneamente
- Validación automática del límite máximo
- Almacenamiento con JSONB para performance óptimo

### 📊 Gestión de Comparaciones

- Guardar comparaciones para uso posterior
- Actualizar vehículos en comparación existente
- Renombrar comparaciones
- Eliminar comparaciones

### 🔗 Compartir Comparaciones

- Generar links públicos con tokens únicos
- Permitir acceso anónimo a comparaciones compartidas
- URLs amigables: `/compare/{token}`
- Hacer privada una comparación pública

### 👤 Por Usuario

- Listar todas las comparaciones del usuario
- Autenticación JWT requerida
- Aislamiento de datos por usuario

## 🏗️ Arquitectura

```
ComparisonService/
├── ComparisonService.Domain/          # Entities & Interfaces
│   ├── Entities/
│   │   └── Comparison.cs              # Entity con lógica de negocio
│   └── Interfaces/
│       └── IComparisonRepository.cs   # Contrato del repositorio
├── ComparisonService.Infrastructure/  # Data Access
│   ├── Persistence/
│   │   └── ApplicationDbContext.cs    # EF Core DbContext
│   └── Repositories/
│       └── ComparisonRepository.cs    # Implementación
└── ComparisonService.Api/             # REST API
    ├── Controllers/
    │   └── ComparisonsController.cs   # 10 endpoints
    ├── Program.cs                     # DI + JWT + Health
    ├── Dockerfile                     # Multi-stage build
    └── appsettings.json
```

## 📡 API Endpoints

### Autenticados (JWT Required)

| Método   | Endpoint                         | Descripción                      |
| -------- | -------------------------------- | -------------------------------- |
| `GET`    | `/api/comparisons`               | Listar mis comparaciones         |
| `GET`    | `/api/comparisons/{id}`          | Obtener comparación con detalles |
| `POST`   | `/api/comparisons`               | Crear nueva comparación          |
| `PUT`    | `/api/comparisons/{id}/vehicles` | Actualizar vehículos             |
| `PUT`    | `/api/comparisons/{id}/name`     | Renombrar                        |
| `POST`   | `/api/comparisons/{id}/share`    | Hacer pública (genera token)     |
| `DELETE` | `/api/comparisons/{id}/share`    | Hacer privada                    |
| `DELETE` | `/api/comparisons/{id}`          | Eliminar comparación             |

### Públicos (No Auth)

| Método | Endpoint                          | Descripción                |
| ------ | --------------------------------- | -------------------------- |
| `GET`  | `/api/comparisons/shared/{token}` | Ver comparación compartida |
| `GET`  | `/health`                         | Health check               |

## 🔧 Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=comparisonservice;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "YourSecretKey",
    "Issuer": "CarDealer",
    "Audience": "CarDealerUsers"
  },
  "VehiclesServiceUrl": "http://vehiclessaleservice:8080"
}
```

### Variables de Entorno

| Variable                               | Descripción             | Ejemplo                           |
| -------------------------------------- | ----------------------- | --------------------------------- |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection   | `Host=postgres;...`               |
| `Jwt__Secret`                          | JWT signing key         | `YourSecretKey...`                |
| `VEHICLES_SERVICE_URL`                 | VehiclesSaleService URL | `http://vehiclessaleservice:8080` |

## 🗄️ Base de Datos

### Tabla: `comparisons`

| Columna      | Tipo         | Descripción                           |
| ------------ | ------------ | ------------------------------------- |
| `Id`         | UUID         | Primary key                           |
| `UserId`     | UUID         | FK a usuario (auth)                   |
| `Name`       | VARCHAR(200) | Nombre de la comparación              |
| `VehicleIds` | JSONB        | Array de GUIDs                        |
| `IsPublic`   | BOOLEAN      | Si es compartible                     |
| `ShareToken` | VARCHAR(100) | Token único para compartir (nullable) |
| `CreatedAt`  | TIMESTAMP    | Fecha creación                        |
| `UpdatedAt`  | TIMESTAMP    | Última actualización                  |

**Índices:**

- `idx_comparisons_user` en `UserId`
- `idx_comparisons_share_token` UNIQUE en `ShareToken`

## 🚀 Uso con Docker

### Desarrollo Local

```bash
# 1. Levantar PostgreSQL
docker-compose up -d postgres

# 2. Build y run
cd backend/ComparisonService
docker build -t comparisonservice:latest .
docker run -p 5066:8080 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=comparisonservice;Username=postgres;Password=postgres" \
  -e Jwt__Secret="YourSecretKey" \
  comparisonservice:latest

# 3. Verificar
curl http://localhost:5066/health
```

### Producción (Kubernetes)

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: comparisonservice
spec:
  replicas: 2
  template:
    spec:
      containers:
        - name: comparisonservice
          image: ghcr.io/gregorymorenoiem/cardealer-comparisonservice:latest
          ports:
            - containerPort: 8080
          env:
            - name: ConnectionStrings__DefaultConnection
              valueFrom:
                secretKeyRef:
                  name: database-secrets
                  key: comparison-connection
            - name: VEHICLES_SERVICE_URL
              value: "http://vehiclessaleservice:8080"
```

## 📝 Ejemplos de Uso

### 1. Crear Comparación

```bash
curl -X POST http://localhost:5066/api/comparisons \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "SUVs Familiares",
    "vehicleIds": [
      "550e8400-e29b-41d4-a716-446655440001",
      "550e8400-e29b-41d4-a716-446655440002"
    ],
    "isPublic": false
  }'
```

**Respuesta:**

```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "name": "SUVs Familiares",
  "vehicleIds": ["550e8400...", "550e8400..."],
  "vehicleCount": 2,
  "createdAt": "2026-01-08T12:00:00Z",
  "updatedAt": "2026-01-08T12:00:00Z",
  "isPublic": false,
  "hasShareLink": false
}
```

### 2. Compartir Comparación

```bash
curl -X POST http://localhost:5066/api/comparisons/7c9e6679-7425-40de-944b-e07fc1f90ae7/share \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Respuesta:**

```json
{
  "shareToken": "abc123xyz",
  "shareUrl": "https://okla.com.do/compare/abc123xyz"
}
```

### 3. Ver Comparación Compartida (No Auth)

```bash
curl http://localhost:5066/api/comparisons/shared/abc123xyz
```

**Respuesta:**

```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "name": "SUVs Familiares",
  "vehicleIds": ["550e8400...", "550e8400..."],
  "vehicles": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "title": "Toyota RAV4 2024",
      "price": 35000,
      "make": "Toyota",
      "model": "RAV4",
      "year": 2024,
      "mileage": 0,
      "fuelType": "Híbrido",
      "transmission": "Automática",
      "bodyStyle": "SUV",
      "condition": "Nuevo",
      "primaryImageUrl": "https://..."
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440002",
      "title": "Honda CR-V 2024",
      "price": 33000,
      "make": "Honda",
      "model": "CR-V",
      "year": 2024,
      "mileage": 0,
      "fuelType": "Gasolina",
      "transmission": "CVT",
      "bodyStyle": "SUV",
      "condition": "Nuevo",
      "primaryImageUrl": "https://..."
    }
  ],
  "createdAt": "2026-01-08T12:00:00Z",
  "isPublic": true,
  "shareUrl": "https://okla.com.do/compare/abc123xyz"
}
```

## 🧪 Testing

### Health Check

```bash
curl http://localhost:5066/health
# Expected: Healthy
```

### Swagger UI

Abrir navegador: `http://localhost:5066/swagger`

## 🔐 Seguridad

- **Autenticación:** JWT Bearer Token
- **Autorización:** User debe ser owner de la comparación para modificar/eliminar
- **Share Tokens:** Aleatorios de 12 caracteres (Base64)
- **Validación:** Máximo 3 vehículos por comparación

## 📊 Métricas y Monitoreo

- Health check en `/health` incluye verificación de PostgreSQL
- Logs estructurados con `ILogger`
- Ready para APM (Application Performance Monitoring)

## 🐛 Troubleshooting

### Error: "No puede comparar más de 3 vehículos"

- Validación: La entidad `Comparison` limita a 3 vehículos máximo
- Solución: Eliminar vehículos antes de agregar nuevos

### Error: "Token de compartir inválido"

- El token no existe o la comparación fue hecha privada
- Verificar que `ShareToken` no sea null en la DB

### Error: "Cannot fetch vehicle details"

- El VehiclesSaleService no responde
- Verificar variable `VEHICLES_SERVICE_URL`

## 🔄 Integración con VehiclesSaleService

El servicio consume el endpoint `GET /api/vehicles/{id}` de VehiclesSaleService para obtener detalles completos de cada vehículo en la comparación.

**Configuración:**

- Variable de entorno: `VEHICLES_SERVICE_URL`
- Default: `http://vehiclessaleservice:8080`
- Timeout configurado en HttpClient

## 📈 Roadmap

- [ ] Cache de vehículos frecuentes (Redis)
- [ ] Notificaciones por email al compartir
- [ ] Analytics de comparaciones más vistas
- [ ] Export a PDF de comparación

---

**Mantenido por:** Equipo OKLA  
**Última actualización:** Enero 2026  
**Sprint:** Sprint 2 - Experiencia de Usuario
