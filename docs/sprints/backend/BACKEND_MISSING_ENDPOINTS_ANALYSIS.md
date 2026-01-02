# 📋 ANÁLISIS: ENDPOINTS FALTANTES PARA SISTEMA MULTI-NIVEL DE ROLES

**Fecha**: Diciembre 5, 2025  
**Arquitectura**: 🏗️ Microservicios (30+ servicios)  
**Servicio Target**: **UserService** (gestión de usuarios y empleados)  
**Estado Backend**: ✅ Entidad User con roles | ❌ Endpoints de invitaciones faltantes  
**Prioridad**: 🔴 ALTA - Funcionalidad core para gestión de equipos

---

## 🎯 RESUMEN EJECUTIVO

### Arquitectura Detectada:
```
backend/
├── UserService/          ← AQUÍ van los endpoints de empleados
├── RoleService/          ✅ Roles/Permisos genéricos
├── AuthService/          ✅ Autenticación/JWT
├── NotificationService/  ← Envío de emails de invitación
└── ... (27 servicios más)
```

### El backend **YA TIENE**:
- ✅ Entidad `User` con campos multi-nivel:
  * `AccountType` (Guest, Individual, Dealer, DealerEmployee, Admin, PlatformEmployee)
  * `DealerRole` (Owner, Manager, SalesManager, InventoryManager, Salesperson, Viewer)
  * `PlatformRole` (SuperAdmin, Admin, Moderator, Support, Analyst)
  * `DealerPermissions` / `PlatformPermissions` (JSON arrays)
  * `EmployerUserId` (quién contrató al empleado)
  * `DealerId` (a qué dealer pertenece)
- ✅ RoleService con endpoints `/api/roles`, `/api/permissions`
- ✅ AuthService con JWT y refresh tokens
- ✅ NotificationService para emails

### El backend **NO TIENE**:
- ❌ **Sistema de invitaciones** (tablas DealerEmployeeInvitation, PlatformEmployeeInvitation)
- ❌ Controllers en UserService para gestión de empleados
- ❌ UseCases/Commands para invitar empleados
- ❌ UseCases para aceptar invitaciones
- ❌ Endpoints para listar empleados de un dealer/admin
- ❌ Endpoints para actualizar roles/permisos de empleados
- ❌ Endpoints para desactivar/remover empleados
- ❌ Integración UserService ↔ NotificationService para emails

---

## 🏗️ ARQUITECTURA ACTUAL DE MICROSERVICIOS

### Servicios Relevantes:

```
UserService/
├── UserService.Api/
│   └── Controllers/
│       ├── UsersController.cs          ✅ CRUD básico
│       └── UserRolesController.cs      ✅ Roles legacy
├── UserService.Domain/
│   └── Entities/
│       ├── User.cs                     ✅ Con AccountType, DealerRole, PlatformRole
│       └── UserRole.cs                 ✅ Legacy roles
└── UserService.Application/
    └── UseCases/
        ├── Users/                      ✅ CRUD básico
        └── UserRoles/                  ✅ Roles legacy

RoleService/                            ✅ COMPLETO
├── Controllers/
│   ├── RolesController.cs
│   ├── PermissionsController.cs
│   └── RolePermissionsController.cs
└── Entities/
    ├── Role.cs
    ├── Permission.cs
    └── RolePermission.cs

NotificationService/                    ✅ COMPLETO
├── Controllers/
│   └── NotificationsController.cs
└── Templates/
    └── (email templates)

AuthService/                            ✅ COMPLETO
├── Controllers/
│   └── AuthController.cs
└── Services/
    └── TokenService.cs
```

### Campos YA EXISTENTES en User.cs:

```csharp
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    // ✅ Multi-level role system (YA IMPLEMENTADO)
    public AccountType AccountType { get; set; }
    
    // ✅ Platform-level (admin/platform employee)
    public PlatformRole? PlatformRole { get; set; }
    public string? PlatformPermissions { get; set; } // JSON array
    
    // ✅ Dealer-level (dealer/dealer employee)
    public Guid? DealerId { get; set; }
    public DealerRole? DealerRole { get; set; }
    public string? DealerPermissions { get; set; } // JSON array
    
    // ✅ Employee metadata
    public Guid? EmployerUserId { get; set; } // Quién lo contrató
    public Guid? CreatedBy { get; set; }
    
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

// ✅ Enums YA DEFINIDOS:
public enum AccountType
{
    Guest,
    Individual,
    Dealer,
    DealerEmployee,    // ← Para empleados de dealers
    Admin,
    PlatformEmployee   // ← Para empleados de plataforma
}

public enum DealerRole
{
    Owner,
    Manager,
    SalesManager,
    InventoryManager,
    Salesperson,
    Viewer
}

public enum PlatformRole
{
    SuperAdmin,
    Admin,
    Moderator,
    Support,
    Analyst
}
```

---

## ❌ ENTIDADES FALTANTES (CRÍTICO)

### Necesitas crear las tablas de invitaciones:

```csharp
// UserService.Domain/Entities/DealerEmployeeInvitation.cs
public class DealerEmployeeInvitation
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public Guid DealerId { get; set; }
    public DealerRole DealerRole { get; set; }
    public string Permissions { get; set; } = "[]"; // JSON array
    public Guid InvitedBy { get; set; }
    public InvitationStatus Status { get; set; }
    public DateTime InvitationDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string Token { get; set; } = string.Empty;
    
    // Navigation properties
    public User? InvitedByUser { get; set; }
}

// UserService.Domain/Entities/PlatformEmployeeInvitation.cs
public class PlatformEmployeeInvitation
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public PlatformRole PlatformRole { get; set; }
    public string Permissions { get; set; } = "[]";
    public Guid InvitedBy { get; set; }
    public InvitationStatus Status { get; set; }
    public DateTime InvitationDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string Token { get; set; } = string.Empty;
    
    // Navigation properties
    public User? InvitedByUser { get; set; }
}

// UserService.Domain/Enums/InvitationStatus.cs
public enum InvitationStatus
{
    Pending = 0,
    Accepted = 1,
    Expired = 2,
    Revoked = 3
}
```

