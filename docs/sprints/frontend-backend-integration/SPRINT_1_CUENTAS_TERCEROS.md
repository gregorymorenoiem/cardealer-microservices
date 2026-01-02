# 🔑 SPRINT 1 - Cuentas de Servicios de Terceros

**Fecha:** 2 Enero 2026  
**Duración estimada:** 3-4 horas  
**Tokens estimados:** ~22,000  
**Prioridad:** 🔴 CRÍTICO

---

## 🎯 OBJETIVOS

1. Crear cuentas en todos los servicios de terceros necesarios
2. Obtener API Keys y credentials
3. Configurar webhooks y callbacks
4. Documentar límites y costos de cada servicio
5. Actualizar secrets en backend y frontend
6. Probar conectividad con cada servicio

---

## 📋 SERVICIOS A CONFIGURAR

| # | Servicio | Prioridad | Tiempo Est. | Costo Mensual |
|---|----------|-----------|-------------|---------------|
| 1 | Google Cloud Platform | 🔴 Crítico | 30 min | $0 (crédito $200) |
| 2 | Firebase | 🟠 Alta | 20 min | $0 (plan Spark) |
| 3 | Stripe | 🔴 Crítico | 25 min | 2.9% + $0.30/tx |
| 4 | SendGrid | 🟠 Alta | 15 min | $0 (100/día) → $20 |
| 5 | Twilio | 🟡 Media | 15 min | $0 ($15 crédito) |
| 6 | AWS | 🔴 Crítico | 30 min | $0 (Free Tier) → $10 |
| 7 | Sentry | 🟡 Media | 10 min | $0 (5K events/mes) |

**Total:** ~2.5 horas de configuración  
**Costo inicial:** $0 (todos tienen planes gratuitos)  
**Costo proyectado:** $30-50/mes con tráfico moderado

---

## 📝 GUÍAS DE CONFIGURACIÓN

### 1️⃣ Google Cloud Platform (Maps API)

**¿Para qué se usa?**
- Google Maps JavaScript API (mapas interactivos)
- Places API (autocompletado de direcciones)
- Geocoding API (coordenadas de direcciones)
- Directions API (rutas entre ubicaciones)

**Pasos:**

1. **Crear cuenta Google Cloud**
   - Ir a https://console.cloud.google.com
   - Crear nueva cuenta (requiere tarjeta, pero no cobra)
   - Obtener $200 de crédito gratis por 90 días

2. **Crear proyecto**
   ```
   Nombre: CarDealer Production
   ID: cardealer-prod-2026
   ```

3. **Habilitar APIs necesarias**
   - API Library → Buscar y habilitar:
     - ✅ Maps JavaScript API
     - ✅ Places API
     - ✅ Geocoding API
     - ✅ Directions API (opcional)

4. **Crear API Key**
   - Credentials → Create Credentials → API Key
   - Nombre: `CarDealer Web Frontend`
   
5. **Restringir API Key (IMPORTANTE)**
   ```
   Application restrictions:
   - HTTP referrers (web sites)
   - Website restrictions:
     - http://localhost:5174/*
     - http://localhost:5173/*
     - https://cardealer.app/*
     - https://www.cardealer.app/*
   
   API restrictions:
   - Restrict key
   - Select APIs:
     ✅ Maps JavaScript API
     ✅ Places API
     ✅ Geocoding API
   ```

6. **Obtener API Key**
   ```
   Ejemplo: AIzaSyDKVgKqLUzWFaEMcXjkZUUTOFDNa4V0AFI
   ```

7. **Actualizar configuración**
   
   **Frontend `.env`:**
   ```env
   VITE_GOOGLE_MAPS_API_KEY=AIzaSy...tu_key_real
   ```

   **Backend `compose.secrets.yaml`:**
   ```yaml
   GOOGLE_MAPS_API_KEY: "AIzaSy...tu_key_real"
   ```

**Límites del plan gratuito:**
- $200 USD crédito mensual
- Después: $7 por 1,000 cargas de mapa
- $17 por 1,000 requests de Places
- Suficiente para ~28,000 cargas de mapa/mes

**Validación:**
```bash
# Test API Key
curl "https://maps.googleapis.com/maps/api/js?key=AIzaSy...tu_key&callback=initMap"
```

---

### 2️⃣ Firebase (Push Notifications)

