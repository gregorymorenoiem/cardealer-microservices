# 🔔 AlertService - Alertas y Búsquedas Guardadas

## 📋 Descripción

Servicio de microservicios para gestionar **alertas de precio** y **búsquedas guardadas** con notificaciones automáticas. Permite a los usuarios ser notificados cuando un vehículo alcanza su precio objetivo o cuando hay nuevos resultados en sus búsquedas guardadas.

**Puerto:** 5067  
**Base de datos:** PostgreSQL (`alertservice`)

## 🎯 Funcionalidades

### 🔔 Alertas de Precio

- Crear alertas para ser notificado cuando el precio cambie
- Dos condiciones: `≤ precio objetivo` o `≥ precio objetivo`
- Activar/desactivar alertas temporalmente
- Resetear alertas ya disparadas
- Una alerta por vehículo por usuario

### 🔍 Búsquedas Guardadas

- Guardar criterios de búsqueda complejos
- Notificaciones por email: Instant, Daily, Weekly
- Activar/desactivar notificaciones
- Actualizar criterios sin perder historial
- Almacenamiento en JSONB para flexibilidad máxima

## 🏗️ Arquitectura

```
AlertService/
├── AlertService.Domain/                # Entities & Interfaces
│   ├── Entities/
│   │   ├── PriceAlert.cs              # Alertas de precio
│   │   └── SavedSearch.cs             # Búsquedas guardadas
│   └── Interfaces/
│       ├── IPriceAlertRepository.cs
│       └── ISavedSearchRepository.cs
├── AlertService.Infrastructure/        # Data Access
│   ├── Persistence/
│   │   └── ApplicationDbContext.cs    # EF Core DbContext
│   └── Repositories/
│       ├── PriceAlertRepository.cs
│       └── SavedSearchRepository.cs
└── AlertService.Api/                   # REST API
    ├── Controllers/
    │   ├── PriceAlertsController.cs   # 8 endpoints
    │   └── SavedSearchesController.cs # 8 endpoints
    ├── Program.cs                     # DI + JWT + Health
    ├── Dockerfile
    └── appsettings.json
```

## 📡 API Endpoints

### Price Alerts (JWT Required)

| Método   | Endpoint                             | Descripción                |
| -------- | ------------------------------------ | -------------------------- |
| `GET`    | `/api/pricealerts`                   | Listar mis alertas         |
| `GET`    | `/api/pricealerts/{id}`              | Obtener alerta específica  |
| `POST`   | `/api/pricealerts`                   | Crear nueva alerta         |
| `PUT`    | `/api/pricealerts/{id}/target-price` | Actualizar precio objetivo |
| `POST`   | `/api/pricealerts/{id}/activate`     | Activar alerta             |
| `POST`   | `/api/pricealerts/{id}/deactivate`   | Desactivar alerta          |
| `POST`   | `/api/pricealerts/{id}/reset`        | Resetear alerta disparada  |
| `DELETE` | `/api/pricealerts/{id}`              | Eliminar alerta            |

### Saved Searches (JWT Required)

| Método   | Endpoint                                | Descripción                 |
| -------- | --------------------------------------- | --------------------------- |
| `GET`    | `/api/savedsearches`                    | Listar mis búsquedas        |
| `GET`    | `/api/savedsearches/{id}`               | Obtener búsqueda específica |
| `POST`   | `/api/savedsearches`                    | Crear nueva búsqueda        |
| `PUT`    | `/api/savedsearches/{id}/name`          | Renombrar búsqueda          |
| `PUT`    | `/api/savedsearches/{id}/criteria`      | Actualizar criterios        |
| `PUT`    | `/api/savedsearches/{id}/notifications` | Config notificaciones       |
| `POST`   | `/api/savedsearches/{id}/activate`      | Activar búsqueda            |
| `POST`   | `/api/savedsearches/{id}/deactivate`    | Desactivar búsqueda         |
| `DELETE` | `/api/savedsearches/{id}`               | Eliminar búsqueda           |

### Públicos

| Método | Endpoint  | Descripción  |
| ------ | --------- | ------------ |
| `GET`  | `/health` | Health check |

## 🗄️ Base de Datos

### Tabla: `price_alerts`

| Columna       | Tipo          | Descripción                             |
| ------------- | ------------- | --------------------------------------- |
| `Id`          | UUID          | Primary key                             |
| `UserId`      | UUID          | FK a usuario                            |
| `VehicleId`   | UUID          | FK a vehículo                           |
| `TargetPrice` | DECIMAL(18,2) | Precio objetivo                         |
| `Condition`   | INT           | 0=LessThanOrEqual, 1=GreaterThanOrEqual |
| `IsActive`    | BOOLEAN       | Si está activa                          |
| `IsTriggered` | BOOLEAN       | Si ya se disparó                        |
| `TriggeredAt` | TIMESTAMP     | Cuándo se disparó                       |
| `CreatedAt`   | TIMESTAMP     | Fecha creación                          |
| `UpdatedAt`   | TIMESTAMP     | Última actualización                    |

