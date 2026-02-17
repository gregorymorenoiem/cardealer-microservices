# AuthService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** AuthService
- **Puerto en Kubernetes:** 8080
- **Puerto en Desarrollo:** 5001
- **Estado:** ✅ **EN PRODUCCIÓN**
- **Base de Datos:** PostgreSQL (`authservice`)
- **Imagen Docker:** ghcr.io/gregorymorenoiem/cardealer-authservice:latest

### Propósito
Servicio de autenticación y autorización centralizado. Maneja registro de usuarios, login con JWT, refresh tokens, 2FA, verificación de teléfono y autenticación con proveedores externos (Google, Facebook).

---

## 🏗️ ARQUITECTURA

### Clean Architecture Layers

```
AuthService/
├── AuthService.Api/                     # Capa de presentación
│   ├── Controllers/
│   │   ├── AuthController.cs            # Login, Register, Refresh
│   │   ├── ExternalAuthController.cs    # Google, Facebook OAuth
│   │   ├── PhoneVerificationController.cs  # SMS verification
│   │   └── TwoFactorController.cs       # 2FA (TOTP)
│   ├── Middleware/
│   │   └── RequestLoggingMiddleware.cs
│   ├── Program.cs                       # Entry point
│   ├── appsettings.json
│   └── Dockerfile
├── AuthService.Application/             # Capa de aplicación
│   ├── Features/                        # CQRS con MediatR
│   │   ├── Commands/
│   │   │   ├── LoginCommand.cs
│   │   │   ├── RegisterCommand.cs
│   │   │   ├── RefreshTokenCommand.cs
│   │   │   ├── Enable2FACommand.cs
│   │   │   └── VerifyPhoneCommand.cs
│   │   └── Queries/
│   │       ├── GetCurrentUserQuery.cs
│   │       └── ValidateTokenQuery.cs
│   ├── DTOs/
│   │   ├── LoginDto.cs
│   │   ├── RegisterDto.cs
│   │   ├── TokenDto.cs
│   │   └── UserDto.cs
│   └── Validators/                      # FluentValidation
│       ├── LoginDtoValidator.cs
│       └── RegisterDtoValidator.cs
├── AuthService.Domain/                  # Capa de dominio
│   ├── Entities/
│   │   ├── ApplicationUser.cs           # Usuario principal
│   │   ├── RefreshToken.cs              # Tokens de refresco
│   │   ├── TwoFactorAuth.cs             # Configuración 2FA
│   │   └── VerificationToken.cs         # Tokens de verificación
│   ├── Interfaces/
│   │   ├── IAuthRepository.cs
│   │   ├── ITokenService.cs
│   │   ├── IPasswordHasher.cs
│   │   └── ISmsService.cs
│   └── Events/
│       ├── UserRegisteredEvent.cs       # Publicado a RabbitMQ
│       ├── UserLoggedInEvent.cs
│       └── PasswordResetRequestedEvent.cs
├── AuthService.Infrastructure/          # Capa de infraestructura
│   ├── Persistence/
│   │   ├── AuthDbContext.cs             # EF Core DbContext
│   │   ├── Migrations/
│   │   └── Repositories/
│   │       └── AuthRepository.cs
│   ├── Services/
│   │   ├── TokenService.cs              # JWT generation
│   │   ├── PasswordHasher.cs            # BCrypt
│   │   └── SmsService.cs                # Twilio integration
│   ├── Messaging/
│   │   └── RabbitMqEventPublisher.cs    # RabbitMQ client
│   └── BackgroundServices/
│       └── TokenCleanupService.cs       # Limpieza de tokens expirados
└── AuthService.Tests/                   # Tests unitarios
    ├── Unit/
    └── Integration/
```

---

## 📦 ENTIDADES DEL DOMINIO

### ApplicationUser
```csharp
public class ApplicationUser
{
    public Guid Id { get; set; }
    public string Email { get; set; }           // Único
    public string PasswordHash { get; set; }    // BCrypt hashed
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? ExternalProvider { get; set; }  // "Google", "Facebook"
    public string? ExternalProviderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
    
    // Relationships
    public ICollection<RefreshToken> RefreshTokens { get; set; }
    public TwoFactorAuth? TwoFactorAuth { get; set; }
}
```

