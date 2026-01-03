# ✅ SPRINT 1 - CHECKLIST INTERACTIVO

**Fecha inicio:** _________  
**Fecha fin:** _________  
**Tiempo invertido:** _____ horas

---

## 📌 INSTRUCCIONES

1. Lee primero [SPRINT_1_SETUP_GUIDE.md](SPRINT_1_SETUP_GUIDE.md) para instrucciones detalladas
2. Marca con ✅ cada tarea que completes
3. Anota las API keys en un lugar seguro (LastPass, 1Password, etc.)
4. Al terminar, ejecuta `scripts/Validate-Secrets.ps1` para verificar

---

## 🎯 SERVICIOS A CONFIGURAR (3-4 horas)

### 1️⃣ Google Cloud Platform ⏱️ 30 min
- [ ] **1.1** Crear cuenta en https://console.cloud.google.com
- [ ] **1.2** Crear proyecto "CarDealer Production"
- [ ] **1.3** Habilitar APIs:
  - [ ] Maps JavaScript API
  - [ ] Places API  
  - [ ] Geocoding API
  - [ ] Google+ API (OAuth)
- [ ] **1.4** Crear API Key para Maps
  - [ ] Restringir a localhost + dominio producción
  - [ ] Copiar key: `AIzaSy...`
- [ ] **1.5** Crear OAuth 2.0 Client ID
  - [ ] Configurar OAuth Consent Screen
  - [ ] Crear credenciales Web Application
  - [ ] Copiar Client ID: `...googleusercontent.com`
  - [ ] Copiar Client Secret: `GOCSPX-...`
- [ ] **1.6** Actualizar secrets:
  ```bash
  echo "AIzaSy..." > secrets/google_maps_api_key.txt
  echo "...googleusercontent.com" > secrets/google_oauth_client_id.txt
  echo "GOCSPX-..." > secrets/google_oauth_client_secret.txt
  ```

**📝 Notas:**
```
Google Maps API Key: ______________________________________
OAuth Client ID: __________________________________________
OAuth Client Secret: ______________________________________
```

---

### 2️⃣ Firebase (Push Notifications) ⏱️ 20 min
- [ ] **2.1** Ir a https://console.firebase.google.com
- [ ] **2.2** Crear proyecto o usar el de Google Cloud
- [ ] **2.3** Agregar app Web (para frontend)
- [ ] **2.4** Copiar Firebase Config (firebaseConfig object)
- [ ] **2.5** Habilitar Cloud Messaging (FCM)
- [ ] **2.6** Crear Service Account
  - [ ] Project Settings → Service Accounts
  - [ ] Generate new private key (JSON)
  - [ ] Descargar archivo JSON
- [ ] **2.7** Actualizar secrets:
  ```bash
  cp ~/Downloads/cardealer-*.json secrets/firebase_service_account.json
  ```

**📝 Notas:**
```
Firebase Project ID: ______________________________________
```

---

### 3️⃣ Stripe (Pagos) ⏱️ 25 min
- [ ] **3.1** Crear cuenta en https://stripe.com
- [ ] **3.2** Activar modo Test (toggle arriba a la derecha)
- [ ] **3.3** Ir a Developers → API keys
- [ ] **3.4** Copiar Publishable key (pk_test_...)
- [ ] **3.5** Copiar Secret key (sk_test_...)
- [ ] **3.6** Ir a Developers → Webhooks
- [ ] **3.7** Crear webhook endpoint:
  ```
  URL: http://localhost:15008/api/billing/webhook
  Events: payment_intent.succeeded, customer.subscription.*
  ```
- [ ] **3.8** Copiar Webhook Secret (whsec_...)
- [ ] **3.9** Actualizar secrets:
  ```bash
  echo "pk_test_..." > secrets/stripe_publishable_key.txt
  echo "sk_test_..." > secrets/stripe_secret_key.txt
  echo "whsec_..." > secrets/stripe_webhook_secret.txt
  ```

**📝 Notas:**
```
Stripe Publishable Key: ___________________________________
Stripe Secret Key: ________________________________________
Stripe Webhook Secret: ____________________________________
```

---

### 4️⃣ SendGrid (Email) ⏱️ 15 min
🟠 **OPCIONAL** - Puedes usar Resend o SendGrid

