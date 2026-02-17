# SchedulerService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** SchedulerService
- **Puerto en Desarrollo:** 5038
- **Estado:** 🚧 **EN DESARROLLO - NO DESPLEGADO**
- **Base de Datos:** PostgreSQL (`schedulerservice`)
- **Imagen Docker:** Local only

### Propósito
Servicio de programación de tareas (cron jobs). Ejecuta trabajos recurrentes como limpieza de datos, generación de reportes, envío de recordatorios, sincronizaciones y mantenimiento del sistema.

---

## 🏗️ ARQUITECTURA

```
SchedulerService/
├── SchedulerService.Api/
│   ├── Controllers/
│   │   ├── JobsController.cs
│   │   └── ExecutionsController.cs
│   └── Program.cs
├── SchedulerService.Application/
│   ├── Jobs/
│   │   ├── CleanupExpiredTokensJob.cs
│   │   ├── GenerateDailyReportsJob.cs
│   │   ├── SendReminderEmailsJob.cs
│   │   └── SyncDataJob.cs
│   └── Services/
│       └── JobExecutor.cs
├── SchedulerService.Domain/
│   ├── Entities/
│   │   ├── ScheduledJob.cs
│   │   └── JobExecution.cs
│   └── Enums/
│       ├── JobStatus.cs
│       └── ExecutionStatus.cs
└── SchedulerService.Infrastructure/
    └── Hangfire/                         # O Quartz.NET
```

---

## 📦 ENTIDADES PRINCIPALES

### ScheduledJob
```csharp
public class ScheduledJob
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    // Tipo de job (clase que implementa IJob)
    public string JobType { get; set; }            // "CleanupExpiredTokensJob"
    public string? Parameters { get; set; }        // JSON con parámetros
    
    // Schedule (Cron expression)
    public string CronExpression { get; set; }     // "0 2 * * *" (diario 2am)
    public string? TimeZone { get; set; }          // "America/Santo_Domingo"
    
    // Estado
    public JobStatus Status { get; set; }          // Enabled, Disabled, Paused
    public bool IsEnabled { get; set; }
    
    // Ejecución
    public DateTime? LastExecutionAt { get; set; }
    public DateTime? NextExecutionAt { get; set; }
    public ExecutionStatus? LastExecutionStatus { get; set; }
    public string? LastExecutionError { get; set; }
    
    // Métricas
    public int TotalExecutions { get; set; }
    public int SuccessfulExecutions { get; set; }
    public int FailedExecutions { get; set; }
    public TimeSpan? AverageDuration { get; set; }
    
    // Retry policy
    public int MaxRetries { get; set; } = 3;
    public int RetryDelayMinutes { get; set; } = 5;
    
    // Notificaciones
    public bool NotifyOnFailure { get; set; }
    public List<string>? NotificationEmails { get; set; }
    
    // Metadata
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}
```

### JobExecution
```csharp
public class JobExecution
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public ScheduledJob Job { get; set; }
    
    // Ejecución
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration { get; set; }
    
    // Estado
    public ExecutionStatus Status { get; set; }    // Running, Completed, Failed, Cancelled
    public string? Output { get; set; }            // Logs de la ejecución
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
    
    // Retry
    public int RetryAttempt { get; set; }
    public bool IsRetry { get; set; }
    
    // Contexto
    public string? MachineName { get; set; }
    public int? ProcessId { get; set; }
}
```

---

## 📡 ENDPOINTS (Propuestos)

### Jobs
- `GET /api/scheduler/jobs` - Listar jobs programados
- `GET /api/scheduler/jobs/{id}` - Detalle de job
- `POST /api/scheduler/jobs` - Crear job
  ```json
  {
    "name": "Daily Vehicle Cleanup",
    "description": "Remove expired listings",
    "jobType": "CleanupExpiredListingsJob",
    "cronExpression": "0 3 * * *",
    "isEnabled": true,
    "notifyOnFailure": true,
    "notificationEmails": ["admin@okla.com.do"]
  }
  ```
- `PUT /api/scheduler/jobs/{id}` - Actualizar job
- `PUT /api/scheduler/jobs/{id}/enable` - Habilitar job
- `PUT /api/scheduler/jobs/{id}/disable` - Deshabilitar job
- `POST /api/scheduler/jobs/{id}/run-now` - Ejecutar manualmente
- `DELETE /api/scheduler/jobs/{id}` - Eliminar job

