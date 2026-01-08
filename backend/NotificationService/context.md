# NotificationService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** NotificationService
- **Puerto en Kubernetes:** 8080
- **Puerto en Desarrollo:** 5006
- **Estado:** ✅ **EN PRODUCCIÓN**
- **Base de Datos:** PostgreSQL (`notificationservice`)
- **Imagen Docker:** ghcr.io/gregorymorenoiem/cardealer-notificationservice:latest

### Propósito
Servicio centralizado de notificaciones multi-canal: Email (SMTP, SendGrid), SMS (Twilio), Push Notifications (Firebase, OneSignal). Maneja templates, scheduling, retry logic y tracking de entregas.

---

## 🏗️ ARQUITECTURA

```
NotificationService/
├── NotificationService.Api/
│   ├── Controllers/
│   │   ├── NotificationsController.cs
│   │   ├── TemplatesController.cs
│   │   └── SubscriptionsController.cs
│   └── Program.cs
├── NotificationService.Application/
│   ├── Features/
│   │   ├── Commands/
│   │   │   ├── SendEmailCommand.cs
│   │   │   ├── SendSmsCommand.cs
│   │   │   └── SendPushCommand.cs
│   │   └── Queries/
│   │       └── GetNotificationStatusQuery.cs
│   └── DTOs/
├── NotificationService.Domain/
│   ├── Entities/
│   │   ├── Notification.cs
│   │   ├── NotificationTemplate.cs
│   │   └── NotificationSubscription.cs
│   ├── Enums/
│   │   ├── NotificationType.cs         # Email, SMS, Push
│   │   ├── NotificationStatus.cs       # Pending, Sent, Failed
│   │   └── TemplateName.cs
│   └── Interfaces/
│       ├── IEmailService.cs
│       ├── ISmsService.cs
│       └── IPushService.cs
└── NotificationService.Infrastructure/
    ├── Services/
    │   ├── SmtpEmailService.cs
    │   ├── SendGridEmailService.cs
    │   ├── TwilioSmsService.cs
    │   ├── FirebasePushService.cs
    │   └── TemplateEngine.cs           # Razor templates
    └── BackgroundServices/
        └── NotificationRetryWorker.cs
```

---

## 📦 ENTIDADES

### Notification
```csharp
public class Notification
{
    public Guid Id { get; set; }
    
    // Tipo y canal
    public NotificationType Type { get; set; }  // Email, SMS, Push
    public NotificationStatus Status { get; set; }
    
    // Destinatario
    public Guid? UserId { get; set; }
    public string RecipientEmail { get; set; }
    public string? RecipientPhone { get; set; }
    public string? RecipientDeviceToken { get; set; }
    
    // Contenido
    public string Subject { get; set; }
    public string Body { get; set; }
    public string? TemplateId { get; set; }
    public string? TemplateData { get; set; }   // JSON con variables
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public DateTime? SentAt { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Tracking
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsClicked { get; set; }
    public DateTime? ClickedAt { get; set; }
}
```