### RefreshToken
```csharp
public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; }           // GUID único
    public DateTime ExpiresAt { get; set; }     // +30 días
    public DateTime CreatedAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? RevokedByIp { get; set; }
    public DateTime? RevokedAt { get; set; }
    
    // Relationships
    public ApplicationUser User { get; set; }
}
```

### TwoFactorAuth
```csharp
public class TwoFactorAuth
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string SecretKey { get; set; }       // TOTP secret
    public bool IsEnabled { get; set; }
    public DateTime? EnabledAt { get; set; }
    public string[]? RecoveryCodes { get; set; }
    
    // Relationships
    public ApplicationUser User { get; set; }
}
```

### VerificationToken
```csharp
public class VerificationToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; }
    public string Type { get; set; }            // "EmailVerification", "PasswordReset", "PhoneVerification"
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    
    // Relationships
    public ApplicationUser User { get; set; }
}
```

---

## 📡 ENDPOINTS API

### Rutas Públicas (Sin Autenticación)

#### POST `/api/auth/register`
Registro de nuevo usuario.

**Request:**
```json
{
  "email": "usuario@ejemplo.com",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "phoneNumber": "+18095551234"
}
```

**Response (201 Created):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "usuario@ejemplo.com",
  "message": "Usuario registrado exitosamente"
}
```

#### POST `/api/auth/login`
Login con email y password.

**Request:**
```json
{
  "email": "usuario@ejemplo.com",
  "password": "Password123!"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "expiresIn": 86400,
  "tokenType": "Bearer",
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "usuario@ejemplo.com",
    "emailConfirmed": true,
    "phoneNumberConfirmed": false,
    "twoFactorEnabled": false
  }
}
```

#### POST `/api/auth/refresh`
Renovar access token usando refresh token.

**Request:**
```json
{
  "refreshToken": "a1b2c3d4-e5f6-7890-1234-567890abcdef"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "f6e5d4c3-b2a1-0987-6543-210fedcba098",
  "expiresIn": 86400,
  "tokenType": "Bearer"
}
```

### Rutas Protegidas (Requieren JWT)

#### GET `/api/auth/me`
Obtener información del usuario actual.

**Headers:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

**Response (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "usuario@ejemplo.com",
  "phoneNumber": "+18095551234",
  "emailConfirmed": true,
  "phoneNumberConfirmed": true,
  "twoFactorEnabled": false,
  "createdAt": "2026-01-01T00:00:00Z",
  "lastLoginAt": "2026-01-07T10:30:00Z"
}
```

#### POST `/api/auth/logout`
Cerrar sesión (revocar refresh token).

**Request:**
```json
{
  "refreshToken": "a1b2c3d4-e5f6-7890-1234-567890abcdef"
}
```

**Response (200 OK):**
```json
{
  "message": "Sesión cerrada exitosamente"
}
```

### Two-Factor Authentication (2FA)

#### POST `/api/auth/2fa/enable`
Habilitar 2FA para el usuario.

**Response (200 OK):**
```json
{
  "secretKey": "JBSWY3DPEHPK3PXP",
  "qrCodeUrl": "otpauth://totp/OKLA:usuario@ejemplo.com?secret=JBSWY3DPEHPK3PXP&issuer=OKLA",
  "recoveryCodes": ["abc123", "def456", "ghi789"]
}
```

#### POST `/api/auth/2fa/verify`
Verificar código TOTP.

**Request:**
```json
{
  "code": "123456"
}
```

### Phone Verification

#### POST `/api/auth/phone/send-code`
Enviar código de verificación por SMS.

**Request:**
```json
{
  "phoneNumber": "+18095551234"
}
```

#### POST `/api/auth/phone/verify`
Verificar código SMS.

**Request:**
```json
{
  "phoneNumber": "+18095551234",
  "code": "123456"
}
```

### External Authentication

#### POST `/api/auth/external/google`
Login con Google OAuth.

