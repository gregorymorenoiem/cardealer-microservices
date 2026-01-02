# 🛠️ SPRINT 0 - Setup Inicial y Configuración de Entorno

**Fecha:** 2 Enero 2026  
**Duración estimada:** 2-3 horas  
**Tokens estimados:** ~18,000  
**Prioridad:** 🔴 CRÍTICO

---

## 🎯 OBJETIVOS

1. Configurar variables de entorno en frontend y backend
2. Crear archivo `.env` para frontend/web/original
3. Configurar `compose.secrets.yaml` para backend
4. Validar conectividad entre frontend y Gateway
5. Verificar CORS en Gateway
6. Documentar URLs y puertos

---

## 📋 CHECKLIST DE TAREAS

### Fase 1: Frontend Configuration (30 min)

- [ ] 1.1. Crear archivo `.env` en `frontend/web/original/`
- [ ] 1.2. Configurar URLs de microservicios
- [ ] 1.3. Agregar placeholders para APIs de terceros
- [ ] 1.4. Crear `.env.example` con documentación
- [ ] 1.5. Actualizar `.gitignore` para proteger `.env`

### Fase 2: Backend Gateway Configuration (45 min)

- [ ] 2.1. Verificar rutas en `Gateway/Gateway.Api/ocelot.dev.json`
- [ ] 2.2. Configurar CORS para `localhost:5174`
- [ ] 2.3. Validar JWT authentication en Gateway
- [ ] 2.4. Agregar rate limiting para desarrollo
- [ ] 2.5. Documentar rutas disponibles

### Fase 3: Docker Compose Secrets (30 min)

- [ ] 3.1. Crear `compose.secrets.yaml` desde example
- [ ] 3.2. Generar JWT secret key seguro
- [ ] 3.3. Configurar PostgreSQL passwords
- [ ] 3.4. Validar que servicios levanten correctamente
- [ ] 3.5. Documentar proceso de setup

### Fase 4: Connectivity Testing (30 min)

- [ ] 4.1. Levantar backend con `docker-compose up -d`
- [ ] 4.2. Levantar frontend con `npm run dev`
- [ ] 4.3. Probar health check del Gateway
- [ ] 4.4. Probar autenticación básica
- [ ] 4.5. Validar logs en servicios

### Fase 5: Migración de Assets del Frontend (CRÍTICO - 16-20h)

⚠️ **NOTA:** Esta fase es BLOQUEANTE para producción. El frontend actualmente usa imágenes/videos de URLs externas (Unsplash, placeholders). Todos estos recursos deben migrarse al backend.

- [ ] 5.1. **Auditoría de Assets** (4-5h)
  - [ ] Crear script para detectar URLs externas en código
  - [ ] Identificar todas las imágenes hardcodeadas
  - [ ] Listar videos y recursos multimedia
  - [ ] Categorizar por tipo (vehicles, properties, avatars, ui, backgrounds)
  - [ ] Documentar fuente original y licencia

- [ ] 5.2. **Descargar y Organizar** (3-4h)
  - [ ] Script automatizado para descargar assets
  - [ ] Optimizar imágenes (resize, compress, WebP conversion)
  - [ ] Organizar en estructura de carpetas
  - [ ] Validar calidad y resolución
  - [ ] Preparar metadatos (alt text, tags)

- [ ] 5.3. **Seed MediaService/FileStorageService** (6-8h)
  - [ ] Crear script C# de seed para MediaService
  - [ ] Subir assets a S3/Azure Blob Storage
  - [ ] Registrar en tabla `media_files`
  - [ ] Generar URLs públicas
  - [ ] Configurar CDN para assets

- [ ] 5.4. **Actualizar Frontend** (3-4h)
  - [ ] Crear `assetService.ts` para consumir backend
  - [ ] Crear componente `ImageWithFallback.tsx`
  - [ ] Reemplazar TODAS las URLs hardcodeadas
  - [ ] Actualizar componentes (VehicleCard, HeroSection, etc.)
  - [ ] Testing de carga de assets
  - [ ] Validar lazy loading y caching

---

## 📝 IMPLEMENTACIÓN DETALLADA

### 1️⃣ Frontend - Archivo .env

**Archivo:** `frontend/web/original/.env`

