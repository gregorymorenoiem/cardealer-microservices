-- Seed RBAC Data for RoleService
-- Ejecutar: psql -h localhost -p 25432 -U postgres -d RoleService -f seed-rbac-data.sql

-- Limpiar datos existentes (en orden por FK constraints)
DELETE FROM "RolePermissions";
DELETE FROM "Permissions";
DELETE FROM "Roles";

-- ================================================
-- 1. CREAR ROLES
-- ================================================

-- Role: Super Admin (Prioridad más alta)
INSERT INTO "Roles" ("Id", "Name", "Description", "Priority", "IsActive", "IsSystemRole", "CreatedAt", "CreatedBy")
VALUES 
('11111111-1111-1111-1111-111111111111', 'SuperAdmin', 'Administrador con todos los permisos del sistema', 100, true, true, NOW(), 'system');

-- Role: Admin
INSERT INTO "Roles" ("Id", "Name", "Description", "Priority", "IsActive", "IsSystemRole", "CreatedAt", "CreatedBy")
VALUES 
('22222222-2222-2222-2222-222222222222', 'Admin', 'Administrador con permisos de gestión', 90, true, true, NOW(), 'system');

-- Role: Manager
INSERT INTO "Roles" ("Id", "Name", "Description", "Priority", "IsActive", "IsSystemRole", "CreatedAt", "CreatedBy")
VALUES 
('33333333-3333-3333-3333-333333333333', 'Manager', 'Gerente con permisos limitados de administración', 70, true, false, NOW(), 'system');

-- Role: User
INSERT INTO "Roles" ("Id", "Name", "Description", "Priority", "IsActive", "IsSystemRole", "CreatedAt", "CreatedBy")
VALUES 
('44444444-4444-4444-4444-444444444444', 'User', 'Usuario estándar con permisos básicos', 50, true, false, NOW(), 'system');

-- Role: ReadOnly
INSERT INTO "Roles" ("Id", "Name", "Description", "Priority", "IsActive", "IsSystemRole", "CreatedAt", "CreatedBy")
VALUES 
('55555555-5555-5555-5555-555555555555', 'ReadOnly', 'Usuario con permisos solo de lectura', 30, true, false, NOW(), 'system');

-- ================================================
-- 2. CREAR PERMISOS
-- ================================================

-- Permisos de Users (PermissionAction: 0=Create, 1=Read, 2=Update, 3=Delete, 4=Execute, 5=All)
INSERT INTO "Permissions" ("Id", "Name", "Description", "Resource", "Action", "Module", "IsActive", "IsSystemPermission", "CreatedAt")
VALUES 
('a0000001-0000-0000-0000-000000000001', 'users.create', 'Crear nuevos usuarios', 'users', 0, 'UserService', true, true, NOW()),
('a0000001-0000-0000-0000-000000000002', 'users.read', 'Ver información de usuarios', 'users', 1, 'UserService', true, true, NOW()),
('a0000001-0000-0000-0000-000000000003', 'users.update', 'Actualizar usuarios existentes', 'users', 2, 'UserService', true, true, NOW()),
('a0000001-0000-0000-0000-000000000004', 'users.delete', 'Eliminar usuarios', 'users', 3, 'UserService', true, true, NOW()),
('a0000001-0000-0000-0000-000000000005', 'users.all', 'Todos los permisos sobre usuarios', 'users', 5, 'UserService', true, true, NOW());

-- Permisos de Roles
INSERT INTO "Permissions" ("Id", "Name", "Description", "Resource", "Action", "Module", "IsActive", "IsSystemPermission", "CreatedAt")
VALUES 
('a0000002-0000-0000-0000-000000000001', 'roles.create', 'Crear nuevos roles', 'roles', 0, 'RoleService', true, true, NOW()),
('a0000002-0000-0000-0000-000000000002', 'roles.read', 'Ver información de roles', 'roles', 1, 'RoleService', true, true, NOW()),
('a0000002-0000-0000-0000-000000000003', 'roles.update', 'Actualizar roles existentes', 'roles', 2, 'RoleService', true, true, NOW()),
('a0000002-0000-0000-0000-000000000004', 'roles.delete', 'Eliminar roles', 'roles', 3, 'RoleService', true, true, NOW()),
('a0000002-0000-0000-0000-000000000005', 'roles.all', 'Todos los permisos sobre roles', 'roles', 5, 'RoleService', true, true, NOW());

