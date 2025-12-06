-- Migration: Add Multi-Level Role System
-- Date: 2025-12-05
-- Description: Agrega soporte para sistema de roles multi-nivel (Dealer Employees y Platform Employees)

-- 1. Agregar columnas al Usuario
ALTER TABLE Users
ADD AccountType INT NOT NULL DEFAULT 1, -- Individual por defecto
    PlatformRole INT NULL,
    PlatformPermissions NVARCHAR(MAX) NULL,
    DealerId UNIQUEIDENTIFIER NULL,
    DealerRole INT NULL,
    DealerPermissions NVARCHAR(MAX) NULL,
    EmployerUserId UNIQUEIDENTIFIER NULL,
    CreatedBy UNIQUEIDENTIFIER NULL;

-- 2. Crear tabla de Empleados de Dealers
CREATE TABLE DealerEmployees (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    DealerId UNIQUEIDENTIFIER NOT NULL,
    DealerRole INT NOT NULL,
    Permissions NVARCHAR(MAX) NOT NULL DEFAULT '[]',
    InvitedBy UNIQUEIDENTIFIER NOT NULL,
    Status INT NOT NULL DEFAULT 0, -- 0=Pending, 1=Active, 2=Suspended
    InvitationDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ActivationDate DATETIME2 NULL,
    Notes NVARCHAR(500) NULL,
    CONSTRAINT FK_DealerEmployees_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_DealerEmployees_InvitedBy FOREIGN KEY (InvitedBy) REFERENCES Users(Id)
);

-- 3. Crear tabla de Empleados de Plataforma
CREATE TABLE PlatformEmployees (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    PlatformRole INT NOT NULL,
    Permissions NVARCHAR(MAX) NOT NULL DEFAULT '[]',
    AssignedBy UNIQUEIDENTIFIER NOT NULL,
    Status INT NOT NULL DEFAULT 1, -- 1=Active, 2=Suspended
    HireDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Department NVARCHAR(100) NULL,
    Notes NVARCHAR(500) NULL,
    CONSTRAINT FK_PlatformEmployees_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_PlatformEmployees_AssignedBy FOREIGN KEY (AssignedBy) REFERENCES Users(Id)
);

-- 4. Crear tabla de Invitaciones de Dealer
CREATE TABLE DealerEmployeeInvitations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    DealerId UNIQUEIDENTIFIER NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    DealerRole INT NOT NULL,
    Permissions NVARCHAR(MAX) NOT NULL DEFAULT '[]',
    InvitedBy UNIQUEIDENTIFIER NOT NULL,
    Status INT NOT NULL DEFAULT 0, -- 0=Pending, 1=Accepted, 2=Expired, 3=Revoked
    InvitationDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpirationDate DATETIME2 NOT NULL,
    AcceptedDate DATETIME2 NULL,
    Token NVARCHAR(256) NOT NULL,
    CONSTRAINT FK_DealerInvitations_InvitedBy FOREIGN KEY (InvitedBy) REFERENCES Users(Id)
);

-- 5. Crear tabla de Invitaciones de Plataforma
CREATE TABLE PlatformEmployeeInvitations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Email NVARCHAR(256) NOT NULL,
    PlatformRole INT NOT NULL,
    Permissions NVARCHAR(MAX) NOT NULL DEFAULT '[]',
    InvitedBy UNIQUEIDENTIFIER NOT NULL,
    Status INT NOT NULL DEFAULT 0, -- 0=Pending, 1=Accepted, 2=Expired, 3=Revoked
    InvitationDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpirationDate DATETIME2 NOT NULL,
    AcceptedDate DATETIME2 NULL,
    Token NVARCHAR(256) NOT NULL,
    CONSTRAINT FK_PlatformInvitations_InvitedBy FOREIGN KEY (InvitedBy) REFERENCES Users(Id)
);

-- 6. Crear índices
CREATE INDEX IX_DealerEmployees_UserId ON DealerEmployees(UserId);
CREATE INDEX IX_DealerEmployees_DealerId ON DealerEmployees(DealerId);
CREATE INDEX IX_PlatformEmployees_UserId ON PlatformEmployees(UserId);
CREATE INDEX IX_DealerInvitations_Email ON DealerEmployeeInvitations(Email);
CREATE INDEX IX_DealerInvitations_Token ON DealerEmployeeInvitations(Token);
CREATE INDEX IX_PlatformInvitations_Email ON PlatformEmployeeInvitations(Email);
CREATE INDEX IX_PlatformInvitations_Token ON PlatformEmployeeInvitations(Token);
CREATE INDEX IX_Users_AccountType ON Users(AccountType);
CREATE INDEX IX_Users_DealerId ON Users(DealerId);

-- 7. Migrar datos existentes
-- Usuarios con role 'Admin' -> AccountType = Admin (4)
UPDATE Users
SET AccountType = 4, -- Admin
    PlatformRole = 0  -- SuperAdmin
WHERE Id IN (
    SELECT DISTINCT ur.UserId
    FROM UserRoles ur
    INNER JOIN Roles r ON ur.RoleId = r.Id
    WHERE r.Name = 'Admin'
);

-- 8. Script de rollback (comentado)
/*
DROP TABLE IF EXISTS DealerEmployeeInvitations;
DROP TABLE IF EXISTS PlatformEmployeeInvitations;
DROP TABLE IF EXISTS DealerEmployees;
DROP TABLE IF EXISTS PlatformEmployees;

ALTER TABLE Users
DROP COLUMN IF EXISTS AccountType,
                      PlatformRole,
                      PlatformPermissions,
                      DealerId,
                      DealerRole,
                      DealerPermissions,
                      EmployerUserId,
                      CreatedBy;
*/

GO