**Índices:**

- `idx_price_alerts_user` en `UserId`
- `idx_price_alerts_vehicle` en `VehicleId`
- `idx_price_alerts_user_vehicle` UNIQUE en `(UserId, VehicleId)`
- `idx_price_alerts_active` en `IsActive`

### Tabla: `saved_searches`

| Columna                  | Tipo         | Descripción                  |
| ------------------------ | ------------ | ---------------------------- |
| `Id`                     | UUID         | Primary key                  |
| `UserId`                 | UUID         | FK a usuario                 |
| `Name`                   | VARCHAR(200) | Nombre de la búsqueda        |
| `SearchCriteria`         | JSONB        | Criterios de búsqueda        |
| `SendEmailNotifications` | BOOLEAN      | Si enviar emails             |
| `Frequency`              | INT          | 0=Instant, 1=Daily, 2=Weekly |
| `LastNotificationSent`   | TIMESTAMP    | Última notificación enviada  |
| `IsActive`               | BOOLEAN      | Si está activa               |
| `CreatedAt`              | TIMESTAMP    | Fecha creación               |
| `UpdatedAt`              | TIMESTAMP    | Última actualización         |

**Índices:**

- `idx_saved_searches_user` en `UserId`
- `idx_saved_searches_active` en `IsActive`
- `idx_saved_searches_last_notification` en `LastNotificationSent`

## 🔧 Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=alertservice;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "YourSecretKey",
    "Issuer": "CarDealer",
    "Audience": "CarDealerUsers"
  }
}
```

## 📝 Ejemplos de Uso

### 1. Crear Alerta de Precio (Comprador)

Un usuario quiere ser notificado cuando el precio de un vehículo **baje a $25,000 o menos**:

```bash
curl -X POST http://localhost:5067/api/pricealerts \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "vehicleId": "550e8400-e29b-41d4-a716-446655440001",
    "targetPrice": 25000,
    "condition": 0
  }'
```

**Respuesta:**

```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "vehicleId": "550e8400-e29b-41d4-a716-446655440001",
  "targetPrice": 25000,
  "condition": "LessThanOrEqual",
  "isActive": true,
  "isTriggered": false,
  "triggeredAt": null,
  "createdAt": "2026-01-08T15:00:00Z",
  "updatedAt": "2026-01-08T15:00:00Z"
}
```

### 2. Crear Alerta de Precio (Vendedor)

Un vendedor quiere ser notificado cuando el precio de mercado **suba a $30,000 o más** para ajustar su pricing:

```bash
curl -X POST http://localhost:5067/api/pricealerts \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "vehicleId": "550e8400-e29b-41d4-a716-446655440002",
    "targetPrice": 30000,
    "condition": 1
  }'
```

### 3. Crear Búsqueda Guardada

Guardar búsqueda de "SUVs Toyota 2023-2024 bajo $35K" con notificaciones diarias:

```bash
curl -X POST http://localhost:5067/api/savedsearches \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "SUVs Toyota bajo $35K",
    "searchCriteria": "{\"make\":\"Toyota\",\"bodyStyle\":\"SUV\",\"yearMin\":2023,\"yearMax\":2024,\"priceMax\":35000}",
    "sendEmailNotifications": true,
    "frequency": 1
  }'
```

**Respuesta:**

```json
{
  "id": "8d0f7680-8536-52ef-b825-557766551b18",
  "name": "SUVs Toyota bajo $35K",
  "searchCriteria": "{\"make\":\"Toyota\",\"bodyStyle\":\"SUV\",\"yearMin\":2023,\"yearMax\":2024,\"priceMax\":35000}",
  "sendEmailNotifications": true,
  "frequency": "Daily",
  "lastNotificationSent": null,
  "isActive": true,
  "createdAt": "2026-01-08T15:30:00Z",
  "updatedAt": "2026-01-08T15:30:00Z"
}
```

### 4. Actualizar Criterios de Búsqueda

Usuario decide ampliar rango de años a 2020-2024:

```bash
curl -X PUT http://localhost:5067/api/savedsearches/8d0f7680-8536-52ef-b825-557766551b18/criteria \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "searchCriteria": "{\"make\":\"Toyota\",\"bodyStyle\":\"SUV\",\"yearMin\":2020,\"yearMax\":2024,\"priceMax\":35000}"
  }'
```

### 5. Cambiar Frecuencia de Notificaciones

Cambiar de diario a semanal:

```bash
curl -X PUT http://localhost:5067/api/savedsearches/8d0f7680-8536-52ef-b825-557766551b18/notifications \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "sendEmailNotifications": true,
    "frequency": 2
  }'
