# 🧪 Estrategia de Testing - RoleService v2.0

**Fecha:** Enero 9, 2026  
**Estado:** ✅ Estrategia Completa Definida  
**Objetivo:** Testing comprehensivo del sistema RBAC con 80%+ coverage

---

## 📊 Análisis de Componentes a Testear

### 1️⃣ Handlers (10 handlers = 50 tests estimados)

#### Roles (5 handlers → 25 tests)

**CreateRoleCommandHandler** (5 tests):

- ✅ Handle_ValidRequest_ShouldCreateRoleSuccessfully
- ✅ Handle_DuplicateName_ShouldThrowConflictException
- ✅ Handle_WithPermissions_ShouldAssignPermissionsCorrectly
- ✅ Handle_NotificationFailure_ShouldNotAffectRoleCreation
- ✅ Handle_AuditLog_ShouldRecordCreation

**UpdateRoleCommandHandler** (5 tests):

- ✅ Handle_ValidUpdate_ShouldUpdateRoleSuccessfully
- ✅ Handle_SystemRole_ShouldThrowForbiddenException
- ✅ Handle_RoleNotFound_ShouldThrowNotFoundException
- ✅ Handle_WithPermissionSync_ShouldSyncPermissionsCorrectly
- ✅ Handle_CacheInvalidation_ShouldInvalidateRolePermissions

**DeleteRoleCommandHandler** (5 tests):

- ✅ Handle_ValidDeletion_ShouldSoftDeleteRole
- ✅ Handle_SystemRole_ShouldThrowForbiddenException
- ✅ Handle_RoleWithUsers_ShouldThrowConflictException
- ✅ Handle_RoleNotFound_ShouldThrowNotFoundException
- ✅ Handle_AuditLog_ShouldRecordDeletion

**GetRolesQueryHandler** (5 tests):

- ⏳ Handle_WithPagination_ShouldReturnPagedResults
- ⏳ Handle_WithSearchTerm_ShouldFilterByName
- ⏳ Handle_WithStatusFilter_ShouldFilterByStatus
- ⏳ Handle_OrderBy_ShouldSortCorrectly
- ⏳ Handle_EmptyResult_ShouldReturnEmptyPage

**GetRoleByIdQueryHandler** (5 tests):

- ⏳ Handle_ValidId_ShouldReturnRoleWithPermissions
- ⏳ Handle_InvalidId_ShouldThrowNotFoundException
- ⏳ Handle_InactiveRole_ShouldReturnWhenIncludeInactive
- ⏳ Handle_WithoutPermissions_ShouldReturnOnlyRoleData
- ⏳ Handle_SystemRole_ShouldReturnCorrectly

#### Permissions (2 handlers → 10 tests)

**CreatePermissionCommandHandler** (5 tests):

- ✅ Handle_ValidPermission_ShouldCreateSuccessfully
- ✅ Handle_InvalidModule_ShouldThrowBadRequestException
- ✅ Handle_DuplicatePermission_ShouldThrowConflictException
- ✅ Handle_ValidModules_ShouldAcceptAllAllowedModules (Theory)
- ✅ Handle_AuditLog_ShouldRecordCreation

**GetPermissionsQueryHandler** (5 tests):

- ⏳ Handle_WithModuleFilter_ShouldFilterByModule
- ⏳ Handle_WithResourceFilter_ShouldFilterByResource
- ⏳ Handle_WithActionFilter_ShouldFilterByAction
- ⏳ Handle_WithoutFilters_ShouldReturnAll
- ⏳ Handle_WithPagination_ShouldReturnPagedResults

#### RolePermissions (3 handlers → 15 tests)

**AssignPermissionCommandHandler** (5 tests):

- ✅ Handle_ValidAssignment_ShouldAssignPermissionSuccessfully
- ✅ Handle_SystemRole_ShouldThrowForbiddenException
- ✅ Handle_DuplicateAssignment_ShouldThrowConflictException
- ✅ Handle_RoleNotFound_ShouldThrowNotFoundException
- ✅ Handle_PermissionNotFound_ShouldThrowNotFoundException

**RemovePermissionCommandHandler** (5 tests):

- ⏳ Handle_ValidRemoval_ShouldRemovePermissionSuccessfully
- ⏳ Handle_SystemRole_ShouldThrowForbiddenException
- ⏳ Handle_PermissionNotAssigned_ShouldThrowNotFoundException
- ⏳ Handle_RoleNotFound_ShouldThrowNotFoundException
- ⏳ Handle_CacheInvalidation_ShouldInvalidateRolePermissions

