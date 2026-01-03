# 🚀 Sprint 1 - Guía Paso a Paso para Configurar Servicios de Terceros

**Fecha:** 2 Enero 2026  
**Duración estimada:** 3-4 horas  
**Estado:** En progreso

---

## 📋 CHECKLIST GENERAL

- [ ] **1. Google Cloud Platform** (30 min) - Google Maps API
- [ ] **2. Firebase** (20 min) - Push Notifications
- [ ] **3. Stripe** (25 min) - Pagos
- [ ] **4. SendGrid** (15 min) - Email
- [ ] **5. Twilio** (15 min) - SMS (opcional)
- [ ] **6. AWS S3** (30 min) - Almacenamiento de archivos
- [ ] **7. Sentry** (10 min) - Error tracking (opcional)
- [ ] **8. Actualizar configuración** (15 min)
- [ ] **9. Probar conectividad** (10 min)

**Total:** ~2.5-3 horas

---

## 🎯 PRIORIDAD DE SERVICIOS

| Prioridad | Servicios | Razón |
|-----------|-----------|-------|
| 🔴 **CRÍTICO** | Google Maps, AWS S3, Stripe | Funcionalidad core de la app |
| 🟠 **ALTA** | Firebase, SendGrid | Notificaciones esenciales |
| 🟡 **MEDIA** | Twilio, Sentry | Nice-to-have, puede esperar |

**Recomendación:** Si tienes poco tiempo, comienza con los servicios CRÍTICOS primero.

---

## 1️⃣ GOOGLE CLOUD PLATFORM - Google Maps API + OAuth2

### ¿Por qué lo necesitamos?
- **Google Maps:** Mapas interactivos, autocompletado de direcciones, geolocalización
- **OAuth2:** Permitir login con cuenta de Google (Sign in with Google)

### Pasos de configuración:

#### 1.1. Crear cuenta y proyecto en Google Cloud

1. Ve a https://console.cloud.google.com
2. **Si NO tienes cuenta:**
   - Clic en "Get started for free"
   - Inicia sesión con tu Gmail
   - Acepta términos y condiciones
   - Agrega método de pago (tarjeta) - **NO cobra automáticamente**
   - Obtendrás **$300 USD de crédito gratis** por 90 días
3. **Crear proyecto nuevo:**
   - En la barra superior, clic en el dropdown de proyectos
   - Clic en "NEW PROJECT"
   - **Project name:** `CarDealer Production`
   - **Project ID:** Se genera automáticamente (ej: `cardealer-prod-123456`)
   - **Organization:** No organization (o tu org si tienes)
   - Clic en "CREATE"
   - Espera ~30 segundos
4. **Selecciona el proyecto recién creado** en el dropdown superior

#### 1.2. Habilitar APIs necesarias

En el menú lateral (☰) → **APIs & Services** → **Library**

**Buscar y habilitar una por una:**

1. **Maps JavaScript API:**
   - En el buscador escribe: `Maps JavaScript API`
   - Clic en el resultado
   - Clic en "ENABLE"
   - Espera ~10 segundos

2. **Places API:**
   - Regresa a Library (botón atrás)
   - Busca: `Places API`
   - Clic en "ENABLE"

3. **Geocoding API:**
   - Regresa a Library
   - Busca: `Geocoding API`
   - Clic en "ENABLE"

4. **Google+ API** (para OAuth2 login):
   - Regresa a Library
   - Busca: `Google+ API`
   - Clic en "ENABLE"
   - Si aparece aviso de deprecación, ignóralo (aún funciona para OAuth)

5. **People API** (opcional, para info de usuario):
   - Busca: `People API`
   - Clic en "ENABLE"

**Total habilitadas:** 4 APIs (o 5 si incluiste People API)

#### 1.3. Configurar OAuth Consent Screen (OBLIGATORIO)

Antes de crear credenciales OAuth, debes configurar la pantalla de consentimiento:

1. En el menú lateral → **APIs & Services** → **OAuth consent screen**
2. Selecciona tipo de usuario:
   - **External** (cualquier usuario con cuenta de Google puede loguearse)
   - Clic en "CREATE"

