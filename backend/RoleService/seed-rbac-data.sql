-- Seed RBAC Data for RoleService (Staff/Equipo interno de OKLA)
-- NOTA: Estos roles son SOLO para el equipo interno de OKLA.
--   - Compradores/vendedores individuales tienen permisos automáticos (AccountType: Individual)
--   - Dealers gestionan roles de su propio equipo desde su portal (AccountType: Dealer)
-- Ejecutar: psql -h localhost -p 25432 -U postgres -d RoleService -f seed-rbac-data.sql

-- Limpiar datos existentes (en orden por FK constraints)
DELETE FROM "RolePermissions";
DELETE FROM "Permissions";
DELETE FROM "Roles";

-- ================================================
-- 1. CREAR ROLES
-- ================================================

-- Role: Super Admin (Prioridad más alta) - Staff con acceso completo
INSERT INTO "Roles" ("Id", "Name", "DisplayName", "Description", "Priority", "IsActive", "IsSystemRole", "CreatedAt", "CreatedBy")
VALUES 
('11111111-1111-1111-1111-111111111111', 'SuperAdmin', 'Super Admin', 'Staff con acceso completo a todas las funcionalidades de la plataforma', 100, true, true, NOW(), 'system');

-- Role: Admin - Staff de gestión general
INSERT INTO "Roles" ("Id", "Name", "DisplayName", "Description", "Priority", "IsActive", "IsSystemRole", "CreatedAt", "CreatedBy")
VALUES 
('22222222-2222-2222-2222-222222222222', 'Admin', 'Admin', 'Staff de gestión de usuarios, vehículos y dealers', 90, true, true, NOW(), 'system');

-- Role: Moderador - Staff de revisión de contenido
INSERT INTO "Roles" ("Id", "Name", "DisplayName", "Description", "Priority", "IsActive", "IsSystemRole", "CreatedAt", "CreatedBy")
VALUES 
('33333333-3333-3333-3333-333333333333', 'Moderador', 'Moderador', 'Staff de revisión y aprobación de contenido publicado', 70, true, false, NOW(), 'system');

-- Role: Soporte - Staff de atención al cliente
INSERT INTO "Roles" ("Id", "Name", "DisplayName", "Description", "Priority", "IsActive", "IsSystemRole", "CreatedAt", "CreatedBy")
VALUES 
('44444444-4444-4444-4444-444444444444', 'Soporte', 'Soporte', 'Staff de atención al cliente y soporte técnico', 50, true, false, NOW(), 'system');

-- Role: Analista - Staff de reportes y datos
INSERT INTO "Roles" ("Id", "Name", "DisplayName", "Description", "Priority", "IsActive", "IsSystemRole", "CreatedAt", "CreatedBy")
VALUES 
('55555555-5555-5555-5555-555555555555', 'Analista', 'Analista', 'Staff con acceso a reportes y analytics de la plataforma', 30, true, false, NOW(), 'system');

-- ================================================
-- 2. CREAR PERMISOS
-- ================================================

-- Permisos de Users (PermissionAction enum: Create=1, Read=2, Update=3, Delete=4, Ban=10, Suspend=12)
INSERT INTO "Permissions" ("Id", "Name", "DisplayName", "Description", "Resource", "Action", "Module", "IsActive", "IsSystemPermission", "CreatedAt")
VALUES 
('a0000001-0000-0000-0000-000000000001', 'users:create', 'Crear usuarios', 'Crear nuevos usuarios en el sistema', 'users', 1, 'users', true, true, NOW()),
('a0000001-0000-0000-0000-000000000002', 'users:read', 'Ver usuarios', 'Ver lista y detalles de usuarios', 'users', 2, 'users', true, true, NOW()),
('a0000001-0000-0000-0000-000000000003', 'users:update', 'Editar usuarios', 'Modificar información de usuarios', 'users', 3, 'users', true, true, NOW()),
('a0000001-0000-0000-0000-000000000004', 'users:delete', 'Eliminar usuarios', 'Eliminar cuentas de usuarios', 'users', 4, 'users', true, true, NOW()),
('a0000001-0000-0000-0000-000000000005', 'users:ban', 'Suspender usuarios', 'Suspender o banear usuarios', 'users', 10, 'users', true, true, NOW());