**¿Para qué se usa?**
- Firebase Cloud Messaging (notificaciones push)
- Firebase Authentication (OAuth alternativo - opcional)
- Firebase Analytics (métricas - opcional)

**Pasos:**

1. **Crear proyecto Firebase**
   - Ir a https://console.firebase.google.com
   - Add Project → `CarDealer Production`
   - Disable Google Analytics (o enable si lo vas a usar)

2. **Registrar app web**
   - Add app → Web (icono </>)
   - Nickname: `CarDealer Web`
   - ✅ Also set up Firebase Hosting (opcional)

3. **Obtener config**
   ```javascript
   const firebaseConfig = {
     apiKey: "AIzaSyC...",
     authDomain: "cardealer-prod.firebaseapp.com",
     projectId: "cardealer-prod",
     storageBucket: "cardealer-prod.appspot.com",
     messagingSenderId: "123456789",
     appId: "1:123456789:web:abcdef",
     measurementId: "G-ABCDEF123"
   };
   ```

4. **Habilitar Cloud Messaging**
   - Project Settings → Cloud Messaging
   - Generar Web Push certificate (Keypair)
   - Copiar Server Key

5. **Crear Service Account para backend**
   - Project Settings → Service Accounts
   - Generate new private key
   - Guardar archivo JSON: `backend/firebase-dev-key.json`

6. **Actualizar configuración**

   **Frontend `.env`:**
   ```env
   VITE_FIREBASE_API_KEY=AIzaSyC...
   VITE_FIREBASE_AUTH_DOMAIN=cardealer-prod.firebaseapp.com
   VITE_FIREBASE_PROJECT_ID=cardealer-prod
   VITE_FIREBASE_STORAGE_BUCKET=cardealer-prod.appspot.com
   VITE_FIREBASE_MESSAGING_SENDER_ID=123456789
   VITE_FIREBASE_APP_ID=1:123456789:web:abcdef
   ```

   **Backend `compose.secrets.yaml`:**
   ```yaml
   FIREBASE_PROJECT_ID: "cardealer-prod"
   FIREBASE_PRIVATE_KEY_PATH: "/app/firebase-dev-key.json"
   ```

   **Docker compose.yaml** (agregar volume):
   ```yaml
   notificationservice:
     volumes:
       - ./backend/firebase-dev-key.json:/app/firebase-dev-key.json:ro
   ```

**Límites del plan Spark (Free):**
- 10K verificaciones de autenticación/mes
- Notificaciones push ilimitadas (sí, gratis)
- 1 GB almacenamiento
- 10 GB transferencia/mes

**Validación:**
```bash
# Test con curl (desde backend)
curl -X POST https://fcm.googleapis.com/fcm/send \
  -H "Authorization: key=YOUR_SERVER_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "to": "device_token",
    "notification": {
      "title": "Test",
      "body": "Hello from CarDealer"
    }
  }'
```

---

### 3️⃣ Stripe (Pagos y Suscripciones)

**¿Para qué se usa?**
- Procesamiento de pagos con tarjeta
- Gestión de suscripciones recurrentes
- Webhooks para eventos de pago
- Customer portal para gestionar suscripciones

**Pasos:**

1. **Crear cuenta Stripe**
   - Ir a https://dashboard.stripe.com/register
   - Registrar con email business
   - Completar verificación (puede tardar)

2. **Activar modo Test**
   - Dashboard → Developers → Toggle "Test mode"
   - Todas las pruebas usarán datos ficticios

3. **Obtener API Keys**
   - Developers → API Keys
   - Copiar:
     - **Publishable key** (frontend): `pk_test_...`
     - **Secret key** (backend): `sk_test_...`

4. **Crear Productos y Precios**
   
   **Plan Basic:**
   ```
   Product: CarDealer Basic
   Price: $29/month
   ID: price_basic_monthly
   ```

   **Plan Professional:**
   ```
   Product: CarDealer Professional
   Price: $79/month
   ID: price_pro_monthly
   ```

   **Plan Enterprise:**
   ```
   Product: CarDealer Enterprise
   Price: $199/month
   ID: price_enterprise_monthly
   ```

5. **Configurar Webhooks**
   - Developers → Webhooks → Add endpoint
   - Endpoint URL: `https://yourdomain.com/api/billing/webhooks/stripe`
   - Events to send:
     ```
     ✅ payment_intent.succeeded
     ✅ payment_intent.payment_failed
     ✅ invoice.paid
     ✅ invoice.payment_failed
     ✅ customer.subscription.created
     ✅ customer.subscription.updated
     ✅ customer.subscription.deleted
     ```
   - Copiar Signing secret: `whsec_...`