```env
# ============================================================================
# CarDealer Frontend - Environment Variables
# ============================================================================
# IMPORTANTE: NO subir este archivo a Git
# Copiar desde .env.example y configurar valores reales
# ============================================================================

# ----------------------------------------------------------------------------
# API ENDPOINTS
# ----------------------------------------------------------------------------

# API Gateway (Ocelot) - Punto de entrada único
VITE_API_URL=http://localhost:18443/api

# Servicios específicos (solo si necesitas bypass del Gateway)
VITE_AUTH_SERVICE_URL=http://localhost:15085/api
VITE_VEHICLE_SERVICE_URL=http://localhost:15006/api
VITE_UPLOAD_SERVICE_URL=http://localhost:15007/api
VITE_BILLING_SERVICE_URL=http://localhost:15008/api
VITE_NOTIFICATION_SERVICE_URL=http://localhost:15084/api
VITE_MESSAGE_SERVICE_URL=http://localhost:15004/api
VITE_ADMIN_SERVICE_URL=http://localhost:15011/api

# ----------------------------------------------------------------------------
# THIRD PARTY APIs (Se configurarán en Sprint 1)
# ----------------------------------------------------------------------------

# Google Maps API
VITE_GOOGLE_MAPS_API_KEY=AIzaSyDKVgKqLUzWFaEMcXjkZUUTOFDNa4V0AFI

# Firebase (Push Notifications)
VITE_FIREBASE_API_KEY=placeholder
VITE_FIREBASE_AUTH_DOMAIN=placeholder
VITE_FIREBASE_PROJECT_ID=placeholder
VITE_FIREBASE_STORAGE_BUCKET=placeholder
VITE_FIREBASE_MESSAGING_SENDER_ID=placeholder
VITE_FIREBASE_APP_ID=placeholder
VITE_FIREBASE_MEASUREMENT_ID=placeholder

# Stripe (Payments)
VITE_STRIPE_PUBLIC_KEY=pk_test_placeholder

# ----------------------------------------------------------------------------
# FEATURE FLAGS
# ----------------------------------------------------------------------------

# Development flags
VITE_USE_MOCK_AUTH=false
VITE_ENABLE_LOGGING=true
VITE_ENABLE_ANALYTICS=false

# Feature toggles
VITE_ENABLE_2FA=true
VITE_ENABLE_PUSH_NOTIFICATIONS=false
VITE_ENABLE_LIVE_CHAT=false
VITE_ENABLE_STRIPE_PAYMENTS=false

# ----------------------------------------------------------------------------
# APP CONFIGURATION
# ----------------------------------------------------------------------------

# App metadata
VITE_APP_NAME=CarDealer
VITE_APP_VERSION=1.0.0
VITE_APP_ENV=development

# Pagination
VITE_DEFAULT_PAGE_SIZE=12
VITE_MAX_PAGE_SIZE=100

# Upload limits
VITE_MAX_IMAGE_SIZE_MB=5
VITE_MAX_IMAGES_PER_VEHICLE=20
VITE_SUPPORTED_IMAGE_FORMATS=image/jpeg,image/png,image/webp

# ----------------------------------------------------------------------------
# TIMEOUTS & LIMITS
# ----------------------------------------------------------------------------

VITE_API_TIMEOUT=30000
VITE_UPLOAD_TIMEOUT=120000
VITE_JWT_REFRESH_BEFORE_EXPIRY=300000

# ----------------------------------------------------------------------------
# MONITORING (Opcional)
# ----------------------------------------------------------------------------

# Sentry (Error tracking)
VITE_SENTRY_DSN=
VITE_SENTRY_ENVIRONMENT=development

# Google Analytics
VITE_GA_TRACKING_ID=
```

---

### 2️⃣ Frontend - .env.example

**Archivo:** `frontend/web/original/.env.example`

