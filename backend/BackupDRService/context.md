# BackupDRService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** BackupDRService
- **Puerto en Desarrollo:** 5036
- **Estado:** 🚧 **EN DESARROLLO - NO DESPLEGADO**
- **Base de Datos:** PostgreSQL (`backupdrservice`)
- **Imagen Docker:** Local only

### Propósito
Servicio de backup y disaster recovery. Gestiona backups automáticos de bases de datos, archivos y configuraciones. Implementa estrategias de recovery point objective (RPO) y recovery time objective (RTO).

---

## 🏗️ ARQUITECTURA

```
BackupDRService/
├── BackupDRService.Api/
│   ├── Controllers/
│   │   ├── BackupsController.cs
│   │   ├── RestoreController.cs
│   │   └── DRPlanController.cs
│   └── Program.cs
├── BackupDRService.Application/
│   └── Services/
│       ├── PostgresBackupService.cs
│       ├── S3BackupService.cs
│       └── RestoreService.cs
├── BackupDRService.Domain/
│   ├── Entities/
│   │   ├── BackupJob.cs
│   │   ├── BackupSnapshot.cs
│   │   └── RestorePoint.cs
│   └── Enums/
│       ├── BackupType.cs
│       └── BackupStatus.cs
└── BackupDRService.Infrastructure/
```

---

## 📦 ENTIDADES PRINCIPALES

### BackupJob
```csharp
public class BackupJob
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    // Tipo
    public BackupType Type { get; set; }           // Database, Files, Configuration, Full
    public string SourceType { get; set; }         // "PostgreSQL", "S3", "K8s"
    public string SourceIdentifier { get; set; }   // DB name, bucket name, namespace
    
    // Schedule
    public string CronExpression { get; set; }     // "0 2 * * *" (diario 2am)
    public bool IsEnabled { get; set; }
    
    // Retention
    public int RetentionDays { get; set; }         // Días a mantener backups
    
    // Destino
    public string DestinationBucket { get; set; }
    public string DestinationPath { get; set; }
    
    // Estado
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public BackupStatus LastStatus { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
```

### BackupSnapshot
```csharp
public class BackupSnapshot
{
    public Guid Id { get; set; }
    public Guid BackupJobId { get; set; }
    public BackupJob BackupJob { get; set; }
    
    // Snapshot info
    public string SnapshotName { get; set; }
    public DateTime SnapshotDate { get; set; }
    
    // Ubicación
    public string StorageLocation { get; set; }    // S3 URL
    public string FileName { get; set; }
    public long SizeInBytes { get; set; }
    
    // Metadata
    public string? DatabaseName { get; set; }
    public string? DatabaseVersion { get; set; }
    public int? RecordCount { get; set; }
    
    // Verificación
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public bool? VerificationSuccess { get; set; }
    
    // Estado
    public BackupStatus Status { get; set; }       // InProgress, Completed, Failed
    public string? ErrorMessage { get; set; }
    public TimeSpan? Duration { get; set; }
    
    // Lifecycle
    public DateTime ExpiresAt { get; set; }
    public bool IsDeleted { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
```

### RestorePoint
```csharp
public class RestorePoint
{
    public Guid Id { get; set; }
    public Guid SnapshotId { get; set; }
    public BackupSnapshot Snapshot { get; set; }
    
    // Restore info
    public string RestoreTarget { get; set; }      // Dónde se restauró
    public RestoreStatus Status { get; set; }      // Initiated, InProgress, Completed, Failed
    
    // Solicitante
    public Guid RequestedByUserId { get; set; }
    public string RequestedByUserName { get; set; }
    public string Reason { get; set; }             // "Disaster recovery", "Data corruption"
    
    // Timing
    public DateTime InitiatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration { get; set; }
    
    // Resultado
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RestoreLog { get; set; }
}
```

---

## 📡 ENDPOINTS (Propuestos)

### Backup Jobs
- `POST /api/backups/jobs` - Crear backup job
  ```json
  {
    "name": "Daily PostgreSQL Backup",
    "type": "Database",
    "sourceType": "PostgreSQL",
    "sourceIdentifier": "vehiclessaleservice",
    "cronExpression": "0 2 * * *",
    "retentionDays": 30,
    "destinationBucket": "okla-backups",
    "destinationPath": "/postgres/vehiclessaleservice/"
  }
  ```
- `GET /api/backups/jobs` - Listar jobs
- `POST /api/backups/jobs/{id}/run-now` - Ejecutar manualmente
- `PUT /api/backups/jobs/{id}/disable` - Deshabilitar

### Snapshots
- `GET /api/backups/snapshots` - Listar snapshots (filtros: job, fecha)
- `GET /api/backups/snapshots/{id}` - Detalle de snapshot
- `POST /api/backups/snapshots/{id}/verify` - Verificar integridad
- `DELETE /api/backups/snapshots/{id}` - Eliminar snapshot

### Restore
- `POST /api/backups/restore` - Iniciar restore
  ```json
  {
    "snapshotId": "uuid",
    "restoreTarget": "vehiclessaleservice-restored",
    "reason": "Testing disaster recovery procedure"
  }
  ```
- `GET /api/backups/restore/{id}` - Estado del restore
- `GET /api/backups/restore-points` - Historial de restores

### DR Plan
- `GET /api/backups/dr-status` - Estado de disaster recovery
- `GET /api/backups/rpo-rto` - Métricas RPO/RTO actuales

---

