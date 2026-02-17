# Backup & DR Service

## 📋 Descripción

Servicio de backup y disaster recovery automatizado para CarDealer Microservices. Proporciona backup automatizado de bases de datos PostgreSQL, gestión de restore points, y capacidades de recuperación ante desastres.

## 🚀 Características

- 💾 **Database Backups**: Backup automatizado de PostgreSQL usando `pg_dump`
- ⏮️ **Restore Points**: Gestión de puntos de restauración
- 🔄 **Point-in-Time Recovery**: Restauración a puntos específicos en el tiempo
- 📊 **Estadísticas**: Métricas de backups, restauraciones y uso de almacenamiento
- ⏰ **Scheduling**: Programación con expresiones cron
- 🗑️ **Cleanup Automático**: Limpieza de backups expirados
- ✅ **Verificación**: Verificación de integridad con checksums SHA-256
- 🔐 **Compresión**: Backups comprimidos para ahorro de espacio

## 🏗️ Arquitectura

```
BackupDRService/
├── BackupDRService.Core/           # Lógica de negocio
│   ├── Models/                     # Entidades y DTOs
│   │   ├── BackupJob.cs           # Configuración de job
│   │   ├── BackupResult.cs        # Resultado de backup
│   │   ├── RestorePoint.cs        # Punto de restauración
│   │   ├── RestoreResult.cs       # Resultado de restauración
│   │   ├── BackupOptions.cs       # Opciones de configuración
│   │   └── BackupStatistics.cs    # Estadísticas
│   ├── Interfaces/                 # Contratos
│   │   ├── IBackupService.cs
│   │   ├── IRestoreService.cs
│   │   ├── IStorageProvider.cs
│   │   └── IDatabaseBackupProvider.cs
│   └── Services/                   # Implementaciones
│       ├── BackupService.cs
│       ├── RestoreService.cs
│       ├── LocalStorageProvider.cs
│       └── PostgreSqlBackupProvider.cs
├── BackupDRService.Api/            # API REST
│   ├── Controllers/
│   │   ├── BackupController.cs    # Gestión de backups
│   │   └── RestoreController.cs   # Gestión de restores
│   └── Program.cs
└── BackupDRService.Tests/          # Tests unitarios
```

## 📡 API Endpoints

### Backup Jobs

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/backup/jobs` | Lista todos los jobs |
| GET | `/api/backup/jobs/enabled` | Lista jobs habilitados |
| GET | `/api/backup/jobs/{id}` | Obtiene un job |
| GET | `/api/backup/jobs/by-name/{name}` | Busca job por nombre |
| POST | `/api/backup/jobs` | Crea un job |
| PUT | `/api/backup/jobs/{id}` | Actualiza un job |
| DELETE | `/api/backup/jobs/{id}` | Elimina un job |
| POST | `/api/backup/jobs/{id}/enable` | Habilita un job |
| POST | `/api/backup/jobs/{id}/disable` | Deshabilita un job |

### Backup Execution

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/backup/jobs/{id}/execute` | Ejecuta backup manualmente |
| POST | `/api/backup/results/{id}/cancel` | Cancela backup en ejecución |
| GET | `/api/backup/results` | Resultados recientes |
| GET | `/api/backup/jobs/{jobId}/results` | Resultados por job |
| GET | `/api/backup/results/{id}` | Obtiene un resultado |
| GET | `/api/backup/results/by-date` | Resultados por rango de fechas |
| POST | `/api/backup/results/{id}/verify` | Verifica integridad |

### Restore Points

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/restore/points` | Lista restore points |
| GET | `/api/restore/points/available` | Lista points disponibles |
| GET | `/api/restore/points/{id}` | Obtiene un point |
| POST | `/api/restore/points` | Crea restore point |
| DELETE | `/api/restore/points/{id}` | Elimina point |
| POST | `/api/restore/points/{id}/verify` | Verifica point |
| POST | `/api/restore/points/{id}/test` | Testea point |

### Restore Execution

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/restore/points/{id}/restore` | Restaura desde point |
| POST | `/api/restore/from-backup/{backupResultId}` | Restaura desde backup |
| POST | `/api/restore/results/{id}/cancel` | Cancela restauración |
| GET | `/api/restore/results` | Lista resultados |
| GET | `/api/restore/results/recent` | Resultados recientes |