-- Permisos de Vehicles
INSERT INTO "Permissions" ("Id", "Name", "DisplayName", "Description", "Resource", "Action", "Module", "IsActive", "IsSystemPermission", "CreatedAt")
VALUES 
('a0000002-0000-0000-0000-000000000001', 'vehicles:create', 'Crear vehículos', 'Crear publicaciones de vehículos', 'vehicles', 1, 'vehicles', true, true, NOW()),
('a0000002-0000-0000-0000-000000000002', 'vehicles:read', 'Ver vehículos', 'Ver lista y detalles de vehículos', 'vehicles', 2, 'vehicles', true, true, NOW()),
('a0000002-0000-0000-0000-000000000003', 'vehicles:update', 'Editar vehículos', 'Modificar publicaciones de vehículos', 'vehicles', 3, 'vehicles', true, true, NOW()),
('a0000002-0000-0000-0000-000000000004', 'vehicles:delete', 'Eliminar vehículos', 'Eliminar publicaciones de vehículos', 'vehicles', 4, 'vehicles', true, true, NOW()),
('a0000002-0000-0000-0000-000000000005', 'vehicles:approve', 'Aprobar vehículos', 'Aprobar o rechazar publicaciones de vehículos', 'vehicles', 16, 'vehicles', true, true, NOW());

-- Permisos de Dealers
INSERT INTO "Permissions" ("Id", "Name", "DisplayName", "Description", "Resource", "Action", "Module", "IsActive", "IsSystemPermission", "CreatedAt")
VALUES 
('a0000003-0000-0000-0000-000000000001', 'dealers:read', 'Ver dealers', 'Ver lista y detalles de dealers', 'dealers', 2, 'dealers', true, true, NOW()),
('a0000003-0000-0000-0000-000000000002', 'dealers:update', 'Editar dealers', 'Modificar información de dealers', 'dealers', 3, 'dealers', true, true, NOW()),
('a0000003-0000-0000-0000-000000000003', 'dealers:verify', 'Verificar dealers', 'Aprobar o rechazar nuevos dealers', 'dealers', 11, 'dealers', true, true, NOW());

-- Permisos de Reportes y Analytics
INSERT INTO "Permissions" ("Id", "Name", "DisplayName", "Description", "Resource", "Action", "Module", "IsActive", "IsSystemPermission", "CreatedAt")
VALUES 
('a0000004-0000-0000-0000-000000000001', 'reports:read', 'Ver reportes', 'Ver reportes de usuarios y contenido', 'reports', 2, 'analytics', true, true, NOW()),
('a0000004-0000-0000-0000-000000000002', 'analytics:read', 'Ver analytics', 'Acceso al dashboard de analytics', 'analytics', 2, 'analytics', true, true, NOW()),
('a0000004-0000-0000-0000-000000000003', 'analytics:export', 'Exportar datos', 'Exportar reportes y datos del sistema', 'analytics', 9, 'analytics', true, true, NOW());

-- Permisos de Configuración
INSERT INTO "Permissions" ("Id", "Name", "DisplayName", "Description", "Resource", "Action", "Module", "IsActive", "IsSystemPermission", "CreatedAt")
VALUES 
('a0000005-0000-0000-0000-000000000001', 'settings:read', 'Ver configuración', 'Ver ajustes del sistema', 'settings', 2, 'admin', true, true, NOW()),
('a0000005-0000-0000-0000-000000000002', 'settings:update', 'Editar configuración', 'Modificar ajustes del sistema', 'settings', 3, 'admin', true, true, NOW());

-- Permisos de Roles y Permisos (RBAC management)
INSERT INTO "Permissions" ("Id", "Name", "DisplayName", "Description", "Resource", "Action", "Module", "IsActive", "IsSystemPermission", "CreatedAt")
VALUES 
('a0000006-0000-0000-0000-000000000001', 'roles:read', 'Ver roles', 'Ver información de roles del sistema', 'roles', 2, 'admin', true, true, NOW()),
('a0000006-0000-0000-0000-000000000002', 'roles:manage', 'Gestionar roles', 'Crear, editar y eliminar roles', 'roles', 21, 'admin', true, true, NOW()),
('a0000006-0000-0000-0000-000000000003', 'permissions:read', 'Ver permisos', 'Ver permisos del sistema', 'permissions', 2, 'admin', true, true, NOW()),
('a0000006-0000-0000-0000-000000000004', 'permissions:manage', 'Gestionar permisos', 'Crear y modificar permisos', 'permissions', 22, 'admin', true, true, NOW());

