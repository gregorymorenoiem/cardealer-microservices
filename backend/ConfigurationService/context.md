# ConfigurationService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** ConfigurationService
- **Puerto en Desarrollo:** 5015
- **Estado:** ⚠️ **SOLO DESARROLLO LOCAL**
- **Base de Datos:** PostgreSQL (`configurationservice`)
- **Imagen Docker:** Local only

### Propósito
Servicio de configuración centralizada para todos los microservicios. Permite cambiar configuraciones sin redeploy. Alternativa self-hosted a AWS Parameter Store o Azure App Configuration.

---

## 🏗️ ARQUITECTURA

```
ConfigurationService/
├── ConfigurationService.Api/
│   ├── Controllers/
│   │   ├── ConfigController.cs
│   │   └── SecretsController.cs
│   └── Program.cs
├── ConfigurationService.Application/
│   └── Services/
│       ├── ConfigurationManager.cs
│       └── SecretManager.cs
├── ConfigurationService.Domain/
│   ├── Entities/
│   │   ├── ConfigurationItem.cs
│   │   └── ConfigurationHistory.cs
│   └── Enums/
│       └── ConfigurationType.cs
└── ConfigurationService.Infrastructure/
    └── Persistence/
```

---

## 📦 ENTIDADES

### ConfigurationItem
```csharp
public class ConfigurationItem
{
    public Guid Id { get; set; }
    public string Key { get; set; }                 // "Features:EnableNewDashboard"
    public string Value { get; set; }
    public string? Description { get; set; }
    public ConfigurationType Type { get; set; }     // String, Number, Boolean, Json
    public string? Environment { get; set; }        // "Production", "Development"
    public bool IsSecret { get; set; }              // Si es sensible, encriptar
    public bool RequiresRestart { get; set; }       // Si cambio requiere restart
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
```

---

## 📡 ENDPOINTS API

#### GET `/api/config`
Obtener todas las configuraciones para un servicio.

**Query Parameters:**
- `service`: Nombre del servicio
- `environment`: Production, Development

**Response (200 OK):**
```json
{
  "configurations": {
    "Database:ConnectionString": "...",
    "Features:EnableNewUI": "true",
    "Cache:DefaultExpirationSeconds": "3600"
  }
}
```

#### GET `/api/config/{key}`
Obtener configuración específica.

#### POST `/api/config`
Crear/Actualizar configuración.

**Request:**
```json
{
  "key": "Features:EnableNewDashboard",
  "value": "true",
  "type": "Boolean",
  "environment": "Production",
  "description": "Habilitar nuevo dashboard"
}
```

#### DELETE `/api/config/{key}`
Eliminar configuración.

---

## 🔧 EJEMPLOS DE CONFIGURACIONES

### Feature Flags
```json
{
  "Features:EnableVehicleRecommendations": "true",
  "Features:EnablePaymentWithCrypto": "false",
  "Features:MaxImagesPerVehicle": "20"
}
```

### Límites y Cuotas
```json
{
  "Limits:MaxVehiclesPerUser": "10",
  "Limits:FreeUserMaxListings": "3",
  "Limits:PremiumUserMaxListings": "50"
}
```

### Integraciones Externas
```json
{
  "Stripe:PublishableKey": "pk_...",
  "Twilio:PhoneNumber": "+18095551234",
  "SendGrid:FromEmail": "noreply@okla.com.do"
}
```

---

## 🔄 ACTUALIZACIÓN EN CALIENTE

### Opción 1: Polling
Servicios hacen request cada N minutos para obtener nueva configuración.

### Opción 2: Webhooks
ConfigurationService envía webhook cuando hay cambios.

### Opción 3: SignalR
Push notifications en tiempo real a servicios suscritos.

---

## 🔐 GESTIÓN DE SECRETS

Los valores marcados como `IsSecret = true` se encriptan en base de datos usando AES-256.

```csharp
// Encriptar
string encrypted = AesEncryption.Encrypt(value, masterKey);

// Desencriptar
string decrypted = AesEncryption.Decrypt(encrypted, masterKey);
```

**Master Key** se almacena en variable de entorno, nunca en BD.

---

## 🚀 ALTERNATIVAS EN PRODUCCIÓN

- **Kubernetes ConfigMaps**: Para configuración no sensible
- **Kubernetes Secrets**: Para datos sensibles
- **AWS Parameter Store**: Config centralizado en AWS
- **Azure App Configuration**: Config centralizado en Azure
- **HashiCorp Vault**: Para secrets management
- **appsettings.json + Environment Variables**: Approach actual en OKLA

---

**Estado:** Solo desarrollo - K8s ConfigMaps/Secrets en producción  
**Versión:** 1.0.0