### NotificationTemplate
```csharp
public class NotificationTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; }            // "WelcomeEmail", "PasswordReset"
    public string DisplayName { get; set; }
    public NotificationType Type { get; set; }
    
    // Plantilla
    public string Subject { get; set; }         // Para emails
    public string BodyTemplate { get; set; }    // Razor template
    public string? SmsTemplate { get; set; }
    
    // Metadata
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### NotificationSubscription
```csharp
public class NotificationSubscription
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    // Preferencias de canal
    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }
    public bool PushEnabled { get; set; }
    
    // Categorías
    public bool MarketingEmails { get; set; }
    public bool TransactionalEmails { get; set; }
    public bool SecurityAlerts { get; set; }
    public bool VehicleUpdates { get; set; }
    
    // Dispositivos para push
    public string? FcmToken { get; set; }       // Firebase Cloud Messaging
    public string? ApnsToken { get; set; }      // Apple Push Notification
}
```

---

## 📡 ENDPOINTS API

### Envío de Notificaciones

#### POST `/api/notifications/email`
Enviar email.

**Request:**
```json
{
  "to": "usuario@ejemplo.com",
  "subject": "Bienvenido a OKLA",
  "body": "<h1>Hola</h1><p>Gracias por registrarte.</p>",
  "isHtml": true
}
```

**Response (202 Accepted):**
```json
{
  "notificationId": "...",
  "status": "Queued",
  "message": "Email en cola para envío"
}
```

#### POST `/api/notifications/email/template`
Enviar email usando template.

**Request:**
```json
{
  "to": "usuario@ejemplo.com",
  "templateName": "WelcomeEmail",
  "templateData": {
    "userName": "Juan Pérez",
    "activationLink": "https://okla.com.do/activate/..."
  }
}
```

#### POST `/api/notifications/sms`
Enviar SMS.

**Request:**
```json
{
  "to": "+18095551234",
  "message": "Tu código de verificación es: 123456"
}
```

#### POST `/api/notifications/push`
Enviar push notification.

**Request:**
```json
{
  "userId": "...",
  "title": "Nuevo mensaje",
  "body": "Tienes una consulta sobre tu vehículo",
  "data": {
    "vehicleId": "...",
    "type": "inquiry"
  }
}
```

### Gestión de Notificaciones

#### GET `/api/notifications/{id}`
Obtener estado de notificación.

**Response (200 OK):**
```json
{
  "id": "...",
  "type": "Email",
  "status": "Sent",
  "recipientEmail": "usuario@ejemplo.com",
  "subject": "Bienvenido a OKLA",
  "createdAt": "2026-01-07T10:00:00Z",
  "sentAt": "2026-01-07T10:00:15Z",
  "isRead": false
}
```

#### GET `/api/notifications/user/{userId}`
Obtener notificaciones de un usuario (inbox).

**Query Parameters:**
- `type`: Filtrar por tipo
- `status`: Filtrar por estado
- `page`: Paginación

**Response (200 OK):**
```json
{
  "notifications": [
    {
      "id": "...",
      "type": "Push",
      "subject": "Nuevo mensaje",
      "body": "...",
      "isRead": false,
      "createdAt": "..."
    }
  ],
  "unreadCount": 5,
  "totalCount": 25
}
```

#### PUT `/api/notifications/{id}/mark-read`
Marcar como leída.

### Templates

#### GET `/api/templates`
Listar templates disponibles.

#### POST `/api/templates`
Crear template (admin only).

**Request:**
```json
{
  "name": "VehiclePublished",
  "displayName": "Vehículo Publicado",
  "type": "Email",
  "subject": "Tu vehículo ha sido publicado",
  "bodyTemplate": "<h1>¡Felicidades @Model.UserName!</h1><p>Tu vehículo @Model.VehicleTitle ya está visible.</p>"
}
```

### Suscripciones

#### GET `/api/subscriptions/{userId}`
Obtener preferencias de notificación del usuario.

#### PUT `/api/subscriptions/{userId}`
Actualizar preferencias.

**Request:**
```json
{
  "emailEnabled": true,
  "smsEnabled": false,
  "pushEnabled": true,
  "marketingEmails": false,
  "vehicleUpdates": true
}
```

---

## 🔧 TECNOLOGÍAS Y DEPENDENCIAS

```xml
<!-- Email -->
<PackageReference Include="MailKit" Version="4.3.0" />
<PackageReference Include="SendGrid" Version="9.29.1" />

<!-- SMS -->
<PackageReference Include="Twilio" Version="6.16.1" />

<!-- Push Notifications -->
<PackageReference Include="FirebaseAdmin" Version="2.4.0" />

<!-- Template Engine -->
<PackageReference Include="RazorLight" Version="2.3.1" />