**Request:**
```json
{
  "idToken": "google_oauth_token_here"
}
```

#### POST `/api/auth/external/facebook`
Login con Facebook OAuth.

**Request:**
```json
{
  "accessToken": "facebook_access_token_here"
}
```

---

## 🔧 TECNOLOGÍAS Y DEPENDENCIAS

### Paquetes NuGet Principales

```xml
<!-- ASP.NET Core -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />

<!-- Entity Framework Core -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />

<!-- CQRS & Mediator -->
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />

<!-- Security -->
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.3" />

<!-- RabbitMQ -->
<PackageReference Include="RabbitMQ.Client" Version="6.8.1" />

<!-- Observability -->
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.7.0" />

<!-- External Auth -->
<PackageReference Include="Google.Apis.Auth" Version="1.64.0" />

<!-- SMS -->
<PackageReference Include="Twilio" Version="6.16.1" />
```

### Servicios Externos
- **PostgreSQL**: Base de datos
- **RabbitMQ**: Event bus para eventos de dominio
- **Twilio**: Envío de SMS
- **Google OAuth**: Autenticación con Google
- **Facebook OAuth**: Autenticación con Facebook

---

## ⚙️ CONFIGURACIÓN (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=${DB_HOST};Port=${DB_PORT};Database=authservice;Username=${DB_USER};Password=${DB_PASSWORD}"
  },
  "Jwt": {
    "Key": "${JWT_SECRET_KEY}",
    "Issuer": "okla-auth-service",
    "Audience": "okla-api",
    "ExpirationInMinutes": 1440
  },
  "RefreshToken": {
    "ExpirationInDays": 30
  },
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  },
  "Twilio": {
    "AccountSid": "${TWILIO_ACCOUNT_SID}",
    "AuthToken": "${TWILIO_AUTH_TOKEN}",
    "PhoneNumber": "${TWILIO_PHONE_NUMBER}"
  },
  "Google": {
    "ClientId": "${GOOGLE_CLIENT_ID}",
    "ClientSecret": "${GOOGLE_CLIENT_SECRET}"
  },
  "Security": {
    "RateLimit": {
      "RequestsPerMinute": 20
    }
  }
}
```

---

## 🔄 EVENTOS PUBLICADOS (RabbitMQ)

### UserRegisteredEvent
Publicado cuando un usuario se registra exitosamente.

```csharp
public record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string? PhoneNumber,
    DateTime RegisteredAt
);
```

**Exchange:** `auth.events`  
**Routing Key:** `user.registered`  
**Consumidores:**
- **UserService**: Crea perfil de usuario
- **NotificationService**: Envía email de bienvenida

### UserLoggedInEvent
Publicado en cada login exitoso.

```csharp
public record UserLoggedInEvent(
    Guid UserId,
    string Email,
    DateTime LoginAt,
    string IpAddress
);
```

**Exchange:** `auth.events`  
**Routing Key:** `user.logged_in`  
**Consumidores:**
- **AuditService**: Registra actividad

### PasswordResetRequestedEvent
Publicado cuando se solicita reset de contraseña.

```csharp
public record PasswordResetRequestedEvent(
    Guid UserId,
    string Email,
    string ResetToken,
    DateTime RequestedAt,
    DateTime ExpiresAt
);
```

**Exchange:** `auth.events`  
**Routing Key:** `password.reset_requested`  
**Consumidores:**
- **NotificationService**: Envía email con token

---

## 🗃️ BASE DE DATOS

### Tablas

- **application_users**: Usuarios principales
- **refresh_tokens**: Tokens de refresco
- **two_factor_auth**: Configuración 2FA
- **verification_tokens**: Tokens de verificación

### Migraciones

```bash
# Crear migración
dotnet ef migrations add MigrationName -p AuthService.Infrastructure -s AuthService.Api

# Aplicar migraciones
dotnet ef database update -p AuthService.Infrastructure -s AuthService.Api
```

### Índices Importantes

```sql
-- Email único (no case-sensitive)
CREATE UNIQUE INDEX idx_users_email ON application_users (LOWER(email));

