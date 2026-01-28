# ChatbotService - OKLA Marketplace

## Descripción

Microservicio de chatbot con integración a Dialogflow ES para ventas de vehículos. Incluye:

- 🤖 **Chatbot entrenado para ventas** de vehículos
- 📊 **Control de interacciones** (por sesión, usuario, día, mes)
- 🔧 **Mantenimiento automatizado** para reducir costos operativos
- 🎯 **Generación automática de leads** con scoring
- 📈 **Reportes y analytics** detallados

## Características Principales

### 1. Integración Dialogflow ES

- Detección de intenciones en español
- 17 categorías de intenciones para ventas automotrices
- Análisis de sentimiento
- Contextos de conversación

### 2. Control de Interacciones (Costos)

| Límite                          | Default | Descripción                     |
| ------------------------------- | ------- | ------------------------------- |
| `MaxInteractionsPerSession`     | 10      | Límite por sesión de chat       |
| `MaxInteractionsPerUserPerDay`  | 50      | Límite diario por usuario       |
| `MaxGlobalInteractionsPerMonth` | 100,000 | Límite mensual global           |
| `FreeInteractionsPerMonth`      | 180     | Interacciones gratis Dialogflow |
| `CostPerInteraction`            | $0.002  | Costo por interacción pagada    |

### 3. Quick Responses (Bypass Dialogflow)

Respuestas rápidas que no consumen interacciones de Dialogflow:

- Saludos básicos
- Horarios de atención
- Información de contacto
- FAQ comunes

### 4. Mantenimiento Automatizado

| Tarea            | Cron           | Descripción                     |
| ---------------- | -------------- | ------------------------------- |
| `InventorySync`  | `0 */4 * * *`  | Sincronizar vehículos cada 4h   |
| `HealthCheck`    | `*/15 * * * *` | Verificar salud cada 15 min     |
| `DailyReport`    | `0 6 * * *`    | Reporte diario a las 6 AM       |
| `AutoLearning`   | `0 2 * * 0`    | Análisis de preguntas semanales |
| `SessionCleanup` | `0 3 * * *`    | Limpiar sesiones inactivas      |

## Arquitectura

```
ChatbotService/
├── ChatbotService.Domain/          # Entidades, Enums, Interfaces
├── ChatbotService.Application/     # DTOs, Commands, Queries, Handlers
├── ChatbotService.Infrastructure/  # DbContext, Repositories, Dialogflow
├── ChatbotService.Api/             # Controllers, Program.cs
└── Dockerfile
```

## API Endpoints

### Chat (Público)

```http
POST /api/chat/start          # Iniciar sesión de chat
POST /api/chat/message        # Enviar mensaje
POST /api/chat/end            # Terminar sesión
POST /api/chat/transfer       # Transferir a agente
GET  /api/chat/session/{id}   # Obtener sesión
GET  /api/chat/usage/{configId} # Ver uso de interacciones
```

### Configuration (Admin)

```http
GET  /api/configuration                    # Listar configuraciones
GET  /api/configuration/{id}               # Obtener configuración
POST /api/configuration                    # Crear/actualizar config
GET  /api/configuration/{id}/quick-responses  # Listar quick responses
POST /api/configuration/{id}/quick-responses  # Crear quick response
GET  /api/configuration/{id}/vehicles         # Listar vehículos sync
```

### Maintenance (Admin)

```http
GET  /api/maintenance/tasks/{configId}    # Listar tareas
POST /api/maintenance/tasks/{taskId}/run  # Ejecutar tarea manual
GET  /api/maintenance/health/{configId}   # Health report
GET  /api/maintenance/alerts/{configId}   # Alertas activas
GET  /api/maintenance/reports/daily/{id}  # Reporte diario
GET  /api/maintenance/reports/monthly/{id} # Reporte mensual
GET  /api/maintenance/reports/costs/{id}  # Análisis de costos
GET  /api/maintenance/learning/unanswered/{id} # Preguntas sin respuesta
POST /api/maintenance/learning/analyze/{id}    # Analizar patrones
```

### Leads (Admin/Dealer)

```http
GET   /api/leads              # Listar leads
GET   /api/leads/hot          # Leads calientes
GET   /api/leads/{id}         # Detalle de lead
PATCH /api/leads/{id}/status  # Actualizar estado
PATCH /api/leads/{id}/assign  # Asignar a vendedor
GET   /api/leads/stats        # Estadísticas
```

## Configuración

### Variables de Entorno

```bash
# Base de datos
ConnectionStrings__DefaultConnection=Host=postgres;Database=chatbotservice;Username=postgres;Password=postgres

# JWT
Jwt__Key=your-secret-key
Jwt__Issuer=okla.com.do
Jwt__Audience=okla.com.do

# Dialogflow
Dialogflow__ProjectId=your-project-id
Dialogflow__CredentialsPath=/path/to/credentials.json
Dialogflow__LanguageCode=es

# Servicios externos
Services__VehiclesSaleService__Url=http://vehiclessaleservice:8080
```

### Docker

```bash
# Build
docker build -t chatbotservice:latest .

# Run
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="..." \
  chatbotservice:latest
```

## Costos Estimados (Dialogflow ES)

### Plan Standard

- **Gratis:** 180 interacciones/mes
- **Costo adicional:** $0.002/interacción

### Proyección Mensual

| Interacciones | Costo Dialogflow | Costo Total |
| ------------- | ---------------- | ----------- |
| 180           | $0.00            | $0.00       |
| 1,000         | $1.64            | ~$2.00      |
| 2,500         | $4.64            | ~$5.00      |
| 10,000        | $19.64           | ~$20.00     |

### Ahorro con Quick Responses

Las Quick Responses bypasean Dialogflow, reduciendo costos:

- Saludos: ~20% del tráfico
- FAQ: ~15% del tráfico
- **Ahorro estimado: 30-40%**

## Desarrollo

### Requisitos

- .NET 8.0 SDK
- PostgreSQL 16+
- Redis
- Cuenta de Dialogflow ES

### Setup Local

```bash
# Restaurar paquetes
dotnet restore

# Aplicar migraciones
dotnet ef database update --project ChatbotService.Infrastructure

# Ejecutar
dotnet run --project ChatbotService.Api
```

### Testing

```bash
dotnet test
```

## Monitoreo

### Health Check

```bash
curl http://localhost:8080/health
```

### Métricas

El servicio expone métricas en:

- Logs estructurados (Serilog)
- Health checks (PostgreSQL, Redis)
- Reportes automatizados

## Licencia

© 2026 OKLA - Todos los derechos reservados