<!-- Base -->
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="RabbitMQ.Client" Version="6.8.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
```

### Servicios Externos
- **SMTP / SendGrid**: Envío de emails
- **Twilio**: Envío de SMS
- **Firebase Cloud Messaging (FCM)**: Push notifications Android/iOS
- **RabbitMQ**: Cola de notificaciones

---

## ⚙️ CONFIGURACIÓN

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=${DB_HOST};Database=notificationservice;..."
  },
  "Email": {
    "Provider": "SendGrid",
    "Smtp": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "Username": "${SMTP_USERNAME}",
      "Password": "${SMTP_PASSWORD}",
      "FromEmail": "noreply@okla.com.do",
      "FromName": "OKLA"
    },
    "SendGrid": {
      "ApiKey": "${SENDGRID_API_KEY}",
      "FromEmail": "noreply@okla.com.do"
    }
  },
  "Sms": {
    "Provider": "Twilio",
    "Twilio": {
      "AccountSid": "${TWILIO_ACCOUNT_SID}",
      "AuthToken": "${TWILIO_AUTH_TOKEN}",
      "PhoneNumber": "${TWILIO_PHONE_NUMBER}"
    }
  },
  "Push": {
    "Provider": "Firebase",
    "Firebase": {
      "ProjectId": "okla-app",
      "CredentialsPath": "/app/firebase-credentials.json"
    }
  },
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Port": 5672
  }
}
```

---

## 🔄 EVENTOS CONSUMIDOS

### UserRegisteredEvent (desde AuthService)
Envía email de bienvenida.

**Handler:** `SendWelcomeEmailHandler`

### VehiclePublishedEvent (desde VehiclesSaleService)
Notifica al vendedor que su vehículo fue publicado.

### PasswordResetRequestedEvent (desde AuthService)
Envía email con token de reset de contraseña.

---

## 📝 REGLAS DE NEGOCIO

### Retry Logic
1. **Email fallido**: Reintento automático 3 veces (15min, 1h, 4h)
2. **SMS fallido**: Reintento 2 veces (5min, 30min)
3. **Push fallido**: No retry (token inválido)

### Rate Limiting
- **Emails por usuario**: 100/día
- **SMS por usuario**: 10/día
- **Push por usuario**: Sin límite

### Unsubscribe
- **Marketing emails**: Link de unsubscribe obligatorio
- **Transactional emails**: No se puede desuscribir
- **Respeto a preferencias**: Verificar antes de enviar

---

## 📊 TEMPLATES PREDEFINIDOS

| Template | Tipo | Descripción |
|----------|------|-------------|
| **WelcomeEmail** | Email | Email de bienvenida al registrarse |
| **EmailVerification** | Email | Verificación de email |
| **PasswordReset** | Email | Reset de contraseña |
| **VehiclePublished** | Email | Vehículo publicado exitosamente |
| **VehicleSold** | Email/Push | Vehículo marcado como vendido |
| **NewInquiry** | Email/Push | Nueva consulta sobre vehículo |
| **PhoneVerification** | SMS | Código de verificación por SMS |
| **SecurityAlert** | Email/SMS | Alerta de seguridad |

---

## 🚀 DESPLIEGUE

### Kubernetes
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: notificationservice
  namespace: okla
spec:
  replicas: 2
  template:
    spec:
      containers:
      - name: notificationservice
        image: ghcr.io/gregorymorenoiem/cardealer-notificationservice:latest
        ports:
        - containerPort: 8080
        env:
        - name: Email__SendGrid__ApiKey
          valueFrom:
            secretKeyRef:
              name: notification-secrets
              key: sendgrid-api-key
        - name: Sms__Twilio__AuthToken
          valueFrom:
            secretKeyRef:
              name: notification-secrets
              key: twilio-auth-token
```

---

## 📅 ÚLTIMA ACTUALIZACIÓN

**Fecha:** Enero 7, 2026  
**Versión:** 1.0.0  
**Estado:** Producción en DOKS
