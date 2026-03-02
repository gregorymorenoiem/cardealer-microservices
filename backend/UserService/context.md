# UserService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** UserService
- **Puerto en Kubernetes:** 8080
- **Puerto en Desarrollo:** 5002
- **Estado:** ✅ **EN PRODUCCIÓN**
- **Base de Datos:** PostgreSQL (`userservice`)
- **Imagen Docker:** ghcr.io/gregorymorenoiem/cardealer-userservice:latest

### Propósito
Servicio de gestión de perfiles de usuarios, incluyendo información personal, preferencias, direcciones y configuración de cuenta. Maneja diferentes tipos de cuentas (Individual, Dealer, Admin) y sus relaciones con roles.

---

## 🏗️ ARQUITECTURA

### Clean Architecture Layers

```
UserService/
├── UserService.Api/                     # Capa de presentación
│   ├── Controllers/
│   │   ├── UsersController.cs           # CRUD de usuarios
│   │   ├── ProfilesController.cs        # Perfiles públicos
│   │   └── PreferencesController.cs     # Configuración de usuario
│   ├── Program.cs
│   ├── appsettings.json
│   └── Dockerfile
├── UserService.Application/             # Capa de aplicación
│   ├── Features/                        # CQRS con MediatR
│   │   ├── Commands/
│   │   │   ├── CreateUserCommand.cs
│   │   │   ├── UpdateUserCommand.cs
│   │   │   ├── DeleteUserCommand.cs
│   │   │   └── UpdateProfileCommand.cs
│   │   └── Queries/
│   │       ├── GetUserByIdQuery.cs
│   │       ├── GetUserByEmailQuery.cs
│   │       ├── GetUserProfileQuery.cs
│   │       └── SearchUsersQuery.cs
│   ├── DTOs/
│   │   ├── UserDto.cs
│   │   ├── ProfileDto.cs
│   │   ├── AddressDto.cs
│   │   └── PreferencesDto.cs
│   └── Validators/
│       ├── CreateUserDtoValidator.cs
│       └── UpdateUserDtoValidator.cs
├── UserService.Domain/                  # Capa de dominio
│   ├── Entities/
│   │   ├── User.cs                      # Usuario principal
│   │   ├── Address.cs                   # Direcciones
│   │   └── UserPreferences.cs           # Preferencias
│   ├── Enums/
│   │   ├── AccountType.cs               # Guest, Individual, Dealer, Admin
│   │   ├── DealerRole.cs                # Roles de dealer
│   │   └── PlatformRole.cs              # Roles de plataforma
│   ├── Interfaces/
│   │   ├── IUserRepository.cs
│   │   └── IAddressRepository.cs
│   └── Events/
│       ├── UserCreatedEvent.cs          # Publicado a RabbitMQ
│       ├── UserUpdatedEvent.cs
│       ├── UserDeletedEvent.cs
│       └── ProfileUpdatedEvent.cs
├── UserService.Infrastructure/          # Capa de infraestructura
│   ├── Persistence/
│   │   ├── UserDbContext.cs
│   │   ├── Migrations/
│   │   └── Repositories/
│   │       ├── UserRepository.cs
│   │       └── AddressRepository.cs
│   ├── Messaging/
│   │   ├── RabbitMqEventPublisher.cs
│   │   └── UserRegisteredEventConsumer.cs  # Consume de AuthService
│   └── Services/
│       └── AvatarService.cs             # Upload de avatares
└── UserService.Tests/
```

---

## 📦 ENTIDADES DEL DOMINIO

### User
```csharp
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }            // Sincronizado desde AuthService
    
    // Información Personal
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName => $"{FirstName} {LastName}".Trim();
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime? DateOfBirth { get; set; }
    
    // Tipo de Cuenta
    public AccountType AccountType { get; set; }  // Guest, Individual, Dealer, Admin, etc.
    public DealerRole? DealerRole { get; set; }
    public PlatformRole? PlatformRole { get; set; }
    
    // Dealer Information (si AccountType == Dealer)
    public Guid? DealerId { get; set; }
    public string? DealerName { get; set; }
    public string? DealerLicense { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    
    // Relationships
    public ICollection<Address> Addresses { get; set; }
    public UserPreferences? Preferences { get; set; }
}
```

