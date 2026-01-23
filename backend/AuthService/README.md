# AuthService

## 📋 Overview
The **AuthService** is responsible for user authentication, authorization, and identity management within the CarDealer ecosystem. It handles user registration, login, token generation (JWT), and role management.

## 🚀 Features
- **Authentication**: User login with email/password.
- **Authorization**: Role-based access control (RBAC).
- **Token Management**: JWT generation, validation, and refresh tokens.
- **Two-Factor Authentication (2FA)**: TOTP-based 2FA with recovery codes.
- **Security**: Password hashing, account lockout, and rate limiting.
- **Integration**: Publishes events to RabbitMQ for other services.

### 🔐 Sprint 18: Seguridad Avanzada 2FA (v3.0.0)

| Feature | Description |
|---------|-------------|
| **Recovery Codes Dual Persistence** | Codes stored in Redis (365d) + PostgreSQL for resilience |
| **Security Alert Notifications** | Email alerts after 3+ failed login/2FA attempts |
| **CAPTCHA Integration** | Google reCAPTCHA v3 required after 2 failed logins |
| **Device Fingerprinting** | Track and manage trusted devices per user |
| **SIEM Audit Logging** | Structured logs for Splunk/Elasticsearch/Datadog |

## 🔐 Two-Factor Authentication (2FA)

### Endpoints

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/TwoFactor/enable` | POST | ✅ | Enable 2FA for user |
| `/api/TwoFactor/verify` | POST | ✅ | Verify TOTP code |
| `/api/TwoFactor/disable` | POST | ✅ | Disable 2FA |
| `/api/TwoFactor/login` | POST | ❌ | Login with TOTP code |
| `/api/TwoFactor/login-with-recovery` | POST | ❌ | Login with single recovery code |
| `/api/TwoFactor/recover-with-all-codes` | POST | ❌ | **Account recovery with ALL 10 codes** |
| `/api/TwoFactor/generate-recovery-codes` | POST | ✅ | Generate new recovery codes |

### Account Recovery Flow

When a user loses access to their authenticator app, they can recover their account by providing **ALL 10 original recovery codes**:

```
POST /api/TwoFactor/recover-with-all-codes
{
  "tempToken": "<from login response>",
  "recoveryCodes": ["CODE1", "CODE2", ..., "CODE10"]
}
```

**Response (success):**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJ...",
    "refreshToken": "abc123...",
    "newAuthenticatorSecret": "ABCD1234...",
    "newQrCodeUri": "<base64 PNG image>",
    "newRecoveryCodes": ["NEW1", "NEW2", ..., "NEW10"],
    "message": "Account recovered successfully..."
  }
}
```

### ⚠️ Nota Temporal: Envío de QR por Correo

**Estado actual (Enero 2026):**
- 🔧 **TEMPORAL**: El código QR se envía por correo electrónico al usuario durante la configuración de 2FA y recuperación de cuenta.
- 📧 El email incluye: QR code (imagen base64), secret manual, y los 10 recovery codes.

**Cuando el UI esté listo:**
- ✅ El código QR se mostrará **directamente en la interfaz de usuario**.
- 📧 Por correo solo se enviarán los **10 recovery codes** como respaldo.
- 🔐 El secret nunca viajará por email en producción final.

### Recovery Codes

- Se generan **10 códigos únicos** de 8 caracteres cuando se habilita 2FA.
- Cada código es de **un solo uso** para `login-with-recovery`.
- Para **recuperación total** (`recover-with-all-codes`) se requieren los 10 códigos.
- Después de una recuperación exitosa, se generan **10 códigos nuevos**.

## 🛠️ Tech Stack
- **Framework**: .NET 8 (ASP.NET Core Web API)
- **Database**: PostgreSQL (Entity Framework Core)
- **Caching**: Redis (Distributed Cache)
- **Messaging**: RabbitMQ (MassTransit/Raw Client)
- **Observability**: OpenTelemetry, Serilog
- **Resilience**: Polly

## 🏃‍♂️ Getting Started

### Prerequisites
- .NET 8 SDK
- Docker & Docker Compose
- PostgreSQL
- Redis
- RabbitMQ

### Running Locally
```bash
cd backend/AuthService/AuthService.Api
dotnet run
```

### Running with Docker
```bash
docker-compose up -d authservice
```

## 🧪 Testing
```bash
dotnet test backend/AuthService/AuthService.Tests
```

## 📚 Documentation
- [Architecture](ARCHITECTURE.md)
- [Changelog](CHANGELOG.md)
- [Troubleshooting](TROUBLESHOOTING.md)
- [API Documentation](http://localhost:5000/swagger)
