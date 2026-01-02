# 📧 SPRINT 6 - Notifications (Email, SMS, Push)

**Fecha:** 2 Enero 2026  
**Duración estimada:** 3-4 horas  
**Tokens estimados:** ~22,000  
**Prioridad:** 🟡 Media

---

## 🎯 OBJETIVOS

1. Integrar SendGrid para emails
2. Integrar Twilio para SMS
3. Integrar Firebase Cloud Messaging para push notifications
4. Crear sistema de templates de notificaciones
5. Implementar notificaciones en frontend
6. Crear preferencias de notificaciones de usuario
7. Implementar notificaciones en tiempo real

---

## 📋 CHECKLIST DE TAREAS

### Fase 1: Backend - Email con SendGrid (1 hora)

- [ ] 1.1. Configurar SendGrid en NotificationService
- [ ] 1.2. Crear templates de emails (welcome, vehicle_approved, etc.)
- [ ] 1.3. Implementar EmailService
- [ ] 1.4. Crear endpoints de notificaciones

### Fase 2: Backend - SMS con Twilio (45 min)

- [ ] 2.1. Configurar Twilio SDK
- [ ] 2.2. Crear SMSService
- [ ] 2.3. Implementar verificación por SMS
- [ ] 2.4. Agregar rate limiting para SMS

### Fase 3: Backend - Push con Firebase (1 hora)

- [ ] 3.1. Configurar Firebase Admin SDK
- [ ] 3.2. Crear PushNotificationService
- [ ] 3.3. Gestionar device tokens
- [ ] 3.4. Implementar notificaciones grupales

### Fase 4: Frontend - Notification Center (1 hora)

- [ ] 4.1. Crear NotificationCenter component
- [ ] 4.2. Implementar toast notifications
- [ ] 4.3. Crear página de preferencias
- [ ] 4.4. Agregar service worker para push

---

## 📝 IMPLEMENTACIÓN DETALLADA

### 1️⃣ Backend - Email Templates

**Archivo:** `backend/NotificationService/NotificationService.Application/Templates/EmailTemplates.cs`

```csharp
namespace NotificationService.Application.Templates;

public static class EmailTemplates
{
    public static class Welcome
    {
        public static string Subject => "¡Bienvenido a CarDealer!";
        
        public static string Body(string userName) => $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #2563eb; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; background: #f9fafb; }}
        .button {{ display: inline-block; padding: 12px 24px; background: #2563eb; 
                   color: white; text-decoration: none; border-radius: 6px; }}
        .footer {{ text-align: center; padding: 20px; color: #6b7280; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🚗 CarDealer</h1>
        </div>
        <div class='content'>
            <h2>¡Hola {userName}!</h2>
            <p>Gracias por registrarte en CarDealer, el marketplace líder para compra y venta de vehículos.</p>
            <p>Tu cuenta ha sido creada exitosamente. Ahora puedes:</p>
            <ul>
                <li>Publicar vehículos en venta</li>
                <li>Buscar el auto perfecto</li>
                <li>Contactar dealers verificados</li>
                <li>Guardar búsquedas y favoritos</li>
            </ul>
            <p style='text-align: center; margin: 30px 0;'>
                <a href='https://cardealer.app/dashboard' class='button'>Ir a mi Dashboard</a>
            </p>
        </div>
        <div class='footer'>
            <p>CarDealer Inc. | Todos los derechos reservados</p>
            <p>No responder a este email</p>
        </div>
    </div>
</body>
</html>";
    }

    public static class VehicleApproved
    {
        public static string Subject => "✅ Tu vehículo ha sido aprobado";
        
        public static string Body(string vehicleTitle, string vehicleUrl) => $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #10b981; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; background: #f9fafb; }}
        .button {{ display: inline-block; padding: 12px 24px; background: #10b981; 
                   color: white; text-decoration: none; border-radius: 6px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>¡Publicación Aprobada!</h1>
        </div>
        <div class='content'>
            <h2>Buenas noticias 🎉</h2>
            <p>Tu vehículo <strong>{vehicleTitle}</strong> ha sido aprobado y ahora está visible en CarDealer.</p>
            <p>Los compradores ya pueden ver tu publicación y contactarte.</p>
            <p style='text-align: center; margin: 30px 0;'>
                <a href='{vehicleUrl}' class='button'>Ver Mi Publicación</a>
            </p>
        </div>
    </div>
</body>
</html>";
    }

    public static class NewMessage
    {
        public static string Subject => "💬 Nuevo mensaje sobre tu vehículo";
        
        public static string Body(string vehicleTitle, string senderName, string message) => $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #8b5cf6; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; background: #f9fafb; }}
        .message-box {{ background: white; padding: 15px; border-left: 4px solid #8b5cf6; 
                        margin: 20px 0; }}
        .button {{ display: inline-block; padding: 12px 24px; background: #8b5cf6; 
                   color: white; text-decoration: none; border-radius: 6px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Nuevo Mensaje</h1>
        </div>
        <div class='content'>
            <p>Tienes un nuevo mensaje sobre <strong>{vehicleTitle}</strong></p>
            <p><strong>De:</strong> {senderName}</p>
            <div class='message-box'>
                <p>{message}</p>
            </div>
            <p style='text-align: center; margin: 30px 0;'>
                <a href='https://cardealer.app/messages' class='button'>Responder Mensaje</a>
            </p>
        </div>
    </div>
</body>
</html>";
    }
}
```

