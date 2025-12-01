# 🔐 Arquitectura de Autenticación y Autorización - Sistema CarDealer

## 📊 Diagrama de Flujo Completo

```
┌──────────────────────────────────────────────────────────────────────┐
│                         LOGIN FLOW                                    │
└──────────────────────────────────────────────────────────────────────┘

1. Usuario hace login
   ┌─────────┐      POST /login      ┌──────────────┐
   │ Cliente │ ───────────────────▶  │ AuthService  │
   │  (UI)   │                        │              │
   └─────────┘                        └──────┬───────┘
                                             │
                                             │ Valida credenciales
                                             │ Genera JWT básico
                                             ▼
                                      ┌─────────────┐
                                      │ JWT Token   │
                                      │ Claims:     │
                                      │ - userId    │
                                      │ - email     │
                                      │ - exp       │
                                      └──────┬──────┘
                                             │
                                             ▼
                                      Token enviado al cliente


┌──────────────────────────────────────────────────────────────────────┐
│                    REQUEST FLOW CON AUTORIZACIÓN                     │
└──────────────────────────────────────────────────────────────────────┘

2. Usuario hace request con token
   ┌─────────┐   GET /api/vehicles   ┌─────────────┐
   │ Cliente │ ────────────────────▶  │   Gateway   │
   │  (UI)   │   Header:              │  (Ocelot)   │
   └─────────┘   Authorization:       └──────┬──────┘
                 Bearer {token}               │
                                              │ 1. Valida JWT
                                              │ 2. Extrae userId
                                              │ 3. Enriquece token
                                              ▼
                                    ┌──────────────────┐
                                    │  UserService     │
                                    │  GET /users/     │
                                    │  {userId}/roles  │
                                    └────────┬─────────┘
                                             │
                                             │ Retorna roles del usuario
                                             ▼
                                    ┌──────────────────┐
                                    │ RoleService      │
                                    │ GET /roles/{id}  │
                                    │ con permisos     │
                                    └────────┬─────────┘
                                             │
                                             │ Retorna permisos
                                             ▼
                                    ┌──────────────────┐
                                    │ JWT Enriquecido  │
                                    │ Claims:          │
                                    │ - userId         │
                                    │ - email          │
                                    │ - roles: [...]   │
                                    │ - permissions: []│
                                    └────────┬─────────┘
                                             │
                                             ▼
                                    ┌──────────────────┐
                                    │ VehicleService   │
                                    │ Verifica permisos│
                                    │ vehicles.read    │
                                    └──────────────────┘


┌──────────────────────────────────────────────────────────────────────┐
│                  MICROSERVICIOS Y SUS RELACIONES                     │
└──────────────────────────────────────────────────────────────────────┘

┌────────────────┐
│  AuthService   │  ◀── Genera JWT básico (userId, email)
└───────┬────────┘      Solo autenticación, NO autorización
        │
        │ JWT
        ▼
┌────────────────┐
│    Gateway     │  ◀── Punto de entrada único
│   (Ocelot)     │      Enriquece JWT con roles/permisos
└───┬────────┬───┘      Cachea permisos (Redis)
    │        │
    │        └──────────┐
    │                   │
    ▼                   ▼
┌────────────────┐  ┌────────────────┐
│  UserService   │  │  RoleService   │
│                │  │                │
│ Tabla:         │  │ Tabla:         │
│ - Users        │  │ - Roles        │
│ - UserRoles ───┼──▶ - Permissions  │
│                │  │ - RolePerms    │
└────────────────┘  └────────────────┘
     │
     │ Comunica con RoleService
     │ para validar roleIds
     │
     ▼
┌──────────────────────────────────────────┐
│  Business Services (Negocio)            │
│  - VehicleService                       │
│  - MediaService                         │
│  - ContactService                       │
│                                         │
│  Reciben JWT enriquecido con permisos   │
│  Validan permisos localmente            │
└──────────────────────────────────────────┘

┌──────────────────────────────────────────┐
│  Core Services (Infraestructura)        │
│  - AuditService    ◀── RabbitMQ Events  │
│  - NotificationService ◀── RabbitMQ     │
│  - ErrorService    ◀── RabbitMQ         │
│                                         │
│  ❌ NO participan en RBAC               │
│  ✅ Comunicación asíncrona (eventos)    │
└──────────────────────────────────────────┘
```

## 🔄 Flujo Detallado de Asignación de Roles

### **Paso 1: Crear Usuario en UserService**
```http
POST https://localhost:5002/api/users
Content-Type: application/json

{
  "email": "admin@cardealer.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe"
}

Response:
{
  "id": "user-guid-123",
  "email": "admin@cardealer.com"
}
```