6. **Actualizar configuración**

   **Frontend `.env`:**
   ```env
   VITE_STRIPE_PUBLIC_KEY=pk_test_tu_publishable_key
   VITE_ENABLE_STRIPE_PAYMENTS=true
   ```

   **Backend `compose.secrets.yaml`:**
   ```yaml
   STRIPE_SECRET_KEY: "sk_test_tu_secret_key"
   STRIPE_WEBHOOK_SECRET: "whsec_tu_webhook_secret"
   
   # Price IDs
   STRIPE_PRICE_BASIC: "price_basic_monthly"
   STRIPE_PRICE_PRO: "price_pro_monthly"
   STRIPE_PRICE_ENTERPRISE: "price_enterprise_monthly"
   ```

**Tarjetas de prueba:**
```
Visa exitosa:     4242 4242 4242 4242
Mastercard:       5555 5555 5555 4444
Pago declined:    4000 0000 0000 0002
3D Secure:        4000 0027 6000 3184

CVV: cualquier 3 dígitos
Fecha: cualquier fecha futura
```

**Costos:**
- Sin costo mensual
- 2.9% + $0.30 por transacción exitosa
- Sin cargos por transacciones fallidas

**Validación:**
```bash
# Test API
curl https://api.stripe.com/v1/customers \
  -u sk_test_tu_secret_key: \
  -d "email=test@example.com"
```

---

### 4️⃣ SendGrid (Email Transaccional)

**¿Para qué se usa?**
- Emails de bienvenida
- Verificación de email
- Recuperación de contraseña
- Notificaciones por email
- Alertas de nuevos vehículos

**Pasos:**

1. **Crear cuenta SendGrid**
   - Ir a https://signup.sendgrid.com
   - Plan Free (100 emails/día)

2. **Verificar dominio**
   - Settings → Sender Authentication
   - Authenticate Your Domain
   - Agregar registros DNS:
     ```
     CNAME s1._domainkey.cardealer.app → s1.domainkey.u123456.wl.sendgrid.net
     CNAME s2._domainkey.cardealer.app → s2.domainkey.u123456.wl.sendgrid.net
     ```
   - Si no tienes dominio aún: Usar Single Sender Verification
     ```
     From Email: noreply@tudominio.com
     From Name: CarDealer
     ```

3. **Crear API Key**
   - Settings → API Keys → Create API Key
   - Name: `CarDealer Backend Production`
   - Permissions: Full Access
   - Copiar key: `SG.abc123...` (solo se muestra una vez)

4. **Crear templates (opcional)**
   - Email API → Dynamic Templates
   - Templates útiles:
     - Welcome Email
     - Password Reset
     - Email Verification
     - New Vehicle Alert

5. **Actualizar configuración**

   **Backend `compose.secrets.yaml`:**
   ```yaml
   SENDGRID_API_KEY: "SG.abc123..."
   SENDGRID_FROM_EMAIL: "noreply@cardealer.app"
   SENDGRID_FROM_NAME: "CarDealer"
   
   # Template IDs (si los creaste)
   SENDGRID_TEMPLATE_WELCOME: "d-template_id_1"
   SENDGRID_TEMPLATE_PASSWORD_RESET: "d-template_id_2"
   ```

**Límites plan Free:**
- 100 emails/día (3,000/mes)
- Sin límite de destinatarios
- Tracking básico

**Plan Essentials ($20/mes):**
- 50,000 emails/mes
- Email validation
- Advanced analytics

**Validación:**
```bash
# Test envío
curl -X POST https://api.sendgrid.com/v3/mail/send \
  -H "Authorization: Bearer SG.abc123..." \
  -H "Content-Type: application/json" \
  -d '{
    "personalizations": [{
      "to": [{"email": "test@example.com"}]
    }],
    "from": {"email": "noreply@cardealer.app"},
    "subject": "Test Email",
    "content": [{
      "type": "text/plain",
      "value": "Hello from CarDealer"
    }]
  }'
```

---

### 5️⃣ Twilio (SMS Notifications)

**¿Para qué se usa?**
- Verificación de teléfono (2FA)
- Alertas por SMS
- Notificaciones urgentes

**Pasos:**