3. **Paso 1 - OAuth consent screen:**
   ```
   App name: CarDealer
   User support email: tu@email.com (tu Gmail)
   App logo: (dejar vacío por ahora)
   
   Application home page: http://localhost:5174
   Application privacy policy link: (dejar vacío)
   Application terms of service link: (dejar vacío)
   
   Authorized domains: (dejar vacío por ahora)
   
   Developer contact information:
   Email addresses: tu@email.com
   ```
   - Clic en "SAVE AND CONTINUE"

4. **Paso 2 - Scopes:**
   - Clic en "ADD OR REMOVE SCOPES"
   - En el modal, busca y selecciona estos 3 scopes:
     - ✅ `.../auth/userinfo.email`
     - ✅ `.../auth/userinfo.profile`
     - ✅ `openid`
   - Clic en "UPDATE"
   - Clic en "SAVE AND CONTINUE"

5. **Paso 3 - Test users (opcional):**
   - Para desarrollo, puedes agregar emails de prueba
   - Clic en "ADD USERS"
   - Agrega tu email personal
   - Clic en "SAVE AND CONTINUE"

6. **Paso 4 - Summary:**
   - Revisa la configuración
   - Clic en "BACK TO DASHBOARD"

**Estado:** Tu app estará en modo "Testing" (máximo 100 usuarios)

#### 1.4. Crear API Key (para Google Maps)

1. En el menú lateral → **APIs & Services** → **Credentials**
2. Clic en "**+ CREATE CREDENTIALS**" (arriba)
3. Selecciona "**API key**"
4. Se creará la key y verás un modal con la key:
   - Ejemplo: `AIzaSyDKVgKqLUzWFaEMcXjkZUUTOFDNa4V0AFI`
   - **⚠️ COPIA LA KEY AHORA** (aparece solo una vez)
5. **NO cierres el modal todavía**
6. Clic en "RESTRICT KEY" en el modal

**Restringir la API Key:**

7. **Name:** Cambia a `CarDealer Maps API Key`
8. **Application restrictions:**
   - Selecciona: **HTTP referrers (web sites)**
   - Clic en "ADD AN ITEM"
   - Agrega estos dominios uno por uno:
     ```
     http://localhost:5174/*
     http://localhost:5173/*
     http://localhost:3000/*
     https://cardealer.app/*
     https://www.cardealer.app/*
     ```

9. **API restrictions:**
   - Selecciona: **Restrict key**
   - Selecciona SOLO estas 3 APIs:
     - ✅ Maps JavaScript API
     - ✅ Places API
     - ✅ Geocoding API

10. Clic en "**SAVE**"

#### 1.5. Crear OAuth Client ID (para Google Login)

1. En **APIs & Services** → **Credentials**
2. Clic en "**+ CREATE CREDENTIALS**"
3. Selecciona "**OAuth client ID**"
4. **Application type:** Web application
5. **Name:** `CarDealer Web OAuth`

6. **Authorized JavaScript origins:**
   - Clic en "ADD URI" por cada uno:
     ```
     http://localhost:5174
     http://localhost:5173
     http://localhost:3000
     http://localhost:18443
     https://cardealer.app
     https://www.cardealer.app
     ```

7. **Authorized redirect URIs:**
   - Clic en "ADD URI" por cada uno:
     ```
     http://localhost:5174/auth/callback/google
     http://localhost:5173/auth/callback/google
     http://localhost:18443/api/auth/google-callback
     https://cardealer.app/auth/callback/google
     https://api.cardealer.app/api/auth/google-callback
     ```

8. Clic en "**CREATE**"

9. Verás un modal con tus credenciales:
   - **Client ID:** `123456789012-abc...apps.googleusercontent.com`
   - **Client Secret:** `GOCSPX-abc123...`
   - **⚠️ COPIA AMBOS AHORA**

10. Clic en "OK"

#### 1.6. Guardar TODAS las credenciales

**1. Google Maps API Key:**

Archivo: `secrets/google_maps_api_key.txt`
```
AIzaSyDKVgKqLUzWFaEMcXjkZUUTOFDNa4V0AFI
```

