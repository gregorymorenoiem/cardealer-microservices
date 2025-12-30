# 📧 NotificationService

Microservicio de notificaciones multi-canal (Email, SMS, Push) para CarDealer.

## 🚀 Quick Start

### Desarrollo Local

```bash
cd backend/NotificationService/NotificationService.Api
dotnet run
```

### Docker

```bash
docker build -t notificationservice -f NotificationService/Dockerfile .
docker run -p 5000:80 \
  -e DATABASE_CONNECTION_STRING="Host=db;Database=notifications;..." \
  -e SENDGRID_API_KEY="your-api-key" \
  notificationservice
```

## 🔐 Variables de Entorno Requeridas

### Base de Datos (REQUERIDO)

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `DATABASE_CONNECTION_STRING` | Connection string PostgreSQL completa | `Host=db;Port=5432;Database=notifications;Username=app;Password=secret` |

**O usar variables individuales:**

| Variable | Descripción |
|----------|-------------|
| `DATABASE_HOST` | Host de PostgreSQL |
| `DATABASE_PORT` | Puerto (default: 5432) |
| `DATABASE_NAME` | Nombre de la base de datos |
| `DATABASE_USER` | Usuario |
| `DATABASE_PASSWORD` | Contraseña |

### RabbitMQ (REQUERIDO para message bus)

| Variable | Descripción | Default |
|----------|-------------|---------|
| `RABBITMQ_HOST` | Host de RabbitMQ | `localhost` |
| `RABBITMQ_PORT` | Puerto | `5672` |
| `RABBITMQ_USER` | Usuario | `guest` |
| `RABBITMQ_PASSWORD` | Contraseña | `guest` |

### Email - SendGrid (OPCIONAL)

| Variable | Descripción |
|----------|-------------|
| `SENDGRID_API_KEY` | API Key de SendGrid |
| `SENDGRID_FROM_EMAIL` | Email del remitente |
| `SENDGRID_FROM_NAME` | Nombre del remitente |

> ⚠️ Si no se configura, las notificaciones de email se deshabilitarán automáticamente.

### SMS - Twilio (OPCIONAL)

| Variable | Descripción |
|----------|-------------|
| `TWILIO_ACCOUNT_SID` | Account SID de Twilio |
| `TWILIO_AUTH_TOKEN` | Auth Token |
| `TWILIO_FROM_NUMBER` | Número de teléfono origen |

> ⚠️ Si no se configura, las notificaciones SMS se deshabilitarán automáticamente.

### Push Notifications - Firebase (OPCIONAL)

| Variable | Descripción |
|----------|-------------|
| `FIREBASE_PROJECT_ID` | ID del proyecto Firebase |
| `FIREBASE_SERVICE_ACCOUNT_JSON` | JSON del service account (base64) |

**O credenciales individuales:**

| Variable | Descripción |
|----------|-------------|
| `FIREBASE_PROJECT_ID` | ID del proyecto |
| `FIREBASE_PRIVATE_KEY` | Private key (base64) |
| `FIREBASE_CLIENT_EMAIL` | Email del service account |

> ⚠️ Si no se configura, las push notifications se deshabilitarán automáticamente.

## 🐳 Docker Compose Example

```yaml
services:
  notificationservice:
    build:
      context: ./backend
      dockerfile: NotificationService/Dockerfile
    environment:
      # Database
      - DATABASE_CONNECTION_STRING=Host=postgres;Port=5432;Database=notifications;Username=app;Password=${DB_PASSWORD}
      
      # RabbitMQ
      - RABBITMQ_HOST=rabbitmq
      - RABBITMQ_USER=app
      - RABBITMQ_PASSWORD=${RABBITMQ_PASSWORD}
      
      # SendGrid (optional)
      - SENDGRID_API_KEY=${SENDGRID_API_KEY}
      - SENDGRID_FROM_EMAIL=noreply@example.com
      - SENDGRID_FROM_NAME=CarDealer
      
      # Twilio (optional)
      - TWILIO_ACCOUNT_SID=${TWILIO_ACCOUNT_SID}
      - TWILIO_AUTH_TOKEN=${TWILIO_AUTH_TOKEN}
      - TWILIO_FROM_NUMBER=${TWILIO_FROM_NUMBER}
      
      # Firebase (optional)
      - FIREBASE_PROJECT_ID=${FIREBASE_PROJECT_ID}
      - FIREBASE_SERVICE_ACCOUNT_JSON=${FIREBASE_SERVICE_ACCOUNT_JSON}
    
    # Docker Secrets (alternative)
    secrets:
      - db_password
      - sendgrid_api_key
      - twilio_auth_token
      - firebase_service_account

secrets:
  db_password:
    file: ./secrets/db_password.txt
  sendgrid_api_key:
    file: ./secrets/sendgrid_api_key.txt
  twilio_auth_token:
    file: ./secrets/twilio_auth_token.txt
  firebase_service_account:
    file: ./secrets/firebase_service_account.json
```

## 📊 Health Check

```bash
curl http://localhost:5000/health
```

Response:
```json
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "rabbitmq": "Healthy",
    "email": "Healthy",
    "sms": "Degraded",  // Si Twilio no está configurado
    "push": "Degraded"  // Si Firebase no está configurado
  }
}
```

## 🔧 Graceful Degradation

Este servicio soporta **graceful degradation**:

- Si SendGrid no está configurado → Email notifications deshabilitadas
- Si Twilio no está configurado → SMS notifications deshabilitadas
- Si Firebase no está configurado → Push notifications deshabilitadas

El servicio seguirá funcionando con los canales disponibles.

## 📁 Estructura

```
NotificationService/
├── NotificationService.Api/          # API REST
├── NotificationService.Application/  # Business logic (CQRS)
├── NotificationService.Domain/       # Entities, Interfaces
├── NotificationService.Infrastructure/
│   ├── Configuration/               # Secret providers
│   ├── External/                    # SendGrid, Twilio, Firebase
│   ├── MessageBus/                  # RabbitMQ
│   ├── Persistence/                 # EF Core
│   └── Services/                    # Templates, Queue
├── NotificationService.Shared/       # DTOs, Settings
└── NotificationService.Tests/        # Unit tests
```

## 🔐 Security Notes

1. **NUNCA** commit secretos en appsettings.json
2. Usar variables de entorno o Docker secrets en producción
3. Los secretos hardcoded en dev son solo para desarrollo local
4. El archivo `appsettings.Docker.json` no contiene secretos