1. **Crear cuenta Twilio**
   - Ir a https://www.twilio.com/try-twilio
   - Registrar (gratis con $15 crédito)

2. **Verificar teléfono**
   - Verify your phone number
   - Ingresar tu número real para testing

3. **Obtener credenciales**
   - Console Dashboard
   - Copiar:
     - Account SID: `ACxxxx...`
     - Auth Token: `xxx...` (click Show)

4. **Obtener número Twilio**
   - Phone Numbers → Get a Number
   - Seleccionar país y tipo (Voice + SMS)
   - Número ejemplo: `+1 555-123-4567`

5. **Configurar Webhooks (opcional)**
   - Para SMS entrantes
   - URL: `https://yourdomain.com/api/notifications/webhooks/twilio`

6. **Actualizar configuración**

   **Backend `compose.secrets.yaml`:**
   ```yaml
   TWILIO_ACCOUNT_SID: "ACxxxx..."
   TWILIO_AUTH_TOKEN: "xxx..."
   TWILIO_PHONE_NUMBER: "+15551234567"
   ```

**Costos:**
- $15 crédito inicial
- SMS USA: $0.0075 por mensaje
- SMS internacional: $0.03-0.15
- Número teléfono: $1/mes

**Validación:**
```bash
# Test SMS
curl -X POST "https://api.twilio.com/2010-04-01/Accounts/ACxxxx.../Messages.json" \
  --data-urlencode "From=+15551234567" \
  --data-urlencode "To=+1234567890" \
  --data-urlencode "Body=Test from CarDealer" \
  -u ACxxxx...:auth_token
```

---

### 6️⃣ AWS S3 (Almacenamiento de Imágenes)

**¿Para qué se usa?**
- Almacenar imágenes de vehículos
- Archivos adjuntos
- Assets estáticos
- Backups

**Pasos:**

1. **Crear cuenta AWS**
   - Ir a https://aws.amazon.com
   - Create Free Tier Account
   - Requiere tarjeta (no cobra en Free Tier)

2. **Crear usuario IAM**
   - IAM → Users → Add User
   - Username: `cardealer-s3-uploader`
   - Access type: ✅ Programmatic access
   - Permissions: Attach existing policies
     - ✅ AmazonS3FullAccess (o crear política restrictiva)
   - Copiar:
     - Access Key ID: `AKIAIOSFODNN7EXAMPLE`
     - Secret Access Key: `wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY`

3. **Crear bucket S3**
   - S3 → Create bucket
   - Configuración:
     ```
     Bucket name: cardealer-images-prod
     Region: us-east-1 (o tu región preferida)
     Block all public access: ❌ (desmarcar)
     Bucket Versioning: Disabled
     Default encryption: Enable (AES-256)
     ```

4. **Configurar CORS**
   - Bucket → Permissions → CORS
   ```json
   [
     {
       "AllowedOrigins": [
         "http://localhost:5174",
         "http://localhost:5173",
         "https://cardealer.app"
       ],
       "AllowedMethods": ["GET", "PUT", "POST", "DELETE"],
       "AllowedHeaders": ["*"],
       "ExposeHeaders": ["ETag"],
       "MaxAgeSeconds": 3000
     }
   ]
   ```

5. **Configurar Bucket Policy**
   - Permissions → Bucket Policy
   ```json
   {
     "Version": "2012-10-17",
     "Statement": [
       {
         "Sid": "PublicReadGetObject",
         "Effect": "Allow",
         "Principal": "*",
         "Action": "s3:GetObject",
         "Resource": "arn:aws:s3:::cardealer-images-prod/*"
       }
     ]
   }
   ```

6. **Configurar CloudFront (opcional, para CDN)**
   - CloudFront → Create Distribution
   - Origin: `cardealer-images-prod.s3.amazonaws.com`
   - Copiar Distribution Domain Name: `d123abc.cloudfront.net`

7. **Actualizar configuración**

   **Backend `compose.secrets.yaml`:**
   ```yaml
   AWS_ACCESS_KEY_ID: "AKIAIOSFODNN7EXAMPLE"
   AWS_SECRET_ACCESS_KEY: "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY"
   AWS_REGION: "us-east-1"
   AWS_BUCKET_NAME: "cardealer-images-prod"
   AWS_CLOUDFRONT_URL: "https://d123abc.cloudfront.net" # opcional
   ```