Archivo: `frontend/web/original/.env` (busca la línea y reemplaza):
```env
VITE_GOOGLE_MAPS_API_KEY=AIzaSyDKVgKqLUzWFaEMcXjkZUUTOFDNa4V0AFI
```

**2. Google OAuth Client ID:**

Archivo: `secrets/google_client_id.txt`
```
123456789012-abc...apps.googleusercontent.com
```

Archivo: `frontend/web/original/.env` (agrega DESPUÉS de Google Maps):
```env
# Google Maps API
VITE_GOOGLE_MAPS_API_KEY=AIzaSyDKVgKqLUzWFaEMcXjkZUUTOFDNa4V0AFI

# Google OAuth2 (agregar esta línea)
VITE_GOOGLE_OAUTH_CLIENT_ID=123456789012-abc...apps.googleusercontent.com
```

**3. Google OAuth Client Secret:**

Archivo: `secrets/google_client_secret.txt`
```
GOCSPX-abc123...
```

#### 1.7. Verificar configuración

**Verificar Google Maps API:**

Abre el navegador y ve a:
```
https://maps.googleapis.com/maps/api/js?key=TU_API_KEY&callback=console.log
```

Reemplaza `TU_API_KEY` con tu key real. Deberías ver un script de JavaScript cargándose sin errores.

**Verificar OAuth2:**

No puedes probar hasta que el frontend esté conectado (Sprint 2).

### 💰 Costos

- **Gratis:** $300 USD de crédito inicial (90 días)
- **Google Maps después del crédito:**
  - $7 por 1,000 cargas de mapa
  - $17 por 1,000 requests de Places API
- **OAuth2:** Completamente **GRATIS** (sin límites)
- **Suficiente para:** ~42,000 cargas de mapa/mes GRATIS

### ✅ Checklist completo

**Configuración inicial:**
- [ ] Cuenta de Google Cloud creada
- [ ] Método de pago agregado
- [ ] Proyecto `CarDealer Production` creado
- [ ] Proyecto seleccionado en dropdown

**APIs habilitadas:**
- [ ] Maps JavaScript API habilitada
- [ ] Places API habilitada
- [ ] Geocoding API habilitada
- [ ] Google+ API habilitada
- [ ] People API habilitada (opcional)

**OAuth Consent Screen:**
- [ ] Tipo External seleccionado
- [ ] App name "CarDealer" configurado
- [ ] User support email configurado
- [ ] 3 scopes agregados (email, profile, openid)
- [ ] Summary revisado

**Credenciales creadas:**
- [ ] API Key creada
- [ ] API Key restringida (dominios)
- [ ] API Key restringida (3 APIs)
- [ ] OAuth Client ID creado
- [ ] JavaScript origins agregados (6 URIs)
- [ ] Redirect URIs agregados (5 URIs)

**Credenciales guardadas:**
- [ ] API Key guardada en `secrets/google_maps_api_key.txt`
- [ ] API Key guardada en `frontend/.env`
- [ ] Client ID guardado en `secrets/google_client_id.txt`
- [ ] Client ID agregado en `frontend/.env`
- [ ] Client Secret guardado en `secrets/google_client_secret.txt`

**Verificación:**
- [ ] Maps API test URL carga sin errores

---

## 2️⃣ FIREBASE - Push Notifications

### ¿Por qué lo necesitamos?
- Notificaciones push a navegadores web
- Notificaciones push a apps móviles (futuro)
- Análisis de usuarios (opcional)

### Pasos de configuración:

#### 2.1. Crear proyecto Firebase

1. Ve a https://console.firebase.google.com
2. Clic en "Add project"
3. Nombre del proyecto: `CarDealer Production`
4. Google Analytics: **Deshabilitar** (o habilitar si lo vas a usar)
5. Crear proyecto (tarda ~30 segundos)

#### 2.2. Registrar Web App

1. En el dashboard de Firebase, clic en el ícono **Web** (`</>`)
2. Nickname de la app: `CarDealer Web`
3. **NO** marcar "Set up Firebase Hosting" (por ahora)
4. Registrar app