```

## 🔄 Lógica de Negocio

### PriceAlert.ShouldTrigger()

```csharp
public bool ShouldTrigger(decimal currentPrice)
{
    if (!IsActive || IsTriggered)
        return false;

    return Condition switch
    {
        AlertCondition.LessThanOrEqual => currentPrice <= TargetPrice,
        AlertCondition.GreaterThanOrEqual => currentPrice >= TargetPrice,
        _ => false
    };
}
```

**Uso en Background Service:**

1. Obtener todas las alertas activas: `GetActiveAlertsAsync()`
2. Para cada alerta, obtener precio actual del vehículo (VehiclesSaleService)
3. Llamar `ShouldTrigger(currentPrice)`
4. Si retorna `true`, disparar notificación y llamar `alert.Trigger()`

### SavedSearch.ShouldSendNotification()

```csharp
public bool ShouldSendNotification()
{
    if (!IsActive || !SendEmailNotifications)
        return false;

    if (LastNotificationSent == null)
        return true; // Primera vez

    var timeSinceLastNotification = DateTime.UtcNow - LastNotificationSent.Value;

    return Frequency switch
    {
        NotificationFrequency.Instant => true,
        NotificationFrequency.Daily => timeSinceLastNotification.TotalHours >= 24,
        NotificationFrequency.Weekly => timeSinceLastNotification.TotalDays >= 7,
        _ => false
    };
}
```

**Uso en Background Service:**

1. Obtener búsquedas que necesitan notificación: `GetSearchesDueForNotificationAsync()`
2. Para cada búsqueda, ejecutar query contra VehiclesSaleService
3. Si hay resultados nuevos, enviar email
4. Llamar `search.MarkNotificationSent()`

## 🚀 Uso con Docker

### Desarrollo Local

```bash
# 1. Levantar PostgreSQL
docker-compose up -d postgres

# 2. Build y run
cd backend/AlertService
docker build -t alertservice:latest .
docker run -p 5067:8080 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=alertservice;Username=postgres;Password=postgres" \
  -e Jwt__Secret="YourSecretKey" \
  alertservice:latest

# 3. Verificar
curl http://localhost:5067/health
```

### Producción (Kubernetes)

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: alertservice
spec:
  replicas: 2
  template:
    spec:
      containers:
        - name: alertservice
          image: ghcr.io/gregorymorenoiem/cardealer-alertservice:latest
          ports:
            - containerPort: 8080
          env:
            - name: ConnectionStrings__DefaultConnection
              valueFrom:
                secretKeyRef:
                  name: database-secrets
                  key: alert-connection
```

## 🔐 Seguridad

- **Autenticación:** JWT Bearer Token obligatorio
- **Autorización:** Users solo pueden ver/modificar sus propias alertas y búsquedas
- **Validación:** Unique constraint en (UserId, VehicleId) para price alerts
- **Aislamiento:** Todos los queries filtran por UserId del JWT

## 📊 Métricas y Monitoreo

### Métricas Recomendadas

- Total de alertas activas por usuario
- Tasa de alertas disparadas por día
- Búsquedas guardadas más populares (criterios comunes)
- Frecuencia de notificaciones promedio

### Background Jobs Requeridos

1. **PriceAlertChecker** (Cada 5 minutos):

   - Obtener alertas activas
   - Verificar precios actuales
   - Disparar notificaciones

2. **SavedSearchNotifier** (Cada 1 hora):
   - Obtener búsquedas que necesitan notificación
   - Ejecutar queries
   - Enviar emails con resultados nuevos

## 🔗 Integración con Otros Servicios

### VehiclesSaleService

- Obtener precio actual de vehículos
- Ejecutar búsquedas con criterios guardados

### NotificationService

- Enviar emails cuando se dispara alerta de precio
- Enviar emails con resultados de búsquedas guardadas

### UserService

- Obtener preferencias de notificación del usuario
- Validar userId en JWT

## 🐛 Troubleshooting

### Error: "Ya existe una alerta para este vehículo"

- Solo se permite una alerta por vehículo por usuario
- Solución: Actualizar alerta existente o eliminarla primero

### Error: "Target price must be greater than zero"

- Validación: El precio objetivo debe ser positivo
- Solución: Enviar precio válido > 0

### Alerta no se dispara

1. Verificar que `IsActive = true`
2. Verificar que `IsTriggered = false`
3. Verificar lógica de `ShouldTrigger()` con precio actual

## 📈 Roadmap

- [ ] Background service para verificación automática de alertas
- [ ] Background service para notificaciones de búsquedas guardadas
- [ ] Dashboard de alertas disparadas (historial)
- [ ] Export de búsquedas guardadas
- [ ] Alertas de precio con rango (ej: entre $20K-$25K)
- [ ] Push notifications (mobile)

---

**Mantenido por:** Equipo OKLA  
**Última actualización:** Enero 2026  
**Sprint:** Sprint 2 - Experiencia de Usuario