**Costos Free Tier (primer año):**
- 5 GB storage
- 20,000 GET requests
- 2,000 PUT requests
- 100 GB transferencia salida

**Después del Free Tier:**
- $0.023/GB storage
- $0.0004 por 1,000 GET
- $0.005 por 1,000 PUT

**Validación:**
```bash
# Test upload (requiere AWS CLI)
aws s3 cp test.jpg s3://cardealer-images-prod/test.jpg
aws s3 ls s3://cardealer-images-prod/
```

---

### 7️⃣ Sentry (Error Tracking - Opcional)

**¿Para qué se usa?**
- Tracking de errores en producción
- Performance monitoring
- Alertas de errores críticos

**Pasos:**

1. **Crear cuenta Sentry**
   - Ir a https://sentry.io/signup
   - Plan Developer (gratis, 5K events/mes)

2. **Crear proyecto**
   - Create Project
   - Platform: React
   - Name: `CarDealer Frontend`

3. **Obtener DSN**
   ```
   https://abc123@o123.ingest.sentry.io/456
   ```

4. **Crear segundo proyecto para Backend**
   - Platform: .NET
   - Name: `CarDealer Backend`
   - DSN: `https://def456@o123.ingest.sentry.io/789`

5. **Actualizar configuración**

   **Frontend `.env`:**
   ```env
   VITE_SENTRY_DSN=https://abc123@o123.ingest.sentry.io/456
   VITE_SENTRY_ENVIRONMENT=development
   ```

   **Backend `compose.secrets.yaml`:**
   ```yaml
   SENTRY_DSN: "https://def456@o123.ingest.sentry.io/789"
   SENTRY_ENVIRONMENT: "development"
   ```

**Límites plan Developer:**
- 5,000 errors/mes
- 30 días retención
- 1 miembro del equipo

**Validación:**
```javascript
// Frontend - disparar error de prueba
Sentry.captureException(new Error("Test error from CarDealer"));
```

---

## 📊 RESUMEN DE CONFIGURACIÓN

### Checklist de Secrets Configurados

- [ ] `GOOGLE_MAPS_API_KEY` - Frontend y Backend
- [ ] `FIREBASE_*` - 7 variables en Frontend
- [ ] `FIREBASE_PROJECT_ID` y `FIREBASE_PRIVATE_KEY_PATH` - Backend
- [ ] `STRIPE_PUBLIC_KEY` - Frontend
- [ ] `STRIPE_SECRET_KEY` y `STRIPE_WEBHOOK_SECRET` - Backend
- [ ] `SENDGRID_API_KEY` - Backend
- [ ] `TWILIO_ACCOUNT_SID`, `TWILIO_AUTH_TOKEN` - Backend
- [ ] `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY` - Backend
- [ ] `SENTRY_DSN` - Frontend y Backend (opcional)

### Archivo Frontend `.env` Final

```env
# APIs de Terceros
VITE_GOOGLE_MAPS_API_KEY=AIzaSy...real_key
VITE_STRIPE_PUBLIC_KEY=pk_test_...real_key

# Firebase
VITE_FIREBASE_API_KEY=AIzaSyC...real_key
VITE_FIREBASE_AUTH_DOMAIN=cardealer-prod.firebaseapp.com
VITE_FIREBASE_PROJECT_ID=cardealer-prod
VITE_FIREBASE_STORAGE_BUCKET=cardealer-prod.appspot.com
VITE_FIREBASE_MESSAGING_SENDER_ID=123456789
VITE_FIREBASE_APP_ID=1:123456789:web:abcdef
VITE_FIREBASE_MEASUREMENT_ID=G-ABCDEF123

# Sentry (opcional)
VITE_SENTRY_DSN=https://abc@sentry.io/123
VITE_SENTRY_ENVIRONMENT=development

# Feature Flags
VITE_ENABLE_STRIPE_PAYMENTS=true
VITE_ENABLE_PUSH_NOTIFICATIONS=true
```

### Archivo Backend `compose.secrets.yaml` Final

