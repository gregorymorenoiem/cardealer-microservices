# 🔧 Spyne API - Guía de Configuración

**Última actualización:** Enero 21, 2026  
**Documentación oficial:** https://docs.spyne.ai/docs

---

## 📋 Índice

1. [Obtener API Key](#-obtener-api-key)
2. [Configuración en OKLA](#-configuración-en-okla)
3. [Endpoints Disponibles](#-endpoints-disponibles)
4. [Autenticación](#-autenticación)
5. [Configuración de Webhooks](#-configuración-de-webhooks)
6. [Variables de Entorno](#-variables-de-entorno)
7. [Testing Local](#-testing-local)
8. [Troubleshooting](#-troubleshooting)

---

## 🔑 Obtener API Key

### Pasos para Obtener la API Key

1. Visitar [Spyne Console](https://console.spyne.ai/)
2. Login o Signup
3. Ir a **Developer Hub > API Keys**
4. Click en **Generate Key** y asignar un nombre (ej: `Production`)
5. **Copiar la key** - ¡Solo se muestra una vez!

> ⚠️ Si pierdes la key, debes regenerar una nueva desde el mismo lugar.

### Formato de la API Key

La API Key de Spyne es un **JWT Token** con el siguiente formato:

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbnRlcnByaXNlSWQiOiIwOTg3ODAwZjEiLCJ0ZWFtSWQiOiI5ZDJjMjVmNTQ2IiwidXNlcklkIjoiYTQ1ZGE3MWEiLCJzZWNyZXRLZXkiOiI2OTFlZWM3MjFiZDg0YzMwOWEwZTE0NzBiYjljZjIxYiIsImlhdCI6MTc2ODk1MzQ1OCwiZXhwIjoxODYzNTYxNDU4fQ.XXXXXXXXXXXXXXXXXXXXXXXX
```

El JWT contiene:

- `enterpriseId` - ID de tu empresa
- `teamId` - ID del equipo (dealerId)
- `userId` - ID del usuario
- `secretKey` - Clave secreta interna
- `exp` - Fecha de expiración

---

## ⚙️ Configuración en OKLA

### 1. Backend - appsettings.json

```json
{
  "SpyneApi": {
    "BaseUrl": "https://api.spyne.ai/api",
    "ApiKey": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "TimeoutSeconds": 120,
    "RetryCount": 3,
    "WebhookUrl": "https://api.okla.com.do/api/webhooks/spyne"
  }
}
```

### 2. Docker Compose - compose.yaml

```yaml
spyneintegrationservice:
  build:
    context: ./backend/SpyneIntegrationService/SpyneIntegrationService.Api
    dockerfile: Dockerfile
  ports:
    - "15070:8080"
  environment:
    - ASPNETCORE_ENVIRONMENT=Development
    - SpyneApi__ApiKey=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
    - SpyneApi__TimeoutSeconds=120
    - SpyneApi__WebhookUrl=https://api.okla.com.do/api/webhooks/spyne
  depends_on:
    postgres:
      condition: service_healthy
    rabbitmq:
      condition: service_healthy
```

### 3. Gateway - ocelot.dev.json

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/vehicle-images/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "spyneintegrationservice", "Port": 8080 }
      ],
      "UpstreamPathTemplate": "/api/spyne/vehicle-images/{everything}",
      "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"]
    },
    {
      "DownstreamPathTemplate": "/health",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "spyneintegrationservice", "Port": 8080 }
      ],
      "UpstreamPathTemplate": "/api/spyne/health",
      "UpstreamHttpMethod": ["GET"]
    }
  ]
}
```

---

## 🌐 Endpoints Disponibles

### Base URL

| Ambiente        | URL                                  |
| --------------- | ------------------------------------ |
| Spyne API       | `https://api.spyne.ai/api/`          |
| OKLA Local      | `http://localhost:15070/`            |
| OKLA Gateway    | `http://localhost:18443/api/spyne/`  |
| OKLA Producción | `https://api.okla.com.do/api/spyne/` |

### Endpoints de Spyne (Unified API v2)

| Método | Endpoint                            | Descripción                         |
| ------ | ----------------------------------- | ----------------------------------- |
| `POST` | `/pv1/merchandise/process`          | Procesar imágenes/video de vehículo |
| `GET`  | `/pv1/merchandise?dealerVinId={id}` | Obtener resultado procesado         |

### Endpoints de OKLA (SpyneIntegrationService)

| Método | Endpoint                              | Descripción                    |
| ------ | ------------------------------------- | ------------------------------ |
| `POST` | `/api/vehicle-images/transform`       | Transformar una imagen         |
| `POST` | `/api/vehicle-images/transform/batch` | Transformar múltiples imágenes |
| `GET`  | `/api/vehicle-images/status/{jobId}`  | Verificar estado del job       |
| `GET`  | `/api/vehicle-images/backgrounds`     | Listar backgrounds disponibles |
| `GET`  | `/health`                             | Health check del servicio      |

---

## 🔐 Autenticación

Spyne usa **Bearer Token Authentication**:

```bash
curl -X POST "https://api.spyne.ai/api/pv1/merchandise/process" \
  -H "Authorization: Bearer YOUR_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{...}'
```

### Implementación en C#

```csharp
_httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
```

> ⚠️ **NO usar** `x-api-key` header - Spyne usa Bearer tokens.

---

## 🪝 Configuración de Webhooks

### Registro en Spyne Console

1. Ir a [Spyne Console](https://console.spyne.ai/)
2. Login y abrir **More > Developer Hub > Webhooks**
3. Click **Add Webhook Endpoint**
4. Ingresar URL: `https://api.okla.com.do/api/webhooks/spyne`
5. Seleccionar eventos:
   - `after_ai_done` - Cuando AI termina
   - `after_qc_done` - Después de QC manual
   - `after_upload_done` - Cuando se sube media (SDK)

### Eventos de Webhook

| Evento              | Descripción               | Cuándo usar                                     |
| ------------------- | ------------------------- | ----------------------------------------------- |
| `after_ai_done`     | AI completó procesamiento | **Recomendado** - Para obtener imagen procesada |
| `after_qc_done`     | QC manual completado      | Si tienes QC habilitado                         |
| `after_upload_done` | Media subida (SDK)        | Solo para SDK mobile                            |

### Payload del Webhook

```json
{
  "vin": "M64QSUZG39P400000",
  "source": "api",
  "dealerId": "9d2c25f546",
  "dealerVinID": "e2adca58-66bc-4d56-ad16-f73823af9ba1",
  "mediaData": {
    "image": {
      "aiStatus": "DONE",
      "qcStatus": "qc_done",
      "imageData": [
        {
          "inputImage": "https://spyne-media.s3.amazonaws.com/...",
          "outputImage": "https://spyne-media.s3.amazonaws.com/...",
          "backgroundId": "20883",
          "category": "Exterior"
        }
      ]
    }
  }
}
```

### Respuesta Esperada

Tu endpoint debe retornar `2xx` para confirmar recepción:

```json
{ "status": "received" }
```

> 📝 Spyne usa **retry con backoff exponencial** si no recibe 2xx.

---

## 🔧 Variables de Entorno

### Desarrollo Local

```env
SpyneApi__BaseUrl=https://api.spyne.ai/api
SpyneApi__ApiKey=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
SpyneApi__TimeoutSeconds=120
SpyneApi__RetryCount=3
SpyneApi__WebhookUrl=https://api.okla.com.do/api/webhooks/spyne
```

### Kubernetes (ConfigMap/Secret)

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: spyne-secrets
  namespace: okla
type: Opaque
stringData:
  api-key: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## 🧪 Testing Local

### 1. Verificar Health

```bash
# Directo
curl http://localhost:15070/health

# Vía Gateway
curl http://localhost:18443/api/spyne/health
```

### 2. Listar Backgrounds

```bash
curl http://localhost:15070/api/vehicle-images/backgrounds | jq .
```

### 3. Transformar Imagen

```bash
curl -X POST http://localhost:15070/api/vehicle-images/transform \
  -H "Content-Type: application/json" \
  -d '{
    "imageUrl": "https://example.com/car.jpg",
    "stockNumber": "TEST-001"
  }' | jq .
```

### 4. Verificar Status

```bash
curl http://localhost:15070/api/vehicle-images/status/{jobId} | jq .
```

### 5. Test Directo a Spyne API

```bash
# Enviar imagen
curl -X POST "https://api.spyne.ai/api/pv1/merchandise/process" \
  -H "Authorization: Bearer YOUR_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "stockNumber": "TEST-001",
    "media": { "image": true },
    "mediaInput": {
      "imageData": [{ "url": "https://example.com/car.jpg" }]
    },
    "processingDetails": {
      "backgroundId": "20883",
      "image": { "backgroundType": "legacy" }
    }
  }' | jq .

# Verificar resultado
curl "https://api.spyne.ai/api/pv1/merchandise?dealerVinId={dealerVinId}" \
  -H "Authorization: Bearer YOUR_API_KEY" | jq .
```

---

## 🐛 Troubleshooting

### Error: "Invalid Auth key" / 401 Unauthorized

**Causa:** Header de autenticación incorrecto.

**Solución:** Usar `Authorization: Bearer TOKEN` (no `x-api-key`).

```csharp
// ❌ Incorrecto
_httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

// ✅ Correcto
_httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", apiKey);
```

### Error: "BackgroundId is not mapped"

**Causa:** El backgroundId no está autorizado para tu cuenta.

**Solución:** Usar `20883` (default de documentación) o contactar a Spyne para obtener tus IDs.

```json
// ❌ Incorrecto (ID no autorizado)
"processingDetails": { "backgroundId": "75282" }

// ✅ Correcto (ID de documentación)
"processingDetails": { "backgroundId": "20883" }
```

### Error: 404 en get-media

**Causa:** Endpoint incorrecto.

**Solución:** Usar `/pv1/merchandise` (no `/pv1/merchandise/get-media`).

```csharp
// ❌ Incorrecto
$"pv1/merchandise/get-media?dealerVinId={id}"

// ✅ Correcto
$"pv1/merchandise?dealerVinId={id}"
```

### Status siempre "processing"

**Causa:** El modelo de datos no mapea correctamente la respuesta.

**Solución:** La respuesta usa `mediaData` (no `outputData`).

```csharp
// Respuesta real de Spyne
{
  "mediaData": {
    "image": {
      "aiStatus": "DONE",
      "imageData": [...]
    }
  }
}
```

### Timeout en procesamiento

**Causa:** Spyne toma 60-120 segundos para procesar.

**Solución:**

1. Aumentar timeout a 120s
2. Usar polling cada 10-15 segundos
3. Configurar webhook para notificación automática

---

## 📚 Referencias

- [Documentación Oficial Spyne](https://docs.spyne.ai/docs)
- [API Reference](https://docs.spyne.ai/reference)
- [Spyne Console](https://console.spyne.ai/)
- [Transform your first Vehicle](https://docs.spyne.ai/docs/transform-your-first-vehicle-1)
- [Webhook Guide](https://docs.spyne.ai/docs/webhook-1)

---

**Autor:** Equipo OKLA  
**Contacto Spyne:** support@spyne.ai