### Agregar DbSets en ApplicationDbContext:

```csharp
// UserService.Infrastructure/Persistence/ApplicationDbContext.cs
public class ApplicationDbContext : DbContext
{
    // ... DbSets existentes ...
    
    // ❌ AGREGAR ESTAS LÍNEAS:
    public DbSet<DealerEmployeeInvitation> DealerEmployeeInvitations { get; set; }
    public DbSet<PlatformEmployeeInvitation> PlatformEmployeeInvitations { get; set; }
}
```

### Migration necesaria:

```sql
-- UserService/Migrations/20251205_AddEmployeeInvitations.sql

CREATE TABLE DealerEmployeeInvitations (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Email VARCHAR(255) NOT NULL,
    DealerId UUID NOT NULL,
    DealerRole INT NOT NULL,
    Permissions TEXT NOT NULL DEFAULT '[]',
    InvitedBy UUID NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    InvitationDate TIMESTAMP NOT NULL DEFAULT NOW(),
    ExpirationDate TIMESTAMP NOT NULL,
    Token VARCHAR(500) NOT NULL UNIQUE,
    
    CONSTRAINT FK_DealerInvitations_InvitedBy FOREIGN KEY (InvitedBy) 
        REFERENCES Users(Id) ON DELETE RESTRICT,
    
    INDEX IX_DealerInvitations_Email (Email),
    INDEX IX_DealerInvitations_Token (Token),
    INDEX IX_DealerInvitations_Status (Status),
    INDEX IX_DealerInvitations_DealerId (DealerId)
);

CREATE TABLE PlatformEmployeeInvitations (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Email VARCHAR(255) NOT NULL,
    PlatformRole INT NOT NULL,
    Permissions TEXT NOT NULL DEFAULT '[]',
    InvitedBy UUID NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    InvitationDate TIMESTAMP NOT NULL DEFAULT NOW(),
    ExpirationDate TIMESTAMP NOT NULL,
    Token VARCHAR(500) NOT NULL UNIQUE,
    
    CONSTRAINT FK_PlatformInvitations_InvitedBy FOREIGN KEY (InvitedBy) 
        REFERENCES Users(Id) ON DELETE RESTRICT,
    
    INDEX IX_PlatformInvitations_Email (Email),
    INDEX IX_PlatformInvitations_Token (Token),
    INDEX IX_PlatformInvitations_Status (Status)
);
```

---

## ❌ ENDPOINTS FALTANTES EN USERSERVICE (CRÍTICOS)

### 1️⃣ **DealerEmployeesController** (NO EXISTE)

**Ubicación**: `backend/UserService/UserService.Api/Controllers/DealerEmployeesController.cs`

#### Endpoints requeridos para DEALERS:

```csharp
// UserService/UserService.Api/Controllers/DealerEmployeesController.cs

[ApiController]
[Route("api/dealers/{dealerId}/employees")]
public class DealerEmployeesController : ControllerBase
{
    /// 1. Invitar empleado
    /// POST /api/dealers/{dealerId}/employees/invite
    /// Body: { email, dealerRole, permissions[] }
    /// Genera token de invitación y envía email
    [HttpPost("invite")]
    public async Task<ActionResult<InvitationResponse>> InviteEmployee(
        Guid dealerId,
        [FromBody] InviteDealerEmployeeRequest request);

    /// 2. Listar empleados del dealer
    /// GET /api/dealers/{dealerId}/employees
    /// Query: ?status=active&role=manager&page=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<DealerEmployeeDto>>> GetEmployees(
        Guid dealerId,
        [FromQuery] EmployeeStatus? status = null,
        [FromQuery] DealerRole? role = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20);

    /// 3. Obtener detalle de empleado
    /// GET /api/dealers/{dealerId}/employees/{employeeId}
    [HttpGet("{employeeId}")]
    public async Task<ActionResult<DealerEmployeeDetailDto>> GetEmployee(
        Guid dealerId,
        Guid employeeId);

    /// 4. Actualizar rol/permisos de empleado
    /// PUT /api/dealers/{dealerId}/employees/{employeeId}
    /// Body: { dealerRole?, permissions[]? }
    [HttpPut("{employeeId}")]
    public async Task<ActionResult> UpdateEmployee(
        Guid dealerId,
        Guid employeeId,
        [FromBody] UpdateDealerEmployeeRequest request);

    /// 5. Desactivar empleado
    /// DELETE /api/dealers/{dealerId}/employees/{employeeId}
    /// Query: ?terminate=true (soft delete vs hard delete)
    [HttpDelete("{employeeId}")]
    public async Task<ActionResult> RemoveEmployee(
        Guid dealerId,
        Guid employeeId,
        [FromQuery] bool terminate = false);

    /// 6. Reenviar invitación
    /// POST /api/dealers/{dealerId}/employees/invitations/{invitationId}/resend
    [HttpPost("invitations/{invitationId}/resend")]
    public async Task<ActionResult> ResendInvitation(
        Guid dealerId,
        Guid invitationId);

    /// 7. Cancelar invitación
    /// DELETE /api/dealers/{dealerId}/employees/invitations/{invitationId}
    [HttpDelete("invitations/{invitationId}")]
    public async Task<ActionResult> CancelInvitation(
        Guid dealerId,
        Guid invitationId);

    /// 8. Listar invitaciones pendientes
    /// GET /api/dealers/{dealerId}/employees/invitations
    /// Query: ?status=pending
    [HttpGet("invitations")]
    public async Task<ActionResult<PaginatedResult<DealerInvitationDto>>> GetInvitations(
        Guid dealerId,
        [FromQuery] InvitationStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20);
}
```

