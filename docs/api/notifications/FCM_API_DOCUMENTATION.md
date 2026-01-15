# 🔥 Firebase Cloud Messaging (FCM) - Documentación Técnica

**API Provider:** Google Firebase  
**Versión:** v1  
**Tipo:** Push Notification Service  
**Status en OKLA:** 📝 Planificado (Q3 2026)  
**Última actualización:** Enero 15, 2026

---

## 📋 Descripción General

**Firebase Cloud Messaging (FCM)** es el servicio de push notifications para:

- Push notifications en app móvil (Flutter)
- Alertas en tiempo real
- Mensajes de chat in-app
- Updates de vehículos
- Recordatorios de ofertas

**¿Por qué Firebase FCM?**

- ✅ **Free tier gratuito** (sin límites)
- ✅ **Integración nativa** con Flutter
- ✅ **Targeting avanzado** (por user, device, topic)
- ✅ **Analytics integrado**
- ✅ **Multi-platform** (iOS, Android, Web)
- ✅ **Offline support** (queue automático)

---

## 🔑 Autenticación

### Crear Proyecto en Firebase Console

1. Ir a [Firebase Console](https://console.firebase.google.com/)
2. Crear nuevo proyecto o usar existente
3. Ir a **Project Settings** → **Service Accounts**
4. Generar nueva clave privada (JSON)

### Firebase Service Account Key

```json
{
  "type": "service_account",
  "project_id": "okla-marketplace",
  "private_key_id": "key-id-here",
  "private_key": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----\n",
  "client_email": "firebase-adminsdk-xxxxx@okla-marketplace.iam.gserviceaccount.com",
  "client_id": "123456789",
  "auth_uri": "https://accounts.google.com/o/oauth2/auth",
  "token_uri": "https://oauth2.googleapis.com/token",
  "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs"
}
```

### En appsettings.json

```json
{
  "Firebase": {
    "ServiceAccountPath": "/path/to/serviceAccountKey.json",
    "DatabaseUrl": "https://okla-marketplace.firebaseio.com"
  }
}
```

---

## 🔌 Endpoints Principales

### Enviar Mensaje a un Device

```
POST https://fcm.googleapis.com/v1/projects/{project_id}/messages:send
```

**Headers:**

```
Authorization: Bearer {ID_TOKEN}
Content-Type: application/json
```

**Body:**

```json
{
  "message": {
    "token": "device_token_here",
    "notification": {
      "title": "Nuevo vehículo similar encontrado",
      "body": "Toyota Corolla 2020 - $15,000"
    },
    "data": {
      "vehicleId": "123",
      "vehicleUrl": "https://okla.com.do/vehicles/123",
      "price": "15000"
    },
    "android": {
      "priority": "high",
      "notification": {
        "click_action": "FLUTTER_NOTIFICATION_CLICK"
      }
    },
    "apns": {
      "headers": {
        "apns-priority": "10"
      },
      "payload": {
        "aps": {
          "alert": {
            "title": "Nuevo vehículo",
            "body": "Toyota Corolla 2020"
          },
          "sound": "default",
          "badge": 1
        }
      }
    }
  }
}
```

**Response (200 OK):**

```json
{
  "name": "projects/okla-marketplace/messages/1234567890"
}
```

### Enviar a Múltiples Devices (Multicast)

```
POST https://fcm.googleapis.com/v1/projects/{project_id}/messages:send
```

**Body:** (Array de tokens)

```json
{
  "message": {
    "tokens": ["token1", "token2", "token3"],
    "notification": { ... },
    "data": { ... }
  }
}
```

### Suscribir a Topic

```
POST https://iid.googleapis.com/iid/v1/{device_token}/rel/topics/{topic}
```

**Headers:**

```
Authorization: Bearer {ID_TOKEN}
```

---

## 💻 Implementación en C#/.NET

### Instalación del paquete

```bash
dotnet add package FirebaseAdmin
```

### FirebasePushNotificationService.cs

```csharp
using Firebase.Auth;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;

namespace NotificationService.Infrastructure.Services;

public class FirebasePushNotificationService : IPushNotificationService
{
    private readonly FirebaseMessaging _firebaseMessaging;
    private readonly ILogger<FirebasePushNotificationService> _logger;

    public FirebasePushNotificationService(
        string serviceAccountPath,
        ILogger<FirebasePushNotificationService> logger)
    {
        // Inicializar Firebase
        if (FirebaseApp.DefaultInstance == null)
        {
            var options = new AppOptions
            {
                Credential = GoogleCredential.FromFile(serviceAccountPath)
            };
            FirebaseApp.Create(options);
        }

        _firebaseMessaging = FirebaseMessaging.DefaultInstance;
        _logger = logger;
    }

    // ✅ Enviar push a un device
    public async Task<Result<string>> SendPushNotificationAsync(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string> data = null,
        CancellationToken ct = default)
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
                Data = data ?? new Dictionary<string, string>(),
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        ClickAction = "FLUTTER_NOTIFICATION_CLICK"
                    }
                },
                Apns = new ApnsConfig
                {
                    Headers = new Dictionary<string, string>
                    {
                        { "apns-priority", "10" }
                    }
                }
            };

            var messageId = await _firebaseMessaging.SendAsync(message);

            _logger.LogInformation($"Push notification sent to {deviceToken}. MessageId: {messageId}");
            return Result<string>.Success(messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending push notification");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    // ✅ Enviar a múltiples devices
    public async Task<Result<MulticastResult>> SendMulticastPushAsync(
        List<string> deviceTokens,
        string title,
        string body,
        Dictionary<string, string> data = null,
        CancellationToken ct = default)
    {
        try
        {
            var message = new MulticastMessage
            {
                Tokens = deviceTokens,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data ?? new Dictionary<string, string>(),
                Android = new AndroidConfig
                {
                    Priority = Priority.High
                }
            };

            var result = await _firebaseMessaging.SendMulticastAsync(message);

            _logger.LogInformation($"Multicast sent to {deviceTokens.Count} devices. " +
                $"Success: {result.SuccessCount}, Failed: {result.FailureCount}");

            return Result<MulticastResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending multicast");
            return Result<MulticastResult>.Failure($"Error: {ex.Message}");
        }
    }

    // ✅ Suscribir device a topic
    public async Task<Result<bool>> SubscribeToTopicAsync(
        List<string> deviceTokens,
        string topic,
        CancellationToken ct = default)
    {
        try
        {
            await _firebaseMessaging.SubscribeToTopicAsync(deviceTokens, topic);

            _logger.LogInformation($"Subscribed {deviceTokens.Count} devices to topic: {topic}");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception subscribing to topic");
            return Result<bool>.Failure($"Error: {ex.Message}");
        }
    }

    // ✅ Enviar a topic
    public async Task<Result<string>> SendToTopicAsync(
        string topic,
        string title,
        string body,
        Dictionary<string, string> data = null,
        CancellationToken ct = default)
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
                },
                Data = data ?? new Dictionary<string, string>()
            };

            var messageId = await _firebaseMessaging.SendAsync(message);

            _logger.LogInformation($"Message sent to topic {topic}. MessageId: {messageId}");
            return Result<string>.Success(messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending to topic");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    // ✅ Obtener información del mensaje
    public async Task<Result<Message>> GetMessageAsync(
        string messageId,
        CancellationToken ct = default)
    {
        try
        {
            // FCM no proporciona endpoint para obtener estado de mensajes
            // Se debe usar webhooks para tracking
            _logger.LogWarning("Use webhooks for message status tracking, not direct API");
            return Result<Message>.Failure("Use webhooks instead");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception getting message");
            return Result<Message>.Failure($"Error: {ex.Message}");
        }
    }
}
```

---

## 🎯 Casos de Uso en OKLA

### 1. Nuevo Vehículo Similar Encontrado

```csharp
var buyers = await _userService.GetBuyersWithAlertAsync(criteria);

foreach (var buyer in buyers)
{
    var data = new Dictionary<string, string>
    {
        { "vehicleId", vehicle.Id.ToString() },
        { "vehicleUrl", $"https://okla.com.do/vehicles/{vehicle.Id}" },
        { "price", vehicle.Price.ToString() }
    };

    await _pushService.SendPushNotificationAsync(
        buyer.DeviceToken,
        "Nuevo vehículo encontrado",
        $"{vehicle.Title} - ${vehicle.Price}",
        data);
}
```

### 2. Alerta de Nueva Consulta (Dealer)

```csharp
var dealer = await _dealerService.GetAsync(vehicleListingId);

await _pushService.SendPushNotificationAsync(
    dealer.DeviceToken,
    "Nueva consulta",
    $"{buyer.Name} está interesado en {vehicle.Title}",
    new Dictionary<string, string>
    {
        { "inquiryId", inquiry.Id.ToString() },
        { "buyerName", buyer.Name }
    });
```

### 3. Broadcast a Todos los Dealers (Topic)

```csharp
// Suscribir todos los dealers a topic "dealer-news"
await _pushService.SubscribeToTopicAsync(dealerTokens, "dealer-news");

// Enviar a todos
await _pushService.SendToTopicAsync(
    "dealer-news",
    "Mantenimiento programado",
    "Mantenimiento el 15/02 de 23:00 a 01:00 UTC");
```

### 4. Recordatorio de Test Drive

```csharp
var reminder = await _testDriveService.GetAsync(testDriveId);

await _pushService.SendPushNotificationAsync(
    buyer.DeviceToken,
    "Recordatorio: Test Drive",
    $"{reminder.DealerName} - {reminder.DateTime:g}",
    new Dictionary<string, string>
    {
        { "testDriveId", testDriveId.ToString() }
    });
```

---

## 🔄 Flutter Integration

### pubspec.yaml

```yaml
dependencies:
  firebase_messaging: ^14.0.0
  flutter_local_notifications: ^14.0.0
```

### main.dart

```dart
import 'package:firebase_messaging/firebase_messaging.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // Initialize Firebase
  await Firebase.initializeApp();

  // Request permission
  await FirebaseMessaging.instance.requestPermission();

  // Get token
  String? token = await FirebaseMessaging.instance.getToken();
  print('Device Token: $token');

  // Handle messages
  FirebaseMessaging.onMessage.listen((RemoteMessage message) {
    print('Message received: ${message.notification?.title}');
  });

  runApp(const MyApp());
}
```

---

## ⚠️ Manejo de Errores

| Error                  | Causa                     | Solución                        |
| ---------------------- | ------------------------- | ------------------------------- |
| **invalid_argument**   | Token inválido o expirado | Solicitar nuevo token al device |
| **not_found**          | Device no registrado      | Eliminar token de base de datos |
| **permission_denied**  | Credenciales inválidas    | Verificar service account key   |
| **resource_exhausted** | Rate limit                | Implementar backoff exponencial |
| **unavailable**        | Firebase down             | Implementar retry logic         |

---

## 💰 Costos

**Firebase Cloud Messaging:** ✅ **GRATUITO**

- ✅ Mensajes ilimitados
- ✅ Sin costos adicionales
- ✅ Analytics incluido

---

## 📊 Analytics en Firebase Console

```
https://console.firebase.google.com/project/okla-marketplace/messaging
```

**Monitorear:**

- Mensajes enviados
- Tasa de apertura
- Interacciones
- Errores y fallos

---

**Mantenido por:** Mobile Team  
**Última revisión:** Enero 15, 2026  
**Próxima implementación:** Q3 2026