```env
# CarDealer Frontend - Environment Variables Template
# Copiar a .env y configurar valores reales

# API Gateway
VITE_API_URL=http://localhost:18443/api

# Microservicios (opcional, usar Gateway por defecto)
VITE_AUTH_SERVICE_URL=http://localhost:15085/api
VITE_VEHICLE_SERVICE_URL=http://localhost:15006/api
VITE_UPLOAD_SERVICE_URL=http://localhost:15007/api

# APIs de terceros (configurar en Sprint 1)
VITE_GOOGLE_MAPS_API_KEY=your-google-maps-api-key
VITE_STRIPE_PUBLIC_KEY=pk_test_your-stripe-public-key
VITE_FIREBASE_API_KEY=your-firebase-api-key

# Feature flags
VITE_USE_MOCK_AUTH=false
VITE_ENABLE_2FA=true
VITE_ENABLE_PUSH_NOTIFICATIONS=false
```

---

### 3️⃣ Gateway - CORS Configuration

**Archivo:** `backend/Gateway/Gateway.Api/Program.cs`

Verificar que tenga esta configuración de CORS:

```csharp
// CORS - Agregar frontend origin
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",    // Vite default
            "http://localhost:5174",    // Frontend original
            "http://localhost:3000",    // React alternative
            "https://cardealer.app",    // Production (futuro)
            "https://www.cardealer.app" // Production www
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();  // Importante para JWT cookies
    });
});

// ... resto del código

// Middleware CORS DEBE ir ANTES de UseAuthorization
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
```

---

### 4️⃣ Gateway - Rutas Ocelot

**Archivo:** `backend/Gateway/Gateway.Api/ocelot.dev.json`

Verificar que tenga todas las rutas necesarias:

```json
{
  "Routes": [
    {
      "UpstreamPathTemplate": "/api/auth/{everything}",
      "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"],
      "DownstreamPathTemplate": "/api/auth/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "authservice", "Port": 80 }
      ],
      "RateLimitOptions": {
        "ClientWhitelist": [],
        "EnableRateLimiting": true,
        "Period": "1m",
        "PeriodTimespan": 60,
        "Limit": 100
      }
    },
    {
      "UpstreamPathTemplate": "/api/vehicles/{everything}",
      "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"],
      "DownstreamPathTemplate": "/api/products/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "productservice", "Port": 80 }
      ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      }
    },
    {
      "UpstreamPathTemplate": "/api/media/{everything}",
      "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"],
      "DownstreamPathTemplate": "/api/media/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "mediaservice", "Port": 80 }
      ]
    },
    {
      "UpstreamPathTemplate": "/api/billing/{everything}",
      "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"],
      "DownstreamPathTemplate": "/api/billing/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "billingservice", "Port": 80 }
      ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      }
    },
    {
      "UpstreamPathTemplate": "/api/notifications/{everything}",
      "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"],
      "DownstreamPathTemplate": "/api/notifications/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "notificationservice", "Port": 80 }
      ]
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "http://localhost:18443",
    "RateLimitOptions": {
      "DisableRateLimitHeaders": false,
      "QuotaExceededMessage": "Rate limit exceeded",
      "HttpStatusCode": 429
    }
  }
}
```

---

### 5️⃣ Backend - compose.secrets.yaml

**Archivo:** `compose.secrets.yaml`