---

### 2️⃣ Backend - SendGrid Email Service

**Archivo:** `backend/NotificationService/NotificationService.Infrastructure/Services/SendGridEmailService.cs`

```csharp
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NotificationService.Infrastructure.Services;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string toEmail, string userName);
    Task SendVehicleApprovedEmailAsync(string toEmail, string vehicleTitle, string vehicleUrl);
    Task SendNewMessageEmailAsync(string toEmail, string vehicleTitle, string senderName, string message);
    Task SendCustomEmailAsync(string toEmail, string subject, string htmlContent);
}

public class SendGridEmailService : IEmailService
{
    private readonly ISendGridClient _client;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(IConfiguration configuration, ILogger<SendGridEmailService> logger)
    {
        _logger = logger;
        
        var apiKey = configuration["SendGrid:ApiKey"];
        _fromEmail = configuration["SendGrid:FromEmail"] ?? "noreply@cardealer.app";
        _fromName = configuration["SendGrid:FromName"] ?? "CarDealer";

        _client = new SendGridClient(apiKey);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName)
    {
        var subject = EmailTemplates.Welcome.Subject;
        var htmlContent = EmailTemplates.Welcome.Body(userName);
        
        await SendEmailAsync(toEmail, subject, htmlContent);
    }

    public async Task SendVehicleApprovedEmailAsync(string toEmail, string vehicleTitle, string vehicleUrl)
    {
        var subject = EmailTemplates.VehicleApproved.Subject;
        var htmlContent = EmailTemplates.VehicleApproved.Body(vehicleTitle, vehicleUrl);
        
        await SendEmailAsync(toEmail, subject, htmlContent);
    }

    public async Task SendNewMessageEmailAsync(
        string toEmail, 
        string vehicleTitle, 
        string senderName, 
        string message)
    {
        var subject = EmailTemplates.NewMessage.Subject;
        var htmlContent = EmailTemplates.NewMessage.Body(vehicleTitle, senderName, message);
        
        await SendEmailAsync(toEmail, subject, htmlContent);
    }

    public async Task SendCustomEmailAsync(string toEmail, string subject, string htmlContent)
    {
        await SendEmailAsync(toEmail, subject, htmlContent);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
    {
        try
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);

            var response = await _client.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            else
            {
                _logger.LogError(
                    "Failed to send email to {Email}. Status: {Status}", 
                    toEmail, 
                    response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", toEmail);
            throw;
        }
    }
}
```

**Instalar paquete NuGet:**

```xml
<PackageReference Include="SendGrid" Version="9.29.3" />
```

---

