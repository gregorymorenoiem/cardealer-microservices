# RoleService - Guía de Pruebas RBAC

## 🚀 Inicio Rápido

### 1. Prerequisitos
- PostgreSQL corriendo en puerto 25432
- RoleService corriendo: `dotnet run --project RoleService.Api`
- Swagger: https://localhost:45952/swagger

### 2. Datos de Prueba Cargados

#### Roles Disponibles
```
SuperAdmin (Priority 100) - 10 permisos - Sistema
Admin      (Priority 90)  - 9 permisos  - Sistema
Manager    (Priority 70)  - 8 permisos
User       (Priority 50)  - 2 permisos
ReadOnly   (Priority 30)  - 5 permisos
```

#### IDs de Roles (para pruebas)
```
SuperAdmin: 11111111-1111-1111-1111-111111111111
Admin:      22222222-2222-2222-2222-222222222222
Manager:    33333333-3333-3333-3333-333333333333
User:       44444444-4444-4444-4444-444444444444
ReadOnly:   55555555-5555-5555-5555-555555555555
```

## 📋 Pruebas de Endpoints

### ✅ 1. Listar Roles (GET /api/roles)

**Request**:
```http
GET https://localhost:45952/api/roles?page=1&pageSize=10&isActive=true
Authorization: Bearer {token}
```

**Response esperado**:
```json
{
  "items": [
    {
      "id": "11111111-1111-1111-1111-111111111111",
      "name": "SuperAdmin",
      "description": "Administrador con todos los permisos del sistema",
      "priority": 100,
      "isActive": true,
      "isSystemRole": true,
      "permissionCount": 10,
      "createdAt": "2024-12-01T..."
    },
    ...
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 5,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

### ✅ 2. Obtener Rol por ID (GET /api/roles/{id})

**Request**:
```http
GET https://localhost:45952/api/roles/11111111-1111-1111-1111-111111111111
Authorization: Bearer {token}
```

**Response esperado**:
```json
{
  "id": "11111111-1111-1111-1111-111111111111",
  "name": "SuperAdmin",
  "description": "Administrador con todos los permisos del sistema",
  "priority": 100,
  "isActive": true,
  "isSystemRole": true,
  "permissions": [
    {
      "id": "a0000001-0000-0000-0000-000000000005",
      "name": "users.all",
      "resource": "users",
      "action": "All",
      "module": "UserService"
    },
    ...
  ],
  "createdAt": "2024-12-01T...",
  "createdBy": "system"
}
```

### ✅ 3. Crear Nuevo Rol (POST /api/roles)

**Request**:
```http
POST https://localhost:45952/api/roles
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Editor",
  "description": "Usuario con permisos de edición",
  "priority": 60
}
```

**Response esperado** (201 Created):
```json
{
  "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "name": "Editor",
  "description": "Usuario con permisos de edición",
  "priority": 60,
  "isActive": true,
  "isSystemRole": false,
  "createdAt": "2024-12-01T..."
}
```

**Validaciones**:
- ❌ Name vacío → 400 Bad Request
- ❌ Name duplicado → 400 Bad Request
- ❌ Priority < 0 o > 100 → 400 Bad Request
- ❌ Description > 500 caracteres → 400 Bad Request

### ✅ 4. Actualizar Rol (PUT /api/roles/{id})

**Request**:
```http
PUT https://localhost:45952/api/roles/44444444-4444-4444-4444-444444444444
Authorization: Bearer {token}
Content-Type: application/json