**CheckPermissionQueryHandler** (5 tests):

- ✅ Handle_CacheHit_ShouldReturnCachedResult
- ✅ Handle_CacheMiss_ShouldQueryDatabaseAndCache
- ✅ Handle_NoPermission_ShouldReturnFalseAndCache
- ✅ Handle_RoleNotFound_ShouldThrowNotFoundException
- ✅ Handle_DifferentResourceActions_ShouldCacheIndependently (Theory)

---

### 2️⃣ Validators (3 validators = 20 tests estimados)

#### CreateRoleCommandValidator (9 tests)

- ✅ Validate_ValidRequest_ShouldPass
- ✅ Validate_InvalidName_ShouldFail (Theory: empty, null, too short)
- ✅ Validate_NameTooLong_ShouldFail (> 100 chars)
- ✅ Validate_ValidNameFormat_ShouldPass (Theory: Valid_Name, Valid-Name, ValidName123)
- ✅ Validate_InvalidNameFormat_ShouldFail (Theory: spaces, dots, @, #)
- ✅ Validate_DisplayNameTooLong_ShouldFail (> 200 chars)
- ✅ Validate_DescriptionTooLong_ShouldFail (> 500 chars)
- ✅ Validate_TooManyPermissions_ShouldFail (> 100)
- ✅ Validate_EmptyPermissionsList_ShouldPass

#### UpdateRoleCommandValidator (6 tests)

- ⏳ Validate_ValidUpdate_ShouldPass
- ⏳ Validate_OptionalFields_ShouldAllowNull
- ⏳ Validate_NameTooLong_ShouldFail
- ⏳ Validate_DisplayNameTooLong_ShouldFail
- ⏳ Validate_DescriptionTooLong_ShouldFail
- ⏳ Validate_TooManyPermissions_ShouldFail

#### CreatePermissionCommandValidator (8 tests)

- ✅ Validate_ValidPermissionName_ShouldPass (Theory: vehicles:read, users:create, etc)
- ✅ Validate_InvalidPermissionNameFormat_ShouldFail (Theory: no ':', extra ':', uppercase)
- ✅ Validate_AllowedModules_ShouldPass (Theory: auth, users, roles, vehicles, dealers, media, analytics, billing, notifications, admin, api, maintenance)
- ✅ Validate_DisallowedModules_ShouldFail (Theory: invalid, notallowed, test)
- ✅ Validate_EmptyName_ShouldFail
- ✅ Validate_EmptyModule_ShouldFail
- ✅ Validate_DisplayNameTooLong_ShouldFail
- ✅ Validate_DescriptionTooLong_ShouldFail

---

### 3️⃣ Services (1 servicio = 8 tests)

#### PermissionCacheService (8 tests)

- ✅ GetPermissionCheckAsync_CacheHit_ShouldReturnCachedValue
- ✅ GetPermissionCheckAsync_CacheMiss_ShouldReturnNull
- ✅ SetPermissionCheckAsync_ShouldStoreTrueValue
- ✅ SetPermissionCheckAsync_ShouldStoreFalseValue
- ✅ InvalidateRolePermissionsAsync_ShouldRemoveAllRoleKeys
- ✅ GetPermissionCheckAsync_DifferentResourceActions_ShouldUseDifferentKeys (Theory)
- ✅ CacheFailure_ShouldLogWarningAndNotThrow
- ✅ SetPermissionCheckAsync_WithTTL_ShouldRespectCheckDuration (5 min TTL)

---

### 4️⃣ Repositories (3 repos = 15 tests estimados)

#### RoleRepository (5 tests)

- ⏳ GetByIdAsync_ValidId_ShouldReturnRole
- ⏳ GetByNameAsync_ExistingName_ShouldReturnRole
- ⏳ CreateAsync_ValidRole_ShouldPersist
- ⏳ ExistsByNameAsync_ExistingName_ShouldReturnTrue
- ⏳ GetWithPermissionsAsync_ValidId_ShouldIncludePermissions

#### PermissionRepository (5 tests)

- ⏳ GetByIdAsync_ValidId_ShouldReturnPermission
- ⏳ GetByIdsAsync_ValidIds_ShouldReturnAll
- ⏳ CreateAsync_ValidPermission_ShouldPersist
- ⏳ GetAllAsync_WithModuleFilter_ShouldFilter
- ⏳ ExistsByNameAsync_ExistingName_ShouldReturnTrue

#### RolePermissionRepository (5 tests)

- ⏳ AssignAsync_NewPermission_ShouldPersist
- ⏳ RemoveAsync_ExistingPermission_ShouldRemove
- ⏳ ExistsAsync_ExistingAssignment_ShouldReturnTrue
- ⏳ HasPermissionAsync_WithPermission_ShouldReturnTrue
- ⏳ GetByRoleIdAsync_ValidId_ShouldReturnAssignedPermissions

---

### 5️⃣ Controllers (3 controllers = 15 tests de integración)

#### RolesController (5 endpoints)

- ⏳ POST /api/roles - CreateRole with ManageRoles policy
- ⏳ GET /api/roles - GetRoles public
- ⏳ GET /api/roles/{id} - GetRoleById public
- ⏳ PUT /api/roles/{id} - UpdateRole with ManageRoles policy
- ⏳ DELETE /api/roles/{id} - DeleteRole with ManageRoles policy

#### PermissionsController (3 endpoints)

- ⏳ POST /api/permissions - CreatePermission with ManagePermissions policy
- ⏳ GET /api/permissions - GetPermissions public

#### RolePermissionsController (3 endpoints)

- ⏳ POST /api/roles/{id}/permissions - AssignPermission with ManageRoles policy
- ⏳ DELETE /api/roles/{id}/permissions/{permissionId} - RemovePermission with ManageRoles policy
- ⏳ GET /api/roles/{roleId}/permissions/check - CheckPermission public (rate limit 500/min)

---

### 6️⃣ Authorization (5 tests de políticas)

#### Policy Tests

- ⏳ ManageRoles_SuperAdmin_ShouldAllow
- ⏳ ManageRoles_Admin_ShouldAllow
- ⏳ ManageRoles_RegularUser_ShouldDeny
- ⏳ ManagePermissions_SuperAdmin_ShouldAllow
- ⏳ ManagePermissions_Admin_ShouldDeny

---

### 7️⃣ E2E (5 flujos completos)

#### Complete Flows

- ⏳ Flow1_CreateRoleWithPermissions_AssignPermissions_CheckPermissionWithCache
- ⏳ Flow2_UpdateRole_CacheInvalidation_VerifyNewPermissions
- ⏳ Flow3_RemovePermission_CacheInvalidation_VerifyDenial
- ⏳ Flow4_CreatePermissionWithInvalidModule_Receive400BadRequest
- ⏳ Flow5_ModifySystemRole_Receive403Forbidden

---

## 📊 Resumen de Progress

| Categoría         | Total Tests | Completados | Pendientes | % Progreso |
| ----------------- | ----------- | ----------- | ---------- | ---------- |
| **Handlers**      | 50          | 27          | 23         | 54%        |
| **Validators**    | 23          | 17          | 6          | 74%        |
| **Services**      | 8           | 8           | 0          | ✅ 100%    |
| **Repositories**  | 15          | 0           | 15         | 0%         |
| **Controllers**   | 11          | 0           | 11         | 0%         |
| **Authorization** | 5           | 0           | 5          | 0%         |
| **E2E**           | 5           | 0           | 5          | 0%         |
| **TOTAL**         | **117**     | **52**      | **65**     | **44%**    |

---

## 🎯 Prioridad de Implementación

### ✅ Fase 1: Unit Tests Core (COMPLETADA - 52 tests)

- ✅ Handlers críticos (Create, Update, Delete, CheckPermission)
- ✅ Validators completos
- ✅ PermissionCacheService completo

### 🔄 Fase 2: Unit Tests Restantes (23 tests)

- ⏳ GetRolesQueryHandler (5 tests)
- ⏳ GetRoleByIdQueryHandler (5 tests)
- ⏳ GetPermissionsQueryHandler (5 tests)
- ⏳ RemovePermissionCommandHandler (5 tests)
- ⏳ UpdateRoleCommandValidator (6 tests)

### 🔄 Fase 3: Repository Tests (15 tests)

- ⏳ RoleRepository (5 tests)
- ⏳ PermissionRepository (5 tests)
- ⏳ RolePermissionRepository (5 tests)

### 🔄 Fase 4: Integration Tests (16 tests)

- ⏳ Controllers (11 tests)
- ⏳ Authorization policies (5 tests)

### 🔄 Fase 5: E2E Tests (5 tests)

- ⏳ Complete user flows

---

## 🛠️ Framework y Herramientas

### Dependencias de Testing

```xml
<ItemGroup>
    <!-- Test Framework -->
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xUnit" Version="2.5.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />

    <!-- Assertion Libraries -->
    <PackageReference Include="FluentAssertions" Version="6.12.0" />

    <!-- Mocking -->
    <PackageReference Include="Moq" Version="4.20.70" />

    <!-- In-Memory Database -->
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.11" />

    <!-- Integration Testing -->
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.11" />

    <!-- Code Coverage -->
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
    <PackageReference Include="coverlet.msbuild" Version="6.0.0" />
</ItemGroup>
```

### Estructura de Archivos

```
RoleService.Tests/
├── Handlers/
│   ├── Roles/
│   │   ├── CreateRoleCommandHandlerTests.cs
│   │   ├── UpdateRoleCommandHandlerTests.cs
│   │   ├── DeleteRoleCommandHandlerTests.cs
│   │   ├── GetRolesQueryHandlerTests.cs
│   │   └── GetRoleByIdQueryHandlerTests.cs
│   ├── Permissions/
│   │   ├── CreatePermissionCommandHandlerTests.cs
│   │   └── GetPermissionsQueryHandlerTests.cs
│   └── RolePermissions/
│       ├── AssignPermissionCommandHandlerTests.cs
│       ├── RemovePermissionCommandHandlerTests.cs
│       └── CheckPermissionQueryHandlerTests.cs
├── Validators/
│   ├── CreateRoleCommandValidatorTests.cs
│   ├── UpdateRoleCommandValidatorTests.cs
│   └── CreatePermissionCommandValidatorTests.cs
├── Services/
│   └── PermissionCacheServiceTests.cs
├── Repositories/
│   ├── RoleRepositoryTests.cs
│   ├── PermissionRepositoryTests.cs
│   └── RolePermissionRepositoryTests.cs
├── Controllers/
│   ├── RolesControllerTests.cs
│   ├── PermissionsControllerTests.cs
│   └── RolePermissionsControllerTests.cs
├── Authorization/
│   └── AuthorizationPolicyTests.cs
└── E2E/
    └── RBACFlowTests.cs
```

---

## 📈 Meta de Coverage

| Capa               | Target Coverage |
| ------------------ | --------------- |
| **Domain**         | 100%            |
| **Application**    | 90%             |
| **Infrastructure** | 70%             |
| **API**            | 80%             |
| **TOTAL**          | **85%**         |

---

## ⚙️ Comandos de Ejecución

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar tests con coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Ejecutar sólo unit tests
dotnet test --filter "FullyQualifiedName~.Handlers|FullyQualifiedName~.Validators|FullyQualifiedName~.Services"

# Ejecutar sólo integration tests
dotnet test --filter "FullyQualifiedName~.Controllers|FullyQualifiedName~.Authorization"

# Ejecutar E2E tests
dotnet test --filter "FullyQualifiedName~.E2E"

# Generar reporte HTML de coverage
reportgenerator -reports:coverage.cobertura.xml -targetdir:coveragereport -reporttypes:Html
```

---

## 🐛 Issues Identificados

### ❌ Problemas de Compilación Actuales

Los tests creados inicialmente tienen namespaces incorrectos. Estructura correcta:

```csharp
// ❌ INCORRECTO (usado en tests antiguos):
using RoleService.Application.Features.Roles.CreateRole;
using RoleService.Shared.Interfaces.IAuditServiceClient;

// ✅ CORRECTO (estructura real del proyecto):
using RoleService.Application.UseCases.Roles.CreateRole;
using RoleService.Domain.Interfaces; // IRoleRepository, IPermissionRepository
using RoleService.Application.Interfaces; // IAuditServiceClient, INotificationServiceClient
using RoleService.Shared.Exceptions; // ConflictException, NotFoundException
using RoleService.Application.DTOs.Roles; // CreateRoleResponse, RoleDto
```

### ✅ Solución

1. Eliminar tests con namespaces incorrectos
2. Recrear tests siguiendo estructura real del proyecto
3. Verificar compilación antes de proceder
4. Ejecutar tests y validar 100% passing

---

## 📝 Próximos Pasos

1. **Arreglar namespaces** en tests existentes (52 tests)
2. **Compilar** y validar 0 errores
3. **Ejecutar** tests y asegurar 100% passing
4. **Implementar** tests restantes Fase 2 (23 tests)
5. **Implementar** tests Fase 3 (15 tests)
6. **Implementar** tests Fase 4 (16 tests)
7. **Implementar** tests Fase 5 (5 tests)
8. **Generar** coverage report final
9. **Documentar** resultados en `TESTING_RESULTS.md`

---

**✅ Estrategia Definida**  
**🔧 Requiere Ajustes de Namespaces**  
**🎯 Meta: 85% Coverage Total**

_Última actualización: Enero 9, 2026_