### 3️⃣ Backend - Twilio SMS Service

**Archivo:** `backend/NotificationService/NotificationService.Infrastructure/Services/TwilioSMSService.cs`

```csharp
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NotificationService.Infrastructure.Services;

public interface ISMSService
{
    Task SendVerificationCodeAsync(string phoneNumber, string code);
    Task SendVehicleAlertAsync(string phoneNumber, string vehicleTitle);
}

public class TwilioSMSService : ISMSService
{
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromPhoneNumber;
    private readonly ILogger<TwilioSMSService> _logger;

    public TwilioSMSService(IConfiguration configuration, ILogger<TwilioSMSService> logger)
    {
        _logger = logger;
        _accountSid = configuration["Twilio:AccountSid"] ?? "";
        _authToken = configuration["Twilio:AuthToken"] ?? "";
        _fromPhoneNumber = configuration["Twilio:PhoneNumber"] ?? "";

        TwilioClient.Init(_accountSid, _authToken);
    }

    public async Task SendVerificationCodeAsync(string phoneNumber, string code)
    {
        var message = $"Tu código de verificación CarDealer es: {code}. Válido por 10 minutos.";
        await SendSMSAsync(phoneNumber, message);
    }

    public async Task SendVehicleAlertAsync(string phoneNumber, string vehicleTitle)
    {
        var message = $"¡Nueva coincidencia! {vehicleTitle} está disponible. Ver en: cardealer.app";
        await SendSMSAsync(phoneNumber, message);
    }

    private async Task SendSMSAsync(string toPhoneNumber, string message)
    {
        try
        {
            var messageResource = await MessageResource.CreateAsync(
                to: new PhoneNumber(toPhoneNumber),
                from: new PhoneNumber(_fromPhoneNumber),
                body: message
            );

            _logger.LogInformation(
                "SMS sent successfully. SID: {Sid}, To: {To}", 
                messageResource.Sid, 
                toPhoneNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", toPhoneNumber);
            throw;
        }
    }
}
```

**Instalar paquete NuGet:**

```xml
<PackageReference Include="Twilio" Version="7.8.2" />
```

---

### 4️⃣ Backend - Firebase Push Notifications

**Archivo:** `backend/NotificationService/NotificationService.Infrastructure/Services/FirebasePushService.cs`

```csharp
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NotificationService.Infrastructure.Services;

public interface IPushNotificationService
{
    Task SendToDeviceAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null);
    Task SendToTopicAsync(string topic, string title, string body);
}

public class FirebasePushService : IPushNotificationService
{
    private readonly FirebaseMessaging _messaging;
    private readonly ILogger<FirebasePushService> _logger;

    public FirebasePushService(IConfiguration configuration, ILogger<FirebasePushService> logger)
    {
        _logger = logger;

        var credentialPath = configuration["Firebase:CredentialPath"];
        
        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(credentialPath)
            });
        }

        _messaging = FirebaseMessaging.DefaultInstance;
    }

    public async Task SendToDeviceAsync(
        string deviceToken, 
        string title, 
        string body, 
        Dictionary<string, string>? data = null)
    {
        try
        {
            var message = new Message
            {
                Token = deviceToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data,
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        Icon = "ic_notification",
                        Color = "#2563eb"
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Alert = new ApsAlert
                        {
                            Title = title,
                            Body = body
                        },
                        Sound = "default"
                    }
                }
            };

            var response = await _messaging.SendAsync(message);
            _logger.LogInformation("Push notification sent. Response: {Response}", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification to {Token}", deviceToken);
            throw;
        }
    }

    public async Task SendToTopicAsync(string topic, string title, string body)
    {
        try
        {
            var message = new Message
            {
                Topic = topic,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                }
            };

            var response = await _messaging.SendAsync(message);
            _logger.LogInformation("Topic notification sent. Topic: {Topic}, Response: {Response}", topic, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send topic notification to {Topic}", topic);
            throw;
        }
    }
}
```

**Instalar paquete NuGet:**

