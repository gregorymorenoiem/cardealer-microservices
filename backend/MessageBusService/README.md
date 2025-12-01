# MessageBusService - RabbitMQ Message Bus with Dead Letter Queue

## 📋 Resumen

Message Bus Service es un servicio de mensajería empresarial basado en RabbitMQ que implementa el patrón Publisher/Subscriber con soporte completo para dead letter queue, reintentos y priorización de mensajes. Construido con Clean Architecture en ASP.NET Core 8.0.

## 🏗️ Arquitectura

### Clean Architecture - 4 Capas

```
MessageBusService/
├── MessageBusService.Domain/          # Entities + Enums (sin dependencias)
│   ├── Entities/
│   │   ├── Message.cs
│   │   ├── MessageBatch.cs
│   │   ├── Subscription.cs
│   │   └── DeadLetterMessage.cs
│   └── Enums/
│       ├── MessageStatus.cs
│       └── MessagePriority.cs
│
├── MessageBusService.Application/     # Lógica de negocio (CQRS con MediatR)
│   ├── Interfaces/
│   │   ├── IMessagePublisher.cs
│   │   ├── IMessageSubscriber.cs
│   │   └── IDeadLetterManager.cs
│   ├── Commands/
│   │   ├── PublishMessageCommand.cs
│   │   ├── SubscribeToTopicCommand.cs
│   │   └── RetryDeadLetterCommand.cs
│   └── Queries/
│       ├── GetMessageHistoryQuery.cs
│       └── GetDeadLettersQuery.cs
│
├── MessageBusService.Infrastructure/  # Implementación (RabbitMQ + PostgreSQL)
│   ├── Data/
│   │   └── MessageBusDbContext.cs
│   └── Services/
│       ├── RabbitMQPublisher.cs
│       ├── RabbitMQSubscriber.cs
│       └── DeadLetterManager.cs
│
├── MessageBusService.Api/             # REST API (Controllers)
│   ├── Controllers/
│   │   ├── MessagesController.cs
│   │   ├── SubscriptionsController.cs
│   │   └── DeadLetterController.cs
│   └── Program.cs
│
├── MessageBusService.Tests/           # Unit Tests (10 tests)
└── MessageBusService.IntegrationTests/ # API Tests (12 tests - requieren Docker)
```

## 🚀 Tecnologías

- **Framework**: ASP.NET Core 8.0
- **Message Broker**: RabbitMQ 6.8.1 (Fanout Exchange)
- **Base de Datos**: PostgreSQL 15.x (EF Core 8.0)
- **CQRS/Mediator**: MediatR 12.2.0
- **Testing**: xUnit 2.5.3 + Moq 4.20.70 + Testcontainers 3.10.0

## 📦 Paquetes NuGet

```xml
<!-- Infrastructure -->
<PackageReference Include="RabbitMQ.Client" Version="6.8.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />

<!-- Application -->
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.2" />

<!-- Testing -->
<PackageReference Include="xUnit" Version="2.5.3" />
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
<PackageReference Include="Testcontainers.RabbitMq" Version="3.10.0" />
<PackageReference Include="Testcontainers.PostgreSql" Version="3.10.0" />
```

## 🔌 Endpoints API

### **Messages Controller**

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `POST` | `/api/messages` | Publicar un mensaje |
| `POST` | `/api/messages/batch` | Publicar lote de mensajes |
| `GET` | `/api/messages/{messageId}` | Obtener estado de mensaje |

**Request Example**:
```json
POST /api/messages
{
  "topic": "order.created",
  "payload": "{\"orderId\": 12345, \"customer\": \"John Doe\"}",
  "priority": "Normal",
  "headers": {
    "correlationId": "uuid-1234",
    "source": "OrderService"
  }
}
```

### **Subscriptions Controller**

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `POST` | `/api/subscriptions` | Crear suscripción |
| `DELETE` | `/api/subscriptions/{id}` | Eliminar suscripción |
| `GET` | `/api/subscriptions?topic={topic}` | Listar suscripciones |