### Estadísticas y Limpieza

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/backup/statistics` | Estadísticas del sistema |
| POST | `/api/backup/cleanup` | Limpia backups expirados |
| POST | `/api/restore/cleanup` | Limpia points expirados |

## ⚙️ Configuración

### appsettings.json

```json
{
  "BackupOptions": {
    "DefaultRetentionDays": 30,
    "MaxConcurrentJobs": 3,
    "DefaultStorageType": "Local",
    "LocalStoragePath": "/var/backups/cardealer",
    "EnableCompressionByDefault": true,
    "VerifyBackupAfterCreation": true,
    "BackupTimeoutMinutes": 60,
    "RestoreTimeoutMinutes": 120,
    "EnableAutomaticCleanup": true,
    "CleanupSchedule": "0 2 * * *",
    "PgDumpPath": "pg_dump",
    "PgRestorePath": "pg_restore"
  }
}
```

## 📦 Tipos de Backup

### BackupType
- **Full**: Backup completo de la base de datos
- **Incremental**: Solo cambios desde el último backup
- **Differential**: Cambios desde el último backup full

### BackupTarget
- **PostgreSQL**: Base de datos PostgreSQL (implementado)
- **SqlServer**: SQL Server (futuro)
- **MongoDB**: MongoDB (futuro)
- **Redis**: Redis (futuro)
- **FileSystem**: Sistema de archivos (futuro)

### StorageType
- **Local**: Sistema de archivos local (implementado)
- **AzureBlob**: Azure Blob Storage (futuro)
- **S3**: Amazon S3 (futuro)
- **Ftp**: FTP/SFTP (futuro)

## 📖 Ejemplos de Uso

### Crear un Backup Job

```bash
curl -X POST http://localhost:15098/api/backup/jobs \
  -H "Content-Type: application/json" \
  -d '{
    "name": "UserService Daily Backup",
    "description": "Backup diario de UserService",
    "type": "Full",
    "target": "PostgreSQL",
    "connectionString": "Host=postgresql;Database=userservice;Username=postgres;Password=postgres",
    "databaseName": "userservice",
    "schedule": "0 2 * * *",
    "storageType": "Local",
    "storagePath": "userservice",
    "retentionDays": 30,
    "isEnabled": true,
    "compressBackup": true
  }'
```

### Ejecutar Backup Manualmente

```bash
curl -X POST http://localhost:15098/api/backup/jobs/{jobId}/execute
```

### Crear Restore Point

```bash
curl -X POST http://localhost:15098/api/restore/points \
  -H "Content-Type: application/json" \
  -d '{
    "backupResultId": "backup-result-id",
    "name": "Pre-Deploy Checkpoint",
    "description": "Punto de restauración antes del deploy"
  }'
```

### Restaurar desde un Point

```bash
curl -X POST http://localhost:15098/api/restore/points/{pointId}/restore \
  -H "Content-Type: application/json" \
  -d '{
    "targetDatabaseName": "userservice_restored",
    "mode": "NewDatabase",
    "createIfNotExists": true,
    "initiatedBy": "admin"
  }'
```

### Ver Estadísticas

```bash
curl http://localhost:15098/api/backup/statistics
```

Respuesta:
```json
{
  "totalJobs": 5,
  "enabledJobs": 4,
  "disabledJobs": 1,
  "runningJobs": 0,
  "totalBackups": 150,
  "successfulBackups": 145,
  "failedBackups": 5,
  "successRate": 96.67,
  "totalStorageUsedBytes": 1073741824,
  "lastBackupAt": "2025-12-02T02:00:00Z"
}
```

## 🐳 Docker

```bash
# Build
docker build -t backupdrservice .

# Run
docker run -p 15098:8080 \
  -v /var/backups:/var/backups/cardealer \
  -e BackupOptions__LocalStoragePath=/var/backups/cardealer \
  backupdrservice
```

## 🧪 Tests

```bash
cd BackupDRService
dotnet test
```

## 🔧 Requisitos

- .NET 8.0
- PostgreSQL con pg_dump/pg_restore en PATH
- Acceso de escritura al directorio de backups

## 📈 Monitoreo

El servicio expone:
- `/health` - Health check
- `/api/backup/statistics` - Métricas de backup/restore

## 🚨 Alertas Recomendadas

1. **Backup fallido**: Cuando `status = Failed`
2. **Almacenamiento bajo**: Cuando storage > 80% capacidad
3. **Job deshabilitado**: Jobs críticos deshabilitados
4. **Restore fallido**: Restauraciones fallidas
5. **Sin backups recientes**: Última ejecución > 24h

## 📝 Notas

- Los backups se crean en formato custom de PostgreSQL (-Fc)
- La verificación usa SHA-256 para integridad
- Los backups expirados se limpian automáticamente según schedule
- Se recomienda probar los restore points periódicamente