```xml
<PackageReference Include="FirebaseAdmin" Version="3.0.1" />
```

---

### 5️⃣ Frontend - Notification Service

**Archivo:** `frontend/web/original/src/services/notificationService.ts`

```typescript
import { api } from './api';

export interface NotificationPreferences {
  emailEnabled: boolean;
  smsEnabled: boolean;
  pushEnabled: boolean;
  newMessages: boolean;
  vehicleApproved: boolean;
  vehicleExpiring: boolean;
  searchAlerts: boolean;
}

export const notificationService = {
  /**
   * Get notification preferences
   */
  async getPreferences(): Promise<NotificationPreferences> {
    const response = await api.get<NotificationPreferences>('/notifications/preferences');
    return response.data;
  },

  /**
   * Update notification preferences
   */
  async updatePreferences(preferences: NotificationPreferences): Promise<void> {
    await api.put('/notifications/preferences', preferences);
  },

  /**
   * Register device token for push notifications
   */
  async registerDeviceToken(token: string): Promise<void> {
    await api.post('/notifications/device-token', { token });
  },

  /**
   * Request permission for push notifications
   */
  async requestPushPermission(): Promise<string | null> {
    if (!('Notification' in window)) {
      console.warn('This browser does not support notifications');
      return null;
    }

    if (Notification.permission === 'granted') {
      return await this.getDeviceToken();
    }

    if (Notification.permission !== 'denied') {
      const permission = await Notification.requestPermission();
      if (permission === 'granted') {
        return await this.getDeviceToken();
      }
    }

    return null;
  },

  /**
   * Get Firebase device token
   */
  async getDeviceToken(): Promise<string | null> {
    try {
      const { getMessaging, getToken } = await import('firebase/messaging');
      const messaging = getMessaging();
      
      const token = await getToken(messaging, {
        vapidKey: import.meta.env.VITE_FIREBASE_VAPID_KEY
      });

      return token;
    } catch (error) {
      console.error('Error getting device token:', error);
      return null;
    }
  },

  /**
   * Subscribe to topic
   */
  async subscribeToTopic(topic: string): Promise<void> {
    await api.post('/notifications/subscribe', { topic });
  },
};
```

---

### 6️⃣ Frontend - Notification Center Component

**Archivo:** `frontend/web/original/src/components/NotificationCenter.tsx`