**Request Example**:
```json
POST /api/subscriptions
{
  "topic": "order.created",
  "consumerName": "NotificationService"
}
```

### **Dead Letter Controller**

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/deadletter?pageNumber=1&pageSize=10` | Listar mensajes fallidos |
| `POST` | `/api/deadletter/{messageId}/retry` | Reintentar mensaje |
| `DELETE` | `/api/deadletter/{messageId}` | Descartar mensaje |
| `GET` | `/api/deadletter/{messageId}` | Obtener detalles |

## ⚙️ Configuración

### **appsettings.json**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=25432;Database=messagebus_db;Username=admin;Password=admin123"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  }
}
```

### **Requisitos de Infraestructura**

1. **RabbitMQ**: Puerto 5672 (AMQP), 15672 (Management UI)
2. **PostgreSQL**: Puerto 25432

## 🐳 Despliegue con Docker

### **docker-compose.yml** (ejemplo)

```yaml
version: '3.8'

services:
  rabbitmq:
    image: rabbitmq:3.12-management
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest

  postgres:
    image: postgres:15-alpine
    ports:
      - "25432:5432"
    environment:
      POSTGRES_DB: messagebus_db
      POSTGRES_USER: admin
      POSTGRES_PASSWORD: admin123

  messagebus-api:
    build: .
    ports:
      - "5000:80"
    depends_on:
      - rabbitmq
      - postgres
    environment:
      ConnectionStrings__DefaultConnection: "Host=postgres;Port=5432;Database=messagebus_db;Username=admin;Password=admin123"
      RabbitMQ__Host: "rabbitmq"
```

## 🧪 Testing

### **Ejecutar Tests**

```powershell
# Todos los tests
dotnet test MessageBusService.sln

# Solo tests unitarios
dotnet test MessageBusService.Tests\MessageBusService.Tests.csproj

# Solo tests de API (requiere Docker)
dotnet test MessageBusService.IntegrationTests\MessageBusService.IntegrationTests.csproj
```

### **Resultados de Tests**

#### ✅ **Unit Tests (10/10 PASSED)**

```
Passed!  - Failed: 0, Passed: 10, Skipped: 0, Total: 10, Duration: 11 ms

Tests:
- PublishMessageCommandHandlerTests (2 tests)
- SubscribeToTopicCommandHandlerTests (2 tests)
- RetryDeadLetterCommandHandlerTests (2 tests)
- MessagesControllerTests (2 tests)
- DeadLetterControllerTests (2 tests)
```

#### 🐳 **API/Integration Tests (12 tests - requieren Docker)**

```
Failed: 12 (Docker no disponible)
- MessagesControllerApiTests (5 tests)
- SubscriptionsControllerApiTests (3 tests)
- DeadLetterControllerApiTests (4 tests)
```

**Nota**: Los tests de integración requieren Docker Desktop corriendo con:
- Testcontainers.RabbitMq (RabbitMQ 3.12-management)
- Testcontainers.PostgreSql (postgres:15-alpine)

## 📊 Modelo de Datos

### **Message** (Mensajes principales)
- **Id**: Guid (PK)
- **Topic**: string (indexed)
- **Payload**: string (JSON)
- **Status**: MessageStatus enum
- **Priority**: MessagePriority enum
- **CreatedAt**: DateTime (indexed)
- **ProcessedAt**: DateTime?
- **ExpiresAt**: DateTime?
- **RetryCount**: int
- **MaxRetries**: int
- **ErrorMessage**: string?
- **CorrelationId**: string?
- **Headers**: Dictionary<string, string>

### **MessageStatus** Enum
- `Pending = 0`
- `Processing = 1`
- `Completed = 2`
- `Failed = 3`
- `DeadLettered = 4`

### **MessagePriority** Enum
- `Low = 0`
- `Normal = 1`
- `High = 2`
- `Critical = 3`