```yaml
# ============================================================================
# CarDealer Microservices - Secrets Configuration
# ============================================================================
# IMPORTANTE: NO subir este archivo a Git
# Copiar desde compose.secrets.example.yaml
# ============================================================================

# ----------------------------------------------------------------------------
# JWT AUTHENTICATION
# ----------------------------------------------------------------------------
JWT__KEY: "YourSuperSecretJWTKeyMinimum32CharactersLongForHS256Algorithm!"

# ----------------------------------------------------------------------------
# DATABASES
# ----------------------------------------------------------------------------
POSTGRES_PASSWORD: "YourSecurePostgresPassword123!"

# PostgreSQL por servicio (si necesitas passwords diferentes)
AUTHSERVICE_DB_PASSWORD: "AuthServiceDBPass123!"
PRODUCTSERVICE_DB_PASSWORD: "ProductServiceDBPass123!"
MEDIASERVICE_DB_PASSWORD: "MediaServiceDBPass123!"

# ----------------------------------------------------------------------------
# THIRD PARTY APIs (Configurar en Sprint 1)
# ----------------------------------------------------------------------------

# Google Cloud Platform
GOOGLE_MAPS_API_KEY: "placeholder"
GOOGLE_OAUTH_CLIENT_ID: "placeholder"
GOOGLE_OAUTH_CLIENT_SECRET: "placeholder"

# Microsoft OAuth
MICROSOFT_OAUTH_CLIENT_ID: "placeholder"
MICROSOFT_OAUTH_CLIENT_SECRET: "placeholder"

# Firebase (Push Notifications)
FIREBASE_PROJECT_ID: "placeholder"
FIREBASE_PRIVATE_KEY: "placeholder"
FIREBASE_CLIENT_EMAIL: "placeholder"

# Stripe (Payments)
STRIPE_SECRET_KEY: "sk_test_placeholder"
STRIPE_WEBHOOK_SECRET: "whsec_placeholder"

# SendGrid (Email)
SENDGRID_API_KEY: "SG.placeholder"
SENDGRID_FROM_EMAIL: "noreply@cardealer.app"

# Twilio (SMS)
TWILIO_ACCOUNT_SID: "ACplaceholder"
TWILIO_AUTH_TOKEN: "placeholder"
TWILIO_PHONE_NUMBER: "+1234567890"

# AWS S3 (Storage)
AWS_ACCESS_KEY_ID: "AKIAPLACEHOLDER"
AWS_SECRET_ACCESS_KEY: "placeholder"
AWS_REGION: "us-east-1"
AWS_BUCKET_NAME: "cardealer-images"

# Azure Blob Storage (alternativa)
AZURE_STORAGE_CONNECTION_STRING: "placeholder"
AZURE_STORAGE_CONTAINER: "cardealer-images"

# ----------------------------------------------------------------------------
# MONITORING & LOGGING
# ----------------------------------------------------------------------------

# Sentry (Error tracking)
SENTRY_DSN: ""
SENTRY_ENVIRONMENT: "development"

# Elasticsearch (si se usa)
ELASTICSEARCH_PASSWORD: "placeholder"
```

---

### 6️⃣ Script de Generación de Secrets

**Archivo:** `scripts/generate-jwt-secret.ps1`

```powershell
#!/usr/bin/env pwsh
# Generate secure JWT secret key

$bytes = New-Object byte[] 32
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes)
$secret = [Convert]::ToBase64String($bytes)

Write-Host "Generated JWT Secret Key:" -ForegroundColor Green
Write-Host $secret
Write-Host ""
Write-Host "Copy this to compose.secrets.yaml:" -ForegroundColor Yellow
Write-Host "JWT__KEY: `"$secret`""
```

**Ejecutar:**

```bash
chmod +x scripts/generate-jwt-secret.ps1
./scripts/generate-jwt-secret.ps1
```

---

### 7️⃣ Frontend - Actualizar servicios para usar Gateway

**Archivo:** `frontend/web/original/src/services/api.ts`

Actualizar para usar la variable de entorno:

```typescript
import axios, { type AxiosError, type AxiosInstance, type InternalAxiosRequestConfig } from 'axios';

// Usar Gateway como punto de entrada único
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443/api';

// Create axios instance
export const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: parseInt(import.meta.env.VITE_API_TIMEOUT || '30000'),
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true, // Importante para cookies/CORS
});

// ... resto del código igual
```

---

## ✅ CRITERIOS DE ACEPTACIÓN

### Validación Frontend

```bash
cd frontend/web/original

# 1. Verificar que .env existe
test -f .env && echo "✅ .env exists" || echo "❌ .env missing"

# 2. Verificar que puede leer variables
npm run dev &
sleep 5
curl http://localhost:5174 -I | grep "200 OK"
pkill -f "vite"
```

### Validación Backend

```bash
cd backend

# 1. Verificar que compose.secrets.yaml existe
test -f ../compose.secrets.yaml && echo "✅ secrets file exists"

# 2. Levantar servicios
docker-compose up -d redis rabbitmq gateway authservice

# 3. Esperar 30 segundos
sleep 30

# 4. Verificar Gateway health
curl http://localhost:18443/health

# 5. Verificar AuthService health
curl http://localhost:18443/api/auth/health
```

### Validación CORS

```bash
# Desde el frontend, probar request con CORS
curl -H "Origin: http://localhost:5174" \
     -H "Access-Control-Request-Method: GET" \
     -H "Access-Control-Request-Headers: X-Requested-With" \
     -X OPTIONS \
     --verbose \
     http://localhost:18443/api/auth/health
     