### Address
```csharp
public class Address
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    public string Street { get; set; }
    public string? Street2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    
    public bool IsPrimary { get; set; }
    public bool IsShipping { get; set; }
    public bool IsBilling { get; set; }
    
    // Relationships
    public User User { get; set; }
}
```

### UserPreferences
```csharp
public class UserPreferences
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    // Notificaciones
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;
    public bool PushNotifications { get; set; } = true;
    
    // Marketing
    public bool MarketingEmails { get; set; } = false;
    public bool NewsletterSubscription { get; set; } = false;
    
    // Privacidad
    public bool ShowEmail { get; set; } = false;
    public bool ShowPhone { get; set; } = false;
    public bool ShowProfile { get; set; } = true;
    
    // UI
    public string Theme { get; set; } = "light";  // light, dark, auto
    public string Language { get; set; } = "es";  // es, en
    public string Currency { get; set; } = "DOP";
    
    // Relationships
    public User User { get; set; }
}
```

### AccountType Enum
```csharp
public enum AccountType
{
    Guest,              // Usuario no registrado (browsing only)
    Individual,         // Usuario individual (comprador/vendedor)
    Dealer,             // Concesionario/Dealer
    DealerEmployee,     // Empleado de concesionario
    Admin,              // Administrador de plataforma
    PlatformEmployee    // Empleado de OKLA
}
```

---

## 📡 ENDPOINTS API

### CRUD de Usuarios

#### GET `/api/users/{id}`
Obtener usuario por ID.

**Response (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "usuario@ejemplo.com",
  "firstName": "Juan",
  "lastName": "Pérez",
  "fullName": "Juan Pérez",
  "phoneNumber": "+18095551234",
  "avatarUrl": "https://s3.amazonaws.com/avatars/user123.jpg",
  "accountType": "Individual",
  "isActive": true,
  "isEmailVerified": true,
  "isPhoneVerified": true,
  "createdAt": "2026-01-01T00:00:00Z"
}
```

#### PUT `/api/users/{id}`
Actualizar usuario (requiere autenticación).

**Request:**
```json
{
  "firstName": "Juan",
  "lastName": "Pérez",
  "phoneNumber": "+18095551234",
  "dateOfBirth": "1990-05-15"
}
```

#### DELETE `/api/users/{id}`
Eliminar usuario (soft delete).

**Response (204 No Content)**

#### GET `/api/users`
Listar usuarios (admin only, paginado).

**Query Parameters:**
- `page`: Número de página (default: 1)
- `pageSize`: Tamaño de página (default: 10, max: 100)
- `search`: Búsqueda por nombre o email
- `accountType`: Filtrar por tipo de cuenta

**Response (200 OK):**
```json
{
  "items": [...],
  "totalCount": 150,
  "page": 1,
  "pageSize": 10,
  "totalPages": 15
}
```

### Perfiles Públicos

#### GET `/api/profiles/{userId}`
Ver perfil público de un usuario.

**Response (200 OK):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fullName": "Juan Pérez",
  "avatarUrl": "https://s3.amazonaws.com/avatars/user123.jpg",
  "accountType": "Individual",
  "memberSince": "2026-01-01T00:00:00Z",
  "vehiclesListed": 5,
  "rating": 4.8
}
```

### Preferencias

#### GET `/api/preferences`
Obtener preferencias del usuario autenticado.

**Response (200 OK):**
```json
{
  "emailNotifications": true,
  "smsNotifications": false,
  "pushNotifications": true,
  "marketingEmails": false,
  "theme": "light",
  "language": "es",
  "currency": "DOP"
}
```

#### PUT `/api/preferences`
Actualizar preferencias.

**Request:**
```json
{
  "emailNotifications": false,
  "theme": "dark"
}
```

### Direcciones

#### GET `/api/users/{userId}/addresses`
Obtener direcciones del usuario.

#### POST `/api/users/{userId}/addresses`
Agregar dirección.

