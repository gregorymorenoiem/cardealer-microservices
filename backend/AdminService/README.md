# 👤 AdminService

Servicio de administración de usuarios con roles y permisos para el sistema CarDealer.

## 📋 Descripción

Microservicio responsable de la gestión administrativa de usuarios, incluyendo creación, actualización, asignación de roles y gestión de permisos.

## 🚀 Características

- **Gestión de Usuarios**: CRUD completo de usuarios administrativos
- **Roles y Permisos**: Asignación y validación de permisos
- **Auditoría**: Registro de todas las operaciones administrativas
- **Clean Architecture**: Separación en capas Domain, Application, Infrastructure, API
- **CQRS**: Implementado con MediatR
- **PostgreSQL**: Base de datos relacional
- **Error Handling**: Integración con ErrorService

## 🏗️ Arquitectura

```
AdminService.Api (Puerto 5010)
├── Controllers/
│   └── AdminController.cs
├── AdminService.Application/
│   ├── Commands/
│   ├── Queries/
│   └── Interfaces/
├── AdminService.Domain/
│   ├── Entities/
│   └── ValueObjects/
└── AdminService.Infrastructure/
    ├── Data/
    ├── Repositories/
    └── External/
```

## 📦 Dependencias Principales

- **.NET 8.0**
- **Entity Framework Core 8.0** - ORM
- **MediatR 12.2.0** - CQRS
- **FluentValidation 11.8.0** - Validación
- **Serilog** - Logging
- **Npgsql** - PostgreSQL provider

## ⚙️ Configuración

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=admindb;Username=admin;Password=***"
  },
  "ServiceUrls": {
    "ErrorService": "http://localhost:5001",
    "AuditService": "http://localhost:5002"
  }
}
```

### Variables de Entorno
```bash
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Host=postgres;Database=admindb;...
ErrorService__BaseUrl=http://errorservice
```

## 🔌 API Endpoints

### Usuarios
```http
GET    /api/admin/users              # Listar usuarios
GET    /api/admin/users/{id}         # Obtener usuario
POST   /api/admin/users              # Crear usuario
PUT    /api/admin/users/{id}         # Actualizar usuario
DELETE /api/admin/users/{id}         # Eliminar usuario
```

### Roles
```http
GET    /api/admin/users/{id}/roles   # Obtener roles de usuario
POST   /api/admin/users/{id}/roles   # Asignar rol
DELETE /api/admin/users/{id}/roles/{roleId}  # Remover rol
```

### Health Check
```http
GET /health
```

## 📝 Ejemplos de Uso

### Crear Usuario
```bash
curl -X POST http://localhost:5010/api/admin/users \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin@cardealer.com",
    "email": "admin@cardealer.com",
    "fullName": "Administrator",
    "roleIds": ["admin", "manager"]
  }'
```

### Asignar Rol
```bash
curl -X POST http://localhost:5010/api/admin/users/{userId}/roles \
  -H "Content-Type: application/json" \
  -d '{
    "roleId": "manager"
  }'
```

## 🧪 Testing

```bash
# Ejecutar tests
dotnet test AdminService.Tests/

# Con cobertura
dotnet test /p:CollectCoverage=true
```

## 🐳 Docker

```bash
# Build
docker build -t adminservice:latest .

# Run
docker run -d -p 5010:80 \
  -e ConnectionStrings__DefaultConnection="Host=postgres;Database=admindb;..." \
  --name adminservice \
  adminservice:latest
```

## 📊 Base de Datos

### Tablas Principales
- `Users` - Usuarios administrativos
- `Roles` - Roles del sistema
- `UserRoles` - Relación usuarios-roles
- `Permissions` - Permisos granulares
- `AuditLogs` - Registro de operaciones

### Migrations
```bash
# Crear migración
dotnet ef migrations add InitialCreate -p AdminService.Infrastructure

# Aplicar migraciones
dotnet ef database update -p AdminService.Infrastructure
```

## 🔐 Seguridad

- **Autenticación**: JWT tokens requeridos
- **Autorización**: Basada en roles y permisos
- **Auditoría**: Todas las operaciones son registradas
- **Validación**: FluentValidation en todas las entradas

## 📈 Monitoreo

### Logs
```bash
docker logs -f adminservice
```

### Métricas
- Operaciones de administración por tipo
- Tiempo de respuesta de queries
- Errores y excepciones

## 🚦 Estado

- ✅ **Build**: OK
- ✅ **Tests**: 100% pasando
- ✅ **Docker**: Configurado
- ✅ **Database**: Migrations listas

---

**Puerto**: 5010  
**Base de Datos**: PostgreSQL (admindb)  
**Estado**: ✅ Production Ready