-- Permisos de Permissions
INSERT INTO "Permissions" ("Id", "Name", "Description", "Resource", "Action", "Module", "IsActive", "IsSystemPermission", "CreatedAt")
VALUES 
('a0000003-0000-0000-0000-000000000001', 'permissions.create', 'Crear nuevos permisos', 'permissions', 0, 'RoleService', true, true, NOW()),
('a0000003-0000-0000-0000-000000000002', 'permissions.read', 'Ver información de permisos', 'permissions', 1, 'RoleService', true, true, NOW()),
('a0000003-0000-0000-0000-000000000005', 'permissions.all', 'Todos los permisos sobre permisos', 'permissions', 5, 'RoleService', true, true, NOW());

-- Permisos de Vehicles
INSERT INTO "Permissions" ("Id", "Name", "Description", "Resource", "Action", "Module", "IsActive", "IsSystemPermission", "CreatedAt")
VALUES 
('a0000004-0000-0000-0000-000000000001', 'vehicles.create', 'Crear vehículos', 'vehicles', 0, 'VehicleService', true, false, NOW()),
('a0000004-0000-0000-0000-000000000002', 'vehicles.read', 'Ver vehículos', 'vehicles', 1, 'VehicleService', true, false, NOW()),
('a0000004-0000-0000-0000-000000000003', 'vehicles.update', 'Actualizar vehículos', 'vehicles', 2, 'VehicleService', true, false, NOW()),
('a0000004-0000-0000-0000-000000000004', 'vehicles.delete', 'Eliminar vehículos', 'vehicles', 3, 'VehicleService', true, false, NOW());

-- Permisos de Media
INSERT INTO "Permissions" ("Id", "Name", "Description", "Resource", "Action", "Module", "IsActive", "IsSystemPermission", "CreatedAt")
VALUES 
('a0000005-0000-0000-0000-000000000001', 'media.create', 'Subir archivos multimedia', 'media', 0, 'MediaService', true, false, NOW()),
('a0000005-0000-0000-0000-000000000002', 'media.read', 'Ver archivos multimedia', 'media', 1, 'MediaService', true, false, NOW()),
('a0000005-0000-0000-0000-000000000004', 'media.delete', 'Eliminar archivos multimedia', 'media', 3, 'MediaService', true, false, NOW());

-- ================================================
-- 3. ASIGNAR PERMISOS A ROLES
-- ================================================

-- SuperAdmin: TODOS los permisos (usando permisos .all)
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId", "AssignedAt", "AssignedBy")
VALUES 
('b1111111-0000-0000-0000-000000000001', '11111111-1111-1111-1111-111111111111', 'a0000001-0000-0000-0000-000000000005', NOW(), 'system'),
('b1111111-0000-0000-0000-000000000002', '11111111-1111-1111-1111-111111111111', 'a0000002-0000-0000-0000-000000000005', NOW(), 'system'),
('b1111111-0000-0000-0000-000000000003', '11111111-1111-1111-1111-111111111111', 'a0000003-0000-0000-0000-000000000005', NOW(), 'system'),
('b1111111-0000-0000-0000-000000000004', '11111111-1111-1111-1111-111111111111', 'a0000004-0000-0000-0000-000000000001', NOW(), 'system'),
('b1111111-0000-0000-0000-000000000005', '11111111-1111-1111-1111-111111111111', 'a0000004-0000-0000-0000-000000000002', NOW(), 'system'),
('b1111111-0000-0000-0000-000000000006', '11111111-1111-1111-1111-111111111111', 'a0000004-0000-0000-0000-000000000003', NOW(), 'system'),
('b1111111-0000-0000-0000-000000000007', '11111111-1111-1111-1111-111111111111', 'a0000004-0000-0000-0000-000000000004', NOW(), 'system'),
('b1111111-0000-0000-0000-000000000008', '11111111-1111-1111-1111-111111111111', 'a0000005-0000-0000-0000-000000000001', NOW(), 'system'),
('b1111111-0000-0000-0000-000000000009', '11111111-1111-1111-1111-111111111111', 'a0000005-0000-0000-0000-000000000002', NOW(), 'system'),
('b1111111-0000-0000-0000-000000000010', '11111111-1111-1111-1111-111111111111', 'a0000005-0000-0000-0000-000000000004', NOW(), 'system');