### **Subscription** (Suscripciones activas)
- **Id**: Guid (PK)
- **Topic**: string (indexed)
- **ConsumerName**: string
- **QueueName**: string (unique)
- **IsActive**: bool
- **CreatedAt**: DateTime
- **LastActivityAt**: DateTime?
- **MessagesConsumed**: int

### **DeadLetterMessage** (Mensajes fallidos)
- **Id**: Guid (PK)
- **OriginalMessageId**: Guid
- **Topic**: string
- **Payload**: string
- **FailureReason**: string
- **RetryCount**: int
- **FailedAt**: DateTime (indexed)
- **RetriedAt**: DateTime?
- **IsDiscarded**: bool

## 🔧 Características Principales

### **1. Publisher/Subscriber Pattern**
- Fanout Exchange: todos los consumidores reciben todos los mensajes del topic
- Queues únicas: `{topic}.{consumerName}`
- Mensajes persistentes (durability = true)

### **2. Priorización de Mensajes**
- 4 niveles: Low, Normal, High, Critical
- RabbitMQ Priority Queue con prioridad 0-3
- Procesamiento FIFO dentro de cada nivel de prioridad

### **3. Dead Letter Queue**
- Captura de mensajes fallidos
- Retry manual con tracking de reintentos
- Opción de descarte permanente
- Historial completo de fallos

### **4. Persistencia**
- Todos los mensajes guardados en PostgreSQL
- Tracking de estado: Pending → Processing → Completed/Failed
- Historial de procesamiento con timestamps

### **5. Observability**
- Logging estructurado con ILogger<T>
- Correlation IDs para tracking distribuido
- Metadatos de mensajes (headers personalizados)

## 🏃 Cómo Ejecutar

### **1. Levantar Infraestructura**

```powershell
# Con Docker Compose
docker-compose up -d rabbitmq postgres

# O manualmente
# RabbitMQ
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.12-management

# PostgreSQL
docker run -d --name postgres -p 25432:5432 -e POSTGRES_DB=messagebus_db -e POSTGRES_USER=admin -e POSTGRES_PASSWORD=admin123 postgres:15-alpine
```

### **2. Aplicar Migraciones**

```powershell
cd MessageBusService.Infrastructure
dotnet ef database update --project ../MessageBusService.Api
```

### **3. Ejecutar API**

```powershell
cd MessageBusService.Api
dotnet run
```

La API estará disponible en: `https://localhost:5001` y `http://localhost:5000`

Swagger UI: `https://localhost:5001/swagger`

## 📈 Próximos Pasos (Roadmap)

- [ ] **Autenticación/Autorización**: JWT + OAuth2
- [ ] **Rate Limiting**: Throttling por consumer
- [ ] **Métricas**: Prometheus + Grafana dashboards
- [ ] **Distributed Tracing**: OpenTelemetry
- [ ] **Circuit Breaker**: Polly para resiliencia
- [ ] **Message TTL**: Expiración automática de mensajes antiguos
- [ ] **Retry Policies**: Reintentos automáticos configurables
- [ ] **Message Schemas**: Validación con JSON Schema
- [ ] **Batch Processing**: Procesamiento de lotes optimizado
- [ ] **Multi-Tenancy**: Soporte para múltiples tenants

## 📝 Notas de Desarrollo

### **Compilación**
```
✅ BUILD SUCCEEDED
- 0 Warning(s)
- 0 Error(s)
- Time: 00:00:03.32
```

### **Tests Unitarios**
```
✅ 10/10 PASSED
- Duration: 11 ms
- Coverage: Handlers + Controllers
```

### **Git Status**
```
✅ Committed: 102db9f
- 41 files, 1587 lines
- Branch: backup/before-refactor-20251128
- Remote: ✅ Pushed successfully
```

## 👥 Contribución

Este servicio forma parte de la arquitectura de microservicios de CarDealer. Ver roadmap completo en `ROADMAP_SERVICIOS_TRANSVERSALES.md`.

## 📄 Licencia

Propietario: CarDealer Microservices Architecture