#### 2.3. Copiar configuración

Verás un código como este:

```javascript
const firebaseConfig = {
  apiKey: "AIzaSyC9x...",
  authDomain: "cardealer-prod.firebaseapp.com",
  projectId: "cardealer-prod-12345",
  storageBucket: "cardealer-prod.appspot.com",
  messagingSenderId: "123456789012",
  appId: "1:123456789012:web:abc123def456",
  measurementId: "G-ABCDEF123"
};
```

#### 2.4. Habilitar Cloud Messaging

1. En el menú lateral: **Project Settings** (ícono engranaje)
2. Pestaña **Cloud Messaging**
3. Scroll hasta "Web configuration"
4. Clic en **Generate key pair** (Web Push certificates)
5. Copia el **Vapid Key** (algo como: `BGt...`)

#### 2.5. Crear Service Account (Backend)

1. En Project Settings → **Service accounts**
2. Clic en "Generate new private key"
3. Descargar el archivo JSON
4. Renombrar a: `firebase-service-account.json`
5. Mover a: `backend/secrets/firebase_service_account.json`

#### 2.6. Guardar configuración

**Frontend `.env`:**

```env
VITE_FIREBASE_API_KEY=AIzaSyC9x...
VITE_FIREBASE_AUTH_DOMAIN=cardealer-prod.firebaseapp.com
VITE_FIREBASE_PROJECT_ID=cardealer-prod-12345
VITE_FIREBASE_STORAGE_BUCKET=cardealer-prod.appspot.com
VITE_FIREBASE_MESSAGING_SENDER_ID=123456789012
VITE_FIREBASE_APP_ID=1:123456789012:web:abc123def456
VITE_FIREBASE_MEASUREMENT_ID=G-ABCDEF123
VITE_FIREBASE_VAPID_KEY=BGt...
```

**Backend `compose.yaml` (verificar que exista este volume):**

```yaml
notificationservice:
  volumes:
    - ./backend/secrets/firebase_service_account.json:/app/firebase_service_account.json:ro
```

### 💰 Costos

- **Plan Spark (Free):**
  - Notificaciones push: **ILIMITADAS** (sí, gratis)
  - 10,000 verificaciones de autenticación/mes
  - 1 GB almacenamiento
- **Plan Blaze (Pay-as-you-go):** Solo pagas lo que usas

### ✅ Checklist

- [ ] Proyecto Firebase creado
- [ ] Web App registrada
- [ ] Config copiada
- [ ] Cloud Messaging habilitado
- [ ] Vapid Key obtenida
- [ ] Service Account JSON descargado
- [ ] JSON movido a `backend/secrets/`
- [ ] Variables guardadas en `.env` frontend
- [ ] Volume configurado en `compose.yaml`

---

## 3️⃣ STRIPE - Pagos y Suscripciones

### ¿Por qué lo necesitamos?
- Procesar pagos con tarjeta de crédito/débito
- Gestionar suscripciones mensuales (Basic, Pro, Premium)
- Webhooks para actualizar estado de suscripciones
- Customer portal para que usuarios gestionen su suscripción

### Pasos de configuración:

#### 3.1. Crear cuenta Stripe

1. Ve a https://dashboard.stripe.com/register
2. Registra tu cuenta con email
3. Completa la verificación básica
4. **Activa el modo Test** (toggle arriba a la derecha)

#### 3.2. Obtener API Keys

1. En el menú lateral: **Developers** → **API keys**
2. Verás dos keys en modo Test:
   - **Publishable key** (frontend): `pk_test_...`
   - **Secret key** (backend): `sk_test_...`

**⚠️ IMPORTANTE:** NUNCA compartas la Secret Key públicamente

#### 3.3. Crear productos y precios

1. En el menú: **Products** → **Add product**

**Plan Basic:**
```
Name: CarDealer Basic
Description: Para dealers pequeños con hasta 50 listings
Pricing: Recurring
Price: $29.00 USD
Billing period: Monthly
Price ID: price_basic_monthly (se genera automáticamente)
```