```typescript
import { useState, useEffect, type FC } from 'react';
import { Bell, X, Check, Mail, MessageSquare, Car } from 'lucide-react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/services/api';
import toast from 'react-hot-toast';

interface Notification {
  id: string;
  type: 'message' | 'vehicle' | 'system';
  title: string;
  message: string;
  read: boolean;
  createdAt: string;
  url?: string;
}

export const NotificationCenter: FC = () => {
  const [isOpen, setIsOpen] = useState(false);
  const queryClient = useQueryClient();

  const { data: notifications = [] } = useQuery({
    queryKey: ['notifications'],
    queryFn: async () => {
      const response = await api.get<Notification[]>('/notifications');
      return response.data;
    },
    refetchInterval: 30000, // Poll every 30s
  });

  const markAsReadMutation = useMutation({
    mutationFn: (id: string) => api.put(`/notifications/${id}/read`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
    },
  });

  const markAllAsReadMutation = useMutation({
    mutationFn: () => api.put('/notifications/read-all'),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
      toast.success('Todas las notificaciones marcadas como leídas');
    },
  });

  const unreadCount = notifications.filter(n => !n.read).length;

  const getIcon = (type: Notification['type']) => {
    switch (type) {
      case 'message':
        return <MessageSquare className="w-5 h-5 text-blue-600" />;
      case 'vehicle':
        return <Car className="w-5 h-5 text-green-600" />;
      case 'system':
        return <Mail className="w-5 h-5 text-purple-600" />;
    }
  };

  return (
    <div className="relative">
      {/* Bell Icon */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="relative p-2 text-gray-600 hover:text-gray-900 transition-colors"
      >
        <Bell className="w-6 h-6" />
        {unreadCount > 0 && (
          <span className="absolute top-0 right-0 w-5 h-5 bg-red-600 text-white text-xs 
                         rounded-full flex items-center justify-center">
            {unreadCount > 9 ? '9+' : unreadCount}
          </span>
        )}
      </button>

      {/* Dropdown */}
      {isOpen && (
        <>
          <div
            className="fixed inset-0 z-40"
            onClick={() => setIsOpen(false)}
          />
          
          <div className="absolute right-0 mt-2 w-96 bg-white rounded-lg shadow-xl 
                        border border-gray-200 z-50 max-h-[600px] flex flex-col">
            {/* Header */}
            <div className="px-4 py-3 border-b border-gray-200 flex items-center justify-between">
              <h3 className="text-lg font-semibold text-gray-900">
                Notificaciones
              </h3>
              {unreadCount > 0 && (
                <button
                  onClick={() => markAllAsReadMutation.mutate()}
                  className="text-sm text-blue-600 hover:text-blue-700 flex items-center gap-1"
                >
                  <Check className="w-4 h-4" />
                  Marcar todas
                </button>
              )}
            </div>

            {/* Notifications List */}
            <div className="flex-1 overflow-y-auto">
              {notifications.length === 0 ? (
                <div className="px-4 py-12 text-center text-gray-500">
                  <Bell className="w-12 h-12 mx-auto mb-3 text-gray-400" />
                  <p>No tienes notificaciones</p>
                </div>
              ) : (
                <div className="divide-y divide-gray-100">
                  {notifications.map((notification) => (
                    <div
                      key={notification.id}
                      className={`px-4 py-3 hover:bg-gray-50 cursor-pointer transition-colors
                        ${!notification.read ? 'bg-blue-50' : ''}`}
                      onClick={() => {
                        markAsReadMutation.mutate(notification.id);
                        if (notification.url) {
                          window.location.href = notification.url;
                        }
                      }}
                    >
                      <div className="flex items-start gap-3">
                        <div className="mt-1">{getIcon(notification.type)}</div>
                        <div className="flex-1 min-w-0">
                          <p className="text-sm font-medium text-gray-900">
                            {notification.title}
                          </p>
                          <p className="text-sm text-gray-600 mt-1">
                            {notification.message}
                          </p>
                          <p className="text-xs text-gray-500 mt-1">
                            {new Date(notification.createdAt).toLocaleDateString('es-MX', {
                              hour: '2-digit',
                              minute: '2-digit'
                            })}
                          </p>
                        </div>
                        {!notification.read && (
                          <div className="w-2 h-2 bg-blue-600 rounded-full mt-2" />
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </>
      )}
    </div>
  );
};
```

---

## ✅ CRITERIOS DE ACEPTACIÓN

### Test Backend

```bash
# Test email
curl -X POST http://localhost:15084/api/notifications/email/send \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "to": "test@example.com",
    "subject": "Test",
    "body": "Test email"
  }'

# Test SMS
curl -X POST http://localhost:15084/api/notifications/sms/send \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+1234567890",
    "message": "Test SMS"
  }'
```

### Test Frontend

1. Login como usuario
2. Hacer alguna acción que genere notificación
3. Ver badge con conteo en bell icon
4. Click en bell → Ver dropdown con notificaciones
5. Click en notificación → Marcar como leída
6. Click "Marcar todas" → Todas se marcan

---

## 📊 ESTIMACIÓN DE TOKENS

| Tarea | Tokens |
|-------|--------|
| Email templates | 3,000 |
| SendGrid service | 4,000 |
| Twilio SMS service | 3,000 |
| Firebase push service | 4,000 |
| Frontend notification service | 3,000 |
| NotificationCenter component | 4,000 |
| Testing | 1,000 |
| **TOTAL** | **~22,000** |

**Con buffer 15%:** ~25,000 tokens

---

## ➡️ PRÓXIMO SPRINT

**Sprint 7:** Messaging & CRM

---

**Estado:** ⚪ Pendiente  
**Última actualización:** 2 Enero 2026
