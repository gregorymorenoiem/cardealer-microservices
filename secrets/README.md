# 🔐 SECRETS DIRECTORY - CarDealer Microservices

Este directorio contiene los secrets (API keys, passwords, tokens) necesarios para conectar con servicios externos.

⚠️ **IMPORTANTE:** Los archivos aquí NO deben commiterse a Git. Ya están en `.gitignore`.

---

## 📋 Quick Start (Sprint 1)

1. **Lee la guía completa:** [SPRINT_1_CHECKLIST.md](../docs/sprints/frontend-backend-integration/SPRINT_1_CHECKLIST.md)

2. **Crea tus archivos de secrets** (ejemplos):
   ```bash
   # JWT Secret (generar uno random)
   openssl rand -base64 32 > secrets/jwt_secret_key.txt
   
   # PostgreSQL Password
   echo "your_secure_password" > secrets/db_password.txt
   
   # Google Maps API Key (obtener de console.cloud.google.com)
   echo "AIzaSy..." > secrets/google_maps_api_key.txt
   
   # AWS Credentials (obtener de aws.amazon.com)
   echo "AKIA..." > secrets/aws_access_key_id.txt
   echo "secret_key" > secrets/aws_secret_access_key.txt
   echo "cardealer-media-dev" > secrets/aws_s3_bucket.txt
   
   # Stripe (obtener de stripe.com)
   echo "sk_test_..." > secrets/stripe_secret_key.txt
   echo "pk_test_..." > secrets/stripe_publishable_key.txt
   
   # SendGrid (obtener de sendgrid.com)
   echo "SG...." > secrets/sendgrid_api_key.txt
   ```

3. **Valida tu configuración:**
   ```bash
   ./scripts/Validate-Secrets.ps1
   ```

4. **Genera compose.secrets.yaml:**
   ```bash
   ./scripts/Update-ComposeSecrets.ps1
   ```

5. **Prueba conectividad:**
   ```bash
   ./scripts/Test-Third-Party-Services.ps1
   ```

---

## 📝 Archivos Requeridos

### 🔴 CRÍTICOS (obligatorios)

| Archivo | Descripción | Ejemplo | Cómo obtener |
|---------|-------------|---------|--------------|
| `jwt_secret_key.txt` | JWT secret key (min 32 chars) | `a1b2c3...` | `openssl rand -base64 32` |
| `db_password.txt` | PostgreSQL password | `MyP@ssw0rd!` | Elegir uno seguro |
| `google_maps_api_key.txt` | Google Maps API | `AIzaSy...` | console.cloud.google.com |
| `aws_access_key_id.txt` | AWS Access Key | `AKIA...` | aws.amazon.com/iam |
| `aws_secret_access_key.txt` | AWS Secret Key | `abc123...` | aws.amazon.com/iam |
| `aws_s3_bucket.txt` | S3 Bucket name | `cardealer-media-dev` | s3.console.aws.amazon.com |
| `stripe_secret_key.txt` | Stripe Secret | `sk_test_...` | dashboard.stripe.com |
| `stripe_publishable_key.txt` | Stripe Public | `pk_test_...` | dashboard.stripe.com |

### 🟠 ALTA PRIORIDAD (recomendados)

| Archivo | Descripción | Ejemplo |
|---------|-------------|---------|
| `sendgrid_api_key.txt` | SendGrid API Key | `SG....` |
| `firebase_service_account.json` | Firebase Admin SDK | JSON file |
| `google_oauth_client_id.txt` | OAuth Client ID | `....apps.googleusercontent.com` |
| `google_oauth_client_secret.txt` | OAuth Secret | `GOCSPX-...` |

### 🟡 OPCIONALES

| Archivo | Descripción |
|---------|-------------|
| `twilio_account_sid.txt` | Twilio Account SID |
| `twilio_auth_token.txt` | Twilio Auth Token |
| `resend_api_key.txt` | Resend API (alternativa a SendGrid) |
| `sentry_dsn_backend.txt` | Sentry DSN Backend |
| `sentry_dsn_frontend.txt` | Sentry DSN Frontend |

---

## 🛠️ Scripts Disponibles

### 1. Validate-Secrets.ps1
```bash
./scripts/Validate-Secrets.ps1
```
Verifica que todos los secrets tengan valores válidos.

### 2. Update-ComposeSecrets.ps1
```bash
./scripts/Update-ComposeSecrets.ps1
```
Genera `compose.secrets.yaml` automáticamente desde secrets/.

### 3. Test-Third-Party-Services.ps1
```bash
./scripts/Test-Third-Party-Services.ps1
```
Prueba conectividad real con servicios externos.

---

## 🚫 Qué NO hacer

- ❌ NO commites estos archivos a Git
- ❌ NO compartas secrets en mensajes
- ❌ NO uses secrets de producción en desarrollo
- ❌ NO uses valores como `123456`, `password`, `test`

---

## ✅ Qué SÍ hacer

- ✅ Usa un password manager (1Password, LastPass)
- ✅ Genera passwords aleatorios fuertes
- ✅ Usa claves de TEST en desarrollo
- ✅ Rota secrets regularmente (cada 90 días)

---

## 📚 Referencias

- **Guía Sprint 1:** [SPRINT_1_SETUP_GUIDE.md](../docs/sprints/frontend-backend-integration/SPRINT_1_SETUP_GUIDE.md)
- **Checklist:** [SPRINT_1_CHECKLIST.md](../docs/sprints/frontend-backend-integration/SPRINT_1_CHECKLIST.md)
- **Progress:** [PROGRESS_TRACKER.md](../docs/sprints/frontend-backend-integration/PROGRESS_TRACKER.md)