### Executions
- `GET /api/scheduler/executions` - Historial de ejecuciones
- `GET /api/scheduler/executions/{id}` - Detalle de ejecución
- `GET /api/scheduler/jobs/{jobId}/executions` - Ejecuciones de un job específico

### Dashboard
- `GET /api/scheduler/dashboard` - Estadísticas generales
  ```json
  {
    "totalJobs": 15,
    "enabledJobs": 12,
    "runningJobs": 2,
    "nextExecution": {
      "jobName": "Backup Database",
      "scheduledAt": "2026-01-08T02:00:00Z"
    },
    "recentFailures": 3,
    "avgExecutionTime": "00:02:15"
  }
  ```

---

## 💡 JOBS PREDEFINIDOS

### 1. CleanupExpiredTokensJob
Eliminar refresh tokens y verification tokens expirados.
```csharp
public class CleanupExpiredTokensJob : IJob
{
    public async Task ExecuteAsync(CancellationToken ct)
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(t => t.ExpiresAt < DateTime.UtcNow)
            .ToListAsync(ct);
        
        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync(ct);
        
        _logger.LogInformation($"Deleted {expiredTokens.Count} expired tokens");
    }
}
```
**Schedule:** Diario 3:00 AM

### 2. GenerateDailyReportsJob
Generar y enviar reportes automáticos.
```csharp
public class GenerateDailyReportsJob : IJob
{
    public async Task ExecuteAsync(CancellationToken ct)
    {
        // Sales report
        var salesReport = await _reportsService.GenerateSalesReportAsync(
            DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
        
        // Email a managers
        await _notificationService.SendAsync(
            _managerEmails,
            "Daily Sales Report",
            salesReport.ToHtml());
    }
}
```
**Schedule:** Diario 8:00 AM

### 3. SendReminderEmailsJob
Enviar recordatorios de appointments próximos.
```csharp
public class SendReminderEmailsJob : IJob
{
    public async Task ExecuteAsync(CancellationToken ct)
    {
        var tomorrow = DateTime.UtcNow.AddDays(1);
        var appointments = await _context.Appointments
            .Where(a => a.ScheduledDate.Date == tomorrow.Date && !a.ReminderSent)
            .ToListAsync(ct);
        
        foreach (var apt in appointments)
        {
            await _notificationService.SendAsync(
                apt.ClientEmail,
                "Reminder: Appointment Tomorrow",
                $"Your appointment is scheduled for {apt.ScheduledTime}");
            
            apt.ReminderSent = true;
        }
        
        await _context.SaveChangesAsync(ct);
    }
}
```
**Schedule:** Diario 9:00 AM

### 4. CleanupOldLogsJob
Archivar logs antiguos a cold storage.
```csharp
public class CleanupOldLogsJob : IJob
{
    public async Task ExecuteAsync(CancellationToken ct)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-90);
        
        var oldLogs = await _context.Logs
            .Where(l => l.Timestamp < cutoffDate)
            .ToListAsync(ct);
        
        // Export a S3 Glacier
        await _s3Service.ArchiveLogsAsync(oldLogs);
        
        // Delete from DB
        _context.Logs.RemoveRange(oldLogs);
        await _context.SaveChangesAsync(ct);
    }
}
```
**Schedule:** Semanal, Domingo 1:00 AM

### 5. UpdateVehicleListingStatusJob
Marcar listings como expirados si no renovados.
```csharp
public class UpdateVehicleListingStatusJob : IJob
{
    public async Task ExecuteAsync(CancellationToken ct)
    {
        var expirationDate = DateTime.UtcNow.AddDays(-30);
        
        var expiredListings = await _context.Vehicles
            .Where(v => v.PublishedAt < expirationDate && v.Status == "Active")
            .ToListAsync(ct);
        
        foreach (var vehicle in expiredListings)
        {
            vehicle.Status = "Expired";
            
            // Notificar al vendedor
            await _notificationService.SendAsync(
                vehicle.SellerId,
                "Listing Expired",
                $"Your listing for {vehicle.Title} has expired. Renew to keep it active.");
        }
        
        await _context.SaveChangesAsync(ct);
    }
}
```
**Schedule:** Diario 4:00 AM