---

### 2️⃣ **PlatformEmployeesController** (NO EXISTE)

#### Endpoints requeridos para ADMINS de PLATAFORMA:

```csharp
// UserService/UserService.Api/Controllers/PlatformEmployeesController.cs

[ApiController]
[Route("api/platform/employees")]
public class PlatformEmployeesController : ControllerBase
{
    /// 1. Invitar empleado de plataforma
    /// POST /api/platform/employees/invite
    /// Body: { email, platformRole, permissions[] }
    [HttpPost("invite")]
    public async Task<ActionResult<InvitationResponse>> InviteEmployee(
        [FromBody] InvitePlatformEmployeeRequest request);

    /// 2. Listar empleados de plataforma
    /// GET /api/platform/employees
    /// Query: ?status=active&role=moderator&page=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<PlatformEmployeeDto>>> GetEmployees(
        [FromQuery] EmployeeStatus? status = null,
        [FromQuery] PlatformRole? role = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20);

    /// 3. Obtener detalle de empleado
    /// GET /api/platform/employees/{employeeId}
    [HttpGet("{employeeId}")]
    public async Task<ActionResult<PlatformEmployeeDetailDto>> GetEmployee(
        Guid employeeId);

    /// 4. Actualizar rol/permisos
    /// PUT /api/platform/employees/{employeeId}
    /// Body: { platformRole?, permissions[]? }
    [HttpPut("{employeeId}")]
    public async Task<ActionResult> UpdateEmployee(
        Guid employeeId,
        [FromBody] UpdatePlatformEmployeeRequest request);

    /// 5. Desactivar empleado
    /// DELETE /api/platform/employees/{employeeId}
    [HttpDelete("{employeeId}")]
    public async Task<ActionResult> RemoveEmployee(
        Guid employeeId,
        [FromQuery] bool terminate = false);

    /// 6. Reenviar invitación
    /// POST /api/platform/employees/invitations/{invitationId}/resend
    [HttpPost("invitations/{invitationId}/resend")]
    public async Task<ActionResult> ResendInvitation(
        Guid invitationId);

    /// 7. Cancelar invitación
    /// DELETE /api/platform/employees/invitations/{invitationId}
    [HttpDelete("invitations/{invitationId}")]
    public async Task<ActionResult> CancelInvitation(
        Guid invitationId);

    /// 8. Listar invitaciones pendientes
    /// GET /api/platform/employees/invitations
    [HttpGet("invitations")]
    public async Task<ActionResult<PaginatedResult<PlatformInvitationDto>>> GetInvitations(
        [FromQuery] InvitationStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20);
}
```

---

### 3️⃣ **InvitationsController** (NO EXISTE)

#### Endpoints públicos para aceptar invitaciones:

```csharp
// UserService/UserService.Api/Controllers/InvitationsController.cs

[ApiController]
[Route("api/invitations")]
public class InvitationsController : ControllerBase
{
    /// 1. Validar token de invitación (dealer)
    /// GET /api/invitations/dealer/validate/{token}
    /// Retorna info de la invitación sin aceptarla
    [HttpGet("dealer/validate/{token}")]
    [AllowAnonymous]
    public async Task<ActionResult<InvitationDetailsDto>> ValidateDealerInvitation(
        string token);

    /// 2. Aceptar invitación de dealer
    /// POST /api/invitations/dealer/accept/{token}
    /// Body: { password (si es nuevo usuario) }
    [HttpPost("dealer/accept/{token}")]
    [AllowAnonymous]
    public async Task<ActionResult<AcceptInvitationResponse>> AcceptDealerInvitation(
        string token,
        [FromBody] AcceptInvitationRequest request);

    /// 3. Validar token de invitación (plataforma)
    /// GET /api/invitations/platform/validate/{token}
    [HttpGet("platform/validate/{token}")]
    [AllowAnonymous]
    public async Task<ActionResult<InvitationDetailsDto>> ValidatePlatformInvitation(
        string token);

    /// 4. Aceptar invitación de plataforma
    /// POST /api/invitations/platform/accept/{token}
    /// Body: { password (si es nuevo usuario) }
    [HttpPost("platform/accept/{token}")]
    [AllowAnonymous]
    public async Task<ActionResult<AcceptInvitationResponse>> AcceptPlatformInvitation(
        string token,
        [FromBody] AcceptInvitationRequest request);

    /// 5. Rechazar invitación
    /// POST /api/invitations/{token}/decline
    [HttpPost("{token}/decline")]
    [AllowAnonymous]
    public async Task<ActionResult> DeclineInvitation(string token);
}
```

---

## 📦 USECASES FALTANTES (NECESARIOS)

### Dealer Employees UseCases:

```
UserService.Application/UseCases/DealerEmployees/
├── InviteDealerEmployee/
│   ├── InviteDealerEmployeeCommand.cs
│   ├── InviteDealerEmployeeCommandHandler.cs
│   └── InviteDealerEmployeeValidator.cs
├── GetDealerEmployees/
│   ├── GetDealerEmployeesQuery.cs
│   └── GetDealerEmployeesQueryHandler.cs
├── GetDealerEmployee/
│   ├── GetDealerEmployeeQuery.cs
│   └── GetDealerEmployeeQueryHandler.cs
├── UpdateDealerEmployee/
│   ├── UpdateDealerEmployeeCommand.cs
│   ├── UpdateDealerEmployeeCommandHandler.cs
│   └── UpdateDealerEmployeeValidator.cs
├── RemoveDealerEmployee/
│   ├── RemoveDealerEmployeeCommand.cs
│   └── RemoveDealerEmployeeCommandHandler.cs
├── ResendDealerInvitation/
│   ├── ResendDealerInvitationCommand.cs
│   └── ResendDealerInvitationCommandHandler.cs
├── CancelDealerInvitation/
│   ├── CancelDealerInvitationCommand.cs
│   └── CancelDealerInvitationCommandHandler.cs
└── GetDealerInvitations/
    ├── GetDealerInvitationsQuery.cs
    └── GetDealerInvitationsQueryHandler.cs
```

### Platform Employees UseCases:

```
UserService.Application/UseCases/PlatformEmployees/
├── InvitePlatformEmployee/
├── GetPlatformEmployees/
├── GetPlatformEmployee/
├── UpdatePlatformEmployee/
├── RemovePlatformEmployee/
├── ResendPlatformInvitation/
├── CancelPlatformInvitation/
└── GetPlatformInvitations/
```

### Invitations UseCases:

```
UserService.Application/UseCases/Invitations/
├── ValidateDealerInvitation/
│   ├── ValidateDealerInvitationQuery.cs
│   └── ValidateDealerInvitationQueryHandler.cs
├── AcceptDealerInvitation/
│   ├── AcceptDealerInvitationCommand.cs
│   ├── AcceptDealerInvitationCommandHandler.cs
│   └── AcceptDealerInvitationValidator.cs
├── ValidatePlatformInvitation/
├── AcceptPlatformInvitation/
└── DeclineInvitation/
    ├── DeclineInvitationCommand.cs
    └── DeclineInvitationCommandHandler.cs
```

---

## 📝 DTOs FALTANTES

### Request DTOs:

```csharp
// Application/DTOs/DealerEmployees/

public record InviteDealerEmployeeRequest
{
    public string Email { get; init; }
    public DealerRole DealerRole { get; init; }
    public List<string> Permissions { get; init; } = new();
    public string? Notes { get; init; }
}

public record UpdateDealerEmployeeRequest
{
    public DealerRole? DealerRole { get; init; }
    public List<string>? Permissions { get; init; }
    public string? Notes { get; init; }
}

public record InvitePlatformEmployeeRequest
{
    public string Email { get; init; }
    public PlatformRole PlatformRole { get; init; }
    public List<string> Permissions { get; init; } = new();
    public string? Notes { get; init; }
}

public record UpdatePlatformEmployeeRequest
{
    public PlatformRole? PlatformRole { get; init; }
    public List<string>? Permissions { get; init; }
    public string? Notes { get; init; }
}

public record AcceptInvitationRequest
{
    public string? Password { get; init; } // Solo si es nuevo usuario
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}
```

### Response DTOs:

```csharp
// Application/DTOs/DealerEmployees/

public record DealerEmployeeDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Email { get; init; }
    public string FullName { get; init; }
    public string? Avatar { get; init; }
    public DealerRole DealerRole { get; init; }
    public List<string> Permissions { get; init; }
    public EmployeeStatus Status { get; init; }
    public DateTime InvitationDate { get; init; }
    public DateTime? ActivationDate { get; init; }
    public string InvitedByName { get; init; }
}

public record DealerEmployeeDetailDto : DealerEmployeeDto
{
    public string? Notes { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public List<ActivityLogDto> RecentActivity { get; init; } = new();
}

public record PlatformEmployeeDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Email { get; init; }
    public string FullName { get; init; }
    public string? Avatar { get; init; }
    public PlatformRole PlatformRole { get; init; }
    public List<string> Permissions { get; init; }
    public EmployeeStatus Status { get; init; }
    public DateTime InvitationDate { get; init; }
    public DateTime? ActivationDate { get; init; }
    public string InvitedByName { get; init; }
}

public record DealerInvitationDto
{
    public Guid Id { get; init; }
    public string Email { get; init; }
    public DealerRole DealerRole { get; init; }
    public List<string> Permissions { get; init; }
    public InvitationStatus Status { get; init; }
    public DateTime InvitationDate { get; init; }
    public DateTime ExpirationDate { get; init; }
    public string InvitedByName { get; init; }
}

public record PlatformInvitationDto
{
    public Guid Id { get; init; }
    public string Email { get; init; }
    public PlatformRole PlatformRole { get; init; }
    public List<string> Permissions { get; init; }
    public InvitationStatus Status { get; init; }
    public DateTime InvitationDate { get; init; }
    public DateTime ExpirationDate { get; init; }
    public string InvitedByName { get; init; }
}

public record InvitationResponse
{
    public Guid InvitationId { get; init; }
    public string Token { get; init; }
    public string InvitationLink { get; init; }
    public DateTime ExpirationDate { get; init; }
    public string Message { get; init; }
}

public record InvitationDetailsDto
{
    public string Email { get; init; }
    public string InviterName { get; init; }
    public string InviterCompany { get; init; } // Dealer name o "Platform Admin"
    public string RoleName { get; init; }
    public List<string> Permissions { get; init; }
    public DateTime ExpirationDate { get; init; }
    public bool IsExpired { get; init; }
    public bool AlreadyHasAccount { get; init; }
}

public record AcceptInvitationResponse
{
    public Guid UserId { get; init; }
    public string Email { get; init; }
    public string AccessToken { get; init; }
    public string RefreshToken { get; init; }
    public bool IsNewUser { get; init; }
    public string Message { get; init; }
}
```

---

## 🔐 AUTORIZACIÓN Y PERMISOS

### Policies requeridas:

```csharp
// Program.cs o Startup.cs

services.AddAuthorization(options =>
{
    // Dealer Employees Management
    options.AddPolicy("ManageDealerEmployees", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("accountType", "dealer") &&
            context.User.HasClaim("dealerRole", "owner") ||
            context.User.HasClaim("permission", "dealer:team:manage")
        ));

    options.AddPolicy("InviteDealerEmployees", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("accountType", "dealer") &&
            (context.User.HasClaim("dealerRole", "owner") ||
             context.User.HasClaim("permission", "dealer:team:invite"))
        ));

    // Platform Employees Management
    options.AddPolicy("ManagePlatformEmployees", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("accountType", "admin") &&
            context.User.HasClaim("platformRole", "super_admin")
        ));

    options.AddPolicy("InvitePlatformEmployees", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("accountType", "admin") &&
            (context.User.HasClaim("platformRole", "super_admin") ||
             context.User.HasClaim("permission", "platform:users:edit"))
        ));
});
```

---

## 🔗 INTEGRACIÓN ENTRE MICROSERVICIOS

### UserService → NotificationService (HTTP/gRPC):

```csharp
// UserService.Application/Services/NotificationHttpClient.cs
public interface INotificationClient
{
    Task SendEmployeeInvitationAsync(SendInvitationEmailRequest request);
    Task SendEmployeeWelcomeAsync(SendWelcomeEmailRequest request);
    Task SendRoleChangedAsync(SendRoleChangedEmailRequest request);
}

public class NotificationHttpClient : INotificationClient
{
    private readonly HttpClient _httpClient;
    
    public NotificationHttpClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("NotificationService");
    }
    
    public async Task SendEmployeeInvitationAsync(SendInvitationEmailRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/api/notifications/employee-invitation", 
            request
        );
        response.EnsureSuccessStatusCode();
    }
}

// Configuración en Program.cs
builder.Services.AddHttpClient("NotificationService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:NotificationService:Url"]);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<INotificationClient, NotificationHttpClient>();
```

### Llamada desde UseCase:

```csharp
// InviteDealerEmployeeCommandHandler.cs
public class InviteDealerEmployeeCommandHandler : IRequestHandler<InviteDealerEmployeeCommand, InvitationResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationClient _notificationClient;
    
    public async Task<InvitationResponse> Handle(InviteDealerEmployeeCommand request, CancellationToken ct)
    {
        // 1. Crear invitación en BD
        var invitation = new DealerEmployeeInvitation
        {
            Email = request.Email,
            DealerId = request.DealerId,
            Token = GenerateSecureToken(),
            // ...
        };
        await _context.DealerEmployeeInvitations.AddAsync(invitation, ct);
        await _context.SaveChangesAsync(ct);
        
        // 2. Enviar email vía NotificationService
        await _notificationClient.SendEmployeeInvitationAsync(new SendInvitationEmailRequest
        {
            ToEmail = invitation.Email,
            InviterName = request.InviterName,
            CompanyName = request.DealerName,
            InvitationLink = $"https://app.cardealer.com/invitations/accept/{invitation.Token}",
            RoleName = invitation.DealerRole.ToString(),
            ExpirationDate = invitation.ExpirationDate
        });
        
        return new InvitationResponse { ... };
    }
}
```

---

## 🎨 VISTAS FRONTEND FALTANTES

### Para DEALERS (Panel Dealer):

```
frontend/web/src/pages/dealer/team/
├── TeamManagementPage.tsx         ❌ Listar empleados del dealer
├── InviteEmployeePage.tsx         ❌ Form para invitar empleado
├── EmployeeDetailPage.tsx         ❌ Ver/editar empleado específico
├── PendingInvitationsPage.tsx     ❌ Listar invitaciones pendientes
└── components/
    ├── EmployeeCard.tsx           ❌ Card de empleado
    ├── InviteEmployeeModal.tsx    ❌ Modal de invitación
    ├── EmployeeRoleSelector.tsx   ❌ Selector de roles
    ├── PermissionsMatrix.tsx      ❌ Matriz de permisos
    └── EmployeeActivityLog.tsx    ❌ Log de actividad
```

### Para ADMINS (Panel Admin):

```
frontend/web/src/pages/admin/team/
├── PlatformTeamPage.tsx           ❌ Listar empleados de plataforma
├── InvitePlatformEmployeePage.tsx ❌ Form para invitar
├── PlatformEmployeeDetailPage.tsx ❌ Ver/editar empleado
└── components/
    ├── PlatformEmployeeCard.tsx   ❌ Card de empleado
    ├── InvitePlatformModal.tsx    ❌ Modal de invitación
    └── PlatformRoleSelector.tsx   ❌ Selector de roles
```

### Páginas públicas (Aceptar invitaciones):

```
frontend/web/src/pages/auth/
├── AcceptInvitationPage.tsx       ❌ Aceptar invitación de dealer
└── AcceptPlatformInvitationPage.tsx ❌ Aceptar invitación de plataforma
```

---

## 📧 INTEGRACIÓN CON NOTIFICATIONSERVICE

### Endpoints requeridos en NotificationService:

```csharp
// NotificationService/NotificationService.Api/Controllers/NotificationsController.cs

[HttpPost("employee-invitation")]
public async Task<IActionResult> SendEmployeeInvitation([FromBody] SendInvitationEmailRequest request)
{
    // Template: DealerEmployeeInvitation.cshtml o PlatformEmployeeInvitation.cshtml
    await _emailService.SendAsync(new EmailMessage
    {
        To = request.ToEmail,
        Subject = $"You've been invited to join {request.CompanyName}",
        Template = "EmployeeInvitation",
        Data = new
        {
            InviterName = request.InviterName,
            CompanyName = request.CompanyName,
            InvitationLink = request.InvitationLink,
            RoleName = request.RoleName,
            ExpirationDate = request.ExpirationDate.ToString("MMMM dd, yyyy")
        }
    });
    return Ok();
}

[HttpPost("employee-welcome")]
public async Task<IActionResult> SendEmployeeWelcome([FromBody] SendWelcomeEmailRequest request)
{
    // Template: EmployeeWelcome.cshtml
    return Ok();
}

[HttpPost("role-changed")]
public async Task<IActionResult> SendRoleChanged([FromBody] SendRoleChangedEmailRequest request)
{
    // Template: EmployeeRoleChanged.cshtml
    return Ok();
}
```

### Email templates necesarios:

```
backend/NotificationService/Templates/
├── EmployeeInvitation.cshtml          ❌ Email de invitación (dealer/platform)
├── EmployeeWelcome.cshtml             ❌ Bienvenida a nuevo empleado
├── EmployeeRoleChanged.cshtml         ❌ Cambio de rol
└── EmployeeRemoved.cshtml             ❌ Empleado removido
```

### Ejemplo de template (EmployeeInvitation.cshtml):

```html
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .button { background-color: #007bff; color: white; padding: 12px 24px; 
                  text-decoration: none; border-radius: 4px; display: inline-block; }
    </style>
</head>
<body>
    <div class="container">
        <h2>You've been invited to join @Model.CompanyName</h2>
        <p>Hi there,</p>
        <p><strong>@Model.InviterName</strong> has invited you to join <strong>@Model.CompanyName</strong> 
           as a <strong>@Model.RoleName</strong>.</p>
        
        <p>Click the button below to accept your invitation:</p>
        <p><a href="@Model.InvitationLink" class="button">Accept Invitation</a></p>
        
        <p><small>This invitation will expire on @Model.ExpirationDate</small></p>
        <p><small>If you didn't expect this invitation, you can safely ignore this email.</small></p>
    </div>
</body>
</html>
```

---

## 🚀 PLAN DE IMPLEMENTACIÓN RECOMENDADO (Arquitectura Microservicios)

### FASE 0: Preparación (0.5 días)

0. ✅ **Crear entidades de invitaciones en UserService.Domain**:
   - `DealerEmployeeInvitation.cs`
   - `PlatformEmployeeInvitation.cs`
   - `InvitationStatus.cs` (enum)
1. ✅ **Crear migration** en UserService para tablas de invitaciones
2. ✅ **Ejecutar migration** en PostgreSQL
3. ✅ **Agregar DbSets** en ApplicationDbContext

### FASE 1: Backend UserService - Core (2-3 días)

4. ✅ **Crear HttpClient para NotificationService**:
   - `INotificationClient` interface
   - `NotificationHttpClient` implementation
   - Configurar en Program.cs
5. ✅ **Crear DTOs** en UserService.Application:
   - Request DTOs (InviteDealerEmployeeRequest, etc.)
   - Response DTOs (DealerEmployeeDto, InvitationResponse, etc.)
6. ✅ **Implementar UseCases** en UserService.Application:
   - `InviteDealerEmployee/` (Command + Handler + Validator)
   - `GetDealerEmployees/` (Query + Handler)
   - `AcceptDealerInvitation/` (Command + Handler)
   - Similar para Platform employees
7. ✅ **Crear Controllers** en UserService.Api:
   - `DealerEmployeesController.cs` (8 endpoints)
   - `PlatformEmployeesController.cs` (8 endpoints)
   - `InvitationsController.cs` (5 endpoints públicos)
8. ✅ **Configurar políticas de autorización** en Program.cs

### FASE 2: Backend NotificationService - Templates (1 día)

9. ✅ **Crear endpoint** en NotificationService.Api:
   - POST `/api/notifications/employee-invitation`
   - POST `/api/notifications/employee-welcome`
   - POST `/api/notifications/role-changed`
10. ✅ **Crear email templates** en NotificationService/Templates:
    - `EmployeeInvitation.cshtml`
    - `EmployeeWelcome.cshtml`
    - `EmployeeRoleChanged.cshtml`
    - `EmployeeRemoved.cshtml`

### FASE 3: Frontend Dealer - Team Management (2-3 días)

11. ✅ **Crear servicio de API** en frontend:
    - `web/src/services/employeeService.ts` (llamadas a UserService)
12. ✅ **Crear páginas** en `web/src/pages/dealer/team/`:
    - `TeamManagementPage.tsx` (listar empleados + invitaciones)
    - `InviteEmployeePage.tsx` (form para invitar)
    - `EmployeeDetailPage.tsx` (ver/editar empleado)
13. ✅ **Crear componentes** reutilizables:
    - `EmployeeCard.tsx`
    - `InviteEmployeeModal.tsx`
    - `EmployeeRoleSelector.tsx` (dropdown con 6 roles)
    - `PermissionsMatrix.tsx` (checkbox matrix)
    - `EmployeeActivityLog.tsx`

### FASE 4: Frontend Invitaciones Públicas (1-2 días)

14. ✅ **Crear página de aceptación** en `web/src/pages/auth/`:
    - `AcceptInvitationPage.tsx` (dealer employees)
    - `AcceptPlatformInvitationPage.tsx` (platform employees)
15. ✅ **Integrar con AuthService**:
    - Auto-login después de aceptar
    - Redirect a dashboard correcto según AccountType

### FASE 5: Frontend Admin - Platform Team (2 días)

16. ✅ **Crear páginas** en `web/src/pages/admin/team/`:
    - `PlatformTeamPage.tsx`
    - `InvitePlatformEmployeePage.tsx`
    - `PlatformEmployeeDetailPage.tsx`