#### Opción A: SendGrid (gratis 100 emails/día)
- [ ] **4.1** Crear cuenta en https://sendgrid.com
- [ ] **4.2** Verificar email
- [ ] **4.3** Settings → API Keys → Create API Key
- [ ] **4.4** Nombre: `CarDealer Development`
- [ ] **4.5** Permisos: Full Access
- [ ] **4.6** Copiar API Key (empieza con SG.)
- [ ] **4.7** Actualizar secrets:
  ```bash
  echo "SG...." > secrets/sendgrid_api_key.txt
  ```

#### Opción B: Resend (gratis 3000 emails/mes)
- [ ] **4.1** Crear cuenta en https://resend.com
- [ ] **4.2** API Keys → Create API Key
- [ ] **4.3** Copiar key (re_...)
- [ ] **4.4** Actualizar secrets:
  ```bash
  echo "re_..." > secrets/resend_api_key.txt
  ```

**📝 Notas:**
```
Email Provider: [SendGrid / Resend]
API Key: __________________________________________________
```

---

### 5️⃣ Twilio (SMS) ⏱️ 15 min
🟡 **OPCIONAL** - Solo si necesitas SMS

- [ ] **5.1** Crear cuenta en https://twilio.com
- [ ] **5.2** Verificar teléfono
- [ ] **5.3** Crear proyecto
- [ ] **5.4** Dashboard → Account Info
- [ ] **5.5** Copiar Account SID (AC...)
- [ ] **5.6** Copiar Auth Token
- [ ] **5.7** Phone Numbers → Get a number (gratis trial)
- [ ] **5.8** Copiar Phone Number (+1...)
- [ ] **5.9** Actualizar secrets:
  ```bash
  echo "AC..." > secrets/twilio_account_sid.txt
  echo "..." > secrets/twilio_auth_token.txt
  echo "+1..." > secrets/twilio_phone_number.txt
  ```

**📝 Notas:**
```
Twilio Account SID: _______________________________________
Twilio Auth Token: ________________________________________
Twilio Phone Number: ______________________________________
```

---

### 6️⃣ AWS S3 (Almacenamiento) ⏱️ 30 min
- [ ] **6.1** Crear cuenta en https://aws.amazon.com
- [ ] **6.2** Ir a IAM → Users → Add users
- [ ] **6.3** Nombre: `cardealer-app`
- [ ] **6.4** Access type: Programmatic access
- [ ] **6.5** Permissions: Attach existing policies → `AmazonS3FullAccess`
- [ ] **6.6** Copiar Access Key ID (AKIA...)
- [ ] **6.7** Copiar Secret Access Key
- [ ] **6.8** Ir a S3 → Create bucket
  ```
  Bucket name: cardealer-media-dev
  Region: us-east-1
  Block all public access: OFF
  ACLs: Enabled
  ```
- [ ] **6.9** Bucket → Permissions → CORS Configuration:
  ```json
  [
    {
      "AllowedHeaders": ["*"],
      "AllowedMethods": ["GET", "PUT", "POST", "DELETE"],
      "AllowedOrigins": ["http://localhost:5174"],
      "ExposeHeaders": ["ETag"]
    }
  ]
  ```
- [ ] **6.10** Actualizar secrets:
  ```bash
  echo "AKIA..." > secrets/aws_access_key_id.txt
  echo "..." > secrets/aws_secret_access_key.txt
  echo "cardealer-media-dev" > secrets/aws_s3_bucket.txt
  echo "us-east-1" > secrets/aws_region.txt
  ```

**📝 Notas:**
```
AWS Access Key ID: ________________________________________
AWS Secret Access Key: ____________________________________
AWS S3 Bucket: ____________________________________________
AWS Region: _______________________________________________
```

---

### 7️⃣ Sentry (Error Tracking) ⏱️ 10 min
🟡 **OPCIONAL** - Útil para debugging

- [ ] **7.1** Crear cuenta en https://sentry.io
- [ ] **7.2** Crear proyecto:
  ```
  Platform: ASP.NET Core
  Name: CarDealer Backend
  ```