**Plan Professional:**
```
Name: CarDealer Professional
Description: Para dealers medianos con hasta 200 listings
Pricing: Recurring
Price: $79.00 USD
Billing period: Monthly
Price ID: price_pro_monthly
```

**Plan Premium:**
```
Name: CarDealer Premium
Description: Listings ilimitados + features avanzadas
Pricing: Recurring
Price: $149.00 USD
Billing period: Monthly
Price ID: price_premium_monthly
```

#### 3.4. Configurar Webhook

1. **Developers** → **Webhooks** → **Add endpoint**
2. Endpoint URL: 
   - Development: `http://localhost:15008/api/billing/webhooks/stripe`
   - Production: `https://api.cardealer.app/api/billing/webhooks/stripe`
3. Seleccionar eventos a escuchar:
   - `customer.subscription.created`
   - `customer.subscription.updated`
   - `customer.subscription.deleted`
   - `invoice.payment_succeeded`
   - `invoice.payment_failed`
4. Guardar → Copiar el **Signing secret** (empieza con `whsec_...`)

#### 3.5. Guardar configuración

**Frontend `.env`:**

```env
VITE_STRIPE_PUBLIC_KEY=pk_test_51A...
```

**Backend `secrets/stripe_secret_key.txt`:**

```
sk_test_51A...
```

**Backend `secrets/stripe_webhook_secret.txt`:**

```
whsec_...
```

#### 3.6. Testing con tarjetas de prueba

Stripe provee tarjetas de prueba:

```
Tarjeta exitosa:
4242 4242 4242 4242
Exp: 12/34
CVC: 123
ZIP: 12345

Tarjeta que falla:
4000 0000 0000 0002
```

### 💰 Costos

- **Modo Test:** GRATIS, sin límites
- **Producción:**
  - 2.9% + $0.30 por transacción exitosa
  - Sin costos mensuales fijos
  - Sin setup fee

### ✅ Checklist

- [ ] Cuenta Stripe creada
- [ ] Modo Test activado
- [ ] Publishable key copiada
- [ ] Secret key copiada
- [ ] 3 productos creados (Basic, Pro, Premium)
- [ ] Price IDs copiados
- [ ] Webhook endpoint configurado
- [ ] Webhook secret copiado
- [ ] Keys guardadas en `.env` y `secrets/`
- [ ] Probado con tarjeta de prueba

---

## 4️⃣ SENDGRID - Email Transaccional

### ¿Por qué lo necesitamos?
- Emails de bienvenida
- Confirmación de email
- Recuperación de contraseña
- Notificaciones de actividad
- Facturas y recibos

### Pasos de configuración:

#### 4.1. Crear cuenta SendGrid

1. Ve a https://signup.sendgrid.com
2. Registra tu cuenta
3. Completa la verificación de identidad
4. Plan: **Free** (100 emails/día)

#### 4.2. Verificar dominio o email sender

**Opción A - Single Sender (más rápido):**
1. **Settings** → **Sender Authentication**
2. Clic en "Verify a Single Sender"
3. Completa el formulario:
   - From Email: `noreply@tudominio.com` (o tu email personal para testing)
   - From Name: `CarDealer`
4. Verifica el email que te envían

**Opción B - Domain Authentication (recomendado para producción):**
1. Requiere acceso a DNS de tu dominio
2. Configurar registros DNS (CNAME)
3. Proceso tarda ~48 horas

#### 4.3. Crear API Key

1. **Settings** → **API Keys**
2. Clic en "Create API Key"
3. Name: `CarDealer Backend`
4. Permissions: **Restricted Access**
5. Habilitar solo: **Mail Send** → Full Access
6. Crear y copiar la key (empieza con `SG.`)

**⚠️ IMPORTANTE:** La key solo se muestra UNA vez, guárdala inmediatamente

#### 4.4. Guardar configuración

**Backend `secrets/sendgrid_api_key.txt`:**

```
SG.abc123...
```

**Backend appsettings (verificar):**

```json
{
  "SendGrid": {
    "ApiKey": "will-be-loaded-from-secrets",
    "FromEmail": "noreply@tudominio.com",
    "FromName": "CarDealer"
  }
}
```