17. ✅ **Adaptar componentes** del dealer para admin:
    - `PlatformEmployeeCard.tsx`
    - `PlatformRoleSelector.tsx` (5 roles de plataforma)

### FASE 6: Testing & Documentación (2 días)

18. ✅ **Tests backend**:
    - Unit tests para UseCases (UserService)
    - Integration tests para Controllers
    - Tests de comunicación UserService ↔ NotificationService
19. ✅ **Tests frontend**:
    - Unit tests para components (Vitest)
    - E2E tests para flujo completo (Playwright):
      * Dealer invita empleado → Email → Aceptar → Login
20. ✅ **Documentación**:
    - Swagger en UserService actualizado
    - README con flujo de invitaciones
    - Postman collection actualizada

---

## 🎯 PRIORIDADES POR ROL

### Para DEALERS (Alta prioridad):
1. 🔴 Invitar empleados
2. 🔴 Listar empleados actuales
3. 🟡 Actualizar roles/permisos
4. 🟡 Ver actividad de empleados
5. 🟢 Desactivar empleados

### Para PLATFORM ADMINS (Media prioridad):
1. 🟡 Invitar empleados de plataforma
2. 🟡 Listar empleados de plataforma
3. 🟢 Gestionar roles/permisos

### Para EMPLEADOS (Alta prioridad):
1. 🔴 Aceptar invitación
2. 🟡 Ver sus propios permisos
3. 🟢 Actualizar perfil

---

## 💡 RECOMENDACIONES TÉCNICAS (Microservicios)

### 1. Seguridad:
- ✅ Tokens de invitación con expiración (7 días)
- ✅ CSRF protection en endpoints públicos
- ✅ Rate limiting en endpoints de invitación (usar RateLimitingService)
- ✅ Validar que el dealer tiene permisos suficientes
- ✅ Validar que no se auto-invite
- ✅ **API Gateway**: Validar JWT antes de llamar a UserService
- ✅ **Service-to-service auth**: UserService → NotificationService con API Key

### 2. UX:
- ✅ Auto-login después de aceptar invitación
- ✅ Preview de permisos antes de aceptar
- ✅ Notificaciones en tiempo real (SignalR si existe en tu stack)
- ✅ Indicadores visuales de estado (pending, active, etc.)
- ✅ **Loading states** durante llamadas a microservicios

### 3. Performance:
- ✅ Índices en tablas de invitaciones (email, token, status)
- ✅ Paginación en listados de empleados
- ✅ **Cache de permisos** en CacheService/Redis
- ✅ **Circuit breaker** para llamadas UserService → NotificationService
- ✅ **Retry policy** con exponential backoff (Polly)
- ✅ **Asynchronous email sending** (no bloquear aceptación de invitación)

### 4. Resiliencia:
- ✅ **Fallback**: Si NotificationService falla, guardar invitación igual
- ✅ **Dead Letter Queue**: Emails fallidos en MessageBusService
- ✅ **Health checks**: Verificar conectividad entre servicios
- ✅ **Distributed tracing**: Observar flujo UserService → NotificationService

### 5. Testing:
- ✅ Unit tests para cada UseCase
- ✅ Integration tests con TestContainers (PostgreSQL + Redis)
- ✅ **Contract tests** entre UserService y NotificationService (Pact)
- ✅ E2E tests con Playwright
- ✅ Load testing para endpoints públicos (K6 o JMeter)

---

## 📊 IMPACTO ESTIMADO (Arquitectura Microservicios)

| Componente | Servicio | Archivos nuevos | LOC estimado | Tiempo |
|------------|----------|----------------|--------------|--------|
| **Entidades & Migration** | UserService | 4 | ~250 | 0.5 días |
| **HttpClient Integration** | UserService | 3 | ~150 | 0.5 días |
| **DTOs** | UserService | ~15 | ~600 | 0.5 días |
| **UseCases** | UserService | ~24 | ~2200 | 2 días |
| **Controllers** | UserService | 3 | ~700 | 1 día |
| **Authorization Policies** | UserService | 1 | ~100 | 0.5 días |
| **Notification Endpoints** | NotificationService | 1 | ~150 | 0.5 días |
| **Email Templates** | NotificationService | 4 | ~400 | 0.5 días |
| **Frontend Services** | Frontend | 2 | ~300 | 0.5 días |
| **Frontend Pages** | Frontend | ~8 | ~2500 | 3 días |
| **Frontend Components** | Frontend | ~10 | ~1500 | 2 días |
| **Tests (Backend)** | UserService | ~20 | ~1500 | 1.5 días |
| **Tests (Frontend)** | Frontend | ~10 | ~800 | 1 día |
| **Documentación** | Varios | ~5 | ~500 | 0.5 días |
| **TOTAL** | **3 servicios** | **~110** | **~11,650** | **~15 días** |

### Desglose por servicio:

#### UserService (Backend Core):
- **Tiempo**: 5-6 días
- **Archivos**: ~75
- **LOC**: ~4,650
- **Complejidad**: 🔴 Alta (lógica de negocio + integración)

#### NotificationService (Email Templates):
- **Tiempo**: 1 día
- **Archivos**: ~5
- **LOC**: ~550
- **Complejidad**: 🟢 Baja (templates + endpoint simple)

#### Frontend (Web):
- **Tiempo**: 6-7 días
- **Archivos**: ~30
- **LOC**: ~5,100
- **Complejidad**: 🟡 Media (UI + state management)

#### Gateway (Routing):
- **Tiempo**: 0.5 días
- **Archivos**: 1
- **LOC**: ~50
- **Complejidad**: 🟢 Baja (agregar rutas a UserService)