### **Paso 2: Asignar Rol al Usuario**
```http
POST https://localhost:5002/api/users/user-guid-123/roles
Content-Type: application/json
Authorization: Bearer {admin-token}

{
  "roleId": "11111111-1111-1111-1111-111111111111"  // SuperAdmin
}

Flujo interno:
1. UserService valida que el usuario existe
2. UserService llama a RoleService: GET /api/roles/{roleId}
3. RoleService confirma que el rol existe
4. UserService crea registro en tabla UserRoles:
   {
     "id": "new-guid",
     "userId": "user-guid-123",
     "roleId": "11111111-1111-1111-1111-111111111111",
     "assignedAt": "2024-12-01T...",
     "assignedBy": "admin-user",
     "isActive": true
   }
```

### **Paso 3: Login y Obtención de Permisos**
```http
POST https://localhost:5001/api/auth/login
Content-Type: application/json

{
  "email": "admin@cardealer.com",
  "password": "SecurePass123!"
}

Flujo:
1. AuthService valida credenciales
2. AuthService genera JWT básico:
   {
     "sub": "user-guid-123",
     "email": "admin@cardealer.com",
     "exp": 1234567890
   }
3. Cliente recibe token

4. En la primera request al Gateway:
   Gateway intercepta el token
   Gateway llama a UserService: GET /users/user-guid-123/roles
   UserService retorna:
   {
     "userId": "user-guid-123",
     "roles": [
       {
         "roleId": "11111111-1111-1111-1111-111111111111",
         "roleName": "SuperAdmin",
         "priority": 100,
         "permissions": [
           {"resource": "users", "action": "All"},
           {"resource": "roles", "action": "All"},
           ...
         ]
       }
     ]
   }

5. Gateway enriquece JWT y cachea en Redis:
   Key: "user:user-guid-123:permissions"
   Value: ["users.all", "roles.all", "vehicles.all", ...]
   TTL: 15 minutos
```

## 🗄️ Schema de Base de Datos - UserService

```sql
-- Tabla Users (UserService)
CREATE TABLE "Users" (
    "Id" uuid PRIMARY KEY,
    "Email" varchar(255) UNIQUE NOT NULL,
    "PasswordHash" varchar(500) NOT NULL,
    "FirstName" varchar(100),
    "LastName" varchar(100),
    "IsActive" boolean DEFAULT true,
    "CreatedAt" timestamp NOT NULL,
    "UpdatedAt" timestamp
);

-- Tabla UserRoles (UserService)
CREATE TABLE "UserRoles" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "RoleId" uuid NOT NULL,  -- NO FK porque está en otro servicio
    "AssignedAt" timestamp NOT NULL,
    "AssignedBy" varchar(100) NOT NULL,
    "RevokedAt" timestamp,
    "RevokedBy" varchar(100),
    "IsActive" boolean DEFAULT true,
    
    CONSTRAINT "UQ_UserRoles_UserId_RoleId" UNIQUE ("UserId", "RoleId")
);

CREATE INDEX "IX_UserRoles_UserId" ON "UserRoles"("UserId");
CREATE INDEX "IX_UserRoles_RoleId" ON "UserRoles"("RoleId");
CREATE INDEX "IX_UserRoles_IsActive" ON "UserRoles"("IsActive");
```

## 🚀 Endpoints del Sistema

### **AuthService (Puerto 5001)**
```
POST   /api/auth/register         - Registrar usuario
POST   /api/auth/login            - Login y obtener JWT
POST   /api/auth/refresh-token    - Refrescar token
POST   /api/auth/logout           - Logout
GET    /api/auth/me               - Obtener usuario actual
```

### **UserService (Puerto 5002)**
```
POST   /api/users                     - Crear usuario
GET    /api/users/{id}                - Obtener usuario
PUT    /api/users/{id}                - Actualizar usuario
DELETE /api/users/{id}                - Eliminar usuario

GET    /api/users/{id}/roles          - Obtener roles del usuario
POST   /api/users/{id}/roles          - Asignar rol al usuario
DELETE /api/users/{id}/roles/{roleId} - Revocar rol del usuario

GET    /api/users/{id}/permissions/check?resource=users&action=create
                                       - Verificar permiso
```

### **RoleService (Puerto 5003)**
```
GET    /api/roles                 - Listar roles
POST   /api/roles                 - Crear rol
GET    /api/roles/{id}            - Obtener rol con permisos
PUT    /api/roles/{id}            - Actualizar rol
DELETE /api/roles/{id}            - Eliminar rol

GET    /api/permissions           - Listar permisos
POST   /api/permissions           - Crear permiso

POST   /api/role-permissions/assign  - Asignar permiso a rol
POST   /api/role-permissions/remove  - Remover permiso de rol
POST   /api/role-permissions/check   - Verificar permiso de rol
```

### **Gateway (Puerto 5000)**
```
/*  - Proxy a todos los servicios
    - Enriquecimiento de JWT
    - Rate limiting
    - Logging centralizado
```