-- Permisos de Media
INSERT INTO "Permissions" ("Id", "Name", "DisplayName", "Description", "Resource", "Action", "Module", "IsActive", "IsSystemPermission", "CreatedAt")
VALUES 
('a0000007-0000-0000-0000-000000000001', 'media:create', 'Subir archivos', 'Subir archivos multimedia', 'media', 1, 'media', true, false, NOW()),
('a0000007-0000-0000-0000-000000000002', 'media:read', 'Ver archivos', 'Ver archivos multimedia', 'media', 2, 'media', true, false, NOW()),
('a0000007-0000-0000-0000-000000000003', 'media:delete', 'Eliminar archivos', 'Eliminar archivos multimedia', 'media', 4, 'media', true, false, NOW());

-- Permisos de Soporte
INSERT INTO "Permissions" ("Id", "Name", "DisplayName", "Description", "Resource", "Action", "Module", "IsActive", "IsSystemPermission", "CreatedAt")
VALUES 
('a0000008-0000-0000-0000-000000000001', 'tickets:read', 'Ver tickets', 'Ver tickets de soporte', 'tickets', 2, 'support', true, true, NOW()),
('a0000008-0000-0000-0000-000000000002', 'tickets:update', 'Gestionar tickets', 'Responder y gestionar tickets de soporte', 'tickets', 3, 'support', true, true, NOW()),
('a0000008-0000-0000-0000-000000000003', 'messages:read', 'Ver mensajes', 'Ver mensajes de usuarios', 'messages', 2, 'support', true, true, NOW());

-- ================================================
-- 3. ASIGNAR PERMISOS A ROLES
-- ================================================

-- SuperAdmin: TODOS los permisos
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId", "AssignedAt", "AssignedBy")
SELECT gen_random_uuid(), '11111111-1111-1111-1111-111111111111', "Id", NOW(), 'system'
FROM "Permissions";

-- Admin: Todos los permisos excepto gestión de permisos del sistema
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId", "AssignedAt", "AssignedBy")
SELECT gen_random_uuid(), '22222222-2222-2222-2222-222222222222', "Id", NOW(), 'system'
FROM "Permissions"
WHERE "Name" NOT IN ('permissions:manage');

-- Moderador: Vehículos (leer, editar, aprobar), reportes, lectura de usuarios y dealers, media
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId", "AssignedAt", "AssignedBy")
SELECT gen_random_uuid(), '33333333-3333-3333-3333-333333333333', "Id", NOW(), 'system'
FROM "Permissions"
WHERE "Name" IN (
  'vehicles:read', 'vehicles:update', 'vehicles:approve',
  'users:read', 'dealers:read',
  'reports:read',
  'media:read'
);

-- Soporte: Lectura de usuarios, tickets, mensajes, vehículos
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId", "AssignedAt", "AssignedBy")
SELECT gen_random_uuid(), '44444444-4444-4444-4444-444444444444', "Id", NOW(), 'system'
FROM "Permissions"
WHERE "Name" IN (
  'users:read',
  'tickets:read', 'tickets:update', 'messages:read',
  'vehicles:read'
);

-- Analista: Analytics y reportes, lectura de usuarios y vehículos
INSERT INTO "RolePermissions" ("Id", "RoleId", "PermissionId", "AssignedAt", "AssignedBy")
SELECT gen_random_uuid(), '55555555-5555-5555-5555-555555555555', "Id", NOW(), 'system'
FROM "Permissions"
WHERE "Name" IN (
  'reports:read', 'analytics:read', 'analytics:export',
  'users:read', 'vehicles:read'
);

-- ================================================
-- 4. VERIFICAR DATOS
-- ================================================

SELECT 
    r."Name" as "Role",
    r."DisplayName",
    r."Priority",
    r."IsSystemRole",
    COUNT(rp."PermissionId") as "PermissionCount"
FROM "Roles" r
LEFT JOIN "RolePermissions" rp ON r."Id" = rp."RoleId"
GROUP BY r."Id", r."Name", r."DisplayName", r."Priority", r."IsSystemRole"
ORDER BY r."Priority" DESC;

SELECT 
    "Module",
    COUNT(*) as "PermissionCount"
FROM "Permissions"
GROUP BY "Module"
ORDER BY "Module";
