-- UserService Seed Data
-- Usuarios de prueba con diferentes roles del RoleService
-- Contraseña para todos: "Test123!" (hasheado con BCrypt)

-- IMPORTANTE: Este script asume que RoleService ya tiene los roles seeded
-- SuperAdmin: 11111111-1111-1111-1111-111111111111
-- Admin:      22222222-2222-2222-2222-222222222222
-- Manager:    33333333-3333-3333-3333-333333333333
-- Viewer:     44444444-4444-4444-4444-444444444444

-- ============================================
-- 1. Insertar usuarios de prueba
-- ============================================

-- Administrador principal
INSERT INTO "Users" ("Id", "Email", "PasswordHash", "FirstName", "LastName", "PhoneNumber", "IsActive", "EmailConfirmed", "CreatedAt")
VALUES (
    'a0000000-0000-0000-0000-000000000001',
    'admin@cardealer.com',
    '$2a$11$XzN0ZV9xPjQx.LqT8HZgH.vJ9QY7J7kZ.NvFqGHxKlJQ8XQ9Q7zGW', -- Test123!
    'Admin',
    'Principal',
    '+1234567890',
    TRUE,
    TRUE,
    NOW()
);

-- Manager de ventas
INSERT INTO "Users" ("Id", "Email", "PasswordHash", "FirstName", "LastName", "PhoneNumber", "IsActive", "EmailConfirmed", "CreatedAt")
VALUES (
    'a0000000-0000-0000-0000-000000000002',
    'manager@cardealer.com',
    '$2a$11$XzN0ZV9xPjQx.LqT8HZgH.vJ9QY7J7kZ.NvFqGHxKlJQ8XQ9Q7zGW', -- Test123!
    'Sales',
    'Manager',
    '+1234567891',
    TRUE,
    TRUE,
    NOW()
);

-- Usuario viewer
INSERT INTO "Users" ("Id", "Email", "PasswordHash", "FirstName", "LastName", "PhoneNumber", "IsActive", "EmailConfirmed", "CreatedAt")
VALUES (
    'a0000000-0000-0000-0000-000000000003',
    'viewer@cardealer.com',
    '$2a$11$XzN0ZV9xPjQx.LqT8HZgH.vJ9QY7J7kZ.NvFqGHxKlJQ8XQ9Q7zGW', -- Test123!
    'Read',
    'Only',
    '+1234567892',
    TRUE,
    TRUE,
    NOW()
);

-- Usuario inactivo (para testing)
INSERT INTO "Users" ("Id", "Email", "PasswordHash", "FirstName", "LastName", "PhoneNumber", "IsActive", "EmailConfirmed", "CreatedAt", "UpdatedAt")
VALUES (
    'a0000000-0000-0000-0000-000000000004',
    'inactive@cardealer.com',
    '$2a$11$XzN0ZV9xPjQx.LqT8HZgH.vJ9QY7J7kZ.NvFqGHxKlJQ8XQ9Q7zGW', -- Test123!
    'Inactive',
    'User',
    '+1234567893',
    FALSE,
    TRUE,
    NOW(),
    NOW()
);

-- Usuario sin confirmar email
INSERT INTO "Users" ("Id", "Email", "PasswordHash", "FirstName", "LastName", "PhoneNumber", "IsActive", "EmailConfirmed", "CreatedAt")
VALUES (
    'a0000000-0000-0000-0000-000000000005',
    'pending@cardealer.com',
    '$2a$11$XzN0ZV9xPjQx.LqT8HZgH.vJ9QY7J7kZ.NvFqGHxKlJQ8XQ9Q7zGW', -- Test123!
    'Pending',
    'Confirmation',
    '+1234567894',
    TRUE,
    FALSE,
    NOW()
);

-- ============================================
-- 2. Asignar roles a usuarios
-- ============================================

-- Admin Principal -> SuperAdmin role
INSERT INTO "UserRoles" ("Id", "UserId", "RoleId", "AssignedAt", "AssignedBy", "IsActive")
VALUES (
    'b0000000-0000-0000-0000-000000000001',
    'a0000000-0000-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111111', -- SuperAdmin
    NOW(),
    'seed-script',
    TRUE
);

-- Admin Principal -> Admin role (multiple roles)
INSERT INTO "UserRoles" ("Id", "UserId", "RoleId", "AssignedAt", "AssignedBy", "IsActive")
VALUES (
    'b0000000-0000-0000-0000-000000000002',
    'a0000000-0000-0000-0000-000000000001',
    '22222222-2222-2222-2222-222222222222', -- Admin
    NOW(),
    'seed-script',
    TRUE
);

-- Manager -> Manager role
INSERT INTO "UserRoles" ("Id", "UserId", "RoleId", "AssignedAt", "AssignedBy", "IsActive")
VALUES (
    'b0000000-0000-0000-0000-000000000003',
    'a0000000-0000-0000-0000-000000000002',
    '33333333-3333-3333-3333-333333333333', -- Manager
    NOW(),
    'seed-script',
    TRUE
);

-- Viewer -> Viewer role
INSERT INTO "UserRoles" ("Id", "UserId", "RoleId", "AssignedAt", "AssignedBy", "IsActive")
VALUES (
    'b0000000-0000-0000-0000-000000000004',
    'a0000000-0000-0000-0000-000000000003',
    '44444444-4444-4444-4444-444444444444', -- Viewer
    NOW(),
    'seed-script',
    TRUE
);

-- Usuario inactivo -> Admin role (pero usuario está inactivo)
INSERT INTO "UserRoles" ("Id", "UserId", "RoleId", "AssignedAt", "AssignedBy", "IsActive")
VALUES (
    'b0000000-0000-0000-0000-000000000005',
    'a0000000-0000-0000-0000-000000000004',
    '22222222-2222-2222-2222-222222222222', -- Admin
    NOW(),
    'seed-script',
    TRUE
);

-- Rol revocado (para testing de revocación)
INSERT INTO "UserRoles" ("Id", "UserId", "RoleId", "AssignedAt", "AssignedBy", "RevokedAt", "RevokedBy", "IsActive")
VALUES (
    'b0000000-0000-0000-0000-000000000006',
    'a0000000-0000-0000-0000-000000000002',
    '44444444-4444-4444-4444-444444444444', -- Viewer (revoked from manager)
    NOW() - INTERVAL '7 days',
    'seed-script',
    NOW() - INTERVAL '2 days',
    'admin@cardealer.com',
    FALSE
);

-- ============================================
-- Verificación de los datos insertados
-- ============================================

-- SELECT * FROM "Users";
-- SELECT * FROM "UserRoles";
-- SELECT u."Email", u."FirstName", u."LastName", ur."RoleId", ur."IsActive" 
-- FROM "Users" u
-- JOIN "UserRoles" ur ON u."Id" = ur."UserId"
-- ORDER BY u."Email";