## 💾 Cacheo de Permisos en Gateway

```csharp
// Middleware en Gateway para enriquecer JWT
public class JwtEnrichmentMiddleware
{
    private readonly IDistributedCache _cache;
    private readonly HttpClient _userServiceClient;

    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
            return;

        // Intentar obtener permisos del cache
        var cacheKey = $"user:{userId}:permissions";
        var cachedPermissions = await _cache.GetStringAsync(cacheKey);

        List<string> permissions;
        if (!string.IsNullOrEmpty(cachedPermissions))
        {
            permissions = JsonSerializer.Deserialize<List<string>>(cachedPermissions);
        }
        else
        {
            // Obtener permisos de UserService
            var userRoles = await _userServiceClient
                .GetFromJsonAsync<UserRolesResponse>($"/api/users/{userId}/roles");
            
            permissions = userRoles.Roles
                .SelectMany(r => r.Permissions)
                .Select(p => $"{p.Resource}.{p.Action}".ToLower())
                .Distinct()
                .ToList();

            // Cachear por 15 minutos
            await _cache.SetStringAsync(cacheKey, 
                JsonSerializer.Serialize(permissions),
                new DistributedCacheEntryOptions 
                { 
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) 
                });
        }

        // Agregar permisos al contexto
        var claims = permissions.Select(p => new Claim("permission", p));
        var identity = new ClaimsIdentity(claims);
        context.User.AddIdentity(identity);
    }
}
```

## 📝 Ejemplo Completo de Uso

```csharp
// 1. Admin asigna rol SuperAdmin al usuario
POST /api/users/user-123/roles
{
  "roleId": "11111111-1111-1111-1111-111111111111"
}

// 2. Usuario hace login
POST /api/auth/login
{
  "email": "admin@example.com",
  "password": "pass123"
}
// Recibe: JWT básico

// 3. Usuario hace request
GET /api/vehicles
Authorization: Bearer {jwt}

// 4. Gateway:
//    - Valida JWT
//    - Obtiene roles de UserService
//    - Obtiene permisos de RoleService
//    - Cachea permisos
//    - Enriquece JWT
//    - Forward a VehicleService

// 5. VehicleService:
//    - Verifica claim "permission" contains "vehicles.read"
//    - Retorna datos si tiene permiso
//    - Retorna 403 Forbidden si no tiene permiso
```

## 🔗 Comunicación entre RoleService y otros Microservicios

### **❌ RoleService NO se comunica con:**
- **AuthService** - NO valida tokens JWT, NO autentica usuarios
- **Otros servicios** - Es un servicio pasivo que solo responde consultas

### **✅ RoleService se comunica directamente con:**
- **Ninguno** - Es un servicio de lectura/escritura que NO hace llamadas salientes
- Solo expone endpoints REST para que otros servicios lo consulten

---

### **📞 Microservicios que SE COMUNICAN CON RoleService:**

#### **1. UserService → RoleService**
```
Propósito: Validar roles antes de asignarlos a usuarios
Llamadas:
  - GET /api/roles/{id}  - Verificar que rol existe
  - GET /api/roles/{id}  - Obtener información del rol con permisos
  
Usado en:
  - AssignRoleToUserCommand (validar roleId antes de crear UserRole)
  - GetUserRolesQuery (obtener detalles de roles del usuario)
```

#### **2. Gateway → RoleService**
```
Propósito: Enriquecer JWT con permisos después de autenticación
Llamadas:
  - GET /api/roles/{id}  - Obtener permisos de un rol
  
Flujo:
  1. Usuario se loguea en AuthService → recibe JWT básico
  2. Gateway intercepta request con JWT
  3. Gateway consulta UserService → obtiene roleIds del usuario
  4. Gateway consulta RoleService → obtiene permisos de cada rol
  5. Gateway enriquece JWT con claims de permisos
  6. Gateway cachea permisos en Redis (15 min)
  
Usado en:
  - JwtEnrichmentMiddleware
```

#### **3. AdminService → RoleService**
```
Propósito: Administrar el sistema RBAC
Llamadas:
  - GET    /api/roles           - Listar todos los roles
  - POST   /api/roles           - Crear nuevos roles
  - PUT    /api/roles/{id}      - Actualizar roles
  - DELETE /api/roles/{id}      - Eliminar roles
  - POST   /api/permissions     - Crear nuevos permisos
  - POST   /api/role-permissions/assign  - Asignar permisos a roles
  
Usado en:
  - Panel de administración RBAC
  - Scripts de configuración inicial
```

---

### **🚫 Servicios que NO llaman a RoleService:**

#### **AuthService**
```
❌ NO se comunica con RoleService
✅ Solo autentica (valida email/password)
✅ Solo genera JWT básico con userId y email
❌ NO conoce roles ni permisos
```