# Debe retornar:
# Access-Control-Allow-Origin: http://localhost:5174
# Access-Control-Allow-Credentials: true
```

---

## 🧪 TESTING

### Test 1: Gateway Health Check

```bash
# Debe retornar 200 OK
curl -i http://localhost:18443/health
```

**Resultado esperado:**

```
HTTP/1.1 200 OK
Content-Type: application/json

{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567"
}
```

### Test 2: AuthService via Gateway

```bash
# Registro de usuario
curl -X POST http://localhost:18443/api/auth/register \
  -H "Content-Type: application/json" \
  -H "Origin: http://localhost:5174" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!",
    "fullName": "Test User"
  }'
```

**Resultado esperado:**

```json
{
  "success": true,
  "data": {
    "user": {
      "id": "...",
      "email": "test@example.com",
      "fullName": "Test User"
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "..."
  }
}
```

### Test 3: Frontend conecta a Backend

1. Levantar backend: `docker-compose up -d`
2. Levantar frontend: `cd frontend/web/original && npm run dev`
3. Abrir http://localhost:5174
4. Ir a página de Login
5. Abrir DevTools → Network
6. Intentar login con credenciales de prueba
7. Verificar request a `http://localhost:18443/api/auth/login`

---

## 📊 ESTIMACIÓN DE TOKENS

| Tarea | Lectura | Escritura | Total |
|-------|---------|-----------|-------|
| Crear .env frontend | 500 | 2,000 | 2,500 |
| Actualizar api.ts | 1,000 | 500 | 1,500 |
| Configurar Gateway CORS | 2,000 | 1,000 | 3,000 |
| Verificar ocelot.json | 3,000 | 500 | 3,500 |
| Crear compose.secrets.yaml | 1,000 | 2,500 | 3,500 |
| Scripts de validación | 500 | 1,000 | 1,500 |
| Testing y debugging | 1,500 | 500 | 2,000 |
| **TOTAL** | **9,500** | **8,000** | **17,500** |

**Con buffer 15%:** ~20,000 tokens

---

## 🚨 TROUBLESHOOTING

### Problema: Frontend no puede conectar a Gateway

**Síntomas:**
```
Access to XMLHttpRequest blocked by CORS policy
```

**Solución:**
1. Verificar que Gateway tiene CORS configurado
2. Verificar que `localhost:5174` está en lista de origins
3. Verificar que `withCredentials: true` en axios
4. Reiniciar Gateway: `docker-compose restart gateway`

### Problema: Gateway retorna 404 en todas las rutas

**Síntomas:**
```
404 Not Found
```

**Solución:**
1. Verificar que `ocelot.dev.json` está en la imagen Docker
2. Verificar variable `ASPNETCORE_ENVIRONMENT=Development`
3. Ver logs: `docker logs gateway`
4. Verificar que servicios downstream están UP

### Problema: JWT secret no funciona

**Síntomas:**
```
401 Unauthorized
Invalid token
```

**Solución:**
1. Verificar que `JWT__KEY` tiene al menos 32 caracteres
2. Verificar que AuthService y Gateway usan el MISMO secret
3. Regenerar secret: `./scripts/generate-jwt-secret.ps1`
4. Actualizar en `compose.secrets.yaml`
5. Reiniciar servicios

---

## 📝 DOCUMENTACIÓN GENERADA

Al completar este sprint, deben existir:

- [ ] `frontend/web/original/.env` (no en Git)
- [ ] `frontend/web/original/.env.example` (en Git)
- [ ] `compose.secrets.yaml` (no en Git)
- [ ] `compose.secrets.example.yaml` (en Git)
- [ ] `docs/API_ENDPOINTS.md` (documentación de rutas)
- [ ] `docs/ENVIRONMENT_VARIABLES.md` (referencia completa)

---

## ➡️ PRÓXIMO SPRINT

**Sprint 1:** [SPRINT_1_CUENTAS_TERCEROS.md](SPRINT_1_CUENTAS_TERCEROS.md)

Crear cuentas en servicios de terceros:
- Google Cloud Platform (Maps API)
- Firebase (Push Notifications)
- Stripe (Payments)
- SendGrid (Email)
- AWS S3 (Storage)

---

**Estado:** ⚪ Pendiente  
**Última actualización:** 2 Enero 2026