```yaml
# Google
GOOGLE_MAPS_API_KEY: "AIzaSy...real_key"

# Firebase
FIREBASE_PROJECT_ID: "cardealer-prod"
FIREBASE_PRIVATE_KEY_PATH: "/app/firebase-dev-key.json"

# Stripe
STRIPE_SECRET_KEY: "sk_test_...real_key"
STRIPE_WEBHOOK_SECRET: "whsec_...real_secret"
STRIPE_PRICE_BASIC: "price_basic_monthly"
STRIPE_PRICE_PRO: "price_pro_monthly"
STRIPE_PRICE_ENTERPRISE: "price_enterprise_monthly"

# SendGrid
SENDGRID_API_KEY: "SG...real_key"
SENDGRID_FROM_EMAIL: "noreply@cardealer.app"
SENDGRID_FROM_NAME: "CarDealer"

# Twilio
TWILIO_ACCOUNT_SID: "ACxxxx...real_sid"
TWILIO_AUTH_TOKEN: "xxx...real_token"
TWILIO_PHONE_NUMBER: "+15551234567"

# AWS S3
AWS_ACCESS_KEY_ID: "AKIA...real_key"
AWS_SECRET_ACCESS_KEY: "wJalrX...real_secret"
AWS_REGION: "us-east-1"
AWS_BUCKET_NAME: "cardealer-images-prod"

# Sentry (opcional)
SENTRY_DSN: "https://def@sentry.io/789"
SENTRY_ENVIRONMENT: "development"
```

---

## ✅ VALIDACIÓN COMPLETA

### Script de Validación

**Archivo:** `scripts/validate-third-party-apis.sh`

```bash
#!/bin/bash

echo "🔍 Validando APIs de Terceros..."

# Google Maps
echo "Testing Google Maps API..."
GMAPS_KEY=$(grep VITE_GOOGLE_MAPS_API_KEY frontend/web/original/.env | cut -d '=' -f2)
curl -s "https://maps.googleapis.com/maps/api/js?key=$GMAPS_KEY" | grep -q "initMap" && echo "✅ Google Maps" || echo "❌ Google Maps"

# Stripe
echo "Testing Stripe API..."
STRIPE_KEY=$(grep STRIPE_SECRET_KEY compose.secrets.yaml | cut -d ':' -f2 | tr -d ' "')
curl -s -u "$STRIPE_KEY:" https://api.stripe.com/v1/customers | grep -q "object" && echo "✅ Stripe" || echo "❌ Stripe"

# SendGrid
echo "Testing SendGrid API..."
SENDGRID_KEY=$(grep SENDGRID_API_KEY compose.secrets.yaml | cut -d ':' -f2 | tr -d ' "')
curl -s -H "Authorization: Bearer $SENDGRID_KEY" https://api.sendgrid.com/v3/user/profile | grep -q "email" && echo "✅ SendGrid" || echo "❌ SendGrid"

# AWS S3
echo "Testing AWS S3..."
aws s3 ls 2>&1 | grep -q "cardealer-images" && echo "✅ AWS S3" || echo "❌ AWS S3"

echo "✅ Validación completada"
```

---

## 📈 COSTOS PROYECTADOS

### Breakdown Mensual (tráfico bajo-medio)

| Servicio | Free Tier | Uso Estimado | Costo |
|----------|-----------|--------------|-------|
| Google Maps | $200 crédito | 10K cargas/mes | $0 |
| Firebase | Ilimitado | Push notifications | $0 |
| Stripe | N/A | 100 transacciones | ~$30 |
| SendGrid | 100/día | 2K emails/mes | $0 |
| Twilio | $15 crédito | 50 SMS/mes | $0 |
| AWS S3 | 5GB | 2GB storage | $0 |
| Sentry | 5K events | 3K errors/mes | $0 |
| **TOTAL** | - | - | **~$30/mes** |

### Costos con Tráfico Alto (1K usuarios activos)

| Servicio | Uso | Costo Mensual |
|----------|-----|---------------|
| Google Maps | 50K cargas | $50 |
| Firebase | Push ilimitado | $0 |
| Stripe | 500 transacciones | $150 |
| SendGrid | 20K emails | $20 |
| Twilio | 500 SMS | $4 |
| AWS S3 | 50GB | $1 |
| Sentry | 20K events | $26 |
| **TOTAL** | - | **~$251/mes** |

---

## ➡️ PRÓXIMO SPRINT

**Sprint 2:** [SPRINT_2_AUTH_INTEGRATION.md](SPRINT_2_AUTH_INTEGRATION.md)

Integrar completamente el AuthService con el frontend:
- Login/Registro
- OAuth2 (Google, Microsoft)
- JWT token management
- Refresh token flow
- Perfil de usuario

---

**Estado:** ⚪ Pendiente  
**Última actualización:** 2 Enero 2026