## 💡 FUNCIONALIDADES PLANEADAS

### PostgreSQL Backup
```csharp
public async Task<BackupSnapshot> BackupPostgreSQLAsync(string dbName)
{
    var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
    var fileName = $"{dbName}_{timestamp}.sql.gz";
    
    // pg_dump con compresión
    var command = $"pg_dump -h postgres-host -U postgres -d {dbName} | gzip > /tmp/{fileName}";
    await ExecuteCommandAsync(command);
    
    // Upload a S3
    var s3Key = $"postgres/{dbName}/{fileName}";
    await _s3Client.PutObjectAsync(new PutObjectRequest
    {
        BucketName = "okla-backups",
        Key = s3Key,
        FilePath = $"/tmp/{fileName}"
    });
    
    // Registrar snapshot
    var snapshot = new BackupSnapshot
    {
        SnapshotName = fileName,
        StorageLocation = s3Key,
        SizeInBytes = new FileInfo($"/tmp/{fileName}").Length,
        Status = BackupStatus.Completed
    };
    
    return snapshot;
}
```

### S3/Spaces Backup
Replicate buckets a backup region:
```csharp
public async Task BackupS3BucketAsync(string sourceBucket, string destBucket)
{
    var objects = await _s3Client.ListObjectsV2Async(new ListObjectsV2Request
    {
        BucketName = sourceBucket
    });
    
    foreach (var obj in objects.S3Objects)
    {
        await _s3Client.CopyObjectAsync(new CopyObjectRequest
        {
            SourceBucket = sourceBucket,
            SourceKey = obj.Key,
            DestinationBucket = destBucket,
            DestinationKey = obj.Key
        });
    }
}
```

### Kubernetes Config Backup
```bash
# Backup de deployments, services, configmaps
kubectl get deployments,services,configmaps -n okla -o yaml > k8s-backup.yaml

# Upload a S3
aws s3 cp k8s-backup.yaml s3://okla-backups/k8s/backup-$(date +%Y%m%d).yaml
```

### Backup Verification
Verificar que backup es válido:
```csharp
public async Task<bool> VerifyBackupAsync(BackupSnapshot snapshot)
{
    // Descargar backup
    var localPath = await DownloadFromS3(snapshot.StorageLocation);
    
    // Intentar restore en DB temporal
    var testDbName = $"verify_{Guid.NewGuid():N}";
    try
    {
        await RestoreToDatabaseAsync(localPath, testDbName);
        
        // Query básico para verificar
        var count = await ExecuteScalarAsync(testDbName, "SELECT COUNT(*) FROM vehicles");
        
        // Limpiar DB temporal
        await DropDatabaseAsync(testDbName);
        
        return count > 0;
    }
    catch
    {
        return false;
    }
}
```

### Point-in-Time Recovery (PITR)
Para PostgreSQL con WAL archiving:
```sql
-- Restore hasta punto específico en el tiempo
SELECT pg_create_restore_point('before_data_corruption');

-- Restore usando WAL logs
RESTORE DATABASE vehiclessaleservice
  FROM BACKUP
  WITH RECOVERY_TARGET_TIME = '2026-01-07 09:30:00';
```

### Incremental Backups
- **Full Backup:** Domingo 2am
- **Incremental:** Lunes-Sábado 2am (solo cambios desde último backup)

Reduce tiempo y espacio de storage.

### 3-2-1 Backup Rule
- **3** copias de datos
- **2** tipos diferentes de media
- **1** copia offsite

Implementación:
1. Producción (Digital Ocean)
2. S3 Standard (same region)
3. S3 Glacier (different region)

---

## 📊 MÉTRICAS RPO/RTO

### Recovery Point Objective (RPO)
**Objetivo:** Perder máximo 1 hora de datos

- Backups cada 1 hora para DBs críticos
- Backups cada 6 horas para DBs secundarios

### Recovery Time Objective (RTO)
**Objetivo:** Restaurar en máximo 4 horas

Tiempos de restore estimados:
- PostgreSQL (5GB): ~30 min
- S3 bucket (100GB): ~2 horas
- Kubernetes cluster: ~1 hora

---

## 🔗 INTEGRACIÓN CON OTROS SERVICIOS

### NotificationService
- Alertas cuando backup falla
- Resumen diario de backups exitosos

### AuditService
- Registrar todas las operaciones de backup/restore

### AdminService
- Dashboard de estado de backups

---

## 🚨 DISASTER RECOVERY PLAN

### Escenarios

#### 1. Database Corruption
1. Identificar última snapshot válido
2. Crear DB temporal con nombre `{db}_restored`
3. Restore snapshot
4. Verificar integridad
5. Switch traffic a DB restaurado
6. Investigar causa de corrupción

#### 2. Complete Data Center Loss
1. Activar cluster en región secundaria
2. Restore último backup de S3 Glacier
3. Update DNS para apuntar a nueva región
4. Verificar todos los servicios

#### 3. Ransomware Attack
1. Aislar sistemas infectados
2. Restore desde backup anterior a infección
3. Verificar malware eliminado
4. Patch vulnerabilidades

---

## ⚠️ ALERTAS CRÍTICAS

- Backup job falla 2 veces consecutivas
- No hay backup exitoso en últimas 24h
- Espacio de storage > 90%
- Verification de backup falla

---

**Estado:** 🚧 EN DESARROLLO - No desplegado en producción  
**Versión:** 0.1.0  
**RPO:** 1 hora  
**RTO:** 4 horas