{
  "description": "Usuario estándar actualizado",
  "priority": 55,
  "isActive": true
}
```

**Response esperado** (200 OK):
```json
{
  "id": "44444444-4444-4444-4444-444444444444",
  "name": "User",
  "description": "Usuario estándar actualizado",
  "priority": 55,
  "isActive": true,
  "isSystemRole": false,
  "updatedAt": "2024-12-01T..."
}
```

**Restricciones**:
- ❌ No se puede modificar un rol de sistema (IsSystemRole=true)
- ❌ No se puede cambiar el nombre a uno existente

### ✅ 5. Eliminar Rol (DELETE /api/roles/{id})

**Request**:
```http
DELETE https://localhost:45952/api/roles/44444444-4444-4444-4444-444444444444
Authorization: Bearer {token}
```

**Response esperado** (204 No Content)

**Restricciones**:
- ❌ No se pueden eliminar roles de sistema
- ✅ Elimina automáticamente todas las asignaciones de permisos (cascade)

### ✅ 6. Listar Permisos (GET /api/permissions)

**Request**:
```http
GET https://localhost:45952/api/permissions?module=UserService
Authorization: Bearer {token}
```

**Response esperado**:
```json
[
  {
    "id": "a0000001-0000-0000-0000-000000000001",
    "name": "users.create",
    "description": "Crear nuevos usuarios",
    "resource": "users",
    "action": "Create",
    "module": "UserService",
    "isActive": true,
    "isSystemPermission": true,
    "createdAt": "2024-12-01T..."
  },
  ...
]
```

**Filtros disponibles**:
- `?module=UserService` - Filtrar por módulo
- `?resource=users` - Filtrar por recurso

### ✅ 7. Crear Permiso (POST /api/permissions)

**Request**:
```http
POST https://localhost:45952/api/permissions
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "reports.generate",
  "description": "Generar reportes del sistema",
  "resource": "reports",
  "action": "Execute",
  "module": "ReportService"
}
```

**Response esperado** (201 Created):
```json
{
  "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "name": "reports.generate",
  "description": "Generar reportes del sistema",
  "resource": "reports",
  "action": "Execute",
  "module": "ReportService",
  "isActive": true,
  "isSystemPermission": false,
  "createdAt": "2024-12-01T..."
}
```

**Validaciones**:
- ❌ Action debe ser: Create, Read, Update, Delete, Execute, All
- ❌ Combinación Resource+Action+Module debe ser única
- ❌ Name debe tener formato: `{resource}.{action}` (lowercase)

### ✅ 8. Asignar Permiso a Rol (POST /api/role-permissions/assign)

**Request**:
```http
POST https://localhost:45952/api/role-permissions/assign
Authorization: Bearer {token}
Content-Type: application/json

{
  "roleId": "44444444-4444-4444-4444-444444444444",
  "permissionId": "a0000004-0000-0000-0000-000000000003"
}
```

**Response esperado** (200 OK):
```json
{
  "success": true,
  "message": "Permission assigned successfully"
}
```

**Validaciones**:
- ❌ RoleId no existe → 404 Not Found
- ❌ PermissionId no existe → 404 Not Found
- ❌ Permiso ya asignado → 400 Bad Request

### ✅ 9. Remover Permiso de Rol (POST /api/role-permissions/remove)

**Request**:
```http
POST https://localhost:45952/api/role-permissions/remove
Authorization: Bearer {token}
Content-Type: application/json

{
  "roleId": "44444444-4444-4444-4444-444444444444",
  "permissionId": "a0000004-0000-0000-0000-000000000003"
}
```

**Response esperado** (200 OK):
```json
{
  "success": true,
  "message": "Permission removed successfully"
}
```

### ✅ 10. Verificar Permiso (POST /api/role-permissions/check)

**Request**:
```http
POST https://localhost:45952/api/role-permissions/check
Authorization: Bearer {token}
Content-Type: application/json