-- Admin: Gestión de usuarios, roles y permisos + lectura de vehículos
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId", "AssignedAt", "AssignedBy")
VALUES 
('b2222222-0000-0000-0000-000000000001', '22222222-2222-2222-2222-222222222222', 'a0000001-0000-0000-0000-000000000001', NOW(), 'system'),
('b2222222-0000-0000-0000-000000000002', '22222222-2222-2222-2222-222222222222', 'a0000001-0000-0000-0000-000000000002', NOW(), 'system'),
('b2222222-0000-0000-0000-000000000003', '22222222-2222-2222-2222-222222222222', 'a0000001-0000-0000-0000-000000000003', NOW(), 'system'),
('b2222222-0000-0000-0000-000000000004', '22222222-2222-2222-2222-222222222222', 'a0000001-0000-0000-0000-000000000004', NOW(), 'system'),
('b2222222-0000-0000-0000-000000000005', '22222222-2222-2222-2222-222222222222', 'a0000002-0000-0000-0000-000000000002', NOW(), 'system'),
('b2222222-0000-0000-0000-000000000006', '22222222-2222-2222-2222-222222222222', 'a0000002-0000-0000-0000-000000000003', NOW(), 'system'),
('b2222222-0000-0000-0000-000000000007', '22222222-2222-2222-2222-222222222222', 'a0000003-0000-0000-0000-000000000002', NOW(), 'system'),
('b2222222-0000-0000-0000-000000000008', '22222222-2222-2222-2222-222222222222', 'a0000004-0000-0000-0000-000000000002', NOW(), 'system'),
('b2222222-0000-0000-0000-000000000009', '22222222-2222-2222-2222-222222222222', 'a0000005-0000-0000-0000-000000000002', NOW(), 'system');

-- Manager: Gestión de vehículos y media, lectura de usuarios
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId", "AssignedAt", "AssignedBy")
VALUES 
('b3333333-0000-0000-0000-000000000001', '33333333-3333-3333-3333-333333333333', 'a0000001-0000-0000-0000-000000000002', NOW(), 'system'),
('b3333333-0000-0000-0000-000000000002', '33333333-3333-3333-3333-333333333333', 'a0000004-0000-0000-0000-000000000001', NOW(), 'system'),
('b3333333-0000-0000-0000-000000000003', '33333333-3333-3333-3333-333333333333', 'a0000004-0000-0000-0000-000000000002', NOW(), 'system'),
('b3333333-0000-0000-0000-000000000004', '33333333-3333-3333-3333-333333333333', 'a0000004-0000-0000-0000-000000000003', NOW(), 'system'),
('b3333333-0000-0000-0000-000000000005', '33333333-3333-3333-3333-333333333333', 'a0000004-0000-0000-0000-000000000004', NOW(), 'system'),
('b3333333-0000-0000-0000-000000000006', '33333333-3333-3333-3333-333333333333', 'a0000005-0000-0000-0000-000000000001', NOW(), 'system'),
('b3333333-0000-0000-0000-000000000007', '33333333-3333-3333-3333-333333333333', 'a0000005-0000-0000-0000-000000000002', NOW(), 'system'),
('b3333333-0000-0000-0000-000000000008', '33333333-3333-3333-3333-333333333333', 'a0000005-0000-0000-0000-000000000004', NOW(), 'system');

-- User: Lectura de vehículos y media
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId", "AssignedAt", "AssignedBy")
VALUES 
('b4444444-0000-0000-0000-000000000001', '44444444-4444-4444-4444-444444444444', 'a0000004-0000-0000-0000-000000000002', NOW(), 'system'),
('b4444444-0000-0000-0000-000000000002', '44444444-4444-4444-4444-444444444444', 'a0000005-0000-0000-0000-000000000002', NOW(), 'system');

-- ReadOnly: Solo lectura de todo
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId", "AssignedAt", "AssignedBy")
VALUES 
('b5555555-0000-0000-0000-000000000001', '55555555-5555-5555-5555-555555555555', 'a0000001-0000-0000-0000-000000000002', NOW(), 'system'),
('b5555555-0000-0000-0000-000000000002', '55555555-5555-5555-5555-555555555555', 'a0000002-0000-0000-0000-000000000002', NOW(), 'system'),
('b5555555-0000-0000-0000-000000000003', '55555555-5555-5555-5555-555555555555', 'a0000003-0000-0000-0000-000000000002', NOW(), 'system'),
('b5555555-0000-0000-0000-000000000004', '55555555-5555-5555-5555-555555555555', 'a0000004-0000-0000-0000-000000000002', NOW(), 'system'),
('b5555555-0000-0000-0000-000000000005', '55555555-5555-5555-5555-555555555555', 'a0000005-0000-0000-0000-000000000002', NOW(), 'system');

-- ================================================
-- 4. VERIFICAR DATOS
-- ================================================

-- Ver resumen de roles
SELECT 
    r."Name" as "Role",
    r."Priority",
    r."IsSystemRole",
    COUNT(rp."PermissionId") as "PermissionCount"
FROM "Roles" r
LEFT JOIN "RolePermissions" rp ON r."Id" = rp."RoleId"
GROUP BY r."Id", r."Name", r."Priority", r."IsSystemRole"
ORDER BY r."Priority" DESC;

-- Ver permisos por módulo
SELECT 
    "Module",
    COUNT(*) as "PermissionCount"
FROM "Permissions"
GROUP BY "Module"
ORDER BY "Module";