#### 4.5. Probar envío

Puedes usar el script de testing o enviar un email de prueba desde NotificationService:

```bash
curl -X POST http://localhost:15084/api/notifications/email/send \
  -H "Content-Type: application/json" \
  -d '{
    "to": "tu@email.com",
    "subject": "Test desde CarDealer",
    "body": "Este es un email de prueba"
  }'
```

### 💰 Costos

- **Free:** 100 emails/día (3,000/mes) - GRATIS
- **Essentials:** $19.95/mes - 50,000 emails/mes
- **Pro:** $89.95/mes - 100,000 emails/mes

### ✅ Checklist

- [ ] Cuenta SendGrid creada
- [ ] Single Sender verificado (email)
- [ ] API Key creada
- [ ] Permissions configuradas (Mail Send only)
- [ ] Key guardada en `secrets/sendgrid_api_key.txt`
- [ ] From Email configurado
- [ ] Email de prueba enviado exitosamente

---

## 5️⃣ TWILIO - SMS (Opcional)

### ¿Por qué lo necesitamos?
- SMS de verificación (2FA)
- Notificaciones urgentes por SMS
- Alertas de actividad importante

**⚠️ NOTA:** Puedes saltarte esto si solo usarás email/push para notificaciones.

### Pasos de configuración:

#### 5.1. Crear cuenta Twilio

1. Ve a https://www.twilio.com/try-twilio
2. Registra tu cuenta
3. Verifica tu número de teléfono
4. Obtendrás **$15 USD de crédito gratis**

#### 5.2. Obtener credenciales

1. En el dashboard de Twilio:
   - **Account SID** (empieza con `AC...`)
   - **Auth Token** (ver/ocultar en el dashboard)

#### 5.3. Obtener un número de teléfono

1. **Phone Numbers** → **Buy a number**
2. Buscar un número con capacidades SMS
3. Comprar número (usa tu crédito gratis)
4. Copiar el número en formato E.164: `+1234567890`

#### 5.4. Guardar configuración

**Backend `secrets/twilio_account_sid.txt`:**

```
ACabc123...
```

**Backend `secrets/twilio_auth_token.txt`:**

```
def456...
```

**Backend `secrets/twilio_phone_number.txt`:**

```
+1234567890
```

### 💰 Costos

- **Free:** $15 USD crédito inicial
- **Después:**
  - $1.00 por número/mes
  - $0.0075 por SMS enviado (EE.UU.)
  - ~2,000 SMS con $15 de crédito

### ✅ Checklist

- [ ] Cuenta Twilio creada
- [ ] Número de teléfono verificado
- [ ] Account SID copiado
- [ ] Auth Token copiado
- [ ] Número de teléfono comprado
- [ ] Credenciales guardadas en `secrets/`
- [ ] SMS de prueba enviado

---

## 6️⃣ AWS S3 - Almacenamiento de Archivos

### ¿Por qué lo necesitamos?
- Almacenar imágenes de vehículos/propiedades
- Almacenar documentos (PDF, contratos)
- CDN para servir assets rápidamente
- Backup de archivos importantes

### Pasos de configuración:

#### 6.1. Crear cuenta AWS

1. Ve a https://aws.amazon.com
2. Clic en "Create an AWS Account"
3. Completa el registro (requiere tarjeta)
4. **Free Tier:** 5 GB de almacenamiento S3 gratis por 12 meses

#### 6.2. Crear un bucket S3

1. En AWS Console, busca **S3**
2. Clic en "Create bucket"
3. Configuración:
   - Bucket name: `cardealer-media-prod` (debe ser único globalmente)
   - Region: `us-east-1` (o tu región más cercana)
   - Block Public Access: **DESACTIVAR** "Block all public access"
   - ⚠️ Confirmar que sabes que el bucket será público
   - Versioning: Habilitar (opcional pero recomendado)
   - Encryption: Habilitar SSE-S3
4. Crear bucket

#### 6.3. Configurar política del bucket (CORS)

1. Selecciona tu bucket
2. Pestaña **Permissions**
3. Scroll hasta **Cross-origin resource sharing (CORS)**
4. Editar y pegar:

```json
[
    {
        "AllowedHeaders": ["*"],
        "AllowedMethods": ["GET", "PUT", "POST", "DELETE"],
        "AllowedOrigins": [
            "http://localhost:5174",
            "http://localhost:5173",
            "https://cardealer.app",
            "https://www.cardealer.app"
        ],
        "ExposeHeaders": ["ETag"]
    }
]
```

#### 6.4. Configurar política de acceso público (lectura)

En **Permissions** → **Bucket policy** → Editar:

```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Sid": "PublicReadGetObject",
            "Effect": "Allow",
            "Principal": "*",
            "Action": "s3:GetObject",
            "Resource": "arn:aws:s3:::cardealer-media-prod/*"
        }
    ]
}
```

#### 6.5. Crear usuario IAM con acceso programático

1. En AWS Console, busca **IAM**
2. **Users** → **Add user**
3. User name: `cardealer-backend`
4. Access type: **Programmatic access** (genera Access Key)
5. Permissions: **Attach policies directly**
6. Buscar y seleccionar: `AmazonS3FullAccess`
7. Crear usuario
8. **⚠️ IMPORTANTE:** Copia el **Access Key ID** y **Secret Access Key**

#### 6.6. Guardar configuración

**Backend `secrets/aws_access_key_id.txt`:**

```
AKIAIOSFODNN7EXAMPLE
```

**Backend `secrets/aws_secret_access_key.txt`:**

```
wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY
```

**Backend `secrets/aws_s3_bucket.txt`:**

```
cardealer-media-prod
```

**Backend `secrets/aws_region.txt`:**

```
us-east-1
```

### 💰 Costos

- **Free Tier (12 meses):**
  - 5 GB almacenamiento
  - 20,000 GET requests
  - 2,000 PUT requests
- **Después:**
  - $0.023 por GB/mes (primeros 50 TB)
  - $0.0004 por 1,000 GET requests
  - **Estimado:** $2-10/mes con uso moderado

### ✅ Checklist

- [ ] Cuenta AWS creada
- [ ] Bucket S3 creado (`cardealer-media-prod`)
- [ ] Public access permitido
- [ ] CORS configurado
- [ ] Bucket policy configurada
- [ ] Usuario IAM creado
- [ ] Access Key ID copiado
- [ ] Secret Access Key copiado
- [ ] Credenciales guardadas en `secrets/`
- [ ] Bucket name y region guardados

---

## 7️⃣ SENTRY - Error Tracking (Opcional)

### ¿Por qué lo necesitamos?
- Capturar errores de JavaScript en producción
- Stack traces detallados
- Alertas cuando ocurren errores críticos
- Performance monitoring

### Pasos rápidos:

1. Ve a https://sentry.io/signup/
2. Crea una cuenta (plan Free: 5,000 eventos/mes)
3. Crea un proyecto → **React**
4. Copia el **DSN** (algo como: `https://abc@123.ingest.sentry.io/456`)
5. Guardar en `.env` frontend:

```env
VITE_SENTRY_DSN=https://abc@123.ingest.sentry.io/456
VITE_SENTRY_ENVIRONMENT=development
```

### ✅ Checklist

- [ ] Cuenta Sentry creada
- [ ] Proyecto React creado
- [ ] DSN copiado
- [ ] DSN guardado en `.env`

---

## 8️⃣ ACTUALIZAR CONFIGURACIÓN

### 8.1. Verificar archivos `.env`

**Frontend `frontend/web/original/.env`:**

Verifica que todas las variables de terceros estén configuradas (no más `placeholder`):

```env
# Google Maps - ✅ Real value
VITE_GOOGLE_MAPS_API_KEY=AIzaSy...

# Firebase - ✅ Real values
VITE_FIREBASE_API_KEY=AIzaSy...
VITE_FIREBASE_AUTH_DOMAIN=cardealer-prod.firebaseapp.com
# ... etc

# Stripe - ✅ Real value
VITE_STRIPE_PUBLIC_KEY=pk_test_...
```

### 8.2. Verificar archivos de secrets

Ejecuta este script para validar:

```powershell
# En la raíz del proyecto
.\scripts\Validate-Secrets.ps1
```

Debe mostrar:

```
✅ jwt_secret_key.txt - OK
✅ google_maps_api_key.txt - OK
✅ firebase_service_account.json - OK
✅ stripe_secret_key.txt - OK
✅ sendgrid_api_key.txt - OK
✅ aws_access_key_id.txt - OK
...
```

### 8.3. Reiniciar servicios

```powershell
# Detener todo
docker-compose down

# Iniciar con nuevos secrets
docker-compose up -d

# Esperar 60 segundos
Start-Sleep -Seconds 60

# Verificar logs
docker-compose logs notificationservice | Select-String -Pattern "SendGrid|Firebase"
```

---

## 9️⃣ PROBAR CONECTIVIDAD

### 9.1. Test Google Maps

Abre en el navegador:

```
http://localhost:5174
```

1. Ve a una página que use mapas (ej: VehicleDetailPage)
2. Verifica que el mapa se carga correctamente
3. Busca en consola si hay errores de Maps API

### 9.2. Test Firebase Push

```bash
# Enviar notificación de prueba
curl -X POST http://localhost:15084/api/notifications/push/send \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "test-user-id",
    "title": "Test Notification",
    "body": "Esta es una notificación de prueba"
  }'
```

### 9.3. Test Stripe

1. Ve a http://localhost:5174/billing
2. Intenta crear una suscripción
3. Usa la tarjeta de prueba: `4242 4242 4242 4242`
4. Verifica que el pago se procesa

### 9.4. Test SendGrid

```bash
curl -X POST http://localhost:15084/api/notifications/email/send \
  -H "Content-Type: application/json" \
  -d '{
    "to": "tu@email.com",
    "subject": "Test desde CarDealer",
    "body": "<h1>Hola</h1><p>Este es un email de prueba.</p>"
  }'
```

Revisa tu bandeja de entrada (también spam).

### 9.5. Test AWS S3

```bash
# Upload de imagen de prueba
curl -X POST http://localhost:15090/api/media/upload \
  -F "file=@path/to/test-image.jpg" \
  -F "category=vehicles"
```

Verifica que la imagen se sube correctamente.

---

## ✅ CHECKLIST FINAL

- [ ] Todos los servicios de terceros configurados
- [ ] Variables de entorno actualizadas (sin placeholders)
- [ ] Archivos de secrets creados y poblados
- [ ] Docker services reiniciados
- [ ] Tests de conectividad pasando
- [ ] Google Maps cargando en frontend
- [ ] Firebase push notifications funcionando
- [ ] Stripe procesando pagos de prueba
- [ ] SendGrid enviando emails
- [ ] AWS S3 almacenando archivos

---

## 🎉 ¡SPRINT 1 COMPLETADO!

Si todos los checkboxes están marcados, ¡felicidades! Has completado exitosamente la configuración de servicios de terceros.

### Próximos pasos:

1. **Sprint 2:** Integración completa de autenticación (JWT, OAuth2)
2. **Sprint 3:** Crear VehicleService y CRUD de vehículos
3. **Sprint 4:** Upload de imágenes y videos

---

## 📝 NOTAS Y TROUBLESHOOTING

### Problema: Google Maps no carga

**Error:** `RefererNotAllowedMapError`

**Solución:**
1. Verifica que el dominio esté en la lista de restricciones de la API Key
2. Asegúrate de usar `http://` o `https://` según corresponda
3. Reinicia el servidor frontend

### Problema: Firebase push no funciona

**Error:** `messaging/unsupported-browser`

**Solución:**
1. Solo funciona en navegadores modernos (Chrome, Firefox, Edge)
2. No funciona en Safari (iOS)
3. Requiere HTTPS en producción

### Problema: Stripe rechaza todas las tarjetas

**Solución:**
1. Verifica que estás en modo **Test**
2. Usa tarjetas de prueba de Stripe (4242...)
3. No uses tarjetas reales en modo Test

---

**Última actualización:** 2 Enero 2026  
**Estado:** En progreso  
**Documentado por:** GitHub Copilot