{
  "roleId": "11111111-1111-1111-1111-111111111111",
  "resource": "users",
  "action": "Create"
}
```

**Response esperado**:
```json
{
  "hasPermission": true,
  "matchedPermission": {
    "id": "a0000001-0000-0000-0000-000000000005",
    "name": "users.all",
    "action": "All"
  }
}
```

**Casos de prueba**:
- ✅ SuperAdmin tiene `users.all` → hasPermission=true para cualquier acción sobre users
- ✅ User tiene `vehicles.read` → hasPermission=true solo para Read
- ❌ User no tiene `users.create` → hasPermission=false

## 🔧 Escenarios de Prueba Completos

### Escenario 1: Gestión de Roles
```bash
1. GET /api/roles → Ver todos los roles
2. POST /api/roles → Crear rol "Editor"
3. GET /api/roles/{editorId} → Ver detalles del nuevo rol
4. PUT /api/roles/{editorId} → Actualizar descripción
5. DELETE /api/roles/{editorId} → Eliminar rol
```

### Escenario 2: Sistema de Permisos
```bash
1. GET /api/permissions?module=UserService → Ver permisos de usuarios
2. POST /api/permissions → Crear permiso "reports.generate"
3. POST /api/role-permissions/assign → Asignar a rol Manager
4. POST /api/role-permissions/check → Verificar que Manager lo tiene
5. POST /api/role-permissions/remove → Remover permiso
6. POST /api/role-permissions/check → Verificar que ya no lo tiene
```

### Escenario 3: Jerarquía de Permisos
```bash
1. POST /api/role-permissions/check con SuperAdmin + users.create → TRUE (tiene users.all)
2. POST /api/role-permissions/check con Admin + users.create → TRUE (tiene users.create específico)
3. POST /api/role-permissions/check con User + users.create → FALSE (solo tiene users.read implícito)
```

## 🎯 Matriz de Permisos por Rol

| Permiso          | SuperAdmin | Admin | Manager | User | ReadOnly |
|------------------|------------|-------|---------|------|----------|
| users.create     | ✅ (all)   | ✅    | ❌      | ❌   | ❌       |
| users.read       | ✅ (all)   | ✅    | ✅      | ❌   | ✅       |
| users.update     | ✅ (all)   | ✅    | ❌      | ❌   | ❌       |
| users.delete     | ✅ (all)   | ✅    | ❌      | ❌   | ❌       |
| roles.create     | ✅ (all)   | ❌    | ❌      | ❌   | ❌       |
| roles.read       | ✅ (all)   | ✅    | ❌      | ❌   | ✅       |
| roles.update     | ✅ (all)   | ✅    | ❌      | ❌   | ❌       |
| permissions.read | ✅ (all)   | ✅    | ❌      | ❌   | ✅       |
| vehicles.create  | ✅         | ❌    | ✅      | ❌   | ❌       |
| vehicles.read    | ✅         | ✅    | ✅      | ✅   | ✅       |
| vehicles.update  | ✅         | ❌    | ✅      | ❌   | ❌       |
| vehicles.delete  | ✅         | ❌    | ✅      | ❌   | ❌       |
| media.create     | ✅         | ❌    | ✅      | ❌   | ❌       |
| media.read       | ✅         | ✅    | ✅      | ✅   | ✅       |
| media.delete     | ✅         | ❌    | ✅      | ❌   | ❌       |

## 🔐 Autenticación JWT

**Nota**: Todos los endpoints requieren JWT con claim "RoleServiceAccess". 

Para pruebas sin JWT, puedes comentar temporalmente `[Authorize(Policy = "RoleServiceAccess")]` en los controllers.

## 📊 Rate Limiting

Todos los endpoints tienen límites configurados:
- GET endpoints: 200 requests / 60 segundos
- POST/PUT/DELETE: 50-100 requests / 60 segundos
- Health endpoint: Sin límite

## ✅ Checklist de Pruebas

- [ ] Listar roles paginados
- [ ] Obtener rol por ID con permisos incluidos
- [ ] Crear rol nuevo (validar campos)
- [ ] Actualizar rol existente
- [ ] Intentar actualizar rol de sistema (debe fallar)
- [ ] Eliminar rol no-sistema
- [ ] Intentar eliminar rol de sistema (debe fallar)
- [ ] Listar permisos con filtros
- [ ] Crear permiso nuevo
- [ ] Asignar permiso a rol
- [ ] Intentar asignar permiso duplicado (debe fallar)
- [ ] Verificar permiso específico
- [ ] Verificar permiso con wildcard (.all)
- [ ] Remover permiso de rol
- [ ] Verificar cascade delete de role-permissions