#### **Business Services (Servicios de Negocio)**
```
VehicleService, MediaService, ContactService

❌ NO llaman directamente a RoleService
✅ Reciben JWT ya enriquecido desde Gateway
✅ Validan permisos usando claims del JWT
✅ Ejemplo: VehicleService verifica si JWT contiene claim "permission:vehicles.read"
```

#### **Core Services (Servicios de Infraestructura)**
```
AuditService, NotificationService, ErrorService

❌ NO llaman a RoleService
❌ RoleService NO llama a estos servicios
✅ Son servicios pasivos que reciben eventos
✅ Comunicación vía RabbitMQ (event-driven)
✅ No participan en flujo de autenticación/autorización

Ejemplos:
  - AuditService: Escucha eventos de auditoría por RabbitMQ
  - NotificationService: Escucha eventos de notificaciones por RabbitMQ  
  - ErrorService: Recibe logs de errores por RabbitMQ
```

---

### **🔄 Diagrama de Comunicación Completo**

```
                    ┌──────────────┐
                    │ AuthService  │
                    │              │
                    │ ❌ NO llama  │
                    │ RoleService  │
                    └──────┬───────┘
                           │
                           │ 1. Genera JWT básico
                           │    (userId, email)
                           ▼
┌─────────────────────────────────────────────┐
│                   Gateway                    │
│                                             │
│  2. Valida JWT                              │
│  3. Llama UserService → obtiene roleIds     │
│  4. Llama RoleService → obtiene permisos ✅ │
│  5. Enriquece JWT con permisos              │
└────────────┬────────────────────────────────┘
             │
    ┌────────┴────────┐
    ▼                 ▼
┌──────────┐    ┌─────────────┐
│UserService│    │ RoleService │
│          │    │             │
│Llama ✅ ─┼───▶│  Endpoints: │
│          │    │  - Roles    │
└──────────┘    │  - Perms    │
                │  - RolePerms│
                └─────────────┘
                      ▲
                      │
                      │ También llama
                      │
                ┌─────┴──────┐
                │AdminService│
                └────────────┘
```

---

### **📋 Tabla Resumen de Comunicaciones**

| Servicio             | Tipo        | Llama a RoleService | Llama a AuthService | Recibe JWT | Comunicación |
|----------------------|-------------|---------------------|---------------------|------------|--------------|
| **AuthService**      | Auth        | ❌ NO               | -                   | ❌ NO      | HTTP REST    |
| **RoleService**      | Auth        | -                   | ❌ NO               | ✅ SI      | HTTP REST    |
| **UserService**      | Auth        | ✅ SI               | ❌ NO               | ✅ SI      | HTTP REST    |
| **Gateway**          | Proxy       | ✅ SI               | ❌ NO*              | ✅ SI      | HTTP REST    |
| **AdminService**     | Business    | ✅ SI               | ✅ SI (login)       | ✅ SI      | HTTP REST    |
| **VehicleService**   | Business    | ❌ NO               | ❌ NO               | ✅ SI      | HTTP REST    |
| **MediaService**     | Business    | ❌ NO               | ❌ NO               | ✅ SI      | HTTP REST    |
| **ContactService**   | Business    | ❌ NO               | ❌ NO               | ✅ SI      | HTTP REST    |
| **AuditService**     | Core/Infra  | ❌ NO               | ❌ NO               | ❌ NO      | RabbitMQ     |
| **NotificationService** | Core/Infra | ❌ NO            | ❌ NO               | ❌ NO      | RabbitMQ     |
| **ErrorService**     | Core/Infra  | ❌ NO               | ❌ NO               | ❌ NO      | RabbitMQ     |

\* Gateway puede validar JWT usando configuración de AuthService, pero no hace llamadas HTTP directas

---

### **🔧 Core Services - Patrón Event-Driven**

Los **Core Services** NO participan en el flujo HTTP/REST:

```
┌──────────────┐     Publica evento      ┌─────────────┐
│ RoleService  │────────────────────────▶│  RabbitMQ   │
│              │  "role.created"         │   Queue     │
└──────────────┘                         └──────┬──────┘
                                                │
                                                │ Consume evento
                                                ▼
                                      ┌──────────────────┐
                                      │  AuditService    │
                                      │  Registra acción │
                                      └──────────────────┘

Ejemplos de eventos desde RoleService:
  - role.created       → AuditService registra auditoría
  - role.updated       → AuditService registra cambio
  - role.deleted       → AuditService registra eliminación
  - permission.granted → NotificationService envía email a admin
  - error.occurred     → ErrorService registra error
```

**Importante**: 
- RoleService NO llama directamente a AuditService/NotificationService/ErrorService
- Solo publica eventos en RabbitMQ
- Core Services escuchan eventos de forma asíncrona