-- Búsqueda rápida de refresh tokens
CREATE INDEX idx_refresh_tokens_user_id ON refresh_tokens (user_id);
CREATE INDEX idx_refresh_tokens_token ON refresh_tokens (token);
CREATE INDEX idx_refresh_tokens_expires_at ON refresh_tokens (expires_at);

-- Verificación de tokens
CREATE INDEX idx_verification_tokens_user_id ON verification_tokens (user_id);
CREATE INDEX idx_verification_tokens_token ON verification_tokens (token);
```

---

## 🔐 SEGURIDAD

### Password Hashing
- **Algoritmo:** BCrypt con salt automático
- **Work Factor:** 12 (2^12 = 4096 iterations)

```csharp
// Hash password
string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

// Verify password
bool isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
```

### JWT Token
- **Algoritmo:** HS256 (HMAC-SHA256)
- **Expiración:** 24 horas (1440 minutos)
- **Claims incluidos:**
  - `sub`: User ID
  - `email`: Email
  - `jti`: Token ID único
  - `iat`: Issued At
  - `exp`: Expiration

### Refresh Token
- **Formato:** GUID aleatorio
- **Expiración:** 30 días
- **Rotación:** Se genera nuevo refresh token en cada uso

### Rate Limiting
- **Login:** 20 intentos/minuto por IP
- **Register:** 5 registros/hora por IP
- **Phone Verification:** 3 códigos/hora por número

---

## 🚀 DESPLIEGUE

### Kubernetes Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: authservice
  namespace: okla
spec:
  replicas: 2
  selector:
    matchLabels:
      app: authservice
  template:
    spec:
      containers:
      - name: authservice
        image: ghcr.io/gregorymorenoiem/cardealer-authservice:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ASPNETCORE_URLS
          value: "http://+:8080"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: postgres-secret
              key: authservice-connection
        - name: Jwt__Key
          valueFrom:
            secretKeyRef:
              name: jwt-secret
              key: jwt-key
```

---

## 🔗 RELACIONES CON OTROS SERVICIOS

### Publica Eventos A:
- **UserService**: Sincronización de usuarios
- **NotificationService**: Emails y SMS
- **AuditService**: Registro de actividad

### Consulta A:
- Ninguno (servicio independiente)

### Consultado Por:
- **Gateway**: Validación de tokens
- **Todos los servicios**: Autenticación de usuarios

---

## 📝 REGLAS DE NEGOCIO

### Registro de Usuario
1. Email debe ser único (case-insensitive)
2. Password debe tener:
   - Mínimo 8 caracteres
   - Al menos 1 mayúscula
   - Al menos 1 minúscula
   - Al menos 1 número
   - Al menos 1 carácter especial
3. Email de verificación se envía automáticamente

### Login
1. Máximo 5 intentos fallidos antes de bloqueo temporal (15 minutos)
2. Si 2FA está habilitado, se requiere código TOTP
3. Refresh token se genera automáticamente

### Refresh Token
1. Solo válido si no ha expirado y no está revocado
2. Un refresh token solo puede usarse una vez (rotación)
3. Tokens antiguos se limpian automáticamente después de 60 días

---

## 🐛 TROUBLESHOOTING

### JWT Token Inválido

**Error:** `401 Unauthorized - Invalid token`

**Causas:**
- Token expirado
- Clave secreta diferente entre AuthService y Gateway
- Issuer/Audience no coinciden

**Solución:**
```bash
# Verificar secret en K8s
kubectl get secret jwt-secret -n okla -o jsonpath='{.data.jwt-key}' | base64 -d

# Debe ser idéntico en AuthService y Gateway
```

### Eventos no se publican a RabbitMQ

**Causa:** RabbitMQ no disponible o credenciales incorrectas

**Solución:**
```bash
# Verificar conectividad
kubectl exec -it deployment/authservice -n okla -- \
  wget -qO- http://rabbitmq:15672/api/health/checks/alarms
```

---

## 📅 ÚLTIMA ACTUALIZACIÓN

**Fecha:** Enero 7, 2026  
**Versión:** 1.0.0  
**Estado:** Producción estable en DOKS