**Request:**
```json
{
  "street": "Av. Winston Churchill 1100",
  "city": "Santo Domingo",
  "state": "Distrito Nacional",
  "postalCode": "10147",
  "country": "DO",
  "isPrimary": true
}
```

---

## 🔄 EVENTOS

### Eventos Consumidos (desde AuthService)

#### UserRegisteredEvent
Cuando un usuario se registra en AuthService, UserService crea el perfil básico.

```csharp
public record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string? PhoneNumber,
    DateTime RegisteredAt
);
```

**Handler:** `UserRegisteredEventConsumer`  
**Acción:** Crear registro en `users` table con datos básicos.

### Eventos Publicados

#### UserCreatedEvent
```csharp
public record UserCreatedEvent(
    Guid UserId,
    string Email,
    string? FullName,
    AccountType AccountType,
    DateTime CreatedAt
);
```

**Exchange:** `user.events`  
**Routing Key:** `user.created`  
**Consumidores:**
- **NotificationService**: Email de bienvenida
- **CRMService**: Agregar a CRM

#### UserUpdatedEvent
```csharp
public record UserUpdatedEvent(
    Guid UserId,
    string Email,
    Dictionary<string, object> Changes,
    DateTime UpdatedAt
);
```

#### ProfileUpdatedEvent
Publicado cuando se actualiza avatar o información pública.

---

## 🔧 TECNOLOGÍAS Y DEPENDENCIAS

### Paquetes NuGet
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="RabbitMQ.Client" Version="6.8.1" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="OpenTelemetry" Version="1.7.0" />
<PackageReference Include="AWSSDK.S3" Version="3.7.305" />
```

### Servicios Externos
- **PostgreSQL**: Base de datos principal
- **RabbitMQ**: Event bus
- **AWS S3**: Almacenamiento de avatares
- **MediaService**: Upload de imágenes

---

## ⚙️ CONFIGURACIÓN

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=${DB_HOST};Database=userservice;Username=${DB_USER};Password=${DB_PASSWORD}"
  },
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
  },
  "AWS": {
    "Region": "us-east-1",
    "S3": {
      "BucketName": "okla-avatars"
    }
  }
}
```

---

## 🗃️ BASE DE DATOS

### Tablas
- **users**: Usuarios principales
- **addresses**: Direcciones de usuarios
- **user_preferences**: Configuración de usuario

### Índices
```sql
CREATE UNIQUE INDEX idx_users_email ON users (LOWER(email));
CREATE INDEX idx_users_account_type ON users (account_type);
CREATE INDEX idx_users_dealer_id ON users (dealer_id) WHERE dealer_id IS NOT NULL;
CREATE INDEX idx_addresses_user_id ON addresses (user_id);
```

---

## 🔗 RELACIONES CON OTROS SERVICIOS

### Consume Eventos De:
- **AuthService**: UserRegisteredEvent

### Publica Eventos A:
- **NotificationService**: Notificaciones
- **CRMService**: Sincronización de contactos
- **AuditService**: Cambios de perfil

### Consulta A:
- **MediaService**: Upload de avatares
- **RoleService**: Verificación de permisos

---

## 📝 REGLAS DE NEGOCIO

1. **Email único**: Sincronizado desde AuthService
2. **Soft delete**: Usuarios eliminados se marcan como `IsActive = false`
3. **Avatar**: Máximo 5MB, formatos JPG/PNG/WEBP
4. **Dirección primaria**: Solo puede haber una dirección primaria por usuario
5. **AccountType**: No puede cambiar después de creado (requiere aprobación admin)

---

## 🚀 DESPLIEGUE

### Kubernetes
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: userservice
  namespace: okla
spec:
  replicas: 2
  template:
    spec:
      containers:
      - name: userservice
        image: ghcr.io/gregorymorenoiem/cardealer-userservice:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_URLS
          value: "http://+:8080"
```

---

## 📅 ÚLTIMA ACTUALIZACIÓN

**Fecha:** Enero 7, 2026  
**Versión:** 1.0.0  
**Estado:** Producción estable en DOKS