- [ ] **7.3** Copiar DSN (https://...@o...ingest.sentry.io/...)
- [ ] **7.4** Crear otro proyecto para frontend:
  ```
  Platform: React
  Name: CarDealer Frontend
  ```
- [ ] **7.5** Copiar DSN del frontend
- [ ] **7.6** Actualizar secrets:
  ```bash
  echo "https://...backend..." > secrets/sentry_dsn_backend.txt
  echo "https://...frontend..." > secrets/sentry_dsn_frontend.txt
  ```

**📝 Notas:**
```
Sentry DSN Backend: _______________________________________
Sentry DSN Frontend: ______________________________________
```

---

## 8️⃣ ACTUALIZAR CONFIGURACIÓN ⏱️ 15 min

- [ ] **8.1** Actualizar `compose.secrets.yaml`:
  ```bash
  # El script lo hace automáticamente si pusiste los archivos en secrets/
  ./scripts/Update-ComposeSecrets.ps1
  ```

- [ ] **8.2** Actualizar Frontend `.env`:
  ```bash
  cd frontend/web
  cp .env.example .env
  # Editar .env con tus keys reales
  ```

- [ ] **8.3** Verificar Backend `appsettings.json`:
  ```bash
  # Verificar que los servicios estén configurados para leer desde secrets
  grep -r "Jwt__Key" backend/*/appsettings.json
  ```

---

## 9️⃣ VALIDAR CONFIGURACIÓN ⏱️ 10 min

- [ ] **9.1** Ejecutar script de validación:
  ```bash
  ./scripts/Validate-Secrets.ps1
  ```

- [ ] **9.2** Revisar resultados:
  - [ ] ✅ Todos los secrets críticos tienen valores válidos
  - [ ] ✅ JWT key tiene mínimo 32 caracteres
  - [ ] ✅ Google Maps API key empieza con `AIzaSy`
  - [ ] ✅ Stripe keys empiezan con `sk_test_` / `pk_test_`
  - [ ] ✅ AWS keys empiezan con `AKIA`

- [ ] **9.3** Probar conectividad (opcional):
  ```bash
  ./scripts/Test-Third-Party-Services.ps1
  ```

- [ ] **9.4** Levantar servicios con secrets:
  ```bash
  docker-compose -f compose.yaml -f compose.secrets.yaml up -d
  ```

- [ ] **9.5** Verificar logs:
  ```bash
  docker logs authservice | grep "JWT"
  docker logs mediaservice | grep "S3"
  docker logs billingservice | grep "Stripe"
  ```

---

## 📊 RESUMEN DE PROGRESO

```
Total servicios: 7
Completados: ___ / 7
Críticos completados: ___ / 4 (Google Maps, AWS S3, Stripe, SendGrid)

Tiempo invertido: _____ horas
Estado: [ EN PROGRESO / COMPLETADO ]
```

---

## ✅ CRITERIOS DE ÉXITO

Sprint 1 se considera **COMPLETO** cuando:

- ✅ Al menos los 4 servicios CRÍTICOS están configurados:
  - Google Maps API Key
  - AWS S3 (Access Key + Bucket)
  - Stripe (Secret Key)
  - SendGrid o Resend (API Key)

- ✅ Todos los secrets están en `secrets/*.txt`

- ✅ `./scripts/Validate-Secrets.ps1` pasa sin errores críticos

- ✅ Frontend `.env` tiene las keys correctas

- ✅ Los servicios Docker pueden leer los secrets

---

## 🚨 TROUBLESHOOTING

### Google Maps no carga en frontend
```bash
# Verificar que la key esté en .env
grep VITE_GOOGLE_MAPS_API_KEY frontend/web/.env

# Verificar que la key tenga permisos
curl "https://maps.googleapis.com/maps/api/js?key=TU_KEY&libraries=places"
```

### Stripe webhooks fallan
```bash
# En desarrollo, usar Stripe CLI para forward
stripe login
stripe listen --forward-to localhost:15008/api/billing/webhook
```

### AWS S3 uploads fallan con CORS
```bash
# Verificar CORS config del bucket
aws s3api get-bucket-cors --bucket cardealer-media-dev
```

### Firebase push notifications no llegan
```bash
# Verificar que el service account JSON esté en secrets/
ls -lh secrets/firebase_service_account.json

# Verificar permisos del archivo
cat secrets/firebase_service_account.json | jq .type
# Debe mostrar: "service_account"
```

---

## 📚 REFERENCIAS

- [SPRINT_1_SETUP_GUIDE.md](SPRINT_1_SETUP_GUIDE.md) - Guía detallada paso a paso
- [SPRINT_1_CUENTAS_TERCEROS.md](SPRINT_1_CUENTAS_TERCEROS.md) - Documentación completa
- [scripts/Validate-Secrets.ps1](../../scripts/Validate-Secrets.ps1) - Script de validación

---

## 🎉 AL COMPLETAR

Una vez que marques todas las casillas críticas:

1. Ejecuta `./scripts/Validate-Secrets.ps1` ✅
2. Actualiza [PROGRESS_TRACKER.md](PROGRESS_TRACKER.md):
   ```markdown
   SPRINT 1: Terceros ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━100% ✅
   ```
3. Continúa con **Sprint 2: Auth Integration** 🚀
