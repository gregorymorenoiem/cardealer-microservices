# MessageBusService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** MessageBusService
- **Puerto en Desarrollo:** 5011
- **Estado:** ⚠️ **SOLO DESARROLLO LOCAL**
- **Backend:** RabbitMQ
- **Base de Datos:** N/A
- **Imagen Docker:** Local only

### Propósito
Servicio de abstracción sobre RabbitMQ para gestión de mensajería. Proporciona API REST para publicar mensajes, suscribirse a eventos y gestionar colas. En producción, los servicios interactúan directamente con RabbitMQ.

---

## 🏗️ ARQUITECTURA

```
MessageBusService/
├── MessageBusService.Api/
│   ├── Controllers/
│   │   ├── PublishController.cs
│   │   ├── SubscriptionsController.cs
│   │   └── QueuesController.cs
│   └── Program.cs
├── MessageBusService.Application/
│   └── Services/
│       └── MessageBusService.cs
└── MessageBusService.Infrastructure/
    └── RabbitMQ/
        └── RabbitMqClient.cs
```

---

## 📡 ENDPOINTS API

#### POST `/api/messages/publish`
Publicar mensaje a exchange.

**Request:**
```json
{
  "exchange": "vehicle.events",
  "routingKey": "vehicle.created",
  "message": {
    "vehicleId": "123",
    "sellerId": "456",
    "createdAt": "2026-01-07T10:30:00Z"
  }
}
```

#### GET `/api/queues`
Listar colas activas.

**Response:**
```json
{
  "queues": [
    {
      "name": "notification-service-queue",
      "messages": 15,
      "consumers": 2
    }
  ]
}
```

#### POST `/api/queues/{queueName}/purge`
Limpiar cola (desarrollo/testing).

---

## 🔧 CONFIGURACIÓN

```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  }
}
```

---

## 📝 EXCHANGES Y ROUTING KEYS

### Exchanges Principales

| Exchange | Type | Description |
|----------|------|-------------|
| `auth.events` | topic | Eventos de autenticación |
| `user.events` | topic | Eventos de usuarios |
| `vehicle.events` | topic | Eventos de vehículos |
| `billing.events` | topic | Eventos de facturación |
| `notification.events` | topic | Eventos de notificaciones |

### Routing Keys Comunes

- `*.created` - Entidad creada
- `*.updated` - Entidad actualizada
- `*.deleted` - Entidad eliminada
- `*.published` - Entidad publicada

---

**Estado:** Solo desarrollo local - Servicios usan RabbitMQ directamente  
**Versión:** 1.0.0