### 6. SyncVehiclePricesJob
Actualizar precios sugeridos basados en mercado.
```csharp
public class SyncVehiclePricesJob : IJob
{
    public async Task ExecuteAsync(CancellationToken ct)
    {
        var vehicles = await _context.Vehicles
            .Where(v => v.Status == "Active")
            .ToListAsync(ct);
        
        foreach (var vehicle in vehicles)
        {
            // Consultar API de pricing (KBB, etc.)
            var marketPrice = await _pricingService.GetMarketValueAsync(
                vehicle.Make, vehicle.Model, vehicle.Year, vehicle.Mileage);
            
            vehicle.SuggestedPrice = marketPrice;
        }
        
        await _context.SaveChangesAsync(ct);
    }
}
```
**Schedule:** Semanal, Lunes 2:00 AM

### 7. BackupDatabaseJob
Ejecutar backup de bases de datos.
```csharp
public class BackupDatabaseJob : IJob
{
    public async Task ExecuteAsync(CancellationToken ct)
    {
        var databases = new[] { "vehiclessaleservice", "authservice", "userservice" };
        
        foreach (var db in databases)
        {
            await _backupService.BackupPostgreSQLAsync(db);
        }
    }
}
```
**Schedule:** Diario 2:00 AM

### 8. HealthCheckMonitoringJob
Verificar salud de servicios.
```csharp
public class HealthCheckMonitoringJob : IJob
{
    public async Task ExecuteAsync(CancellationToken ct)
    {
        var services = new[] { "vehiclessaleservice", "authservice", "gateway" };
        
        foreach (var service in services)
        {
            var health = await _httpClient.GetAsync($"http://{service}:8080/health");
            
            if (!health.IsSuccessStatusCode)
            {
                // Alertar
                await _notificationService.SendAlertAsync(
                    $"Service {service} is unhealthy!");
            }
        }
    }
}
```
**Schedule:** Cada 5 minutos

---

## 🛠️ IMPLEMENTACIÓN

### Using Hangfire
```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHangfire(config => config
            .UsePostgreSqlStorage(_connectionString));
        
        services.AddHangfireServer();
    }
    
    public void Configure(IApplicationBuilder app)
    {
        app.UseHangfireDashboard("/hangfire");
        
        // Registrar jobs
        RecurringJob.AddOrUpdate<CleanupExpiredTokensJob>(
            "cleanup-expired-tokens",
            job => job.ExecuteAsync(CancellationToken.None),
            "0 3 * * *");  // Cron expression
    }
}
```

### Job Interface
```csharp
public interface IJob
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
```

---

## 📊 MONITORING

### Dashboard Metrics
- Jobs running
- Jobs queued
- Succeeded jobs (last 24h)
- Failed jobs (last 24h)
- Average execution time
- Next scheduled execution

### Alerting
- Job failed > 2 consecutive times
- Job execution time > threshold
- Job not executed in expected time window

---

## 🔗 INTEGRACIÓN CON OTROS SERVICIOS

### Todos los servicios
Puede ejecutar jobs que interactúan con cualquier servicio.

### NotificationService
- Enviar alertas de jobs fallidos
- Reportes programados

### AuditService
- Registrar ejecución de jobs

---

## 📝 CRON EXPRESSIONS REFERENCE

```
┌───────────── minute (0 - 59)
│ ┌───────────── hour (0 - 23)
│ │ ┌───────────── day of month (1 - 31)
│ │ │ ┌───────────── month (1 - 12)
│ │ │ │ ┌───────────── day of week (0 - 6) (Sunday=0)
│ │ │ │ │
* * * * *

Examples:
"0 2 * * *"      - Diario 2:00 AM
"0 */6 * * *"    - Cada 6 horas
"0 0 * * 0"      - Semanal Domingo medianoche
"0 0 1 * *"      - Mensual el día 1 medianoche
"*/5 * * * *"    - Cada 5 minutos
"0 9 * * 1-5"    - Lunes a Viernes 9:00 AM
```

---

**Estado:** 🚧 EN DESARROLLO - No desplegado en producción  
**Versión:** 0.1.0  
**Framework:** Hangfire / Quartz.NET  
**Total Jobs:** 8+ predefinidos