### Paralelización posible:
- ✅ **Backend** (UserService + NotificationService): 1 dev, 6 días
- ✅ **Frontend**: 1 dev, 6-7 días
- 🔥 **Total con 2 devs**: ~7-8 días (paralelo)

| Componente | Archivos nuevos | LOC estimado | Tiempo |
|------------|----------------|--------------|--------|
| Controllers | 3 | ~600 | 1 día |
| UseCases | ~24 | ~2000 | 2 días |
| DTOs | ~15 | ~500 | 0.5 días |
| Frontend Pages | ~8 | ~2500 | 3 días |
| Components | ~10 | ~1500 | 2 días |
| Email Templates | 5 | ~300 | 0.5 días |
| Tests | ~30 | ~2000 | 2 días |
| **TOTAL** | **~95** | **~9400** | **11-12 días** |

---

## ✅ CHECKLIST PARA CONSIDERARSE COMPLETO

### Backend - UserService:
- [ ] **Entidades & Migration**:
  - [ ] `DealerEmployeeInvitation.cs` entity
  - [ ] `PlatformEmployeeInvitation.cs` entity
  - [ ] `InvitationStatus.cs` enum
  - [ ] Migration SQL ejecutada
  - [ ] DbSets agregados a ApplicationDbContext
- [ ] **Integration**:
  - [ ] `INotificationClient` interface
  - [ ] `NotificationHttpClient` implementation
  - [ ] HttpClient configurado en Program.cs
  - [ ] Circuit breaker configurado (Polly)
- [ ] **DTOs**:
  - [ ] Request DTOs (~8 clases)
  - [ ] Response DTOs (~7 clases)
- [ ] **UseCases** (24 total):
  - [ ] Dealer: Invite, Get, GetAll, Update, Remove, Resend, Cancel, GetInvitations
  - [ ] Platform: Invite, Get, GetAll, Update, Remove, Resend, Cancel, GetInvitations
  - [ ] Public: ValidateDealer, AcceptDealer, ValidatePlatform, AcceptPlatform, Decline
- [ ] **Controllers**:
  - [ ] `DealerEmployeesController` con 8 endpoints
  - [ ] `PlatformEmployeesController` con 8 endpoints
  - [ ] `InvitationsController` con 5 endpoints públicos
- [ ] **Authorization**:
  - [ ] Policies configuradas (ManageDealerEmployees, InviteDealerEmployees, etc.)
  - [ ] Claims validados en JWT
- [ ] **Tests**:
  - [ ] Unit tests para UseCases (>80% coverage)
  - [ ] Integration tests para Controllers
  - [ ] Contract tests con NotificationService (Pact)

### Backend - NotificationService:
- [ ] **Endpoints**:
  - [ ] POST `/api/notifications/employee-invitation`
  - [ ] POST `/api/notifications/employee-welcome`
  - [ ] POST `/api/notifications/role-changed`
- [ ] **Templates**:
  - [ ] `EmployeeInvitation.cshtml`
  - [ ] `EmployeeWelcome.cshtml`
  - [ ] `EmployeeRoleChanged.cshtml`
  - [ ] `EmployeeRemoved.cshtml`

### Backend - Gateway (Opcional):
- [ ] Rutas agregadas para nuevos endpoints UserService

### Frontend - Dealer Pages:
- [ ] **Service Layer**:
  - [ ] `employeeService.ts` (API calls)
  - [ ] Types/interfaces para employees
- [ ] **Pages**:
  - [ ] `TeamManagementPage.tsx` (listar empleados + invitaciones)
  - [ ] `InviteEmployeePage.tsx` (form invitar)
  - [ ] `EmployeeDetailPage.tsx` (ver/editar)
- [ ] **Components**:
  - [ ] `EmployeeCard.tsx`
  - [ ] `InviteEmployeeModal.tsx`
  - [ ] `EmployeeRoleSelector.tsx` (6 dealer roles)
  - [ ] `PermissionsMatrix.tsx`
  - [ ] `EmployeeActivityLog.tsx`

### Frontend - Admin Pages:
- [ ] **Pages**:
  - [ ] `PlatformTeamPage.tsx`
  - [ ] `InvitePlatformEmployeePage.tsx`
  - [ ] `PlatformEmployeeDetailPage.tsx`
- [ ] **Components**:
  - [ ] `PlatformEmployeeCard.tsx`
  - [ ] `PlatformRoleSelector.tsx` (5 platform roles)

### Frontend - Public Pages:
- [ ] **Pages**:
  - [ ] `AcceptInvitationPage.tsx` (dealer)
  - [ ] `AcceptPlatformInvitationPage.tsx` (platform)
- [ ] **Integration**:
  - [ ] Auto-login después de aceptar
  - [ ] Redirect a dashboard correcto
- [ ] **Tests**:
  - [ ] E2E test: Dealer invita → Email → Aceptar → Dashboard
  - [ ] E2E test: Admin invita → Email → Aceptar → Dashboard

### Documentación:
- [ ] Swagger actualizado con nuevos endpoints
- [ ] README con flujo de invitaciones
- [ ] Postman collection actualizada
- [ ] Architecture diagram actualizado
- [ ] Permiso matrix documentada

### Infraestructura:
- [ ] Variables de entorno configuradas (NotificationService URL)
- [ ] Health checks actualizados
- [ ] Logs estructurados (TracingService)
- [ ] Métricas configuradas (Prometheus)

---

**Estado Actual**: 📋 Documentado | ⏳ Pendiente de implementación  
**Arquitectura**: 🏗️ Microservicios (UserService + NotificationService + Frontend)  
**Próximo paso**: Comenzar FASE 0 - Crear entidades de invitaciones  
**Tiempo estimado**: 15 días (1 dev) | 7-8 días (2 devs en paralelo)
